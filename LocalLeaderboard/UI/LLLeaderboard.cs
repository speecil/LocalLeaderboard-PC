using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using LeaderboardCore;
using LocalLeaderboard.UI.ViewControllers;
using System;

namespace LocalLeaderboard.UI
{
    internal class LLLeaderboard : CustomLeaderboard, IDisposable
    {
        private readonly CustomLeaderboardManager _manager;
        private LeaderboardView _leaderboardView;

        internal LLLeaderboard(CustomLeaderboardManager customLeaderboardManager, PanelView panelView, LeaderboardView leaderboardView)
        {
            panelViewController = panelView;
            _leaderboardView = leaderboardView;
            _manager = customLeaderboardManager;
            _manager.Register(this);
        }

        protected override ViewController panelViewController { get; }
        protected override ViewController leaderboardViewController => _leaderboardView;

        public void Dispose()
        {
            _manager.Unregister(this);
        }
    }
}
