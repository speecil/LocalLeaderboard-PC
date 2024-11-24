using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using LocalLeaderboard.UI.ViewControllers;
using System;

namespace LocalLeaderboard.UI
{
    internal class LLLeaderboard : CustomLeaderboard, IDisposable
    {
        private readonly CustomLeaderboardManager _manager;
        private readonly LeaderboardView _leaderboardView;

        public override bool ShowForLevel(BeatmapKey? selectedLevel)
        {
            return true;
        }
        public override string leaderboardId => "LocalLeaderboard";

        internal LLLeaderboard(CustomLeaderboardManager customLeaderboardManager, PanelView panelView, LeaderboardView leaderboardView)
        {
            panelViewController = panelView;
            _leaderboardView = leaderboardView;
            _manager = customLeaderboardManager;
            _manager.Register(this);
        }

        public override ViewController panelViewController { get; }
        public override ViewController leaderboardViewController => _leaderboardView;

        public void Dispose()
        {
            _manager.Unregister(this);
        }
    }
}
