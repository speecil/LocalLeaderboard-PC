using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BeatLeader.Models;
using ModestTree;
using UnityEngine;

namespace LocalLeaderboard.Utils
{
    internal class TimeUtils
    {

        // used from beatleader, https://github.com/BeatLeader/beatleader-mod/blob/9b151a170a9975da220806e06db12efeac1821cc/Source/7_Utils/StaticUtils/FormatUtils.cs#L203

        private const int Second = 1;
        private const int Minute = 60 * Second;
        private const int Hour = 60 * Minute;
        private const int Day = 24 * Hour;
        private const int Month = 30 * Day;

        public static TimeSpan GetRelativeTime(string timeSet)
        {
            var dateTime = AsUnixTime(long.Parse(timeSet));
            return DateTime.UtcNow - dateTime;
        }

        public static string GetRelativeTimeString(string timeSet)
        {
            return GetRelativeTimeString(GetRelativeTime(timeSet));
        }

        public static string GetRelativeTimeString(TimeSpan timeSpan)
        {
            switch (timeSpan.TotalSeconds)
            {
                case < 0: return "-";
                case < 1 * Minute: return timeSpan.Seconds == 1 ? "1 second ago" : timeSpan.Seconds + " seconds ago";
                case < 2 * Minute: return "1 minute ago";
                case < 1 * Hour: return timeSpan.Minutes + " minutes ago";
                case < 2 * Hour: return "1 hour ago";
                case < 24 * Hour: return timeSpan.Hours + " hours ago";
                case < 2 * Day: return "Yesterday";
                case < 30 * Day: return timeSpan.Days + " days ago";
                case < 12 * Month:
                    {
                        var months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
                        return months <= 1 ? "1 month ago" : months + " months ago";
                    }
                default:
                    {
                        var years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
                        return years <= 1 ? "1 year ago" : years + " years ago";
                    }
            }
        }
        public static DateTime AsUnixTime(long longvalue)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixEpoch.AddSeconds(longvalue);
        }
    }
}
