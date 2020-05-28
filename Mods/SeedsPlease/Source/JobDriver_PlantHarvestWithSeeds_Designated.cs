using RimWorld;
using Verse;

namespace SeedsPlease
{
    public class JobDriver_PlantHarvestWithSeeds_Designated : JobDriver_PlantHarvestWithSeeds
    {
        protected override DesignationDef RequiredDesignation => DesignationDefOf.HarvestPlant;
    }
}
