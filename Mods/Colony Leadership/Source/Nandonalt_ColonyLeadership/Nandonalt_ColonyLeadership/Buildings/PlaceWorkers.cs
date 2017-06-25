using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Nandonalt_ColonyLeadership
{
    public class PlaceWorker_TeachingSpot : PlaceWorker
    {
        
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {
                    
            List<Thing> allBuildingsColonist = base.Map.listerThings.AllThings;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                Thing thing = allBuildingsColonist[i];
                if(thing.def.defName == "TeachingSpot" || thing.def.defName == "TeachingSpot_Blueprint")
                {
                    return new AcceptanceReport("OnlyOnePerColony".Translate(new object[] { thing.def.LabelCap }));
                }
            }
       
            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, center, rot, base.Map).ToList<IntVec3>());
        }
    }

    public class PlaceWorker_BallotBox : PlaceWorker
    {

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null)
        {

            List<Thing> allBuildingsColonist = base.Map.listerThings.AllThings;
            for (int i = 0; i < allBuildingsColonist.Count; i++)
            {
                Thing thing = allBuildingsColonist[i];
                if (thing.def.defName == "BallotBox" || thing.def.defName == "BallotBox_Blueprint")
                {
                    return new AcceptanceReport("OnlyOnePerColony".Translate(new object[] { thing.def.LabelCap }));
                }
            }

            return true;
        }

    }
}

