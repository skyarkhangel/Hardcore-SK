// Decompiled with JetBrains decompiler
// Type: ModCommon.Radar
// Assembly: RimWorld_AtomicPowerMod, Version=0.6.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3BA90F32-070E-4A95-8557-409C6CD87E36
// Assembly location: E:\Downloads\RimWorld671Win\Mods\AtomicPower\Assemblies\RimWorld_AtomicPowerMod.dll

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace SK_NPP
{
    public class Radar
    {
        public static IEnumerable<Pawn> FindEnemyPawns(IntVec3 Position, float Distance)
        {
            return Enumerable.Where<Pawn>(Find.ListerPawns.PawnsHostileToColony, (Func<Pawn, bool>)(p => !p.InContainer && p.Position.InHorDistOf(Position, Distance)));
        }

        public static IEnumerable<Pawn> FindEnemyPawns()
        {
            return Find.ListerPawns.PawnsHostileToColony;
        }

        public static int FindEnemyPawnsCount()
        {
            IEnumerable<Pawn> enemyPawns = Radar.FindEnemyPawns();
            if (enemyPawns == null)
                return 0;
            else
                return Enumerable.Count<Pawn>(enemyPawns);
        }

        public static IEnumerable<Pawn> FindFriendlyPawns(IntVec3 Position, float Distance)
        {
            IEnumerable<Pawn> allPawns = Radar.FindAllPawns(Position, Distance);
            if (allPawns == null)
                return (IEnumerable<Pawn>)null;
            else
                return Enumerable.Where<Pawn>(allPawns, (Func<Pawn, bool>)(p => !p.InContainer && !FactionUtility.HostileTo(p.Faction, Faction.OfColony) && (!p.RaceProps.Animal && !p.IsColonist) && !p.IsPrisonerOfColony));
        }

        public static IEnumerable<Pawn> FindFriendlyPawns()
        {
            IEnumerable<Pawn> allPawns = Radar.FindAllPawns();
            if (allPawns == null)
                return (IEnumerable<Pawn>)null;
            else
                return Enumerable.Where<Pawn>(allPawns, (Func<Pawn, bool>)(p => !p.InContainer && !Radar.IsHostile((Thing)p, Faction.OfColony) && (!p.RaceProps.Animal && !p.IsColonist) && !p.IsPrisonerOfColony));
        }

        public static int FindFriendlyPawnsCount()
        {
            IEnumerable<Pawn> friendlyPawns = Radar.FindFriendlyPawns();
            if (friendlyPawns == null)
                return 0;
            else
                return Enumerable.Count<Pawn>(friendlyPawns);
        }

        public static IEnumerable<Pawn> FindAllPawns(IntVec3 Position, float Distance)
        {
            return Enumerable.Where<Pawn>(Find.ListerPawns.AllPawns, (Func<Pawn, bool>)(p => !p.InContainer && p.Position.InHorDistOf(Position, Distance)));
        }

        public static IEnumerable<Pawn> FindAllPawns()
        {
            return Find.ListerPawns.AllPawns;
        }

        public static int FindAllPawnsCount()
        {
            IEnumerable<Pawn> allPawns = Radar.FindAllPawns();
            if (allPawns == null)
                return 0;
            else
                return Enumerable.Count<Pawn>(allPawns);
        }

        public static IEnumerable<Pawn> FindColonistPawns(IntVec3 Position, float Distance)
        {
            return Enumerable.Where<Pawn>(Find.ListerPawns.FreeColonists, (Func<Pawn, bool>)(p => p.Position.InHorDistOf(Position, Distance)));
        }

        public static IEnumerable<Pawn> FindColonistPawns()
        {
            return Find.ListerPawns.FreeColonists;
        }

        public static int FindColonistPawnsCount()
        {
            IEnumerable<Pawn> colonistPawns = Radar.FindColonistPawns();
            if (colonistPawns == null)
                return 0;
            else
                return Enumerable.Count<Pawn>(colonistPawns);
        }

        public static IEnumerable<Pawn> FindPrisonerPawns(IntVec3 Position, float Distance)
        {
            return Enumerable.Where<Pawn>(Find.ListerPawns.PrisonersOfColony, (Func<Pawn, bool>)(p => !p.InContainer && p.Position.InHorDistOf(Position, Distance)));
        }

        public static IEnumerable<Pawn> FindPrisonerPawns()
        {
            return Find.ListerPawns.PrisonersOfColony;
        }

        public static int FindPrisonerPawnsCount()
        {
            IEnumerable<Pawn> prisonerPawns = Radar.FindPrisonerPawns();
            if (prisonerPawns == null)
                return 0;
            else
                return Enumerable.Count<Pawn>(prisonerPawns);
        }

        public static IEnumerable<Pawn> FindAllPawnsInRoom(Room room)
        {
            return Enumerable.Where<Pawn>(Find.ListerPawns.AllPawns, (Func<Pawn, bool>)(p => !p.InContainer && room == RoomQuery.RoomAt(p.Position)));
        }

        public static IEnumerable<Pawn> FindAllPawnsInRoom(IntVec3 position, Room room, float distance)
        {
            return Enumerable.Where<Pawn>(Find.ListerPawns.AllPawns, (Func<Pawn, bool>)(p => !p.InContainer && RoomQuery.RoomAt(p.Position) == room && p.Position.InHorDistOf(position, distance)));
        }

        public static int FindAllPawnsInRoomCount(Room room)
        {
            IEnumerable<Pawn> allPawnsInRoom = Radar.FindAllPawnsInRoom(room);
            if (allPawnsInRoom == null)
                return 0;
            else
                return Enumerable.Count<Pawn>(allPawnsInRoom);
        }

        public static IEnumerable<Pawn> FindAllAnimals(IntVec3 position, float distance)
        {
            return Enumerable.Where<Pawn>(Find.ListerPawns.AllPawns, (Func<Pawn, bool>)(p => p.RaceProps.Animal && p.Position.InHorDistOf(position, distance)));
        }

        public static bool IsHostile(Thing baseThing, Thing targetThing)
        {
            return GenHostility.HostileTo(baseThing, targetThing.Faction);
        }

        public static bool IsHostile(Thing baseThing, Faction targetFaction)
        {
            return GenHostility.HostileTo(baseThing, targetFaction);
        }

        public static bool IsDayTime()
        {
            return (double)GenDate.HourInt >= 5.0 && (double)GenDate.HourInt <= 19.0;
        }

        public static bool IsNightTime()
        {
            return !Radar.IsDayTime();
        }

        public static IEnumerable<Thing> FindAllThings(IntVec3 position, float distance)
        {
            return Enumerable.Where<Thing>(Find.ListerThings.AllThings, (Func<Thing, bool>)(t => t.Position.InHorDistOf(position, distance)));
        }

        public static IEnumerable<Thing> FindThings(IntVec3 position, float distance, ThingCategory category)
        {
            return Enumerable.Where<Thing>(Find.ListerThings.AllThings, (Func<Thing, bool>)(t => t.Position.InHorDistOf(position, distance) && t.def.category == category));
        }

        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position)
        {
            Room room = RoomQuery.RoomAtFast(position);
            return Enumerable.Where<Thing>(Find.ListerThings.AllThings, (Func<Thing, bool>)(t => room == RoomQuery.RoomAtFast(t.Position)));
        }

        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position, Room room)
        {
            return Enumerable.Where<Thing>(Find.ListerThings.AllThings, (Func<Thing, bool>)(t => room == RoomQuery.RoomAtFast(t.Position)));
        }

        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position, float distance)
        {
            Room room = RoomQuery.RoomAtFast(position);
            return Enumerable.Where<Thing>(Find.ListerThings.AllThings, (Func<Thing, bool>)(t => t.Position.InHorDistOf(position, distance) && room == RoomQuery.RoomAtFast(t.Position)));
        }

        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position, Room room, float distance)
        {
            return Enumerable.Where<Thing>(Find.ListerThings.AllThings, (Func<Thing, bool>)(t => t.Position.InHorDistOf(position, distance) && room == RoomQuery.RoomAtFast(t.Position)));
        }

        public static bool IsOutdoors(Pawn pawn, bool countRooflessAsOutdoors = false)
        {
            if (countRooflessAsOutdoors)
                return !Find.RoofGrid.Roofed(pawn.Position);
            else
                return Radar.IsOutdoors(GridsUtility.GetRoom(pawn.Position));
        }

        public static bool IsOutdoors(Room room)
        {
            return room != null && room.TouchesMapEdge;
        }
    }
}
