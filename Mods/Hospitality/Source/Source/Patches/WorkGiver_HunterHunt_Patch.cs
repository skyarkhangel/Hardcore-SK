using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches 
{
    /// <summary>
    /// Don't let guests hunt outside their zone
    /// </summary>
    public class WorkGiver_HunterHunt_Patch
    {
        [HarmonyPatch(typeof(WorkGiver_HunterHunt), nameof(WorkGiver_HunterHunt.HasJobOnThing), typeof(Pawn), typeof(Thing), typeof(bool))]
        public class HasJobOnThing
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn pawn, Thing t)
            {
                if (!__result) return;
                if (!pawn.IsArrivedGuest(out var guestComp)) return;

                var area = guestComp.GuestArea;
                if (area == null) return;
                if (!t.Position.IsValid || !area[t.Position]) __result = false;

                //Log.Message($"Guest {traverseParams.pawn.LabelShort} tried to traverse to {t.Label}. This was {(__result ? "allowed" : "not allowed")}");
            }
        }
    }
}