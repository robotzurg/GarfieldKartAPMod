using BepInEx;
using UnityEngine;
using System;
using HarmonyLib;
using System.Reflection;
using static Rewired.Controller;
using System.Collections.Generic;
using System.Linq;

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
            Log.Message("Connected to Archipelago");
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

    [HarmonyPatch(typeof(RacePuzzlePiece), "DoTrigger")]
    public class RacePuzzlePiece_DoTrigger
    {
        static void Prefix(RacePuzzlePiece __instance)
        {
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
                GarfieldKartAPMod.APClient.SendLocationFromName("Win Ice Cream Cup");
            }
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
                UITextureSwapper.SwapPuzzlePieceIcons(__instance);
            }
        }
    }
}