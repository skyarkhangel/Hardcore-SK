using HarmonyLib;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Make filters for beds also accept guest beds
    /// </summary>
    public class ThingFilter_Patch
    {
        [HarmonyPatch(typeof(ThingFilter), "Allows", typeof(ThingDef))]
        public class Allows
        {
            [HarmonyPrefix]
            public static bool Prefix(ref ThingDef def)
            {
                // Is from a guest bed?
                if (def?.defName != null && def.thingClass == typeof(Building_GuestBed))
                {
                    var bedDef = DefDatabase<ThingDef>.GetNamed(def.defName.Substring(0, def.defName.Length - 5)); // remove "Guest" from name
                    // Use def of original bed instead
                    if (bedDef != null) def = bedDef;
                }

                // Business as usual
                return true;
            }
        }
    }
}
