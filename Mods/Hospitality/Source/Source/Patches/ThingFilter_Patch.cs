using HarmonyLib;
using System.Collections.Generic;
using Verse;

namespace Hospitality.Patches
{
    /// <summary>
    /// Make filters for beds also accept guest beds
    /// </summary>
    public class ThingFilter_Patch
    {
        [HarmonyPatch(typeof(ThingFilter), nameof(ThingFilter.Allows), typeof(ThingDef))]
        public class Allows
        {
            private static Dictionary<string, ThingDef> bedNameToThing = new Dictionary<string, ThingDef>();
            [HarmonyPrefix]
            public static bool Prefix(ref ThingDef def)
            {
                // Is from a guest bed?
                if (def?.defName == null || def.thingClass != typeof(Building_GuestBed)) return true;


                if (!bedNameToThing.TryGetValue(def.defName, out var bedDef))
                {
                    bedDef = DefDatabase<ThingDef>.GetNamed(def.defName.Substring(0, def.defName.Length - 5)); // remove "Guest" from name
                    bedNameToThing.Add(def.defName, bedDef); // we don't care if its null, just trying to avoid the string manipulation and DefDatabase lookups
                }

                // Use def of original bed instead
                if (bedDef != null)
                    def = bedDef;

                // Business as usual
                return true;
            }
        }
    }
}