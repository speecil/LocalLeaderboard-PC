using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using UnityEngine;

namespace LocalLeaderboard.Utils
{
    internal static class Constants
    {
        internal const string CONFIG_DIR = "./UserData/LocalLeaderboard/";
        internal const string CONFIG_PATH = CONFIG_DIR + "LocalLeaderboardData.json";
        internal const string LLREPLAYS_PATH = CONFIG_DIR + "Replays/";
        internal const string BLREPLAY_PATH = "./UserData/BeatLeader/Replays/";
        internal const string STEAM_API_PATH = "./Beat Saber_Data/Plugins/x86_64/steam_api64.dll";

        internal const string DISCORD_URL = "https://discord.gg/2KyykDXpBk";
        internal const string PATREON_URL = "https://patreon.com/speecil";
        internal const string WEBSITE_URL = "https://speecil.dev/localleaderboard.html";

        internal static readonly Color SPEECIL_COLOUR = new Color(0.156f, 0.69f, 0.46666f, 1);
        internal static readonly Color SPEECIL_COLOUR_BRIGHTER = new Color((float)47 / 255, (float)212 / 255, (float)143 / 255, 1);


        public static Version GetGameVersion()
        {
            try
            {
                List<int> Version = Application.version.Split('.').Select(int.Parse).ToList();
                return new Version(Version[0], Version[1], Version[2]);
            }
            catch
            {
                return new Version(0, 0, 0);
            }
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
    }
}
