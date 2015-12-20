using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace SK_Oilfield
{
    public class PlacementRestrictor_OnFissure : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot)
        {
            Fissure thing = (Fissure)Find.ThingGrid.ThingAt(loc, ThingDef.Named("Fissure"));
            if (thing != null && thing.Position == loc)
                return true;
            else
                return "OnFissureReportString".Translate();
        }
    }
}
