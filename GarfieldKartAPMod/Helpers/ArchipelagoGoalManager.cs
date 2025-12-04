using System;

namespace GarfieldKartAPMod.Helpers
{
    // Goal Helper Classs
    public static class ArchipelagoGoalManager
    {
        public static long GetGoalId()
        {
            string goalString = GarfieldKartAPMod.APClient.GetSlotDataValue("goal");
            Int64.TryParse(goalString, out long goalId);

            return goalId;
        }

        public static void CheckAndCompleteGoal()
        {
            if (!ArchipelagoHelper.IsConnectedAndEnabled) return;

            long goalId = GetGoalId();

            switch (goalId)
            {
                case ArchipelagoConstants.GOAL_GRAND_PRIX:
                    CheckGrandPrixGoal();
                    break;
                case ArchipelagoConstants.GOAL_PUZZLE_PIECE:
                    CheckPuzzlePieceGoal();
                    break;
                case ArchipelagoConstants.GOAL_RACES:
                    CheckRacesGoal();
                    break;
                case ArchipelagoConstants.GOAL_TIME_TRIALS:
                    CheckTimeTrialsGoal();
                    break;
                default:
                    Log.Debug($"Unknown goal ID: {goalId}");
                    break;
            }
        }

        private static void CheckGrandPrixGoal()
        {
            int winCount = ArchipelagoItemTracker.GetCupVictoryCount();
            if (winCount == 4)
            {
                CompleteGoal();
            }
        }

        private static void CheckPuzzlePieceGoal()
        {
            long reqPuzzleCount = ArchipelagoHelper.GetPuzzlePieceCount();
            int puzzlePieceCount = ArchipelagoItemTracker.GetOverallPuzzlePieceCount();

            if (puzzlePieceCount >= reqPuzzleCount)
            {
                CompleteGoal();
            }
        }

        private static void CheckRacesGoal()
        {
            int raceWinCount = ArchipelagoItemTracker.GetRaceVictoryCount();
            if (raceWinCount == 16)
            {
                CompleteGoal();
            }
        }

        private static void CheckTimeTrialsGoal()
        {
            int timeTrialWinCount = ArchipelagoItemTracker.GetTimeTrialVictoryCount();
            if (timeTrialWinCount == 16)
            {
                CompleteGoal();
            }
        }

        private static void CompleteGoal()
        {
            GarfieldKartAPMod.APClient.GetSession().SetGoalAchieved();
        }
    }
}
