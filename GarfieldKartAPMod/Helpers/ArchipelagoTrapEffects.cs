using Aube;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace GarfieldKartAPMod.Helpers
{
    // Applies the in-game effects of trap items managed by ArchipelagoFillerManager.
    //
    // Duration traps are a live function of "trap active AND player racing", re-evaluated every
    // frame from Update(), so one expiring mid-race turns its effect off on its own. Broken
    // Drift isn't here: the Kart.UpdateDriftState patch reads IsFillerActive directly.
    internal static class ArchipelagoTrapEffects
    {

        private static PostProcessVolume grayscaleVolume;
        private static bool grayscaleActive;
        private static bool mirrorAppliedByUs;

        // The timed-trap countdown itself lives in GarfieldKartAPMod.TickActiveTrapTimers
        public static void Update()
        {
            Kart localKart = GetLocalHumanKart();
            bool racing = IsRacing(localKart);

            SyncMirror(localKart, racing);
            SyncGrayscale(racing);
        }
        
        public static void OnRaceStart(Kart kart)
        {
            if (kart == null) return;

            TryFireInstantTrap(kart, ArchipelagoConstants.ITEM_SLEEP_TRAP);
            TryFireInstantTrap(kart, ArchipelagoConstants.ITEM_BOUNCE_TRAP);
        }

        // Duration traps are picked up by the per-frame Update() sync once active, so only the
        // instant ones need handling here - they've already missed OnRaceStart.
        public static void OnMidRaceReceive(long itemId)
        {
            if (itemId != ArchipelagoConstants.ITEM_SLEEP_TRAP
                && itemId != ArchipelagoConstants.ITEM_BOUNCE_TRAP) return;

            Kart kart = GetLocalHumanKart();
            if (!IsRacing(kart)) return;

            TryFireInstantTrap(kart, itemId);
        }

        public static void ClearAll()
        {
            SyncMirror(GetLocalHumanKart(), false);
            SyncGrayscale(false);
        }

        // Only consumed once the effect has actually landed, so a trap arriving before the kart's
        // bonus effects have loaded stays pending for the next race instead of being swallowed.
        private static void TryFireInstantTrap(Kart kart, long itemId)
        {
            if (!ArchipelagoFillerManager.IsFillerArmed(itemId)) return;

            BonusEffectMgr mgr = kart.GetBonusMgr()?.GetBonusEffectMgr();
            if (mgr == null || !mgr.AreEffectsLoaded) return;

            if (itemId == ArchipelagoConstants.ITEM_SLEEP_TRAP)
            {
                // Nap reads its launcher when computing duration; the player is both here
                if (mgr.GetBonusEffect(EBonusEffect.BONUSEFFECT_SLEPT) is NapBonusEffect nap)
                    nap.Launcher = kart;

                mgr.ActivateBonusEffect(EBonusEffect.BONUSEFFECT_SLEPT);
                Log.Message("[Trap] Sleep Trap put the player to sleep");
            }
            else
            {
                mgr.ActivateBonusEffect(EBonusEffect.BONUSEFFECT_JUMP);
                Log.Message("[Trap] Bounce Trap sprung the player into the air");
            }

            ArchipelagoFillerManager.TryConsumeInstant(itemId);
        }

        private static void SyncMirror(Kart kart, bool racing)
        {
            bool wantMirror = racing
                && ArchipelagoFillerManager.IsFillerActive(ArchipelagoConstants.ITEM_MIRROR_TRAP);
            
            if (!wantMirror && !mirrorAppliedByUs) return;

            RcHumanController controller = kart?.Driver?.GetComponentInChildren<RcHumanController>();

            if (wantMirror)
            {
                // Re-apply every frame: each race spawns a fresh controller that defaults off
                if (controller != null)
                {
                    controller.SetMirrorMode(true);
                    mirrorAppliedByUs = true;
                }
                return;
            }

            controller?.SetMirrorMode(false);
            mirrorAppliedByUs = false;
        }

        private static void SyncGrayscale(bool racing)
        {
            bool wantGray = racing
                && ArchipelagoFillerManager.IsFillerActive(ArchipelagoConstants.ITEM_GRAYSCALE_TRAP);
            if (wantGray == grayscaleActive) return;

            if (wantGray) EnableGrayscale();
            else DisableGrayscale();
        }

        private static void EnableGrayscale()
        {
            if (grayscaleVolume == null)
            {
                ColorGrading grading = ScriptableObject.CreateInstance<ColorGrading>();
                grading.enabled.Override(true);
                grading.gradingMode.Override(GradingMode.LowDefinitionRange);
                grading.saturation.Override(-100f);

                grayscaleVolume = PostProcessManager.instance.QuickVolume(GetPostProcessLayer(), 1000f, grading);
                Log.Message("[Trap] Grayscale Trap enabled");
            }

            grayscaleActive = true;
        }

        private static void DisableGrayscale()
        {
            if (grayscaleVolume != null)
            {
                RuntimeUtilities.DestroyVolume(grayscaleVolume, destroyProfile: true, destroyGameObject: true);
                grayscaleVolume = null;
                Log.Message("[Trap] Grayscale Trap disabled");
            }

            grayscaleActive = false;
        }

        // A QuickVolume is only seen by a PostProcessLayer whose volumeLayer mask includes
        // the volume's layer, so borrow the lowest layer an existing layer already renders
        private static int GetPostProcessLayer()
        {
            PostProcessLayer layer = Object.FindObjectOfType<PostProcessLayer>();
            if (layer != null)
            {
                int mask = layer.volumeLayer.value;
                for (int i = 0; i < 32; i++)
                    if ((mask & (1 << i)) != 0) return i;
            }

            return 0;
        }

        private static bool IsRacing(Kart kart)
        {
            if (kart == null || kart.IsRaceEnded()) return false;
            return Singleton<GameManager>.Instance?.GameMode is InGameGameMode inGame && inGame.HasRaceStarted;
        }

        private static Kart GetLocalHumanKart()
        {
            GameManager gameManager = Singleton<GameManager>.Instance;
            if (gameManager?.GameMode?.Drivers == null) return null;

            foreach (Driver driver in gameManager.GameMode.Drivers.Values)
                if (driver.IsHuman && driver.IsLocal) return driver.Kart;

            return null;
        }
    }
}
