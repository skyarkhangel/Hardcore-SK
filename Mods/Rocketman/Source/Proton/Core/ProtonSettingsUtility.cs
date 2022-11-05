using System;
using System.Collections.Generic;
using System.Linq;
using RocketMan;
using Verse;

namespace Proton
{
    public static class ProtonSettingsUtility
    {
        [Main.OnScribe]
        public static void OnScribe()
        {
            Scribe_Deep.Look(ref Context.settingsInt, "protonSettings");
            if (Context.settingsInt == null)
            {
                Context.settingsInt = new ProtonSettings();
            }
            RocketEnvironmentInfo.ProtonLoaded = true;
        }
    }
}
