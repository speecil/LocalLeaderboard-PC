using IPA;
using IPA.Config.Stores;
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
        public static string userName;
        public static bool beatLeaderInstalled;

        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, IPA.Config.Config conf)
        {
            SettingsConfig.Instance = conf.Generated<SettingsConfig>();
            LeaderboardData.LeaderboardData.Setup();
            zenjector.UseLogger(logger);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<AppInstaller>(Location.App);
            zenjector.Install<GameInstaller>(Location.GameCore);
        }

        public static string GetGameVersion()
        {
            return IPA.Utilities.UnityGame.GameVersion.StringValue;
        }

        public static Assembly GetAssemblyByName(string name)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == name);
        }
    }
}
