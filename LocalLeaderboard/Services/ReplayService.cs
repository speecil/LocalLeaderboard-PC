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
            var method = Plugin.GetAssemblyByName("BeatLeader").GetType("BeatLeader.Models.Replay.ReplayDecoder").GetMethod("Decode", BindingFlags.Public | BindingFlags.Static);
            if (method == null) { replay = default; return false; }
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

                    return true;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Debug(e);
            }

            replay = default;
            return false;
        }

        public void StartReplay(BeatLeader.Models.Replay.Replay replay, BeatLeader.Models.Player player)
        {
            if (_replayerMenuLoader == null) return;
            ((BeatLeader.Replayer.ReplayerMenuLoader)_replayerMenuLoader).StartReplayAsync(replay, player, null);
        }
    }
}
