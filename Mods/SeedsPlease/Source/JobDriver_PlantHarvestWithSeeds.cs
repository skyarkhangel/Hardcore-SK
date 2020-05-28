using RimWorld;
using Verse.AI;

namespace SeedsPlease
{
    public class JobDriver_PlantHarvestWithSeeds : JobDriver_PlantWorkWithSeeds
    {
        protected override void Init ()
        {
            xpPerTick = 0.17f;
        }

        protected override Toil PlantWorkDoneToil ()
        {
            return Toils_General.RemoveDesignationsOnThing (TargetIndex.A, DesignationDefOf.HarvestPlant);
        }
    }
}
