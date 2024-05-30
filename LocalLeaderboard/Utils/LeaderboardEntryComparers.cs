using System.Collections.Generic;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;

namespace LocalLeaderboard.Utils
{
    public class LeaderboardEntryDatePlayedComparer : IComparer<LLeaderboardEntry>
    {
        public int Compare(LLeaderboardEntry x, LLeaderboardEntry y)
        {
            return long.TryParse(x.datePlayed, out long fl) && long.TryParse(y.datePlayed, out long sl) ? fl.CompareTo(sl) : 0;
        }
    }

    public class LeaderboardEntryAccComparer : IComparer<LLeaderboardEntry>
    {
        public int Compare(LLeaderboardEntry x, LLeaderboardEntry y)
        {
            return x.acc.CompareTo(y.acc);
        }
    }
}