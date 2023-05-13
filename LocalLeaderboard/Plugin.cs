using LocalLeaderboard.Installers;
using IPA;
using SiraUtil.Zenject;
using Zenject;
using IPALogger = IPA.Logging.Logger;
using System.Runtime.InteropServices;
using IPA.Config.Stores;
using Newtonsoft.Json.Linq;
using HarmonyLib;
using System.Reflection;

namespace LocalLeaderboard
{
    [NoEnableDisable]
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector, IPA.Config.Config conf)
        {
            Instance = this;
            Log = logger;
            LeaderboardData.LeaderboardData.createConfigIfNeeded();
            Log.Info("HEY IM INIT");
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<AppInstaller>(Location.App);
        }
    }
}