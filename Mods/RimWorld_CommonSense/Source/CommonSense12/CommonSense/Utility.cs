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
            get
            {
                if (fCleaningDef == null)
                {
                    fCleaningDef = DefDatabase<WorkTypeDef>.GetNamed("Cleaning");
                }
                return fCleaningDef;
            }
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
        {
            if (q.Count > 0)
            {
                int x = 0;
                int idx = 0;
                int n = 0;
                LocalTargetInfo out_of_all_things_they_didnt_add_a_simple_swap = null;

                if (Starter != null)
                {
                    if (q[0].Cell == null)
                        n = int.MaxValue;
                    else
                        n = q[0].Cell.DistanceToSquared(Starter.Position);

                    for (int i = 1; i < q.Count(); i++)
                    {
                        if (q[i].Cell == null)
                            continue;
                        x = q[i].Cell.DistanceToSquared(Starter.Position);
                        if (Math.Abs(x) < Math.Abs(n))
                        {
                            n = x;
                            idx = i;
                        }
                    }

                    if (idx != 0)
                    {
                        out_of_all_things_they_didnt_add_a_simple_swap = q[idx];
                        q[idx] = q[0];
                        q[0] = out_of_all_things_they_didnt_add_a_simple_swap;
                    }
                }

                for (int i = 0; i < q.Count() - 1; i++)
                {
                    if (q[i + 1].Cell == null)
                        continue;

                    n = q[i].Cell.DistanceToSquared(q[i + 1].Cell);
                    idx = i + 1;
                    for (int c = i + 2; c < q.Count(); c++)
                    {
                        if (q[c].Cell == null)
                            continue;

                        x = q[i].Cell.DistanceToSquared(q[c].Cell);
                        if (Math.Abs(x) < Math.Abs(n))
                        {
                            n = x;
                            idx = c;
                        }
                    }

                    if (idx != i + 1)
                    {
                        out_of_all_things_they_didnt_add_a_simple_swap = q[idx];
                        q[idx] = q[i + 1];
                        q[i + 1] = out_of_all_things_they_didnt_add_a_simple_swap;
                    }
                }
            }
        }

        static public void OptimizePath(List<ThingCount> q, Thing Starter = null)
        {
            if (q.Count > 0)
            {
                int x = 0;
                int idx = 0;
                int n = 0;
                ThingCount out_of_all_things_they_didnt_add_a_simple_swap = default(ThingCount);

                if (Starter != null)
                {
                    if (q[0].Thing.Position == null)
                        n = int.MaxValue;
                    else
                        n = q[0].Thing.Position.DistanceToSquared(Starter.Position);

                    for (int i = 1; i < q.Count(); i++)
                    {
                        if (q[i].Thing.Position == null)
                            continue;
                        x = q[i].Thing.Position.DistanceToSquared(Starter.Position);
                        if (Math.Abs(x) < Math.Abs(n))
                        {
                            n = x;
                            idx = i;
                        }
                    }
                    if (idx != 0)
                    {
                        out_of_all_things_they_didnt_add_a_simple_swap = q[idx];
                        q[idx] = q[0];
                        q[0] = out_of_all_things_they_didnt_add_a_simple_swap;
                    }
                }

                for (int i = 0; i < q.Count() - 1; i++)
                {
                    if (q[i + 1].Thing.Position == null)
                        continue;

                    n = q[i].Thing.Position.DistanceToSquared(q[i + 1].Thing.Position);
                    idx = i + 1;
                    for (int c = i + 2; c < q.Count(); c++)
                    {
                        if (q[c].Thing.Position == null)
                            continue;

                        x = q[i].Thing.Position.DistanceToSquared(q[c].Thing.Position);
                        if (Math.Abs(x) < Math.Abs(n))
                        {
                            n = x;
                            idx = c;
                        }
                    }

                    if (idx != i + 1)
                    {
                        out_of_all_things_they_didnt_add_a_simple_swap = q[idx];
                        q[idx] = q[i + 1];
                        q[i + 1] = out_of_all_things_they_didnt_add_a_simple_swap;
                    }
                }
            }
        }
    }
}
