using IPA.Utilities;
using IPA.Utilities.Async;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LocalLeaderboard.Services
{
    internal class PlayerService
    {
        public Task<(string, string)> GetPlayerInfo()
        {
            TaskCompletionSource<(string, string)> taskCompletionSource = new TaskCompletionSource<(string, string)>();

            if (File.Exists(Path.Combine(UnityGame.InstallPath, "Beat Saber_Data", "Plugins", "x86_64", "steam_api64.dll")))
            {
                //Steamworks.SteamFriends.GetPersonaName();
                taskCompletionSource.SetResult(("3033139560125578", "Speecil")); // i'm you now until you fix your references
            }
            else
            {
                Oculus.Platform.Users.GetLoggedInUser().OnComplete(user => taskCompletionSource.SetResult((user.Data.ID.ToString(), user.Data.OculusID)));
            }

            return taskCompletionSource.Task;
        }

        private async void GetPatreonStatusAsync(Action<bool, string, string> callback)
        {
            (string playerID, string username) = await GetPlayerInfo();
            string patronListUrl = "https://raw.githubusercontent.com/speecil/Patrons/main/patrons.txt";

            string timestamp = DateTime.UtcNow.Ticks.ToString();
            patronListUrl += "?timestamp=" + timestamp;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue
            {
                NoCache = true
            };

            string patronList = await httpClient.GetStringAsync(patronListUrl);
            string[] patrons = patronList.Split(',');
            bool isPatron = patrons.Contains(playerID);

            await UnityMainThreadTaskScheduler.Factory.StartNew(() => callback(isPatron, playerID, username));
        }




        public void GetPatreonStatus(Action<bool, string, string> callback) => Task.Run(() => GetPatreonStatusAsync(callback));
    }
}
