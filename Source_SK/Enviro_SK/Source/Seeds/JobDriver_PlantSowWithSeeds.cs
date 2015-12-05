using System.Collections.Generic;

using Verse;
using Verse.AI;
using RimWorld;

namespace SK_Enviro.Seeds
{
    internal class JobDriver_PlantSowWithSeeds : JobDriver
    {
        private const TargetIndex targetCellIndex = TargetIndex.A;

        private const TargetIndex seedsTargetIndex = TargetIndex.B;

        private float m_fSowWorkRemaining;

        public override string GetReport()
        {
            string text = JobDefOf.Sow.reportString;
            if (base.CurJob.plantDefToSow != null)
            {
                text = text.Replace("TargetA", GenLabel.ThingLabel(base.CurJob.plantDefToSow, null, 1));
            }
            else
            {
                text = text.Replace("TargetA", "area");
            }
            return text;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnBurningImmobile(TargetIndex.A);
            this.FailOnDestroyed(TargetIndex.B);
            this.FailOnForbidden(TargetIndex.B);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1);
            yield return Toils_Reserve.ReserveQueue(TargetIndex.B, 1);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B);
            Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            yield return toil;
            yield return this.SowSeedToil();
            yield return Toils_Reserve.Release(TargetIndex.A);
            yield return this.TryToSetAdditionalPlantingSiteWithEndJobOnFailToil();
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Jump.Jump(toil);
            yield break;
        }

        private bool GetNearbyPlantingSite(IntVec3 originPos, out IntVec3 newSite)
        {
            return CellFinder.TryFindRandomCellNear(originPos, 5, (IntVec3 tempCell) => Utils_Plants.IsCellOpenForSowingPlantOfType(tempCell, base.CurJob.plantDefToSow) && base.GetActor().CanReserveAndReach(tempCell, PathEndMode.Touch, base.GetActor().NormalMaxDanger(), 1), out newSite);
        }

        private bool IsActorCarryingAppropriateSeed()
        {
            Pawn actor = base.GetActor();
            return actor != null && actor.carrier != null && actor.carrier.CarriedThing != null && actor.carrier.CarriedThing.stackCount > 0 && Utils_Plants.IsSeedForPlant(actor.carrier.CarriedThing.def, base.CurJob.plantDefToSow);
        }

        private Toil TryToSetAdditionalPlantingSiteWithEndJobOnFailToil()
        {
            return new Toil
            {
                initAction = delegate
                {
                    IntVec3 vec;
                    if (this.IsActorCarryingAppropriateSeed() && this.GetNearbyPlantingSite(base.CurJob.GetTarget(TargetIndex.A).Cell, out vec))
                    {
                        Toils_Reserve.Release(TargetIndex.A);
                        base.CurJob.SetTarget(TargetIndex.A, vec);
                        return;
                    }
                    base.EndJobWith(JobCondition.Incompletable);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        private Toil SowSeedToil()
        {
            Toil sowToil = new Toil();
            sowToil.initAction = delegate
            {
                sowToil.actor.pather.StopDead();
                if (this.IsActorCarryingAppropriateSeed())
                {
                    Thing thing = GenSpawn.Spawn(this.CurJob.plantDefToSow, this.CurJob.GetTarget(TargetIndex.A).Cell);
                    if (thing != null)
                    {
                        PlantWithSeeds expr_5D = (PlantWithSeeds)thing;
                        expr_5D.growth = 0f;
                        expr_5D.genome = ((ThingDef_PlantWithSeeds)expr_5D.def).genome.RandomGenome();
                        this.CurJob.SetTarget(TargetIndex.C, new TargetInfo(thing));
                        this.m_fSowWorkRemaining = this.CurJob.plantDefToSow.plant.sowWork;
                        sowToil.WithEffect("Sow", TargetIndex.A);
                    }
                }
            };
            sowToil.AddFinishAction(delegate
            {
                TargetInfo target = base.CurJob.GetTarget(TargetIndex.C);
                if (target != null)
                {
                    if (target.HasThing && !target.ThingDestroyed)
                    {
                        Plant plant = (Plant)target.Thing;
                        if (plant.LifeStage == PlantLifeStage.Sowing)
                        {
                            plant.Destroy(DestroyMode.Vanish);
                        }
                    }
                    base.CurJob.SetTarget(TargetIndex.C, null);
                }
            });
            sowToil.tickAction = delegate
            {
                Pawn actor = sowToil.actor;
                TargetInfo target = this.CurJob.GetTarget(TargetIndex.C);
                if (!this.IsActorCarryingAppropriateSeed() || target == null || !target.HasThing || target.ThingDestroyed)
                {
                    this.EndJobWith(JobCondition.Incompletable);
                    return;
                }
                SkillRecord skill = actor.skills.GetSkill(SkillDefOf.Growing);
                if (skill != null)
                {
                    skill.Learn(0.11f * skill.LearningFactor);
                }
                this.m_fSowWorkRemaining -= actor.GetStatValue(StatDefOf.PlantWorkSpeed, true);
                if (this.m_fSowWorkRemaining <= 0f)
                {
                    if (sowToil.actor.carrier.CarriedThing.stackCount <= 1)
                    {
                        sowToil.actor.carrier.CarriedThing.Destroy(DestroyMode.Vanish);
                    }
                    else
                    {
                        sowToil.actor.carrier.CarriedThing.stackCount--;
                    }
                    if (actor.needs.mood != null)
                    {
                        actor.needs.mood.thoughts.TryGainThought(DefDatabase<ThoughtDef>.GetNamed("GreenThumbHappy", true));
                    }
                    Plant plant = (Plant)target.Thing;
                    plant.growth = 0.05f;
                    Find.MapDrawer.MapMeshDirty(plant.Position, MapMeshFlag.Things);
                    this.ReadyForNextToil();
                }
            };
            sowToil.defaultCompleteMode = ToilCompleteMode.Never;
            return sowToil;
        }
    }
}
