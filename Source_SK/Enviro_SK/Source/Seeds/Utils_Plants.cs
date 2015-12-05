using Verse;
using RimWorld;

namespace SK_Enviro.Seeds
{
    internal class Utils_Plants
    {
        public static IPlantToGrowSettable GetPlayerSetPlantForCell(IntVec3 cell)
        {
            IPlantToGrowSettable plantToGrowSettable = cell.GetEdifice() as IPlantToGrowSettable;
            if (plantToGrowSettable == null)
                plantToGrowSettable = Find.ZoneManager.ZoneAt(cell) as IPlantToGrowSettable;
            
            return plantToGrowSettable;
        }

        public static bool IsCellOpenForSowingPlantOfType(IntVec3 cell, ThingDef plantDef)
        {
            IPlantToGrowSettable plantToGrowSettable = GetPlayerSetPlantForCell(cell);
            if (plantToGrowSettable == null || !plantToGrowSettable.CanAcceptSowNow())
                return false;

            ThingDef plantDefToGrow = plantToGrowSettable.GetPlantDefToGrow();
            if (plantDefToGrow == null || plantDefToGrow != plantDef)
                return false;

            // check if there's already a plant occupying the cell
            if (cell.GetPlant() != null)
                return false;

            // check if there are nearby cells which block growth
            if (GenPlant.AdjacentSowBlocker(plantDefToGrow, cell) != null)
                return false;

            // check through all the things in the cell which might block growth
            foreach (Thing tempThing in Find.ThingGrid.ThingsListAt(cell))
                if (tempThing.def.BlockPlanting)
                    return false;

            if (!plantDefToGrow.CanEverPlantAt(cell) || !GenPlant.GrowthSeasonNow(cell))
                return false;

            return true;
        }

        public static bool IsSeedForPlant(ThingDef seedDef, ThingDef plantDef)
        {
            ThingDef_PlantWithSeeds customPlantDef = plantDef as ThingDef_PlantWithSeeds;
            if (customPlantDef != null)
                return seedDef == customPlantDef.SeedDef;

            return false;
        }
    }
}

