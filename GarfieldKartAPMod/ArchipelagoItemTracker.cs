using System.Collections.Generic;

namespace GarfieldKartAPMod
{
    public class ArchipelagoItemTracker
    {
        private static HashSet<string> receivedItems = new HashSet<string>();
        private static Dictionary<string, string> cupToItemMap = new Dictionary<string, string>();

        public static void Initialize()
        {
            Log.Message("Initializing Archipelago Item Tracker");

            // TODO: Update these to be accurate
            cupToItemMap["IceCream"] = "Ice Cream Cup";
            cupToItemMap["Lasagna"] = "Lasagna Cup";
            cupToItemMap["Pizza"] = "Pizza Cup";
            cupToItemMap["Pastacup"] = "Pasta Cup";

        }

        public static void AddReceivedItem(string itemName)
        {
            if (receivedItems.Add(itemName))
            {
                Log.Message($"[AP] Received item: {itemName}");
            }
        }

        public static bool HasItem(string itemName)
        {
            return receivedItems.Contains(itemName);
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
                    string itemName = session.Items.GetItemName(item.ItemId);
                    AddReceivedItem(itemName);
                }
            }
        }
    }
}