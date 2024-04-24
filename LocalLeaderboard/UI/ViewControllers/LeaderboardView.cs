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
using LocalLeaderboard.AffinityPatches;
using LocalLeaderboard.Services;
using LocalLeaderboard.Utils;
using SiraUtil.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        [Inject] private readonly PanelView _panelView;
        [Inject] private readonly PlatformLeaderboardViewController _plvc;
        [Inject] private readonly TweeningService _tweeningService;
        [Inject] private readonly PlayerService _playerService;
        [Inject] private readonly SiraLog _log;

        //[Inject] private PlayerService _playerService;

        [InjectOptional] private readonly List<IExternalDataService> _externalDataProviders;

        private bool Ascending = true;
        public static LLeaderboardEntry[] buttonEntryArray = new LLeaderboardEntry[10];
        public bool UserIsPatron = false;

        readonly SettingsConfig config = SettingsConfig.Instance;

        public BeatmapKey currentBeatmapKey;

        [UIComponent("leaderboardTableView")]
        private readonly LeaderboardTableView leaderboardTableView = null;

        [UIComponent("leaderboardTableView")]
        private readonly Transform leaderboardTransform = null;

        [UIComponent("errorText")]
        private readonly TextMeshProUGUI errorText;

        [UIComponent("headerText")]
        private readonly TextMeshProUGUI headerText;

        [UIComponent("up_button")]
        private readonly Button up_button;

        [UIComponent("down_button")]
        private readonly Button down_button;

        [UIComponent("sorter")]
        private readonly ImageView sorter;

        [UIComponent("myHeader")]
        private readonly Backgroundable myHeader;

        [UIComponent("discordButton")]
        private readonly Button discordButton;

        [UIComponent("patreonButton")]
        private readonly Button patreonButton;

        [UIComponent("websiteButton")]
        private readonly Button websiteButton;

        [UIComponent("retryButton")]
        private readonly Button retryButton;

        [UIComponent("uwuToggle")]
        private readonly ToggleSetting uwuToggle;

        [UIComponent("nameToggle")]
        private readonly ToggleSetting nameToggle;

        [UIComponent("americanToggle")]
        private readonly ToggleSetting americanToggle;

        [UIComponent("relativeToggle")]
        private readonly ToggleSetting relativeToggle;

        [UIComponent("infoModal")]
        private readonly ModalView infoModal;

        [UIComponent("settingsModal")]
        private readonly ModalView settingsModal;

        [UIValue("EntryHolders")]
        [Inject] private readonly List<EntryHolder> holders;

        [UIComponent("scoreInfoModal")]
        [Inject] private readonly ScoreInfoModal scoreInfoModal;

        [UIParams]
        readonly BSMLParserParams parserParams;


        public int page = 0;
        public int totalPages;
        public int sortMethod;

        [UIAction("OnPageUp")]
        private void OnPageUp() => UpdatePageChanged(-1);

        [UIAction("OnPageDown")]
        private void OnPageDown() => UpdatePageChanged(1);

        private void UpdatePageButtons()
        {
            up_button.interactable = page > 0;
            down_button.interactable = page < totalPages - 1;
        }

        private void UpdatePageChanged(int inc)
        {
            page = Mathf.Clamp(page + inc, 0, totalPages - 1);
            OnLeaderboardSet(currentBeatmapKey);
        }

        [UIAction("changeSort")]
        private void changeSort()
        {
            sorter.gameObject.SetActive(true);
            if (!sorter.gameObject.activeSelf) return;

            _tweeningService.RotateTransform(sorter.GetComponentInChildren<ImageView>().transform, 180f, 0.1f, () =>
            {
                Ascending = !Ascending;
                UnityMainThreadTaskScheduler.Factory.StartNew(() => OnLeaderboardSet(currentBeatmapKey));
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
                OnLeaderboardSet(currentBeatmapKey);
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
                _panelView.toggleRainbow(value);
            }
        }

        [UIValue("nameheaderoption")]
        private bool nameheaderoption
        {
            get
            {
                if (!UserIsPatron) return false;
                setHeaderText(headerText, UserIsPatron);
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
                OnLeaderboardSet(currentBeatmapKey);
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
        private void Retry() => OnLeaderboardSet(currentBeatmapKey);

        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index)
        {
            sortMethod = index;
            OnLeaderboardSet(currentBeatmapKey);
        }

        [UIValue("leaderboardIcons")]
        private List<IconSegmentedControl.DataItem> leaderboardIcons
        {
            get
            {
                return new List<IconSegmentedControl.DataItem>()
                {
                new(
                    Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.clock.png"), "Date / Time"),
                new(
                    Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.score.png"), "Accuracy")
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
            Transform loadingContainer = _loadingControl.transform.Find("LoadingContainer");
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
            if (!isActiveAndEnabled) return;
            if (!_plvc) return;
            if (!_panelView.isActiveAndEnabled) return;
            if (firstActivation)
            {

                _playerService.GetPatreonStatus((isPatron, username) =>
                {
                    Plugin.userName = username;
                    UserIsPatron = isPatron;
                    if (!isPatron)
                    {
                        SettingsConfig.Instance.rainbowsuwu = false;
                        SettingsConfig.Instance.nameHeaderToggle = false;
                    }
                    uwuToggle.interactable = isPatron;
                    nameToggle.interactable = isPatron;
                });
            }
            _plvc.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0, 0, 0, 0);
            OnLeaderboardSet(currentBeatmapKey);
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
                TextMeshProUGUI nameText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText");
                TextMeshProUGUI rankText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText");
                TextMeshProUGUI scoreText = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText");
                nameText.richText = true;
                nameText.gameObject.SetActive(false);
                rankText.gameObject.SetActive(false);
                scoreText.gameObject.SetActive(false);


                ImageView seperator = cell.GetField<Image, LeaderboardTableCell>("_separatorImage") as ImageView;

                cell.interactable = true;
                EntryHolder EntryHolder = holders[cell.idx];

                CellClicker clicky;

                if (cell.gameObject.GetComponent<CellClicker>() == null)
                {
                    clicky = cell.gameObject.AddComponent<CellClicker>();
                }
                else
                {
                    clicky = cell.gameObject.GetComponent<CellClicker>();
                }

                clicky.onClick = EntryHolder.infoClick;
                clicky.index = cell.idx;
                clicky.seperator = seperator;

                //clicky.Reset();

                if (cell.gameObject.activeSelf && leaderboardTransform.gameObject.activeSelf)
                {
                    _tweeningService.FadeText(nameText, true, 0.4f);
                    _tweeningService.FadeText(rankText, true, 0.4f);
                    _tweeningService.FadeText(scoreText, true, 0.4f);
                }

            }
        }

        public IEnumerator setcolor(Button button)
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

        public IEnumerator setToggle(ToggleSetting toggle)
        {
            Color offColour = new(0, 0, 0, 0.5f);
            ImageView bgImage = toggle.gameObject.transform.Find("SwitchView/BackgroundImage/KnobSlideArea").GetComponentInChildren<ImageView>();
            Color bgColour = Constants.SPEECIL_COLOUR_BRIGHTER;
            while (toggle.gameObject.activeInHierarchy)
            {
                if (toggle.Value)
                {
                    bgImage.color = bgColour;
                }
                yield return null;
            }
        }

        private CancellationTokenSource _refreshCTS;

        public async void OnLeaderboardSet(BeatmapKey difficultyBeatmap)
        {
            currentBeatmapKey = difficultyBeatmap;
            _refreshCTS?.Cancel();
            _refreshCTS?.Dispose();
            _refreshCTS = new CancellationTokenSource();
            CancellationToken token = _refreshCTS.Token;
            if (!isActivated) return;
            string mapId = difficultyBeatmap.levelId;
            int difficulty = Results.GetOriginalIdentifier(difficultyBeatmap);
            string mapType = difficultyBeatmap.beatmapCharacteristic.serializedName;
            string balls = mapType + difficulty.ToString();
            List<LLeaderboardEntry> leaderboardEntries = LeaderboardData.LeaderboardData.LoadBeatMapInfo(mapId, balls);

            if (token.IsCancellationRequested) return;

            try
            {
                if (_externalDataProviders != null)
                {
                    foreach (IExternalDataService provider in _externalDataProviders)
                    {
                        leaderboardEntries.AddRange(await provider.GetLeaderboardEntries(difficultyBeatmap, token));
                    }
                    if (token.IsCancellationRequested) return;
                }
            }
            catch (OperationCanceledException e) when (e.CancellationToken == token)
            {
                return;
            }
            catch (Exception e)
            {
                _log.Error($"Failed to get leaderboard entries from external data providers: {e}");
                return;
            }

            try
            {
                leaderboardEntries = DedupeAndSortLeaderboardEntries(leaderboardEntries);
                if (token.IsCancellationRequested) return; // these are some relatively long functions
                totalPages = Mathf.CeilToInt((float)leaderboardEntries.Count / 10);
                UpdatePageButtons();
                leaderboardTableView.SetScores(CreateLeaderboardData(leaderboardEntries, page), -1);
                if (token.IsCancellationRequested) return;
                RichMyText(leaderboardTableView);
                if (token.IsCancellationRequested) return;

                if (leaderboardEntries.Count > 0) HandleLeaderboardEntriesExistence(leaderboardEntries);
                else HandleNoLeaderboardEntries();
            }
            catch (Exception ex)
            {
                _log.Error(ex);
                errorText.gameObject.SetActive(true);
                errorText.text = "Error!";
                retryButton.gameObject.SetActive(true);
            }
        }

        private List<LLeaderboardEntry> DedupeAndSortLeaderboardEntries(List<LLeaderboardEntry> leaderboardEntries)
        {
            if (leaderboardEntries.Count <= 0) return leaderboardEntries;

            List<LLeaderboardEntry> intermediate = new(leaderboardEntries.Count);

            LLeaderboardEntry? prev = null;
            foreach (LLeaderboardEntry entry in leaderboardEntries.OrderBy(entry => entry, new LeaderboardEntryDatePlayedComparer()))
            {
                if (prev == null)
                {
                    intermediate.Add(entry);
                }
                else
                {
                    LLeaderboardEntry previous = prev.Value;
                    if (entry.IsSamePlay(previous))
                    {
                        //TODO if we have multiple duplicated external entries, try aggregate the data.
                        // it is fine for now, as we only have one external provider
                        if (!entry.isExternal)
                        {
                            // replace the duplicated previous with this internal one
                            intermediate[intermediate.Count - 1] = entry;
                        }
                    }
                    else
                    {
                        // not a duplicate
                        intermediate.Add(entry);
                    }
                }

                prev = entry;
            }

            List<LLeaderboardEntry> sortedResults;
            if (sortMethod == 0)
            {
                LLeaderboardEntry recent;
                if (Ascending)
                {
                    // sortedResults = intermediate.OrderBy(entry => entry, new LeaderboardEntryDatePlayedComparer()).ToList();
                    sortedResults = new List<LLeaderboardEntry>(intermediate);// already sorted
                    recent = sortedResults[sortedResults.Count - 1];
                }
                else
                {
                    sortedResults = intermediate.OrderByDescending(entry => entry, new LeaderboardEntryDatePlayedComparer()).ToList();
                    recent = sortedResults[0];
                }

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
                if (Ascending)
                {
                    sortedResults = intermediate.OrderBy(entry => entry, new LeaderboardEntryAccComparer()).ToList();
                }
                else
                {
                    sortedResults = intermediate.OrderByDescending(entry => entry, new LeaderboardEntryAccComparer()).ToList();
                }

                _panelView.lastPlayed.text = (Ascending ? "Lowest Acc : " : "Highest Acc : ") + sortedResults[0].acc.ToString("F2") + "%";
            }
            else
            {
                sortedResults = intermediate.ToList();
            }

            return sortedResults;
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
            List<ScoreData> tableData = new();
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
                    string colorCode = entry.isExternal ? "#a94cf5" : (entry.didFail ? "#f09f48" : "#28b077");
                    formattedDate = string.Format("<color={0}>{1}</color></size>", colorCode, TimeUtils.GetRelativeTimeString(relativeTime));
                }
                else
                {
                    string colorCode = entry.didFail ? "#f09f48" : "#28b077";
                    string dateFormat = config.BurgerDate ? "MM/dd/yyyy hh:mm tt" : "dd/MM/yyyy hh:mm tt";
                    formattedDate = string.Format("<color={0}>{1}</color></size>", colorCode, datePlayed.ToString(dateFormat));
                }

            }

            string formattedAcc = string.Format(" - (<color=#ffd42a>{0:0.00}%</color>)", entry.acc);
            score = entry.score;
            string formattedCombo = "";
            if (entry.fullCombo) formattedCombo = " -<color=green> FC </color>";
            else formattedCombo = entry.badCutCount + entry.missCount < 0 ? "" : $" - <color=red>x{entry.badCutCount + entry.missCount} </color>";

            string formattedMods = string.Format("  <size=60%>{0}</size>", entry.mods);

            string result;

            string size = SettingsConfig.Instance.useRelativeTime ? "120%" : "90%";

            result = $"<size={size}>" + formattedDate + formattedAcc + formattedCombo + formattedMods + "</size>";
            return new ScoreData(score, result, rank, false);
        }


        internal class EntryHolder
        {
            private readonly Action<LLeaderboardEntry> onClick;

            public EntryHolder(Action<LLeaderboardEntry> endmylife)
            {
                onClick = endmylife;
            }

            //[UIComponent("infoButton")]
            //public Button infoButton;

            //[UIAction("infoClick")]
            internal void infoClick(int index) => onClick?.Invoke(buttonEntryArray[index]);
        }

        public class CellClicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
        {
            public Action<int> onClick;
            public int index;
            public ImageView seperator;
            private Vector3 originalScale;
            private bool isScaled = false;

            private Color origColour = new(1, 1, 1, 1);
            private Color origColour0 = new(1, 1, 1, 0.2509804f);
            private Color origColour1 = new(1, 1, 1, 0);

            private void Start()
            {
                originalScale = seperator.transform.localScale;
            }

            public void Reset()
            {
                seperator.color = origColour;
                seperator.color0 = origColour0;
                seperator.color1 = origColour1;
                seperator.transform.localScale = originalScale;
                isScaled = false;
            }


            public void OnPointerClick(PointerEventData data)
            {
                BeatSaberUI.BasicUIAudioManager.GetType().GetRuntimeMethod("HandleButtonClickEvent", new Type[0]);
                onClick(index);
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (!isScaled)
                {
                    seperator.transform.localScale = originalScale * 1.8f;
                    isScaled = true;
                }

                Color targetColor = Color.white;
                Color targetColor0 = Color.white;
                Color targetColor1 = new(1, 1, 1, 0);

                float lerpDuration = 0.15f;

                StopAllCoroutines();
                StartCoroutine(LerpColors(seperator, seperator.color, targetColor, seperator.color0, targetColor0, seperator.color1, targetColor1, lerpDuration));
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (isScaled)
                {
                    seperator.transform.localScale = originalScale;
                    isScaled = false;
                }

                float lerpDuration = 0.05f;

                StopAllCoroutines();
                StartCoroutine(LerpColors(seperator, seperator.color, origColour, seperator.color0, origColour0, seperator.color1, origColour1, lerpDuration));
            }


            private IEnumerator LerpColors(ImageView target, Color startColor, Color endColor, Color startColor0, Color endColor0, Color startColor1, Color endColor1, float duration)
            {
                float elapsedTime = 0f;
                while (elapsedTime < duration)
                {
                    float t = elapsedTime / duration;
                    target.color = Color.Lerp(startColor, endColor, t);
                    target.color0 = Color.Lerp(startColor0, endColor0, t);
                    target.color1 = Color.Lerp(startColor1, endColor1, t);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                target.color = endColor;
                target.color0 = endColor0;
                target.color1 = endColor1;
            }

            private void OnDestroy()
            {
                StopAllCoroutines();
                onClick = null;
                seperator.color = origColour;
                seperator.color0 = origColour0;
                seperator.color1 = origColour1;
            }
        }
    }
}
