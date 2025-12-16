using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarfieldKartAPMod.Helpers
{
    internal class ArchipelagoFillerManager
    {
        public class FillerItem
        {
            public long Id;
            public string Name;
            public string Description;
        }
        public class TrapItem : FillerItem 
        { 
        }

        public class ActiveFillerItem
        {
            public FillerItem Item { get; set; }
            public int RemainingRaces { get; set; }
        }

        public static readonly List<FillerItem> FillerItems = [
            // Filler items
            new FillerItem()
            {
                Id = ArchipelagoConstants.ITEM_FILLER,
                Name = "Filler Item",
                Description = "Dummy filler item without any effect."
            },

            // Trap items
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_TRAP,
                Name = "Trap Item",
                Description = "Dummy trap item without any effect."
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_HANDLING_TRAP,
                Name = "Handling Trap",
                Description = "Drastically increases the player's handling."
            }
        ];

        public static int RacesPerFiller { get; set; } = 1;
        public static int MaxConcurrentFiller { get; set; } = 1;


        public static Queue<FillerItem> fillerQueue = new();
        public static Queue<ActiveFillerItem> activeFiller = new();
        public static List<FillerItem> completedFiller = [];


        public static FillerItem GetFillerById(long id)
        {
            foreach (FillerItem item in FillerItems)
            {
                if (item.Id == id) return item;
            }

            return null;
        }

        public static void TryQueueFiller(long itemID)
        {
            FillerItem fillerItem = GetFillerById(itemID);

            // We're just throwing all items into this function, most of which won't be filler
            if (fillerItem == null) return;
            QueueFiller(fillerItem);
        }

        public static void QueueFiller(FillerItem filler)
        {
            fillerQueue.Enqueue(filler);
            UpdateFillerQueue();
        }

        public static void WinRace()
        {
            foreach (ActiveFillerItem activeFillerItem in activeFiller)
            {
                activeFillerItem.RemainingRaces--;
            }

            UpdateFillerQueue();
        }

        public static bool IsFillerActive(long fillerID)
        {
            foreach (ActiveFillerItem fillerItem in activeFiller)
            {
                if (fillerItem.Item.Id == fillerID) return true;
            }

            return false;
        }
        public static void UpdateFillerQueue()
        {
            // Dequeue expired filler items
            while (activeFiller.Peek().RemainingRaces == 0)
            {
                FillerItem completedFillerItem = activeFiller.Dequeue().Item;
                completedFiller.Add(completedFillerItem);
            }

            // Enqueue new filler items up to the limit 
            while (fillerQueue.Count > 0 && activeFiller.Count < MaxConcurrentFiller)
            {
                FillerItem filler = fillerQueue.Dequeue();
                ActiveFillerItem activeFillerItem = new()
                {
                    Item = filler,
                    RemainingRaces = RacesPerFiller
                };
                activeFiller.Enqueue(activeFillerItem);
                ShowFillerPopup(activeFillerItem);
            }

            // Save all changes
            SaveFiller();
        }

        public static void ShowFillerPopup(ActiveFillerItem fillerItem)
        {
            FillerItem item = fillerItem.Item;
            bool isTrap = item is TrapItem;
            PopupHD.POPUP_TYPE popupType = isTrap ? PopupHD.POPUP_TYPE.ERROR : PopupHD.POPUP_TYPE.INFORMATION;
            string typeString = isTrap ? "trap" : "filler";
            PopupManager.OpenPopup($"Temporary {typeString} effect activated: {item.Name}", PopupHD.POPUP_TYPE.ERROR, PopupHD.POPUP_PRIORITY.NORMAL);
        }

        public static void LoadFillerFromReceivedItems(List<ItemInfo> itemList)
        {
            // Hi Jeff, I'll be honest I'm feeling a little dizzy while writing this so this function is going to be written
            // especially poorly and I'd really appreciate if you could rewrite it.
            // ~Felucia
            Queue<ActiveFillerItem> fillerSaveData = LoadFiller();

            // Filter out non trap or filler items from the items
            Dictionary<long, int> fillerItemCounts = new();
            foreach (ItemInfo item in itemList)
            {
                if (GetFillerById(item.ItemId) == null)
                {
                    continue;
                }

                if (!fillerItemCounts.ContainsKey(item.ItemId)) fillerItemCounts.Add(item.ItemId, 0);
                fillerItemCounts[item.ItemId]++;
            }

            while (fillerSaveData.Count > 0)
            {
                ActiveFillerItem item = fillerSaveData.Dequeue();
                long itemId = item.Item.Id;
                fillerItemCounts[itemId]--;
                if (item.RemainingRaces == 0)
                {
                    completedFiller.Add(item.Item);
                }
                else
                {
                    activeFiller.Enqueue(item);
                }
            }

            // Create a list of remaining items and shuffle it
            // this should technically be in order of received traps
            // but it really doesn't matter and this is way cleaner than the code I had.
            // If you don't like it, revert the commit that added this comment.
            List<long> remainingItemIDs = new();
            foreach (long id in fillerItemCounts.Keys)
            {
                while (fillerItemCounts[id] > 0)
                {
                    remainingItemIDs.Add(id);
                    fillerItemCounts[id]--;
                }
            }

            Utils.Shuffle(remainingItemIDs);
            foreach (long id in remainingItemIDs)
            {
                fillerQueue.Enqueue(GetFillerById(id));
            }
        }
        public static Queue<ActiveFillerItem> LoadFiller()
        {
            var queue = new Queue<ActiveFillerItem>();

            // TODO: Actually load a string instead of just initialising an empty string.
            string saveDataString = "";
            string[] saveData = saveDataString.Split(',');
            foreach (string entry in saveData)
            {
                string[] parts = entry.Split('-');

                ActiveFillerItem item = new()
                {
                    Item = GetFillerById(Int64.Parse(parts[0])),
                    RemainingRaces = Int32.Parse(parts[1])
                };
                queue.Enqueue(item);
            }

            return queue;
        }

        public static void SaveFiller()
        {
            // Save current filler state to a file. This needs to save:
            // - activeFiller contents (currently active filler item IDs and their remaining races
            // - completedFiller contents (IDs only)
            // 
            // fillerQueue does not need to be stored, because it can be reconstructed from the server (If you know which filler items are
            // active or completed, you know everything else should be in the queue.

            List<string> saveData = [];
            foreach (var completedFillerItem in completedFiller)
            {
                saveData.Add($"{completedFillerItem.Id}-0");
            }

            foreach (var activeFillerItem in activeFiller)
            {
                saveData.Add($"{activeFillerItem.Item.Id}-{activeFillerItem.RemainingRaces}");
            }

            string saveDataString = string.Join(",", saveData);
            // TODO: Actually saving this is not implemented yet! 
        }
    }
}
