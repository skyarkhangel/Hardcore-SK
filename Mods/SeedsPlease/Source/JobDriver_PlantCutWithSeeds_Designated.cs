using RimWorld;
using Verse;

namespace SeedsPlease
{
    public class JobDriver_PlantCutWithSeeds_Designated : JobDriver_PlantCutWithSeeds
    {
        protected override DesignationDef RequiredDesignation => DesignationDefOf.CutPlant;
    }
}
