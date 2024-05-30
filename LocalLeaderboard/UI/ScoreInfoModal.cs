using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using LocalLeaderboard.AffinityPatches;
using LocalLeaderboard.Services;
using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.Utils;
using ModestTree;
using SiraUtil.Logging;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;


namespace LocalLeaderboard.UI
{
    internal class ScoreInfoModal
    {
        [Inject] private readonly SiraLog _log;

        [UIComponent("scoreInfo")]
        public ModalView infoModal;

        [UIComponent("dateScoreText")]
        private readonly TextMeshProUGUI dateScoreText;

        [UIComponent("accScoreText")]
        private readonly TextMeshProUGUI accScoreText;

        [UIComponent("scoreScoreText")]
        private readonly TextMeshProUGUI scoreScoreText;

        [UIComponent("fcScoreText")]
        private readonly TextMeshProUGUI fcScoreText;

        [UIComponent("failScoreText")]
        private readonly TextMeshProUGUI failScoreText;

        [UIComponent("avgHitscoreScoreText")]
        private readonly TextMeshProUGUI avgHitscoreScoreText;

        [UIComponent("maxComboScoreText")]
        private readonly TextMeshProUGUI maxComboScoreText;

        [UIComponent("modifiersScoreText")]
        private readonly TextMeshProUGUI modifiersScoreText;

        [UIComponent("watchReplayButton")]
        private readonly Button watchReplayButton;

        [UIObject("normalModalInfo")]
        private readonly GameObject normalModalInfo;

        [UIObject("moreModalInfo")]
        private readonly GameObject moreModalInfo;

        [UIComponent("moreInfoButton")]
        private readonly Button moreInfoButton;

        [UIComponent("backInfoButton")]
        private readonly Button backInfoButton;

        [UIComponent("leftHandAverageScore")]
        private readonly TextMeshProUGUI leftHandAverageScore;

        [UIComponent("rightHandAverageScore")]
        private readonly TextMeshProUGUI rightHandAverageScore;

        [UIComponent("leftHandTimeDependency")]
        private readonly TextMeshProUGUI leftHandTimeDependency;

        [UIComponent("rightHandTimeDependency")]
        private readonly TextMeshProUGUI rightHandTimeDependency;

        [UIComponent("pauses")]
        private readonly TextMeshProUGUI pauses;

        [UIComponent("perfectStreak")]
        private readonly TextMeshProUGUI perfectStreak;

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

        [InjectOptional] readonly ReplayService _replayService;
        [Inject] readonly LeaderboardView _leaderboardView;

        LLeaderboardEntry currentEntry;

        const int scoreDetails = 4;

        const float infoFontSize = 4.2f;

        /*
            I dont know why it wont work when i use the injected IEnumerator but this does and im tired
         */
        private IEnumerator setButtoncolor(Button button)
        {
            ImageView bgImage = button.transform.Find("BG").gameObject.GetComponent<ImageView>();
            ImageView bgBorder = button.transform.Find("Border").gameObject.GetComponent<ImageView>();
            ImageView bgOutline = button.transform.Find("OutlineWrapper/Outline").gameObject.GetComponent<ImageView>();
            TextMeshProUGUI buttonText = button.transform.Find("Content/Text").gameObject.GetComponent<TextMeshProUGUI>();
            Color bgColour = Constants.SPEECIL_COLOUR;
            Color textColour = Color.white;
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
        private bool isMoreInfo = false;

        [UIAction("moreInfoButtonCLICK")]
        public void moreInfoButtonCLICK()
        {
            isMoreInfo = !isMoreInfo;
            moreInfoButton.gameObject.SetActive(!isMoreInfo);
            backInfoButton.gameObject.SetActive(isMoreInfo);
            moreModalInfo.SetActive(isMoreInfo);
            normalModalInfo.SetActive(!isMoreInfo);

            if (!isMoreInfo)
            {
                moreInfoButton.StopAllCoroutines();
                moreInfoButton.StartCoroutine(setButtoncolor(moreInfoButton));
            }
        }
        public void setScoreModalText(LLeaderboardEntry entry)
        {
            isMoreInfo = false;
            backInfoButton.gameObject.SetActive(false);
            moreInfoButton.gameObject.SetActive(true);
            moreModalInfo.SetActive(false);
            normalModalInfo.SetActive(true);
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
            else fcScoreText.text = entry.badCutCount + entry.missCount < 0 ? "" : $"Mistakes: <size=4><color=red>{entry.badCutCount + entry.missCount}</color></size>";

            avgHitscoreScoreText.text = entry.averageHitscore < 0 ? "" : $"Avg Hitscore: <size={infoFontSize}>{entry.averageHitscore.ToString("F2")}</size>";
            maxComboScoreText.text = entry.maxCombo < 0 ? "" : $"Max Combo: <size={infoFontSize}>{entry.maxCombo}</size>";
            parserParams.EmitEvent("showScoreInfo");
            currentEntry = entry;
            failScoreText.gameObject.SetActive(entry.didFail);

            if (entry.avgAccLeft != 0) leftHandAverageScore.text = $"Left Hand Acc: <size={infoFontSize}><color=#38f2a4>{entry.avgAccLeft:0.##}</color></size>"; else leftHandAverageScore.text = "";
            if (entry.avgAccRight != 0) rightHandAverageScore.text = $"Right Hand Acc: <size={infoFontSize}><color=#38f2a4>{entry.avgAccRight:0.##}</color></size>"; else rightHandAverageScore.text = "";
            if (entry.leftHandTimeDependency != 0) leftHandTimeDependency.text = $"Left Hand TD: <size={infoFontSize}><color=#38f2a4>{entry.leftHandTimeDependency:0.##}</color></size>"; else leftHandTimeDependency.text = "";
            if (entry.rightHandTimeDependency != 0) rightHandTimeDependency.text = $"Right Hand TD: <size={infoFontSize}><color=#38f2a4>{entry.rightHandTimeDependency:0.##}</color></size>"; else rightHandTimeDependency.text = "";
            if (entry.pauses != -1) pauses.text = $"Pauses: <size={infoFontSize}><color=#38f2a4>{entry.pauses}</color></size>"; else pauses.text = "";
            if (entry.perfectStreak != -1)
            {
                perfectStreak.text = $"Perfect Streak: <size={infoFontSize}><color=#38f2a4>{entry.perfectStreak}</color></size>";
                moreInfoButton.interactable = true;
            }
            else
            {
                perfectStreak.text = "";
                moreInfoButton.interactable = false;
            }

            if (moreInfoButton.interactable)
            {
                AttachTextHoverEffect(accScoreText.gameObject, true, accScoreText.text, $"<color=#ffd42a>FC</color> Accuracy: <size={infoFontSize}><color=#38f2a4><i>{entry.fcAcc:F2}%</color></size>", FontStyles.Normal);
                AttachTextHoverEffect(leftHandAverageScore.gameObject, true, leftHandAverageScore.text, $"Left Hand Acc: <size={infoFontSize}><color=#38f2a4><i>{GetAccPercentFromHand(entry.avgAccLeft)}</color></size>", FontStyles.Normal);
                AttachTextHoverEffect(rightHandAverageScore.gameObject, true, rightHandAverageScore.text, $"Right Hand Acc: <size={infoFontSize}><color=#38f2a4><i>{GetAccPercentFromHand(entry.avgAccRight)}</color></size>", FontStyles.Normal);
                moreInfoButton.StartCoroutine(setButtoncolor(moreInfoButton));
            }
            else
            {
                RemoveTextHoverEffect(accScoreText.gameObject);
                RemoveTextHoverEffect(leftHandAverageScore.gameObject);
                RemoveTextHoverEffect(rightHandAverageScore.gameObject);
            }


            if (File.Exists(Constants.LLREPLAYS_PATH + entry.bsorPath) && Constants.BL_INSTALLED())
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

        internal string GetAccPercentFromHand(float handAcc)
        {
            return GetAccPercentFromHandFloat(handAcc).ToString("0.00") + "%";
        }
        internal static float GetAccPercentFromHandFloat(float handAcc)
        {
            return handAcc / 115 * 100;
        }

        private void silly(LLeaderboardEntry leaderboardEntry)
        {
            if (_replayService == null)
            {
                _log.Error("REPLAY SERVICE NULL");
                return;
            }
            _log.Notice("STARTING LOCALLEADERBOARD REPLAY");
            string fileLocation = Constants.LLREPLAYS_PATH + leaderboardEntry.bsorPath;
            if (_replayService.TryReadReplay(fileLocation, out BeatLeader.Models.Replay.Replay replay1))
            {
                parserParams.EmitEvent("hideScoreInfo");
                BeatLeader.Models.Player player = new();
                player.avatar = "https://raw.githubusercontent.com/speecil/LocalLeaderboard-PC/master/LocalLeaderboard/Images/LocalLeaderboard_logo.png";
                player.country = "AUS";
                player.pp = leaderboardEntry.acc;
                player.rank = leaderboardEntry.score;
                DateTime a = new();
                string format = SettingsConfig.Instance.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt";
                if (long.TryParse(leaderboardEntry.datePlayed, out long unixTimestamp))
                {
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                    DateTime datePlayed = dateTimeOffset.LocalDateTime;
                    a = datePlayed;
                    player.name = string.Format("<size=110%><u>You</u>   Date: <color=#28b077>{0}</color></size>", datePlayed.ToString(format));
                }
                ExtraSongData.coolReplayText = "<i><b><color=#28b077>LocalLeaderboard</color> <color=\"red\">REPLAY</color></b>   " + player.name;
                player.name = string.Format("You   Date: {0}", a.ToString(format));
                _replayService.StartReplay(replay1, player);
            }
        }

        public TextHoverEffect AttachTextHoverEffect(GameObject gameObject, bool shouldChangeText = false, string oldText = "", string newText = "", FontStyles daStyle = FontStyles.Normal, FontStyles origStyle = FontStyles.Normal)
        {
            TextHoverEffect textHoverEffect = gameObject.GetComponent<TextHoverEffect>() ?? gameObject.AddComponent<TextHoverEffect>();
            textHoverEffect.daComponent = gameObject.GetComponent<TextMeshProUGUI>();
            textHoverEffect.shouldChangeText = shouldChangeText;
            textHoverEffect.oldText = oldText;
            textHoverEffect.newText = newText;
            textHoverEffect.daStyle = daStyle;
            textHoverEffect.origStyle = origStyle;
            return textHoverEffect;
        }

        public bool RemoveTextHoverEffect(GameObject gameobject)
        {
            TextHoverEffect textHoverEffect = gameobject.GetComponent<TextHoverEffect>();
            if (textHoverEffect != null)
            {
                UnityEngine.Object.Destroy(textHoverEffect);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class TextHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public TextMeshProUGUI daComponent;
        private bool isScaled;
        public bool shouldChangeText = false;
        public string oldText = "";
        public string newText = "";
        public FontStyles daStyle = FontStyles.Normal;
        public FontStyles origStyle = FontStyles.Normal;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isScaled)
            {
                if (shouldChangeText)
                {
                    daComponent.text = newText;
                }
                daComponent.fontStyle = daStyle;
                isScaled = true;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isScaled)
            {
                if (shouldChangeText)
                {
                    daComponent.text = oldText;
                }
                daComponent.fontStyle = origStyle;
                isScaled = false;
            }
        }
    }
}
