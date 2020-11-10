using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using RimWorld;

namespace CommonSense
{
    class Utility
    {
        static private WorkGiverDef cleanFilth = null;
        public const byte largeRoomSize = 160;

        static private WorkTypeDef fCleaningDef = null;
        static public WorkTypeDef CleaningDef
        {
            get => fCleaningDef ?? (fCleaningDef = DefDatabase<WorkTypeDef>.GetNamed("Cleaning"));
        }

        public static bool IncapableOfCleaning(Pawn pawn)
        {
            return pawn.def.race == null ||
                (int)pawn.def.race.intelligence < 2 ||
                pawn.Faction != Faction.OfPlayer ||
                //pawn.Drafted || 
                (int)pawn.RaceProps.intelligence < 2 ||
                pawn.WorkTagIsDisabled(WorkTags.ManualDumb | WorkTags.Cleaning) ||
                pawn.InMentalState || pawn.IsBurning() ||
                pawn.workSettings == null || !pawn.workSettings.WorkIsActive(CleaningDef);
        }

        public static IEnumerable<Filth> SelectAllFilth(Pawn pawn, LocalTargetInfo target, int Limit = int.MaxValue)
        {
            Room room = null;
            if (target.Thing == null)
                if (target.Cell == null)
                    Log.Error("Invalid target: cell or thing it must be");
                else
                    room = GridsUtility.GetRoom(target.Cell, pawn.Map);
            else
                room = target.Thing.GetRoom();

            if (room == null)
                return new List<Filth>();

            PathGrid pathGrid = pawn.Map.pathGrid;
            if (pathGrid == null)
                return new List<Filth>();

            if (cleanFilth == null)
                cleanFilth = DefDatabase<WorkGiverDef>.GetNamed("CleanFilth");

            if (cleanFilth.Worker == null)
                return new List<Filth>();

            IEnumerable<Filth> enumerable = null;
            if (room.IsHuge || room.CellCount > largeRoomSize)
            {
                enumerable = new List<Filth>();
                for (int i = 0; i < 200; i++)
                {
                    IntVec3 intVec = target.Cell + GenRadial.RadialPattern[i];
                    if (intVec.InBounds(pawn.Map) && intVec.InAllowedArea(pawn) && intVec.GetRoom(pawn.Map) == room)
                        ((List<Filth>)enumerable).AddRange(intVec.GetThingList(pawn.Map).OfType<Filth>().Where(f => !f.Destroyed
                            && ((WorkGiver_Scanner)cleanFilth.Worker).HasJobOnThing(pawn, f)).Take(Limit == 0 ? int.MaxValue : Limit));
                    if (Limit > 0 && enumerable.Count() >= Limit)
                        break;
                }
            }
            else
                enumerable = room.ContainedAndAdjacentThings.OfType<Filth>().Where(delegate (Filth f)
                {
                    //Log.Message(f.ToString() + "," + f.Destroyed.ToString()+","+ f.Position.InAllowedArea(pawn).ToString()+","+ ((WorkGiver_Scanner)cleanFilth.Worker).HasJobOnThing(pawn, f).ToString());
                    if (f == null || f.Destroyed || !f.Position.InAllowedArea(pawn) || !((WorkGiver_Scanner)cleanFilth.Worker).HasJobOnThing(pawn, f))
                        return false;

                    Room room2 = f.GetRoom();
                    if (room2 == null || room2 != room && !room2.IsDoorway)
                        return false;

                    return true;
                }).Take(Limit == 0 ? int.MaxValue : Limit);
            return enumerable;
        }

        public static void AddFilthToQueue(Job j, TargetIndex ind, IEnumerable<Filth> l, Pawn pawn)
        {
            foreach (Filth f in (l))
                j.AddQueuedTarget(ind, f);

            OptimizePath(j.GetTargetQueue(ind), pawn);
        }

        static public void OptimizePath(List<LocalTargetInfo> q, Thing Starter)
            => OptimizePath(q, Starter?.Position, lti => lti.Cell);

        static public void OptimizePath(List<ThingCount> q, Thing Starter = null)
            => OptimizePath(q, Starter?.Position, tc => tc.Thing.Position);

        static private void OptimizePath<T>(List<T> q, IntVec3? startPosition, Func<T, IntVec3> position)
        {
            if (q.Count > 0)
            {
                int vertexCostOption = 0;
                int idx = 0;
                int vertexCostCurrent = 0;

                if (startPosition != null)
                {
                    if (startPosition.Value == null)
                        vertexCostCurrent = int.MaxValue;
                    else
                        vertexCostCurrent = position(q[0]).DistanceToSquared(startPosition.Value);

                    for (int i = 1; i < q.Count(); i++)
                    {
                        var positionI = position(q[i]);
                        if (positionI == null)  // are these null checks actually doing anything? It's a struct but that operator is overridden.
                            continue;
                        vertexCostOption = positionI.DistanceToSquared(startPosition.Value);
                        if (Math.Abs(vertexCostOption) < Math.Abs(vertexCostCurrent))
                        {
                            vertexCostCurrent = vertexCostOption;
                            idx = i;
                        }
                    }
                    q.SwapIndices(idx, 0);
                }

                for (int i = 0; i < q.Count() - 1; i++)
                {
                    var position1 = position(q[i + 1]);
                    if (position1 == null)
                        continue;

                    var position0 = position(q[i]);
                    vertexCostCurrent = position0.DistanceToSquared(position1);
                    idx = i + 1;
                    for (int c = i + 2; c < q.Count(); c++)
                    {
                        var position2 = position(q[c]);
                        if (position2 == null)
                            continue;

                        vertexCostOption = position0.DistanceToSquared(position2);
                        if (Math.Abs(vertexCostOption) < Math.Abs(vertexCostCurrent))
                        {
                            vertexCostCurrent = vertexCostOption;
                            idx = c;
                        }
                    }

                    q.SwapIndices(idx, i + 1);
                }
            }
        }
    }
}
