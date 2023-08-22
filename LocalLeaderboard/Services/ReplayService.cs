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
            Plugin.Log.Info("TRYREADREPLAY");
            //BeatLeader.Models.Replay.ReplayDecoder
            var method = Plugin.GetAssemblyByName("BeatLeader").GetType("BeatLeader.Models.Replay.ReplayDecoder").GetMethod("DecodeReplay", BindingFlags.Public | BindingFlags.Static);
            Plugin.Log.Info("method attempt");
            if (method == null) { replay = default; Plugin.Log.Info("method null bruh"); return false;  }
            try
            {
                if (File.Exists(filename))
                {
                    Stream stream = File.Open(filename, FileMode.Open);
                    int arrayLength = (int)stream.Length;
                    byte[] buffer = new byte[arrayLength];
                    stream.Read(buffer, 0, arrayLength);
                    stream.Close();

                    replay = (BeatLeader.Models.Replay.Replay)method.Invoke(null, new object[] { buffer });
                    Plugin.Log.Info("SUCCESS READING REPLAY!");
                    return true;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Error(e);
            }
            Plugin.Log.Info("FAILED TO READ REPLAY!");
            replay = default;
            return false;
        }

        public void StartReplay(BeatLeader.Models.Replay.Replay replay, BeatLeader.Models.Player player)
        {
            Plugin.Log.Info("START REPLAY!");
            if (_replayerMenuLoader == null) return;
            Plugin.Log.Info("AFTER NULL CHECK REPLAY!");
            ((BeatLeader.Replayer.ReplayerMenuLoader)_replayerMenuLoader).StartReplayAsync(replay, player, null);
        }
    }
}
