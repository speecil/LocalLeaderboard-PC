using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using LocalLeaderboard.Installers;
using SiraUtil.Zenject;
using System;
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
            try
            {
                var method = GetAssemblyByName("BeatLeader");
                beatLeaderInstalled = method != null;
            }
            catch
            {
                beatLeaderInstalled = false;
            }
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<AppInstaller>(Location.App);
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().
                   SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
    }
}