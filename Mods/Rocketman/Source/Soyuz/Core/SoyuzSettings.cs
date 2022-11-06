using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using HarmonyLib;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz
{

    public class RaceSettings : IExposable
    {
        public ThingDef def;

        public bool enabled = true;

        public bool ignoreFactions = false;

        public bool ignorePlayerFaction = false;

        public bool isFastMoving = false;

        private int version;

        private const int SETTINGS_VERSION = 1;

        public int DilationInt
        {
            get
            {
                int val = 0;
                if (enabled) val = val | 1;
                if (ignoreFactions) val = val | 2;
                if (ignorePlayerFaction) val = val | 4;
                return val;
            }
        }

        public RaceSettings()
        {
        }

        public RaceSettings(ThingDef def)
        {
            this.def = def;
            this.version = SETTINGS_VERSION;
            if (this.def.StatBaseDefined(StatDefOf.MoveSpeed))
            {
                this.isFastMoving = this.def.GetStatValueAbstract(StatDefOf.MoveSpeed) > 8;
            }
        }

        public void ExposeData()
        {
            try
            {
                Scribe_Defs.Look(ref def, "pawnDef");
            }
            finally
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
                Scribe_Values.Look(ref version, "version", -1);
                Scribe_Values.Look(ref ignoreFactions, "ignoreFactions");
                Scribe_Values.Look(ref ignorePlayerFaction, "ignorePlayerFaction");
                Scribe_Values.Look(ref isFastMoving, "isFastMoving", false);
            }
            if (this.version != SETTINGS_VERSION)
            {
                this.Notify_VersionChanged();
                this.version = SETTINGS_VERSION;
            }
        }

        public void Prepare(bool updating = false)
        {
            Context.DilationEnabled[def.index] = this.enabled;
            if (!updating)
            {
                Context.DilationByDef[def] = this;
                Context.DilationFastMovingRace[def.index] = isFastMoving;
            }
        }

        private void Notify_VersionChanged()
        {
            if (this.def?.race?.IsMechanoid ?? false)
            {
                this.enabled = false;
                this.ignoreFactions = false;
                this.ignorePlayerFaction = false;
            }
        }
    }

    public class JobSettings : IExposable
    {
        private const int SETTINGS_VERSION = 2;

        private int version;

        public JobDef def;

        public JobThrottleMode throttleMode = JobThrottleMode.Full;

        public JobThrottleFilter throttleFilter = JobThrottleFilter.Animals;

        public JobSettings()
        {
        }

        public JobSettings(JobDef def)
        {
            this.def = def;
        }

        public void ExposeData()
        {
            try
            {
                Scribe_Defs.Look(ref def, "job");
            }
            finally
            {
                Scribe_Values.Look(ref version, "version", -1);
                Scribe_Values.Look(ref throttleMode, "throttleMode", JobThrottleMode.Full);
                Scribe_Values.Look(ref throttleFilter, "throttleFilter", JobThrottleFilter.Animals);
            }
            if (this.version != SETTINGS_VERSION)
            {
                this.Notify_VersionChanged();
            }
        }

        public void RestoreDefault()
        {
            throttleMode = JobThrottleMode.Full;
            throttleFilter = JobThrottleFilter.Animals;
        }

        public void Prepare(bool updating = false)
        {
            if (!updating && def != null)
            {
                Context.JobDilationByDef[def] = this;
            }
        }

        private void Notify_VersionChanged()
        {
            this.RestoreDefault();
            this.version = SETTINGS_VERSION;
        }
    }

    public class SoyuzSettings : IExposable
    {
        private bool fresh = true;

        public List<RaceSettings> AllRaceSettings = new List<RaceSettings>();

        public List<JobSettings> AllJobsSettings = new List<JobSettings>();

        public float dilationFactorOffscreen = 0.8f;

        public float dilationFactorOnscreen = 0.2f;

        public bool Fresh
        {
            get => fresh;
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && fresh)
            {
                fresh = false;
            }
            Scribe_Values.Look(ref fresh, "firstUse", true);
            Scribe_Collections.Look(ref AllRaceSettings, "AllRaceSettings_NewTemp", LookMode.Deep);
            Scribe_Collections.Look(ref AllJobsSettings, "AllJobsSettings", LookMode.Deep);
            if (AllRaceSettings == null)
            {
                AllRaceSettings = new List<RaceSettings>();
            }
            if (AllJobsSettings == null)
            { 
                AllJobsSettings = new List<JobSettings>();
            }            
            Scribe_Values.Look(ref dilationFactorOffscreen, "dilationFactorOffscreen", 0.6f);
            if (dilationFactorOffscreen < 0.6f)
                dilationFactorOffscreen = 0.6f;
            Scribe_Values.Look(ref dilationFactorOnscreen, "dilationFactorOnscreen", 0.2f);
            if (dilationFactorOnscreen < 0.2f)
                dilationFactorOnscreen = 0.2f;
        }
    }
}