using Aube;
using BepInEx;
using BepInEx.Configuration;
using GarfieldKartAPMod.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MenuHDTrackSelection;

#if DEBUG
using UnityHotReloadNS;
#endif

namespace GarfieldKartAPMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class GarfieldKartAPMod : BaseUnityPlugin
    {
        private const string PluginGuid = PluginAuthor + "." + PluginName;
        private const string PluginAuthor = "Jeffdev";
        private const string PluginName = "GarfieldKartAPMod";
        private const string PluginVersion = "0.5.0";

        public static ConfigEntry<int> notificationTime;

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
#endif

            if (APClient != null && APClient.HasPendingNotifications())
            {
                var notification = APClient.DequeuePendingNotification();
                notificationDisplay.ShowNotification(notification);
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
    // Button Helper Class
    // TODO: Maybe move this class to its own file, but it's probably fine here
    public static class ButtonHelper
    {
        public static void DisableButtonsByIndices(object buttonsArray, params int[] indices)
        {
            try
            {
                var buttonsType = buttonsArray.GetType();
                var lengthProp = buttonsType.GetProperty("Length");
                var indexerProp = buttonsType.GetProperty("Item", [typeof(int)]);
                if (lengthProp == null) return;
                int length = (int)lengthProp.GetValue(buttonsArray);

                foreach (int i in indices)
                {
                    if (i >= length) continue;

                    if (indexerProp == null) continue;
                    var button = indexerProp.GetValue(buttonsArray, [i]) as BetterButton;
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

        public static void DisableLockedRaceButtons(object instance, object m_buttons, int currentCupIndex)
        {
            try
            {
                var buttonsType = m_buttons.GetType();

                // Get Length property
                var lengthProp = buttonsType.GetProperty("Length");
                if (lengthProp == null) return;
                int length = (int)lengthProp.GetValue(m_buttons);

                // Get the indexer with specific parameters (int index)
                var indexerProp = buttonsType.GetProperty("Item", [typeof(int)]);

                for (int i = 0; i < length; i++)
                {
                    if (indexerProp != null)
                    {
                        var button = indexerProp.GetValue(m_buttons, [i]) as BetterButton;
                        if (button == null) continue;

                        if (i == 4)
                        {
                            button.gameObject.SetActive(false);
                            continue; // Skip the last button
                        }

                        int raceId = 4 * currentCupIndex + i; // Race IDs
                    
                        if (!ArchipelagoItemTracker.HasRace(raceId))
                        {
                            button.interactable = false;
                            continue;
                        }

                        button.interactable = true;
                        GkEventSystem.Current.SelectButton(button);
                    }
                    if (instance is MenuHDTrackSelection selection)
                        selection.UpdateRacesButtons(currentCupIndex);
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
            if (cupList.Count == 0)
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

            for (int i = 0; i < tabs.Length; i++)
            {
                bool activateButton = false;
                bool hasRaceInCup = ArchipelagoItemTracker.HasRaceInCup(i + (int)ArchipelagoConstants.ITEM_CUP_UNLOCK_LASAGNA);

                if (gameMode == E_GameModeType.CHAMPIONSHIP && ArchipelagoItemTracker.CanAccessCup(i))
                    activateButton = true;

                else if ((gameMode == E_GameModeType.SINGLE || gameMode == E_GameModeType.TIME_TRIAL) && hasRaceInCup)
                    activateButton = true;

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
                var text = Singleton<GameConfigurator>.Instance.ChampionShipData.Tracks[i];
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

            int lapCount = ArchipelagoHelper.GetLapCount();
            __instance.SetRaceNbLap(lapCount);
#if DEBUG
            __instance.SetRaceNbLap(1); // For testing, set laps to 1
#endif
        }
    }

    [HarmonyPatch(typeof(KartBonusMgr), "SetItem")]
    public class KartBonusMgr_SetItem_Patch
    {
        static bool Prefix(KartBonusMgr __instance, Kart ___m_kart, ref BonusCategory bonus, ref int iQuantity, int byPassSlot = -1, bool isFromCheat = false)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return true;
            if (!___m_kart.Driver.IsHuman) return !ArchipelagoHelper.IsCPUItemsDisabled();

            if (ArchipelagoHelper.IsSpringsOnly()) bonus = BonusCategory.SPRING;

            if (ArchipelagoItemTracker.HasBonusAvailable(bonus))
            {
                return true;
            } 

            return false;
        }
        
        static void Postfix(KartBonusMgr __instance, Kart ___m_kart, BonusCategory bonus, int iQuantity, int byPassSlot = -1, bool isFromCheat = false)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            if (___m_kart.Driver.IsHuman && ArchipelagoItemTracker.HasBonusAvailable(bonus))
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

            if (hasPuzzlePiece)
            {
                Material[] materials = __instance.GetComponent<Renderer>().materials;
                if (materials.Length == 1)
                {
                    materials[0] = __instance.TransparentMaterial;
                }
                __instance.GetComponent<Renderer>().materials = materials;
            }
        }

    }

    [HarmonyPatch(typeof(HUDPositionHD), "TakePuzzlePiece")]
    public class HUDPositionHD_TakePuzzlePiece_Patch
    {
        static bool Prefix(HUDPositionHD __instance, int iIndex, List<Animation> ___m_puzzlesAnimation, List<Image> ___m_puzzleImages, int ___m_iLogPuzzle)
        {
            if (iIndex < 0 || iIndex >= 3)
            {
                return false;
            }
            bool flag = true;
            for (int i = 0; i < 2; i++)
            {
                if (i != iIndex)
                {
                    long puzzlePieceLocation = ArchipelagoConstants.GetPuzzlePieceLoc(Singleton<GameConfigurator>.Instance.StartScene, i);

                    if (!ArchipelagoItemTracker.HasLocation(puzzlePieceLocation))
                    {
                        flag = false;
                        break;
                    }
                }
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

            if (___m_puzzleImages[iIndex] != null)
            {
                ___m_puzzleImages[iIndex].sprite = UITextureSwapper.puzzlePieceFilledSprite;
                if (LogManager.Instance != null)
                {
                    ___m_iLogPuzzle++;
                }
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

            var pieceData = piece.Split('_');
            Int32.TryParse(pieceData[1], out var pieceIndex);

            __result = ArchipelagoItemTracker.HasItem(
                ArchipelagoConstants.GetPuzzlePiece(pieceData[0], pieceIndex));

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

    [HarmonyPatch(typeof(KartSelectionNavigation), "Enter")]
    public class KartSelectionNavigation_Enter_Patch
    {
        static bool Prefix(KartSelectionNavigation __instance, EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> ___m_items)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return true;

            UpdateCharacterUnlocks(__instance, ___m_items);
            UpdateKartUnlocks(__instance, ___m_items);

            return true; // Continue to original method after doing character/kart unlocks
        }

        private static void UpdateCharacterUnlocks(KartSelectionNavigation instance, EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> items)
        {
            if (items != null)
            {
                var indexer = items.GetType().GetMethod(
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

                    bool isUnlocked = (state == UnlockableItemSate.UNLOCKED || state == UnlockableItemSate.NEWUNLOCKED);
                    item.SetLock(!isUnlocked);
                }
            }
        }

        private static void UpdateKartUnlocks(KartSelectionNavigation instance, EnumArray<MenuHDKartSelection.KARTSELECT_TYPE, KartSelectionItem[]> items)
        {
            if (items != null)
            {
                var indexer = items.GetType().GetMethod(
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
                }
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

            if (!charRando || !ArchipelagoHelper.IsConnectedAndEnabled)
                return true;

            long charItemId = 301 + (long)character; // Character IDs start at 301
            Log.Info($"{character}, {charItemId}");

            __result = ArchipelagoItemTracker.HasItem(charItemId) ? UnlockableItemSate.UNLOCKED : UnlockableItemSate.LOCKED;

            return false;
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "GetKartState")]
    public class GameSaveManager_GetKartState_Patch
    {
        static bool Prefix(GameSaveManager __instance, ECharacter kart, ref UnlockableItemSate __result)
        {
            bool kartRando = ArchipelagoHelper.IsKartRandomizerEnabled();
            Log.Info($"{kartRando} CHARACTER RANDO CHECK");

            if (!kartRando || !ArchipelagoHelper.IsConnectedAndEnabled)
                return true;

            long kartItemId = 351 + (long)kart; // Kart IDs start at 351

            __result = ArchipelagoItemTracker.HasItem(kartItemId) ? UnlockableItemSate.UNLOCKED : UnlockableItemSate.LOCKED;

            return false;
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "GetHatState")]
    public class GameSaveManager_GetHatState_Patch
    {
        static bool Prefix(GameSaveManager __instance, string hat, ref UnlockableItemSate __result)
        {
            bool hatRando = ArchipelagoHelper.IsHatRandomizerEnabled();
            bool hatProgressive = ArchipelagoHelper.IsProgressiveHatEnabled();
            char hatTierChar = hat[hat.Length - 1];
            int hatTier = 1;

            switch (hatTierChar)
            {
                case 'N':
                    hatTier = 1;
                    break;
                case 'R':
                    hatTier = 2;
                    break;
                case 'U':
                    hatTier = 3;
                    break;
            }

            long hatItemId = ArchipelagoConstants.GetHatItemId(hat, hatProgressive);

            if (!hatRando || !ArchipelagoHelper.IsConnectedAndEnabled)
                return true;

            if (ArchipelagoItemTracker.HasItem(hatItemId) && !hatProgressive)
            {
                __result = UnlockableItemSate.UNLOCKED;
            }
            else if (hatProgressive && ArchipelagoItemTracker.AmountOfItem(hatItemId) >= hatTier)
            {
                __result = UnlockableItemSate.UNLOCKED;
            }
            else
            {
                __result = UnlockableItemSate.LOCKED;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(GameSaveManager), "GetCustomState")]
    public class GameSaveManager_GetCustomState_Patch
    {
        static bool Prefix(GameSaveManager __instance, string custom, ref UnlockableItemSate __result)
        {
            bool spoilerRando = ArchipelagoHelper.IsSpoilerRandomizerEnabled();
            bool spoilerProgressive = ArchipelagoHelper.IsProgressiveSpoilerEnabled();
            char customTierChar = custom[custom.Length - 1];
            int customTier = 1;

            switch (customTierChar)
            {
                case 'N':
                    customTier = 1;
                    break;
                case 'R':
                    customTier = 2;
                    break;
                case 'U':
                    customTier = 3;
                    break;
            }

            if (!spoilerRando || !ArchipelagoHelper.IsConnectedAndEnabled) 
                return true;


            long customItemId = ArchipelagoConstants.GetSpoilerItemId(custom, spoilerProgressive);

            if (ArchipelagoItemTracker.HasItem(customItemId) && !spoilerProgressive)
            {
                __result = UnlockableItemSate.UNLOCKED;
            } 
            else if (spoilerProgressive && ArchipelagoItemTracker.AmountOfItem(customItemId) >= customTier)
            {
                __result = UnlockableItemSate.UNLOCKED;
            }
            else
            {
                __result = UnlockableItemSate.LOCKED;
            }

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
                    __result = "Receive the Archipelago item for this hat to utilize it!";
                    break;
                case "MENU_GARAGE_UNLOCK_GRAND_PRIX":
                    __result = "Receive the Archipelago item for this spoiler to utilize it!";
                    break;
                case "MENU_GARAGE_UNLOCK_TIME_TRIAL":
                    __result = "";
                    break;
                case "MENU_GARAGE_UNLOCK_OR":
                    __result = "";
                    break;
            }
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
            
            Difficulty difficulty = Singleton<GameConfigurator>.Instance.Difficulty;
            PlayerConfig playerConfig = Singleton<GameConfigurator>.Instance.GetPlayerConfig();
            ECharacter character = playerConfig.Character;
            ECharacter kart = playerConfig.Kart;

            if (gameMode == E_GameModeType.SINGLE && rank == 0)
            {
                string ccReqString = "any";
                if (ArchipelagoGoalManager.GetGoalId() == ArchipelagoConstants.GOAL_GRAND_PRIX || 
                    ArchipelagoGoalManager.GetGoalId() == ArchipelagoConstants.GOAL_RACES)
                {
                    ccReqString = ArchipelagoHelper.GetCCRequirement();
                }

                // TODO: Verify this value actually contains a string in slot data. I'm pretty sure all slot data values are a number at the moment
                if (ccReqString != "any")
                {
                    if ((ccReqString == "easy" && difficulty != Difficulty.EASY) ||
                        (ccReqString == "normal" && difficulty != Difficulty.NORMAL) ||
                        (ccReqString == "hard" && difficulty != Difficulty.HARD))
                    {
                        Log.Message($"Skipping cup victory location send due to CC requirement ({ccReqString})");
                        return;
                    }
                }

                GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.GetRaceVictoryLoc(track));
                GarfieldKartAPMod.APClient.SendLocation((long)character + ArchipelagoConstants.LOC_WIN_RACE_AS_GARFIELD);
                GarfieldKartAPMod.APClient.SendLocation((long)kart + ArchipelagoConstants.LOC_WIN_RACE_WITH_FORMULA_ZZZZ); 

                ArchipelagoGoalManager.CheckAndCompleteGoal();

                var hatLocs = ArchipelagoConstants.GetHatLocs(track, difficulty);
                foreach (var loc in hatLocs)
                {
                    GarfieldKartAPMod.APClient.SendLocation(loc);
                }

            }

            if (gameMode == E_GameModeType.TIME_TRIAL && medal != E_TimeTrialMedal.None)
            {

                var timeTrialLocs = ArchipelagoConstants.GetTimeTrialLocs(track, medal);

                string ttReqString = "bronze";
                if (ArchipelagoGoalManager.GetGoalId() == ArchipelagoConstants.GOAL_TIME_TRIALS)
                {
                    ttReqString = ArchipelagoHelper.GetTimeTrialGoalGrade();
                }

                if ((ttReqString == "bronze" && medal != E_TimeTrialMedal.Bronze) ||
                    (ttReqString == "silver" && medal != E_TimeTrialMedal.Silver) ||
                    (ttReqString == "gold" && medal != E_TimeTrialMedal.Gold) ||
                    (ttReqString == "platinum" && medal != E_TimeTrialMedal.Platinium))
                {
                    Log.Message($"Skipping cup victory location send due to CC requirement ({ttReqString})");
                    return;
                }

                foreach (var loc in timeTrialLocs)
                {
                    GarfieldKartAPMod.APClient.SendLocation(loc);
                }

                // Persist the completed time trial locally since there is no AP location for the goal
                var fw = GameObject.FindObjectOfType<FileWriter>();
                fw?.WriteTimeTrialData(track);

                // We do -1 here because medals go from 1 to 3 and difficulties from 0 to 2
                Difficulty medalDiff = (Difficulty)((int)difficulty - 1);

                var hatLocs = ArchipelagoConstants.GetHatLocs(track, medalDiff);
                foreach (long loc in hatLocs)
                {
                    GarfieldKartAPMod.APClient.SendLocation(loc);
                }

                // Re-` goals after persisting
                ArchipelagoGoalManager.CheckAndCompleteGoal();
            }

            if (gameMode == E_GameModeType.CHAMPIONSHIP && nbFirstPlace == 4)
            {
                var spoilerLocs = ArchipelagoConstants.GetSpoilerLocs(cup, difficulty);
                foreach (long loc in spoilerLocs)
                {
                    GarfieldKartAPMod.APClient.SendLocation(loc);
                }
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
            ArchipelagoGoalManager.CheckAndCompleteGoal();
        }

        private static void SendCupVictoryLocation()
        {
            string name = Singleton<GameConfigurator>.Instance.ChampionShipData.ChampionShipNameId;
            string ccReqString = "any";
            if (ArchipelagoGoalManager.GetGoalId() == ArchipelagoConstants.GOAL_GRAND_PRIX || 
                ArchipelagoGoalManager.GetGoalId() == ArchipelagoConstants.GOAL_RACES)
            {
                ccReqString = ArchipelagoHelper.GetCCRequirement();
            }

            if (ccReqString != "any")
            {
                Difficulty difficulty = Singleton<GameConfigurator>.Instance.Difficulty;
                if ((ccReqString == "easy" && difficulty != Difficulty.EASY) ||
                    (ccReqString == "normal" && difficulty != Difficulty.NORMAL) ||
                    (ccReqString == "hard" && difficulty != Difficulty.HARD))
                {
                    Log.Message($"Skipping cup victory location send due to CC requirement ({ccReqString})");
                    return;
                }
            }

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