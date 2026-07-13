namespace GarfieldKartAPMod.Helpers
{
    // Everything a finished race is worth, for both the single race and cup race patches
    public static class ArchipelagoRaceVictory
    {
        // GetRank() is 0-indexed, so rank 0 is 1st place
        public static void SendChecks(string track, int rank)
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled || string.IsNullOrEmpty(track)) return;

            Difficulty difficulty = Singleton<GameConfigurator>.Instance.Difficulty;

            SendFinalLapCheck(track, rank, difficulty);

            if (rank != 0) return;

            PlayerConfig playerConfig = Singleton<GameConfigurator>.Instance.GetPlayerConfig();
            ECharacter character = playerConfig.Character;
            ECharacter kart = playerConfig.Kart;

            long goalId = ArchipelagoGoalManager.GetGoalId();
            bool ccGated = (goalId == ArchipelagoConstants.GOAL_GRAND_PRIX || goalId == ArchipelagoConstants.GOAL_RACES)
                           && !ArchipelagoHelper.MeetsCCRequirement(difficulty);

            if (ccGated)
            {
                Log.Message("Skipping race victory location send due to CC requirement");
            }
            else
            {
                GarfieldKartAPMod.APClient.SendLocation(ArchipelagoConstants.GetRaceVictoryLoc(track));
                ApJsonSaveFile.RecordRaceVictory(track);
            }

            foreach (long ccLoc in ArchipelagoConstants.GetRaceVictoryCCLocs(track, difficulty))
            {
                GarfieldKartAPMod.APClient.SendLocation(ccLoc);
            }
            GarfieldKartAPMod.APClient.SendLocation((long)character + ArchipelagoConstants.LOC_WIN_RACE_AS_GARFIELD);
            GarfieldKartAPMod.APClient.SendLocation((long)kart + ArchipelagoConstants.LOC_WIN_RACE_WITH_FORMULA_ZZZZ);

            ArchipelagoGoalManager.CheckAndCompleteGoal();

            long hatLoc = ArchipelagoConstants.GetHatLoc(track);
            if (hatLoc != -1)
            {
                GarfieldKartAPMod.APClient.SendLocation(hatLoc);
            }
        }

        // The final lap never crosses the start line, so it's sent here instead of from the
        // CrossStartLine patch
        private static void SendFinalLapCheck(string track, int rank, Difficulty difficulty)
        {
            if (!ArchipelagoHelper.IsLapSanityEnabled()) return;

            if (!ArchipelagoHelper.MeetsCCRequirement(difficulty))
            {
                Log.Message("Skipping lap sanity check due to CC requirement");
                return;
            }

            // The config value is 1-indexed (1 = 1st place)
            if (rank >= GarfieldKartAPMod.lapSanityPlacementRequirement.Value) return;

            int lastLapIndex = ArchipelagoHelper.GetLapCount() - 1;
            long lapSanityLocId = ArchipelagoConstants.GetLapSanityLoc(track, lastLapIndex);
            if (lapSanityLocId == -1) return;

            GarfieldKartAPMod.APClient.SendLocation(lapSanityLocId);
            Log.Message($"Sent final lap sanity check for {track}, lap {lastLapIndex + 1}");
        }
    }
}
