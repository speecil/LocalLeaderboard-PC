using IPA.Utilities.Async;
using LocalLeaderboard.Services;
using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.Utils;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using SiraUtil.Submissions;
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
        [InjectOptional] private readonly ReplayService _replayService;

        public static float GetModifierScoreMultiplier(LevelCompletionResults results, GameplayModifiersModelSO modifiersModel)
        {
            if (modifiersModel == null || results == null)
            {
                return 1;
            }
            return modifiersModel.GetTotalMultiplier(modifiersModel.CreateModifierParamsList(results.gameplayModifiers), results.energy);
        }

        public static int GetOriginalIdentifier(BeatmapKey key)
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
            // i hate this
            if (__result.levelEndStateType != LevelCompletionResults.LevelEndStateType.Cleared)
            {
                return;
            }
            if(BS_Utils.Gameplay.ScoreSubmission.Disabled)
            {
                return;
            }
            _log.Info("Cleared results postfix called.");
            LevelCompletionResults localLevelCompletionResults = __result;
            IScoreController localScoreController = ____scoreController;
            IReadonlyBeatmapData localTransformedBeatmapData = ____beatmapData;
            GameplayModifiersModelSO localGameplayModifiersModelSO = ____gameplayModifiersModelSO;
            UnityMainThreadTaskScheduler.Factory.StartNew(async () => await PostfixTask(localLevelCompletionResults, localScoreController, localGameplayModifiersModelSO, localTransformedBeatmapData));
        }

        private async Task PostfixTask(LevelCompletionResults __result, IScoreController ____scoreController, GameplayModifiersModelSO ____gameplayModifiersModelSO, IReadonlyBeatmapData ____beatmapData)
        {
            await Task.Delay(150); // this is literally only so i can get the replay 100% of the time instead of gambling on the replay being saved in time
            if (ExtraSongDataHolder.beatmapKey == null || ExtraSongDataHolder.beatmapLevel == null || __result == null || ExtraSongData.IsLocalLeaderboardReplay || ____beatmapData == null || ____gameplayModifiersModelSO == null || ____scoreController == null)
            {
                ExtraSongData.IsLocalLeaderboardReplay = false;
                return;
            }
            float maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(____beatmapData);
            float modifiedScore = __result.modifiedScore;

            if (modifiedScore == 0 || maxScore == 0)
                return;
            float acc = modifiedScore / maxScore * 100;
            int score = __result.modifiedScore;
            int badCut = __result.badCutsCount;
            int misses = __result.missedCount;
            bool fc = __result.fullCombo;

            _log.Info("Results: " + acc + " " + score + " " + badCut + " " + misses + " " + fc);

            long unixTimestampSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();

            string currentTime = unixTimestampSeconds.ToString();

            string mapId = ExtraSongDataHolder.beatmapLevel.levelID;

            int difficulty = GetOriginalIdentifier(ExtraSongDataHolder.beatmapKey);
            string mapType = ExtraSongDataHolder.beatmapKey.beatmapCharacteristic.serializedName;

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

            if (Directory.Exists(Constants.BLREPLAY_PATH) && Constants.BL_INSTALLED())
            {
                DirectoryInfo directory = new(Constants.BLREPLAY_PATH);
                FileInfo filePath = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
                _log.Info("Found BL Replay: " + filePath.FullName);

                if(_replayService != null)
                {
                    // open the replay to get the info to check if the hash matches
                    if (_replayService.TryReadReplay(filePath.FullName, out BeatLeader.Models.Replay.Replay replay))
                    {
                        if (mapId.Contains(replay.info.hash))
                        {
                            string replayFileName = filePath.Name;

                            if (!Directory.Exists(Constants.LLREPLAYS_PATH))
                            {
                                Directory.CreateDirectory(Constants.LLREPLAYS_PATH);
                            }

                            string timestamp = DateTime.UtcNow.Ticks.ToString();
                            destinationFileName = Path.GetFileNameWithoutExtension(filePath.Name) + "_" + timestamp + Path.GetExtension(filePath.Name);
                            string destinationFilePath = Path.Combine(Constants.LLREPLAYS_PATH, destinationFileName);
                            File.Copy(filePath.FullName, destinationFilePath);
                        }
                    }
                }
            }

            ExtraSongData.IsLocalLeaderboardReplay = false;
            LeaderboardData.LeaderboardData.UpdateBeatMapInfo(mapId, balls, misses, badCut, fc, currentTime, acc, score, GetModifiersString(__result), maxCombo, averageHitscore, didFail, destinationFileName, rightHandAverageScore, leftHandAverageScore, perfectStreak, rightHandTimeDependency, leftHandTimeDependency, fcAcc, pauses);
            LeaderboardView lb = Resources.FindObjectsOfTypeAll<LeaderboardView>().FirstOrDefault();
            lb.OnLeaderboardSet(lb.currentBeatmapKey);
        }
    }
}
