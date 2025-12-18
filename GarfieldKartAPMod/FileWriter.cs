using Archipelago.MultiClient.Net;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace GarfieldKartAPMod
{
    public class FileWriter : MonoBehaviour
    {
        private const string LastConnectionFileName = "last_connection.txt";

        // Persist a completed time-trial scene to a separate per-session file.
        public void WriteTimeTrialData(string track)
        {
            if (GarfieldKartAPMod.APClient == null || !GarfieldKartAPMod.APClient.IsConnected)
                return;

            ArchipelagoSession session = GarfieldKartAPMod.APClient.GetSession();
            if (session == null)
                return;

            string sessionSeed = session.RoomState.Seed;
            string path = Application.persistentDataPath + $"/{sessionSeed}_timetrials.txt";

            HashSet<string> existingLines = [];
            if (File.Exists(path))
            {
                existingLines = new HashSet<string>(File.ReadAllLines(path));
            }

            if (existingLines.Contains(track)) return;
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(track);
            }
            Debug.Log($"AP TimeTrial file written to: {path}");
        }

        public void WriteFillerData(string data)
        {
            if (GarfieldKartAPMod.APClient == null || !GarfieldKartAPMod.APClient.IsConnected)
                return;

            ArchipelagoSession session = GarfieldKartAPMod.APClient.GetSession();
            if (session == null)
                return;

            string sessionSeed = session.RoomState.Seed;
            string path = Application.persistentDataPath + $"/{sessionSeed}_filler.txt";

            try
            {
                File.WriteAllText(path, data);
                Debug.Log($"AP Filler file written to: {path}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write filler data: {ex.Message}");
            }
        }

        public string ReadFillerData()
        {
            if (GarfieldKartAPMod.APClient == null || !GarfieldKartAPMod.APClient.IsConnected)
                return string.Empty;

            ArchipelagoSession session = GarfieldKartAPMod.APClient.GetSession();
            if (session == null)
                return string.Empty;

            string sessionSeed = session.RoomState.Seed;
            string path = Application.persistentDataPath + $"/{sessionSeed}_filler.txt";

            if (!File.Exists(path))
                return string.Empty;

            try
            {
                return File.ReadAllText(path);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to read filler data: {ex.Message}");
                return string.Empty;
            }
        }

        // Save the last used connection info to disk. Overwrites each time, so it's the default on the next run.
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

                string[] lines = File.ReadAllLines(path);
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