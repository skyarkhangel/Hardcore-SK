using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using Verse;

namespace Proton
{
    public class ProtonSettings : IExposable
    {
        public float executionTimeLimit = 35f;
        public float minInterval = 2.5f;

        private List<AlertSettings> alertsSettings;

        public void ExposeData()
        {
            alertsSettings = Context.AlertSettingsByIndex?.ToList() ?? new List<AlertSettings>();

            Scribe_Values.Look(ref executionTimeLimit, "executionTimeLimit_NewTemp", 35);
            Scribe_Values.Look(ref minInterval, "minInterval_NewTemp", 2f);
            Scribe_Collections.Look(ref alertsSettings, "settings", LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.Saving)
            {
                if (alertsSettings == null)
                {
                    alertsSettings = new List<AlertSettings>();
                }
                foreach (var s in alertsSettings)
                {
                    Context.TypeIdToSettings[s.typeId] = s;
                }
            }
        }
    }
}
