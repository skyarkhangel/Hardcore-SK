using HarmonyLib;
using SimpleSidearms.rimworld;
using System;
using Verse;

using static PeteTimesSix.SimpleSidearms.SimpleSidearms;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{
    [HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
    public static class PawnGenerator_GenerateGearFor_Postfix
    {
        [HarmonyPostfix]
        public static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
        {
            try { 
                //Log.Message("generating sidearms for " + pawn.Label);
                float modifiedChance = Settings.SidearmSpawnChance;
                float modifiedBudgetMultiplier = Settings.SidearmBudgetMultiplier;
                bool more = true;
                int sanityLimiter = 0;

                while (more && modifiedChance > 0 && modifiedBudgetMultiplier > 0 && sanityLimiter < 10)
                {
                    sanityLimiter++;
                    //Log.Message("generating sidearm number " + sanityLimiter + " chance: "+modifiedChance+" budgetMult:"+modifiedBudgetMultiplier);
                    more = PawnSidearmsGenerator.TryGenerateSidearmFor(pawn, modifiedChance, modifiedBudgetMultiplier, request);
                    modifiedChance -= Settings.SidearmSpawnChanceDropoff;
                    modifiedBudgetMultiplier -= Settings.SidearmBudgetDropoff;
                }
            }
            catch(Exception e) 
            {
                Log.Error("Exception during pawn gear generation intercept. Cancelling intercept. Exception: " + e.ToString());
            }

        }
    }
}
