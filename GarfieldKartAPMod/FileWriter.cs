using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GarfieldKartAPMod
{
    public class FileWriter : MonoBehaviour
    {
        private NotificationDisplay notificationDisplay;

        void Start()
        {
            notificationDisplay = FindObjectOfType<NotificationDisplay>();
            if (notificationDisplay == null)
            {
                GameObject notifObj = new GameObject("NotificationDisplay");
                notificationDisplay = notifObj.AddComponent<NotificationDisplay>();
                notificationDisplay.Initialize();
                DontDestroyOnLoad(notifObj);
            }
        }

        public void WriteNotificationData((long itemId, int playerId, string itemName, string playerName) notification)
        {
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
                var session = GarfieldKartAPMod.APClient.GetSession();
                string sessionSeed = session.RoomState.Seed;
                string path = Application.persistentDataPath + $"/{sessionSeed}.txt";

                // Read existing lines if file exists
                HashSet<string> existingLines = new HashSet<string>();
                if (File.Exists(path))
                {
                    existingLines = new HashSet<string>(File.ReadAllLines(path));
                }

                // Only write if the text doesn't already exist
                if (!existingLines.Contains($"{notification.itemId}"))
                {
                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        writer.WriteLine($"{notification.itemId}");
                    }
                    Debug.Log($"AP File written to: {path}");

                    // Show notification ONLY for new data
                    if (notificationDisplay != null)
                    {
                        notificationDisplay.ShowNotification($"Received {notification.itemName} from {notification.playerName}!");
                    }
                }
            }
        }
    }
}