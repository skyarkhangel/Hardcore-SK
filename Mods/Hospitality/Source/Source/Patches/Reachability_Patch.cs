using HarmonyLib;
using Hospitality.Utilities;
using Verse;
using Verse.AI;

namespace Hospitality.Patches 
{
    /// <summary>
    /// Make sure things outside the guest zone are not reachable for guests
    /// </summary>
    public class Reachability_Patch
    {
        [HarmonyPatch(typeof(Reachability), nameof(Reachability.CanReach), typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms))]
        public class CanReach
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, LocalTargetInfo dest, TraverseParms traverseParams)
            {
                if (!__result) return;

                if (!traverseParams.pawn.IsArrivedGuest(out var guestComp)) return;

                var area = guestComp.GuestArea;
                if (area == null) return;

                if (!dest.IsValid || !area[dest.Cell]) __result = false;

                //Log.Message($"Guest {traverseParams.pawn.LabelShort} tried to traverse to {dest.Cell}. This was {(__result ? "allowed" : "not allowed")}");
            }
        }
    }
}