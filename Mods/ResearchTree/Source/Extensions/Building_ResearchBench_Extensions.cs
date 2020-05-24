// Building_ResearchBench_Extensions.cs
// Copyright Karel Kroeze, 2016-2020

using System.Linq;
using RimWorld;
using Verse;

namespace FluffyResearchTree
{
    public static class Building_ResearchBench_Extensions
    {
        public static bool HasFacility( this Building_ResearchBench building, ThingDef facility )
        {
            var comp = building.GetComp<CompAffectedByFacilities>();
            if ( comp == null )
                return false;

            if ( comp.LinkedFacilitiesListForReading.Select( f => f.def ).Contains( facility ) )
                return true;

            return false;
        }
    }
}