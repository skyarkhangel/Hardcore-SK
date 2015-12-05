using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SK
{
    public class PlaceWorker_MaximumFive : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            ThingDef def = checkingDef as ThingDef;
            if (def != null)
            {
                int num = Find.ListerBuildings.allBuildingsColonist.Where((Building b) => b.def == def).Count();
                num += Find.ListerThings.ThingsOfDef(def.blueprintDef).Count;
                if (num < 5)
                {
                    return true;
                }
                return "MaximumFiveReportString".Translate();
            }
            return "Def not found";
        }
    }
}