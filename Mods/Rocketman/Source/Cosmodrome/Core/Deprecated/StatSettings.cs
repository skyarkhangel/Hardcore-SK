using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RocketMan
{
    public class StatSettings : IExposable
    {
        public StatDef statDef;

        public float expiryAfter;

        const int SETTINGS_VERSION = 1;

        private int version = -1;

        public StatSettings()
        {
        }

        public StatSettings(StatDef statDef)
        {
            this.statDef = statDef;
            this.expiryAfter = Tools.PredictStatExpiryFromString(statDef.defName);
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && statDef != null)
            {
                Resolve();
            }
            try
            {
                Scribe_Defs.Look(ref statDef, "statDef");
            }
            finally
            {
                Scribe_Values.Look(ref expiryAfter, "expiryAfter");
                Scribe_Values.Look(ref version, "version", -1);
                if (version != SETTINGS_VERSION)
                    Notify_VersionChanged();
            }
        }

        public void Notify_VersionChanged()
        {
            version = SETTINGS_VERSION;
            expiryAfter = statDef?.label?.PredictStatExpiryFromString() ?? 240;
        }

        public void Prepare()
        {
            RocketStates.StatExpiry[statDef.index] = this.expiryAfter;
        }

        public void Resolve()
        {
            this.expiryAfter = RocketStates.StatExpiry[statDef.index];
        }
    }

    public class StatSettingsGroup : IExposable
    {
        public List<StatSettings> AllSettings = new List<StatSettings>();

        public void ExposeData()
        {
            Scribe_Collections.Look(ref AllSettings, "AllSettings", LookMode.Deep);

            if (AllSettings == null)
            {
                AllSettings = new List<StatSettings>();
            }
        }
    }
}
