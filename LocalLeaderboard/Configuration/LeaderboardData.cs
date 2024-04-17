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

        public static void Setup()
        {
            if (LocalLeaderboardData != null) return;
            if (File.Exists(Constants.CONFIG_PATH))
            {
                string configJson = File.ReadAllText(Constants.CONFIG_PATH);
                LocalLeaderboardData = JObject.Parse(configJson);
            }
            else
            {
                LocalLeaderboardData = new JObject();
                Directory.CreateDirectory(Constants.CONFIG_DIR);

                LocalLeaderboardData["WARNING"] = "Hey there, please do not manually edit this json file as its useless to do so and may break things - Speecil";

                using (StreamWriter file = File.CreateText(Constants.CONFIG_PATH))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, LocalLeaderboardData);
                }
            }
        }

        static void writeData()
        {
            if (File.Exists(Constants.CONFIG_PATH))
            {
                File.WriteAllText(Constants.CONFIG_PATH, LocalLeaderboardData.ToString());
            }
            else
            {
                Setup();
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
            public float averageHitscore;
            public bool didFail;
            public string bsorPath;
            public float avgAccRight;
            public float avgAccLeft;
            public int perfectStreak;
            public float rightHandTimeDependency;
            public float leftHandTimeDependency;
            public float fcAcc;
            public int pauses;

            public bool isExternal;

            public LeaderboardEntry(int missCount, int badCutCount, float acc, bool fullCombo, string datePlayed, int score, string mods, int maxCombo, float averageHitscore, bool didFail, string bsorPath, float avgAccRight, float avgAccLeft, int perfectStreak, float rightHandTimeDependency, float leftHandTimeDependency, float fcAcc, int pauses, bool isExternal)
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
                this.avgAccRight = avgAccRight;
                this.avgAccLeft = avgAccLeft;
                this.perfectStreak = perfectStreak;
                this.rightHandTimeDependency = rightHandTimeDependency;
                this.leftHandTimeDependency = leftHandTimeDependency;
                this.fcAcc = fcAcc;
                this.pauses = pauses;
                this.isExternal = isExternal;
            }

            public bool IsSamePlay(LeaderboardEntry other)
            {
                // If the scores are the same and the time played is within 10 seconds of each other, consider them the same play.
                return score == other.score && long.TryParse(datePlayed, out var x) && long.TryParse(other.datePlayed, out var y) && Math.Abs(x - y) <= 10;
            }
        }

        public static void AddBeatMap(string mapID, string diff, int missCount, int badCutCount, bool fullCombo, string datePlayed, float acc, int score, string mods, int maxCombo, float averageHitscore, bool didFail, string bsorPath, float avgAccRight, float avgAccLeft, int perfectStreak, float rightHandTimeDependency, float leftHandTimeDependency, float fcAcc, int pauses)
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
                { "rightHandTimeDependency", rightHandTimeDependency },
                { "leftHandTimeDependency", leftHandTimeDependency },
                { "rightHandAverageScore", avgAccRight },
                { "leftHandAverageScore", avgAccLeft},
                { "perfectStreak", perfectStreak },
                { "fcAccuracy", fcAcc },
                { "pauses", pauses }


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
                { "rightHandTimeDependency", rightHandTimeDependency },
                { "leftHandTimeDependency", leftHandTimeDependency },
                { "rightHandAverageScore", avgAccRight },
                { "leftHandAverageScore", avgAccLeft},
                { "perfectStreak", perfectStreak },
                { "fcAccuracy", fcAcc },
                { "pauses", pauses }


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
            { "rightHandTimeDependency", rightHandTimeDependency },
            { "leftHandTimeDependency", leftHandTimeDependency },
            { "rightHandAverageScore", avgAccRight },
            { "leftHandAverageScore", avgAccLeft},
            { "perfectStreak", perfectStreak },
            { "fcAccuracy", fcAcc },
            { "pauses", pauses }
        }) } };
                LocalLeaderboardData.Add(mapID, mapData);
            }
            writeData();
        }



        public static void UpdateBeatMapInfo(string mapID, string diff, int missCount, int badCutCount, bool fullCombo, string datePlayed, float acc, int score, string mods, int maxCombo, float averageHitscore, bool didFail, string bsorPath, float avgAccRight, float avgAccLeft, int perfectStreak, float rightHandTimeDependency, float leftHandTimeDependency, float fcAcc, int pauses)
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
            { "didFail", didFail },
            { "bsorPath", bsorPath },
            { "rightHandTimeDependency", rightHandTimeDependency },
            { "leftHandTimeDependency", leftHandTimeDependency },
            { "rightHandAverageScore", avgAccRight },
            { "leftHandAverageScore", avgAccLeft},
            { "perfectStreak", perfectStreak },
            { "fcAccuracy", fcAcc },
            { "pauses", pauses }
        };

            if (LocalLeaderboardData[mapID] == null)
            {
                AddBeatMap(mapID, diff, missCount, badCutCount, fullCombo, datePlayed, acc, score, mods, maxCombo, averageHitscore, didFail, bsorPath, avgAccRight, avgAccLeft, perfectStreak, rightHandTimeDependency, leftHandTimeDependency, fcAcc, pauses);
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
                    string? datePlayed = scoreData["datePlayed"]?.Value<string>();
                    int? score = scoreData["score"]?.Value<int>();
                    string modifiers = scoreData["modifiers"]?.Value<string>();
                    int? maxCombo = scoreData["maxCombo"]?.Value<int>();
                    float? averageHitscore = scoreData["averageHitscore"]?.Value<float>();
                    bool? didFail = scoreData["didFail"]?.Value<bool>();
                    string? bsorPath = scoreData["bsorPath"]?.Value<string>();
                    float? avgAccRight = scoreData["rightHandAverageScore"]?.Value<float>();
                    float? avgAccLeft = scoreData["leftHandAverageScore"]?.Value<float>();
                    int? perfectStreak = scoreData["perfectStreak"]?.Value<int>();
                    float? rightHandTimeDependency = scoreData["rightHandTimeDependency"]?.Value<float>();
                    float? leftHandTimeDependency = scoreData["leftHandTimeDependency"]?.Value<float>();
                    float? fcAcc = scoreData["fcAccuracy"]?.Value<float>();
                    int? pauses = scoreData["pauses"]?.Value<int>();

                    if (score == null) continue;  // If score is null, this is invalid and should be ignored.
                    
                    leaderboard.Add(new LeaderboardEntry(
                        missCount ?? -1,
                        badCutCount ?? -1,
                        acc ?? 0f,
                        fullCombo ?? false,
                        datePlayed ?? "",
                        score ?? 0,
                        modifiers ?? "",
                        maxCombo ?? -1,
                        averageHitscore ?? -1,
                        didFail ?? false,
                        bsorPath ?? "",
                        avgAccRight ?? 0,
                        avgAccLeft ?? 0,
                        perfectStreak ?? -1,
                        rightHandTimeDependency ?? 0,
                        leftHandTimeDependency ?? 0,
                        fcAcc ?? 0,
                        pauses ?? -1,
                        false
                        ));
                }
            }
            return leaderboard;
        }
    }
}