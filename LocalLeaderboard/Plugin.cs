using LocalLeaderboard.Installers;
using IPA;
using SiraUtil.Zenject;
using Zenject;
using IPALogger = IPA.Logging.Logger;
using System.Runtime.InteropServices;
using IPA.Config.Stores;
using IPA.Config;
using LocalLeaderboard;
namespace LocalLeaderboard
{
    [NoEnableDisable]
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        public static bool UserIsPatron;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, IPA.Config.Config conf)
        {
            Instance = this;
            Log = logger;
            SettingsConfig.Instance = conf.Generated<SettingsConfig>();
            LeaderboardData.LeaderboardData.createConfigIfNeeded();
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<AppInstaller>(Location.App);
        }

        [OnEnable]
        public void OnEnable()
        {
            SettingsConfig.Instance.ApplyValues();
        }
    }
}