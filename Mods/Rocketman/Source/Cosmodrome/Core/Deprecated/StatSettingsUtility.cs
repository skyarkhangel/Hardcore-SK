using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace RocketMan
{
    public static class StatSettingsUtility
    {
        private static HashSet<StatDef> processedDefs = new HashSet<StatDef>();

        [Main.OnScribe]
        public static void OnScribe()
        {
            Scribe_Deep.Look(ref Finder.StatSettings, "StatSettings");

            if (Finder.StatSettings == null)
            {
                Finder.StatSettings = new StatSettingsGroup();
            }
        }

        [Main.OnSettingsScribedLoaded]
        public static void OnSettingsScribedLoaded()
        {
            Finder.StatSettings.AllSettings = Finder.StatSettings.AllSettings
                .AsParallel()
                .Where(s => s != null && s.statDef != null).ToList();
            foreach (StatSettings settings in Finder.StatSettings.AllSettings)
            {
                processedDefs.Add(settings.statDef);
            }
            foreach (StatDef statDef in DefDatabase<StatDef>.AllDefs.Where(s => !processedDefs.Contains(s)))
            {
                StatSettings settings = new StatSettings(statDef);
                Finder.StatSettings.AllSettings.Add(settings);
            }
            foreach (StatSettings settings in Finder.StatSettings.AllSettings)
            {
                settings.Prepare();
            }
        }
    }
}
