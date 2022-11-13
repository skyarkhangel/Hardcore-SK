using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RocketMan;
using Verse;

namespace Soyuz
{
    public static partial class SoyuzSettingsUtility
    {
        private static readonly HashSet<ThingDef> processedDefs = new HashSet<ThingDef>();

        private static readonly HashSet<JobDef> processedJobDefs = new HashSet<JobDef>();

        [Main.OnScribe]
        public static void OnScribe()
        {
            Scribe_Deep.Look(ref Context.Settings, "soyuzSettings_NewTemp");
            if (Context.Settings == null)
            {
                Context.Settings = new SoyuzSettings();
            }
        }

        [Main.OnSettingsScribedLoaded]
        public static void OnSettingsScribedLoaded()
        {
            PrepareRaceSettings();
            PrepareJobSettings();
        }

        private static void PrepareRaceSettings()
        {
            Context.Settings.AllRaceSettings = Context.Settings.AllRaceSettings
                .AsParallel()
                .Where(s => s?.def != null).ToList();
            foreach (RaceSettings settings in Context.Settings.AllRaceSettings)
            {
                processedDefs.Add(settings.def);
            }
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs
                .AsParallel()
                .Where(d => d.race != null && !processedDefs.Contains(d)))
            {
                processedDefs.Add(def);
                bool disabled = def.thingClass != typeof(Pawn);
                Context.Settings.AllRaceSettings.Add(new RaceSettings(def)
                {
                    enabled = def.race.Animal
                        && !disabled
                        && !def.race.Humanlike
                        && !def.race.IsMechanoid
                        && !IgnoreMeDatabase.ShouldIgnore(def),
                    ignoreFactions = false
                });
            }
            foreach (RaceSettings settings in Context.Settings.AllRaceSettings)
            {
                settings.Prepare();
            }
        }

        private static void PrepareJobSettings()
        {
            Context.Settings.AllJobsSettings = Context.Settings.AllJobsSettings
                .AsParallel()
                .Where(s => s?.def != null).ToList();
            foreach (JobSettings settings in Context.Settings.AllJobsSettings)
            {
                processedJobDefs.Add(settings.def);
            }
            foreach (JobDef def in DefDatabase<JobDef>.AllDefs
                .AsParallel()
                .Where(d => !processedJobDefs.Contains(d)))
            {
                processedJobDefs.Add(def);

                Context.Settings.AllJobsSettings.Add(new JobSettings(def));
            }
            Context.Settings.AllJobsSettings = Context.Settings.AllJobsSettings
                .AsParallel()
                .Where(s => s?.def != null).ToList();
            foreach (JobSettings settings in Context.Settings.AllJobsSettings)
            {
                settings.Prepare();
            }
        }
    }
}