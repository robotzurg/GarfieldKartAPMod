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
            Log.Init(Logger);

            // Force load Newtonsoft.Json early
            try
            {
                var jsonType = typeof(Newtonsoft.Json.JsonConvert);
                Log.Message($"Loaded Newtonsoft.Json version: {jsonType.Assembly.GetName().Version}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to preload Newtonsoft.Json: {ex.Message}");
            }

            // Set up assembly resolver
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            // Check if System.Numerics types are available
            try
            {
                var bigIntType = Type.GetType("System.Numerics.BigInteger, System.Numerics");
                Logger.LogInfo($"BigInteger available: {bigIntType != null}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"BigInteger check failed: {ex.Message}");
            }

            UITextureSwapper.Initialize();
            fileWriter = gameObject.AddComponent<FileWriter>();

            // Initialize Archipelago client
            APClient = new ArchipelagoClient();

            // Hook up save management to connection events
            APClient.OnConnected += OnArchipelagoConnected;
            APClient.OnDisconnected += OnArchipelagoDisconnected;

            harmony = new Harmony(PluginGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo($"{PluginName} loaded successfully!");
        }

        public void Update()
        {
#if DEBUG
            if (Input.GetKeyUp(KeyCode.F2))
            {
                UnityHotReload.LoadNewAssemblyVersion(
                    typeof(GarfieldKartAPMod).Assembly, // The currently loaded assembly to replace.
                    "C:\\Users\\robot\\AppData\\Roaming\\r2modmanPlus-local\\GarfieldKartFuriousRacing\\profiles\\Default\\BepInEx\\plugins\\Jeffdev-GarfieldKartArchipelago/GarfieldKartAPMod.dll"  // The path to the newly compiled DLL.
                );
            }
#endif

            if (APClient != null && APClient.HasPendingNotifications())
            {
                var notification = APClient.DequeuePendingNotification();

                // Write to file (will only show notification if it's new data)
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
                // Return the already loaded Newtonsoft.Json assembly
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
            if (!uiCreated)
            {
                Log.Message("Creating Archipelago UI...");
                uiObject = new GameObject("ArchipelagoUI");
                DontDestroyOnLoad(uiObject);
                var ui = uiObject.AddComponent<ConnectionUI>();
                ui.Initialize(APClient);

                // Add notification display
                notificationDisplay = uiObject.AddComponent<NotificationDisplay>();
                notificationDisplay.Initialize();

                uiCreated = true;
            }
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
    public class ButtonDisableHelper
    {
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

                    if (!ArchipelagoItemTracker.HasItem(raceItemIdx))
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
                Log.Error($"Failed to disable buttons: {ex}");
            }
        }
    }

    // Patch the main menu start
    [HarmonyPatch(typeof(MenuHDMain), "Enter")]
    public class MenuHDMain_Enter_Patch
    {
        static void Postfix()
        {
            Log.Message("Main menu started, creating UI... Test");
            GarfieldKartAPMod.CreateUI();
        }
    }

    [HarmonyPatch(typeof(MenuHDGameMode), "Enter")]
    public class MenuHDGameMode_Enter_Patch
    {
        static void Postfix(MenuHDGameMode __instance, object ___m_buttons)
        {
            if (!GarfieldKartAPMod.APClient.IsConnected)
                return;

            try
            {
                var buttonsType = ___m_buttons.GetType();

                // Get Length property
                var lengthProp = buttonsType.GetProperty("Length");
                int length = (int)lengthProp.GetValue(___m_buttons);

                // Get the indexer with specific parameters (int index)
                var indexerProp = buttonsType.GetProperty("Item", new Type[] { typeof(int) });

                for (int i = 0; i < length; i++)
                {
                    if (i == 1)
                    {
                        long[] cups = ArchipelagoItemTracker.GetAvailableCups();

                        if (cups.Count() == 0)
                        {
                            var button = indexerProp.GetValue(___m_buttons, new object[] { i }) as BetterButton;
                            if (button != null)
                            {
                                button.interactable = false;
                            }
                        }
                        
                    } else if (i == 2)
                    {
                        var button = indexerProp.GetValue(___m_buttons, new object[] { i }) as BetterButton;
                        if (button != null)
                        {
                            button.interactable = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to disable buttons: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(MenuHDGameMode), "OnSubmitChampionShip")]
    public class MenuHDModeSelect_OnSubmitChampionShip_Patch
    {
        static bool Prefix(MenuHDGameMode __instance)
        {
            if (!GarfieldKartAPMod.APClient.IsConnected)
                return true;

            long[] cups = ArchipelagoItemTracker.GetAvailableCups();

            if (cups.Count() == 0)
            {
                PopupManager.OpenPopup("You haven't unlocked any cups!", PopupHD.POPUP_TYPE.WARNING, PopupHD.POPUP_PRIORITY.NORMAL);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackSelection), "Enter")]
    public class MenuHDTrackSelection_Enter_Patch
    {
        static void Postfix(MenuHDTrackSelection __instance, EnumArray<TAB, BetterToggle> ___m_tabs, object ___m_buttons, ref int ___m_currentChampionshipIndex)
        {
            Log.Message("Track Selection Menu opened");
            ArchipelagoItemTracker.ResyncFromServer();

            object puzzleCheckObj = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_puzzle_pieces");
            bool puzzleCheck = puzzleCheckObj != null && (puzzleCheckObj.ToString() == "true" || puzzleCheckObj.ToString() == "1");

            // Swap puzzle piece icons if connected to Archipelago
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
                if (puzzleCheck)
                {
                    UITextureSwapper.SwapPuzzlePieceIcons(__instance.gameObject);
                }

                int foundTab = -1;
                E_GameModeType gameMode = Singleton<GameConfigurator>.Instance.GameModeType;
                long[] availableCups = ArchipelagoItemTracker.GetAvailableCups();
                int progCups = ArchipelagoItemTracker.AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK);

                for (int i = 0; i < ___m_tabs.Length; i++)
                {
                    long cupItemIdx = 201 + i;
                    bool activateButton = false;
                    if (ArchipelagoItemTracker.HasItem(cupItemIdx) || ArchipelagoItemTracker.HasRaceInCup(cupItemIdx) || (progCups - 1) == i)
                    {
                        if (gameMode == E_GameModeType.CHAMPIONSHIP && (availableCups.Contains(cupItemIdx) || (progCups - 1) == i))
                            activateButton = true;
                        else if (gameMode == E_GameModeType.SINGLE)
                            activateButton = true;

                        if (activateButton)
                        {
                            ___m_tabs[i].gameObject.SetActive(true);
                            if (foundTab == -1)
                            {
                                GkEventSystem.Current.SelectTab(___m_tabs[i]);
                                foundTab = i;
                            }
                        }
                        else
                        {
                            ___m_tabs[i].gameObject.SetActive(false);
                        }

                    }
                    else
                        ___m_tabs[i].gameObject.SetActive(false);
                }

                ButtonDisableHelper.DisableLockedRaceButtons(__instance, ___m_buttons, foundTab != -1 ? foundTab : ___m_currentChampionshipIndex);
            }
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackSelection), "OnSelectChampionship")]
    public class MenuHDTrackSelection_OnSelectChampionship_Patch
    {
        static void Postfix(MenuHDTrackSelection __instance, object ___m_buttons, int iId)
        {
            ButtonDisableHelper.DisableLockedRaceButtons(__instance, ___m_buttons, iId);
        }
    }

    [HarmonyPatch(typeof(GkEventSystem), "OnSecondaryMove")]
    public class GkEventSystem_OnSecondaryMove_Patch
    {
        static Exception Finalizer(Exception __exception)
        {
            if (__exception is InvalidCastException)
            {
                // Suppress the error - return null to prevent it from being thrown
                Log.Debug("Suppressed InvalidCastException in OnSecondaryMove (navigating to disabled tab)");
                return null;
            }

            // Re-throw any other exceptions
            return __exception;
        }
    }

    [HarmonyPatch(typeof(RcRace), "StartRace")]
    public class RcRace_StartRace_Patch
    {
        static void Prefix(RcRace __instance)
        {
            // Swap puzzle piece icons if connected to Archipelago
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
#if DEBUG
                __instance.SetRaceNbLap(1); // For testing, set laps to 1
#endif
            }
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
            if (!GarfieldKartAPMod.APClient.IsConnected)
                return;

            try
            {
                var buttonsType = ___m_buttons.GetType();

                // Get Length property
                var lengthProp = buttonsType.GetProperty("Length");
                int length = (int)lengthProp.GetValue(___m_buttons);

                // Get the indexer with specific parameters (int index)
                var indexerProp = buttonsType.GetProperty("Item", new Type[] { typeof(int) });

                for (int i = 0; i < length; i++)
                {
                    if ((i == 1) || (i == 2))
                    {
                        var button = indexerProp.GetValue(___m_buttons, new object[] { i }) as BetterButton;
                        if (button != null)
                        {
                            button.interactable = false;
                            Log.Info($"Disabled button at index {i}");
                        }
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to disable buttons: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackSelection), "OnSubmitTrack")]
    public class MenuHDTrackSelection_OnSubmitTrack_Patch
    {
        static bool Prefix(MenuHDTrackSelection __instance, int track, int ___m_currentChampionshipIndex, int ___m_currentSelectedButton)
        {
            // Only override when connected to AP
            if (!GarfieldKartAPMod.APClient.IsConnected)
                return true;

            E_GameModeType gameMode = Singleton<GameConfigurator>.Instance.GameModeType;
            if (gameMode == E_GameModeType.CHAMPIONSHIP || gameMode == E_GameModeType.SINGLE)
            {
                object pcs;
                bool progressiveCups = false;
                if (GarfieldKartAPMod.sessionSlotData.TryGetValue("progressive_cups", out pcs))
                {
                    progressiveCups = pcs.ToString() == "true" || pcs.ToString() == "1";
                }

                object rcs;
                string randomizeRaces = "off";
                if (GarfieldKartAPMod.sessionSlotData.TryGetValue("randomize_races", out rcs))
                {
                    randomizeRaces = rcs.ToString();
                }

                // Get the constant for the cup
                long cupLoc = 201 + ___m_currentChampionshipIndex;
                long raceLoc = 101 + (4 * ___m_currentChampionshipIndex) + ___m_currentSelectedButton;
                long progCups = ArchipelagoItemTracker.AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK);
                bool unlock = false;

                // Check if unlocked via Archipelago
                if (ArchipelagoItemTracker.HasItem(cupLoc) || ArchipelagoItemTracker.HasItem(raceLoc))
                {
                    unlock = true;
                }
                else if (progressiveCups)
                {
                    unlock = (cupLoc <= (201 + progCups));
                }

                if (unlock)
                {
                    Log.Message($"[AP] Cup '{cupLoc}' is unlocked - allowing selection");
                    return true; // Run original method
                } 
                else
                {
                    Log.Message($"[AP] Track '{cupLoc}' is LOCKED - showing popup");
                    PopupManager.OpenPopup("You haven't unlocked this!", PopupHD.POPUP_TYPE.WARNING, PopupHD.POPUP_PRIORITY.NORMAL);
                    return false;
                }
            } 
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "IsPuzzlePieceUnlocked")]
    public class GameSaveManager_IsPuzzlePieceUnlocked_Patch
    {
        static bool Prefix(GameSaveManager __instance, string piece, Dictionary<string, bool> ___m_puzzlePieces, ref bool __result)
        {
            object puzzleCheckObj = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_puzzle_pieces");
            bool puzzleCheck = puzzleCheckObj != null && (puzzleCheckObj.ToString() == "true" || puzzleCheckObj.ToString() == "1");

            // Only override when connected to AP
            if (GarfieldKartAPMod.APClient.IsConnected && puzzleCheck)
            {
                var pieceData = piece.Split('_');
                int pieceIndex;
                Int32.TryParse(pieceData[1], out pieceIndex);

                // Set the result that will be returned
                __result = ArchipelagoItemTracker.HasLocation(ArchipelagoConstants.GetPuzzlePieceLoc(pieceData[0], pieceIndex));

                // Return false to skip the original method
                return false;
            }

            // Return true to run the original method
            return true;
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
            TextMeshProUGUI ___m_textCupCircuit
            )
        {
            // Only override when connected to AP
            if (!GarfieldKartAPMod.APClient.IsConnected)
                return true;

            object puzzleCheckObj = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_puzzle_pieces");
            bool puzzleCheck = puzzleCheckObj != null && (puzzleCheckObj.ToString() == "true" || puzzleCheckObj.ToString() == "1");

            if (!puzzleCheck)
                return true;

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
                method.Invoke(__instance, [___m_currentSelectedButton]);
            }
            if (___m_currentSelectedButton < Singleton<GameConfigurator>.Instance.ChampionShipData.TracksName.Length)
            {
                ___m_textCupCircuit.text = Singleton<GameConfigurator>.Instance.ChampionShipData.ChampionShipName + " - " + Singleton<GameConfigurator>.Instance.ChampionShipData.TracksName[___m_currentSelectedButton];
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(HD_TrackSelection_Item), "UpdatePuzzleText")]
    public class HD_TrackSelection_Item_UpdatePuzzleText_Patch
    {
        static bool Prefix(HD_TrackSelection_Item __instance, int value, TextMeshProUGUI ___m_puzzleText, GameObject ___m_boardPuzzle, GameObject ___m_boardPuzzleFull, int ___m_maxPuzzleValue)
        {

            object puzzleCheckObj = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_puzzle_pieces");
            bool puzzleCheck = puzzleCheckObj != null && (puzzleCheckObj.ToString() == "true" || puzzleCheckObj.ToString() == "1");

            // Only override when connected to Archipelago
            if (!GarfieldKartAPMod.APClient.IsConnected || !puzzleCheck)
                return true;

            // Update the text to show "X/3"
            ___m_puzzleText.text = $"{value}/{___m_maxPuzzleValue}";

            // Handle the visual state (full board vs partial)
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

            return false; // Skip original method
        }
    }


    [HarmonyPatch(typeof(RacePuzzlePiece), "DoTrigger")]
    public class RacePuzzlePiece_DoTrigger_Patch
    {
        static void Postfix(RacePuzzlePiece __instance, RcVehicle pVehicle)
        {
            // Swap puzzle piece icons if connected to Archipelago
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
                if (pVehicle && pVehicle.GetControlType() == RcVehicle.ControlType.Human)
                {
                    long puzzlePieceLocId = ArchipelagoConstants.GetPuzzlePieceLoc(Singleton<GameConfigurator>.Instance.StartScene, __instance.Index);
                    GarfieldKartAPMod.APClient.SendLocation(puzzlePieceLocId);
                    Log.Message($"Sending Puzzle Piece {Singleton<GameConfigurator>.Instance.StartScene + "_" + __instance.Index}");
                }
            }

        }
    }

    [HarmonyPatch(typeof(RewardManager), "EarnReward")]
    public class RewardManager_EarnReward_Patch
    {
        static void Postfix(RewardManager __instance, string track, int rank = 0)
        {
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
                switch (Singleton<GameConfigurator>.Instance.GameModeType)
                {
                    case E_GameModeType.SINGLE:
                        if (rank == 0)
                        {
                            GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.GetRaceVictoryLoc(track));
                            int raceWinCount = ArchipelagoItemTracker.GetRaceVictoryCount();
                            if (raceWinCount == 16)
                            {
                                GarfieldKartAPMod.APClient.GetSession().SetGoalAchieved();
                            }
                        }
                    break;
                }
            }
        }
    }

    [HarmonyPatch(typeof(RewardManager), "EndChampionShip")]
    public class RewardManager_EndChampionShip_Patch
    {
        static void Postfix(RewardManager __instance, int pFinalRank, int pNbFirstPlace, bool save)
        {
            string name = Singleton<GameConfigurator>.Instance.ChampionShipData.ChampionShipNameId;
            if (GarfieldKartAPMod.APClient.IsConnected && pFinalRank == 0)
            {
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

                object goal;
                long goalId = ArchipelagoConstants.GOAL_GRAND_PRIX;

                if (GarfieldKartAPMod.sessionSlotData.TryGetValue("goal", out goal))
                {
                    goalId = (long)goal;
                    Log.Message($"Goal is: {goalId}");
                }

                if (goalId == ArchipelagoConstants.GOAL_GRAND_PRIX)
                {
                    int winCount = ArchipelagoItemTracker.GetCupVictoryCount();
                    if (winCount == 4)
                    {
                        GarfieldKartAPMod.APClient.GetSession().SetGoalAchieved();
                    }
                }
                else if (goalId == ArchipelagoConstants.GOAL_PUZZLE_PIECE)
                {
                    object reqPuzzleCountGet;
                    int reqPuzzleCount = 48;
                    if (GarfieldKartAPMod.sessionSlotData.TryGetValue("puzzle_piece_count", out reqPuzzleCountGet))
                    {
                        reqPuzzleCount = (int)reqPuzzleCountGet;
                    }

                    int puzzlePieceCount = ArchipelagoItemTracker.GetOverallPuzzlePieceCount();
                    if (puzzlePieceCount == reqPuzzleCount)
                    {
                        GarfieldKartAPMod.APClient.GetSession().SetGoalAchieved();
                    }
                }
                else if (goalId == ArchipelagoConstants.GOAL_RACES)
                {
                    int raceWinCount = ArchipelagoItemTracker.GetRaceVictoryCount();
                    if (raceWinCount == 16)
                    {
                        GarfieldKartAPMod.APClient.GetSession().SetGoalAchieved();
                    }
                }
                else
                {
                    Log.Debug(goalId);
                }
            } 
        }
    }
}