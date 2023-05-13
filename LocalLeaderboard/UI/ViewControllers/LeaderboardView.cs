using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using LeaderboardCore.Interfaces;
using Microsoft.SqlServer.Server;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private List<ScoreData> scores = new List<ScoreData>();

        public IDifficultyBeatmap currentDifficultyBeatmap;

        [UIComponent("leaderboardTableView")]
        private LeaderboardTableView leaderboardTableView = null;

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


        public int page;
        public int sortMethod;

        [UIAction("OnPageUp")]
        private void OnPageUp()
        {
            Plugin.Log.Info("OnPageUp");
        }

        [UIAction("OnPageDown")]
        private void OnPageDown()
        {
            Plugin.Log.Info("OnPageDown");

        }
        [UIAction("changeSort")]
        private void changeSort()
        {
            Plugin.Log.Info("changeSort");
        }

        [UIAction("OnIconSelected")]
        private void OnIconSelected(SegmentedControl segmentedControl, int index)
        {
            Plugin.Log.Notice(index.ToString());
            Plugin.Log.Info("OnIconSelected");
        }


        [UIValue("leaderboardIcons")]
        private List<IconSegmentedControl.DataItem> leaderboardIcons
        {
            get
            {
                return new List<IconSegmentedControl.DataItem>()
                {
                new IconSegmentedControl.DataItem(
                    BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.clock.png"), "FORNTIE"),
                new IconSegmentedControl.DataItem(
                    BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("LocalLeaderboard.Images.score.png"), "FORNTIE")
                };
            }
        }



        [UIAction("#post-parse")]
        private void PostParse()
        {

        }

        private static Texture2D LoadEmbeddedResourceTexture(string resourcePath)
        {
            Texture2D texture = new Texture2D(2, 2);

            // Load the embedded resource as a byte array
            byte[] bytes = null;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
            }

            // Load the byte array into the texture
            texture.LoadImage(bytes);

            return texture;
        }

        void RichMyText(LeaderboardTableView tableView)
        {
            foreach (LeaderboardTableCell cell in tableView.GetComponentsInChildren<LeaderboardTableCell>())
            {
                cell.showSeparator = true;
                cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText").richText = true;
            }
        }

        PanelView Panel;
        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            Panel = UnityEngine.Resources.FindObjectsOfTypeAll<PanelView>().FirstOrDefault();
            currentDifficultyBeatmap = difficultyBeatmap;
            if (!this.isActivated) return;
            errorText.gameObject.SetActive(false);
            string mapId = difficultyBeatmap.level.levelID;
            int difficulty = difficultyBeatmap.difficultyRank;
            string mapType = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string balls = mapType + difficulty.ToString();
            List<LeaderboardData.LeaderboardData.LeaderboardEntry> leaderboardEntries = LeaderboardData.LeaderboardData.LoadBeatMapInfo(mapId, balls);

            // Check that there are leaderboard entries
            if (leaderboardEntries.Count > 0)
            {
                // Create a copy of the leaderboard data as it is already sorted by last played
                LeaderboardData.LeaderboardData.LeaderboardEntry recent = leaderboardEntries[leaderboardEntries.Count - 1];

                // Set the lastPlayed text to the most recent play
                Panel.lastPlayed.SetText("Last Played: " + recent.datePlayed);
            }
            leaderboardTableView.SetScores(CreateLeaderboardData(leaderboardEntries, page), -1);
            RichMyText(leaderboardTableView);
        }

        public List<LeaderboardTableView.ScoreData> CreateLeaderboardData(List<LeaderboardData.LeaderboardData.LeaderboardEntry> leaderboard, int page)
        {
            UnityEngine.Debug.Log("Creating Leaderboard Data");
            List<LeaderboardTableView.ScoreData> tableData = new List<LeaderboardTableView.ScoreData>();
            int pageIndex = page * 10;
            for (int i = pageIndex; i < leaderboard.Count && i < pageIndex + 10; i++)
            {
                int score = leaderboard[i].score;
                UnityEngine.Debug.LogFormat("creating leaderboard entry number {0}", i);
                tableData.Add(CreateLeaderboardEntryData(leaderboard[i], i + 1, score));
            }
            UnityEngine.Debug.Log("Created Leaderboard Data");
            return tableData;
        }

        public LeaderboardTableView.ScoreData CreateLeaderboardEntryData(LeaderboardData.LeaderboardData.LeaderboardEntry entry, int rank, int score)
        {
            UnityEngine.Debug.Log("Creating Entry Data");

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

            UnityEngine.Debug.Log("Created Entry Data");
            return new LeaderboardTableView.ScoreData(score, result, rank, false);
        }


    }
}