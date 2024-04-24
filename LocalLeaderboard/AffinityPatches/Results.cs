using IPA.Utilities;
using IPA.Utilities.Async;
using LocalLeaderboard.Services;
using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.Utils;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace LocalLeaderboard.AffinityPatches
{
    internal class Results : IAffinity
    {
        [Inject] private readonly SiraLog _log;
        
        public static float GetModifierScoreMultiplier(LevelCompletionResults results, GameplayModifiersModelSO modifiersModel)
        {
            if(modifiersModel == null || results == null)
            {
                return 1;
            }
            return modifiersModel.GetTotalMultiplier(modifiersModel.CreateModifierParamsList(results.gameplayModifiers), results.energy);
        }

        public static int GetOriginalIdentifier(IDifficultyBeatmap key)
        {
            if (key == null)
            {
                return 0;
            }
            return key.difficulty switch
            {
                BeatmapDifficulty.Easy => 1,
                BeatmapDifficulty.Normal => 3,
                BeatmapDifficulty.Hard => 5,
                BeatmapDifficulty.Expert => 7,
                BeatmapDifficulty.ExpertPlus => 9,
                _ => 0,
            };
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
        [AffinityPatch(typeof(PrepareLevelCompletionResults), nameof(PrepareLevelCompletionResults.FillLevelCompletionResults))]
        private void Postfix(ref LevelCompletionResults __result, ref IScoreController ____scoreController, ref GameplayModifiersModelSO ____gameplayModifiersModelSO, ref IReadonlyBeatmapData ____beatmapData)
        {
            if (__result.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared)
            {
                return;
            }
            _log.Info("Results postfix called.");
            // i hate this
            LevelCompletionResults localLevelCompletionResults = __result;
            IScoreController localScoreController = ____scoreController;
            GameplayModifiersModelSO localGameplayModifiersModelSO = ____gameplayModifiersModelSO;
            IReadonlyBeatmapData localBeatmapData = ____beatmapData;
            UnityMainThreadTaskScheduler.Factory.StartNew(async () => await PostfixTask(localLevelCompletionResults, localScoreController, localGameplayModifiersModelSO, localBeatmapData));
        }

        private async Task PostfixTask( LevelCompletionResults __result,  IScoreController ____scoreController,  GameplayModifiersModelSO ____gameplayModifiersModelSO,  IReadonlyBeatmapData ____beatmapData)
        {
            await Task.Delay(150); // this is literally only so i can get the replay 100% of the time instead of gambling on the replay being saved in time
            if(ExtraSongDataHolder.IDifficultyBeatmap == null || ExtraSongDataHolder.IDifficultyBeatmap == null || __result == null || ExtraSongData.IsLocalLeaderboardReplay || ____beatmapData == null || ____gameplayModifiersModelSO == null || ____scoreController == null)
            {
                ExtraSongData.IsLocalLeaderboardReplay = false;
                return;
            }
            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(____beatmapData);
            float modifiedScore = __result.modifiedScore;

            if (modifiedScore == 0 || maxScore == 0)
                return;
            float acc = (modifiedScore / maxScore) * 100;
            int score = __result.modifiedScore;
            int badCut = __result.badCutsCount;
            int misses = __result.missedCount;
            bool fc = __result.fullCombo;

            _log.Info("Results: " + acc + " " + score + " " + badCut + " " + misses + " " + fc);

            long unixTimestampSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();

            string currentTime = unixTimestampSeconds.ToString();

            string mapId = ExtraSongDataHolder.IDifficultyBeatmap.level.levelID;

            int difficulty = GetOriginalIdentifier(ExtraSongDataHolder.IDifficultyBeatmap);
            string mapType = ExtraSongDataHolder.IDifficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;

            string balls = mapType + difficulty.ToString(); // BeatMap Allocated Level Label String

            int pauses = ExtraSongDataHolder.pauses;
            float rightHandAverageScore = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.rightHandAverageScore);
            float leftHandAverageScore = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.leftHandAverageScore);
            int perfectStreak = ExtraSongDataHolder.perfectStreak;

            float rightHandTimeDependency = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.rightHandTimeDependency);
            float leftHandTimeDependency = ExtraSongDataHolder.GetAverageFromList(ExtraSongDataHolder.leftHandTimeDependency);
            float fcAcc;
            if (fc) fcAcc = acc;
            else fcAcc = ExtraSongDataHolder.GetFcAcc(GetModifierScoreMultiplier(__result, ____gameplayModifiersModelSO));

            bool didFail = __result.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed;
            int maxCombo = __result.maxCombo;
            float averageHitscore = __result.averageCutScoreForNotesWithFullScoreScoringType;

            string destinationFileName = "BL REPLAY NOT FOUND";

            if (Directory.Exists(Constants.BLREPLAY_PATH) && Plugin.beatLeaderInstalled)
            {
                var directory = new DirectoryInfo(Constants.BLREPLAY_PATH);
                var filePath = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                _log.Info("Found BL Replay: " + filePath.FullName);
                var replayFileName = filePath.Name;

                if (!Directory.Exists(Constants.LLREPLAYS_PATH))
                {
                    Directory.CreateDirectory(Constants.LLREPLAYS_PATH);
                }

                string timestamp = DateTime.UtcNow.Ticks.ToString();
                destinationFileName = Path.GetFileNameWithoutExtension(filePath.Name) + "_" + timestamp + Path.GetExtension(filePath.Name);
                string destinationFilePath = Path.Combine(Constants.LLREPLAYS_PATH, destinationFileName);
                File.Copy(filePath.FullName, destinationFilePath);
            }
            ExtraSongData.IsLocalLeaderboardReplay = false;
            LeaderboardData.LeaderboardData.UpdateBeatMapInfo(mapId, balls, misses, badCut, fc, currentTime, acc, score, GetModifiersString(__result), maxCombo, averageHitscore, didFail, destinationFileName, rightHandAverageScore, leftHandAverageScore, perfectStreak, rightHandTimeDependency, leftHandTimeDependency, fcAcc, pauses);
            var lb = Resources.FindObjectsOfTypeAll<LeaderboardView>().FirstOrDefault();
            lb.OnLeaderboardSet(lb.currentIDifficultyBeatmap);
        }
    }
}
