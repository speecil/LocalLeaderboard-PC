using LocalLeaderboard.Services;
using LocalLeaderboard.UI;
using LocalLeaderboard.UI.ViewControllers;
using System.Collections.Generic;
using System.Linq;
using IPA.Loader;
using Zenject;
using EntryHolder = LocalLeaderboard.UI.ViewControllers.LeaderboardView.EntryHolder;
using Hive.Versioning;
using SiraUtil.Logging;

namespace LocalLeaderboard.Installers
{
    internal class MenuInstaller : Installer
    {

        [Inject]
        private SiraLog _logger;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<LLLeaderboard>().AsSingle();
            Container.Bind<TweeningService>().AsSingle();
            Container.Bind<PlayerService>().AsSingle();

            if (!Plugin.GetGameVersion().Contains("1.35"))
            {
                Plugin.beatLeaderInstalled = false;
            }
            else
            {
                Plugin.beatLeaderInstalled = Plugin.GetAssemblyByName("BeatLeader") != null;
            }
            if (Plugin.beatLeaderInstalled) Container.Bind<ReplayService>().AsSingle();

            
            if (Plugin.GetAssemblyByName("BeatLeader") != null) Container.Bind<ReplayService>().AsSingle();
            var sph = PluginManager.GetPluginFromId("SongPlayHistory");
            if (sph != null && sph.HVersion >= new Version(2, 1, 0)) 
            {
                _logger.Debug("Found supported SPH, installing SPHInstaller");
                Container.Install<SPHInstaller>();
            }     

            ScoreInfoModal scoreInfoModal = new ScoreInfoModal();
            List<EntryHolder> holder = Enumerable.Range(0, 10).Select(x => new EntryHolder(scoreInfoModal.setScoreModalText)).ToList();
            Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.Bind<List<EntryHolder>>().FromInstance(holder).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.QueueForInject(scoreInfoModal);
        }
    }
}
