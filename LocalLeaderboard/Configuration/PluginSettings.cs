using IPA.Config.Stores;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace LocalLeaderboard
{
    internal class SettingsConfig
    {
        public static SettingsConfig Instance { get; set; }

        public bool BurgerDate = false;

        public virtual void Changed() => ApplyValues();

        public void ApplyValues()
        {
            
        }
    }
}
