using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LocalLeaderboard.LeaderboardData
{
    public static class LeaderboardData
    {
        public static JObject LocalLeaderboardData;

        public static void createConfigIfNeeded()
        {
            if (LocalLeaderboardData != null) return;
            if (File.Exists("./UserData/LocalLeaderboard/LocalLeaderboardData.json"))
            {
                string configJson = File.ReadAllText("./UserData/LocalLeaderboard/LocalLeaderboardData.json");
                LocalLeaderboardData = JObject.Parse(configJson);
            }
            else
            {
                // Create a new empty JObject and file if the config file doesn't exist
                LocalLeaderboardData = new JObject();
                Directory.CreateDirectory("./UserData/LocalLeaderboard");
                using (StreamWriter file = File.CreateText("./UserData/LocalLeaderboard/LocalLeaderboardData.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, LocalLeaderboardData);
                }
            }
        }

        static void writeData()
        {
            Plugin.Log.Notice("WRITING DATA");
            if (File.Exists("./UserData/LocalLeaderboard/LocalLeaderboardData.json"))
            {
                File.WriteAllText("./UserData/LocalLeaderboard/LocalLeaderboardData.json", LocalLeaderboardData.ToString());
            }
            else
            {
                createConfigIfNeeded();
                File.WriteAllText("./UserData/LocalLeaderboard/LocalLeaderboardData.json", LocalLeaderboardData.ToString());
            }
        }




        public struct LeaderboardEntry
        {
            public int missCount;
            public int badCutCount;
            public float acc;
            public bool fullCombo;
            public string datePlayed;
            public int score;
            public string mods;
            public int maxCombo;
            public int averageHitscore;

            public LeaderboardEntry(int missCount, int badCutCount, float acc, bool fullCombo, string datePlayed, int score, string mods, int maxCombo, int averageHitscore)
            {
                this.missCount = missCount;
                this.badCutCount = badCutCount;
                this.acc = acc;
                this.fullCombo = fullCombo;
                this.datePlayed = datePlayed;
                this.score = score;
                this.mods = mods;
                this.maxCombo = maxCombo;
                this.averageHitscore = averageHitscore;
            }
        }

        public static void AddBeatMap(string mapID, string diff, int missCount, int badCutCount, bool fullCombo, string datePlayed, float acc, int score, string mods, int maxCombo, int averageHitscore)
        {
            if (string.IsNullOrEmpty(mapID) || string.IsNullOrEmpty(diff))
            {
                throw new ArgumentException("mapID and diff must not be null or empty.");
            }

            if (LocalLeaderboardData.ContainsKey(mapID))
            {
                var mapData = LocalLeaderboardData[mapID];

                if (mapData.Contains(diff))
                {
                    ((JArray)mapData[diff]).Add(new JObject
            {
                { "missCount", missCount },
                { "badCutCount", badCutCount },
                { "fullCombo", fullCombo },
                { "datePlayed", datePlayed },
                { "acc", acc },
                { "score", score },
                { "modifiers", mods },
                { "maxCombo", maxCombo },
                { "averageHitscore", averageHitscore },
            });
                }
                else
                {
                    ((JArray)mapData[diff]).Add(new JObject
            {
                { "missCount", missCount },
                { "badCutCount", badCutCount },
                { "fullCombo", fullCombo },
                { "datePlayed", datePlayed },
                { "acc", acc },
                { "score", score },
                { "modifiers", mods },
                { "maxCombo", maxCombo },
                { "averageHitscore", averageHitscore },
            });
                }
            }
            else
            {
                var mapData = new JObject { { diff, new JArray(new JObject
        {
            { "missCount", missCount },
            { "badCutCount", badCutCount },
            { "fullCombo", fullCombo },
            { "datePlayed", datePlayed },
            { "acc", acc },
            { "score", score },
            { "modifiers", mods },
            { "maxCombo", maxCombo },
            { "averageHitscore", averageHitscore },
        }) } };
                LocalLeaderboardData.Add(mapID, mapData);
            }
            writeData();
        }



        public static void UpdateBeatMapInfo(string mapID, string diff, int missCount, int badCutCount, bool fullCombo, string datePlayed, float acc, int score, string mods, int maxCombo, int averageHitscore)
        {
            var difficulty = new JObject
        {
            { "missCount", missCount },
            { "badCutCount", badCutCount },
            { "fullCombo", fullCombo },
            { "datePlayed", datePlayed },
            { "acc", acc },
            { "score", score },
            { "modifiers", mods },
            { "maxCombo", maxCombo },
            { "averageHitscore", averageHitscore },
        };

            if (LocalLeaderboardData[mapID] == null)
            {
                AddBeatMap(mapID, diff, missCount, badCutCount, fullCombo, datePlayed, acc, score, mods, maxCombo, averageHitscore);
                return;
            }

            if (LocalLeaderboardData[mapID][diff] == null)
            {
                LocalLeaderboardData[mapID][diff] = new JArray(difficulty);
            }
            else
            {
                ((JArray)LocalLeaderboardData[mapID][diff]).Add(difficulty);
            }
            writeData();
        }

        public static List<LeaderboardEntry> LoadBeatMapInfo(string mapID, string diff)
        {
            var leaderboard = new List<LeaderboardEntry>();

            if (LocalLeaderboardData[mapID] != null && LocalLeaderboardData[mapID][diff] != null)
            {
                foreach (var scoreData in LocalLeaderboardData[mapID][diff])
                {
                    leaderboard.Add(new LeaderboardEntry(
                        scoreData["missCount"].Value<int>(),
                        scoreData["badCutCount"].Value<int>(),
                        scoreData["acc"].Value<float>(),
                        scoreData["fullCombo"].Value<bool>(),
                        scoreData["datePlayed"].Value<string>(),
                        scoreData["score"].Value<int>(),
                        scoreData["modifiers"].Value<string>(),
                        scoreData["maxCombo"].Value<int>(),
                        scoreData["averageHitscore"].Value<int>()
                        ));
                }
            }
            return leaderboard;
        }
    }
}