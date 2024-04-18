using LocalLeaderboard.SongPlayHistory;
using SiraUtil.Logging;
using Zenject;

namespace LocalLeaderboard.Installers
{
    public class SPHInstaller : Installer
    {

        [Inject]
        private readonly SiraLog _logger;

        public override void InstallBindings()
        {
            _logger.Notice("Binding SongPlayHistoryDataService");
            Container.BindInterfacesTo<SongPlayHistoryDataService>().AsSingle();
        }
    }
}