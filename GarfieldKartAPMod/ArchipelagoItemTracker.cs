using System.Collections.Generic;
using System.Linq;

namespace GarfieldKartAPMod
{
    public class ArchipelagoItemTracker
    {
        private static List<long> receivedItems = new List<long>();
        private static HashSet<long> checkedLocations = new HashSet<long>();

        public static void Initialize()
        {
            Log.Message("Initializing Archipelago Item Tracker");
        }

        // ========== ITEM METHODS ==========

        public static void AddReceivedItem(long itemId)
        {
            receivedItems.Add(itemId);
        }

        public static bool HasItem(long itemId)
        {
            return receivedItems.Contains(itemId);
        }

        public static int AmountOfItem(long itemId)
        {
            return receivedItems.Count(x => x == itemId);
        }

        // ========== LOCATION METHODS ==========

        public static void AddCheckedLocation(long locationId)
        {
            checkedLocations.Add(locationId);
        }

        public static bool HasLocation(long locationId)
        {
            return checkedLocations.Contains(locationId);
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

        public static void LoadFromServer()
        {
            var session = GarfieldKartAPMod.APClient.GetSession();
            if (session != null)
            {
                // Load items
                var items = session.Items.AllItemsReceived;
                Log.Message($"[AP] Loading {items.Count} items from server");
                foreach (var item in items)
                {
                    Log.Message($"[AP] Item: {item.ItemName} (ID: {item.ItemId})");
                    AddReceivedItem(item.ItemId);
                }

                // Load locations
                var locations = session.Locations.AllLocationsChecked;
                Log.Message($"[AP] Loading {locations.Count} checked locations from server");
                foreach (var locationId in locations)
                {
                    Log.Message($"[AP] Location checked: {locationId}");
                    AddCheckedLocation(locationId);
                }
            }
        }

        // ========== HELPER METHODS ==========

        public static int GetCupVictoryCount()
        {
            long[] cupVictories = new[]
            {
                ArchipelagoConstants.LOC_LASAGNA_CUP_VICTORY,
                ArchipelagoConstants.LOC_PIZZA_CUP_VICTORY,
                ArchipelagoConstants.LOC_BURGER_CUP_VICTORY,
                ArchipelagoConstants.LOC_ICE_CREAM_CUP_VICTORY
            };

            return cupVictories.Count(loc => HasLocation(loc));
        }

        public static int GetPuzzlePieceCount(string startScene)
        {
            long basePuzzlePieceId = ArchipelagoConstants.GetPuzzlePiece(startScene, 0);
            if (basePuzzlePieceId == -1)
                return 0;

            int count = 0;
            for (int i = 0; i < 3; i++)
            {
                if (HasItem(basePuzzlePieceId + i))
                {
                    count++;
                }
            }
            return count;
        }

        public static int GetOverallPuzzlePieceCount()
        {
            var tracks = new[]
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

        // ========== DEBUG METHODS ==========

        public static void LogAllReceivedItems()
        {
            Log.Message($"[AP Debug] === All Received Items ({receivedItems.Count} total) ===");
            foreach (var itemId in receivedItems.OrderBy(x => x))
            {
                Log.Message($"[AP Debug] Item ID: {itemId}");
            }
        }

        public static void LogAllCheckedLocations()
        {
            Log.Message($"[AP Debug] === All Checked Locations ({checkedLocations.Count} total) ===");
            foreach (var locationId in checkedLocations.OrderBy(x => x))
            {
                Log.Message($"[AP Debug] Location ID: {locationId}");
            }
        }
    }
}