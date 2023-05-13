using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalLeaderboard.AffinityPatches
{
    internal class Results : IAffinity
    {
        [AffinityPostfix]
        [AffinityPatch(typeof(LevelCompletionResultsHelper), nameof(LevelCompletionResultsHelper.ProcessScore))]
        private void Postfix(ref PlayerData playerData, ref PlayerLevelStatsData playerLevelStats, ref LevelCompletionResults levelCompletionResults, ref IReadonlyBeatmapData transformedBeatmapData, ref IDifficultyBeatmap difficultyBeatmap, ref PlatformLeaderboardsModel platformLeaderboardsModel)
        {
            Plugin.Log.Info("PROCESS SCORE");

            Plugin.Log.Notice($"Score: {levelCompletionResults.modifiedScore}");

            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(transformedBeatmapData);
            float modifiedScore = levelCompletionResults.modifiedScore;
            if (modifiedScore == 0 || maxScore == 0)
                return;
            float acc = (modifiedScore / maxScore) * 100;
            int score = levelCompletionResults.modifiedScore;
            int badCut = levelCompletionResults.badCutsCount;
            int misses = levelCompletionResults.missedCount;
            bool fc = levelCompletionResults.fullCombo;

            string currentTime = DateTime.UtcNow.ToLocalTime().ToString("dd/MM/yy h:mm tt");

            string mapId = difficultyBeatmap.level.levelID;

            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = playerLevelStats.beatmapCharacteristic.serializedName;

            string balls = mapType + difficulty.ToString(); // BeatMap Allocated Level Label String

            LeaderboardData.LeaderboardData.UpdateBeatMapInfo(mapId, balls, misses, badCut, fc, currentTime, acc, score, "MODS HERE");
        }
    }
}
