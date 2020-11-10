using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CommonSense
{
    class DrugPolicy
    {

        [HarmonyPatch(typeof(Pawn_DrugPolicyTracker), "AllowedToTakeScheduledNow", new Type[] { typeof(ThingDef) })]
        static class Pawn_DrugPolicyTracker_AllowedToTakeScheduledNow_CommonSensePatch
        {
            static bool Prefix(ref bool __result, ref Pawn_DrugPolicyTracker __instance, ref ThingDef thingDef)
            {
                if (!Settings.drugs_use_potential_mood)
                    return true;
                //i've failed trying to inject "actually changed part" in exact part of a code (result looked too monstrous to me)
                //so I've used a simple solution
                //
                //duplicating initial part
                if (!thingDef.IsIngestible)
                {
                    Log.Error(thingDef + " is not ingestible.");
                    __result = false;
                    return false;
                }
                if (!thingDef.IsDrug)
                {
                    Log.Error("AllowedToTakeScheduledEver on non-drug " + thingDef);
                    __result = false;
                    return false;
                }
                if (!__instance.AllowedToTakeScheduledEver(thingDef))
                {
                    __result = false;
                    return false;
                }
                DrugPolicyEntry drugPolicyEntry = __instance.CurrentPolicy[thingDef];
                
                //actually changed part
                if (drugPolicyEntry.onlyIfMoodBelow < 1f && __instance.pawn.needs.mood != null 
                    && (__instance.pawn.needs.mood.CurLevelPercentage >= drugPolicyEntry.onlyIfMoodBelow || __instance.pawn.needs.mood.CurInstantLevelPercentage >= drugPolicyEntry.onlyIfMoodBelow))
                {
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}
