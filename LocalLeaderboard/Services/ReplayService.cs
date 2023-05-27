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
        [Inject] private ReplayerMenuLoader _replayerMenuLoader;
        public bool TryReadReplay(string filename, out Replay replay)
        {
            try
            {
                if (File.Exists(filename))
                {
                    Stream stream = File.Open(filename, FileMode.Open);
                    int arrayLength = (int)stream.Length;
                    byte[] buffer = new byte[arrayLength];
                    stream.Read(buffer, 0, arrayLength);
                    stream.Close();

                    var method = typeof(BeatLeader.Plugin).Assembly.GetType("BeatLeader.Models.Replay.ReplayDecoder").GetMethod("Decode", BindingFlags.Public | BindingFlags.Static);
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

        public void StartReplay(Replay replay) => _replayerMenuLoader.StartReplayAsync(replay, null, null);
    }
}
