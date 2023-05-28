using System;
using System.IO;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using LocalLeaderboard.Services;
using ModestTree;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static UnityEngine.EventSystems.EventTrigger;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;


namespace LocalLeaderboard.UI
{
    internal class ScoreInfoModal
    {
        [UIComponent("scoreInfo")]
        public ModalView infoModal;

        [UIComponent("dateScoreText")]
        private TextMeshProUGUI dateScoreText;

        [UIComponent("accScoreText")]
        private TextMeshProUGUI accScoreText;

        [UIComponent("scoreScoreText")]
        private TextMeshProUGUI scoreScoreText;

        [UIComponent("fcScoreText")]
        private TextMeshProUGUI fcScoreText;

        [UIComponent("failScoreText")]
        private TextMeshProUGUI failScoreText;

        [UIComponent("avgHitscoreScoreText")]
        private TextMeshProUGUI avgHitscoreScoreText;

        [UIComponent("maxComboScoreText")]
        private TextMeshProUGUI maxComboScoreText;

        [UIComponent("modifiersScoreText")]
        private TextMeshProUGUI modifiersScoreText;

        [UIComponent("watchReplayButton")]
        private Button watchReplayButton;

        [UIAction("replayStart")]
        void replayStart() => silly(currentEntry);

        [UIParams]
        public BSMLParserParams parserParams;

        [Inject] ReplayService _replayService;
        private static readonly string ReplaysFolderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";
        LLeaderboardEntry currentEntry;

        public void setScoreModalText(LLeaderboardEntry entry)
        {
            string formattedDate = "Error";
            if (long.TryParse(entry.datePlayed, out long unixTimestamp))
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                DateTime datePlayed = dateTimeOffset.LocalDateTime;
                string format = SettingsConfig.Instance.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt";
                dateScoreText.text = string.Format("Date set: <size=6><color=#28b077>{0}</color></size>", datePlayed.ToString(format));
            }
            accScoreText.text = $"Accuracy: <size=6><color=#ffd42a>{entry.acc.ToString("F2")}%</color></size>";
            scoreScoreText.text = $"Score: <size=6>{entry.score}</size>";
            modifiersScoreText.text = $"Mods: <size=5>{entry.mods}</size>";

            if (entry.mods.IsEmpty()) modifiersScoreText.gameObject.SetActive(false);
            else modifiersScoreText.gameObject.SetActive(true);

            if (entry.fullCombo) fcScoreText.text = "<color=green>Full Combo</color>";
            else fcScoreText.text = string.Format("Mistakes: <size=6><color=red>{0}</color></size>", entry.badCutCount + entry.missCount);

            failScoreText.gameObject.SetActive(entry.didFail);
            avgHitscoreScoreText.text = $"Average Hitscore: <size=6>{entry.averageHitscore}</size>";
            maxComboScoreText.text = $"Max Combo: <size=6>{entry.maxCombo}</size>";
            parserParams.EmitEvent("showScoreInfo");
            currentEntry = entry;

            if (File.Exists(ReplaysFolderPath + entry.bsorPath)) watchReplayButton.interactable = true;
            else watchReplayButton.interactable = false;
        }

        private void silly(LLeaderboardEntry leaderboardEntry)
        {
            Plugin.Log.Info("STARTING REPLAY");
            string fileLocation = ReplaysFolderPath + leaderboardEntry.bsorPath;
            Plugin.Log.Info(fileLocation);
            if (_replayService.TryReadReplay(fileLocation, out var replay1))
            {
                BeatLeader.Models.Player player = new BeatLeader.Models.Player();
                player.avatar = "https://raw.githubusercontent.com/speecil/Patrons/main/Untitled%20design%20(16).png";
                player.country = "AUS";
                player.pp = 1;
                player.rank = 1;
                if (long.TryParse(leaderboardEntry.datePlayed, out long unixTimestamp))
                {
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                    DateTime datePlayed = dateTimeOffset.LocalDateTime;
                    string format = SettingsConfig.Instance.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt";
                    player.name = string.Format("You   Date: {0}", datePlayed.ToString(format));
                }
                parserParams.EmitEvent("hideScoreInfo");
                _replayService.StartReplay(replay1, player);
                //GameObject.Find("Replayer2DViewControllerScreen/Replayer2DViewController/BSMLHorizontalLayoutGroup/BSMLVerticalLayoutGroup/BSMLVerticalLayoutGroup/BSMLVerticalLayoutGroup/BSMLHorizontalLayoutGroup/BSMLVerticalLayoutGroup/BSMLHorizontalLayoutGroup/BSMLVerticalLayoutGroup/BSMLHorizontalLayoutGroup/BSMLText").GetComponent<TextMeshProUGUI>().richText = true;
            }
        }
    }
}
