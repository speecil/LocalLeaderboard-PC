﻿using IPA.Utilities;
using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.Utils;
using SiraUtil.Affinity;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LocalLeaderboard.AffinityPatches
{
    internal class Results : IAffinity
    {
        float GetModifierScoreMultiplier(LevelCompletionResults results, GameplayModifiersModelSO modifiersModel)
        {
            return modifiersModel.GetTotalMultiplier(modifiersModel.CreateModifierParamsList(results.gameplayModifiers), results.energy);
        }

        public static string GetModifiersString(LevelCompletionResults levelCompletionResults)
        {
            string mods = "";

            if (levelCompletionResults.gameplayModifiers.noFailOn0Energy && levelCompletionResults.energy == 0)
            {
                mods += "NF";
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
            return mods.TrimEnd();
        }

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

            DateTime currentDateTime = DateTime.Now;
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            long unixTimestampSeconds = (long)(currentDateTime.ToUniversalTime() - unixEpoch).TotalSeconds;

            string currentTime = unixTimestampSeconds.ToString();

            string mapId = difficultyBeatmap.level.levelID;

            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = playerLevelStats.beatmapCharacteristic.serializedName;

            string balls = mapType + difficulty.ToString(); // BeatMap Allocated Level Label String

            int pauses = ExtraSongDataHolder.pauses;
            float rightHandAverageScore = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.rightHandAverageScore);
            float leftHandAverageScore = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.leftHandAverageScore);
            int perfectStreak = ExtraSongDataHolder.perfectStreak;

            float rightHandTimeDependency = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.rightHandTimeDependency);
            float leftHandTimeDependency = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.leftHandTimeDependency);
            float fcAcc;
            if (fc) fcAcc = acc;
            else fcAcc = ExtraSongDataHolder.GetFcAcc(GetModifierScoreMultiplier(levelCompletionResults, platformLeaderboardsModel.GetField<GameplayModifiersModelSO, PlatformLeaderboardsModel>("_gameplayModifiersModel")));

            bool didFail = levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed;
            int maxCombo = levelCompletionResults.maxCombo;
            float averageHitscore = levelCompletionResults.averageCutScoreForNotesWithFullScoreScoringType;

            string destinationFileName = "BL REPLAY NOT FOUND";

            if (Directory.Exists(Constants.BLREPLAY_PATH) && Plugin.beatLeaderInstalled)
            {
                var directory = new DirectoryInfo(Constants.BLREPLAY_PATH);
                var filePath = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                var replayFileName = filePath.Name;

                if (!Directory.Exists(Constants.LLREPLAYS_PATH))
                {
                    Directory.CreateDirectory(Constants.LLREPLAYS_PATH);
                }

                string timestamp = DateTime.UtcNow.Ticks.ToString();
                destinationFileName = Path.GetFileNameWithoutExtension(difficultyBeatmap.level.levelID + difficultyBeatmap.difficultyRank) + "_" + timestamp + Path.GetExtension(filePath.Name);
                string destinationFilePath = Path.Combine(Constants.LLREPLAYS_PATH, destinationFileName);
                File.Copy(filePath.FullName, destinationFilePath, true);
            }

            LeaderboardData.LeaderboardData.UpdateBeatMapInfo(mapId, balls, misses, badCut, fc, currentTime, acc, score, GetModifiersString(levelCompletionResults), maxCombo, averageHitscore, didFail, destinationFileName, rightHandAverageScore, leftHandAverageScore, perfectStreak, rightHandTimeDependency, leftHandTimeDependency, fcAcc, pauses);
            var lb = Resources.FindObjectsOfTypeAll<LeaderboardView>().FirstOrDefault();
            lb.OnLeaderboardSet(lb.currentDifficultyBeatmap);
        }
    }
}
