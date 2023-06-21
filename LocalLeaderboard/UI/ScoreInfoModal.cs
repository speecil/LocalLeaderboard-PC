using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using LocalLeaderboard.Services;
using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.Utils;
using ModestTree;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
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

        private string _replayHint = "Watch Replay";

        [UIValue("replayHint")]
        private string replayHint
        {
            get => _replayHint;
            set
            {
                _replayHint = value;
            }
        }

        [UIAction("replayStart")]
        void replayStart() => silly(currentEntry);

        [UIParams]
        public BSMLParserParams parserParams;

        [InjectOptional] ReplayService _replayService;
        [Inject] LeaderboardView _leaderboardView;

        LLeaderboardEntry currentEntry;

        const int scoreDetails = 4;

        const float infoFontSize = 4.2f;

        /*
            I dont know why it wont work when i use the injected IEnumerator but this does and im tired
         */
        private IEnumerator setButtoncolor(Button button)
        {
            var bgImage = button.transform.Find("BG").gameObject.GetComponent<ImageView>();
            var bgBorder = button.transform.Find("Border").gameObject.GetComponent<ImageView>();
            var bgOutline = button.transform.Find("OutlineWrapper/Outline").gameObject.GetComponent<ImageView>();
            var buttonText = button.transform.Find("Content/Text").gameObject.GetComponent<TextMeshProUGUI>();
            var bgColour = Constants.SPEECIL_COLOUR;
            var textColour = Color.white;
            while (infoModal.gameObject.activeInHierarchy)
            {
                bgImage.color0 = bgColour;
                bgImage.color1 = bgColour;

                bgBorder.color0 = bgColour;
                bgBorder.color1 = bgColour;
                bgBorder.color = bgColour;

                bgOutline.color = bgColour;
                bgOutline.color0 = bgColour;
                bgOutline.color1 = bgColour;

                buttonText.color = textColour;
                yield return null;
            }
        }

        public void setScoreModalText(LLeaderboardEntry entry)
        {
            string formattedDate = "Error";
            if (long.TryParse(entry.datePlayed, out long unixTimestamp))
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                DateTime datePlayed = dateTimeOffset.LocalDateTime;
                string format = SettingsConfig.Instance.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt";

                if (SettingsConfig.Instance.useRelativeTime)
                {
                    TimeSpan relativeTime = TimeUtils.GetRelativeTime(entry.datePlayed);
                    dateScoreText.text = string.Format("Date set: <size=4.8><color=#28b077>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));
                }
                else
                {
                    dateScoreText.text = string.Format("Date set: <size=4.8><color=#28b077>{0}</color></size>", datePlayed.ToString(format));
                }
            }
            accScoreText.text = $"Accuracy: <size={infoFontSize}><color=#ffd42a>{entry.acc.ToString("F2")}%</color></size>";
            scoreScoreText.text = $"Score: <size={infoFontSize}>{entry.score}</size>";
            modifiersScoreText.text = $"Mods: <size=4.4>{entry.mods}</size>";

            if (entry.mods.IsEmpty()) modifiersScoreText.gameObject.SetActive(false);
            else modifiersScoreText.gameObject.SetActive(true);

            if (entry.fullCombo) fcScoreText.text = "<size=4><color=green>Full Combo!</color></size>";
            else fcScoreText.text = string.Format("Mistakes: <size=4><color=red>{0}</color></size>", entry.badCutCount + entry.missCount);

            avgHitscoreScoreText.text = $"Avg Hitscore: <size={infoFontSize}>{entry.averageHitscore.ToString("F2")}</size>";
            maxComboScoreText.text = $"Max Combo: <size={infoFontSize}>{entry.maxCombo}</size>";
            parserParams.EmitEvent("showScoreInfo");
            currentEntry = entry;
            failScoreText.gameObject.SetActive(entry.didFail);


            if (File.Exists(Constants.LLREPLAYS_PATH + entry.bsorPath))
            {
                watchReplayButton.interactable = true;
                replayHint = "Watch Replay!";
                watchReplayButton.StartCoroutine(setButtoncolor(watchReplayButton));
            }
            else
            {
                watchReplayButton.interactable = false;
                replayHint = "Beatleader not installed or replay does not exist!";
            }
        }

        private void silly(LLeaderboardEntry leaderboardEntry)
        {
            if (_replayService == null) return;
            Plugin.Log.Info("STARTING REPLAY");
            string fileLocation = Constants.LLREPLAYS_PATH + leaderboardEntry.bsorPath;
            Plugin.Log.Info(fileLocation);
            if (_replayService.TryReadReplay(fileLocation, out var replay1))
            {
                BeatLeader.Models.Player player = new BeatLeader.Models.Player();
                player.avatar = "https://raw.githubusercontent.com/speecil/Patrons/main/Untitled%20design%20(16).png";
                player.country = "AUS";
                player.pp = leaderboardEntry.acc;
                player.rank = leaderboardEntry.score;
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
