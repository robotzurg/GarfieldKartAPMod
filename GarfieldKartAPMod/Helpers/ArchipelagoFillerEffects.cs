using Aube;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GarfieldKartAPMod.Helpers
{
    // Applies the in-game effects of the non-trap filler items managed by ArchipelagoFillerManager
    internal static class ArchipelagoFillerEffects
    {
        private static readonly Dictionary<BonusCategory, string> BonusDisplayNames = new Dictionary<BonusCategory, string>
        {
            { BonusCategory.PIE, "Pie" },
            { BonusCategory.AUTOLOCK_PIE, "Homing Pie" },
            { BonusCategory.SPRING, "Spring" },
            { BonusCategory.LASAGNA, "Lasagna" },
            { BonusCategory.DIAMOND, "Diamond" },
            { BonusCategory.UFO, "UFO" },
            { BonusCategory.NAP, "Pillow" },
            { BonusCategory.PARFUME, "Perfume" },
            { BonusCategory.MAGIC, "Magic Wand" }
        };

        // Guarantee the best possible boost off the starting line. Nothing can stop the boost
        // applying, so the copy is consumed up front. Call right before the boost is applied
        // (Kart.StartRace -> KartBonusMgr.StartRace).
        public static void TryApplyStartBoost(Kart kart)
        {
            if (!ArchipelagoFillerManager.TryConsumeInstant(ArchipelagoConstants.ITEM_START_BOOST_HELPER_FILLER)) return;

            kart.BoostStartType = Kart.BoostType.SUPER;
            GarfieldKartAPMod.APClient?.QueueNotification("Start Boost Helper used: perfect start boost!");
            Log.Message("[Filler] Start Boost Helper consumed: guaranteed super start boost");
        }

        // Call from the race-end hook before OnRaceEnd ticks the filler down, or the quote it's
        // active for won't fire
        public static void TryShowGarfieldQuote()
        {
            if (!ArchipelagoFillerManager.IsFillerActive(ArchipelagoConstants.ITEM_QUOTE_FILLER)) return;

            ArchipelagoPopupManager.ShowRandomGarfieldQuote();
            Log.Message("[Filler] Inspirational Garfield Quote shown");
        }

        // Give a random unlocked item the next time a slot is empty. Polled at race start and
        // again whenever the player uses an item, since a full slot can't take the box.
        public static void TryGrantRandomItemBox(KartBonusMgr bonusMgr)
        {
            if (bonusMgr == null || !bonusMgr.CanGetItem()) return;
            if (!ArchipelagoFillerManager.IsFillerArmed(ArchipelagoConstants.ITEM_RANDOM_ITEM_BOX_FILLER)) return;

            List<BonusCategory> available = BonusDisplayNames.Keys
                .Where(ArchipelagoItemTracker.HasBonusAvailable)
                .ToList();
            if (Singleton<BonusMgr>.Instance != null && Singleton<BonusMgr>.Instance.IsUfoInUse)
                available.Remove(BonusCategory.UFO);

            // Nothing unlocked yet - leave the copy pending until an item unlock arrives
            if (available.Count == 0) return;

            if (!ArchipelagoFillerManager.TryConsumeInstant(ArchipelagoConstants.ITEM_RANDOM_ITEM_BOX_FILLER)) return;

            BonusCategory bonus = available[Random.Range(0, available.Count)];
            bonusMgr.SetItem(bonus, 1, isFromCheat: true);
            GarfieldKartAPMod.APClient?.QueueNotification($"Random Item Box opened: {BonusDisplayNames[bonus]}!");
            Log.Message($"[Filler] Random Item Box granted {bonus}");
        }
    }
}
