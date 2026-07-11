using static System.Int32;

namespace GarfieldKartAPMod.Helpers
{
    // General Helper Class
    public static class ArchipelagoHelper
    {
        private static bool IsTrue(string str)
        {
            return str is "true" or "1" or "True";
        }

        public static bool IsConnectedAndEnabled =>
            GarfieldKartAPMod.APClient?.IsConnected ?? false;

        public static bool IsPuzzleRandomizationEnabled()
        {
            string pcs = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_puzzle_pieces");
            return IsTrue(pcs);
        }

        public static bool IsProgressiveCupsEnabled()
        {
            string pcs = GarfieldKartAPMod.APClient.GetSlotDataValue("progressive_cups");
            return IsTrue(pcs);
        }

        public static bool IsCharRandomizerEnabled()
        {
            string charRandoString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_characters");
            return IsTrue(charRandoString);
        }

        public static bool IsKartRandomizerEnabled()
        {
            string kartRandoString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_karts");
            return IsTrue(kartRandoString);
        }

        public static bool IsHatRandomizerEnabled()
        {
            string hatRandoString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_hats");
            return IsTrue(hatRandoString);
        }

        public static bool IsSpoilerRandomizerEnabled()
        {
            string spoilerRandoString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_spoilers");
            return IsTrue(spoilerRandoString);
        }

        public static bool IsItemRandomizerEnabled()
        {
            string itemRandoString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_items");
            return IsTrue(itemRandoString);
        }

        public static bool IsRacesRandomized()
        {
            string raceRandomizerString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_races");

            TryParse(raceRandomizerString, out int raceRandomizer);
            return raceRandomizer == ArchipelagoConstants.OPTION_RANDOMIZE_RACES_RACES || raceRandomizer == ArchipelagoConstants.OPTION_RANDOMIZE_RACES_BOTH;
        }

        public static bool IsCupsRandomized()
        {
            string raceRandomizerString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_races");

            TryParse(raceRandomizerString, out int raceRandomizer);
            return raceRandomizer == ArchipelagoConstants.OPTION_RANDOMIZE_RACES_CUPS || raceRandomizer == ArchipelagoConstants.OPTION_RANDOMIZE_RACES_BOTH;
        }

        public static bool IsRacesAndCupsRandomized()
        {
            string raceRandomizerString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_races");

            TryParse(raceRandomizerString, out int raceRandomizer);
            return raceRandomizer == ArchipelagoConstants.OPTION_RANDOMIZE_RACES_BOTH;
        }

        public static bool IsSpringsOnly()
        {
            string springsOnlyString = GarfieldKartAPMod.APClient.GetSlotDataValue("springs_only");
            return IsTrue(springsOnlyString);
        }

        public static bool IsCPUItemsDisabled()
        {
            string cpuItemString = GarfieldKartAPMod.APClient.GetSlotDataValue("disable_cpu_items");
            return IsTrue(cpuItemString);
        }

        public static bool IsItemManiaEnabled()
        {
            switch (GarfieldKartAPMod.itemManiaMode.Value)
            {
                case ItemManiaMode.On:
                    return true;
                case ItemManiaMode.Off:
                    return false;
                default:
                    return IsTrue(GarfieldKartAPMod.APClient.GetSlotDataValue("item_mania"));
            }
        }

        public static bool IsDeathLinkEnabled()
        {
            switch (GarfieldKartAPMod.deathLink.Value)
            {
                case DeathLinkMode.On:
                    return true;
                case DeathLinkMode.Off:
                    return false;
                default:
                    return IsTrue(GarfieldKartAPMod.APClient.GetSlotDataValue("death_link"));
            }
        }
        public static int GetTimeTrialGoalGrade()
        {
            // Minimum medal grade for the Time Trials goal: 0 = bronze, 1 = silver, 2 = gold
            string grade = GarfieldKartAPMod.APClient.GetSlotDataValue("time_trial_goal_grade");
            TryParse(grade, out int gradeValue);
            return gradeValue;
        }

        public static bool MeetsTimeTrialGoalGrade(E_TimeTrialMedal medal)
        {
            // Medals are 1-indexed (Bronze = 1) while grades are 0-indexed (bronze = 0)
            return (int)medal >= GetTimeTrialGoalGrade() + 1;
        }

        public static int GetCCRequirement()
        {
            // Minimum CC a win must be raced on to count toward the goal: 0 = any, 1 = 50cc, 2 = 100cc, 3 = 150cc
            string requirement = GarfieldKartAPMod.APClient.GetSlotDataValue("cc_requirement");
            TryParse(requirement, out int ccRequirement);
            return ccRequirement;
        }

        public static bool MeetsCCRequirement(Difficulty difficulty)
        {
            // Difficulty EASY/NORMAL/HARD (0-2) races on 50/100/150cc (requirement values 1-3)
            return (int)difficulty + 1 >= GetCCRequirement();
        }

        public static int GetPuzzlePieceCount()
        {
            string puzzleCountString = GarfieldKartAPMod.APClient.GetSlotDataValue("puzzle_piece_count");

            return !TryParse(puzzleCountString, out int reqPuzzleCount) ? throw new SlotDataException($"Invalid puzzle piece goal value passed from slot data: {puzzleCountString}") : reqPuzzleCount;

        }

        public static bool IsLapSanityEnabled()
        {
            string lapSanityString = GarfieldKartAPMod.APClient.GetSlotDataValue("lap_sanity");
            return IsTrue(lapSanityString);
        }

        internal static int GetLapCount()
        {
            if (GarfieldKartAPMod.lapCountOverride.Value > 0)
            {
                return GarfieldKartAPMod.lapCountOverride.Value;
            }
            
            string lapCountString = GarfieldKartAPMod.APClient.GetSlotDataValue("lap_count");

            if (lapCountString == null) return 3;

            return !TryParse(lapCountString, out int lapCount) ? throw new SlotDataException($"Invalid lap count value passed from slot data: {lapCountString}") : lapCount;

        }
    }
}
