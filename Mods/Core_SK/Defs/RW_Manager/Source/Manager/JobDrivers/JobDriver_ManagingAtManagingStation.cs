// Manager/JobDriver_ManagingAtManagingStation.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:25

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace FluffyManager
{
    internal class JobDriver_ManagingAtManagingStation : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Reserve.Reserve( TargetIndex.A ).FailOnDespawnedOrForbiddenPlacedTargets();
            yield return Toils_Goto.GotoThing( TargetIndex.A, PathEndMode.InteractionCell )
                                   .FailOnDespawnedOrForbiddenPlacedTargets();
            yield return Manage( TargetIndex.A ).FailOnDespawnedOrForbiddenPlacedTargets();
            yield return Toils_Reserve.Release( TargetIndex.A );
        }

        private Toil Manage( TargetIndex targetIndex )
        {
            Building_ManagerStation station = CurJob.GetTarget( targetIndex ).Thing as Building_ManagerStation;
            if ( station == null )
            {
                Log.Error( "Target of manager job was not a manager station. This should never happen." );
                return null;
            }
            Comp_ManagerStation comp = station.GetComp<Comp_ManagerStation>();
            if ( comp == null )
            {
                Log.Error( "Target of manager job does not have manager station comp. This should never happen." );
                return null;
            }
            Toil toil = new Toil();
            toil.defaultDuration =
                (int)( comp.props.Speed * ( 1 - pawn.GetStatValue( StatDef.Named( "ManagingSpeed" ) ) + .5 ) );
#if DEBUG_WORKGIVER
            Log.Message("Pawn stat: " + pawn.GetStatValue(StatDef.Named("ManagingSpeed")) + " (+0.5) Station speed: " + comp.props.Speed + "Total time: " + toil.defaultDuration);
#endif
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.tickAction =
                delegate { toil.actor.skills.GetSkill( DefDatabase<SkillDef>.GetNamed( "Managing" ) ).Learn( 0.11f ); };
            List<Action> finishers = new List<Action>();
            finishers.Add( delegate { Manager.Get.DoWork(); } );
            toil.finishActions = finishers;
            return toil;
        }
    }
}