using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
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
                return AccessTools.Method(typeof(Pawn_ApparelTracker), "TryDrop", new[] {typeof(Apparel), typeof(Apparel).MakeByRefType(), typeof(IntVec3), typeof(bool)});
            }

            [HarmonyPrefix]
            public static bool Replacement(Pawn_ApparelTracker __instance, ref bool __result, Apparel ap, ref Apparel resultingAp, ThingOwner<Apparel> ___wornApparel)
            {
                if (!__instance.pawn.IsGuest()) return true;

                __result = ___wornApparel.TryTransferToContainer(ap, __instance.pawn.inventory.innerContainer);
                resultingAp = ap;
                return false;

                //List<Apparel> wornApparel = __instance.pawn.apparel.WornApparel;
                //__result = true;
                //for (int i = wornApparel.Count - 1; i >= 0; i--)
                //{
                //    if (!ApparelUtility.CanWearTogether(ap.def, wornApparel[i].def, __instance.pawn.RaceProps.body))
                //    {
                //        var apparel = wornApparel[i];
                //        __instance.pawn.apparel.Remove(apparel);
                //        if (__instance.pawn.inventory.innerContainer.TryAdd(apparel))
                //        {
                //            Log.Message(__instance.pawn.Name.ToStringShort + " should have taken " + apparel.Label + " to his inventory.");
                //            __instance.pawn.apparel.Notify_ApparelRemoved(apparel);
                //            resultingAp = apparel;
                //        }
                //        else
                //        {
                //            Log.Error(__instance.pawn + " could not add to inventory " + apparel.ToStringSafe());
                //            resultingAp = apparel;
                //        }
                //        break;
                //    }
                //}
                //return false;
            }
        }
    }
}