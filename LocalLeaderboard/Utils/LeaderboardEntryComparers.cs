using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;

namespace LocalLeaderboard.Utils
{
    public class LeaderboardEntryDatePlayedComparer: IComparer<LLeaderboardEntry>
    {
        public int Compare(LLeaderboardEntry x, LLeaderboardEntry y)
        {
            return long.TryParse(x.datePlayed, out var fl) && long.TryParse(y.datePlayed, out var sl) ? fl.CompareTo(sl) : 0;
        }
    }
    
    public class LeaderboardEntryAccComparer: IComparer<LLeaderboardEntry>
    {
        public int Compare(LLeaderboardEntry x, LLeaderboardEntry y)
        {
            return x.acc.CompareTo(y.acc);
        }
    }
}