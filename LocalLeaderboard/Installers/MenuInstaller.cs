using LocalLeaderboard.Services;
using LocalLeaderboard.UI;
using LocalLeaderboard.UI.ViewControllers;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using ButtonHolder = LocalLeaderboard.UI.ViewControllers.LeaderboardView.ButtonHolder;

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

            if (Plugin.beatLeaderInstalled) Container.Bind<ReplayService>().AsSingle();

            Container.Bind<PlayerService>().AsSingle();

            ScoreInfoModal scoreInfoModal = new ScoreInfoModal();
            List<ButtonHolder> holder = Enumerable.Range(0, 10).Select(x => new ButtonHolder(x, scoreInfoModal.setScoreModalText)).ToList();
            Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.Bind<List<ButtonHolder>>().FromInstance(holder).AsSingle().WhenInjectedInto<LeaderboardView>();
            Container.QueueForInject(scoreInfoModal);
        }
    }
}
