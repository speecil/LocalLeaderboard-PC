using LocalLeaderboard.Services;
using LocalLeaderboard.UI;
using LocalLeaderboard.UI.ViewControllers;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using EntryHolder = LocalLeaderboard.UI.ViewControllers.LeaderboardView.EntryHolder;

namespace LocalLeaderboard.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LeaderboardView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PanelView>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<LLLeaderboard>().AsSingle();
            Container.Bind<TweeningService>().AsSingle();
            Container.Bind<PlayerService>().AsSingle();
            if (!Plugin.GetGameVersion().Contains("1.34"))
            {
                Plugin.beatLeaderInstalled = false;
            }
            else
            {
                Plugin.beatLeaderInstalled = Plugin.GetAssemblyByName("BeatLeader") != null;
            }
            if (Plugin.beatLeaderInstalled) Container.Bind<ReplayService>().AsSingle();
            ScoreInfoModal scoreInfoModal = new ScoreInfoModal();
            List<EntryHolder> holder = Enumerable.Range(0, 10).Select(x => new EntryHolder(scoreInfoModal.setScoreModalText)).ToList();
            Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.Bind<List<EntryHolder>>().FromInstance(holder).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.QueueForInject(scoreInfoModal);
        }
    }
}
