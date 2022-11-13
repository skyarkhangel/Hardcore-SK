using HarmonyLib;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(ImmunityRecord), nameof(ImmunityRecord.ImmunityChangePerTick))]
    public class ImmunityRecord_ImmunityChangePerTick_Patch
    {
        public static void Postfix(ImmunityRecord __instance, ref float __result, Pawn pawn)
        {            
            if (pawn?.IsBeingThrottled() ?? false)
                __result *= pawn.GetTimeDelta();
        }
    }
}