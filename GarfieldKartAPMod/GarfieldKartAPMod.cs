using BepInEx;
using UnityEngine;
using System;
using HarmonyLib;

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

        public void Awake()
        {
            Log.Init(Logger);

            // Initialize Harmony with a unique ID
            harmony = new Harmony(PluginGUID);

            // Apply all patches in this assembly
            harmony.PatchAll();

            Logger.LogInfo($"{PluginName} loaded successfully!");
        }

        public void OnDestroy()
        {
            // Clean up patches when mod is unloaded
            harmony?.UnpatchSelf();
        }
    }
}
