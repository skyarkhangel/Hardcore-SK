using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SK_Enviro.Seeds
{
    internal class JobDriver_PlantHarvestWithSeeds : JobDriver_PlantHarvest
    {
        protected float harvestWorkDone;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrForbidden<JobDriver_PlantHarvestWithSeeds>(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            var toil = new Toil();
            toil.tickAction = () =>
            {
                Pawn actor = toil.actor;
                if (actor.skills != null)
                    actor.skills.Learn(SkillDefOf.Growing, 0.154f);
                float num = actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
                Plant plant = (Plant)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
                harvestWorkDone += num;
                if (harvestWorkDone >= plant.def.plant.harvestWork)
                {
                    if (plant.def.plant.harvestedThingDef != null)
                    {
                        int num2 = plant.YieldNow();
                        if (plant is PlantWithSeeds)
                            num2 = ((PlantWithSeeds)plant).YieldNow();
                        if (num2 > 0)
                        {
                            Thing t = ThingMaker.MakeThing(plant.def.plant.harvestedThingDef, null);
                            t.stackCount = num2;
                            if (actor.Faction != Faction.OfColony)
                                t.SetForbidden(true, true);
                            GenPlace.TryPlaceThing(t, actor.Position, ThingPlaceMode.Near);
                        }
                        if (plant is PlantWithSeeds)
                        {
                            PlantWithSeeds seeds = plant as PlantWithSeeds;
                            float num3 = Mathf.Max(Mathf.InverseLerp(seeds.def.plant.harvestMinGrowth, 1.2f, seeds.growth), 1f);
                            if (seeds.m_CustomPlantDef != null && seeds.m_CustomPlantDef.SeedDef != null && Rand.Value < (seeds.genome.baseSeedChance * num3))
                            {
                                Thing thing = ThingMaker.MakeThing(seeds.m_CustomPlantDef.SeedDef, null);
                                if (Rand.Value < (seeds.genome.addSeedChance * num3))
                                    thing.stackCount = 2;
                                else
                                    thing.stackCount = 1;
                                GenPlace.TryPlaceThing(thing, actor.Position, ThingPlaceMode.Near);
                            }
                        }
                    }
                    plant.PlantCollected();
                    plant.def.plant.soundHarvestFinish.PlayOneShot(actor);
                    harvestWorkDone = 0f;
                    ReadyForNextToil();
                }
            };
            toil.FailOnDestroyedOrForbidden<Toil>(TargetIndex.A);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect("Harvest", TargetIndex.A);
            toil.WithSustainer(() => toil.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing.def.plant.soundHarvesting);
            yield return toil;
            yield return Toils_General.RemoveDesignationsOnThing(TargetIndex.A, DesignationDefOf.HarvestPlant);
        }

    }
}

