using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace GarfieldKartAPMod
{
    // The goal progress section exists because collect is STUPID
    // That entire section should be taken as a strike against collect because if it was removed I would be happy
    // I hate it with all of its soul, but this fixes problems with it, so there ya go.
    // - Jeff
    public static class ApJsonSaveFile
    {
        private class SlotSaveData
        {
            // Goal progress: what the player has actually won, so a server-side
            // !collect (which marks locations checked) can't complete a goal
            public HashSet<string> RaceVictories = new HashSet<string>();
            public HashSet<int> CupVictories = new HashSet<int>();
            public HashSet<string> TimeTrialVictories = new HashSet<string>();

            // Filler/trap state. The queue isn't stored because it can be
            // reconstructed from the server: anything received that isn't active
            // or completed must still be queued.
            public List<SavedActiveFiller> ActiveFiller = new List<SavedActiveFiller>();
            public List<long> CompletedFiller = new List<long>();
        }

        public class SavedActiveFiller
        {
            public long Id;
            public int RemainingRaces;
        }

        private static SlotSaveData cached;
        private static string cachedKey;

        // ========== GOAL PROGRESS ==========

        public static void RecordRaceVictory(string track)
        {
            SlotSaveData data = Load();
            if (data == null) return;
            if (data.RaceVictories.Add(track)) Save(data);
        }

        public static void RecordCupVictory(int cupId)
        {
            SlotSaveData data = Load();
            if (data == null) return;
            if (data.CupVictories.Add(cupId)) Save(data);
        }

        public static void RecordTimeTrialVictory(string track)
        {
            SlotSaveData data = Load();
            if (data == null) return;
            if (data.TimeTrialVictories.Add(track)) Save(data);
        }

        public static int GetRaceVictoryCount() => Load()?.RaceVictories.Count ?? 0;
        public static int GetCupVictoryCount() => Load()?.CupVictories.Count ?? 0;
        public static int GetTimeTrialVictoryCount() => Load()?.TimeTrialVictories.Count ?? 0;

        // ========== FILLER STATE ==========

        public static List<SavedActiveFiller> GetActiveFillerState()
        {
            return Load()?.ActiveFiller ?? new List<SavedActiveFiller>();
        }

        public static List<long> GetCompletedFillerState()
        {
            return Load()?.CompletedFiller ?? new List<long>();
        }

        public static void SaveFillerState(List<SavedActiveFiller> active, List<long> completed)
        {
            SlotSaveData data = Load();
            if (data == null) return;
            data.ActiveFiller = active;
            data.CompletedFiller = completed;
            Save(data);
        }

        private static SlotSaveData Load()
        {
            string fileKey = GetFileKey();
            if (fileKey == null) return null;
            if (cached != null && cachedKey == fileKey) return cached;

            var data = new SlotSaveData();
            string path = GetPath(fileKey);
            try
            {
                if (File.Exists(path))
                {
                    data = JsonConvert.DeserializeObject<SlotSaveData>(File.ReadAllText(path)) ?? new SlotSaveData();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to read save file {path}: {ex.Message}");
            }

            MergeLegacyTimeTrialFile(data);

            cached = data;
            cachedKey = fileKey;
            return data;
        }

        private static void Save(SlotSaveData data)
        {
            string fileKey = GetFileKey();
            if (fileKey == null) return;

            string path = GetPath(fileKey);
            try
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to write save file {path}: {ex.Message}");
            }
        }

        // Older versions stored time trial wins in {seed}_timetrials.txt (no slot name)
        private static void MergeLegacyTimeTrialFile(SlotSaveData data)
        {
            string seed = GetSeed();
            if (seed == null) return;

            string legacyPath = Application.persistentDataPath + $"/{seed}_timetrials.txt";
            try
            {
                if (!File.Exists(legacyPath)) return;
                foreach (string line in File.ReadAllLines(legacyPath))
                {
                    if (!string.IsNullOrWhiteSpace(line)) data.TimeTrialVictories.Add(line.Trim());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to merge legacy time trial file: {ex.Message}");
            }
        }

        private static string GetSeed()
        {
            var session = GarfieldKartAPMod.APClient?.GetSession();
            string seed = session?.RoomState?.Seed;
            return string.IsNullOrWhiteSpace(seed) ? null : seed;
        }

        // Seed + slot name is the key
        private static string GetFileKey()
        {
            string seed = GetSeed();
            string slot = GarfieldKartAPMod.APClient?.SlotName;
            if (seed == null || string.IsNullOrWhiteSpace(slot)) return null;
            return $"{seed}_{SanitizeForFileName(slot)}";
        }

        private static string GetPath(string fileKey)
        {
            return Application.persistentDataPath + $"/{fileKey}.json";
        }

        private static string SanitizeForFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return name;
        }
    }
}
