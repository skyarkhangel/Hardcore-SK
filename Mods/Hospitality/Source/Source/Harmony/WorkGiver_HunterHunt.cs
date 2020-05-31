using HarmonyLib;
using Verse;

namespace Hospitality.Harmony {
    /// <summary>
    /// Don't let guests hunt outside their zone
    /// </summary>
    public class WorkGiver_HunterHunt
    {
        [HarmonyPatch(typeof(RimWorld.WorkGiver_HunterHunt), "HasJobOnThing", typeof(Pawn), typeof(Thing), typeof(bool))]
        public class HasJobOnThing
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn pawn, Thing t)
            {
                if (!__result) return;

                if (!pawn.IsArrivedGuest()) return;

                var area = pawn.GetGuestArea();
                if (area == null) return;
                if (!t.Position.IsValid || !area[t.Position]) __result = false;

                //Log.Message($"Guest {traverseParams.pawn.LabelShort} tried to traverse to {t.Label}. This was {(__result ? "allowed" : "not allowed")}");
            }
        }
    }
}