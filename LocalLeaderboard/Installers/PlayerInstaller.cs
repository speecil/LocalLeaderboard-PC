using LocalLeaderboard.AffinityPatches;
using Zenject;

namespace LocalLeaderboard.Installers
{
    internal class PlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ExtraSongData>().AsSingle();
        }
    }
}
