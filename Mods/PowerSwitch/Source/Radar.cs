using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using UnityEngine; // Always needed
using RimWorld; // Needed
using Verse; // Needed
//using Verse.AI; // Needed when you do something with the AI
//using Verse.Sound; // Needed when you do something with the Sound


namespace CommonMisc
{
    /// <summary>
    /// This class bundles some functions to find pawns
    /// </summary>
    /// <author>Haplo</author>
    /// <permission>Please check the provided license info for granted permissions.</permission>
    public class Radar
    {
        // Constructor
        public Radar() {}

        /// <summary>
        /// Find enemy pawns in reach and return a list
        /// </summary>
        /// <param name="Position">The starting position</param>
        /// <param name="Distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindEnemyPawns(IntVec3 Position, float Distance)
        {
            // LINQ version
            return Find.MapPawns.AllPawnsSpawned.Where(p => p.HostileTo(Faction.OfPlayer) && !p.InContainer && p.Position.InHorDistOf(Position, Distance));
        }
        /// <summary>
        /// Find enemy pawns in reach and return a list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindEnemyPawns()
        {
            // LINQ version
            return Find.MapPawns.AllPawnsSpawned.Where(p => p.HostileTo(Faction.OfPlayer));
        }
        /// <summary>
        /// Find enemy pawns and return the count
        /// </summary>
        /// <returns></returns>
        public static int FindEnemyPawnsCount()
        {
            IEnumerable<Pawn> pawns = FindEnemyPawns();
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }


        /// <summary>
        /// Find friendly pawns in reach and return a list
        /// </summary>
        /// <param name="Position">The starting position</param>
        /// <param name="Distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindFriendlyPawns(IntVec3 Position, float Distance)
        {
            // LINQ version
            IEnumerable<Pawn> pawns = FindAllPawns(Position, Distance);
            if (pawns == null)
                return null;
            else
                return pawns.Where(p => !p.InContainer && !p.Faction.HostileTo(Faction.OfPlayer) && !p.RaceProps.Animal && !p.IsColonist && !p.IsPrisonerOfColony);
        }        /// <summary>
        /// Find friendly pawns and return a list
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindFriendlyPawns()
        {
            // LINQ version
            IEnumerable<Pawn> pawns = FindAllPawns();
            if (pawns == null)
                return null;
            else
                return pawns.Where(p => !p.InContainer && !IsHostile(p, Faction.OfPlayer) && !p.RaceProps.Animal && !p.IsColonist && !p.IsPrisonerOfColony);
        }
        /// Find friendly pawns and return the count
        /// </summary>
        /// <returns></returns>
        public static int FindFriendlyPawnsCount()
        {
            IEnumerable<Pawn> pawns = FindFriendlyPawns();
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }


        /// <summary>
        /// Find friendly pawns in reach and return a list
        /// </summary>
        /// <param name="Position">The starting position</param>
        /// <param name="Distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindColonyAnimals(IntVec3 Position, float Distance)
        {
            // LINQ version
            IEnumerable<Pawn> pawns = FindAllPawns(Position, Distance);
            if (pawns == null)
                return null;
            else
                return pawns.Where(p => !p.InContainer && !p.Dead && p.RaceProps.Animal && p.Faction == Faction.OfPlayer);
        }        /// <summary>
                 /// Find colony animals and return a list
                 /// </summary>
                 /// <returns></returns>
        public static IEnumerable<Pawn> FindColonyAnimals()
        {
            // LINQ version
            IEnumerable<Pawn> pawns = FindAllPawns();
            if (pawns == null)
                return null;
            else
                return pawns.Where(p => !p.InContainer && !p.Dead && p.RaceProps.Animal && p.Faction == Faction.OfPlayer);
        }
        /// Find friendly pawns and return the count
        /// </summary>
        /// <returns></returns>
        public static int FindColonyAnimalsCount()
        {
            IEnumerable<Pawn> pawns = FindColonyAnimals();
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }



        /// <summary>
        /// Find all pawns in reach and return an IEnumerable
        /// </summary>
        /// <param name="Position">The starting position</param>
        /// <param name="Distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindAllPawns(IntVec3 Position, float Distance)
        {
            // LINQ version
            return Find.MapPawns.AllPawnsSpawned.Where(p => !p.InContainer && p.Position.InHorDistOf(Position, Distance));
        }
        /// <summary>
        /// Find all pawns and return an IEnumerable
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindAllPawns()
        {
            // LINQ version
            return Find.MapPawns.AllPawnsSpawned;
        }
        /// <summary>
        /// Find all pawns and return the count
        /// </summary>
        /// <returns></returns>
        public static int FindAllPawnsCount()
        {
            IEnumerable<Pawn> pawns = FindAllPawns();
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }


        /// <summary>
        /// Find all colonist pawns in reach and return an IEnumerable
        /// </summary>
        /// <param name="Position">The starting position</param>
        /// <param name="Distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindColonistPawns(IntVec3 Position, float Distance)
        {
            // LINQ version
            return Find.MapPawns.FreeColonistsSpawned.Where(p => !p.InContainer && p.Position.InHorDistOf(Position, Distance));
        }
        /// <summary>
        /// Find all colonist pawns and return an IEnumerable
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindColonistPawns()
        {
            // LINQ version
            return Find.MapPawns.FreeColonistsSpawned;
        }
        /// <summary>
        /// Find all colonist pawns and return the count
        /// </summary>
        /// <returns></returns>
        public static int FindColonistPawnsCount()
        {
            IEnumerable<Pawn> pawns = FindColonistPawns();
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }


        /// <summary>
        /// Find all prisoner pawns in reach and return an IEnumerable
        /// </summary>
        /// <param name="Position">The starting position</param>
        /// <param name="Distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindPrisonerPawns(IntVec3 Position, float Distance)
        {
            // LINQ version
            return Find.MapPawns.PrisonersOfColonySpawned.Where(p => !p.InContainer && p.Position.InHorDistOf(Position, Distance));
        }
        /// <summary>
        /// Find all prisoner pawns and return an IEnumerable
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindPrisonerPawns()
        {
            // LINQ version
            return Find.MapPawns.PrisonersOfColonySpawned;
        }
        /// <summary>
        /// Find all prisoner pawns and return the count
        /// </summary>
        /// <returns></returns>
        public static int FindPrisonerPawnsCount()
        {
            IEnumerable<Pawn> pawns = FindPrisonerPawns();
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }


        /// <summary>
        /// Find all pawns in room and return an IEnumerable
        /// </summary>
        /// <param name="room">Find in which room</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindAllPawnsInRoom(Room room)
        {
            // LINQ version
            return Find.MapPawns.AllPawnsSpawned.Where(p => !p.InContainer && (room == RoomQuery.RoomAt(p.Position)));
        }
        /// <summary>
        /// Find all pawns in room within a certain distance and return an IEnumerable
        /// </summary>
        /// <param name="positon">The source position</param>
        /// <param name="distance">The maximal distance</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindAllPawnsInRoom(IntVec3 position, Room room, float distance)
        {
            // LINQ version
            return Find.MapPawns.AllPawnsSpawned.Where(p => !p.InContainer && 
                                                        (RoomQuery.RoomAt(p.Position) == room) && 
                                                        (p.Position.InHorDistOf(position, distance)));
        }
        /// <summary>
        /// Find all pawns in reach and return an IEnumerable
        /// </summary>
        /// <param name="room">Find in which room</param>
        /// <returns></returns>
        public static int FindAllPawnsInRoomCount(Room room)
        {
            IEnumerable<Pawn> pawns = FindAllPawnsInRoom(room);
            if (pawns == null)
                return 0;
            else
                return pawns.Count();
        }


        /// <summary>
        /// Find all animals nearby and return an IEnumerable
        /// </summary>
        /// <param name="position">The starting position.</param>
        /// <param name="distance">The max distance from the position, where the animals must be.</param>
        /// <returns></returns>
        public static IEnumerable<Pawn> FindAllAnimals(IntVec3 position, float distance)
        {
            // LINQ version
            return Find.MapPawns.AllPawnsSpawned.Where(p => !p.InContainer && p.RaceProps.Animal && p.Position.InHorDistOf(position, distance));
        }


        /// <summary>
        /// Check if the target is hostile
        /// </summary>
        /// <param name="baseThing">Source Thing</param>
        /// <param name="targetThing">Target Thing</param>
        /// <returns></returns>
        public static bool IsHostile(Thing baseThing, Thing targetThing)
        {
            return GenHostility.HostileTo(baseThing, targetThing.Faction);
        }
        /// <summary>
        /// Check if the target is hostile
        /// </summary>
        /// <param name="baseThing">Source Thing</param>
        /// <param name="targetFaction">Target Faction</param>
        /// <returns></returns>
        public static bool IsHostile(Thing baseThing, Faction targetFaction)
        {
            return GenHostility.HostileTo(baseThing, targetFaction);
        }


        /// <summary>
        /// An easy function to check if it is day
        /// </summary>
        /// <returns></returns>
        public static bool IsDayTime()
        { 
            return (GenDate.HourInt >= 5 && GenDate.HourInt <= 19);
        }


        /// <summary>
        /// An easy function to check if it is night
        /// </summary>
        /// <returns></returns>
        public static bool IsNightTime()
        {
            return !IsDayTime();
        }


        /// <summary>
        /// Find all things in reach and return an IEnumerable
        /// </summary>
        /// <param name="position">The starting position</param>
        /// <param name="distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Thing> FindAllThings(IntVec3 position, float distance)
        {
            // LINQ version
            return Find.ListerThings.AllThings.Where(t => t.Position.InHorDistOf(position, distance));
        }

        /// <summary>
        /// Find defined things in reach and return an IEnumerable
        /// </summary>
        /// <param name="position">The starting position</param>
        /// <returns></returns>
        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position)
        {
            Room room = RoomQuery.RoomAt(position);
            // LINQ version
            return Find.ListerThings.AllThings.Where(t => room == RoomQuery.RoomAt(t.Position));
        }
        /// <summary>
        /// Find defined things in reach and return an IEnumerable
        /// </summary>
        /// <param name="position">The starting position</param>
        /// <param name="room">The containing room</param>
        /// <returns></returns>
        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position, Room room)
        {
            // LINQ version
            return Find.ListerThings.AllThings.Where(t => room == RoomQuery.RoomAt(t.Position));
        }
        /// <summary>
        /// Find defined things in reach and return an IEnumerable
        /// </summary>
        /// <param name="position">The starting position</param>
        /// <param name="room">The containing room</param>
        /// <param name="distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position, float distance)
        {
            Room room = RoomQuery.RoomAt(position);
            // LINQ version
            return Find.ListerThings.AllThings.Where(t => t.Position.InHorDistOf(position, distance) && room == RoomQuery.RoomAt(t.Position));
        }
        /// <summary>
        /// Find defined things in reach and return an IEnumerable
        /// </summary>
        /// <param name="position">The starting position</param>
        /// <param name="room">The containing room</param>
        /// <param name="distance">The maximal distance to find targets</param>
        /// <returns></returns>
        public static IEnumerable<Thing> FindThingsInRoom(IntVec3 position, Room room, float distance)
        {
            // LINQ version
            return Find.ListerThings.AllThings.Where(t => t.Position.InHorDistOf(position, distance) && room == RoomQuery.RoomAt(t.Position));
        }


        /// <summary>
        /// Checks if a pawn is outdoors
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsOutdoors(Pawn pawn, bool countRooflessAsOutdoors = false)
        {
            if (countRooflessAsOutdoors)
            {
                return !Find.RoofGrid.Roofed(pawn.Position);
            }
            else
            {
                Room room = pawn.Position.GetRoom();
                return IsOutdoors(room); 
            }
        }
        /// <summary>
        /// Checks if a room is outdoors
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static bool IsOutdoors(Room room)
        {
            return (room != null && room.TouchesMapEdge);
        }



        // Archive of old variants
        // #########################

        //// Find enemy pawns in reach and return a list
        //public static List<Pawn> FindEnemyPawns(IntVec3 Position, float Distance)
        //{
        //    List<Pawn> p = new List<Pawn>();

        //    // Get enemy pawns
        //    IEnumerable<Pawn> hostiles = Find.ListerPawns.PawnsHostileToColony;

        //    // Cycle found pawns
        //    foreach (Pawn enemy in hostiles)
        //    {
        //        if (enemy.Position.WithinHorizontalDistanceOf(Position, Distance))
        //        {
        //            p.Add(enemy);
        //        }
        //    }

        //    // return found pawns list
        //    return p;
        //}


        //// Find all pawns in reach and return a list
        //public static List<Pawn> FindAllPawns(IntVec3 Position, float Distance)
        //{
        //    List<Pawn> foundPawns = new List<Pawn>();

        //    // Get enemy pawns
        //    IEnumerable<Pawn> pawns = Find.ListerPawns.AllPawns;

        //    // Cycle found pawns
        //    foreach (Pawn pawn in pawns)
        //    {
        //        if (pawn.Position.WithinHorizontalDistanceOf(Position, Distance))
        //        {
        //            foundPawns.Add(pawn);
        //        }
        //    }

        //    // return found pawns list
        //    return foundPawns;
        //}

        //// Find all pawns in reach and return a list
        //public static List<Pawn> FindColonistPawns(IntVec3 Position, float Distance)
        //{
        //    List<Pawn> foundPawns = new List<Pawn>();

        //    // Get enemy pawns
        //    IEnumerable<Pawn> pawns = Find.ListerPawns.Colonists;

        //    // Cycle found pawns
        //    foreach (Pawn pawn in pawns)
        //    {
        //        if (pawn.Position.WithinHorizontalDistanceOf(Position, Distance))
        //        {
        //            foundPawns.Add(pawn);
        //        }
        //    }

        //    // return found pawns list
        //    return foundPawns;
        //}


    }
}
