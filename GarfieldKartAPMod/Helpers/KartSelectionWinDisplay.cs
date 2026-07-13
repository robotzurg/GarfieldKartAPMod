using TMPro;
using UnityEngine;

namespace GarfieldKartAPMod.Helpers
{
    // Marks the racers and karts you've already won a race with in the kart selection menu.
    // The "Win Race as/with X" locations are the source of truth, so nothing extra is tracked.
    internal static class KartSelectionWinDisplay
    {
        private const string ObjectName = "APWinMarker";

        private const string WonMark = "<color=#6BE36B>WON</color>";
        private const string NotWonMark = "<color=#8A8A8A>NOT WON</color>";

        public static void AttachOrUpdate(KartSelectionItem item)
        {
            if (item == null) return;

            long locationId = GetWinLocation(item);
            bool show = ArchipelagoHelper.IsConnectedAndEnabled && locationId != -1;

            TextMeshProUGUI text = GetOrCreateText(item);
            if (text == null) return;

            text.gameObject.SetActive(show);
            if (!show)
            {
                Log.Message($"[WinMarker] {item.name}: hidden (type {item.Type}, location {locationId})");
                return;
            }

            bool won = ArchipelagoItemTracker.HasLocation(locationId);
            text.text = won ? WonMark : NotWonMark;
            Log.Message($"[WinMarker] {item.name}: location {locationId}, won={won}");
        }

        // Both location blocks run in ECharacter order. They only exist when their category is
        // randomized, so without that the marker would be stuck on NOT WON forever.
        private static long GetWinLocation(KartSelectionItem item)
        {
            switch (item.Type)
            {
                case MenuHDKartSelection.KARTSELECT_TYPE.CHARACTER:
                    if (item.IconCarac is not CharacterCarac character) return -1;
                    if (!ArchipelagoHelper.IsCharRandomizerEnabled()) return -1;
                    return ArchipelagoConstants.LOC_WIN_RACE_AS_GARFIELD + (int)character.Owner;

                case MenuHDKartSelection.KARTSELECT_TYPE.KART:
                    if (item.IconCarac is not KartCarac kart) return -1;
                    if (!ArchipelagoHelper.IsKartRandomizerEnabled()) return -1;
                    return ArchipelagoConstants.LOC_WIN_RACE_WITH_FORMULA_ZZZZ + (int)kart.Owner;

                default:
                    return -1;
            }
        }

        private static TextMeshProUGUI GetOrCreateText(KartSelectionItem item)
        {
            Transform existing = item.transform.Find(ObjectName);
            if (existing != null) return existing.GetComponent<TextMeshProUGUI>();

            // A KartSelectionItem is all Images with no text of its own, so clone a label from
            // elsewhere in the menu - that brings the font, material and canvas setup with it.
            // Searched from the canvas root because the menu can still be inactive during Enter.
            TextMeshProUGUI template = item.transform.root.GetComponentInChildren<TextMeshProUGUI>(true);
            if (template == null)
            {
                Log.Warning("[WinMarker] No TMP label found to clone in the kart selection menu");
                return null;
            }

            GameObject go = Object.Instantiate(template.gameObject, item.transform);
            go.name = ObjectName;
            go.SetActive(true);
            go.transform.SetAsLastSibling();

            foreach (Transform child in go.transform) Object.Destroy(child.gameObject);

            // Drop localizers/animators that came along with the clone, or they'll fight us for
            // the text
            foreach (Component component in go.GetComponents<Component>())
            {
                if (component is RectTransform || component is CanvasRenderer || component is TextMeshProUGUI) continue;
                Object.Destroy(component);
            }

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 12f;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            text.color = Color.white;

            // Kept inside the tile's own bounds: the items sit in a masked scroll rect, so
            // anything hanging below the icon gets clipped away
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 4f);
            rect.sizeDelta = new Vector2(0f, 16f);
            rect.localScale = Vector3.one;

            return text;
        }
    }
}
