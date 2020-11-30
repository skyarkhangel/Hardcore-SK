using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CommonSense
{
    class WorkGiver_ClearSnow_Patch
    {
        [HarmonyPatch(typeof(WorkGiver_ClearSnow), "ShouldSkip")]
        static class WorkGiver_ClearSnow_ShouldSkip_CommonSensePatch
        {
            static bool Prefix(ref bool __result, Pawn pawn)
            {
                //Log.Message($"skip={Settings.skip_snow_clean},SnowRate={pawn.Map.weatherManager.SnowRate},RainRate={pawn.Map.weatherManager.RainRate}");
                if (!Settings.skip_snow_clean || pawn.Map.weatherManager.SnowRate == 0 && pawn.Map.weatherManager.RainRate == 0)
                    return true;

                __result = true;
                return false;
            }
        }
    }
}
