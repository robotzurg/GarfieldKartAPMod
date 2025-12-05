using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace GarfieldKartAPMod
{
    public class NotificationDisplay : MonoBehaviour
    {
        private TextMeshProUGUI notificationText;
        private Queue<string> notificationQueue = new Queue<string>();
        private bool isDisplaying = false;

        public void Initialize()
        {
            // Create canvas
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; 

            // Create text object
            GameObject textObj = new GameObject("NotificationText");
            textObj.transform.SetParent(transform);

            notificationText = textObj.AddComponent<TextMeshProUGUI>();
            notificationText.fontSize = 48;
            notificationText.alignment = TextAlignmentOptions.Top;
            notificationText.autoSizeTextContainer = true;
            notificationText.enableWordWrapping = true;
            notificationText.color = Color.white;

            // Position at top of screen
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
            rectTransform.anchoredPosition = new Vector2(0, -20);
            rectTransform.sizeDelta = new Vector2(-40, 100);

            notificationText.text = "";
        }

        public void ShowNotification(string message)
        {
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

                yield return new WaitForSeconds(5f); // Display for 5 seconds
            }

            notificationText.text = "";
            isDisplaying = false;
        }
    }
}