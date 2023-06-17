using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace LocalLeaderboard
{
    public class SettingsConfig
    {
        public static SettingsConfig Instance { get; set; }

        public bool BurgerDate = false;

        public bool rainbowsuwu = false;

        public bool nameHeaderToggle = false;

        public bool useRelativeTime = false;
    }
}
