using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace SK_Enviro.AI
{
    public static class Animals_AI
    {
        public static JobDef GetEatMeatJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("AnimalsEat");
        }

        public static JobDef GetEatCorpseJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("AnimalEatCorpse");
        }

        public static JobDef GetHuntForAnimalsJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("HuntForAnimals");
        }

        public static JobDef GetBashDoorJobDef()
        {
            return DefDatabase<JobDef>.GetNamed("AnimalBashDoor");
        }

        public static IEnumerable<Thing> ButcherCorpseProducts(Corpse corpse, Pawn butcher)
        {
            if (corpse.def.butcherProducts != null)
            {
                IEnumerator<Thing> butchEnumerator = corpse.innerPawn.ButcherProducts(butcher, 1f).GetEnumerator();
                try
                {
                    while (butchEnumerator.MoveNext())
                    {
                        yield return butchEnumerator.Current;
                    }
                }
                finally
                {
                    butchEnumerator.Dispose();
                }
            }
            else

                if (corpse.innerPawn.RaceProps.isFlesh)
                {
                    FilthMaker.MakeFilth(butcher.Position, ThingDefOf.FilthBlood, corpse.innerPawn.LabelCap);
                }

            if (corpse.innerPawn.RaceProps.meatDef != null)
            {
                FilthMaker.MakeFilth(butcher.Position, ThingDefOf.FilthBlood, corpse.innerPawn.LabelCap);
                int meatCount = GenMath.RoundRandom(corpse.innerPawn.GetStatValue(StatDefOf.MeatAmount, true) * 0.2f);

                if (meatCount > 0)
                {
                    Thing meat = ThingMaker.MakeThing(ThingDef.Named("GuttedCorpse"));
                    if (meat != null)
                    {
                        meat.stackCount = meatCount;
                        yield
                        return meat;
                    }
                }
            }

            if (corpse.innerPawn.def.race.leatherDef != null)
            {
                int LeatherCount = GenMath.RoundRandom(corpse.innerPawn.GetStatValue(StatDefOf.LeatherAmount, true) * 0f);
                if (LeatherCount > 0)
                {
                    Thing leather = ThingMaker.MakeThing(corpse.innerPawn.def.race.leatherDef, null);
                    if (leather != null)
                    {
                        leather.stackCount = LeatherCount;
                        yield
                        return leather;
                    }
                }
            }
        }

        public static void Ingested(Thing ingested, Pawn ingester, float nutritionWanted)
        {
            if (!ingested.IngestibleNow)
            {
                Log.Error(ingester + " ingested IngestibleNow=false thing " + ingested);
            }
            else
            {
                int count = Mathf.CeilToInt(nutritionWanted / ingested.def.ingestible.nutrition);
                int[] values = new int[] { count, ingested.def.ingestible.maxNumToIngestAtOnce, ingested.stackCount };
                count = Mathf.Max(Mathf.Min(values), 1);
                if (count >= ingested.stackCount)
                {
                    count = ingested.stackCount;
                    ingested.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    ingested.SplitOff(count);
                }
                ingester.needs.food.CurLevel += count * ingested.def.ingestible.nutrition;
                if (ingester.needs.joy != null)
                {
                    JoyKindDef joyKind = (ingested.def.ingestible.joyKind == null) ? JoyKindDefOf.Gluttonous : ingested.def.ingestible.joyKind;
                    ingester.needs.joy.GainJoy(count * ingested.def.ingestible.joy, joyKind);
                }
                ingested.def.ingestible.Worker.IngestedBy(ingester, ingested, count);
            }
        }

        public static Toil FinalizeEatForAnimals(Pawn ingester, TargetIndex ingestibleInd)
        {
            Toil ingest = new Toil() { defaultCompleteMode = ToilCompleteMode.Instant };
            ingest.initAction = new Action(() =>
            {
                Thing ingested = ingest.actor.jobs.curJob.GetTarget(ingestibleInd).Thing;
                float nutritionWanted = 1f - ingester.needs.food.CurLevel;
                Ingested(ingested, ingester, nutritionWanted);
            });
            return ingest;
        }

        public static void StripDeteriorate(this Corpse corpse)
        {
            foreach (Apparel apparel in corpse.innerPawn.apparel.WornApparel)
            {
                int amount = Mathf.RoundToInt(apparel.HitPoints * Rand.Range((float)0.25f, (float)0.75f));
                apparel.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, amount, null, null, null));
            }
            corpse.Strip();
        }
    }
}