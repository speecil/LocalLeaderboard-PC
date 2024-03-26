using LocalLeaderboard.AffinityPatches;
using System;
using System.IO;
using System.Reflection;
using Zenject;

namespace LocalLeaderboard.Services
{
    internal class ReplayService
    {
        private object _replayerMenuLoader;

        [Inject]
        public void Inject(BeatLeader.Replayer.ReplayerMenuLoader replayerMenuLoader)
        {
            _replayerMenuLoader = replayerMenuLoader;
        }

        public bool TryReadReplay(string filename, out BeatLeader.Models.Replay.Replay replay)
        {
            Plugin.Log.Info("TryReadReplay");
            try
            {
                if (File.Exists(filename))
                {
                    Stream stream = File.Open(filename, FileMode.Open);
                    int arrayLength = (int)stream.Length;
                    byte[] buffer = new byte[arrayLength];
                    stream.Read(buffer, 0, arrayLength);
                    stream.Close();

                    replay = BeatLeader.Models.Replay.ReplayDecoder.DecodeReplay(buffer);
                    return true;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
            replay = default;
            return false;
        }

        public void StartReplay(BeatLeader.Models.Replay.Replay replay, BeatLeader.Models.Player player)
        {
            if (_replayerMenuLoader == null)
            {
                ExtraSongData.IsLocalLeaderboardReplay = false;
                Plugin.Log.Error("replayerMenuLoader null");
                return;
            }
            ExtraSongData.IsLocalLeaderboardReplay = true;
            ((BeatLeader.Replayer.ReplayerMenuLoader)_replayerMenuLoader).StartReplayAsync(replay, player, null);
        }
    }
}
