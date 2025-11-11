using Aube;
using Aube.AnimatorData;
using BepInEx;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using static Cinemachine.CinemachineTriggerAction.ActionSettings;
using static MenuHDTrackSelection;
using static Mono.Security.X509.X520;
using static Rewired.Controller;

#if DEBUG
using UnityHotReloadNS;
#endif

namespace GarfieldKartAPMod
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class GarfieldKartAPMod : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Jeffdev";
        public const string PluginName = "GarfieldKartAPMod";
        public const string PluginVersion = "1.0.0";

        private Harmony harmony;
        public static Dictionary<string, object> sessionSlotData;
        public static ArchipelagoClient APClient { get; private set; }
        private static GameObject uiObject;
        private static bool uiCreated = false;
        private static NotificationDisplay notificationDisplay;
        private FileWriter fileWriter;

        public void Awake()
        {
            InitializeLogging();
            InitializeAssemblyResolution();
            InitializeComponents();
            ApplyPatches();

            Logger.LogInfo($"{PluginName} loaded successfully!");
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
        }

        private void ApplyPatches()
        {
            harmony = new Harmony(PluginGUID);
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
#endif

            if (APClient != null && APClient.HasPendingNotifications())
            {
                var notification = APClient.DequeuePendingNotification();
                fileWriter.WriteData(notification);
            }
        }

        private void OnArchipelagoConnected()
        {
            Log.Message("Connected to Archipelago - loading items");
            uiObject.GetComponent<ConnectionUI>().ToggleUI();
            PopupManager.OpenPopup("Connected to Archipelago!", PopupHD.POPUP_TYPE.INFORMATION, PopupHD.POPUP_PRIORITY.NORMAL);
        }

        private void OnArchipelagoDisconnected()
        {
            Log.Message("Disconnected from Archipelago");
            ArchipelagoItemTracker.Clear();
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            if (assemblyName.Name == "Newtonsoft.Json")
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "Newtonsoft.Json")
                    {
                        Log.Message($"Resolved Newtonsoft.Json to version {assembly.GetName().Version}");
                        return assembly;
                    }
                }
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
    // General Helper Class
    public static class ArchipelagoHelper
    {
        public static bool IsConnectedAndEnabled =>
            GarfieldKartAPMod.APClient?.IsConnected ?? false;

        public static bool IsPuzzleRandomizationEnabled()
        {
            if (!IsConnectedAndEnabled) return false;

            var puzzleCheck = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_puzzle_pieces");
            return puzzleCheck != null &&
                   (puzzleCheck.ToString() == "true" || puzzleCheck.ToString() == "1");
        }

        public static bool IsProgressiveCupsEnabled()
        {
            if (!GarfieldKartAPMod.sessionSlotData.TryGetValue("progressive_cups", out var pcs))
                return false;
            return pcs.ToString() == "true" || pcs.ToString() == "1";
        }

        public static T GetSlotDataValue<T>(string key, T defaultValue = default)
        {
            if (GarfieldKartAPMod.sessionSlotData == null ||
                !GarfieldKartAPMod.sessionSlotData.TryGetValue(key, out var value))
            {
                return defaultValue;
            }

            if (typeof(T) == typeof(bool))
            {
                return (T)(object)(value.ToString() == "true" || value.ToString() == "1");
            }

            if (typeof(T) == typeof(long) && value is int intValue)
            {
                return (T)(object)(long)intValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public static string GetRandomizeRacesMode()
        {
            return GetSlotDataValue("randomize_races", "off");
        }
    }

    // Button Helper Class
    public static class ButtonHelper
    {
        public static void DisableButtonsByIndices(object buttonsArray, params int[] indices)
        {
            try
            {
                var buttonsType = buttonsArray.GetType();
                var lengthProp = buttonsType.GetProperty("Length");
                var indexerProp = buttonsType.GetProperty("Item", new Type[] { typeof(int) });
                int length = (int)lengthProp.GetValue(buttonsArray);

                foreach (int i in indices)
                {
                    if (i >= length) continue;

                    var button = indexerProp.GetValue(buttonsArray, new object[] { i }) as BetterButton;
                    if (button != null)
                    {
                        button.interactable = false;
                        Log.Info($"Disabled button at index {i}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to disable buttons: {ex}");
            }
        }

        public static void DisableLockedRaceButtons(object instance, object m_buttons, int currentCupIndex)
        {
            try
            {
                var buttonsType = m_buttons.GetType();

                // Get Length property
                var lengthProp = buttonsType.GetProperty("Length");
                int length = (int)lengthProp.GetValue(m_buttons);

                // Get the indexer with specific parameters (int index)
                var indexerProp = buttonsType.GetProperty("Item", new Type[] { typeof(int) });

                for (int i = 0; i < length; i++)
                {
                    var button = indexerProp.GetValue(m_buttons, new object[] { i }) as BetterButton;

                    if (i == 4)
                    {
                        button.gameObject.SetActive(false);
                        continue; // Skip the last button
                    }

                    long raceItemIdx = 101 + (4 * currentCupIndex) + i; // Race IDs

                    Log.Info($"{raceItemIdx}: {ArchipelagoItemTracker.HasItem(raceItemIdx)}, {currentCupIndex + 201}: {ArchipelagoItemTracker.HasItem(currentCupIndex + 201)}");

                    if (!ArchipelagoItemTracker.HasItem(raceItemIdx) && !ArchipelagoItemTracker.HasItem(currentCupIndex + 201))
                    {
                        if (button != null)
                        {
                            button.interactable = false;
                        }
                    }
                    else
                    {
                        button.interactable = true;
                        GkEventSystem.Current.SelectButton(button);
                        if (instance is MenuHDTrackSelection)
                            (instance as MenuHDTrackSelection).UpdateRacesButtons(currentCupIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to disable race buttons: {ex}");
            }
        }
    }

    // Goal Helper Classs
    public static class GoalManager
    {
        public static void CheckAndCompleteGoal()
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            var goalId = ArchipelagoHelper.GetSlotDataValue("goal", ArchipelagoConstants.GOAL_GRAND_PRIX);

            switch (goalId)
            {
                case ArchipelagoConstants.GOAL_GRAND_PRIX:
                    CheckGrandPrixGoal();
                    break;
                case ArchipelagoConstants.GOAL_PUZZLE_PIECE:
                    CheckPuzzlePieceGoal();
                    break;
                case ArchipelagoConstants.GOAL_RACES:
                    CheckRacesGoal();
                    break;
                default:
                    Log.Debug($"Unknown goal ID: {goalId}");
                    break;
            }
        }

        private static void CheckGrandPrixGoal()
        {
            int winCount = ArchipelagoItemTracker.GetCupVictoryCount();
            if (winCount == 4)
            {
                CompleteGoal();
            }
        }

        private static void CheckPuzzlePieceGoal()
        {
            int reqPuzzleCount = ArchipelagoHelper.GetSlotDataValue("puzzle_piece_count", 48);
            int puzzlePieceCount = ArchipelagoItemTracker.GetOverallPuzzlePieceCount();

            if (puzzlePieceCount >= reqPuzzleCount)
            {
                CompleteGoal();
            }
        }

        private static void CheckRacesGoal()
        {
            int raceWinCount = ArchipelagoItemTracker.GetRaceVictoryCount();
            if (raceWinCount == 16)
            {
                CompleteGoal();
            }
        }

        private static void CheckTimeTrialsGoal()
        {
            // Not implemented
        }

        private static void CompleteGoal()
        {
            GarfieldKartAPMod.APClient.GetSession().SetGoalAchieved();
        }
    }

    // Menu Patches

    [HarmonyPatch(typeof(MenuHDMain), "Enter")]
    public class MenuHDMain_Enter_Patch
    {
        static void Postfix()
        {
            Log.Message("Main menu started, creating UI...");
            GarfieldKartAPMod.CreateUI();
        }
    }

    [HarmonyPatch(typeof(MenuHDGameMode), "Enter")]
    public class MenuHDGameMode_Enter_Patch
    {
        static void Postfix(MenuHDGameMode __instance, object ___m_buttons)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            long[] cups = ArchipelagoItemTracker.GetAvailableCups();

            // Disable Championship button if no cups available
            if (cups.Length == 0)
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

            long[] cups = ArchipelagoItemTracker.GetAvailableCups();

            if (cups.Length == 0)
            {
                PopupManager.OpenPopup("You haven't unlocked any cups!", PopupHD.POPUP_TYPE.WARNING, PopupHD.POPUP_PRIORITY.NORMAL);
                return false;
            }
            return true;
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
                UITextureSwapper.SwapPuzzlePieceIcons(__instance.gameObject);
            }

            int foundTab = SetupCupTabs(___m_tabs, ___m_currentChampionshipIndex);
            ButtonHelper.DisableLockedRaceButtons(__instance, ___m_buttons, foundTab != -1 ? foundTab : ___m_currentChampionshipIndex);
        }

        private static int SetupCupTabs(EnumArray<TAB, BetterToggle> tabs, int currentChampionshipIndex)
        {
            int foundTab = -1;
            E_GameModeType gameMode = Singleton<GameConfigurator>.Instance.GameModeType;
            long[] availableCups = ArchipelagoItemTracker.GetAvailableCups();
            int progCups = ArchipelagoItemTracker.AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK);

            for (int i = 0; i < tabs.Length; i++)
            {
                long cupItemIdx = 201 + i;
                bool activateButton = false;

                if (ArchipelagoItemTracker.HasItem(cupItemIdx) ||
                    ArchipelagoItemTracker.HasRaceInCup(cupItemIdx) ||
                    (progCups - 1) == i)
                {
                    if (gameMode == E_GameModeType.CHAMPIONSHIP && (availableCups.Contains(cupItemIdx) || (progCups - 1) == i))
                        activateButton = true;
                    else if (gameMode == E_GameModeType.SINGLE || gameMode == E_GameModeType.TIME_TRIAL)
                        activateButton = true;

                    if (activateButton)
                    {
                        tabs[i].gameObject.SetActive(true);
                        if (foundTab == -1)
                        {
                            GkEventSystem.Current.SelectTab(tabs[i]);
                            foundTab = i;
                        }
                    }
                    else
                    {
                        tabs[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    tabs[i].gameObject.SetActive(false);
                }
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

    [HarmonyPatch(typeof(MenuHDTrackSelection), "OnSubmitTrack")]
    public class MenuHDTrackSelection_OnSubmitTrack_Patch
    {
        static bool Prefix(MenuHDTrackSelection __instance, int track, int ___m_currentChampionshipIndex, int ___m_currentSelectedButton)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return true;

            E_GameModeType gameMode = Singleton<GameConfigurator>.Instance.GameModeType;
            if (gameMode != E_GameModeType.CHAMPIONSHIP && gameMode != E_GameModeType.SINGLE && gameMode != E_GameModeType.TIME_TRIAL)
            {
                return true;
            }

            bool progressiveCups = ArchipelagoHelper.IsProgressiveCupsEnabled();
            long cupItemIdx = 201 + ___m_currentChampionshipIndex;
            long raceItemIdx = 101 + (4 * ___m_currentChampionshipIndex) + ___m_currentSelectedButton;
            long progCups = ArchipelagoItemTracker.AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK);

            bool unlock = ArchipelagoItemTracker.HasItem(cupItemIdx) ||
                         ArchipelagoItemTracker.HasItem(raceItemIdx) ||
                         (progressiveCups && cupItemIdx <= (201 + progCups));

            if (unlock)
            {
                Log.Message($"[AP] Cup '{cupItemIdx}' is unlocked - allowing selection");
                return true;
            }

            Log.Message($"[AP] Cup '{cupItemIdx}' is LOCKED - showing popup");
            PopupManager.OpenPopup("You haven't unlocked this!", PopupHD.POPUP_TYPE.WARNING, PopupHD.POPUP_PRIORITY.NORMAL);
            return false;
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

            string text = "";

            for (int i = 0; i < ___m_itemsButtons.Count - 1; i++)
            {
                ___m_itemsButtons[i].ChangeBackground(PlayerGameEntities.ChampionShipDataList[cup].Sprites[i]);
                text = Singleton<GameConfigurator>.Instance.ChampionShipData.Tracks[i];
                int puzzleCount = ArchipelagoItemTracker.GetPuzzlePieceCount(text);
                ___m_itemsButtons[i].UpdatePuzzleText(puzzleCount);
                ___m_itemsButtons[i].UpdateTimeTrialText(text);
            }

            if (___m_currentSelectedButton != 4 && ___m_hasFinishedEntering)
            {
                var method = AccessTools.Method(typeof(MenuHDTrackSelection), "UpdateTimeTrialValues");
                method.Invoke(__instance, new object[] { ___m_currentSelectedButton });
            }

            if (___m_currentSelectedButton < Singleton<GameConfigurator>.Instance.ChampionShipData.TracksName.Length)
            {
                ___m_textCupCircuit.text = Singleton<GameConfigurator>.Instance.ChampionShipData.ChampionShipName + " - " + Singleton<GameConfigurator>.Instance.ChampionShipData.TracksName[___m_currentSelectedButton];
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(GkEventSystem), "OnSecondaryMove")]
    public class GkEventSystem_OnSecondaryMove_Patch
    {
        static Exception Finalizer(Exception __exception)
        {
            if (__exception is InvalidCastException)
            {
                Log.Debug("Suppressed InvalidCastException in OnSecondaryMove (navigating to disabled tab)");
                return null;
            }
            return __exception;
        }
    }

    // ========== RACE PATCHES ==========

    [HarmonyPatch(typeof(RcRace), "StartRace")]
    public class RcRace_StartRace_Patch
    {
        static void Prefix(RcRace __instance)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

#if DEBUG
            __instance.SetRaceNbLap(1); // For testing, set laps to 1
#endif
        }
    }

    [HarmonyPatch(typeof(RacePuzzlePiece), "DoTrigger")]
    public class RacePuzzlePiece_DoTrigger_Patch
    {
        static void Postfix(RacePuzzlePiece __instance, RcVehicle pVehicle)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            if (pVehicle && pVehicle.GetControlType() == RcVehicle.ControlType.Human)
            {
                long puzzlePieceLocId = ArchipelagoConstants.GetPuzzlePieceLoc(
                    Singleton<GameConfigurator>.Instance.StartScene,
                    __instance.Index);
                GarfieldKartAPMod.APClient.SendLocation(puzzlePieceLocId);
                Log.Message($"Sending Puzzle Piece {Singleton<GameConfigurator>.Instance.StartScene}_{__instance.Index}");
            }
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

            var pieceData = piece.Split('_');
            int pieceIndex;
            Int32.TryParse(pieceData[1], out pieceIndex);

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
            int cup = 0)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;
            E_GameModeType gameMode = Singleton<GameConfigurator>.Instance.GameModeType;


            if (gameMode == E_GameModeType.SINGLE && rank == 0)
            {
                GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.GetRaceVictoryLoc(track));
                GoalManager.CheckAndCompleteGoal();
            }

            if (gameMode == E_GameModeType.CHAMPIONSHIP && nbFirstPlace == 4)
            {
            }
        }
    }

    [HarmonyPatch(typeof(RewardManager), "EndChampionShip")]
    public class RewardManager_EndChampionShip_Patch
    {
        static void Postfix(RewardManager __instance, int pFinalRank, int pNbFirstPlace, bool save)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled || pFinalRank != 0) return;

            SendCupVictoryLocation();
            GoalManager.CheckAndCompleteGoal();
        }

        private static void SendCupVictoryLocation()
        {
            string name = Singleton<GameConfigurator>.Instance.ChampionShipData.ChampionShipNameId;

            switch (name)
            {
                case "CHAMPIONSHIP_NAME_1":
                    GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_LASAGNA_CUP_VICTORY);
                    break;
                case "CHAMPIONSHIP_NAME_2":
                    GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_PIZZA_CUP_VICTORY);
                    break;
                case "CHAMPIONSHIP_NAME_3":
                    GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_BURGER_CUP_VICTORY);
                    break;
                case "CHAMPIONSHIP_NAME_4":
                    GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.LOC_ICE_CREAM_CUP_VICTORY);
                    break;
            }
        }
    }
}