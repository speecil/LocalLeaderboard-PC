using LocalLeaderboard.UI.ViewControllers;
using LocalLeaderboard.UI;
using Zenject;
using LocalLeaderboard.Services;
using System.Collections.Generic;
using System.Linq;
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
            Container.Bind<ReplayService>().AsSingle();
            Container.Bind<PlayerService>().AsSingle();

            ScoreInfoModal scoreInfoModal = new ScoreInfoModal();
            List<ButtonHolder> holder = Enumerable.Range(0, 10).Select(x => new ButtonHolder(x, scoreInfoModal.setScoreModalText)).ToList();
            Container.Bind<ScoreInfoModal>().FromInstance(scoreInfoModal).AsSingle();
            Container.Bind<List<ButtonHolder>>().FromInstance(holder).AsSingle();
        }
    }
}
