using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Fluffy_Breakdowns
{
    public class JobDriver_Maintenance : JobDriver
    {
        #region Fields

        public const int fullRepairTicks = GenDate.TicksPerHour;

        #endregion Fields

        #region Properties

        public CompBreakdownable comp => TargetA.Thing?.TryGetComp<CompBreakdownable>();

        public float durability
        {
            get
            {
                return MapComponent_Durability.GetDurability( comp );
            }
            set
            {
                MapComponent_Durability.SetDurability( comp, value );
            }
        }

        #endregion Properties

        #region Methods

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve( TargetIndex.A ).FailOnDespawnedNullOrForbidden( TargetIndex.A );
            yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.Touch ).FailOnDespawnedNullOrForbidden( TargetIndex.A );

            Toil maintenance = new Toil();
            maintenance.tickAction = delegate
            {
                var pawn = maintenance.actor;
                durability += pawn.GetStatValue( StatDefOf.ConstructionSpeed ) / fullRepairTicks;

                if ( durability > .99f )
                    EndJobWith( JobCondition.Succeeded );
            };
            maintenance.WithEffect( TargetThingA.def.repairEffect, TargetIndex.A );
            maintenance.defaultCompleteMode = ToilCompleteMode.Never;
            yield return maintenance;
        }

        #endregion Methods
    }
}