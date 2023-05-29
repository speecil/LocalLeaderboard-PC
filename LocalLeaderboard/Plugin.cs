using IPA;
using IPA.Config.Stores;
using IPA.Loader;
using LocalLeaderboard.Installers;
using SiraUtil.Zenject;
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
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<AppInstaller>(Location.App);
        }

        [OnEnable]
        public void OnEnable()
        {
            try
            {
                var method = typeof(BeatLeader.Plugin).Assembly.GetType("BeatLeader.Models.Replay.ReplayDecoder").GetMethod("Decode", BindingFlags.Public | BindingFlags.Static);
                if(method != null)
                {
                    beatLeaderInstalled = true;
                }
                else
                {
                    beatLeaderInstalled = false;
                }
            }
            catch
            {
                beatLeaderInstalled = false;
            }
        }
    }
}