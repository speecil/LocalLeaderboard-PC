using IPA;
using IPA.Config.Stores;
using LocalLeaderboard.Installers;
using SiraUtil.Zenject;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace LocalLeaderboard
{
    [NoEnableDisable]
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        public static bool UserIsPatron;
        public static string userName;
        public static bool beatLeaderInstalled;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, IPA.Config.Config conf)
        {
            Instance = this;
            Log = logger;
            SettingsConfig.Instance = conf.Generated<SettingsConfig>();
            LeaderboardData.LeaderboardData.createConfigIfNeeded();

            if (!GetGameVersion().Contains("1.29"))
            {
                beatLeaderInstalled = false;
            }
            else
            {
                beatLeaderInstalled = GetAssemblyByName("BeatLeader") != null;
            }

            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<AppInstaller>(Location.App);
        }

        public static string GetGameVersion()
        {

            string versionFilePath = "./BeatSaberVersion.txt";

            if (File.Exists(versionFilePath))
            {
                return File.ReadAllText(versionFilePath);
            }
            return string.Empty;
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
    }
}
