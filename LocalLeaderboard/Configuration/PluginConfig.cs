using LocalLeaderboard.Utils;
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
            if (File.Exists(Constants.CONFIG_PATH))
            {
                string configJson = File.ReadAllText(Constants.CONFIG_PATH);
                LocalLeaderboardData = JObject.Parse(configJson);
            }
            else
            {
                // Create a new empty JObject and file if the config file doesn't exist
                LocalLeaderboardData = new JObject();
                Directory.CreateDirectory(Constants.CONFIG_DIR);
                using (StreamWriter file = File.CreateText(Constants.CONFIG_PATH))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, LocalLeaderboardData);
                }
            }
        }

        static void writeData()
        {
            Plugin.Log.Notice("WRITING DATA");
            if (File.Exists(Constants.CONFIG_PATH))
            {
                File.WriteAllText(Constants.CONFIG_PATH, LocalLeaderboardData.ToString());
            }
            else
            {
                createConfigIfNeeded();
                File.WriteAllText(Constants.CONFIG_PATH, LocalLeaderboardData.ToString());
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
            public bool didFail;
            public string bsorPath;

            public LeaderboardEntry(int missCount, int badCutCount, float acc, bool fullCombo, string datePlayed, int score, string mods, int maxCombo, int averageHitscore, bool didFail, string bsorPath)
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
                this.didFail = didFail;
                this.bsorPath = bsorPath;
            }
        }

        public static void AddBeatMap(string mapID, string diff, int missCount, int badCutCount, bool fullCombo, string datePlayed, float acc, int score, string mods, int maxCombo, int averageHitscore, bool didFail, string bsorPath)
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
                { "didFail", didFail },
                { "bsorPath", bsorPath },


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
                { "didFail", didFail },
                { "bsorPath", bsorPath },


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
            { "didFail", didFail },
            { "bsorPath", bsorPath },
        }) } };
                LocalLeaderboardData.Add(mapID, mapData);
            }
            writeData();
        }



        public static void UpdateBeatMapInfo(string mapID, string diff, int missCount, int badCutCount, bool fullCombo, string datePlayed, float acc, int score, string mods, int maxCombo, int averageHitscore, bool didFail, string bsorPath)
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
            { "didFail", averageHitscore },
            { "bsorPath", bsorPath },
        };

            if (LocalLeaderboardData[mapID] == null)
            {
                AddBeatMap(mapID, diff, missCount, badCutCount, fullCombo, datePlayed, acc, score, mods, maxCombo, averageHitscore, didFail, bsorPath);
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
                    int? missCount = scoreData["missCount"]?.Value<int>();
                    int? badCutCount = scoreData["badCutCount"]?.Value<int>();
                    float? acc = scoreData["acc"]?.Value<float>();
                    bool? fullCombo = scoreData["fullCombo"]?.Value<bool>();
                    string datePlayed = scoreData["datePlayed"]?.Value<string>();
                    int? score = scoreData["score"]?.Value<int>();
                    string modifiers = scoreData["modifiers"]?.Value<string>();
                    int? maxCombo = scoreData["maxCombo"]?.Value<int>();
                    int? averageHitscore = scoreData["averageHitscore"]?.Value<int>();
                    bool? didFail = scoreData["didFail"]?.Value<bool>();
                    string bsorPath = scoreData["bsorPath"]?.Value<string>();

                    leaderboard.Add(new LeaderboardEntry(
                        missCount ?? 0,
                        badCutCount ?? 0,
                        acc ?? 0f,
                        fullCombo ?? false,
                        datePlayed,
                        score ?? 0,
                        modifiers,
                        maxCombo ?? 0,
                        averageHitscore ?? 0,
                        didFail ?? false,
                        bsorPath ?? ""
                        ));
                }
            }
            return leaderboard;
        }
    }
}