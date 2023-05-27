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
        public Task<string> GetPlayerName()
        {
            TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>();

            if (File.Exists(Path.Combine(UnityGame.InstallPath, "Beat Saber_Data", "Plugins", "x86_64", "steam_api64.dll")))
            {
                //Steamworks.SteamFriends.GetPersonaName();
                taskCompletionSource.SetResult("speecil"); // i'm you now until you fix your references
            }
            else
            {
                Oculus.Platform.Users.GetLoggedInUser().OnComplete(user => taskCompletionSource.SetResult(user.Data.OculusID));
            }

            return taskCompletionSource.Task;
        }

        public void GetPatreonStatus(Action<bool, string> callback)
        {
            new Thread(async () =>
            {
                string playerName = await GetPlayerName();
                string patronListUrl = "https://raw.githubusercontent.com/speecil/Patrons/main/patrons.txt";
                string patronList = await new HttpClient().GetStringAsync(patronListUrl);
                string[] patrons = patronList.Split(',');
                bool isPatron = patrons.Contains(playerName);
                await UnityMainThreadTaskScheduler.Factory.StartNew(() => callback(isPatron, playerName));
            }).Start();
        }
    }
}
