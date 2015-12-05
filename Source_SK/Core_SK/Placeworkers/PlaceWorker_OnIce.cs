using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SK_collect
{
    public class PlaceWorker_OnIce : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            TerrainDef iceDef = DefDatabase<TerrainDef>.GetNamed("Ice");
            if (Find.TerrainGrid.TerrainAt(loc) == iceDef)
            {
                return true;
            }
            return "OnIceReportString".Translate();
        }
    }
}