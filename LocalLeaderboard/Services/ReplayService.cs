using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using System;
using System.IO;
using System.Reflection;
using Zenject;

namespace LocalLeaderboard.Services
{
    internal class ReplayService
    {
        [InjectOptional] private ReplayerMenuLoader _replayerMenuLoader;
        public bool TryReadReplay(string filename, out Replay replay)
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

                    replay = (Replay)method.Invoke(null, new object[] { buffer });

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

        public void StartReplay(Replay replay, BeatLeader.Models.Player player)
        {
            if (_replayerMenuLoader == null) return;
            _replayerMenuLoader.StartReplayAsync(replay, player, null);
        }
    }
}
