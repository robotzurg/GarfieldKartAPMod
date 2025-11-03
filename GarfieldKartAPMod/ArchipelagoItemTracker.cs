using System.Collections.Generic;

namespace GarfieldKartAPMod
{
    public class ArchipelagoItemTracker
    {
        private static HashSet<string> receivedItems = new HashSet<string>();

        public static void Initialize()
        {
            Log.Message("Initializing Archipelago Item Tracker");
        }

        public static void AddReceivedItem(string itemName)
        {
            if (receivedItems.Add(itemName))
            {
                Log.Message($"[AP] Received item: {itemName}");

                // Unlock the item in the override system
                UnlockItem(itemName);
            }
        }

        public static bool HasItem(string itemName)
        {
            return receivedItems.Contains(itemName);
        }

        public static void Clear()
        {
            receivedItems.Clear();
            ArchipelagoUnlockOverride.Clear();
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
                    string itemName = session.Items.GetItemName(item.ItemId);
                    AddReceivedItem(itemName);
                }
            }
        }

        private static void UnlockItem(string itemName)
        {
            // Map Archipelago item names to game IDs and unlock them

            // Cups
            if (itemName.Contains("Cup"))
            {
                string cupId = itemName.Replace(" Cup", "").Replace(" ", "");
                ArchipelagoUnlockOverride.UnlockItem($"Cup_{cupId}");
                Log.Message($"[AP] Unlocked cup: {cupId}");
            }
        }
    }
}