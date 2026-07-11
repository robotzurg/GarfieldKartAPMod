using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;
using System.Linq;

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

        private class TrapItem : FillerItem 
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
                Id = ArchipelagoConstants.ITEM_RANDOM_ITEM_BOX_FILLER,
                Name = "Random Item Box",
                Description = "Grant a random item box upon receiving."
            },
            new FillerItem()
            {
                Id = ArchipelagoConstants.ITEM_START_BOOST_HELPER_FILLER,
                Name = "Start Boost Helper",
                Description = "Gives the player a guaranteed good starting boost."
            },
            new FillerItem()
            {
                Id = ArchipelagoConstants.ITEM_QUOTE_FILLER,
                Name = "Inspirational Garfield Quote",
                Description = "Gives the player an inspirational garfield quote at the end of the race."
            },

            // Trap items
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_MIRROR_TRAP,
                Name = "Mirror Trap",
                Description = "Mirrors the player's controls."
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_SLEEP_TRAP,
                Name = "Sleep Trap",
                Description = "Puts the player to sleep."
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_GRAYSCALE_TRAP,
                Name = "Grayscale Trap",
                Description = "Turns the screen into grayscale."
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_BROKEN_DRIFT_TRAP,
                Name = "Broken Drift Trap",
                Description = "Weakens drift, making you unable to get the best drift."
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_BOUNCE_TRAP,
                Name = "Bounce Trap",
                Description = "Makes the player have a spring bounce occur."
            }
        ];

        public static int RacesPerFiller { get; set; } = 1;
        public static int MaxConcurrentFiller { get; set; } = 1;


        public static Queue<FillerItem> fillerQueue = new Queue<FillerItem>();
        public static Queue<ActiveFillerItem> activeFiller = new Queue<ActiveFillerItem>();
        public static List<FillerItem> completedFiller = [];


        public static FillerItem GetFillerById(long id)
        {
            return FillerItems.FirstOrDefault(item => item.Id == id);
        }

        public static void TryQueueFiller(long itemId)
        {
            FillerItem fillerItem = GetFillerById(itemId);

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

        public static bool IsFillerActive(long fillerId)
        {
            return activeFiller.Any(fillerItem => fillerItem.Item.Id == fillerId);

        }
        public static void UpdateFillerQueue()
        {
            // Dequeue expired filler items
            while (activeFiller.Count > 0 && activeFiller.Peek().RemainingRaces == 0)
            {
                FillerItem completedFillerItem = activeFiller.Dequeue().Item;
                completedFiller.Add(completedFillerItem);
            }

            // Enqueue new filler items up to the limit 
            while (fillerQueue.Count > 0 && activeFiller.Count < MaxConcurrentFiller)
            {
                FillerItem filler = fillerQueue.Dequeue();
                ActiveFillerItem activeFillerItem = new ActiveFillerItem
                {
                    Item = filler,
                    RemainingRaces = RacesPerFiller
                };
                activeFiller.Enqueue(activeFillerItem);
            }

            // Save all changes
            SaveFiller();
        }

        // public static void ShowFillerPopup(ActiveFillerItem fillerItem)
        // {
        //     FillerItem item = fillerItem.Item;
        //     bool isTrap = item is TrapItem;
        //     PopupHD.POPUP_TYPE popupType = isTrap ? PopupHD.POPUP_TYPE.ERROR : PopupHD.POPUP_TYPE.INFORMATION;
        //     string typeString = isTrap ? "trap" : "filler";
        //     PopupManager.OpenPopup($"Temporary {typeString} effect activated: {item.Name}", popupType, PopupHD.POPUP_PRIORITY.NORMAL);
        // }

        public static void LoadFillerFromReceivedItems(List<ItemInfo> itemList)
        {
            List<long> completedIds = ApJsonSaveFile.GetCompletedFillerState();
            List<ApJsonSaveFile.SavedActiveFiller> activeSaved = ApJsonSaveFile.GetActiveFillerState();

            // Filter out non trap or filler items from the items
            Dictionary<long, int> fillerItemCounts = new Dictionary<long, int>();
            foreach (ItemInfo item in itemList.Where(item => GetFillerById(item.ItemId) != null))
            {
                fillerItemCounts.TryAdd(item.ItemId, 0);
                fillerItemCounts[item.ItemId]++;
            }

            foreach (long id in completedIds)
            {
                FillerItem item = GetFillerById(id);
                if (item == null) continue;

                if (fillerItemCounts.TryGetValue(id, out int count) && count > 0) fillerItemCounts[id] = count - 1;
                completedFiller.Add(item);
            }

            foreach (ApJsonSaveFile.SavedActiveFiller saved in activeSaved)
            {
                FillerItem item = GetFillerById(saved.Id);
                if (item == null) continue;

                if (fillerItemCounts.TryGetValue(saved.Id, out int count) && count > 0) fillerItemCounts[saved.Id] = count - 1;
                activeFiller.Enqueue(new ActiveFillerItem
                {
                    Item = item,
                    RemainingRaces = saved.RemainingRaces
                });
            }

            // Create a list of remaining items and shuffle it
            List<long> remainingItemIDs = [];
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

        public static void SaveFiller()
        {
            List<ApJsonSaveFile.SavedActiveFiller> active = activeFiller.Select(activeFillerItem => new ApJsonSaveFile.SavedActiveFiller
            {
                Id = activeFillerItem.Item.Id,
                RemainingRaces = activeFillerItem.RemainingRaces
            }).ToList();
            List<long> completed = completedFiller.Select(completedFillerItem => completedFillerItem.Id).ToList();

            ApJsonSaveFile.SaveFillerState(active, completed);
        }
    }
}
