using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LocalLeaderboard.Services;
using LocalLeaderboard.Utils;
using SiraUtil.Logging;
using SongPlayHistory.Model;
using SongPlayHistory.SongPlayData;
using Zenject;
using LLeaderboardEntry = LocalLeaderboard.LeaderboardData.LeaderboardData.LeaderboardEntry;


namespace LocalLeaderboard.SongPlayHistory
{
    internal class SongPlayHistoryDataService: IExternalDataService
    {

        [Inject]
        private SiraLog _logger;
        
        [InjectOptional]
        private IRecordManager _recordManager;
        
        [InjectOptional]
        private IScoringCacheManager _scoringCacheManager;
        
        public async Task<IList<LLeaderboardEntry>> GetLeaderboardEntries(IDifficultyBeatmap beatmap, CancellationToken token)
        {

            if (_recordManager == null || _scoringCacheManager == null)
            {
                return new List<LLeaderboardEntry>();
            }
            
            var records = _recordManager.GetRecords(beatmap);
            var scoringCache = await _scoringCacheManager.GetScoringInfo(beatmap, token);
            
            var entries = new List<LLeaderboardEntry>(records.Count);
            foreach (var record in records)
            {
                var playTime = record.LocalTime.ToUniversalTime();
                var unixTimeSeconds = new DateTimeOffset(playTime).ToUnixTimeSeconds();
                
                var modsString = "";
                var param = record.Params;
                
                if (param.HasFlag(SongPlayParam.SubmissionDisabled)) continue;
                
                if (param != SongPlayParam.None)
                {
                    if (record.LevelEnd == LevelEndType.Cleared && record.ModifiedScore == record.RawScore)
                    {
                        param &= ~SongPlayParam.NoFail;
                    }
                    
                    var mods = new List<string>(10);
                    if (param.HasFlag(SongPlayParam.BatteryEnergy)) mods.Add("BE");
                    if (param.HasFlag(SongPlayParam.NoFail)) mods.Add("NF");
                    if (param.HasFlag(SongPlayParam.InstaFail)) mods.Add("IF");
                    if (param.HasFlag(SongPlayParam.NoObstacles)) mods.Add("NO");
                    if (param.HasFlag(SongPlayParam.NoBombs)) mods.Add("NB");
                    if (param.HasFlag(SongPlayParam.FastNotes)) mods.Add("FN");
                    if (param.HasFlag(SongPlayParam.StrictAngles)) mods.Add("SA");
                    if (param.HasFlag(SongPlayParam.DisappearingArrows)) mods.Add("DA");
                    if (param.HasFlag(SongPlayParam.SuperFastSong)) mods.Add("SFS");
                    if (param.HasFlag(SongPlayParam.FasterSong)) mods.Add("FS");
                    if (param.HasFlag(SongPlayParam.SlowerSong)) mods.Add("SS");
                    if (param.HasFlag(SongPlayParam.NoArrows)) mods.Add("NA");
                    if (param.HasFlag(SongPlayParam.GhostNotes)) mods.Add("GN");
                    if (param.HasFlag(SongPlayParam.SmallCubes)) mods.Add("SN");
                    if (param.HasFlag(SongPlayParam.ProMode)) mods.Add("PRO");
                    
                    
                    modsString = string.Join(" ", mods);
                }
                
                float denominator;
                
                if (record.LevelEnd == LevelEndType.Cleared)
                {
                    denominator = scoringCache.MaxMultipliedScore;
                }
                else if (record.MaxRawScore != null)
                {
                    denominator = record.MaxRawScore.Value;
                }
                else if (scoringCache.IsV2Score)
                {
                    denominator = ScoreUtil.CalculateV2MaxScore(record.LastNote);
                }
                else
                {
                    denominator = 0;
                }
                
                var entry = new LLeaderboardEntry(
                    -1,
                    -1,
                    denominator == 0 ? 0f : record.RawScore / denominator * 100,
                    false,
                    unixTimeSeconds.ToString(),
                    record.ModifiedScore,
                    modsString,
                    -1,
                    -1,
                    record.LevelEnd == LevelEndType.Failed,
                    "",
                    0,
                    0,
                    -1,
                    0,
                    0,
                    0,
                    -1,
                    true
                );
                
                entries.Add(entry);
            }

            return entries;
        }
    }
}