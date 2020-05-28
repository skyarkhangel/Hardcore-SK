using RimWorld;
using Verse;
using Verse.AI;

namespace SeedsPlease
{
    public class JobDriver_PlantCutWithSeeds : JobDriver_PlantWorkWithSeeds
    {
        protected override void Init ()
        {
            if (Plant.def.plant.harvestedThingDef != null && Plant.YieldNow () > 0) {
                xpPerTick = 0.17f;
            } else {
                xpPerTick = 0f;
            }
        }

        protected override Toil PlantWorkDoneToil ()
        {
            return new Toil () {
                initAction = delegate {
                    var thing = job.GetTarget (TargetIndex.A).Thing;
                    if (!thing.Destroyed) {
                        thing.Destroy (DestroyMode.Vanish);
                    }
                }
            };
        }
    }
}
