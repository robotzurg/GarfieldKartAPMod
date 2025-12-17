using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using GarfieldKartAPMod.Helpers;

namespace GarfieldKartAPMod
{
    public class ArchipelagoItemTracker
    {
        // Thread-safe collections so background socket callbacks can't corrupt the state
        private static readonly ConcurrentDictionary<long, int> receivedItems = new ConcurrentDictionary<long, int>();
        private static readonly ConcurrentDictionary<long, byte> checkedLocations = new ConcurrentDictionary<long, byte>();

        public static void Initialize()
        {
            Log.Message("Initializing Archipelago Item Tracker");
        }

        // ========== ITEM METHODS ==========

        public static void AddReceivedItem(long itemId)
        {
            receivedItems.AddOrUpdate(itemId, 1, (_, existing) => existing + 1);
            ArchipelagoFillerManager.TryQueueFiller(itemId);
        }

        public static bool HasItem(long itemId)
        {
            return receivedItems.ContainsKey(itemId);
        }

        public static int AmountOfItem(long itemId)
        {
            return receivedItems.GetValueOrDefault(itemId, 0);
        }

        // ========== LOCATION METHODS ==========

        public static void AddCheckedLocation(long locationId)
        {
            checkedLocations.TryAdd(locationId, 0);
        }

        public static bool HasLocation(long locationId)
        {
            return checkedLocations.ContainsKey(locationId);
        }

        public static int GetCheckedLocationCount()
        {
            return checkedLocations.Count;
        }

        // ========== LOAD/CLEAR METHODS ==========

        public static void Clear()
        {
            receivedItems.Clear();
            checkedLocations.Clear();
            Log.Message("[AP] Cleared all received items and checked locations");
        }

        // Make LoadFromServer authoritative: clear local state then populate with server state.
        // Safe to call from background threads because collections are concurrent.
        public static void LoadFromServer()
        {
            try
            {
                var session = GarfieldKartAPMod.APClient.GetSession();
                if (session == null)
                    return;

                Clear();

                // Load items
                var itemsList = session.Items.AllItemsReceived?.ToList();
                if (itemsList != null)
                {
                    Log.Message($"[AP] Loading {itemsList.Count} items from server");
                    foreach (var item in itemsList)
                    {
                        Log.Message($"[AP] Item: {item.ItemName} (ID: {item.ItemId})");
                        receivedItems.AddOrUpdate(item.ItemId, 1, (_, existing) => existing + 1);
                    }

                    // Throw all the received items into the filler manager to load the state.
                    ArchipelagoFillerManager.LoadFillerFromReceivedItems(itemsList);
                }

                // Load locations
                List<long> locationsList = session.Locations.AllLocationsChecked?.ToList();
                if (locationsList != null)
                {
                    Log.Message($"[AP] Loading {locationsList.Count} checked locations from server");
                    foreach (var locationId in locationsList)
                    {
                        Log.Message($"[AP] Location checked: {locationId}");
                        checkedLocations.TryAdd(locationId, 0);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"[AP] LoadFromServer exception: {ex}");
            }
        }

        // Convenience: force a resyncing from the current session (call on reconnection)
        public static void ResyncFromServer()
        {
            Log.Message("[AP] Resyncing Archipelago state from server");
            LoadFromServer();
        }

        // ========== HELPER METHODS ==========

        public static int GetCupVictoryCount()
        {
            var cupVictories = new List<long>
            {
                ArchipelagoConstants.LOC_LASAGNA_CUP_VICTORY,
                ArchipelagoConstants.LOC_PIZZA_CUP_VICTORY,
                ArchipelagoConstants.LOC_BURGER_CUP_VICTORY,
                ArchipelagoConstants.LOC_ICE_CREAM_CUP_VICTORY
            };

            return cupVictories.Count(HasLocation);
        }

        public static int GetRaceVictoryCount()
        {
            int count = 0;

            for (int i = 0; i < 16; i++)
            {
                if (HasLocation(i))
                {
                    count++;
                }
            }

            return count;
        }

        // Time-trial wins are stored in a per-session file by FileWriter. Only count wins for the active session.
        public static int GetTimeTrialVictoryCount()
        {
            try
            {
                var session = GarfieldKartAPMod.APClient?.GetSession();
                if (session == null)
                    return 0;

                string sessionSeed = session.RoomState.Seed;
                if (string.IsNullOrWhiteSpace(sessionSeed))
                    return 0;

                string path = Application.persistentDataPath + $"/{sessionSeed}_timetrials.txt";
                if (!File.Exists(path))
                    return 0;

                var lines = File.ReadAllLines(path)
                                .Where(l => !string.IsNullOrWhiteSpace(l))
                                .Select(l => l.Trim())
                                .Distinct();

                return lines.Count();
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to read time-trial file: {ex.Message}");
                return 0;
            }
        }

        public static List<long> GetAvailableCups()
        {
            var cupUnlocks = new List<long>();

            for (int cupId = 0; cupId < 4; cupId++)
            {
                if (HasCup(cupId)) cupUnlocks.Add(cupId);
            }

            return cupUnlocks;
        }

        public static bool HasRace(int raceId)
        {
            // TODO: If we ever randomize which races are in which cups, this cupId -> raceId conversion will stop working
            int cupId = raceId / 4;

            bool raceRando = ArchipelagoHelper.IsRacesRandomized();
            bool cupRando = ArchipelagoHelper.IsCupsRandomized();
            bool cupAndRaceRando = ArchipelagoHelper.IsRacesAndCupsRandomized();

            if (!raceRando)
            {
                if (!cupRando) // This can never happen in settings but should be true if not connected
                    return true;

                return HasCup(cupId);
            }
            
            var baseRaceId = ArchipelagoConstants.ITEM_COURSE_UNLOCK_CATZ_IN_THE_HOOD;
            var raceItemId = baseRaceId + raceId;

            bool hasRace = HasItem(raceItemId);

            if (!cupRando) return hasRace;
            if (cupAndRaceRando) return hasRace;
            
            return hasRace && HasCup(cupId);
        }

        public static bool HasCup(int cupId)
        {
            Log.Message($"Checking cup access for {cupId}");
            bool cupRando = ArchipelagoHelper.IsCupsRandomized();
            bool cupAndRaceRando = ArchipelagoHelper.IsRacesAndCupsRandomized();

            Log.Message($"Cup randomizer is {cupRando}");
            if (!cupRando) 
                return true;

            bool progressiveCups = ArchipelagoHelper.IsProgressiveCupsEnabled();

            Log.Message($"Progressive cups are {progressiveCups}");
            if (progressiveCups)
            {
                if (cupAndRaceRando)
                    return HasAllRacesInCup(cupId + (int)ArchipelagoConstants.ITEM_CUP_UNLOCK_LASAGNA) && AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK) >= cupId;
                return AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK) >= cupId;
            }

            var baseCupId = ArchipelagoConstants.ITEM_CUP_UNLOCK_LASAGNA;
            var cupItemId = baseCupId + cupId;

            Log.Message($"Cup item ID: {cupItemId}");
            Log.Message($"Item present: {HasItem(cupItemId)}");
            return HasItem(cupItemId);
        }

        public static bool CanAccessCup(int cupId)
        {
            if (!HasCup(cupId)) return false;

            for (int raceId = cupId * 4; raceId < cupId * 5; raceId++)
            {
                if (!HasRace(raceId)) return false;
            }
            return true;
        }

        public static bool HasRaceInCup(int cupItemId)
        {
            int cupId = cupItemId - (int)ArchipelagoConstants.ITEM_CUP_UNLOCK_LASAGNA; // Convert 201-204 to 0-3
            int startRaceId = cupId * 4; // 0, 4, 8, 12

            for (int raceId = startRaceId; raceId < startRaceId + 4; raceId++)
            {
                if (HasRace(raceId)) return true;
            }

            return false;
        }

        public static bool HasAllRacesInCup(int cupItemId)
        {
            int cupId = cupItemId - (int)ArchipelagoConstants.ITEM_CUP_UNLOCK_LASAGNA; // Convert 201-204 to 0-3
            int startRaceId = cupId * 4; // 0, 4, 8, 12
            int raceCount = 0;

            for (int raceId = startRaceId; raceId < startRaceId + 4; raceId++)
            {
                if (HasRace(raceId)) raceCount++;
            }

            return raceCount == 4;
        }

        public static int GetPuzzlePieceCount(string startScene)
        {
            long basePuzzlePieceId = ArchipelagoConstants.GetPuzzlePieceLoc(startScene, 0);
            if (basePuzzlePieceId == -1)
                return 0;

            int count = 0;
            for (int i = 0; i < 3; i++)
            {
                if (HasLocation(basePuzzlePieceId + i))
                {
                    count++;
                }
            }
            return count;
        }

        public static int GetPuzzleLocationCount(string startScene)
        {
            long basePuzzlePieceId = ArchipelagoConstants.GetPuzzlePieceLoc(startScene, 0);
            if (basePuzzlePieceId == -1)
                return 0;

            int count = 0;
            for (int i = 0; i < 3; i++)
            {
                if (HasLocation(basePuzzlePieceId + i))
                {
                    count++;
                }
            }
            return count;
        }

        public static int GetOverallPuzzlePieceCount()
        {
            int count = 0;
            int startingIndex = (int)ArchipelagoConstants.ITEM_CATZ_IN_THE_HOOD_PUZZLE_PIECE_1;
            for (int i = startingIndex; i < startingIndex + 48; i++)
            {
                if (HasItem(i)) count++;
            }

            return count;
        }

        public static int GetCheckedPuzzlePieceCount() { 
            // Keeping this code in case we want to ever use it
            var tracks = new List<string>
            {
                "E2C1", "E4C1", "E3C1", "E1C1", // Lasagna Cup
                "E3C2", "E2C2", "E1C2", "E4C2", // Pizza Cup
                "E1C3", "E3C3", "E4C3", "E2C3", // Burger Cup
                "E4C4", "E1C4", "E2C4", "E3C4"  // Ice Cream Cup
            };

            int count = 0;
            foreach (var track in tracks)
            {
                count += GetPuzzlePieceCount(track);
            }
            return count;
        }

        public static bool HasBonusAvailable(BonusCategory bonus)
        {
            bool randomizeItems = ArchipelagoHelper.IsItemRandomizerEnabled();

            if (!randomizeItems) 
                return true;

            switch (bonus)
            {
                case BonusCategory.PIE:
                    return HasItem(ArchipelagoConstants.ITEM_PIE);
                case BonusCategory.AUTOLOCK_PIE:
                    return HasItem(ArchipelagoConstants.ITEM_HOMING_PIE);
                case BonusCategory.LASAGNA:
                    return HasItem(ArchipelagoConstants.ITEM_LASAGNA);
                case BonusCategory.SPRING:
                    return HasItem(ArchipelagoConstants.ITEM_SPRING);
                case BonusCategory.DIAMOND:
                    return HasItem(ArchipelagoConstants.ITEM_DIAMOND);
                case BonusCategory.UFO:
                    return HasItem(ArchipelagoConstants.ITEM_UFO);
                case BonusCategory.NAP:
                    return HasItem(ArchipelagoConstants.ITEM_PILLOW);
                case BonusCategory.PARFUME:
                    return HasItem(ArchipelagoConstants.ITEM_PERFUME);
                case BonusCategory.MAGIC:
                    return HasItem(ArchipelagoConstants.ITEM_MAGIC_WAND);
                default:
                    return true;
            }
        }

        // ========== DEBUG METHODS ==========

        public static void LogAllReceivedItems()
        {
            var total = receivedItems.Sum(kv => kv.Value);
            Log.Message($"[AP Debug] === All Received Items ({total} total entries) ===");
            foreach (var kv in receivedItems.OrderBy(kv => kv.Key))
            {
                Log.Message($"[AP Debug] Item ID: {kv.Key} Count: {kv.Value}");
            }
        }

        public static void LogAllCheckedLocations()
        {
            Log.Message($"[AP Debug] === All Checked Locations ({checkedLocations.Count} total) ===");
            foreach (var locationId in checkedLocations.Keys.OrderBy(x => x))
            {
                Log.Message($"[AP Debug] Location ID: {locationId}");
            }
        }
    }
}