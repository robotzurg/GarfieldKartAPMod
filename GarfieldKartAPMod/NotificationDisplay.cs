using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace GarfieldKartAPMod
{
    public class NotificationDisplay : MonoBehaviour
    {
        // Everything below is authored against this resolution and the CanvasScaler rescales it
        // from there, so the notification takes up the same share of the screen at any size
        private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

        private TextMeshProUGUI notificationText;
        private TextMeshProUGUI shadowText;
        private readonly Queue<string> notificationQueue = new Queue<string>();
        private bool isDisplaying;

        public void Initialize()
        {
            // Create canvas
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            // Scales it for resolution
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            // Create shadow text object
            GameObject shadowObj = new GameObject("NotificationShadow");
            shadowObj.transform.SetParent(transform);

            shadowText = shadowObj.AddComponent<TextMeshProUGUI>();
            ConfigureText(shadowText);
            shadowText.color = Color.black;

            // Offset shadow
            RectTransform shadowRect = shadowText.GetComponent<RectTransform>();
            shadowRect.anchoredPosition = new Vector2(2, -22);

            // Create main text object
            GameObject textObj = new GameObject("NotificationText");
            textObj.transform.SetParent(transform);

            notificationText = textObj.AddComponent<TextMeshProUGUI>();
            ConfigureText(notificationText);
            notificationText.color = Color.white;

            notificationText.text = "";
            shadowText.text = "";
        }

        private void ConfigureText(TextMeshProUGUI text)
        {
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Top;
            text.autoSizeTextContainer = true;
            text.enableWordWrapping = true;
            text.richText = true;

            // Position at top of screen
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -20);
            rectTransform.sizeDelta = new Vector2(-40, 100);
        }

        public void ShowNotification(string message)
        {
            if (message.Contains("Now that you are connected")) return;
            if (message.Contains("Warning: your client does not")) return;
            
            notificationQueue.Enqueue(message);
            if (!isDisplaying)
            {
                StartCoroutine(DisplayNextNotification());
            }
        }

        private IEnumerator DisplayNextNotification()
        {
            isDisplaying = true;

            while (notificationQueue.Count > 0)
            {
                string message = notificationQueue.Dequeue();
                notificationText.text = message;
                shadowText.text = StripColorTags(message);

                yield return new WaitForSeconds(GarfieldKartAPMod.notificationTime.Value); // Display for configurable value, default 3 seconds
            }

            notificationText.text = "";
            shadowText.text = "";
            isDisplaying = false;
        }

        private string StripColorTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            // Basic stripping of <color=...> and </color>
            string output = input;
            while (output.Contains("<color="))
            {
                int start = output.IndexOf("<color=");
                int end = output.IndexOf(">", start);
                if (end != -1)
                {
                    output = output.Remove(start, end - start + 1);
                }
                else break;
            }
            output = output.Replace("</color>", "");
            return output;
        }
    }
}