using HarmonyLib;
using Verse;
using Verse.AI;

namespace Hospitality.Harmony {
    /// <summary>
    /// Make sure things outside the guest zone are not reachable for guests
    /// </summary>
    public class Reachability_Patch
    {
        [HarmonyPatch(typeof(Reachability), "CanReach", typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms))]
        public class CanReach
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, LocalTargetInfo dest, TraverseParms traverseParams)
            {
                if (!__result) return;

                // Cheaper to check this before IsGuest
                var area = traverseParams.pawn.GetGuestArea();
                if (area == null) return;
                if (!traverseParams.pawn.IsArrivedGuest()) return;

                if (!dest.IsValid || !area[dest.Cell]) __result = false;

                //Log.Message($"Guest {traverseParams.pawn.LabelShort} tried to traverse to {dest.Cell}. This was {(__result ? "allowed" : "not allowed")}");
            }
        }
    }
}