using LocalLeaderboard.AffinityPatches;
using Zenject;

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
