using Zenject;
using LocalLeaderboard.Services;
using LocalLeaderboard.SongPlayHistory;
using SiraUtil.Logging;

namespace LocalLeaderboard.Installers
{
    public class SPHInstaller: Installer
    {
        
        [Inject]
        private SiraLog _logger;
        
        public override void InstallBindings()
        {
            _logger.Notice("Binding SongPlayHistoryDataService");
            Container.BindInterfacesTo<SongPlayHistoryDataService>().AsSingle();
        }
    }
}