using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Prevent guests from overdosing
    /// </summary>
    public class JoyGiver_Ingest_Patch
    {
        [HarmonyPatch(typeof(JoyGiver_Ingest), "CanIngestForJoy")]
        public class CanIngestForJoy
        {
            [HarmonyPostfix]
            public static void Postfix(ref bool __result, Pawn pawn, Thing t)
            {
                if (!__result) return;

                // Is guest
                if (!pawn.IsGuest()) return;

                // It's a drug
                if (t.def.IsDrug)
                {
                    var properties = t.def.GetCompProperties<CompProperties_Drug>();

                    // It can cause overdose >> don't do it
                    if (properties?.CanCauseOverdose == true)
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}