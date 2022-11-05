using System;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorldDaysMatter
{
    public class PartyUtil
    {
        public static bool ShouldPawnKeepPartying(Pawn p)
        {
            // return (p.timetable == null || p.timetable.CurrentAssignment.allowJoy) && GatheringsUtility.ShouldGuestKeepAttendingGathering(p);
            if (p.Drafted)
                return false;
            if (GatheringDefOf.Party.respectTimetable && p.timetable != null && !p.timetable.CurrentAssignment.allowJoy)
                return false;
            return
                GatheringsUtility.ShouldGuestKeepAttendingGathering(p);
        }

        public static Pawn FindRandomPartyOrganizer(Faction faction, Map map)
        {
            Predicate<Pawn> validator = x => x.RaceProps.Humanlike && !x.InBed() && ShouldPawnKeepPartying(x) && x.GetLord() == null;
            Pawn result;
            return (from x in map.mapPawns.SpawnedPawnsInFaction(faction)
                where validator(x)
                select x).TryRandomElement(out result) ? result : null;
        }

        public static bool TryFindPartySpot(Pawn organizer, out IntVec3 result)
        {
            bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer);
            Map map = organizer.Map;
            Predicate<IntVec3> baseValidator = delegate (IntVec3 cell)
            {
                if (!cell.Standable(map))
                {
                    return false;
                }
                if (cell.GetDangerFor(organizer, map) != Danger.None)
                {
                    return false;
                }
                if (!enjoyableOutside && !cell.Roofed(map))
                {
                    return false;
                }
                if (cell.IsForbidden(organizer))
                {
                    return false;
                }
                if (!organizer.CanReserveAndReach(cell, PathEndMode.OnCell, Danger.None))
                {
                    return false;
                }
                Room room = cell.GetRoom(map);
                bool flag = room != null && room.IsPrisonCell;
                return organizer.IsPrisoner == flag;
            };
            if ((from x in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.PartySpot)
                 where baseValidator(x.Position)
                 select x.Position).TryRandomElement(out result))
            {
                return true;
            }
            Predicate<IntVec3> noPartySpotValidator = cell =>
            {
                Room room = cell.GetRoom(map);
                return room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10;
            };
            foreach (CompGatherSpot current in map.gatherSpotLister.activeSpots.InRandomOrder())
            {
                for (int i = 0; i < 10; i++)
                {
                    IntVec3 intVec = CellFinder.RandomClosewalkCellNear(current.parent.Position, current.parent.Map, 4);
                    if (!baseValidator(intVec) || !noPartySpotValidator(intVec)) continue;
                    result = intVec;
                    return true;
                }
            }
            if (CellFinder.TryFindRandomCellNear(organizer.Position, organizer.Map, 25, cell => baseValidator(cell) && noPartySpotValidator(cell), out result))
            {
                return true;
            }
            result = IntVec3.Invalid;
            return false;
        }
    }
}