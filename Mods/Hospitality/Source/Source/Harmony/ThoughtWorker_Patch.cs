using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    /// <summary>
    /// Added that guests can have this thought
    /// </summary>
    public static class ThoughtWorker_Patch
    {
        /// <summary>
        /// Against rare error, when guest's ownership.OwnedBed == null 
        /// </summary>
        [HarmonyPatch(typeof(ThoughtWorker_PrisonBarracksImpressiveness), "CurrentStateInternal")]
        public class PrisonBarracksImpressiveness
        {
            [HarmonyPrefix]
            public static bool CurrentStateInternal(ref ThoughtState __result, Pawn p)
            {
                if (p?.ownership?.OwnedBed == null)
                {
                    __result = ThoughtState.Inactive;
                    return false;
                }
                return true;
            }
        }
        /// <summary>
        /// Against rare error, when guest's ownership.OwnedBed == null 
        /// </summary>
        [HarmonyPatch(typeof(ThoughtWorker_PrisonCellImpressiveness), "CurrentStateInternal")]
        public class PrisonCellImpressiveness
        {
            [HarmonyPrefix]
            public static bool CurrentStateInternal(ref ThoughtState __result, Pawn p)
            {
                if (p?.ownership?.OwnedBed == null)
                {
                    __result = ThoughtState.Inactive;
                    return false;
                }
                return true;
            }
        }
    }
}