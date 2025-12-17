using static System.Int32;

namespace GarfieldKartAPMod.Helpers
{
    // General Helper Class
    public static class ArchipelagoHelper
    {
        private static bool IsTrue(string str)
        {
            return str is "true" or "1";
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
            int hatRandoInt = Parse(hatRandoString);
            return hatRandoInt != ArchipelagoConstants.OPTION_RANDOMIZE_HATS_SPOILERS_OFF;
        }

        public static bool IsProgressiveHatEnabled()
        {
            string hatRandoString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_hats");
            int hatProgInt = Parse(hatRandoString);
            return hatProgInt == ArchipelagoConstants.OPTION_RANDOMIZE_HATS_SPOILERS_PROG;
        }

        public static bool IsSpoilerRandomizerEnabled()
        {
            string spoilerRandoString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_spoilers");
            int spoilerRandoInt = Parse(spoilerRandoString);
            return spoilerRandoInt != ArchipelagoConstants.OPTION_RANDOMIZE_HATS_SPOILERS_OFF;
        }

        public static bool IsProgressiveSpoilerEnabled()
        {
            string spoilerProgString = GarfieldKartAPMod.APClient.GetSlotDataValue("randomize_spoilers");
            int spoilerProgInt = Parse(spoilerProgString);
            return spoilerProgInt == ArchipelagoConstants.OPTION_RANDOMIZE_HATS_SPOILERS_PROG;
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
        public static string GetTimeTrialGoalGrade()
        {
            // TODO: It'd probably be nice to use an enum here
            // For now though, this is fine as just a central helper function
            string grade = GarfieldKartAPMod.APClient.GetSlotDataValue("time_trial_goal_grade");
            return grade;
        }

        public static string GetCCRequirement()
        {
            // TODO: Same as above
            string requirement = GarfieldKartAPMod.APClient.GetSlotDataValue("cc_requirement");
            return requirement;
        }

        public static int GetPuzzlePieceCount()
        {
            string puzzleCountString = GarfieldKartAPMod.APClient.GetSlotDataValue("puzzle_piece_count");

            return !TryParse(puzzleCountString, out int reqPuzzleCount) ? throw new SlotDataException($"Invalid puzzle piece goal value passed from slot data: {puzzleCountString}") : reqPuzzleCount;

        }

        internal static int GetLapCount()
        {
            string lapCountString = GarfieldKartAPMod.APClient.GetSlotDataValue("lap_count");

            if (lapCountString == null) return 3;

            return !TryParse(lapCountString, out int lapCount) ? throw new SlotDataException($"Invalid puzzle piece goal value passed from slot data: {lapCountString}") : lapCount;

        }
    }
}
