using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace MAD
{
    public class PawnChanger
    {
        public static void ExecuteBadThings(Pawn pawn)
        {
            GiveMood(pawn, ThoughtDef.Named("Scrambled"));

            int rng = Rand.RangeInclusive(1, 100);
            if (rng <= 10)
            {
                pawn.health.AddHediff(HediffDef.Named("BadMigraine"), null, null);
            }
            else
                if (rng <= 20)
                {
                    pawn.health.AddHediff(HediffDef.Named("BodyRestart"), null, null);
                }
                else
                    if (rng <= 50)
                    {
                        pawn.health.AddHediff(HediffDef.Named("Dementia"), null, null);
                    }
                    else
                    {
                        pawn.health.AddHediff(HediffDef.Named("Braindeath"), null, null);;
                    }
        }

        public static void SetMood(Pawn pawn)
        {
            if (HasMood(pawn, ThoughtDef.Named("Wrecked")))
            {
                GiveMood(pawn, ThoughtDef.Named("Wrecked"));
                GiveMood(pawn, ThoughtDef.Named("Confused"));
                GiveMood(pawn, ThoughtDef.Named("Wrong"));
            }
            else
                if (HasMood(pawn, ThoughtDef.Named("Confused")))
                {
                    GiveMood(pawn, ThoughtDef.Named("Wrecked"));
                    GiveMood(pawn, ThoughtDef.Named("Confused"));
                    GiveMood(pawn, ThoughtDef.Named("Wrong"));
                }
                else
                    if (HasMood(pawn, ThoughtDef.Named("Wrong")))
                    {

                        GiveMood(pawn, ThoughtDef.Named("Confused"));
                        GiveMood(pawn, ThoughtDef.Named("Wrong"));
                    }
                    else
                    {
                        GiveMood(pawn, ThoughtDef.Named("Wrong"));
                    }
        }

        public static void GiveMood(Pawn pawn, ThoughtDef Tdef)
        {
            pawn.needs.mood.thoughts.TryGainThought(Tdef);
        }

        public static Boolean HasMood(Pawn pawn, ThoughtDef Tdef)
        {
            if (pawn.needs.mood.thoughts.DistinctThoughtDefs.Contains(Tdef))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SetPawnTraits(Pawn pawn, int num)
        {
            pawn.story.traits.allTraits.Clear();
            while (pawn.story.traits.allTraits.Count < num)
            {
                TraitDef newTraitDef = DefDatabase<TraitDef>.GetRandom();

                if (!pawn.story.traits.HasTrait(newTraitDef))
                {
                    if (newTraitDef.conflictingTraits == null || !Contains(pawn, newTraitDef.conflictingTraits))
                    {

                        Trait trait = new Trait(newTraitDef);
                        trait.degree = PawnGenerator.RandomTraitDegree(trait.def);

                        if (pawn.mindState.breaker.HardBreakThreshold + trait.OffsetOfStat(StatDefOf.MentalBreakThreshold) <= 40f)
                        {
                            pawn.story.traits.GainTrait(trait);
                        }
                    }
                }
            }
        }

        private static Boolean Contains(Pawn pPawnSel, List<TraitDef> lTraitDef)
        {

            TraitDef[] tArray = new TraitDef[lTraitDef.Count];

            lTraitDef.CopyTo(tArray, 0);

            for (var i = 0; i < tArray.Length; i++)
            {
                if (pPawnSel.story.traits.HasTrait(tArray[i]))
                {
                    return true;
                }
            }


            return false;

        }


        public static void BreakPawn (Pawn pawn)
        {
            MentalBreaker mB = pawn.mindState.breaker;
            
            //pawn.story.traits.allTraits

            

            pawn.mindState.broken.StartBrokenState(DefDatabase<BrokenStateDef>.GetNamed("Manhunter", true));



            //if (pawn.mindState.broken.CurState == null)
            //{
            //    if (pawn.story != null)
            //    {
            //        List<Trait> allTraits = pawn.story.traits.allTraits;
            //        for (int i = 0; i < allTraits.Count; i++)
            //        {
            //            TraitDegreeData currentData = allTraits[i].CurrentData;
            //            if (currentData.randomBreakState != null)
            //            {
            //                float mtb = currentData.randomBreakMtbDaysMoodCurve.Evaluate(CurMood(pawn));
            //                if (Rand.MTBEventOccurs(mtb, 30000f, 150f) && currentData.randomBreakState.Worker.StateCanOccur(pawn))
            //                {
            //                    pawn.mindState.broken.StartBrokenState(currentData.randomBreakState);
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private static float CurMood(Pawn pawn)
        {
            
                if (pawn.needs.mood == null)
                {
                    return 0.5f;
                }
                return pawn.needs.mood.CurLevel;
            
        }
        
    }
}
