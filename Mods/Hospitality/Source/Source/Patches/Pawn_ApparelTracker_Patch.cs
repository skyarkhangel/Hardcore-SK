using System.Reflection;
using HarmonyLib;
using Hospitality.Utilities;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    /// <summary>
    /// So guests will put apparel in their inventory that they would otherwise drop
    /// </summary>
    public static class Pawn_ApparelTracker_Patch
    {
        [HarmonyPatch]
        public class TryDrop
        {
            // Targeting specific overload with ref!
            [UsedImplicitly]
            public static MethodBase TargetMethod()
            {
                return AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.TryDrop), new[] {typeof(Apparel), typeof(Apparel).MakeByRefType(), typeof(IntVec3), typeof(bool)});
            }

            [HarmonyPrefix]
            public static bool Replacement(Pawn_ApparelTracker __instance, ref bool __result, Apparel ap, ref Apparel resultingAp, ThingOwner<Apparel> ___wornApparel)
            {
                if (!__instance.pawn.IsGuest()) return true;

                __result = ___wornApparel.TryTransferToContainer(ap, __instance.pawn.inventory.innerContainer);
                resultingAp = ap;
                return false;
            }
        }
    }
}