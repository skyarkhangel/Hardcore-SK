using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SK_collect
{
    public class PlaceWorker_OnMudAndWater : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            TerrainDef mudDef = DefDatabase<TerrainDef>.GetNamed("Mud");
            TerrainDef waterDef = DefDatabase<TerrainDef>.GetNamed("WaterShallow");
            if (Find.TerrainGrid.TerrainAt(loc) == mudDef || Find.TerrainGrid.TerrainAt(loc) == waterDef)
            {
                return true;
            }
            return "OnMudOrWaterReportString".Translate();
        }
    }
}