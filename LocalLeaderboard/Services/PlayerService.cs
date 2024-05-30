﻿using IPA.Utilities.Async;
using LocalLeaderboard.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
namespace LocalLeaderboard.Services
{
    internal class PlayerService
    {
        internal string userName;
        public (string, string) OculusSkillIssue()
        {
            string steamID = Steamworks.SteamUser.GetSteamID().ToString();
            string steamName = Steamworks.SteamFriends.GetPersonaName();
            return (steamID, steamName);
        }

        public Task<(string, string)> GetPlayerInfo()
        {
            TaskCompletionSource<(string, string)> taskCompletionSource = new();
            if (File.Exists(Constants.STEAM_API_PATH))
            {
                (string steamID, string steamName) = OculusSkillIssue();
                taskCompletionSource.SetResult((steamID, steamName));
            }
            else
            {
                Oculus.Platform.Users.GetLoggedInUser().OnComplete(user => taskCompletionSource.SetResult((user.Data.ID.ToString(), user.Data.OculusID)));
            }
            return taskCompletionSource.Task;
        }

        private async void GetUserNameStatus(Action<bool, string> callback)
        {
            (string playerID, string playerName) = await GetPlayerInfo();
            const bool MANUALCHANGELMFAO = false;
            await UnityMainThreadTaskScheduler.Factory.StartNew(() => callback(MANUALCHANGELMFAO, playerName));
        }
        public void GetPatreonStatus(Action<bool, string> callback) => Task.Run(() => GetUserNameStatus(callback));
    }
}
