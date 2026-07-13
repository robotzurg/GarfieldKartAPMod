using Archipelago.MultiClient.Net.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        // How far into session.Items.AllItemsReceived the main-thread live poll has processed.
        // Kept in sync with every full LoadFromServer rebuild so the two can't double-count.
        private static int liveItemCursor;

        public static void Initialize()
        {
            Log.Message("Initializing Archipelago Item Tracker");
        }

        // ========== ITEM METHODS ==========

        public static void AddReceivedItem(long itemId)
        {
            receivedItems.AddOrUpdate(itemId, 1, (_, existing) => existing + 1);
            ArchipelagoFillerManager.TryReceiveFiller(itemId);
        }

        // Handle items that land mid-session (e.g. during a race) the moment they arrive
        // rather than waiting for the next menu resync. Polled from the main thread so it
        // can safely mutate the non-concurrent filler queues and call into Unity; the socket
        // thread only ever appends to AllItemsReceived, which is index-stable and growing.
        public static void ProcessLiveReceivedItems()
        {
            ReadOnlyCollection<ItemInfo> allItems = GarfieldKartAPMod.APClient?.GetSession()?.Items?.AllItemsReceived;
            if (allItems == null) return;

            for (; liveItemCursor < allItems.Count; liveItemCursor++)
            {
                long itemId = allItems[liveItemCursor].ItemId;
                AddReceivedItem(itemId);
                ArchipelagoTrapEffects.OnMidRaceReceive(itemId);
            }
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
                    foreach (ItemInfo item in itemsList)
                    {
                        Log.Message($"[AP] Item: {item.ItemName} (ID: {item.ItemId})");
                        receivedItems.AddOrUpdate(item.ItemId, 1, (_, existing) => existing + 1);
                    }

                    // Throw all the received items into the filler manager to load the state.
                    ArchipelagoFillerManager.LoadFillerFromReceivedItems(itemsList);
                }

                // This rebuild already accounts for every item received so far, so the live
                // poll should only handle items that arrive after this point
                liveItemCursor = session.Items?.Index ?? liveItemCursor;

                // Load locations
                List<long> locationsList = session.Locations.AllLocationsChecked?.ToList();
                if (locationsList == null) return;
                Log.Message($"[AP] Loading {locationsList.Count} checked locations from server");
                foreach (long locationId in locationsList)
                {
                    Log.Message($"[AP] Location checked: {locationId}");
                    checkedLocations.TryAdd(locationId, 0);
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
            // raceId is 0-15
            int cupId = raceId / 4;

            bool raceRando = ArchipelagoHelper.IsRacesRandomized();
            bool cupRando = ArchipelagoHelper.IsCupsRandomized();
            
            if (cupRando && !raceRando )
            {
                return HasCup(cupId);
            }
            
            long raceItemId = ArchipelagoConstants.ITEM_COURSE_UNLOCK_CATZ_IN_THE_HOOD + raceId;
            return HasItem(raceItemId);
        }

        public static bool HasCup(int cupId)
        {
            bool cupRando = ArchipelagoHelper.IsCupsRandomized();
            bool raceRando = ArchipelagoHelper.IsRacesRandomized();
            
            if (raceRando && !cupRando)
            {
                return HasAllRacesInCup(cupId);
            }

            if (cupRando)
            {
                if (ArchipelagoHelper.IsProgressiveCupsEnabled())
                {
                    return AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK) >= cupId;
                }

                long cupItemId = ArchipelagoConstants.ITEM_CUP_UNLOCK_LASAGNA + cupId;
                return HasItem(cupItemId);
            }
            
            return true;
        }

        public static bool CanAccessCup(int cupId)
        {
            if (!HasCup(cupId)) 
                return false;

            return HasAllRacesInCup(cupId);
        }

        public static bool HasRaceInCup(int cupId)
        {
            int startRaceId = cupId * 4;

            for (int i = 0; i < 4; i++)
            {
                if (HasRace(startRaceId + i)) 
                    return true;
            }

            return false;
        }

        public static bool HasAllRacesInCup(int cupId)
        {
            int startRaceId = cupId * 4;

            for (int i = 0; i < 4; i++)
            {
                if (!HasRace(startRaceId + i))
                    return false;
            }

            return true;
        }

        // A course's time trials are accessible once the course can be reached by
        // ANY unlock - its own course unlock OR the cup it belongs to
        public static bool CanAccessTimeTrial(int raceId)
        {
            bool raceRando = ArchipelagoHelper.IsRacesRandomized();
            bool cupRando = ArchipelagoHelper.IsCupsRandomized();

            if (!raceRando && !cupRando)
                return true;

            if (raceRando && HasItem(ArchipelagoConstants.ITEM_COURSE_UNLOCK_CATZ_IN_THE_HOOD + raceId))
                return true;

            if (cupRando)
            {
                int cupId = raceId / 4;
                if (ArchipelagoHelper.IsProgressiveCupsEnabled())
                {
                    return AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK) >= cupId;
                }

                return HasItem(ArchipelagoConstants.ITEM_CUP_UNLOCK_LASAGNA + cupId);
            }

            return false;
        }

        public static bool HasTimeTrialInCup(int cupId)
        {
            int startRaceId = cupId * 4;

            for (int i = 0; i < 4; i++)
            {
                if (CanAccessTimeTrial(startRaceId + i))
                    return true;
            }

            return false;
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

        public static int GetOverallPuzzlePieceCount()
        {
            return AmountOfItem(ArchipelagoConstants.ITEM_PUZZLE_PIECE);
        }

        // public static int GetCheckedPuzzlePieceCount() { 
        //     // Keeping this code in case we want to ever use it
        //     var tracks = new List<string>
        //     {
        //         "E2C1", "E4C1", "E3C1", "E1C1", // Lasagna Cup
        //         "E3C2", "E2C2", "E1C2", "E4C2", // Pizza Cup
        //         "E1C3", "E3C3", "E4C3", "E2C3", // Burger Cup
        //         "E4C4", "E1C4", "E2C4", "E3C4"  // Ice Cream Cup
        //     };
        //
        //     int count = 0;
        //     foreach (var track in tracks)
        //     {
        //         count += GetPuzzlePieceCount(track);
        //     }
        //     return count;
        // }

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