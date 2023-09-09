using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using LeaderboardCore.Interfaces;
using LocalLeaderboard.Services;
using LocalLeaderboard.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;
using ScoreData = LeaderboardTableView.ScoreData;

namespace LocalLeaderboard.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"../Views/LeaderboardView.bsml")]
    [ViewDefinition("LocalLeaderboard.UI.Views.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet
    {
        [Inject] private PanelView _panelView;
        [Inject] private PlatformLeaderboardViewController _plvc;
        [Inject] private TweeningService _tweeningService;
        //[Inject] private PlayerService _playerService;

        private bool Ascending = true;
        public static LLeaderboardEntry[] buttonEntryArray = new LLeaderboardEntry[10];
        public bool UserIsPatron = false;

        SettingsConfig config = SettingsConfig.Instance;

        public IDifficultyBeatmap currentDifficultyBeatmap;

        [UIComponent("leaderboardTableView")]
        private LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        private Transform leaderboardTransform = null;

        [UIComponent("errorText")]
        private TextMeshProUGUI errorText;

        [UIComponent("headerText")]
        private TextMeshProUGUI headerText;

        [UIComponent("up_button")]
        private Button up_button;

        [UIComponent("down_button")]
        private Button down_button;

        [UIComponent("sorter")]
        private ImageView sorter;

        [UIComponent("myHeader")]
        private Backgroundable myHeader;

        [UIComponent("discordButton")]
        private Button discordButton;

        [UIComponent("patreonButton")]
        private Button patreonButton;

        [UIComponent("websiteButton")]
        private Button websiteButton;

        [UIComponent("retryButton")]
        private Button retryButton;

        [UIComponent("uwuToggle")]
        private ToggleSetting uwuToggle;

        [UIComponent("nameToggle")]
        private ToggleSetting nameToggle;

        [UIComponent("americanToggle")]
        private ToggleSetting americanToggle;

        [UIComponent("relativeToggle")]
        private ToggleSetting relativeToggle;

        [UIComponent("infoModal")]
        private ModalView infoModal;

        [UIComponent("settingsModal")]
        private ModalView settingsModal;

        [UIValue("buttonHolders")]
        [Inject] private List<ButtonHolder> holders;

        [UIComponent("scoreInfoModal")]
        [Inject] private ScoreInfoModal scoreInfoModal;

        [UIParams]
        BSMLParserParams parserParams;


        public int page = 0;
        public int totalPages;
        public int sortMethod;

        [UIAction("OnPageUp")]
        private void OnPageUp() => UpdatePageChanged(-1);

        [UIAction("OnPageDown")]
        private void OnPageDown() => UpdatePageChanged(1);

        private void UpdatePageButtons()
        {
            up_button.interactable = (page > 0);
            down_button.interactable = (page < totalPages - 1);
        }

        private void UpdatePageChanged(int inc)
        {
            page = Mathf.Clamp(page + inc, 0, totalPages - 1);
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        [UIAction("changeSort")]
        private void changeSort()
        {
            sorter.gameObject.SetActive(true);
            if (!sorter.gameObject.activeSelf) return;

            _tweeningService.RotateTransform(sorter.GetComponentInChildren<ImageView>().transform, 180f, 0.1f, () =>
            {
                Ascending = !Ascending;
                UnityMainThreadTaskScheduler.Factory.StartNew(() => OnLeaderboardSet(currentDifficultyBeatmap));
            });
        }

        [UIAction("Discord")]
        private void Discord() => Application.OpenURL(Constants.DISCORD_URL);

        [UIAction("Patreon")]
        private void Patreon() => Application.OpenURL(Constants.PATREON_URL);

        [UIAction("Website")]
        private void Website() => Application.OpenURL(Constants.WEBSITE_URL);

        [UIAction("showSettings")]
        private void showSettings()
        {
            parserParams.EmitEvent("showSettings");
            uwuToggle.interactable = UserIsPatron;
            nameToggle.interactable = UserIsPatron;
            settingsModal.StartCoroutine(setToggle(americanToggle));
            settingsModal.StartCoroutine(setToggle(uwuToggle));
            settingsModal.StartCoroutine(setToggle(relativeToggle));
            settingsModal.StartCoroutine(setToggle(nameToggle));
        }

        [UIValue("dateoption")]
        private bool dateoption
        {
            get => config.BurgerDate;
            set
            {
                config.BurgerDate = value; // fuck you i removed the line cope
                OnLeaderboardSet(currentDifficultyBeatmap);
            }
        }

        [UIValue("PatreonCheck")]
        private bool PatreonCheck => UserIsPatron;

        [UIValue("rainbowsuwu")]
        private bool rainbowsuwu
        {
            get
            {
                if (!UserIsPatron) return false;
                return config.rainbowsuwu;
            }
            set
            {
                config.rainbowsuwu = value;
                _panelView.toggleRainbow(config.rainbowsuwu);
            }
        }

        [UIValue("nameheaderoption")]
        private bool nameheaderoption
        {
            get
            {
                if (!UserIsPatron) return false;
                return config.nameHeaderToggle;
            }
            set
            {
                config.nameHeaderToggle = value;
                setHeaderText(headerText, UserIsPatron);
            }
        }

        [UIValue("useRelativeTime")]
        private bool useRelativeTime
        {
            get => config.useRelativeTime;
            set
            {
                config.useRelativeTime = value;
                OnLeaderboardSet(currentDifficultyBeatmap);
            }
        }

        [UIValue("patreonHint")]
        private string patreonHint
        {
            get => UserIsPatron ? "Hi patreon user :3" : "Patreon Access Only";
        }

        void setHeaderText(TextMeshProUGUI text, bool patreon)
        {
            if (patreon && config.nameHeaderToggle)
            {
                text.text = Plugin.userName.ToUpper() + "'S LEADERBOARD";
            }
            else
            {
                text.text = "LOCAL LEADERBOARD";
            }
        }

        public void showModal()
        {
            parserParams.EmitEvent("hideSettings");
            parserParams.EmitEvent("showInfoModal");
            scoreInfoModal.parserParams.EmitEvent("hideScoreInfo");
            infoModal.StartCoroutine(setcolor(websiteButton));
            infoModal.StartCoroutine(setcolor(discordButton));
            infoModal.StartCoroutine(setcolor(patreonButton));
        }

        [UIAction("Retry")]
        private void Retry() => OnLeaderboardSet(currentDifficultyBeatmap);

        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index)
        {
            sortMethod = index;
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        [UIValue("leaderboardIcons")]
        private List<IconSegmentedControl.DataItem> leaderboardIcons
        {
            get
            {
                return new List<IconSegmentedControl.DataItem>()
                {
                new IconSegmentedControl.DataItem(
                    Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.clock.png"), "Date / Time"),
                new IconSegmentedControl.DataItem(
                    Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.score.png"), "Highscore")
                };
            }
        }
        internal static readonly FieldAccessor<ImageView, float>.Accessor ImageSkew = FieldAccessor<ImageView, float>.GetAccessor("_skew");
        internal static readonly FieldAccessor<ImageView, bool>.Accessor ImageGradient = FieldAccessor<ImageView, bool>.GetAccessor("_gradient");

        private ImageView _imgView;
        private GameObject _loadingControl;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            myHeader.background.material = Utilities.ImageResources.NoGlowMat;
            _loadingControl = leaderboardTransform.Find("LoadingControl").gameObject;
            var loadingContainer = _loadingControl.transform.Find("LoadingContainer");
            loadingContainer.gameObject.SetActive(false);
            Destroy(loadingContainer.Find("Text").gameObject);
            Destroy(_loadingControl.transform.Find("RefreshContainer").gameObject);
            Destroy(_loadingControl.transform.Find("DownloadingContainer").gameObject);

            _imgView = myHeader.background as ImageView;
            _imgView.color = Constants.SPEECIL_COLOUR;
            _imgView.color0 = Constants.SPEECIL_COLOUR;
            _imgView.color1 = Constants.SPEECIL_COLOUR;
            ImageSkew(ref _imgView) = 0.18f;
            ImageGradient(ref _imgView) = true;
        }

        private static Vector3 origPos;
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!this.isActiveAndEnabled) return;
            if (!_plvc) return;
            OnLeaderboardSet(currentDifficultyBeatmap);
            /*
            if (firstActivation)
            {
                _playerService.GetPatreonStatus((isPatron, username) =>
                {
                    Plugin.userName = username;
                    UserIsPatron = isPatron;
                    setHeaderText(headerText, isPatron);
                    if (!isPatron)
                    {
                        SettingsConfig.Instance.rainbowsuwu = false;
                        SettingsConfig.Instance.nameHeaderToggle = false;
                    }
                    uwuToggle.interactable = isPatron;
                    nameToggle.interactable = isPatron;
                    uwuToggle.Value = false;
                    nameToggle.Value = false;
                });
            }
            */
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0, 0, 0, 0);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_plvc) return;
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
            if (!_plvc.isActivated) return;
            page = 0;
            parserParams.EmitEvent("hideInfoModal");
            scoreInfoModal.parserParams.EmitEvent("hideScoreInfo");
            parserParams.EmitEvent("hideSettings");
        }

        void RichMyText(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                cell.showSeparator = true;
                var nameText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");
                var rankText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText");
                var scoreText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText");
                nameText.richText = true;
                nameText.gameObject.SetActive(false);
                rankText.gameObject.SetActive(false);
                scoreText.gameObject.SetActive(false);

                if (cell.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
                {
                    _tweeningService.FadeText(nameText, true, 0.4f);
                    _tweeningService.FadeText(rankText, true, 0.4f);
                    _tweeningService.FadeText(scoreText, true, 0.4f);
                }
            }
        }

        private void FuckOffButtons() => holders.ForEach(holder => holder.infoButton.gameObject.SetActive(false));

        public IEnumerator setcolor(Button button)
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

        public IEnumerator setToggle(ToggleSetting toggle)
        {
            var offColour = new Color(0, 0, 0, 0.5f);
            var bgImage = toggle.gameObject.transform.Find("SwitchView/BackgroundImage/KnobSlideArea").GetComponentInChildren<ImageView>();
            var bgColour = Constants.SPEECIL_COLOUR_BRIGHTER;
            while (toggle.gameObject.activeInHierarchy)
            {
                if (toggle.Value)
                {
                    bgImage.color = bgColour;
                }
                yield return null;
            }
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            currentDifficultyBeatmap = difficultyBeatmap;
            if (!this.isActivated) return;
            string mapId = difficultyBeatmap.level.levelID;
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string balls = mapType + difficulty.ToString();
            List<LLeaderboardEntry> leaderboardEntries = LeaderboardData.LeaderboardData.LoadBeatMapInfo(mapId, balls);

            totalPages = Mathf.CeilToInt((float)leaderboardEntries.Count / 10);

            try
            {
                FuckOffButtons();
                UpdatePageButtons();
                SortLeaderboardEntries(leaderboardEntries);
                leaderboardTableView.SetScores(CreateLeaderboardData(leaderboardEntries, page), -1);
                RichMyText(leaderboardTableView);

                if (leaderboardEntries.Count > 0) HandleLeaderboardEntriesExistence(leaderboardEntries);
                else HandleNoLeaderboardEntries();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
                errorText.gameObject.SetActive(true);
                errorText.text = "Error!";
                retryButton.gameObject.SetActive(true);
            }
        }

        private void SortLeaderboardEntries(List<LLeaderboardEntry> leaderboardEntries)
        {
            if (sortMethod == 0)
            {
                if (leaderboardEntries.Count <= 0) return;
                LLeaderboardEntry recent = leaderboardEntries[leaderboardEntries.Count - 1];
                if (!Ascending) leaderboardEntries.Reverse();
                long unixTimestamp;
                string formattedDate = "Error";
                if (long.TryParse(recent.datePlayed, out unixTimestamp))
                {
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                    DateTime datePlayed = dateTimeOffset.LocalDateTime;
                    formattedDate = SettingsConfig.Instance.useRelativeTime
                        ? TimeUtils.GetRelativeTimeString(recent.datePlayed)
                        : datePlayed.ToString(config.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt");
                    _panelView.lastPlayed.text = "Last Played: " + formattedDate;
                }

            }
            else if (sortMethod == 1)
            {
                if (Ascending) leaderboardEntries.Sort((first, second) => first.acc.CompareTo(second.acc));
                else leaderboardEntries.Sort((first, second) => second.acc.CompareTo(first.acc));
                if (leaderboardEntries.Count <= 0) return;
                _panelView.lastPlayed.text = (Ascending ? "Lowest Acc : " : "Highest Acc : ") + leaderboardEntries[0].acc.ToString("F2") + "%";
            }


        }

        private void HandleLeaderboardEntriesExistence(List<LLeaderboardEntry> leaderboardEntries)
        {
            if (errorText.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
            {
                _tweeningService.FadeText(errorText, false, 0.4f);
            }

            _panelView.totalScores.text = "Total Scores: " + leaderboardEntries.Count;
            _panelView.lastPlayed.gameObject.SetActive(true);
            _panelView.totalScores.gameObject.SetActive(true);

            if (_panelView.lastPlayed.gameObject.activeSelf && _panelView.totalScores.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
            {
                _tweeningService.FadeText(_panelView.lastPlayed, true, 0.4f);
                _tweeningService.FadeText(_panelView.totalScores, true, 0.4f);
            }
            int startIndex = page * 10;
            int remainingEntries = leaderboardEntries.Count - startIndex;
            int maxEntriesPerPage = Mathf.Min(remainingEntries, 10);

            for (int i = 0; i < maxEntriesPerPage; i++)
            {
                holders[i].infoButton.gameObject.SetActive(true);
            }
        }

        private void HandleNoLeaderboardEntries()
        {
            errorText.text = "No scores on this map!";

            if (_panelView.lastPlayed.gameObject.activeSelf && _panelView.totalScores.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
            {
                _tweeningService.FadeText(_panelView.lastPlayed, false, 0.4f);
                _tweeningService.FadeText(_panelView.totalScores, false, 0.4f);
            }
            if (!errorText.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
            {
                _tweeningService.FadeText(errorText, true, 0.4f);
            }
            if (leaderboardTableView.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
            {
                FadeOut(leaderboardTableView);
            }
        }

        void FadeOut(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                if (!(cell.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)) continue;
                _tweeningService.FadeText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText"), false, 0.4f);
                _tweeningService.FadeText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText"), false, 0.4f);
                _tweeningService.FadeText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText"), false, 0.4f);
            }
        }

        public List<ScoreData> CreateLeaderboardData(List<LLeaderboardEntry> leaderboard, int page)
        {
            List<ScoreData> tableData = new List<ScoreData>();
            int pageIndex = page * 10;
            for (int i = pageIndex; i < leaderboard.Count && i < pageIndex + 10; i++)
            {
                buttonEntryArray[i % 10] = leaderboard[i];
                int score = leaderboard[i].score;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], i + 1, score));
            }
            return tableData;
        }

        public ScoreData CreateLeaderboardEntryData(LLeaderboardEntry entry, int rank, int score)
        {
            string datePlayedString = entry.datePlayed;
            long unixTimestamp;
            string formattedDate = "Error";
            if (long.TryParse(datePlayedString, out unixTimestamp))
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
                DateTime datePlayed = dateTimeOffset.LocalDateTime;

                if (SettingsConfig.Instance.useRelativeTime)
                {
                    TimeSpan relativeTime = TimeUtils.GetRelativeTime(datePlayedString);

                    if (entry.didFail)
                    {
                        formattedDate = string.Format("<color=#f09f48>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));
                    }
                    else
                    {
                        formattedDate = string.Format("<color=#28b077>{0}</color></size>", TimeUtils.GetRelativeTimeString(relativeTime));
                    }
                }
                else
                {
                    if (entry.didFail)
                    {
                        formattedDate = string.Format("<color=#f09f48>{0}</color></size>", datePlayed.ToString(config.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt"));
                    }
                    else
                    {
                        formattedDate = string.Format("<color=#28b077>{0}</color></size>", datePlayed.ToString(config.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt"));
                    }
                }
            }

            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            score = entry.score;
            string formattedCombo = "";
            if (entry.fullCombo) formattedCombo = " -<color=green> FC </color>";
            else formattedCombo = string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);

            string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

            string result;

            string size = SettingsConfig.Instance.useRelativeTime ? "120%" : "90%";

            result = $"<size={size}>" + formattedDate + formattedAcc + formattedCombo + formattedMods + "</size>";
            return new ScoreData(score, result, rank, false);
        }


        internal class ButtonHolder
        {
            private int index;
            private Action<LLeaderboardEntry> onClick;

            public ButtonHolder(int index, Action<LLeaderboardEntry> endmylife)
            {
                this.index = index;
                onClick = endmylife;
            }

            [UIComponent("infoButton")]
            public Button infoButton;

            [UIAction("infoClick")]
            private void infoClick() => onClick?.Invoke(buttonEntryArray[index]);
        }
    }
}
