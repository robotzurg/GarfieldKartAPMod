using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace GarfieldKartAPMod
{
    public class FileWriter : MonoBehaviour
    {
        private NotificationDisplay notificationDisplay;
        private const string LastConnectionFileName = "last_connection.txt";

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

        // Persist a completed time-trial scene to a separate per-session file.
        public void WriteTimeTrialData(string track)
        {
            if (GarfieldKartAPMod.APClient == null || !GarfieldKartAPMod.APClient.IsConnected)
                return;

            var session = GarfieldKartAPMod.APClient.GetSession();
            if (session == null)
                return;

            string sessionSeed = session.RoomState.Seed;
            string path = Application.persistentDataPath + $"/{sessionSeed}_timetrials.txt";

            HashSet<string> existingLines = new HashSet<string>();
            if (File.Exists(path))
            {
                existingLines = new HashSet<string>(File.ReadAllLines(path));
            }

            if (!existingLines.Contains(track))
            {
                using (StreamWriter writer = new StreamWriter(path, true))
                {
                    writer.WriteLine(track);
                }
                Debug.Log($"AP TimeTrial file written to: {path}");

                if (notificationDisplay != null)
                {
                    notificationDisplay.ShowNotification($"Time trial completed: {track}");
                }
            }
        }

        // Save the last used connection info to disk. Overwrites each time so it's the default on next run.
        public void WriteLastConnection(string host, int port, string slotName, string password)
        {
            try
            {
                string path = Application.persistentDataPath + "/" + LastConnectionFileName;
                var lines = new List<string>
                {
                    host ?? "",
                    port.ToString(),
                    slotName ?? "",
                    password ?? ""
                };
                File.WriteAllLines(path, lines);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write last connection info: {ex.Message}");
            }
        }

        public static (string host, string port, string slotName, string password) ReadLastConnection()
        {
            try
            {
                string path = Application.persistentDataPath + "/" + LastConnectionFileName;
                if (!File.Exists(path))
                    return (null, null, null, null);

                var lines = File.ReadAllLines(path);
                string host = lines.Length > 0 ? lines[0] : null;
                string port = lines.Length > 1 ? lines[1] : null;
                string slot = lines.Length > 2 ? lines[2] : null;
                string pass = lines.Length > 3 ? lines[3] : null;
                return (host, port, slot, pass);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read last connection info: {ex.Message}");
                return (null, null, null, null);
            }
        }
    }
}