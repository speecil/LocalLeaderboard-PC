using Hive.Versioning;
using IPA.Loader;
using LocalLeaderboard.Services;
using LocalLeaderboard.UI;
using LocalLeaderboard.UI.ViewControllers;
using SiraUtil.Logging;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using EntryHolder = LocalLeaderboard.UI.ViewControllers.LeaderboardView.EntryHolder;

namespace LocalLeaderboard.Installers
{
    internal class MenuInstaller : Installer
    {

        [Inject]
        private readonly SiraLog _logger;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<LLLeaderboard>().AsSingle();
            Container.Bind<TweeningService>().AsSingle();
            Container.Bind<PlayerService>().AsSingle();

            if (!Plugin.GetGameVersion().Contains("1.36") && Plugin.GetAssemblyByName("BeatLeader") != null)
            {
                Container.Bind<ReplayService>().AsSingle();
            }

            PluginMetadata sph = PluginManager.GetPluginFromId("SongPlayHistory");
            if (sph != null && sph.HVersion >= new Version(2, 1, 0))
            {
                _logger.Debug("Found supported SPH, installing SPHInstaller");
                Container.Install<SPHInstaller>();
            }

            ScoreInfoModal scoreInfoModal = new();
            List<EntryHolder> holder = Enumerable.Range(0, 10).Select(x => new EntryHolder(scoreInfoModal.setScoreModalText)).ToList();
            Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.Bind<List<EntryHolder>>().FromInstance(holder).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.QueueForInject(scoreInfoModal);
        }
    }
}
