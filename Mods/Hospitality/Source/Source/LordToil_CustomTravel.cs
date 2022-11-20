using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Hospitality
{
    public class LordToilData_CustomTravel : LordToilData
    {
        public IntVec3 dest;
        public bool destAssigned;
        public float percentRequired;
        public float distance;
        public HashSet<Pawn> arrivedPawns = new HashSet<Pawn>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref dest, "dest");
            Scribe_Values.Look(ref destAssigned, "destAssigned");
            Scribe_Values.Look(ref percentRequired, "percentRequired", 1);
            Scribe_Values.Look(ref distance, "distance", 10);
            Scribe_Collections.Look(ref arrivedPawns, "arrivedPawns", LookMode.Reference);
        }
    }

    public class LordToil_CustomTravel : LordToil
    {
        private LordToilData_CustomTravel Data => data as LordToilData_CustomTravel;
        public override IntVec3 FlagLoc => Data.dest;
        public override bool AllowSatisfyLongNeeds => false;

        public LordToil_CustomTravel(IntVec3 dest, float percentRequired = 1, float distance = 10)
        {
            data = new LordToilData_CustomTravel {dest = dest, destAssigned = true, percentRequired = percentRequired, distance = distance};
        }

        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns) pawn.mindState.duty = new PawnDuty(DutyDefOf.TravelOrLeave, Data.dest) {locomotion = LocomotionUrgency.Jog, maxDanger = Danger.Some};
        }

        public override void Init()
        {
            base.Init();
            if (Data.destAssigned) return;
            if (!RCellFinder.TryFindTravelDestFrom(lord.ownedPawns[0].Position, Map, out Data.dest))
            {
                Log.Error("Travelers for " + lord.faction + " could not late-find travel destination.");
                Data.dest = lord.ownedPawns[0].Position;
            }
            Data.destAssigned = true;
        }

        public override void LordToilTick()
        {
            if (Data == null) return;

            foreach (var pawn in lord.ownedPawns)
            {
                if (pawn == null) continue;
                if (!pawn.IsHashIntervalTick(205)) continue;
                if (Data.arrivedPawns.Contains(pawn)) continue;
                if (pawn.Dead || pawn.Downed || !pawn.Spawned) OnPawnArrived(pawn);
                else if (pawn.Position.InHorDistOf(Data.dest, Data.distance) && pawn.CanReach(Data.dest, PathEndMode.OnCell, Danger.Some)) OnPawnArrived(pawn);
            }

            void OnPawnArrived(Pawn pawn)
            {
                Data.arrivedPawns.Add(pawn);

                int count = Data.arrivedPawns.Count;
                float percent = 1f*count/lord.ownedPawns.Count(p => p != null);
                if (percent < Data.percentRequired) return;
                lord.ReceiveMemo("TravelArrived");
            }
        }
    }
}
