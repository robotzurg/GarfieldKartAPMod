using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GarfieldKartAPMod.Helpers
{
    public static class TrackChecksDisplay
    {
        private const string ObjectName = "APTrackChecksText";
        private const string LapsanityObjectName = "APLapsanityText";
        private const string TimeTrialObjectName = "APTimeTrialText";
        private const string CupObjectName = "APCupChecksText";

        private const string FoundColor = "#6BE36B";
        private const string MissingColor = "#999999";

        private const string FoundMark = "<color=" + FoundColor + ">Y</color>";
        private const string MissingMark = "<color=" + MissingColor + ">N</color>";

        // Medal grades in E_TimeTrialMedal order (Bronze = 1), so index + 1 is the medal
        private static readonly string[] MedalLabels = ["Bronze", "Silver", "Gold", "Plat"];

        private static readonly string[] CCLabels = ["50cc", "100cc", "150cc"];

        public static void AttachOrUpdate(HD_TrackSelection_Item item, string trackScene)
        {
            if (item == null) return;

            bool connected = ArchipelagoHelper.IsConnectedAndEnabled && !string.IsNullOrEmpty(trackScene);
            bool isTimeTrial = ArchipelagoHelper.IsTimeTrial();
            bool shouldShow = connected && !isTimeTrial;

            // General checks
            TextMeshProUGUI mainText = GetOrCreateText(item, ObjectName, false);
            if (mainText != null)
            {
                mainText.gameObject.SetActive(shouldShow);
                if (shouldShow)
                {
                    mainText.text = BuildChecksText(trackScene);
                }
            }

            // Lapsanity checks
            TextMeshProUGUI lapText = GetOrCreateText(item, LapsanityObjectName, true);
            if (lapText != null)
            {
                bool showLaps = shouldShow && ArchipelagoHelper.IsLapSanityEnabled() && ArchipelagoHelper.GetLapCount() > 1;
                lapText.gameObject.SetActive(showLaps);
                if (showLaps)
                {
                    lapText.text = BuildLapsanityText(trackScene);
                }
            }

            // Time trial medal checks, in the top spot lapsanity takes on the race menu
            TextMeshProUGUI timeTrialText = GetOrCreateText(item, TimeTrialObjectName, true);
            if (timeTrialText != null)
            {
                bool showTimeTrials = connected && isTimeTrial && ArchipelagoHelper.GetTimeTrialRandomization() > 0;
                timeTrialText.gameObject.SetActive(showTimeTrials);
                if (showTimeTrials)
                {
                    timeTrialText.text = BuildTimeTrialText(trackScene);
                }
            }
        }
        
        public static void AttachOrUpdateCup(HD_TrackSelection_Championship panel, int cupId)
        {
            if (panel == null) return;

            bool shouldShow = ArchipelagoHelper.IsConnectedAndEnabled
                              && ArchipelagoConstants.GetCupVictoryLoc(cupId) != -1;
            
            HD_TrackSelection_Item trackItem = panel.GetComponentInParent<MenuHDTrackSelection>()?.GetComponentInChildren<HD_TrackSelection_Item>(true)
                                               ?? Resources.FindObjectsOfTypeAll<HD_TrackSelection_Item>().FirstOrDefault();
            TextMeshProUGUI style = trackItem?.GetComponentInChildren<TextMeshProUGUI>(true);
            TextMeshProUGUI cupText = GetOrCreateText(panel, CupObjectName, false, style);
            if (cupText == null) return;

            cupText.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                cupText.text = BuildCupText(cupId);
            }
        }

        private static TextMeshProUGUI GetOrCreateText(Component item, string name, bool isTop, TextMeshProUGUI style = null)
        {
            Transform existing = item.transform.Find(name);
            if (existing != null) return existing.GetComponent<TextMeshProUGUI>();

            // Clone an existing TMP text on the item so font, material and canvas
            // setup match the game's UI, then strip everything but the text itself
            TextMeshProUGUI template = item.GetComponentInChildren<TextMeshProUGUI>(true);
            if (template == null)
            {
                Log.Warning($"No TMP text found on {item.name} to clone for the checks display");
                return null;
            }

            GameObject go = Object.Instantiate(template.gameObject, item.transform);
            go.name = name;
            go.SetActive(true);

            foreach (Transform child in go.transform)
            {
                Object.Destroy(child.gameObject);
            }

            // Kill localizers/animators that came with the clone so nothing
            // overwrites our text
            foreach (Component component in go.GetComponents<Component>())
            {
                if (component is RectTransform || component is CanvasRenderer || component is TextMeshProUGUI) continue;
                Object.Destroy(component);
            }

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            text.alignment = isTop ? TextAlignmentOptions.Center : TextAlignmentOptions.Bottom;
            text.fontSize = isTop ? 14f : 20f;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
            if (style != null)
            {
                text.font = style.font;
                text.fontSharedMaterial = style.fontSharedMaterial;
                text.color = style.color;
                text.fontStyle = style.fontStyle;
            }

            RectTransform rect = go.GetComponent<RectTransform>();
            if (isTop)
            {
                // Stretch across the top edge
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(20f, -25f);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(30f, 20f);
            }
            rect.sizeDelta = new Vector2(0f, 24f);
            rect.localScale = Vector3.one;

            return text;
        }

        private static string BuildChecksText(string trackScene)
        {
            long victoryLoc = ArchipelagoConstants.GetRaceVictoryLoc(trackScene);
            if (victoryLoc == -1) return "";

            var parts = new List<string> { $"Win: {Mark(ApJsonSaveFile.HasRaceVictory(trackScene))}" };

            // CC victory checks exist cumulatively up to the CC requirement
            int ccRequirement = ArchipelagoHelper.GetCCRequirement();
            for (int cc = 0; cc < ccRequirement && cc < CCLabels.Length; cc++)
            {
                long ccLoc = ArchipelagoConstants.LOC_RACE_VICTORY_CC_BASE + cc * ArchipelagoConstants.LOC_RACE_VICTORY_CC_GAP + victoryLoc;
                parts.Add($"{CCLabels[cc]}: {Mark(ccLoc)}");
            }

            if (ArchipelagoHelper.IsHatRandomizerEnabled())
            {
                parts.Add($"Hat: {Mark(ArchipelagoConstants.GetHatLoc(trackScene))}");
            }

            return string.Join("\n", parts);
        }

        private static string BuildLapsanityText(string trackScene)
        {
            if (!ArchipelagoHelper.IsLapSanityEnabled()) return "";

            int lapCount = ArchipelagoHelper.GetLapCount();
            if (lapCount <= 1) return "";

            var lapMarks = new List<string>();
            for (int i = 0; i < lapCount; i++)
            {
                long lapLoc = ArchipelagoConstants.GetLapSanityLoc(trackScene, i);
                lapMarks.Add(Mark(lapLoc));
            }
            return $"Lapsanity\n{string.Join("/", lapMarks)}";
        }

        // Only the grades the seed put checks on
        private static string BuildTimeTrialText(string trackScene)
        {
            int highestGrade = ArchipelagoHelper.GetTimeTrialRandomization();

            var medalMarks = new List<string>();
            for (int grade = 1; grade <= highestGrade && grade <= MedalLabels.Length; grade++)
            {
                long medalLoc = ArchipelagoConstants.GetTimeTrialLoc(trackScene, (E_TimeTrialMedal)grade);
                if (medalLoc == -1) continue;

                string color = ArchipelagoItemTracker.HasLocation(medalLoc) ? FoundColor : MissingColor;
                medalMarks.Add($"<color={color}>{MedalLabels[grade - 1]}</color>");
            }

            if (medalMarks.Count == 0) return "";
            string won = Mark(ApJsonSaveFile.HasTimeTrialVictory(trackScene));
            return $"Victory: {won}\n{string.Join("/", medalMarks)}";
        }

        private static string BuildCupText(int cupId)
        {
            var parts = new List<string> { $"Win: {Mark(ApJsonSaveFile.HasCupVictory(cupId))}" };
            
            int ccRequirement = ArchipelagoHelper.GetCCRequirement();
            for (int cc = 0; cc < ccRequirement && cc < CCLabels.Length; cc++)
            {
                long ccLoc = ArchipelagoConstants.LOC_CUP_VICTORY_CC_BASE + cc * ArchipelagoConstants.LOC_CUP_VICTORY_CC_GAP + (cupId + 1);
                parts.Add($"{CCLabels[cc]}: {Mark(ccLoc)}");
            }
            
            if (ArchipelagoHelper.IsSpoilerRandomizerEnabled())
            {
                var spoilerLocs = ArchipelagoConstants.GetSpoilerLocs(cupId);
                for (int spoilerIdx = 0; spoilerIdx < spoilerLocs.Count; spoilerIdx++)
                {
                    parts.Add($"Spoiler {spoilerIdx + 1}: {Mark(ArchipelagoItemTracker.HasLocation(spoilerLocs[spoilerIdx]))}");
                }
            }

            return string.Join("\n", parts);
        }

        // Victories come from the local save
        private static string Mark(bool found) => found ? FoundMark : MissingMark;

        private static string Mark(long locationId) => Mark(ArchipelagoItemTracker.HasLocation(locationId));
    }
}
