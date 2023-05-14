using HarmonyLib;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.EventSystems.EventTrigger;

namespace LocalLeaderboard.AffinityPatches
{
    internal class Results : IAffinity
    {
        public static string GetModifiersString(LevelCompletionResults levelCompletionResults, GameEnergyCounter energy)
        {
            string mods = "";

            if (levelCompletionResults.gameplayModifiers.noFailOn0Energy && levelCompletionResults.energy == 0)
            {
                mods += "Fail (NF) ";
            }
            else if (levelCompletionResults.energy == 0)
            {
                mods += "Fail";
            }
            if (levelCompletionResults.gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery)
            {
                mods += "BE ";
            }
            if (levelCompletionResults.gameplayModifiers.instaFail)
            {
                mods += "IF ";
            }
            if (levelCompletionResults.gameplayModifiers.failOnSaberClash)
            {
                mods += "SC ";
            }
            if (levelCompletionResults.gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles)
            {
                mods += "NO ";
            }
            if (levelCompletionResults.gameplayModifiers.noBombs)
            {
                mods += "NB ";
            }
            if (levelCompletionResults.gameplayModifiers.strictAngles)
            {
                mods += "SA ";
            }
            if (levelCompletionResults.gameplayModifiers.disappearingArrows)
            {
                mods += "DA ";
            }
            if (levelCompletionResults.gameplayModifiers.ghostNotes)
            {
                mods += "GN ";
            }
            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower)
            {
                mods += "SS ";
            }
            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster)
            {
                mods += "FS ";
            }
            if (levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast)
            {
                mods += "SF ";
            }
            if (levelCompletionResults.gameplayModifiers.smallCubes)
            {
                mods += "SC ";
            }
            if (levelCompletionResults.gameplayModifiers.proMode)
            {
                mods += "PM ";
            }
            if (levelCompletionResults.gameplayModifiers.noArrows)
            {
                mods += "NA ";
            }
            return mods;
        }

        GameEnergyCounter energy;

        [AffinityPostfix]
        [AffinityPatch(typeof(LevelCompletionResultsHelper), nameof(LevelCompletionResultsHelper.ProcessScore))]
        private void Postfix(ref PlayerData playerData, ref PlayerLevelStatsData playerLevelStats, ref LevelCompletionResults levelCompletionResults, ref IReadonlyBeatmapData transformedBeatmapData, ref IDifficultyBeatmap difficultyBeatmap, ref PlatformLeaderboardsModel platformLeaderboardsModel)
        {
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

            energy = UnityEngine.Resources.FindObjectsOfTypeAll<GameEnergyCounter>().FirstOrDefault();

            LeaderboardData.LeaderboardData.UpdateBeatMapInfo(mapId, balls, misses, badCut, fc, currentTime, acc, score, GetModifiersString(levelCompletionResults, energy));
        }
    }
}
