using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using LeaderboardCore.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LeaderboardTableView;

namespace LocalLeaderboard.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"../Views/LeaderboardView.bsml")]
    [ViewDefinition("LocalLeaderboard.UI.Views.LeaderboardView.bsml")]
    internal class LeaderboardView : BSMLAutomaticViewController, INotifyLeaderboardSet
    {
        private bool Ascending = true;
        private List<ScoreData> scores = new List<ScoreData>();

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

        [UIComponent("segmentedControl")]
        private IconSegmentedControl segmentedControl;

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
            if (sorter.gameObject.active)
            {
                try
                {
                    StartCoroutine(RotateSorter());
                }
                catch(Exception ex)
                {

                }
            }
        }

        [UIAction("Discord")]
        private void Discord()
        {
            UnityEngine.Application.OpenURL("https://discord.gg/2KyykDXpBk");
        }

        [UIAction("Patreon")]
        private void Patreon()
        {
            UnityEngine.Application.OpenURL("https://patreon.com/speecil");
        }

        [UIAction("Website")]
        private void Website()
        {
            UnityEngine.Application.OpenURL("https://speecil.dev");
        }

        public void showModal()
        {
            parserParams.EmitEvent("showInfoModal");
            try
            {
                StopCoroutine(setcolor(websiteButton));
                StopCoroutine(setcolor(discordButton));
                StopCoroutine(setcolor(patreonButton));
            }
            catch(Exception ex)
            {

            }
            try
            {
                StartCoroutine(setcolor(websiteButton));
                StartCoroutine(setcolor(discordButton));
                StartCoroutine(setcolor(patreonButton));
            }
            catch (Exception ex) { }
        }

        [UIAction("Retry")]
        private void Retry()
        {
            OnLeaderboardSet(currentDifficultyBeatmap);
        }

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
                    BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.clock.png"), "Date / Time"),
                new IconSegmentedControl.DataItem(
                    BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.score.png"), "Highscore")
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

        private static Vector3 origPos;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                page = 0;
            }
            OnLeaderboardSet(currentDifficultyBeatmap);
            Thread ac = new Thread(() => Activate(firstActivation));
            ac.Start();
        }

        public static Transform FindChildRecursive(Transform parent, string childName)
        {
            if (parent.name == childName)
            {
                return parent;
            }

            foreach (Transform child in parent)
            {
                Transform result = FindChildRecursive(child, childName);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void Activate(bool first)
        {
            Thread.Sleep(1);
            var screen = this.gameObject.GetComponentInParent<HMUI.Screen>();
            var leaderboardViewController = FindChildRecursive(screen.transform, "PlatformLeaderboardViewController");
            var header = FindChildRecursive(leaderboardViewController, "HeaderPanel").gameObject;
            if (first) origPos = header.transform.localPosition;
            header.transform.localPosition = new Vector3(-999, -999, -999);
            Thread.CurrentThread.Join();
        }
        PanelView Panel;

        private void Deactivate()
        {
            Thread.Sleep(1);
            parserParams.EmitEvent("hideInfoModal");
            var screen = this.gameObject.GetComponentInParent<HMUI.Screen>();
            var leaderboardViewController = FindChildRecursive(screen.transform, "PlatformLeaderboardViewController");
            var header = FindChildRecursive(leaderboardViewController, "HeaderPanel").gameObject;
            header.transform.localPosition = origPos;
            Thread.CurrentThread.Join();
        }


        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            page = 0;
            Thread deac = new Thread(() => Deactivate());
            deac.Start();
        }

        void RichMyText(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                cell.showSeparator = true;
                cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText").richText = true;
                cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText").gameObject.SetActive(false);
                cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText").gameObject.SetActive(false);
                cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText").gameObject.SetActive(false);

                if (cell.gameObject.active && leaderboardTransform.gameObject.active)
                {
                    try
                    {
                        StartCoroutine(FadeInText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText")));
                        StartCoroutine(FadeInText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText")));
                        StartCoroutine(FadeInText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText")));
                    }
                    catch (Exception ex) { }
                }
            }
        }

        private IEnumerator setcolor(Button button)
        {
            var bgImage = button.transform.FindChild("BG").gameObject.GetComponent<ImageView>();
            var bgBorder = button.transform.FindChild("Border").gameObject.GetComponent<ImageView>();
            var bgOutline = button.transform.FindChild("OutlineWrapper/Outline").gameObject.GetComponent<ImageView>();
            var buttonText = button.transform.FindChild("Content/Text").gameObject.GetComponent<TextMeshProUGUI>();

            if(bgImage == null || bgBorder.isActiveAndEnabled == false)
            {
                yield break;
            }

            while (infoModal.GetField<bool, ModalView>("_isShown"))
            {
                bgImage.color0 = new Color(0.156f, 0.69f, 0.46666f, 1);
                bgImage.color1 = new Color(0.156f, 0.69f, 0.46666f, 1);

                bgBorder.color0 = new Color(0.156f, 0.69f, 0.46666f, 1);
                bgBorder.color1 = new Color(0.156f, 0.69f, 0.46666f, 1);
                bgBorder.color = new Color(0.156f, 0.69f, 0.46666f, 1);

                bgOutline.color = new Color(0.156f, 0.69f, 0.46666f, 1);
                bgOutline.color0 = new Color(0.156f, 0.69f, 0.46666f, 1);
                bgOutline.color1 = new Color(0.156f, 0.69f, 0.46666f, 1);

                buttonText.color = new Color(1f, 1f, 1f, 1);
                yield return null;
            }
        }

        bool isAnimating;

        private IEnumerator RotateSorter()
        {
            if (!isAnimating)
            {
                isAnimating = true;
                const float duration = 0.10f;
                float startTime = Time.time;
                float startRotation = sorter.GetComponentInChildren<HMUI.ImageView>().transform.rotation.eulerAngles.z;
                float endRotation = startRotation + 180f;

                while (Time.time < startTime + duration)
                {
                    float t = (Time.time - startTime) / duration;
                    float currentRotation = Mathf.Lerp(startRotation, endRotation, t);
                    sorter.GetComponentInChildren<HMUI.ImageView>().transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
                    yield return null;
                }

                sorter.GetComponentInChildren<HMUI.ImageView>().transform.rotation = Quaternion.Euler(0f, 0f, endRotation);
                Ascending = !Ascending;
                OnLeaderboardSet(currentDifficultyBeatmap);
                isAnimating = false;
            }
        }


        private IEnumerator FadeInText(TextMeshProUGUI text)
        {
            yield return null;
            if (text == null)
            {
                yield break;
            }
            float duration = 0.4f;
            float elapsedTime = 0f;
            text.gameObject.SetActive(true);
            while (elapsedTime < duration)
            {
                if (!text.gameObject.active)
                {
                    yield break;
                }
                text.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            text.alpha = 1f;
        }

        private IEnumerator FadeOutText(TextMeshProUGUI text)
        {
            yield return null;
            if (text == null)
            {
                yield break;
            }
            float duration = 0.4f;
            float elapsedTime = 0f;

            if (!text.gameObject.active)
            {
                yield break;
            }

            while (elapsedTime < duration)
            {
                if (!text.gameObject.active)
                {
                    yield break;
                }

                text.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            text.alpha = 0f;
            text.gameObject.SetActive(false);
        }

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            currentDifficultyBeatmap = difficultyBeatmap;
            if (!this.isActivated) return;
            Panel = UnityEngine.Resources.FindObjectsOfTypeAll<PanelView>().FirstOrDefault();

            string mapId = difficultyBeatmap.level.levelID;
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string balls = mapType + difficulty.ToString();
            List<LeaderboardData.LeaderboardData.LeaderboardEntry> leaderboardEntries = LeaderboardData.LeaderboardData.LoadBeatMapInfo(mapId, balls);
            totalPages = Mathf.CeilToInt((float)leaderboardEntries.Count / 10);

            try
            {
                UpdatePageButtons();

                if (Ascending)
                {
                    SortLeaderboardEntriesAscending(leaderboardEntries);
                }
                else
                {
                    SortLeaderboardEntriesDescending(leaderboardEntries);
                }

                leaderboardTableView.SetScores(CreateLeaderboardData(leaderboardEntries, page), -1);
                RichMyText(leaderboardTableView);

                if (leaderboardEntries.Count > 0)
                {
                    HandleLeaderboardEntriesExistence(leaderboardEntries);
                }
                else
                {
                    HandleNoLeaderboardEntries();
                }
            }
            catch
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Error!";
                retryButton.gameObject.SetActive(true);
            }
        }

        private void SortLeaderboardEntriesAscending(List<LeaderboardData.LeaderboardData.LeaderboardEntry> leaderboardEntries)
        {
            if (sortMethod == 0)
            {
                if (leaderboardEntries.Count > 0)
                {
                    LeaderboardData.LeaderboardData.LeaderboardEntry recent = leaderboardEntries[leaderboardEntries.Count - 1];
                    Panel.lastPlayed.text = "Last Played: " + recent.datePlayed;
                }
            }
            else if (sortMethod == 1)
            {
                leaderboardEntries.Sort((first, second) => first.acc.CompareTo(second.acc));

                if (leaderboardEntries.Count > 0)
                {
                    Panel.lastPlayed.text = "Highest Acc : " + leaderboardEntries[leaderboardEntries.Count - 1].acc.ToString("F2") + "%";
                }
            }
        }

        private void SortLeaderboardEntriesDescending(List<LeaderboardData.LeaderboardData.LeaderboardEntry> leaderboardEntries)
        {
            if (sortMethod == 0)
            {
                if (leaderboardEntries.Count > 0)
                {
                    LeaderboardData.LeaderboardData.LeaderboardEntry recent = leaderboardEntries[leaderboardEntries.Count - 1];
                    leaderboardEntries.Reverse();
                    Panel.lastPlayed.text = "Last Played: " + recent.datePlayed;
                }
            }
            else if (sortMethod == 1)
            {
                leaderboardEntries.Sort((first, second) => second.acc.CompareTo(first.acc));
                if (leaderboardEntries.Count > 0)
                {
                    Panel.lastPlayed.text = "Highest Acc : " + leaderboardEntries[0].acc.ToString("F2") + "%";
                }
            }
        }

        private void HandleLeaderboardEntriesExistence(List<LeaderboardData.LeaderboardData.LeaderboardEntry> leaderboardEntries)
        {
            try
            {
                if (errorText.gameObject.active && leaderboardTransform.gameObject.active)
                {
                    StartCoroutine(FadeOutText(errorText));
                }

                Panel.totalScores.text = "Total Scores: " + leaderboardEntries.Count;
                Panel.lastPlayed.gameObject.SetActive(true);
                Panel.totalScores.gameObject.SetActive(true);

                if (Panel.lastPlayed.gameObject.active && Panel.totalScores.gameObject.active && leaderboardTransform.gameObject.active)
                {
                    StartCoroutine(FadeInText(Panel.lastPlayed));
                    StartCoroutine(FadeInText(Panel.totalScores));
                }
            }
            catch(Exception e) { }
        }

        private void HandleNoLeaderboardEntries()
        {
            errorText.text = "No scores on this map!";

            try
            {
                if (Panel.lastPlayed.gameObject.active && Panel.totalScores.gameObject.active && leaderboardTransform.gameObject.active)
                {
                    StartCoroutine(FadeOutText(Panel.lastPlayed));
                    StartCoroutine(FadeOutText(Panel.totalScores));
                }
                if (!errorText.gameObject.active && leaderboardTransform.gameObject.active)
                {
                    StartCoroutine(FadeInText(errorText));
                }
                if (leaderboardTableView.gameObject.active && leaderboardTransform.gameObject.active)
                {
                    FadeOut(leaderboardTableView);
                }
            }
            catch(Exception e)
            {

            }

        }


        void FadeOut(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                if (cell.gameObject.active && leaderboardTransform.gameObject.active)
                {
                    StartCoroutine(FadeOutText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText")));
                    StartCoroutine(FadeOutText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText")));
                    StartCoroutine(FadeOutText(cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText")));
                }

            }
        }


        public List<LeaderboardTableView.ScoreData> CreateLeaderboardData(List<LeaderboardData.LeaderboardData.LeaderboardEntry> leaderboard, int page)
        {
            List<LeaderboardTableView.ScoreData> tableData = new List<LeaderboardTableView.ScoreData>();
            int pageIndex = page * 10;
            for (int i = pageIndex; i < leaderboard.Count && i < pageIndex + 10; i++)
            {
                int score = leaderboard[i].score;
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], i + 1, score));
            }
            return tableData;
        }

        public LeaderboardTableView.ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardData.LeaderboardEntry entry, int rank, int score)
        {
            string formattedDate = string.Format("<color=#28b077>{0}</color></size>", entry.datePlayed);
            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            score = entry.score;
            string formattedCombo = "";
            if (entry.fullCombo)
            {
                formattedCombo = " -<color=green> FC </color>";
            }
            else
            {
                formattedCombo = string.Format(" - <color=red>x{0} </color>", entry.badCutCount + entry.missCount);
            }

            string formattedMods = string.Format("   {0}</size>", entry.mods);

            string result = "<size=85%>" + formattedDate + formattedAcc + formattedCombo + formattedMods + "</size>";

            return new LeaderboardTableView.ScoreData(score, result, rank, false);
        }
    }
}