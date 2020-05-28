using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SeedsPlease
{
    public abstract class JobDriver_PlantWorkWithSeeds : JobDriver_PlantWork
    {
        const TargetIndex targetCellIndex = TargetIndex.A;

        float workDone;

        protected override IEnumerable<Toil> MakeNewToils ()
        {
            yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue (targetCellIndex);
            yield return Toils_Reserve.ReserveQueue (targetCellIndex);

            var init = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets (targetCellIndex);

            yield return init;
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue (targetCellIndex);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue (targetCellIndex);

            var clear = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets (targetCellIndex);
            yield return Toils_Goto.GotoThing (targetCellIndex, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden (targetCellIndex, clear);

            yield return HarvestSeedsToil ();
            yield return PlantWorkDoneToil ();
            yield return Toils_Jump.JumpIfHaveTargetInQueue (targetCellIndex, init);
        }

        Toil HarvestSeedsToil ()
        {
            var toil = new Toil ();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = Init;
            toil.tickAction = delegate {
                var actor = toil.actor;
                var plant = Plant;

                if (actor.skills != null) {
                    actor.skills.Learn (SkillDefOf.Plants, xpPerTick);
                }

                workDone += actor.GetStatValue (StatDefOf.PlantWorkSpeed, true);
                if (workDone >= plant.def.plant.harvestWork) {
                    if (plant.def.plant.harvestedThingDef != null) {
                        if (actor.RaceProps.Humanlike && plant.def.plant.harvestFailable && Rand.Value > actor.GetStatValue (StatDefOf.PlantHarvestYield, true)) {
                            MoteMaker.ThrowText ((actor.DrawPos + plant.DrawPos) / 2f, actor.Map, ResourceBank.StringTextMoteHarvestFailed, 3.65f);
                        } else {
                            int plantYield = plant.YieldNow ();

                            ThingDef harvestedThingDef;

                            if (plant.def.blueprintDef is SeedDef seedDef && !seedDef.thingCategories.NullOrEmpty() ) {
                                var minGrowth = plant.def.plant.harvestMinGrowth;

                                float parameter;
                                if (minGrowth < 0.9f) {
                                    parameter = Mathf.InverseLerp (minGrowth, 0.9f, plant.Growth);
                                } else if (minGrowth < plant.Growth) {
                                    parameter = 1f;
                                } else {
                                    parameter = 0f;
                                }
                                parameter = Mathf.Min (parameter, 1f);

                                if (seedDef.seed.seedFactor > 0 && Rand.Value < seedDef.seed.baseChance * parameter) {
                                    int count;
                                    if (Rand.Value < seedDef.seed.extraChance) {
                                        count = 2;
                                    } else {
                                        count = 1;
                                    }

                                    Thing seeds = ThingMaker.MakeThing (seedDef, null);
                                    seeds.stackCount = Mathf.RoundToInt (seedDef.seed.seedFactor * count);

                                    GenPlace.TryPlaceThing (seeds, actor.Position, actor.Map, ThingPlaceMode.Near);
                                }

                                plantYield = Mathf.RoundToInt (plantYield * seedDef.seed.harvestFactor);

                                harvestedThingDef = seedDef.harvest;
                            } else {
                                harvestedThingDef = plant.def.plant.harvestedThingDef;
                            }

                            if (plantYield > 0) {
                                var thing = ThingMaker.MakeThing (harvestedThingDef, null);
                                thing.stackCount = plantYield;
                                if (actor.Faction != Faction.OfPlayer) {
                                    thing.SetForbidden (true, true);
                                }
                                GenPlace.TryPlaceThing (thing, actor.Position, actor.Map, ThingPlaceMode.Near, null);
                                actor.records.Increment (RecordDefOf.PlantsHarvested);
                            }
                        }
                    }
                    plant.def.plant.soundHarvestFinish.PlayOneShot (actor);
                    plant.PlantCollected ();
                    workDone = 0;
                    ReadyForNextToil ();
                    return;
                }
            };

            toil.FailOnDespawnedNullOrForbidden (targetCellIndex);
            toil.WithEffect (EffecterDefOf.Harvest, targetCellIndex);
            toil.WithProgressBar (targetCellIndex, () => workDone / Plant.def.plant.harvestWork, true, -0.5f);
            toil.PlaySustainerOrSound (() => Plant.def.plant.soundHarvesting);

            return toil;
        }
    }
}
