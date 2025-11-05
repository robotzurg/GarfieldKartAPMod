using BepInEx;
using UnityEngine;
using System;
using HarmonyLib;
using System.Reflection;
using static Rewired.Controller;
using System.Collections.Generic;
using System.Linq;
using static Mono.Security.X509.X520;
using TMPro;
using Aube.AnimatorData;

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

            // Initialize Archipelago client
            APClient = new ArchipelagoClient();

            // Hook up save management to connection events
            APClient.OnConnected += OnArchipelagoConnected;
            APClient.OnDisconnected += OnArchipelagoDisconnected;

            harmony = new Harmony(PluginGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo($"{PluginName} loaded successfully!");
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
    // Patch the main menu start
    [HarmonyPatch(typeof(MenuHDMain), "Enter")]
    public class MenuHDMain_Enter_Patch
    {
        static void Postfix()
        {
            Log.Message("Main menu started, creating UI...");
            GarfieldKartAPMod.CreateUI();
        }
    }

    [HarmonyPatch(typeof(MenuHDTrackSelection), "Enter")]
    public class MenuHDTrackSelection_Enter_Patch
    {
        static void Postfix(MenuHDTrackSelection __instance)
        {
            Log.Message("Track Selection Menu opened");

            // Swap puzzle piece icons if connected to Archipelago
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
                UITextureSwapper.SwapPuzzlePieceIcons(__instance.gameObject);
            }
        }
    }

    [HarmonyPatch(typeof(MenuHDGameType), "OnSubmitSplitScreen")]
    public class MenuHDGameType_OnSubmitSplitScreen_Patch
    {
        static bool Prefix(MenuHDGameType __instance)
        {
            // Only override when connected to AP
            if (!GarfieldKartAPMod.APClient.IsConnected)
                return true;

            PopupManager.OpenPopup("Multiplayer is not supported for Archipelago.", PopupHD.POPUP_TYPE.INFORMATION, PopupHD.POPUP_PRIORITY.NORMAL);
            return false;
        }
    }

    [HarmonyPatch(typeof(MenuHDGameType), "OnSubmitMultiplayer")]
    public class MenuHDGameType_OnSubmitMultiplayer_Patch
    {
        static bool Prefix(MenuHDGameType __instance)
        {
            // Only override when connected to AP
            if (!GarfieldKartAPMod.APClient.IsConnected)
                return true;

            PopupManager.OpenPopup("Multiplayer is not supported for Archipelago.", PopupHD.POPUP_TYPE.INFORMATION, PopupHD.POPUP_PRIORITY.NORMAL);
            return false;
        }
    }


    [HarmonyPatch(typeof(MenuHDTrackSelection), "OnSubmitTrack")]
    public class MenuHDTrackSelection_OnSubmitTrack_Patch
    {
        static bool Prefix(MenuHDTrackSelection __instance, int track, int ___m_currentChampionshipIndex)
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

                // Get the constant for the cup
                long cupLoc = 201 + ___m_currentChampionshipIndex;
                long progCups = ArchipelagoItemTracker.AmountOfItem(ArchipelagoConstants.ITEM_PROGRESSIVE_CUP_UNLOCK);
                bool unlock = false;

                // Check if unlocked via Archipelago
                if (ArchipelagoItemTracker.HasItem(cupLoc))
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
                    PopupManager.OpenPopup("You haven't unlocked this cup!", PopupHD.POPUP_TYPE.WARNING, PopupHD.POPUP_PRIORITY.NORMAL);
                    return false;
                }
            } 
            else
            {
                return true;
            }
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
            // Only override when connected to Archipelago
            if (!GarfieldKartAPMod.APClient.IsConnected)
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
                    long puzzlePieceLocId = ArchipelagoConstants.GetPuzzlePiece(Singleton<GameConfigurator>.Instance.StartScene, __instance.Index);
                    GarfieldKartAPMod.APClient.SendLocation(puzzlePieceLocId);
                    Log.Message($"Sending Puzzle Piece {Singleton<GameConfigurator>.Instance.StartScene + "_" + __instance.Index}");
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
                    IReadOnlyCollection<long> checkedLocations = GarfieldKartAPMod.APClient.GetSession().Locations.AllLocationsChecked;
                    int winCount = 0;

                    foreach (long location in checkedLocations)
                    {
                        long[] winChecks = [ArchipelagoConstants.LOC_LASAGNA_CUP_VICTORY,
                            ArchipelagoConstants.LOC_PIZZA_CUP_VICTORY,
                            ArchipelagoConstants.LOC_BURGER_CUP_VICTORY,
                            ArchipelagoConstants.LOC_ICE_CREAM_CUP_VICTORY];
                        if (winChecks.Contains(location))
                        {
                            winCount++;
                        }
                    }

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
                        Log.Message($"Goal is: {goalId}");
                    }

                    int puzzlePieceCount = ArchipelagoItemTracker.GetOverallPuzzlePieceCount();
                    if (puzzlePieceCount == reqPuzzleCount)
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