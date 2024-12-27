using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Version = Hive.Versioning.Version;
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
        internal const string WEBSITE_URL = "https://speecil.dev";

        internal static readonly Color SPEECIL_COLOUR = new(0.156f, 0.69f, 0.46666f, 1);
        internal static readonly Color SPEECIL_COLOUR_BRIGHTER = new((float)47 / 255, (float)212 / 255, (float)143 / 255, 1);
        internal static readonly Version CURRENT_GAME_VERSION = new(1, 40, 0);

        internal static bool BL_INSTALLED()
        {
            if (!(GetGameVersion() >= CURRENT_GAME_VERSION))
            {
                return false;
            }
            return PluginManager.GetPluginFromId("BeatLeader") != null;
        }

        public static Version GetGameVersion()
        {
            try
            {
                List<string> versionParts = Application.version.Split('.', '_').Take(3).ToList();
                return new Version(ulong.Parse(versionParts[0]), ulong.Parse(versionParts[1]), ulong.Parse(versionParts[2]));
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
