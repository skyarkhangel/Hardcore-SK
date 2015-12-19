using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SK
{
    public class PlaceWorker_Single : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            if (Find.ListerBuildings.allBuildingsColonist.Any(b => b.def == checkingDef))
                return "NoMoreThanOne".Translate();
            return AcceptanceReport.WasAccepted;
        }
    }
}