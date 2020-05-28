
using System.Linq;
using HugsLib.Utils;
using MorePlanning.Settings;
using UnityEngine;
using Verse;

namespace MorePlanning.Legacy
{
    class UpdateLegacy
    {
        public static void Update()
        {
            UpdateTo_4_1_0();
        }

        private static void UpdateTo_4_1_0()
        {
            #region Update the old world setting object from MorePlanning.MorePlanningMod.PlanningDataStore to MorePlanning.Settings.WorldSettings
            var oldWorldSettings = (MorePlanningMod.PlanningDataStore)Find.WorldObjects.ObjectsAt(0).FirstOrDefault(o => o is MorePlanningMod.PlanningDataStore);
            if (oldWorldSettings != null)
            {
                var newSettings = (WorldSettings)Find.WorldObjects.ObjectsAt(0).FirstOrDefault(o => o is WorldSettings);
                if (newSettings != null)
                {
                    newSettings.PlanningVisibility = oldWorldSettings.PlanningVisibility;
                }
                
                Find.WorldObjects.Remove(oldWorldSettings);
            }
            #endregion
        }
    }
}

namespace MorePlanning
{
    internal partial class MorePlanningMod
    {
        public class PlanningDataStore : UtilityWorldObject
        {
            public bool PlanningVisibility;

            public override void PostAdd()
            {
                base.PostAdd();
                PlanningVisibility = true;
            }

            public override void ExposeData()
            {
                base.ExposeData();
                Scribe_Values.Look(ref PlanningVisibility, "planningVisibility", true);
            }
        }
    }
}

