using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Fluffy_Breakdowns
{
    public class WorkGiver_Maintenance : WorkGiver_Scanner
    {
        #region Properties

        public JobDef JobDefOf_Maintain => DefDatabase<JobDef>.GetNamed( "FluffyBreakdowns_Maintenance" );

        #endregion Properties

        #region Methods

        public override bool HasJobOnThing( Pawn pawn, Thing thing )
        {
            if ( thing.Faction != pawn.Faction )
                return false;

            if ( pawn.Faction == Faction.OfColony && !Find.AreaHome[thing.Position] )
                return false;

            if ( thing.IsBurning() )
                return false;

            if ( Find.DesignationManager.DesignationOn( thing, DesignationDefOf.Deconstruct ) != null )
                return false;

            ThingWithComps twc = thing as ThingWithComps;
            if ( twc == null )
                return false;

            var comp = twc.TryGetComp<CompBreakdownable>();
            if ( comp == null )
                return false;

            if ( !MapComponent_Durability.RequiresMaintenance( comp ) )
                return false;

            if ( !pawn.CanReserveAndReach( thing, PathEndMode.Touch, pawn.NormalMaxDanger() ) )
                return false;

            return true;
        }

        public override Job JobOnThing( Pawn pawn, Thing thing )
        {
            return new Job( JobDefOf_Maintain, thing );
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal( Pawn Pawn )
        {
            return MapComponent_Durability.potentialMaintenanceThings;
        }

        public override bool ShouldSkip( Pawn pawn )
        {
            return MapComponent_Durability.potentialMaintenanceThings.Count() == 0;
        }

        #endregion Methods
    }
}