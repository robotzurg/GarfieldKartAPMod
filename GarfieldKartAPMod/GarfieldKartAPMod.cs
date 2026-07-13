using Aube;
using BepInEx;
using BepInEx.Configuration;
using GarfieldKartAPMod.Helpers;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityHotReloadNS;
using static MenuHDTrackSelection;

namespace GarfieldKartAPMod
{
    public enum ItemManiaMode
    {
        UseYaml,
        On,
        Off
    }

    public enum DeathLinkMode
    {
        UseYaml,
        On,
        Off
    }

    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class GarfieldKartAPMod : BaseUnityPlugin
    {
        private const string PluginGuid = PluginAuthor + "." + PluginName;
        private const string PluginAuthor = "Jeffdev";
        private const string PluginName = "GarfieldKartAPMod";
        private const string PluginVersion = "1.0.0";

        public static ConfigEntry<int> notificationTime;
        public static ConfigEntry<int> lapCountOverride;
        public static ConfigEntry<bool> showNotifications;
        public static ConfigEntry<bool> showOnlyRelevantNotifications;
        public static ConfigEntry<bool> disableStatRandomization;
        public static ConfigEntry<int> lapSanityPlacementRequirement;
        public static ConfigEntry<bool> strictCpuItems;
        public static ConfigEntry<ItemManiaMode> itemManiaMode;
        public static ConfigEntry<DeathLinkMode> deathLink;

        private Harmony harmony;
        public static Dictionary<string, object> sessionSlotData;
        public static ArchipelagoClient APClient { get; private set; }
        private static GameObject uiObject;
        private static bool uiCreated;
        private static NotificationDisplay notificationDisplay;
        private FileWriter fileWriter;

        public void Awake()
        {

            notificationTime = Config.Bind("Archipelago", "Server Message On-Screen Time", 3, "How long to show archipelago server messages and checks on the screen, in seconds.");
            lapCountOverride = Config.Bind("Archipelago", "Lap Count Override", 0, "Override the lap count for races. Set to 0 to use the lap count from Archipelago slot data.");
            showNotifications = Config.Bind("Display", "Show Log Messages", true, "Show Archipelago server log messages at the top of the screen.");
            showOnlyRelevantNotifications = Config.Bind("Display", "Show Only Relevant Messages", true, "Only show log messages relevant to you (items you send or receive, your hints). Other message types still appear.");
            disableStatRandomization = Config.Bind("Archipelago", "Disable Stat Randomization", false, "Disable kart and character stat randomization, even if the Archipelago slot has it enabled.");
            lapSanityPlacementRequirement = Config.Bind("Archipelago", "Lap Sanity Placement Requirement", 1, new ConfigDescription("The placement you must be in (or better) when completing a lap for it to count as a lap sanity check. 1 = 1st place only, 8 = any placement.", new AcceptableValueRange<int>(1, 8)));
            strictCpuItems = Config.Bind("Archipelago", "Strict CPU Items", false, "CPU racers can only use items you have received from Archipelago, instead of being able to use any item.");
            itemManiaMode = Config.Bind("Archipelago", "Item Mania", ItemManiaMode.UseYaml, "Control Item Mania (CPUs hold 3 items and fire them rapidly). UseYaml follows the Archipelago slot setting; On/Off force it regardless of the yaml.");
            deathLink = Config.Bind("Archipelago", "Death Link", DeathLinkMode.UseYaml, "Control DeathLink (falling off the track sends a death to other DeathLink players, and their deaths force your kart to respawn). UseYaml follows the Archipelago slot setting; On/Off force it regardless of the yaml.");
            deathLink.SettingChanged += (_, _) => DeathLinkManager.ApplyConfig();
           
            InitializeLogging();
            InitializeAssemblyResolution();
            InitializeComponents();
            ApplyPatches();

            Log.Info($"{PluginName} loaded successfully!");
        }

        private void InitializeLogging()
        {
            Log.Init(Logger);
        }

        private void InitializeAssemblyResolution()
        {
            ForceLoadNewtonsoftJson();
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            CheckSystemNumericsAvailability();
        }

        private void ForceLoadNewtonsoftJson()
        {
            try
            {
                var jsonType = typeof(Newtonsoft.Json.JsonConvert);
                Log.Message($"Loaded Newtonsoft.Json version: {jsonType.Assembly.GetName().Version}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to preload Newtonsoft.Json: {ex.Message}");
            }
        }

        private void CheckSystemNumericsAvailability()
        {
            try
            {
                var bigIntType = Type.GetType("System.Numerics.BigInteger, System.Numerics");
                Logger.LogInfo($"BigInteger available: {bigIntType != null}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"BigInteger check failed: {ex.Message}");
            }
        }

        private void InitializeComponents()
        {
            UITextureSwapper.Initialize();
            fileWriter = gameObject.AddComponent<FileWriter>();

            APClient = new ArchipelagoClient();
            APClient.OnConnected += OnArchipelagoConnected;
            APClient.OnDisconnected += OnArchipelagoDisconnected;

            CreateUI();
        }

        private void ApplyPatches()
        {
            harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void Update()
        {
#if DEBUG
             if (Input.GetKeyUp(KeyCode.F2))
             {
                 UnityHotReload.LoadNewAssemblyVersion(
                     typeof(GarfieldKartAPMod).Assembly,
                     "C:\\Users\\robot\\AppData\\Roaming\\r2modmanPlus-local\\GarfieldKartFuriousRacing\\profiles\\Default\\BepInEx\\plugins\\Jeffdev-GarfieldKartArchipelago/GarfieldKartAPMod.dll"
                 );
             }

            // Debug keybinds for testing DeathLink: F8 broadcasts a death, F9 fakes receiving one
            if (Input.GetKeyUp(KeyCode.F8)) DeathLinkManager.SendDebugDeath();
            if (Input.GetKeyUp(KeyCode.F9)) DeathLinkManager.SimulateReceivedDeath();
#endif
            
            DeathLinkManager.ProcessPendingDeath();
            TickActiveTrapTimers();

            if (ArchipelagoHelper.IsConnectedAndEnabled)
            {
                ArchipelagoItemTracker.ProcessLiveReceivedItems();
                ArchipelagoFillerEffects.Update();
                ArchipelagoTrapEffects.Update();
            }

            if (APClient == null || !APClient.HasPendingNotifications()) return;
            string notification = APClient.DequeuePendingNotification();
            if (showNotifications.Value)
                notificationDisplay.ShowNotification(notification);
        }

        // Timed traps only count down while the local player is actively racing:
        // race started, not finished, and not a time trial
        private static void TickActiveTrapTimers()
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (!(Singleton<GameManager>.Instance?.GameMode is InGameGameMode gameMode) || !gameMode.HasRaceStarted) return;
            if (Singleton<GameConfigurator>.Instance.GameModeType == E_GameModeType.TIME_TRIAL) return;

            foreach (Driver driver in gameMode.Drivers.Values)
            {
                if (!driver.IsHuman || !driver.IsLocal) continue;
                if (driver.Kart == null || driver.Kart.IsRaceEnded()) return;

                ArchipelagoFillerManager.TickTrapTimers(Time.deltaTime);
                return;
            }
        }

        private void OnArchipelagoConnected()
        {
            Log.Message("Connected to Archipelago - loading items");
            uiObject.GetComponent<ConnectionUI>().ToggleUI();
            // PopupManager.OpenPopup("Connected to Archipelago!", PopupHD.POPUP_TYPE.INFORMATION, PopupHD.POPUP_PRIORITY.NORMAL);
        }

        private void OnArchipelagoDisconnected()
        {
            Log.Message("Disconnected from Archipelago");
            uiObject.GetComponent<ConnectionUI>().ForceShow();
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName assemblyName = new AssemblyName(args.Name);

            if (assemblyName.Name != "Newtonsoft.Json") return null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != "Newtonsoft.Json") continue;
                Log.Message($"Resolved Newtonsoft.Json to version {assembly.GetName().Version}");
                return assembly;
            }

            return null;
        }

        public static void CreateUI()
        {
            if (uiCreated) return;

            Log.Message("Creating Archipelago UI...");
            uiObject = new GameObject("ArchipelagoUI");
            DontDestroyOnLoad(uiObject);

            var ui = uiObject.AddComponent<ConnectionUI>();
            ui.Initialize(APClient);

            notificationDisplay = uiObject.AddComponent<NotificationDisplay>();
            notificationDisplay.Initialize();

            uiCreated = true;
        }

        public void OnDestroy()
        {
            APClient?.Disconnect();
            if (uiObject != null)
            {
                Destroy(uiObject);
            }
            harmony?.UnpatchSelf();
        }
    }
}

namespace GarfieldKartAPMod.Patches
{
    // Button Helper Class
    // TODO: Maybe move this class to its own file, but it's probably fine here
    public static class ButtonHelper
    {
        public static void DisableButtonsByIndices(object buttonsArray, params int[] indices)
        {
            try
            {
                Type buttonsType = buttonsArray.GetType();
                PropertyInfo lengthProp = buttonsType.GetProperty("Length");
                PropertyInfo indexerProp = buttonsType.GetProperty("Item", [typeof(int)]);
                if (lengthProp == null) return;
                int length = (int)lengthProp.GetValue(buttonsArray);

                foreach (int i in indices)
                {
                    if (i >= length) continue;

                    if (indexerProp == null) continue;
                    BetterButton button = indexerProp.GetValue(buttonsArray, [i]) as BetterButton;
                    if (button == null) continue;
                    button.interactable = false;
                    Log.Info($"Disabled button at index {i}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to disable buttons: {ex}");
            }
        }

        public static void DisableLockedRaceButtons(object instance, object m_buttons, int currentCupId)
        {
            try
            {
                Type buttonsType = m_buttons.GetType();

                // Get Length property
                PropertyInfo lengthProp = buttonsType.GetProperty("Length");
                if (lengthProp == null) return;
                int length = (int)lengthProp.GetValue(m_buttons);

                // Get the indexer with specific parameters (int index)
                PropertyInfo indexerProp = buttonsType.GetProperty("Item", [typeof(int)]);

                // Time trials only need the course reachable by any unlock, not the race itself
                bool isTimeTrial = Singleton<GameConfigurator>.Instance.GameModeType == E_GameModeType.TIME_TRIAL;

                for (int i = 0; i < length; i++)
                {
                    if (indexerProp != null)
                    {
                        BetterButton button = indexerProp.GetValue(m_buttons, [i]) as BetterButton;
                        if (button == null) continue;

                        if (i == 4)
                        {
                            button.gameObject.SetActive(false);
                            continue; // Skip the last button
                        }

                        int raceId = 4 * currentCupId + i; // Race IDs

                        bool accessible = isTimeTrial
                            ? ArchipelagoItemTracker.CanAccessTimeTrial(raceId)
                            : ArchipelagoItemTracker.HasRace(raceId);
                        if (!accessible)
                        {
                            button.interactable = false;
                            continue;
                        }

                        button.interactable = true;
                        GkEventSystem.Current.SelectButton(button);
                    }
                    if (instance is MenuHDTrackSelection selection)
                        selection.UpdateRacesButtons(currentCupId);
                }
            }
            catch (Exception ex)
            {
                // TODO: Figure out what errors can throw here and prevent them instead of try catching
                Log.Error($"Failed to disable race buttons: {ex}");
            }
        }
    }

    // Menu Patches

    [HarmonyPatch(typeof(MenuHDMain), "Enter")]
    public class MenuHDMain_Enter_Patch
    {
        // Index of BUTTON.GALLERY in MenuHDMain's private button array
        private const int GalleryButtonIndex = 3;

        static void Postfix(MenuHDMain __instance, object ___m_buttons)
        {
            UITextureSwapper.SwapMainMenuLogo(__instance.transform.root.gameObject);

            // Without a session the button still leads to the real gallery, so leave it alone
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            Button gallery = GetButton(___m_buttons, GalleryButtonIndex);
            if (gallery != null) UITextureSwapper.SwapGalleryButtonIcon(gallery.gameObject);
        }

        // m_buttons is an EnumArray keyed by a private enum, so it can't be named in the patch
        // signature - reach the entry through the indexer instead
        private static Button GetButton(object buttons, int index)
        {
            MethodInfo indexer = buttons?.GetType().GetMethod(
                "get_Item",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [typeof(int)],
                null
            );

            return indexer?.Invoke(buttons, [index]) as Button;
        }
    }

    // The "press any button" screen shows before the main menu, and has its own copy of the logo
    [HarmonyPatch(typeof(MenuHDEngagementScreen), "Enter")]
    public class MenuHDEngagementScreen_Enter_Patch
    {
        static void Postfix(MenuHDEngagementScreen __instance)
        {
            UITextureSwapper.SwapMainMenuLogo(__instance.transform.root.gameObject);
        }
    }

    [HarmonyPatch(typeof(MenuHDGameMode), "Enter")]
    public class MenuHDGameMode_Enter_Patch
    {
        static void Postfix(MenuHDGameMode __instance, object ___m_buttons)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            var cupsList = ArchipelagoItemTracker.GetAvailableCups();

            // Disable Championship button if no cups available
            if (cupsList.Count == 0)
            {
                ButtonHelper.DisableButtonsByIndices(___m_buttons, 1);
            }
        }
    }

    [HarmonyPatch(typeof(MenuHDGameMode), "OnSubmitChampionShip")]
    public class MenuHDModeSelect_OnSubmitChampionShip_Patch
    {
        static bool Prefix(MenuHDGameMode __instance)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return true;

            var cupList = ArchipelagoItemTracker.GetAvailableCups();
            if (cupList.Count != 0) return true;
            ArchipelagoPopupManager.ShowWarning("You haven't unlocked any cups!");
            return false;
        }
    }

    [HarmonyPatch(typeof(MenuHDGameType), "Enter")]
    public class MenuHDGameType_Enter_Patch
    {
        static void Prefix()
        {
            ArchipelagoItemTracker.ResyncFromServer();
        }

        static void Postfix(MenuHDGameType __instance, object ___m_buttons)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            // Disable Versus and Time Trial buttons
            ButtonHelper.DisableButtonsByIndices(___m_buttons, 1, 2);
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackSelection), "Enter")]
    public class MenuHDTrackSelection_Enter_Patch
    {
        static void Postfix(MenuHDTrackSelection __instance, EnumArray<TAB, BetterToggle> ___m_tabs, object ___m_buttons, ref int ___m_currentChampionshipIndex)
        {
            Log.Message("Track Selection Menu opened");
            ArchipelagoItemTracker.ResyncFromServer();

            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            if (ArchipelagoHelper.IsPuzzleRandomizationEnabled())
            {
                UITextureSwapper.ResetSwapFlag();
                UITextureSwapper.SwapPuzzlePieceIcons(__instance.gameObject);
            }

            int foundTab = SetupCupTabs(___m_tabs, ___m_currentChampionshipIndex);
            if (foundTab != -1)
                ___m_currentChampionshipIndex = foundTab;
            ButtonHelper.DisableLockedRaceButtons(__instance, ___m_buttons, foundTab != -1 ? foundTab : ___m_currentChampionshipIndex);
        }

        private static int SetupCupTabs(EnumArray<TAB, BetterToggle> tabs, int currentChampionshipId)
        {
            int foundTab = -1;
            E_GameModeType gameMode = Singleton<GameConfigurator>.Instance.GameModeType;

            for (int i = 0; i < tabs.Length; i++)
            {
                bool activateButton = false;
                bool hasRaceInCup = ArchipelagoItemTracker.HasRaceInCup(i);

                bool canSeeChampionshipCup = ArchipelagoHelper.IsRacesAndCupsRandomized()
                    ? ArchipelagoItemTracker.HasCup(i)
                    : ArchipelagoItemTracker.CanAccessCup(i);
                if (gameMode == E_GameModeType.CHAMPIONSHIP && canSeeChampionshipCup)
                {
                    activateButton = true;
                }
                else if (gameMode == E_GameModeType.SINGLE && hasRaceInCup)
                {
                    activateButton = true;
                }
                else if (gameMode == E_GameModeType.TIME_TRIAL && ArchipelagoItemTracker.HasTimeTrialInCup(i))
                {
                    activateButton = true;
                }

                if (!activateButton)
                {
                    tabs[i].gameObject.SetActive(false);
                    continue;
                }

                if (foundTab == -1)
                {
                    GkEventSystem.Current.SelectTab(tabs[i]);
                    foundTab = i;
                }

                tabs[i].gameObject.SetActive(true);
            }

            return foundTab;
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackSelection), "OnSelectChampionship")]
    public class MenuHDTrackSelection_OnSelectChampionship_Patch
    {
        static void Postfix(MenuHDTrackSelection __instance, object ___m_buttons, int iId)
        {
            ButtonHelper.DisableLockedRaceButtons(__instance, ___m_buttons, iId);
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackSelection), "UpdateRacesButtons")]
    public class MenuHDTrackSelection_UpdateRacesButtons_Patch
    {
        static bool Prefix(MenuHDTrackSelection __instance,
            int cup,
            List<HD_TrackSelection_Item> ___m_itemsButtons,
            int ___m_maxPuzzleNumber,
            int ___m_currentSelectedButton,
            bool ___m_hasFinishedEntering,
            TextMeshProUGUI ___m_textCupCircuit)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled || !ArchipelagoHelper.IsPuzzleRandomizationEnabled())
            {
                return true;
            }

            for (int i = 0; i < ___m_itemsButtons.Count - 1; i++)
            {
                ___m_itemsButtons[i].ChangeBackground(PlayerGameEntities.ChampionShipDataList[cup].Sprites[i]);
                string text = Singleton<GameConfigurator>.Instance.ChampionShipData.Tracks[i];
                int puzzleCount = ArchipelagoItemTracker.GetPuzzlePieceCount(text);
                ___m_itemsButtons[i].UpdatePuzzleText(puzzleCount);
                ___m_itemsButtons[i].UpdateTimeTrialText(text);
            }

            if (___m_currentSelectedButton != 4 && ___m_hasFinishedEntering)
            {
                MethodInfo method = AccessTools.Method(typeof(MenuHDTrackSelection), "UpdateTimeTrialValues");
                method.Invoke(__instance, [___m_currentSelectedButton]);
            }

            if (___m_currentSelectedButton < Singleton<GameConfigurator>.Instance.ChampionShipData.TracksName.Length)
            {
                ___m_textCupCircuit.text = Singleton<GameConfigurator>.Instance.ChampionShipData.ChampionShipName + " - " + Singleton<GameConfigurator>.Instance.ChampionShipData.TracksName[___m_currentSelectedButton];
            }

            return false;
        }
    }

    // Separate postfix so the checks display updates whether or not the puzzle
    // prefix above skipped the original method
    [HarmonyPatch(typeof(MenuHDTrackSelection), "UpdateRacesButtons")]
    public class MenuHDTrackSelection_UpdateRacesButtons_ChecksDisplay_Patch
    {
        static void Postfix(List<HD_TrackSelection_Item> ___m_itemsButtons)
        {
            string[] tracks = Singleton<GameConfigurator>.Instance.ChampionShipData.Tracks;
            for (int i = 0; i < ___m_itemsButtons.Count - 1 && i < tracks.Length; i++)
            {
                TrackChecksDisplay.AttachOrUpdate(___m_itemsButtons[i], tracks[i]);
            }
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackPresentation), "InitPuzzlePieces")]
    public class MenuHDTrackPresentation_InitPuzzlePieces_Patch
    {
        static bool Prefix(Image[] puzzlePiecesImages, string trackName, bool isTimeTrial)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return true;
            if (!ArchipelagoHelper.IsPuzzleRandomizationEnabled()) return true;

            if (GkNetMgr.Instance.IsConnected)
            {
                puzzlePiecesImages[0].transform.parent.gameObject.SetActive(value: false);
                return false;
            }
            puzzlePiecesImages[0].transform.parent.gameObject.SetActive(value: true);
            for (int i = 0; i < puzzlePiecesImages.Length; i++)
            {
                puzzlePiecesImages[i].gameObject.SetActive(!isTimeTrial);

                long puzzlePieceLocation = ArchipelagoConstants.GetPuzzlePieceLoc(trackName, i);
                bool flag = ArchipelagoItemTracker.HasLocation(puzzlePieceLocation);
                puzzlePiecesImages[i].sprite = (flag ? UITextureSwapper.puzzlePieceFilledSprite : UITextureSwapper.puzzlePieceEmptySprite);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(GkEventSystem), "OnSecondaryMove")]
    public class GkEventSystem_OnSecondaryMove_Patch
    {
        static Exception Finalizer(Exception __exception)
        {
            if (__exception is not InvalidCastException) return __exception;
            Log.Debug("Suppressed InvalidCastException in OnSecondaryMove (navigating to disabled tab)");
            return null;
        }
    }

    // ========== RACE PATCHES ==========

    [HarmonyPatch(typeof(RcRace), "StartRace")]
    public class RcRace_StartRace_Patch
    {
        static void Prefix(RcRace __instance)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (Singleton<GameConfigurator>.Instance.GameModeType == E_GameModeType.TIME_TRIAL) return;

            int lapCount = ArchipelagoHelper.GetLapCount();
            __instance.SetRaceNbLap(lapCount);
        }
    }

    [HarmonyPatch(typeof(RcVehicleRaceStats), "CrossStartLine")]
    public class RcVehicleRaceStats_CrossStartLine_Patch
    {
        static void Prefix(int ___m_iNbLapCompleted, ref int __state)
        {
            __state = ___m_iNbLapCompleted;
        }

        static void Postfix(RcVehicleRaceStats __instance, int ___m_iNbLapCompleted, RcVehicle ___m_pVehicle, int __state)
        {
            Log.Debug($"{__instance.GetRank()} RANK");
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (!ArchipelagoHelper.IsLapSanityEnabled()) return;
            if (Singleton<GameConfigurator>.Instance.GameModeType == E_GameModeType.TIME_TRIAL) return;
            if (___m_iNbLapCompleted < __state + 1) return;
            if (___m_pVehicle.IsAutoPilot()) return;
            if (___m_pVehicle.m_eControlType == RcVehicle.ControlType.AI) return;
            
            if (!ArchipelagoHelper.MeetsCCRequirement(Singleton<GameConfigurator>.Instance.Difficulty))
            {
                Log.Message("Skipping lap sanity check due to CC requirement");
                return;
            }

            // GetRank() is 0-indexed, the config value is 1-indexed (1 = 1st place)
            if (__instance.GetRank() >= GarfieldKartAPMod.lapSanityPlacementRequirement.Value) return;

            string track = Singleton<GameConfigurator>.Instance.StartScene;
            int lapIndex = ___m_iNbLapCompleted - 2; // 0-indexed
            long locId = ArchipelagoConstants.GetLapSanityLoc(track, lapIndex);
            if (locId == -1) return;

            GarfieldKartAPMod.APClient.SendLocation(locId);
            Log.Message($"Sent lap sanity check for {track}, lap {lapIndex + 1}");
        }
    }
    
    // Apply fillers
    [HarmonyPatch(typeof(Kart), "StartRace")]
    public class Kart_StartRace_Patch
    {
        static void Prefix(Kart __instance)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (Singleton<GameConfigurator>.Instance.GameModeType == E_GameModeType.TIME_TRIAL) return;

            // StartRace runs once per kart, so gate on the local driver or the race's filler gets
            // pulled out of the backlogs once per racer
            Driver driver = __instance.Driver;
            if (driver == null || !driver.IsHuman || !driver.IsLocal) return;

            // Must come before the effects below go looking for their filler
            ArchipelagoFillerManager.OnRaceStart();

            ArchipelagoFillerEffects.TryApplyStartBoost(__instance);
            ArchipelagoTrapEffects.OnRaceStart(__instance);
        }
    }

    // Broken Drift Trap: cap the local player's drift charge just below the second boost
    // threshold so a drift can never reach the best (blue) boost while the trap is active
    [HarmonyPatch(typeof(Kart), "UpdateDriftState")]
    public class Kart_UpdateDriftState_Patch
    {
        private static readonly AccessTools.FieldRef<Kart, float> BoostChargedRef =
            AccessTools.FieldRefAccess<Kart, float>("m_boostAmountCharged");

        static void Prefix(Kart __instance)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (!ArchipelagoFillerManager.IsFillerActive(ArchipelagoConstants.ITEM_BROKEN_DRIFT_TRAP)) return;

            Driver driver = __instance.Driver;
            if (driver == null || !driver.IsHuman || !driver.IsLocal) return;

            float cap = __instance.FirstBoostThreshold - 0.01f;
            if (BoostChargedRef(__instance) > cap)
                BoostChargedRef(__instance) = cap;
        }
    }

    // Race-duration fillers (traps) tick down once per completed race.
    [HarmonyPatch(typeof(RaceGameState), "OnLocalHumanDriverRaceEnded")]
    public class RaceGameState_OnLocalHumanDriverRaceEnded_Patch
    {
        static void Postfix(RcVehicle pVehicle)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (Singleton<GameConfigurator>.Instance.GameModeType == E_GameModeType.TIME_TRIAL) return;

            ArchipelagoFillerEffects.TryShowGarfieldQuote();

            // GetRank() is 0-indexed, so rank 0 is 1st place
            bool wonRace = pVehicle?.RaceStats != null && pVehicle.RaceStats.GetRank() == 0;
            ArchipelagoFillerManager.OnRaceEnd(wonRace);

            // Drop any duration trap effects now the race is over
            ArchipelagoTrapEffects.ClearAll();
        }
    }

    public static class DrivingCaracStatsRando
    {
        // Character enum values map to these names (offset from 301)
        private static readonly string[] CharacterNames =
            { "Garfield", "Jon", "Liz", "Odie", "Arlene", "Nermal", "Squeak", "Harry" };

        // Kart enum values map to these names (offset from 351)
        private static readonly string[] KartNames =
            { "Formula Zzzz", "Abstract-Kart", "Medi-Kart", "Woof-Mobile", "Kissy-Kart", "Cutie-Pie Cat", "Rat-Racer", "Muck-Madness" };

        public static void ApplyAll()
        {
            Log.Info($"[StatsRando] ApplyAll called. Connected={ArchipelagoHelper.IsConnectedAndEnabled}");
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (GarfieldKartAPMod.disableStatRandomization.Value) return;
            if (GarfieldKartAPMod.sessionSlotData == null) return;

            if (!GarfieldKartAPMod.sessionSlotData.TryGetValue("randomized_stats", out object statsObj)) return;

            JObject statsRoot = statsObj as JObject ?? (statsObj is Dictionary<string, object> d ? JObject.FromObject(d) : null);
            if (statsRoot == null) return;

            JObject kartsData   = statsRoot["karts"]      as JObject;
            JObject charsData   = statsRoot["characters"] as JObject;

            foreach (CharacterCarac character in PlayerGameEntities.CharacterList)
            {
                string name = GetName(character.Owner, CharacterNames);
                JObject stats = charsData?[name] as JObject;
                if (stats == null) continue;
                ApplyStats(character, stats);
            }

            foreach (KartCarac kart in PlayerGameEntities.KartList)
            {
                string name = GetName(kart.Owner, KartNames);
                JObject stats = kartsData?[name] as JObject;
                if (stats == null) continue;
                ApplyStats(kart, stats);
            }
        }

        private static string GetName(ECharacter owner, string[] names)
        {
            int idx = (int)owner;
            return idx >= 0 && idx < names.Length ? names[idx] : owner.ToString();
        }

        private const float StatMin = -25f;
        private const float StatMax = 25f;

        private static void ApplyStats(DrivingCarac instance, JObject stats)
        {
            if (stats["Speed"]        != null) instance.Speed        = Mathf.Clamp(stats["Speed"].Value<float>(),        StatMin, StatMax);
            if (stats["Acceleration"] != null) instance.Acceleration = Mathf.Clamp(stats["Acceleration"].Value<float>(), StatMin, StatMax);
            if (stats["Maniability"]  != null) instance.Maniability  = Mathf.Clamp(stats["Maniability"].Value<float>(),  StatMin, StatMax);
            Log.Info($"[StatsRando] {instance.name}: spd={instance.Speed:F2} acc={instance.Acceleration:F2} man={instance.Maniability:F2}");
        }
    }

    [HarmonyPatch(typeof(KartBonusMgr), "SetItem")]
    public class KartBonusMgr_SetItem_Patch
    {
        static bool Prefix(KartBonusMgr __instance, Kart ___m_kart, ref BonusCategory bonus, ref int iQuantity, int byPassSlot = -1, bool isFromCheat = false)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return true;
            if (isFromCheat) return true;
            if (!___m_kart.Driver.IsHuman)
            {
                if (ArchipelagoHelper.IsItemManiaEnabled())
                    iQuantity = 3;
                if (ArchipelagoHelper.IsCPUItemsDisabled()) return false;
                if (GarfieldKartAPMod.strictCpuItems.Value)
                    return ArchipelagoItemTracker.HasBonusAvailable(bonus);
                return true;
            }

            if (ArchipelagoHelper.IsSpringsOnly())
            {
                bonus = BonusCategory.SPRING;
                return FinishItemRoll(__instance, ___m_kart, bonus);
            }

            if (ArchipelagoHelper.IsItemRandomizerEnabled())
            {
                // An itemsanity check only fires on an item we already own, so only unlocked ones
                // are worth steering the roll toward
                var needed = GetNeededItemsanityBonuses().Where(ArchipelagoItemTracker.HasBonusAvailable).ToList();
                if (needed.Count > 0)
                {
                    bonus = needed[UnityEngine.Random.Range(0, needed.Count)];
                }
                else if (!ArchipelagoItemTracker.HasBonusAvailable(bonus))
                {
                    var availableBonuses = GetAvailableItemsanityBonuses();
                    if (availableBonuses.Count > 0 && UnityEngine.Random.value <= 0.5f)
                        bonus = availableBonuses[UnityEngine.Random.Range(0, availableBonuses.Count)];
                }
            }
            
            // If Puzzle rando is enabled and we have springs and puzzle pieces are uncollected, prioritize springs
            if (ArchipelagoHelper.IsPuzzleRandomizationEnabled() &&
                ArchipelagoItemTracker.HasBonusAvailable(BonusCategory.SPRING))
            {
                string track = Singleton<GameConfigurator>.Instance.StartScene;
                if (ArchipelagoItemTracker.GetPuzzlePieceCount(track) < 3 && UnityEngine.Random.value < 0.5f)
                    bonus = BonusCategory.SPRING;
            }

            return FinishItemRoll(__instance, ___m_kart, bonus);
        }
        
        private static bool FinishItemRoll(KartBonusMgr bonusMgr, Kart kart, BonusCategory bonus)
        {
            if (ArchipelagoItemTracker.HasBonusAvailable(bonus)) return true;

            kart.KartSound.PlaySound(10, sendToOtherClients: false);

            HUDBonusHD hud = bonusMgr.HUDBonus;
            if (hud != null)
            {
                int slot = -1;
                for (int i = 0; i < KartBonusMgr.NB_BONUS_SLOTS; i++)
                {
                    if (bonusMgr.GetItem(i) == BonusCategory.NONE)
                    {
                        slot = i;
                        break;
                    }
                }
                if (slot >= 0)
                {
                    hud.StartAnimation(slot, bonus);
                    kart.StartCoroutine(ClearLockedItemSlot(bonusMgr, hud, slot, bonus));
                }
            }
            return false;
        }

        private static IEnumerator ClearLockedItemSlot(KartBonusMgr bonusMgr, HUDBonusHD hud, int slot, BonusCategory bonus)
        {
            while (hud.Slots[slot].State != BONUS_ANIM_STATE.STOPPED)
                yield return null;
            yield return new WaitForSeconds(0.4f);

            // A real item may have claimed this slot while we waited
            if (bonusMgr.GetItem(slot) != BonusCategory.NONE || hud.Slots[slot].WantedBonus != bonus)
                yield break;

            if (slot == 0)
                hud.ResetSlots();
            else
                hud.ResetSlot2();
        }
        
        private static List<BonusCategory> GetAvailableItemsanityBonuses()
        {
            var availableBonuses = new List<BonusCategory>();
            foreach (BonusCategory bonus in Enum.GetValues(typeof(BonusCategory)))
            {
                if (bonus != BonusCategory.NONE && ArchipelagoItemTracker.HasBonusAvailable(bonus))
                    availableBonuses.Add(bonus);
            }
            return availableBonuses;
        }

        private static List<BonusCategory> GetNeededItemsanityBonuses()
        {
            var needed = new List<BonusCategory>();
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_PIE)) needed.Add(BonusCategory.PIE);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_HOMING_PIE)) needed.Add(BonusCategory.AUTOLOCK_PIE);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_DIAMOND)) needed.Add(BonusCategory.DIAMOND);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_MAGIC_WAND)) needed.Add(BonusCategory.MAGIC);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_PERFUME)) needed.Add(BonusCategory.PARFUME);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_LASAGNA)) needed.Add(BonusCategory.LASAGNA);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_UFO)) needed.Add(BonusCategory.UFO);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_PILLOW)) needed.Add(BonusCategory.NAP);
            if (!ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.LOC_FIND_ITEM_SPRING)) needed.Add(BonusCategory.SPRING);
            return needed;
        }

        static void Postfix(KartBonusMgr __instance, Kart ___m_kart, BonusCategory bonus)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            if (___m_kart.Driver.IsHuman && !___m_kart.IsAutoPilot() && ArchipelagoItemTracker.HasBonusAvailable(bonus))
            {
                switch (bonus)
                {
                    case BonusCategory.PIE:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_PIE);
                        break;
                    case BonusCategory.AUTOLOCK_PIE:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_HOMING_PIE);
                        break;
                    case BonusCategory.LASAGNA:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_LASAGNA);
                        break;
                    case BonusCategory.SPRING:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_SPRING);
                        break;
                    case BonusCategory.DIAMOND:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_DIAMOND);
                        break;
                    case BonusCategory.MAGIC:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_MAGIC_WAND);
                        break;
                    case BonusCategory.NAP:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_PILLOW);
                        break;
                    case BonusCategory.PARFUME:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_PERFUME);
                        break;
                    case BonusCategory.UFO:
                        GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_FIND_ITEM_UFO);
                        break;
                }
            }
        }
     }

    [HarmonyPatch(typeof(GkRacingAI), "ActivateBonus")]
    public class GkRacingAI_ActivateBonus_Patch
    {
        static void Postfix(GkRacingAI __instance, Kart pKart)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (!ArchipelagoHelper.IsItemManiaEnabled()) return;
            if (!pKart.Driver.IsAi) return;

            if (pKart.GetBonusMgr().GetItem(0) != BonusCategory.NONE)
                pKart.StartCoroutine(FireNextItem(__instance, pKart));
        }

        static IEnumerator FireNextItem(GkRacingAI ai, Kart pKart)
        {
            yield return new WaitForSeconds(2.0f);
            if (pKart.GetBonusMgr().GetItem(0) != BonusCategory.NONE)
                ai.ActivateBonus(pKart, UnityEngine.Random.value < 0.5f);
        }
    }

    [HarmonyPatch(typeof(RacePuzzlePiece), "DoTrigger")]
    public class RacePuzzlePiece_DoTrigger_Patch
    {
        static void Postfix(RacePuzzlePiece __instance, RcVehicle pVehicle)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            if (!pVehicle || pVehicle.GetControlType() != RcVehicle.ControlType.Human) return;
            long puzzlePieceLocId = ArchipelagoConstants.GetPuzzlePieceLoc(
                Singleton<GameConfigurator>.Instance.StartScene,
                __instance.Index);
            GarfieldKartAPMod.APClient.SendLocation(puzzlePieceLocId);
            Log.Message($"Sending Puzzle Piece {Singleton<GameConfigurator>.Instance.StartScene}_{__instance.Index}");
        }
    }

    [HarmonyPatch(typeof(RacePuzzlePiece), "Awake")]
    public class RacePuzzlePiece_Awake_Patch
    {
        static void Postfix(RacePuzzlePiece __instance)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (!ArchipelagoHelper.IsPuzzleRandomizationEnabled()) return;

            // Restore the original material so we can fuck with it
            __instance.GetComponent<Renderer>().materials[0] = null;

            long puzzlePieceLocation = ArchipelagoConstants.GetPuzzlePieceLoc(Singleton<GameConfigurator>.Instance.StartScene, __instance.Index);
            bool hasPuzzlePiece = ArchipelagoItemTracker.HasLocation(puzzlePieceLocation);

            if (!hasPuzzlePiece) return;
            Material[] materials = __instance.GetComponent<Renderer>().materials;
            if (materials.Length == 1)
            {
                materials[0] = __instance.TransparentMaterial;
            }
            __instance.GetComponent<Renderer>().materials = materials;
        }

    }

    [HarmonyPatch(typeof(HUDPositionHD), "TakePuzzlePiece")]
    public class HUDPositionHD_TakePuzzlePiece_Patch
    {
        static bool Prefix(HUDPositionHD __instance, int iIndex, List<Animation> ___m_puzzlesAnimation, List<Image> ___m_puzzleImages, int ___m_iLogPuzzle)
        {
            if (iIndex is < 0 or >= 3)
            {
                return false;
            }
            bool flag = true;
            for (int i = 0; i < 2; i++)
            {
                if (i == iIndex) continue;
                long puzzlePieceLocation = ArchipelagoConstants.GetPuzzlePieceLoc(Singleton<GameConfigurator>.Instance.StartScene, i);

                if (ArchipelagoItemTracker.HasLocation(puzzlePieceLocation)) continue;
                flag = false;
                break;
            }

            if (flag)
            {
                foreach (Animation item in ___m_puzzlesAnimation)
                {
                    item.Play("PuzzlePiece_Turn");
                }
            }
            else if (___m_puzzlesAnimation[iIndex] != null)
            {
                ___m_puzzlesAnimation[iIndex].Play("PuzzlePiece_Turn");
            }

            if (___m_puzzleImages[iIndex] == null) return false;
            ___m_puzzleImages[iIndex].sprite = UITextureSwapper.puzzlePieceFilledSprite;
            if (LogManager.Instance != null)
            {
                ___m_iLogPuzzle++;
            }

            return false;
        }
    }

    // ========== SAVE/UNLOCK PATCHES ==========

    [HarmonyPatch(typeof(GameSaveManager), "IsPuzzlePieceUnlocked")]
    public class GameSaveManager_IsPuzzlePieceUnlocked_Patch
    {
        static bool Prefix(GameSaveManager __instance, string piece, Dictionary<string, bool> ___m_puzzlePieces, ref bool __result)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled || !ArchipelagoHelper.IsPuzzleRandomizationEnabled())
            {
                return true;
            }

            string[] pieceData = piece.Split('_');
            Int32.TryParse(pieceData[1], out int pieceIndex);

            __result = ArchipelagoItemTracker.HasLocation(
                ArchipelagoConstants.GetPuzzlePieceLoc(pieceData[0], pieceIndex));

            return false;
        }
    }

    [HarmonyPatch(typeof(HD_TrackSelection_Item), "UpdatePuzzleText")]
    public class HD_TrackSelection_Item_UpdatePuzzleText_Patch
    {
        static bool Prefix(HD_TrackSelection_Item __instance, int value, TextMeshProUGUI ___m_puzzleText, GameObject ___m_boardPuzzle, GameObject ___m_boardPuzzleFull, int ___m_maxPuzzleValue)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled || !ArchipelagoHelper.IsPuzzleRandomizationEnabled())
            {
                return true;
            }

            ___m_puzzleText.text = $"{value}/{___m_maxPuzzleValue}";

            if (value == ___m_maxPuzzleValue)
            {
                if (___m_boardPuzzle.activeSelf)
                {
                    ___m_boardPuzzle.SetActive(false);
                }
                ___m_boardPuzzleFull.SetActive(true);
            }
            else
            {
                if (___m_boardPuzzleFull.activeSelf)
                {
                    ___m_boardPuzzleFull.SetActive(false);
                }
                ___m_boardPuzzle.SetActive(true);
            }

            return false;
        }
    }

    // Swap the artwork gallery out for the Archipelago item list. Postfix rather than a skip, so
    // the menu still sets itself up and B backs out of it normally.
    [HarmonyPatch(typeof(MenuHDGallery), "Enter")]
    public class MenuHDGallery_Enter_Patch
    {
        static void Postfix(MenuHDGallery __instance, TextMeshProUGUI ___m_titleLabel, Canvas ___m_listCanvas,
            GameObject ___m_submitButton, InfoBox ___m_infoBox)
        {
            ApGalleryMenu.Refresh(__instance, ___m_titleLabel, ___m_listCanvas, ___m_submitButton, ___m_infoBox);
        }
    }

    [HarmonyPatch(typeof(KartSelectionNavigation), "Enter")]
    public class KartSelectionNavigation_Enter_Patch
    {
        static bool Prefix(KartSelectionNavigation __instance, EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> ___m_items)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return true;

            DrivingCaracStatsRando.ApplyAll();
            UpdateCharacterUnlocks(__instance, ___m_items);
            UpdateKartUnlocks(__instance, ___m_items);

            return true; // Continue to original method after doing character/kart unlocks
        }
        
        static void Postfix(KartSelectionNavigation __instance,
            EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> ___m_items,
            EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem> ___m_selectedItems)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            ForceUnlockedSelection(__instance, ___m_items, ___m_selectedItems, MenuHDKartSelection.KARTSELECT_TYPE.CHARACTER);
            ForceUnlockedSelection(__instance, ___m_items, ___m_selectedItems, MenuHDKartSelection.KARTSELECT_TYPE.KART);
        }

        private static void ForceUnlockedSelection(
            KartSelectionNavigation navigation,
            EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> items,
            EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem> selectedItems,
            MenuHDKartSelection.KARTSELECT_TYPE type)
        {
            if (items == null || selectedItems == null) return;

            KartSelectionItem selected = selectedItems[(int)type];
            if (selected != null && !selected.Locked) return;

            KartSelectionItem[] all = items[(int)type];
            KartSelectionItem replacement = all?.FirstOrDefault(item =>
                item != null && !item.Locked && item.gameObject.activeSelf);

            if (replacement == null)
            {
                Log.Warning($"[KartSelect] Selected {type} is locked and there's no unlocked one to fall back to");
                return;
            }

            // Goes through the game's own selection path, so the 3D preview, default hat/custom
            // and scroll position all follow along
            navigation.OnChangeSelectedItem(replacement);
            Log.Message($"[KartSelect] Selected {type} was locked, switched to {replacement.IconCarac.name}");
        }

        private static void UpdateCharacterUnlocks(KartSelectionNavigation instance, EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> items)
        {
            if (items == null) return;
            MethodInfo indexer = items.GetType().GetMethod(
                "get_Item",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [typeof(int)],
                null
            );

            if (indexer == null) return;
            var characterItems = (KartSelectionItem[])indexer.Invoke(items, [0]);

            foreach (KartSelectionItem item in characterItems)
            {
                CharacterCarac character = (CharacterCarac)item.IconCarac;
                UnlockableItemSate state = Singleton<GameSaveManager>.Instance.GetCharacterState(character.Owner);

                bool isUnlocked = state is UnlockableItemSate.UNLOCKED or UnlockableItemSate.NEWUNLOCKED;
                item.SetLock(!isUnlocked);
                KartSelectionWinDisplay.AttachOrUpdate(item);
            }
        }

        private static void UpdateKartUnlocks(KartSelectionNavigation instance, EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> items)
        {
            if (items == null) return;
            MethodInfo indexer = items.GetType().GetMethod(
                "get_Item",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                [typeof(int)],
                null
            );

            if (indexer == null) return;
            var kartItems = (KartSelectionItem[])indexer.Invoke(items, [1]);

            foreach (KartSelectionItem item in kartItems)
            {
                KartCarac kart = (KartCarac)item.IconCarac;
                UnlockableItemSate state = Singleton<GameSaveManager>.Instance.GetKartState(kart.Owner);

                bool isUnlocked = (state == UnlockableItemSate.UNLOCKED || state == UnlockableItemSate.NEWUNLOCKED);
                item.SetLock(!isUnlocked);
                KartSelectionWinDisplay.AttachOrUpdate(item);
            }
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "GetCharacterState")]
    public class GameSaveManager_GetCharacterState_Patch
    {
        static bool Prefix(GameSaveManager __instance, ECharacter character, ref UnlockableItemSate __result)
        {
            bool charRando = ArchipelagoHelper.IsCharRandomizerEnabled();
            Log.Info($"{charRando} CHARACTER RANDO CHECK");

            if (!ArchipelagoHelper.IsConnectedAndEnabled)
                return true;

            long charItemId = 301 + (long)character; // Character IDs start at 301
            Log.Info($"{character}, {charItemId}");

            __result = (ArchipelagoItemTracker.HasItem(charItemId) || !charRando) ? UnlockableItemSate.UNLOCKED : UnlockableItemSate.LOCKED;

            return false;
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "GetKartState")]
    public class GameSaveManager_GetKartState_Patch
    {
        static bool Prefix(GameSaveManager __instance, ECharacter kart, ref UnlockableItemSate __result)
        {
            bool kartRando = ArchipelagoHelper.IsKartRandomizerEnabled();
            Log.Info($"{kartRando} KART RANDO CHECK");

            if (!ArchipelagoHelper.IsConnectedAndEnabled)
                return true;

            long kartItemId = 351 + (long)kart; // Kart IDs start at 351

            __result = (ArchipelagoItemTracker.HasItem(kartItemId) || !kartRando) ? UnlockableItemSate.UNLOCKED : UnlockableItemSate.LOCKED;

            return false;
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "GetHatState")]
    public class GameSaveManager_GetHatState_Patch
    {
        static bool Prefix(GameSaveManager __instance, string hat, ref UnlockableItemSate __result)
        {
            if (!ArchipelagoHelper.IsHatRandomizerEnabled() || !ArchipelagoHelper.IsConnectedAndEnabled)
                return true;

            long hatItemId = ArchipelagoConstants.GetHatItemId(hat);

            __result = ArchipelagoItemTracker.HasItem(hatItemId)
                ? UnlockableItemSate.UNLOCKED
                : UnlockableItemSate.LOCKED;

            return false;
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "GetCustomState")]
    public class GameSaveManager_GetCustomState_Patch
    {
        static bool Prefix(GameSaveManager __instance, string custom, ref UnlockableItemSate __result)
        {
            if (!ArchipelagoHelper.IsSpoilerRandomizerEnabled() || !ArchipelagoHelper.IsConnectedAndEnabled)
                return true;

            long customItemId = ArchipelagoConstants.GetSpoilerItemId(custom);

            __result = ArchipelagoItemTracker.HasItem(customItemId)
                ? UnlockableItemSate.UNLOCKED
                : UnlockableItemSate.LOCKED;

            return false;
        }
    }

    [HarmonyPatch(typeof(Localization), "Get")]
    public class Localization_Get_Patch
    {
        static void Postfix(string key, ref string __result)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            switch (key)
            {
                case "MENU_GARAGE_UNLOCK_SINGLE_RACE":
                    if (ArchipelagoHelper.IsHatRandomizerEnabled())
                        __result = "You must be sent the Archipelago item for this hat!";
                    break;
                case "MENU_GARAGE_UNLOCK_GRAND_PRIX":
                    Log.Info($"Grand Prix {ArchipelagoHelper.IsSpoilerRandomizerEnabled()}");
                    if (ArchipelagoHelper.IsSpoilerRandomizerEnabled())
                        __result = "You must be sent the Archipelago item for this spoiler!";
                    break;
                case "MENU_GARAGE_UNLOCK_TIME_TRIAL":
                    if (ArchipelagoHelper.IsHatRandomizerEnabled())
                        __result = "";
                    break;
                case "MENU_GARAGE_UNLOCK_OR":
                    if (ArchipelagoHelper.IsHatRandomizerEnabled())
                        __result = "";
                    break;
                // The main menu button is text rather than art, and shares this key with the
                // gallery's own title, so renaming it here covers both
                case "MENU_MAIN_GALLERY":
                    __result = "Archipelago";
                    break;
            }
        }
    }


    // ========== DEATHLINK PATCHES ==========

    // Entering the fall state (driving off the track) is the closest thing Garfield
    // Kart has to dying, so it's what triggers an outgoing DeathLink
    [HarmonyPatch(typeof(RcVehicle), "OnStartFallState")]
    public class RcVehicle_OnStartFallState_Patch
    {
        static void Postfix(RcVehicle __instance)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (__instance is not Kart kart) return;

            Driver driver = kart.Driver;
            if (driver == null || !driver.IsHuman || !driver.IsLocal) return;

            DeathLinkManager.OnLocalPlayerFell();
        }
    }

    // ========== REWARD PATCHES ==========

    [HarmonyPatch(typeof(RewardManager), "EarnReward")]
    public class RewardManager_EarnReward_Patch
    {
        static void Postfix(RewardManager __instance, 
            string track, 
            int rank = 0,
            E_TimeTrialMedal medal = E_TimeTrialMedal.None, 
            float diffTime = 0f, 
            int nbFirstPlace = 0, 
            int cup = 0
            )
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            E_GameModeType gameMode = Singleton<GameConfigurator>.Instance.GameModeType;

            // Cups pass a hardcoded rank of 0, so they're credited per race in the
            // ChampionShipGameMode patch instead
            if (gameMode == E_GameModeType.SINGLE)
            {
                ArchipelagoRaceVictory.SendChecks(track, rank);
            }

            if (gameMode == E_GameModeType.TIME_TRIAL)
            {
                // Not the medal parameter - that one carries over the game's saved medal
                E_TimeTrialMedal earnedMedal = ArchipelagoHelper.GetMedalEarnedThisRun();
                if (earnedMedal == E_TimeTrialMedal.None) return;

                Log.Message($"Time trial run on {track} earned {earnedMedal} (save reported {medal})");

                foreach (var loc in ArchipelagoConstants.GetTimeTrialLocs(track, earnedMedal))
                {
                    GarfieldKartAPMod.APClient.SendLocation(loc);
                }

                long hatLoc = ArchipelagoConstants.GetHatLoc(track);
                if (hatLoc != -1)
                {
                    GarfieldKartAPMod.APClient.SendLocation(hatLoc);
                }

                if (ArchipelagoGoalManager.GetGoalId() != ArchipelagoConstants.GOAL_TIME_TRIALS ||
                    ArchipelagoHelper.MeetsTimeTrialGoalGrade(earnedMedal))
                {
                    // Persist the completed time trial locally since there is no AP location for the goal
                    ApJsonSaveFile.RecordTimeTrialVictory(track);

                    // Re-check goals after persisting
                    ArchipelagoGoalManager.CheckAndCompleteGoal();
                }
            }

            // ReSharper disable once InvertIf
            if (gameMode == E_GameModeType.CHAMPIONSHIP && nbFirstPlace == 4)
            {
                var spoilerLocs = ArchipelagoConstants.GetSpoilerLocs(cup);
                foreach (long loc in spoilerLocs)
                {
                    GarfieldKartAPMod.APClient.SendLocation(loc);
                }
            }
        }
    }

    // EarnReward only fires after a cup's final track and always claims rank 0, so cup race wins
    // are caught here instead, where the placement is real
    [HarmonyPatch(typeof(ChampionShipGameMode), "OnLocalHumanDriverRaceEnded")]
    public class ChampionShipGameMode_OnLocalHumanDriverRaceEnded_Patch
    {
        static void Postfix(RcVehicle pVehicle)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            if (pVehicle?.RaceStats == null) return;

            ArchipelagoRaceVictory.SendChecks(
                Singleton<GameConfigurator>.Instance.StartScene, pVehicle.RaceStats.GetRank());
        }
    }

    [HarmonyPatch(typeof(RewardManager), "EndChampionShip")]
    public class RewardManager_EndChampionShip_Patch
    {
        static void Postfix(RewardManager __instance, int pFinalRank, int pNbFirstPlace, bool save)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled || pFinalRank != 0) return;
            SendCupVictoryLocation();
            ArchipelagoGoalManager.CheckAndCompleteGoal();
        }

        private static void SendCupVictoryLocation()
        {
            string name = Singleton<GameConfigurator>.Instance.ChampionShipData.ChampionShipNameId;
            Difficulty difficulty = Singleton<GameConfigurator>.Instance.Difficulty;

            int cupId = name switch
            {
                "CHAMPIONSHIP_NAME_1" => 0,
                "CHAMPIONSHIP_NAME_2" => 1,
                "CHAMPIONSHIP_NAME_3" => 2,
                "CHAMPIONSHIP_NAME_4" => 3,
                _ => -1
            };
            if (cupId == -1) return;

            long goalId = ArchipelagoGoalManager.GetGoalId();
            bool ccGated = (goalId == ArchipelagoConstants.GOAL_GRAND_PRIX || goalId == ArchipelagoConstants.GOAL_RACES)
                           && !ArchipelagoHelper.MeetsCCRequirement(difficulty);

            if (ccGated)
            {
                Log.Message("Skipping cup victory location send due to CC requirement");
            }
            else
            {
                GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.GetCupVictoryLoc(cupId));
                ApJsonSaveFile.RecordCupVictory(cupId);
            }

            foreach (long ccLoc in ArchipelagoConstants.GetCupVictoryCCLocs(cupId, difficulty))
            {
                GarfieldKartAPMod.APClient.SendLocation(ccLoc);
            }
        }
    }
}