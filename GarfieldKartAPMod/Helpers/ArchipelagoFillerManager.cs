using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;
using System.Linq;

namespace GarfieldKartAPMod.Helpers
{
    // Tracks the filler/trap items the server has sent and decides when each may fire. The
    // effects themselves live in ArchipelagoFillerEffects and ArchipelagoTrapEffects.
    internal static class ArchipelagoFillerManager
    {
        public enum FillerKind
        {
            Instant,
            Duration
        }

        public class FillerItem
        {
            public long Id;
            public string Name;
            public string Description;
            public FillerKind Kind;
            // Instant items with no mid-race trigger: a copy arriving during a race has to wait
            // for the next one to start
            public bool RaceStartOnly;
        }

        // Marker for the items trap_handling applies to
        private class TrapItem : FillerItem
        {
        }

        public class ActiveFillerItem
        {
            public FillerItem Item;
            public int RemainingRaces;
            public float TrapSecondsActive;
        }

        public static readonly List<FillerItem> FillerItems = [
            // Filler items
            new FillerItem()
            {
                Id = ArchipelagoConstants.ITEM_RANDOM_ITEM_BOX_FILLER,
                Name = "Random Item Box",
                Description = "Grant a random item box upon receiving.",
                Kind = FillerKind.Instant
            },
            new FillerItem()
            {
                Id = ArchipelagoConstants.ITEM_START_BOOST_HELPER_FILLER,
                Name = "Start Boost Helper",
                Description = "Gives the player a guaranteed good starting boost.",
                Kind = FillerKind.Instant,
                RaceStartOnly = true
            },
            new FillerItem()
            {
                Id = ArchipelagoConstants.ITEM_QUOTE_FILLER,
                Name = "Inspirational Garfield Quote",
                Description = "Gives the player an inspirational garfield quote at the end of the race.",
                Kind = FillerKind.Duration
            },

            // Trap items
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_MIRROR_TRAP,
                Name = "Mirror Trap",
                Description = "Mirrors the player's controls.",
                Kind = FillerKind.Duration
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_SLEEP_TRAP,
                Name = "Sleep Trap",
                Description = "Puts the player to sleep.",
                Kind = FillerKind.Instant
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_GRAYSCALE_TRAP,
                Name = "Grayscale Trap",
                Description = "Turns the screen into grayscale.",
                Kind = FillerKind.Duration
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_BROKEN_DRIFT_TRAP,
                Name = "Broken Drift Trap",
                Description = "Weakens drift, making you unable to get the best drift.",
                Kind = FillerKind.Duration
            },
            new TrapItem()
            {
                Id = ArchipelagoConstants.ITEM_BOUNCE_TRAP,
                Name = "Bounce Trap",
                Description = "Makes the player have a spring bounce occur.",
                Kind = FillerKind.Instant
            }
        ];

        // Neither backlog is saved; both are rebuilt in LoadFillerFromReceivedItems
        private static readonly Dictionary<long, int> pendingInstant = new Dictionary<long, int>();
        private static readonly Dictionary<long, int> queuedDuration = new Dictionary<long, int>();
        private static readonly List<ActiveFillerItem> activeDuration = [];
        // The one instant copy of each kind allotted to the current race, rebuilt every race start
        private static readonly HashSet<long> armedInstant = new HashSet<long>();
        // Copies whose effect has been used up, so a resync doesn't hand them out again
        private static readonly List<long> usedFiller = [];

        public static FillerItem GetFillerById(long id)
        {
            return FillerItems.FirstOrDefault(item => item.Id == id);
        }

        // Every received item passes through here, most of which won't be filler. Main thread
        // only (ArchipelagoItemTracker.ProcessLiveReceivedItems), so IsRacing is safe to call.
        public static void TryReceiveFiller(long itemId)
        {
            FillerItem filler = GetFillerById(itemId);
            if (filler == null) return;

            if (filler.Kind == FillerKind.Instant)
            {
                Increment(pendingInstant, filler.Id);
                if (!filler.RaceStartOnly && ArchipelagoHelper.IsRacing()) armedInstant.Add(filler.Id);
                Log.Message($"[Filler] Received {filler.Name} ({PendingCount(filler.Id)} pending)");
                return;
            }

            // Use it now if we're on track, but only one copy of a kind is ever in hand at a time
            if (ArchipelagoHelper.IsRacing() && !IsFillerHeld(filler.Id))
            {
                Activate(filler);
                SaveFiller();
                return;
            }

            Increment(queuedDuration, filler.Id);
            Log.Message($"[Filler] Queued {filler.Name} ({queuedDuration[filler.Id]} in queue)");
        }

        // Runs once per race, before any effect goes looking for its filler
        public static void OnRaceStart()
        {
            armedInstant.Clear();
            foreach (KeyValuePair<long, int> entry in pendingInstant.Where(entry => entry.Value > 0))
                armedInstant.Add(entry.Key);

            // A trap still owed a race gets its full timer over again, so quitting out doesn't
            // eat into the time it's owed
            if (ArchipelagoHelper.GetTrapHandling() != ArchipelagoConstants.OPTION_TRAP_HANDLING_TIME)
            {
                foreach (ActiveFillerItem active in activeDuration.Where(active => active.Item is TrapItem))
                {
                    active.TrapSecondsActive = 0f;
                    GarfieldKartAPMod.APClient.QueueNotification($"{active.Item.Name} Activated!");
                }
            }

            foreach (long id in queuedDuration.Where(entry => entry.Value > 0).Select(entry => entry.Key).ToList())
            {
                FillerItem filler = GetFillerById(id);
                if (filler == null || IsFillerHeld(id)) continue;

                queuedDuration[id]--;
                Activate(filler);
            }

            Log.Message($"[Filler] Race started: {armedInstant.Count} armed, {activeDuration.Count} active");
            SaveFiller();
        }

        // Finishing a race discharges race/win traps; time traps answer to their timer instead
        public static void OnRaceEnd(bool wonRace)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            armedInstant.Clear();

            long trapHandling = ArchipelagoHelper.GetTrapHandling();
            for (int i = activeDuration.Count - 1; i >= 0; i--)
            {
                ActiveFillerItem active = activeDuration[i];

                if (active.Item is not TrapItem)
                {
                    active.RemainingRaces--;
                    if (active.RemainingRaces <= 0) Discharge(i);
                    continue;
                }

                if (trapHandling == ArchipelagoConstants.OPTION_TRAP_HANDLING_TIME) continue;
                // Anything short of a win and it's back next race
                if (trapHandling == ArchipelagoConstants.OPTION_TRAP_HANDLING_WIN && !wonRace) continue;

                Discharge(i);
            }

            SaveFiller();
        }

        // Ticked every frame the local player is actively racing
        public static void TickTrapTimers(float deltaSeconds)
        {
            bool anyFinished = false;
            long trapHandling = ArchipelagoHelper.GetTrapHandling();

            for (int i = activeDuration.Count - 1; i >= 0; i--)
            {
                ActiveFillerItem active = activeDuration[i];
                if (active.Item is not TrapItem) continue;
                if (HasRunItsTime(active)) continue;

                active.TrapSecondsActive += deltaSeconds;
                if (!HasRunItsTime(active)) continue;

                anyFinished = true;

                // A time trap is spent once its effect is off; a race/win trap only goes quiet
                if (trapHandling == ArchipelagoConstants.OPTION_TRAP_HANDLING_TIME)
                {
                    Discharge(i);
                    continue;
                }

                GarfieldKartAPMod.APClient.QueueNotification($"{active.Item.Name} Wore Off!");
                Log.Message($"[Filler] {active.Item.Name} wore off, still owed a race");
            }

            if (!anyFinished) return;
            SaveFiller();
        }

        // Re-checked every frame by the continuous effects, so one whose timer runs out mid-race
        // switches itself off
        public static bool IsFillerActive(long fillerId)
        {
            return activeDuration.Any(active => active.Item.Id == fillerId && !HasRunItsTime(active));
        }

        // In hand at all, effect running or not - gates pulling another copy off the queue
        private static bool IsFillerHeld(long fillerId)
        {
            return activeDuration.Any(active => active.Item.Id == fillerId);
        }

        // Traps run for a fixed slice of in-race time; the quote has no timer and lasts its race
        private static bool HasRunItsTime(ActiveFillerItem active)
        {
            return active.Item is TrapItem
                   && active.TrapSecondsActive >= ArchipelagoConstants.TRAP_DISABLE_SECONDS;
        }

        // Effects that can fail to land (nothing unlocked yet, bonus effects still loading) check
        // this first and only consume once they've actually fired
        public static bool IsFillerArmed(long fillerId)
        {
            return armedInstant.Contains(fillerId);
        }

        public static bool TryConsumeInstant(long fillerId)
        {
            if (!armedInstant.Remove(fillerId)) return false;

            Decrement(pendingInstant, fillerId);
            usedFiller.Add(fillerId);

            Log.Message($"[Filler] Used {GetFillerById(fillerId)?.Name} ({PendingCount(fillerId)} still pending)");
            SaveFiller();
            return true;
        }

        private static void Activate(FillerItem filler)
        {
            activeDuration.Add(new ActiveFillerItem { Item = filler, RemainingRaces = 1 });
            // The quote announces itself at the end of the race, so it doesn't need this
            if (filler.Id != ArchipelagoConstants.ITEM_QUOTE_FILLER)
                GarfieldKartAPMod.APClient.QueueNotification($"{filler.Name} Activated!");
            Log.Message($"[Filler] Activated {filler.Name}");
        }

        // The copy is spent - effect over, nothing left to come back for
        private static void Discharge(int index)
        {
            ActiveFillerItem active = activeDuration[index];
            activeDuration.RemoveAt(index);
            usedFiller.Add(active.Item.Id);

            Log.Message($"[Filler] Expired {active.Item.Name}");
            GarfieldKartAPMod.APClient.QueueNotification($"{active.Item.Name} Expired!");
        }

        // The save holds only what's active and what's used up, so anything else the server has
        // sent must still be waiting its turn. Runs on the socket thread, so no Unity API here -
        // activation is left to the next OnRaceStart.
        public static void LoadFillerFromReceivedItems(List<ItemInfo> itemList)
        {
            pendingInstant.Clear();
            queuedDuration.Clear();
            activeDuration.Clear();
            armedInstant.Clear();
            usedFiller.Clear();

            Dictionary<long, int> waiting = new Dictionary<long, int>();
            foreach (ItemInfo item in itemList.Where(item => GetFillerById(item.ItemId) != null))
                Increment(waiting, item.ItemId);

            foreach (long id in ApJsonSaveFile.GetCompletedFillerState())
            {
                if (GetFillerById(id) == null) continue;

                Decrement(waiting, id);
                usedFiller.Add(id);
            }

            foreach (ApJsonSaveFile.SavedActiveFiller saved in ApJsonSaveFile.GetActiveFillerState())
            {
                FillerItem filler = GetFillerById(saved.Id);
                // A save predating a reclassification can list an instant as active; leaving it
                // out of the subtraction drops it back into the pending pool
                if (filler == null || filler.Kind != FillerKind.Duration) continue;

                Decrement(waiting, saved.Id);
                activeDuration.Add(new ActiveFillerItem
                {
                    Item = filler,
                    RemainingRaces = saved.RemainingRaces,
                    TrapSecondsActive = saved.TrapSecondsActive
                });
            }

            foreach (KeyValuePair<long, int> entry in waiting.Where(entry => entry.Value > 0))
            {
                bool instant = GetFillerById(entry.Key).Kind == FillerKind.Instant;
                (instant ? pendingInstant : queuedDuration)[entry.Key] = entry.Value;
            }

            Log.Message($"[Filler] Rebuilt from {itemList.Count} server items: {activeDuration.Count} active, "
                        + $"{pendingInstant.Values.Sum()} pending, {queuedDuration.Values.Sum()} queued, "
                        + $"{usedFiller.Count} used");
            SaveFiller();
        }

        public static void SaveFiller()
        {
            List<ApJsonSaveFile.SavedActiveFiller> active = activeDuration.Select(item => new ApJsonSaveFile.SavedActiveFiller
            {
                Id = item.Item.Id,
                RemainingRaces = item.RemainingRaces,
                TrapSecondsActive = item.TrapSecondsActive
            }).ToList();

            ApJsonSaveFile.SaveFillerState(active, usedFiller.ToList());
        }

        private static int PendingCount(long id)
        {
            return pendingInstant.TryGetValue(id, out int count) ? count : 0;
        }

        private static void Increment(Dictionary<long, int> counts, long id)
        {
            counts.TryGetValue(id, out int count);
            counts[id] = count + 1;
        }

        private static void Decrement(Dictionary<long, int> counts, long id)
        {
            if (counts.TryGetValue(id, out int count) && count > 0) counts[id] = count - 1;
        }
    }
}
