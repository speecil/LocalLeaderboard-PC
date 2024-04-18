using BeatSaberMarkupLanguage.Components;
using ModestTree;
using SiraUtil.Affinity;
using SiraUtil.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

namespace LocalLeaderboard.AffinityPatches
{
    internal class ExtraSongData : IAffinity
    {
        [Inject] private readonly SiraLog _log;
        [Inject] internal readonly BeatmapKey _key;
        [Inject] internal readonly BeatmapLevel _beatmapLevel;
        internal int currentPerfectHits = 0;
        internal int highestPerfectStreak = 0;

        internal static bool IsLocalLeaderboardReplay = false;

        internal TextMeshPro _replayWatermark;
        internal static string coolReplayText;

        [AffinityPostfix]
        [AffinityPatch(typeof(AudioTimeSyncController), "Start")]
        public void Postfixaaa()
        {
            _log.Debug("Resetting ExtraSongData");
            ExtraSongDataHolder.reset();
            currentPerfectHits = 0;
            highestPerfectStreak = 0;
            ExtraSongDataHolder.beatmapKey = _key;
            ExtraSongDataHolder.beatmapLevel = _beatmapLevel;
            if (IsLocalLeaderboardReplay)
            {
                try
                {
                    TextMeshPro[] textMeshPros = Resources.FindObjectsOfTypeAll<TextMeshPro>();
                    TextMeshPro textMeshPro = textMeshPros.FirstOrDefault(x => x.text.Contains("REPLAY"));
                    if (textMeshPro != null)
                    {
                        textMeshPro.richText = true;
                        _replayWatermark = textMeshPro;
                        _replayWatermark.text = coolReplayText ?? _replayWatermark.text;
                    }
                    //SharedCoroutineStarter.instance.StartCoroutine(WaitForTextActive());
                }
                catch
                {
                }
            }
        }

        internal IEnumerator WaitForTextActive()
        {
            FormattableText ab = null;
            while (ab == null)
            {
                _log.Debug("Waiting for text to be found");
                List<FormattableText> x = Resources.FindObjectsOfTypeAll<FormattableText>().Where(x => x.name.Contains("<size=110%><u>You</u>")).ToList();
                foreach (FormattableText item in x)
                {
                    if (item.text != _replayWatermark.text)
                    {
                        ab = item;
                    }
                }
            }

            _log.Debug("Text found");
            _log.Debug(ab.text);
            ab.richText = true;
            yield return null;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(PauseMenuManager), "RestartButtonPressed")]
        public void PauseMenuManagerRestartButtonPressed()
        {
            _log.Debug("Resetting ExtraSongData");
            ExtraSongDataHolder.reset();
            currentPerfectHits = 0;
            highestPerfectStreak = 0;
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidFinish))]
        public void FlyingScoreEffectHandleCutScoreBufferDidFinish(ref CutScoreBuffer ____cutScoreBuffer)
        {
            if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorA)
            {
                ExtraSongDataHolder.leftHandAverageScore.Add(____cutScoreBuffer.cutScore);
                ExtraSongDataHolder.leftHandTimeDependency.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));
                ExtraSongDataHolder.totalBlocksHit.Add(new Tuple<int, int>(____cutScoreBuffer.cutScore, (int)____cutScoreBuffer.noteCutInfo.noteData.scoringType));
            }
            else if (____cutScoreBuffer.noteCutInfo.noteData.colorType == ColorType.ColorB)
            {
                ExtraSongDataHolder.rightHandAverageScore.Add(____cutScoreBuffer.cutScore);
                ExtraSongDataHolder.rightHandTimeDependency.Add(Math.Abs(____cutScoreBuffer.noteCutInfo.cutNormal.z));
                ExtraSongDataHolder.totalBlocksHit.Add(new Tuple<int, int>(____cutScoreBuffer.cutScore, (int)____cutScoreBuffer.noteCutInfo.noteData.scoringType));
            }

            if (____cutScoreBuffer.cutScore == 115)
            {
                currentPerfectHits++;
                if (currentPerfectHits > highestPerfectStreak)
                {
                    highestPerfectStreak = currentPerfectHits;
                    ExtraSongDataHolder.perfectStreak = highestPerfectStreak;
                }
            }
            else
            {
                currentPerfectHits = 0;
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(GamePause), nameof(GamePause.Pause))]
        public void GamePausePause()
        {
            ExtraSongDataHolder.pauses++;
        }
    }

    internal static class ExtraSongDataHolder
    {
        internal static int pauses;
        internal static List<int> rightHandAverageScore = new();
        internal static List<int> leftHandAverageScore = new();
        internal static List<float> rightHandTimeDependency = new();
        internal static List<float> leftHandTimeDependency = new();
        internal static List<Tuple<int, int>> totalBlocksHit = new();
        internal static int perfectStreak = 0;
        internal static BeatmapKey beatmapKey;
        internal static BeatmapLevel beatmapLevel;

        internal static void reset()
        {
            pauses = 0;
            rightHandAverageScore.Clear();
            leftHandAverageScore.Clear();
            rightHandTimeDependency.Clear();
            leftHandTimeDependency.Clear();
            perfectStreak = 0;
        }

        internal static float GetAverageFromList(List<int> list)
        {
            float sum = 0;
            foreach (float f in list)
            {
                sum += f;
            }
            if (list.Count == 0)
            {
                return 0;
            }
            return sum / list.Count;
        }

        internal static float GetAverageFromList(List<float> list)
        {
            float sum = 0;
            foreach (float f in list)
            {
                sum += f;
            }
            if (list.Count == 0)
            {
                return 0;
            }
            return sum / list.Count;
        }

        internal static int GetTotalFromList(List<int> list)
        {
            int sum = 0;
            foreach (int i in list)
            {
                sum += i;
            }
            return sum;
        }


        internal static int GetMaxScoreForScoringType(int scoringType)
        {
            return scoringType switch
            {
                1 or 2 or 3 => 115,
                4 => 70,
                5 => 20,
                _ => 0,
            };
        }

        internal static float GetFcAcc(float multiplier)
        {
            if (totalBlocksHit.IsEmpty()) return 0.0f;
            float realScore = 0, maxScore = 0;
            foreach (Tuple<int, int> p in totalBlocksHit)
            {
                realScore += p.Item1 * multiplier;
                maxScore += GetMaxScoreForScoringType(p.Item2);
            }
            float fcAcc = (float)realScore / (float)maxScore * 100.0f;
            return fcAcc;
        }
    }
}
