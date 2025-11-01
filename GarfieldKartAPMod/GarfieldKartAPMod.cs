using BepInEx;
using UnityEngine;
using System;
using HarmonyLib;
using System.Reflection;

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

            // Initialize Archipelago client
            APClient = new ArchipelagoClient();

            harmony = new Harmony(PluginGUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Logger.LogInfo($"{PluginName} loaded successfully!");
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
    // Patch the main menu start - adjust class name based on actual game code
    [HarmonyPatch(typeof(MenuHDMain), "Enter")] 
    public class MenuHDMain_Enter_Patch
    {
        static void Postfix()
        {
            Log.Message("Main menu started, creating UI...");
            GarfieldKartAPMod.CreateUI();
        }
    }

    [HarmonyPatch(typeof(RcRace), "StartRace")]
    public class RcRace_StartRace_Patch
    {
        static void Prefix(RcRace __instance)
        {
            if (GarfieldKartAPMod.APClient.IsConnected)
            {
                Log.Message("Race started while connected to Archipelago!");

                GameSaveManager saveManager = GameSaveManager.Instance;
                if (saveManager != null)
                {
                    // Access save data and send locations as needed
                    Log.Message("Save data accessed during race start");
                }
            }
        }
    }
}