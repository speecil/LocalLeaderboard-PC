using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using IPA.Utilities.Async;
using LeaderboardCore.Interfaces;
using LocalLeaderboard.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static LeaderboardTableView;
using static UnityEngine.EventSystems.EventTrigger;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;

namespace LocalLeaderboard.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"../Views/LeaderboardView.bsml")]
    [ViewDefinition("LocalLeaderboard.UI.Views.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet
    {
        private bool Ascending = true;
        private List<ScoreData> scores = new List<ScoreData>();
        private PanelView _panelView;
        private PlatformLeaderboardViewController _plvc;
        private TweeningService _tweeningService;


        private static LLeaderboardEntry button1Entry;
        private static LLeaderboardEntry button2Entry;
        private static LLeaderboardEntry button3Entry;
        private static LLeaderboardEntry button4Entry;
        private static LLeaderboardEntry button5Entry;
        private static LLeaderboardEntry button6Entry;
        private static LLeaderboardEntry button7Entry;
        private static LLeaderboardEntry button8Entry;
        private static LLeaderboardEntry button9Entry;
        private static LLeaderboardEntry button10Entry;

        private static LLeaderboardEntry[] buttonEntryArray = new LLeaderboardEntry[] { button1Entry, button2Entry, button3Entry, button4Entry, button5Entry, button6Entry, button7Entry, button8Entry, button9Entry, button10Entry };


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


        [UIComponent("button1")]
        private Button button1;

        [UIComponent("button2")]
        private Button button2;

        [UIComponent("button3")]
        private Button button3;

        [UIComponent("button4")]
        private Button button4;

        [UIComponent("button5")]
        private Button button5;

        [UIComponent("button6")]
        private Button button6;

        [UIComponent("button7")]
        private Button button7;

        [UIComponent("button8")]
        private Button button8;

        [UIComponent("button9")]
        private Button button9;

        [UIComponent("button10")]
        private Button button10;

        void setScoreModalText(int pos)
        {
            dateScoreText.text = $"Date set: <size=6><color=#28b077>{buttonEntryArray[pos].datePlayed}</color></size>";
            accScoreText.text = $"Accuracy: <size=6><color=#ffd42a>{buttonEntryArray[pos].acc.ToString("F2")}%</color></size>";
            scoreScoreText.text = $"Score: <size=6>{buttonEntryArray[pos].score}</size>";

            if (buttonEntryArray[pos].fullCombo) fcScoreText.text = "<color=green>Full Combo</color>";
            else fcScoreText.text = string.Format("Mistakes: <size=6><color=red>{0}</color></size>", buttonEntryArray[pos].badCutCount + buttonEntryArray[pos].missCount);

            failScoreText.gameObject.SetActive(buttonEntryArray[pos].didFail);
            avgHitscoreScoreText.text = $"Average Hitscore: <size=6>{buttonEntryArray[pos].averageHitscore}</size>";
            maxComboScoreText.text = $"Max Combo: <size=6>{buttonEntryArray[pos].maxCombo}</size>";
            parserParams.EmitEvent("showScoreInfo");
        }

        [UIAction("button1Click")]
        private void button1Click()
        {
            setScoreModalText(0);
        }

        [UIAction("button2Click")]
        private void button2Click()
        {
            setScoreModalText(1);
        }


        [UIAction("button3Click")]
        private void button3Click()
        {
            setScoreModalText(2);
        }

        [UIAction("button4Click")]
        private void button4Click()
        {
            setScoreModalText(3);
        }

        [UIAction("button5Click")]
        private void button5Click()
        {
            setScoreModalText(4);
        }

        [UIAction("button6Click")]
        private void button6Click()
        {
            setScoreModalText(5);
        }

        [UIAction("button7Click")]
        private void button7Click()
        {
            setScoreModalText(6);
        }

        [UIAction("button8Click")]
        private void button8Click()
        {
            setScoreModalText(7);
        }

        [UIAction("button9Click")]
        private void button9Click()
        {
            setScoreModalText(8);
        }

        [UIAction("button10Click")]
        private void button10Click()
        {
            setScoreModalText(9);
        }

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


        [UIComponent("infoModal")]
        private ModalView infoModal;

        [UIParams]
        BSMLParserParams parserParams = null;

        public int page = 0;
        public int totalPages;
        public int sortMethod;

        [UIAction("OnPageUp")]
        private void OnPageUp()
        {
            if (page > 0)
            {
                page--;
                UpdatePageButtons();
            }
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        [UIAction("OnPageDown")]
        private void OnPageDown()
        {
            if (page < totalPages - 1)
            {
                page++;
                UpdatePageButtons();
            }
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

        private void UpdatePageButtons()
        {
            up_button.interactable = (page > 0);
            down_button.interactable = (page < totalPages - 1);
        }

        [UIAction("changeSort")]
        private void changeSort()
        {
            sorter.gameObject.SetActive(true);
            if (!sorter.gameObject.activeSelf) return;

            _tweeningService.RotateTransform(sorter.GetComponentInChildren<ImageView>().transform, 180f, 0.1f,  () =>
            {
                Ascending = !Ascending;
                UnityMainThreadTaskScheduler.Factory.StartNew(() => OnLeaderboardSet(currentDifficultyBeatmap));
            });
        }

        [UIAction("Discord")]
        private void Discord() => Application.OpenURL("https://discord.gg/2KyykDXpBk");

        [UIAction("Patreon")]
        private void Patreon() => Application.OpenURL("https://patreon.com/speecil");

        [UIAction("Website")]
        private void Website() => Application.OpenURL("https://speecil.dev");
        
        public void showModal()
        {
            parserParams.EmitEvent("showInfoModal");
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
        private ImageView _imgView2;

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
            _imgView.color = new Color(0.156f, 0.69f, 0.46666f, 1);
            _imgView.color0 = new Color(0.156f, 0.69f, 0.46666f, 1);
            _imgView.color1 = new Color(0.156f, 0.69f, 0.46666f, 1);
            ImageSkew(ref _imgView) = 0.18f;
            ImageGradient(ref _imgView) = true;
        }

        [Inject]
        public void Inject(TweeningService tweeningService, PanelView panel, PlatformLeaderboardViewController plvc)
        {
            _tweeningService = tweeningService;
            _panelView = panel;
            _plvc = plvc;
        }

        private static Vector3 origPos;
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!this.isActiveAndEnabled) return;
            if (!_plvc) return;
            OnLeaderboardSet(currentDifficultyBeatmap);
            var header = _plvc.transform.Find("HeaderPanel");
            if (firstActivation)
            {
                origPos = header.transform.localPosition;
            }
            header.transform.localPosition = new Vector3(-999, -999, -999);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (!_plvc) return;
            if (!_plvc.isActivated) return;
            var header = _plvc.transform.Find("HeaderPanel");
            header.transform.localPosition = origPos;
            page = 0;
            parserParams.EmitEvent("hideInfoModal");
            parserParams.EmitEvent("hideScoreInfo");
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

        private void FuckOffButtons()
        {
            button1.gameObject.SetActive(false);
            button2.gameObject.SetActive(false);
            button3.gameObject.SetActive(false);
            button4.gameObject.SetActive(false);
            button5.gameObject.SetActive(false);
            button6.gameObject.SetActive(false);
            button7.gameObject.SetActive(false);
            button8.gameObject.SetActive(false);
            button9.gameObject.SetActive(false);
            button10.gameObject.SetActive(false);
        }

        private IEnumerator setcolor(Button button)
        {
            var bgImage = button.transform.Find("BG").gameObject.GetComponent<ImageView>();
            var bgBorder = button.transform.Find("Border").gameObject.GetComponent<ImageView>();
            var bgOutline = button.transform.Find("OutlineWrapper/Outline").gameObject.GetComponent<ImageView>();
            var buttonText = button.transform.Find("Content/Text").gameObject.GetComponent<TextMeshProUGUI>();
            var bgColour = new Color(0.156f, 0.69f, 0.46666f, 1);
            var textColour = new Color(1f, 1f, 1f, 1);
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
            catch(Exception ex)
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
                _panelView.lastPlayed.text = "Last Played: " + recent.datePlayed;    
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
            Button[] buttons = new Button[] { button1, button2, button3, button4, button5, button6, button7, button8, button9, button10 };

            for (int i = 0; i < maxEntriesPerPage; i++)
            {
                buttons[i].gameObject.SetActive(true);
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
            
            string formattedDate = string.Format("<color=#28b077>{0}</color></size>", entry.datePlayed);
            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            score = entry.score;
            string formattedCombo = "";
            if (entry.fullCombo) formattedCombo = " -<color=green> FC </color>";
            else formattedCombo = string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);
            
            string formattedMods = string.Format("   {0}</size>", entry.mods);

            string result = "<size=85%>" + formattedDate + formattedAcc + formattedCombo + formattedMods + "</size>";

            return new ScoreData(score, result, rank, false);
        }
    }
}