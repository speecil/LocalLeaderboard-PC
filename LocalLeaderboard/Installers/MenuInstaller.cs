using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace LocalLeaderboard.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.Log.Info("HEY IM INSTALL BINDINGS");
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<LLLeaderboard>().AsSingle();
        }
    }
}
