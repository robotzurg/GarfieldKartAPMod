using System.Collections.Generic;
using System.Linq;

namespace GarfieldKartAPMod
{
    public class ArchipelagoItemTracker
    {
        private static HashSet<long> receivedItems = new HashSet<long>();

        public static void Initialize()
        {
            Log.Message("Initializing Archipelago Item Tracker");
        }

        public static void AddReceivedItem(long itemId)
        {
            receivedItems.Add(itemId);
        }

        public static bool HasItem(long itemId)
        {
            return receivedItems.Contains(itemId);
        }

        public static void Clear()
        {
            receivedItems.Clear();
            Log.Message("[AP] Cleared all received items");
        }

        public static void LoadFromServer()
        {
            // Called when connecting to load all items we've already received
            var session = GarfieldKartAPMod.APClient.GetSession();
            if (session != null)
            {
                var items = session.Items.AllItemsReceived;
                Log.Message($"[AP] Loading {items.Count} items from server");

                foreach (var item in items)
                {
                    Log.Message($"{item.ItemName} {item.ItemId}");
                    AddReceivedItem(item.ItemId);
                }
            }
        }

        private static void UnlockItem(long itemId)
        {
            AddReceivedItem(itemId);
        }

        public static int GetPuzzlePieceCount(string startScene)
        {
            long basePuzzlePieceId = ArchipelagoConstants.GetPuzzlePiece(startScene, 0);
            if (basePuzzlePieceId == -1)
                return 0;

            int count = 0;
            for (int i = 0; i < 3; i++) // Each track has 3 puzzle pieces
            {
                if (HasItem(basePuzzlePieceId + i))
                {
                    count++;
                }
            }
            return count;
        }

        public static void LogAllReceivedItems()
        {
            Log.Message($"[AP Debug] Total items received: {receivedItems.Count}");
            foreach (var itemId in receivedItems.OrderBy(x => x))
            {
                Log.Message($"[AP Debug] Item ID: {itemId}");
            }
        }
    }
}