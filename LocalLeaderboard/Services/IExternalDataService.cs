using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;

namespace LocalLeaderboard.Services
{
    internal interface IExternalDataService
    {
        public Task<IList<LLeaderboardEntry>> GetLeaderboardEntries(BeatmapKey beatmap, CancellationToken token);
    }
}