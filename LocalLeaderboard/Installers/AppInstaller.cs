using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;
using LocalLeaderboard.AffinityPatches;

namespace LocalLeaderboard.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<Results>().AsSingle();
        }
    }
}
