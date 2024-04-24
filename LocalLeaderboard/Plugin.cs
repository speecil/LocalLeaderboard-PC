using Hive.Versioning;
using IPA;
using IPA.Config.Stores;
using LocalLeaderboard.Installers;
using SiraUtil.Zenject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using Version = Hive.Versioning.Version;

namespace LocalLeaderboard
{
    [NoEnableDisable]
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static string userName;

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
