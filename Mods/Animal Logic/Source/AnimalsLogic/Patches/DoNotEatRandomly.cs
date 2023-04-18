using HarmonyLib;
using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace AnimalsLogic
{
    /*
     * Prevents animals from eating random stuff.
     */

    class DoNotEatRandomly
    {
        [HarmonyPatch(typeof(JobGiver_EatRandom), "TryGiveJob", new Type[] { typeof(Pawn) })]
        static class JobGiver_EatRandom_TryGiveJob_Patch
        {
            static bool Prefix(ref Job __result, Pawn pawn)
            {
                if (pawn.RaceProps.Animal && Settings.prevent_eating_stuff)
                {
                    __result = null;
                    return false;
                }
                return true;
            }
        }
    }
}
