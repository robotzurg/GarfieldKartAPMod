using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GarfieldKartAPMod.Helpers
{
    public static class TrackChecksDisplay
    {
        private const string ObjectName = "APTrackChecksText";
        private const string LapsanityObjectName = "APLapsanityText";
        private const string TimeTrialObjectName = "APTimeTrialText";

        private const string FoundColor = "#6BE36B";
        private const string MissingColor = "#999999";

        private const string FoundMark = "<color=" + FoundColor + ">Y</color>";
        private const string MissingMark = "<color=" + MissingColor + ">N</color>";

        // Medal grades in E_TimeTrialMedal order (Bronze = 1), so index + 1 is the medal
        private static readonly string[] MedalLabels = ["Bronze", "Silver", "Gold", "Plat"];

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

        private static TextMeshProUGUI GetOrCreateText(HD_TrackSelection_Item item, string name, bool isTop)
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

            var parts = new List<string> { $"Win: {Mark(victoryLoc)}" };

            // CC victory checks exist cumulatively up to the CC requirement
            int ccRequirement = ArchipelagoHelper.GetCCRequirement();
            string[] ccLabels = ["50cc", "100cc", "150cc"];
            for (int cc = 0; cc < ccRequirement && cc < ccLabels.Length; cc++)
            {
                long ccLoc = ArchipelagoConstants.LOC_RACE_VICTORY_CC_BASE + cc * ArchipelagoConstants.LOC_RACE_VICTORY_CC_GAP + victoryLoc;
                parts.Add($"{ccLabels[cc]}: {Mark(ccLoc)}");
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
            return $"Time Trials\n{string.Join("/", medalMarks)}";
        }

        private static string Mark(long locationId)
        {
            return ArchipelagoItemTracker.HasLocation(locationId) ? FoundMark : MissingMark;
        }
    }
}
