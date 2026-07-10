using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace GarfieldKartAPMod
{
    // Local, per-seed record of what the player has actually won, because collect is STUPID
    // This entire file should be taken as a strike against collect because it if was removed I would be happy
    // I hate it with all of its soul, but this fixes problems with it, so there ya go.
    // - Jeff
    public static class GoalProgressStore
    {
        private class GoalProgress
        {
            public HashSet<string> RaceVictories = new HashSet<string>();
            public HashSet<int> CupVictories = new HashSet<int>();
            public HashSet<string> TimeTrialVictories = new HashSet<string>();
        }

        private static GoalProgress cached;
        private static string cachedKey;

        public static void RecordRaceVictory(string track)
        {
            GoalProgress progress = Load();
            if (progress == null) return;
            if (progress.RaceVictories.Add(track)) Save(progress);
        }

        public static void RecordCupVictory(int cupId)
        {
            GoalProgress progress = Load();
            if (progress == null) return;
            if (progress.CupVictories.Add(cupId)) Save(progress);
        }

        public static void RecordTimeTrialVictory(string track)
        {
            GoalProgress progress = Load();
            if (progress == null) return;
            if (progress.TimeTrialVictories.Add(track)) Save(progress);
        }

        public static int GetRaceVictoryCount() => Load()?.RaceVictories.Count ?? 0;
        public static int GetCupVictoryCount() => Load()?.CupVictories.Count ?? 0;
        public static int GetTimeTrialVictoryCount() => Load()?.TimeTrialVictories.Count ?? 0;

        private static string GetSeed()
        {
            var session = GarfieldKartAPMod.APClient?.GetSession();
            string seed = session?.RoomState?.Seed;
            return string.IsNullOrWhiteSpace(seed) ? null : seed;
        }

        // Seed + slot name identify the progress file, so two slots from the same
        // multiworld played on one PC don't share progress
        private static string GetFileKey()
        {
            string seed = GetSeed();
            string slot = GarfieldKartAPMod.APClient?.SlotName;
            if (seed == null || string.IsNullOrWhiteSpace(slot)) return null;
            return $"{seed}_{SanitizeForFileName(slot)}";
        }

        private static string SanitizeForFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        private static string GetPath(string fileKey)
        {
            return Application.persistentDataPath + $"/{fileKey}_goalprogress.json";
        }

        private static GoalProgress Load()
        {
            string fileKey = GetFileKey();
            if (fileKey == null) return null;
            if (cached != null && cachedKey == fileKey) return cached;

            var progress = new GoalProgress();
            string path = GetPath(fileKey);
            try
            {
                if (File.Exists(path))
                {
                    progress = JsonConvert.DeserializeObject<GoalProgress>(File.ReadAllText(path)) ?? new GoalProgress();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to read goal progress file: {ex.Message}");
            }

            MergeLegacyTimeTrialFile(progress);

            cached = progress;
            cachedKey = fileKey;
            return progress;
        }

        // Older versions stored time trial wins in {seed}_timetrials.txt (no slot name)
        private static void MergeLegacyTimeTrialFile(GoalProgress progress)
        {
            string seed = GetSeed();
            if (seed == null) return;

            string legacyPath = Application.persistentDataPath + $"/{seed}_timetrials.txt";
            try
            {
                if (!File.Exists(legacyPath)) return;
                foreach (string line in File.ReadAllLines(legacyPath))
                {
                    if (!string.IsNullOrWhiteSpace(line)) progress.TimeTrialVictories.Add(line.Trim());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to merge legacy time trial file: {ex.Message}");
            }
        }

        private static void Save(GoalProgress progress)
        {
            string fileKey = GetFileKey();
            if (fileKey == null) return;
            try
            {
                File.WriteAllText(GetPath(fileKey), JsonConvert.SerializeObject(progress, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to write goal progress file: {ex.Message}");
            }
        }
    }
}
