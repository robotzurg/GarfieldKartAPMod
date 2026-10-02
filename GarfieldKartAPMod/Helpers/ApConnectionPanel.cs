using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GarfieldKartAPMod.Helpers
{
    // Connection form on the "press any button" screen; menu-driven, with hand-rolled mouse clicks
    internal class ApConnectionPanel : MonoBehaviour
    {
        private const string RootName = "APConnectionPanel";

        private const float PanelWidth = 780f;
        private const float RowHeight = 58f;
        private const float LabelWidth = 150f;
        private const float FontSize = 28f;

        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color FieldColor = new Color(1f, 1f, 1f, 0.85f);
        private static readonly Color FieldSelectedColor = new Color(1f, 0.62f, 0.15f, 1f);
        private static readonly Color PlaceholderColor = new Color(0.45f, 0.45f, 0.45f, 1f);
        private static readonly Color ErrorColor = new Color(1f, 1f, 1f, 1f);
        private const string HintText = "";

        private static ApConnectionPanel instance;

        private MenuHDEngagementScreen screen;
        private TMP_InputField hostField, portField, slotField, passwordField;
        private Selectable connectButton;
        private TextMeshProUGUI statusText;
        private readonly List<GameObject> hiddenScreenObjects = new List<GameObject>();
        private bool editingLocked;
        private bool connecting;

        // The engagement screen may only advance once there's a session
        public static bool CanLeaveTitle => GarfieldKartAPMod.APClient?.IsConnected ?? false;

        public static bool Owns(TMP_InputField field) => instance != null && field.transform.IsChildOf(instance.transform);

        public static void SetStatus(string message, bool error = false)
        {
            if (instance == null || instance.statusText == null) return;
            instance.statusText.text = message;
            instance.statusText.color = error ? ErrorColor : Color.white;
        }

        public static void Attach(MenuHDEngagementScreen screen)
        {
            if (screen == null) return;

            if (instance == null)
            {
                Transform existing = screen.transform.Find(RootName);
                instance = existing != null
                    ? existing.GetComponent<ApConnectionPanel>()
                    : Build(screen);
            }

            instance.screen = screen;
            instance.Show(!CanLeaveTitle);
        }

        public static void Refresh()
        {
            if (instance != null && instance.screen != null) instance.Show(!CanLeaveTitle);
        }

        private void Show(bool show)
        {
            gameObject.SetActive(show);
            Cursor.visible = show;

            // The vanilla "press any button" prompt would contradict the form
            if (show)
            {
                foreach (TextMeshProUGUI text in screen.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    if (text.transform.IsChildOf(transform)) continue;
                    text.gameObject.SetActive(false);
                    hiddenScreenObjects.Add(text.gameObject);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
                hostField.text = ArchipelagoConnection.Host;
                portField.text = ArchipelagoConnection.Port;
                slotField.text = ArchipelagoConnection.Slot;
                passwordField.text = ArchipelagoConnection.Password;
                foreach (TMP_InputField field in Fields)
                {
                    field.textComponent.ForceMeshUpdate();
                    PinTextLeft(field);
                }
                if (string.IsNullOrEmpty(statusText.text)) SetStatus(HintText);

                // Land on the first thing that still needs filling in
                Selectable first = new Selectable[] { hostField, portField, slotField }
                    .FirstOrDefault(field => string.IsNullOrWhiteSpace(((TMP_InputField)field).text)) ?? connectButton;
                GkEventSystem.Current.SelectButton(first);
            }
            else
            {
                foreach (GameObject hidden in hiddenScreenObjects)
                    if (hidden != null) hidden.SetActive(true);
                hiddenScreenObjects.Clear();
                statusText.text = "";
                SetEditingLock(false);
            }
        }

        private TMP_InputField[] Fields => new[] { hostField, portField, slotField, passwordField };

        // Typing must not also navigate or re-submit through the menu's own inputs
        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) HandleClick(Input.mousePosition);
            if (Input.GetKeyDown(KeyCode.Tab)) TabToNextField();

            bool editing = false;
            foreach (TMP_InputField field in Fields)
            {
                if (field.isFocused) editing = true;
                // The field scrolls its text to follow the caret and never scrolls back
                else PinTextLeft(field);
                SyncCaret(field);
            }
            SetEditingLock(editing);
        }
        
        private void TabToNextField()
        {
            TMP_InputField[] fields = Fields;
            int current = System.Array.FindIndex(fields, field => field.isFocused);
            if (current < 0) return;

            bool back = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            TMP_InputField next = fields[(current + (back ? -1 : 1) + fields.Length) % fields.Length];
            fields[current].DeactivateInputField();
            GkEventSystem.Current.SelectButton(next);
            next.ActivateInputField();
        }

        private static readonly AccessTools.FieldRef<TMP_InputField, RectTransform> CaretRectRef =
            AccessTools.FieldRefAccess<TMP_InputField, RectTransform>("caretRectTrans");
        
        private static void SyncCaret(TMP_InputField field)
        {
            RectTransform caret = CaretRectRef(field);
            RectTransform text = field.textComponent.rectTransform;
            if (caret == null) return;
            caret.anchorMin = text.anchorMin;
            caret.anchorMax = text.anchorMax;
            caret.pivot = text.pivot;
            caret.sizeDelta = text.sizeDelta;
            caret.localScale = text.localScale;
            caret.anchoredPosition = text.anchoredPosition;
        }
        
        private static void PinTextLeft(TMP_InputField field)
        {
            TMP_Text text = field.textComponent;
            if (text.textInfo == null || text.textInfo.characterCount == 0) return;
            float firstX = text.textInfo.characterInfo[0].origin - text.margin.x;
            text.rectTransform.anchoredPosition = new Vector2(field.textViewport.rect.xMin - firstX, 0f);
        }

        // The game's input module ignores the pointer, so clicks are hit-tested by hand
        private void HandleClick(Vector2 point)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera cam = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            foreach (TMP_InputField field in Fields)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint((RectTransform)field.transform, point, cam)) continue;
                GkEventSystem.Current.SelectButton(field);
                field.ActivateInputField();
                return;
            }

            if (RectTransformUtility.RectangleContainsScreenPoint((RectTransform)connectButton.transform, point, cam))
            {
                foreach (TMP_InputField field in Fields) field.DeactivateInputField();
                GkEventSystem.Current.SelectButton(connectButton);
                OnConnectPressed();
                return;
            }

            foreach (TMP_InputField field in Fields) field.DeactivateInputField();
        }

        private void OnDisable()
        {
            SetEditingLock(false);
        }

        private void SetEditingLock(bool locked)
        {
            if (locked == editingLocked) return;
            editingLocked = locked;
            GkEventSystem.Current?.LockAllMoveInputs(locked);
            GkEventSystem.Current?.LockAllButtonInputs(locked);
        }

        private void OnConnectPressed()
        {
            if (connecting) return;

            ArchipelagoConnection.Host = hostField.text;
            ArchipelagoConnection.Port = portField.text;
            ArchipelagoConnection.Slot = slotField.text;
            ArchipelagoConnection.Password = passwordField.text;

            StartCoroutine(ConnectRoutine());
        }

        // Connect blocks the main thread, so give the status line a frame to render first
        private IEnumerator ConnectRoutine()
        {
            connecting = true;
            SetStatus("Connecting...");
            yield return null;
            yield return null;

            string error = ArchipelagoConnection.TryConnect();
            connecting = false;

            if (error != null)
            {
                SetStatus(error, error: true);
                yield break;
            }

            Show(false);
            screen.OnSubmitAction();
        }

        // ========== BUILDING ==========

        private static ApConnectionPanel Build(MenuHDEngagementScreen screen)
        {
            TextMeshProUGUI fontSource = screen.GetComponentInChildren<TextMeshProUGUI>(true)
                                         ?? Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().FirstOrDefault();
            Button buttonTemplate = FindMenuButton();
            Sprite boxSprite = (buttonTemplate?.targetGraphic as Image)?.sprite;

            GameObject root = new GameObject(RootName, typeof(RectTransform));
            root.transform.SetParent(screen.transform, false);
            root.transform.SetAsLastSibling();

            RectTransform rootRect = (RectTransform)root.transform;
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, 30f);
            rootRect.sizeDelta = new Vector2(PanelWidth, 0f);

            Image backdrop = root.AddComponent<Image>();
            backdrop.sprite = boxSprite;
            backdrop.type = boxSprite != null && boxSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            backdrop.color = BackdropColor;
            backdrop.raycastTarget = false;

            VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 24, 24);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            root.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ApConnectionPanel panel = root.AddComponent<ApConnectionPanel>();

            CreateLabel(root.transform, fontSource, "CONNECT TO ARCHIPELAGO", 40f, TextAlignmentOptions.Center, 52f);

            panel.hostField = CreateRow(root.transform, fontSource, boxSprite, "Host", "archipelago.gg", TMP_InputField.ContentType.Standard);
            panel.portField = CreateRow(root.transform, fontSource, boxSprite, "Port", "38281", TMP_InputField.ContentType.IntegerNumber);
            panel.portField.characterLimit = 5;
            panel.slotField = CreateRow(root.transform, fontSource, boxSprite, "Slot Name", "Your slot name", TMP_InputField.ContentType.Standard);
            panel.passwordField = CreateRow(root.transform, fontSource, boxSprite, "Password", "Optional", TMP_InputField.ContentType.Password);

            panel.connectButton = CreateButton(root.transform, fontSource, buttonTemplate, "CONNECT", panel.OnConnectPressed);
            panel.statusText = CreateLabel(root.transform, fontSource, "", 24f, TextAlignmentOptions.Center, 64f);
            panel.statusText.enableWordWrapping = true;

            LinkVertically(panel.hostField, panel.portField, panel.slotField, panel.passwordField, panel.connectButton);
            return panel;
        }

        // Pause menu first, the main menu's art is cut off on the left
        private static Button FindMenuButton()
        {
            Button button = FindFirstButton<MenuHDPause>() ?? FindFirstButton<MenuHDMain>();
            if (button == null) Log.Warning("[Connection] No menu button found to style the connection panel with");
            return button;
        }

        private static Button FindFirstButton<T>() where T : Component
        {
            T menu = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            return menu?.GetComponentsInChildren<Button>(true).FirstOrDefault();
        }

        private static void LinkVertically(params Selectable[] selectables)
        {
            for (int i = 0; i < selectables.Length; i++)
            {
                selectables[i].navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = selectables[(i - 1 + selectables.Length) % selectables.Length],
                    selectOnDown = selectables[(i + 1) % selectables.Length]
                };
            }
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, TextMeshProUGUI fontSource, string text,
            float fontSize, TextAlignmentOptions alignment, float height)
        {
            GameObject go = new GameObject("APLabel", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            ApplyFont(label, fontSource);
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.color = Color.white;

            LayoutElement element = go.AddComponent<LayoutElement>();
            element.minHeight = height;
            element.preferredHeight = height;
            return label;
        }

        private static TMP_InputField CreateRow(Transform parent, TextMeshProUGUI fontSource, Sprite boxSprite,
            string labelText, string placeholder, TMP_InputField.ContentType contentType)
        {
            GameObject row = new GameObject("APRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            LayoutElement rowElement = row.AddComponent<LayoutElement>();
            rowElement.minHeight = RowHeight;
            rowElement.preferredHeight = RowHeight;

            TextMeshProUGUI label = CreateLabel(row.transform, fontSource, labelText, FontSize, TextAlignmentOptions.Left, RowHeight);
            LayoutElement labelElement = label.GetComponent<LayoutElement>();
            labelElement.minWidth = LabelWidth;
            labelElement.preferredWidth = LabelWidth;

            TMP_InputField field = CreateField(row.transform, fontSource, boxSprite, placeholder, contentType);
            field.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            return field;
        }

        // Built inactive so the input field sees all its parts on its first OnEnable
        private static TMP_InputField CreateField(Transform parent, TextMeshProUGUI fontSource, Sprite boxSprite,
            string placeholder, TMP_InputField.ContentType contentType)
        {
            GameObject go = new GameObject("APField", typeof(RectTransform));
            go.SetActive(false);
            go.transform.SetParent(parent, false);

            Image background = go.AddComponent<Image>();
            background.sprite = boxSprite;
            background.type = boxSprite != null && boxSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;

            GameObject area = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            area.transform.SetParent(go.transform, false);
            RectTransform areaRect = (RectTransform)area.transform;
            Stretch(areaRect, new Vector2(16f, 6f), new Vector2(-16f, -6f));

            TextMeshProUGUI text = CreateFieldText(area.transform, fontSource, "Text");
            text.color = Color.white;
            TextMeshProUGUI placeholderText = CreateFieldText(area.transform, fontSource, "Placeholder");
            placeholderText.text = placeholder;
            placeholderText.color = PlaceholderColor;
            placeholderText.fontStyle = FontStyles.Italic;

            TMP_InputField field = go.AddComponent<TMP_InputField>();
            field.textViewport = areaRect;
            field.textComponent = text;
            field.placeholder = placeholderText;
            field.targetGraphic = background;
            field.contentType = contentType;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.onValidateInput = (input, index, c) => c == '\t' ? '\0' : c;
            field.caretWidth = 3;
            field.customCaretColor = true;
            field.caretColor = Color.white;

            ColorBlock colors = field.colors;
            colors.normalColor = FieldColor;
            colors.highlightedColor = FieldSelectedColor;
            colors.pressedColor = FieldSelectedColor;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            field.colors = colors;

            go.SetActive(true);
            return field;
        }

        private static TextMeshProUGUI CreateFieldText(Transform parent, TextMeshProUGUI fontSource, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Stretch((RectTransform)go.transform, Vector2.zero, Vector2.zero);

            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            ApplyFont(text, fontSource);
            text.fontSize = FontSize;
            text.alignment = TextAlignmentOptions.Left;
            text.enableWordWrapping = false;
            text.raycastTarget = false;
            text.margin = new Vector4(0f, 0f, 8f, 0f);
            return text;
        }
        
        private static Selectable CreateButton(Transform parent, TextMeshProUGUI fontSource, Button template,
            string labelText, UnityEngine.Events.UnityAction onClick)
        {
            // Wrapped so the layout group sizes the slot and the button's own animator owns the rest
            GameObject slot = new GameObject("APButtonSlot", typeof(RectTransform));
            slot.transform.SetParent(parent, false);
            LayoutElement slotElement = slot.AddComponent<LayoutElement>();
            slotElement.minHeight = 72f;
            slotElement.preferredHeight = 72f;

            Button button;
            if (template != null)
            {
                button = Object.Instantiate(template.gameObject, slot.transform).GetComponent<Button>();
                button.name = "APConnectButton";
                if (button is BetterButton better) better.SelectAction = new BetterButton.ButtonSelectedEvent();

                // Icons belong to the menu entry it was cloned from
                foreach (Image image in button.GetComponentsInChildren<Image>(true))
                {
                    if (image == button.targetGraphic) continue;
                    string name = image.name.ToLower();
                    if (name.Contains("icn") || name.Contains("icon")) image.gameObject.SetActive(false);
                }

                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    // Drop localizers so they can't overwrite our text
                    foreach (Component component in label.GetComponents<Component>())
                    {
                        if (component is RectTransform || component is CanvasRenderer || component is TextMeshProUGUI) continue;
                        Object.Destroy(component);
                    }
                    label.text = labelText;
                }

                RectTransform rect = (RectTransform)button.transform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
            }
            else
            {
                GameObject go = new GameObject("APConnectButton", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(slot.transform, false);
                ((RectTransform)go.transform).sizeDelta = new Vector2(320f, 64f);
                button = go.AddComponent<Button>();
                button.targetGraphic = go.GetComponent<Image>();

                ColorBlock colors = button.colors;
                colors.highlightedColor = FieldSelectedColor;
                button.colors = colors;

                TextMeshProUGUI label = CreateFieldText(go.transform, fontSource, "Label");
                label.alignment = TextAlignmentOptions.Center;
                label.color = Color.black;
                label.text = labelText;
            }

            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(onClick);
            return button;
        }

        private static void ApplyFont(TextMeshProUGUI text, TextMeshProUGUI fontSource)
        {
            if (fontSource == null) return;
            text.font = fontSource.font;
            text.fontSharedMaterial = fontSource.fontSharedMaterial;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
