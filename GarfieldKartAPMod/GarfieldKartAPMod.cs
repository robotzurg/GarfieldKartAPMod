using BepInEx;
using UnityEngine;
using System;
using HarmonyLib;
using System.Reflection;
using static Rewired.Controller;
using System.Collections.Generic;
using System.Linq;
using static Mono.Security.X509.X520;

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
            ArchipelagoItemTracker.LoadFromServer();
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

    public static class ArchipelagoUnlockOverride
    {
        private static HashSet<string> archipelagoUnlocks = new HashSet<string>();

        public static void UnlockItem(string itemId)
        {
            archipelagoUnlocks.Add(itemId);
            Log.Message($"[AP Override] Unlocked: {itemId}");
        }

        public static void LockItem(string itemId)
        {
            archipelagoUnlocks.Remove(itemId);
            Log.Message($"[AP Override] Locked: {itemId}");
        }

        public static bool IsUnlocked(string itemId)
        {
            return archipelagoUnlocks.Contains(itemId);
        }

        public static void Clear()
        {
            archipelagoUnlocks.Clear();
            Log.Message("[AP Override] Cleared all overrides");
        }

        public static bool ShouldBeUnlocked(string itemId, bool originalUnlockState)
        {
            if (!GarfieldKartAPMod.APClient.IsConnected)
            {
                return originalUnlockState; // Not connected, use normal save
            }

            // When connected to AP, use our override
            return IsUnlocked(itemId);
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
                
            }
        }
    }
}