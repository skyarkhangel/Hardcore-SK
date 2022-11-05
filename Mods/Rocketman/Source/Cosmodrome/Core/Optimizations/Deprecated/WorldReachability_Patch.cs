using System;
using System.Collections.Generic;
using System.Threading;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RocketMan.Optimizations
{
    public class WorldReachability_Patch
    {
        [RocketPatch(typeof(WorldReachability), nameof(WorldReachability.CanReach), parameters = new[] { typeof(int), typeof(int) })]
        public static class WorldReachability_CanReach_Patch
        {
            internal static HashSet<int> visitedTiles;

            internal static int visitedTilesCount;
            internal static int islandCounter;
            internal static int[] tilesToIsland;

            internal static World world;

            private static readonly Dictionary<int, List<int>> islands = new Dictionary<int, List<int>>();
            private static bool finished;

            private static readonly List<string> messages = new List<string>();
            private static readonly object locker = new object();

            internal static Thread thread;
            internal static ThreadStart threadStart;

            internal static void StartIslandGeneration()
            {
                lock (locker)
                {
                    finished = false;

                    try
                    {
                        GenerateIslands();
                        finished = true;
                    }
                    catch (Exception er)
                    {
                        if (RocketDebugPrefs.Debug)
                        {
                            messages.Add(string.Format("ROCKETMAN: Error in island generation with message {0} at {1}",
                                er.Message, er.StackTrace));
                        }
                    }
                }
            }

            public static void GenerateIslands()
            {
                var world = Find.World;
                var offsets = Find.WorldGrid.tileIDToNeighbors_offsets;
                var tilesIDsFromNeighbor = Find.WorldGrid.tileIDToNeighbors_values;

                var queue = new Queue<Pair<int, int>>(100);

                var passableTiles = new List<int>();

                for (var i = 0; i < Find.WorldGrid.TilesCount; i++)
                    if (!world.Impassable(i))
                        passableTiles.Add(i);

                var currentIslandCounter = 0;

                IEnumerable<int> GetNeighbors(int tile)
                {
                    var limit = tile + 1 < offsets.Count ? offsets[tile + 1] : tilesIDsFromNeighbor.Count;
                    for (var k = offsets[tile]; k < limit; k++)
                        yield return tilesIDsFromNeighbor[k];
                }

                while (visitedTilesCount < passableTiles.Count && world == Find.World || queue.Count > 0)
                    if (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        var currentIsland = current.First;
                        var currentTile = current.Second;
                        visitedTilesCount++;
                        visitedTiles.Add(currentTile);

                        tilesToIsland[currentTile] = currentIsland;
                        foreach (var neighbor in GetNeighbors(currentTile))
                            if (tilesToIsland[neighbor] == currentIsland && tilesToIsland[neighbor] != 0 ||
                                world.Impassable(neighbor))
                            {
                            }
                            else if (tilesToIsland[neighbor] == 0)
                            {
                                tilesToIsland[neighbor] = currentIsland;
                                queue.Enqueue(new Pair<int, int>(currentIsland, neighbor));
                                currentIslandCounter++;
                            }
                            else
                            {
                                var otherIsland = tilesToIsland[neighbor];
                                for (var i = 0; i < tilesToIsland.Length; i++)
                                    if (tilesToIsland[i] == otherIsland)
                                    {
                                        tilesToIsland[i] = currentIsland;
                                        currentIslandCounter++;
                                    }
                            }
                    }
                    else
                    {
                        var randomTile = passableTiles.RandomElement();
                        if (Find.World.Impassable(randomTile))
                            continue;
                        if (tilesToIsland[randomTile] != 0)
                            continue;
                        var nextIsland = islandCounter++;
                        currentIslandCounter = 1;
                        queue.Enqueue(new Pair<int, int>(nextIsland, randomTile));
                    }

                for (var i = 0; i < tilesToIsland.Length; i++)
                    if (islands.TryGetValue(tilesToIsland[i], out var island))
                    {
                        island.Add(i);
                    }
                    else
                    {
                        islands[tilesToIsland[i]] = new List<int>();
                        islands[tilesToIsland[i]].Add(i);
                    }

                if (world != Find.World) return;
#if DEBUG
                if (Prefs.DevMode)
                {
                    if (RocketDebugPrefs.Debug)
                        messages.Add(string.Format("ROCKETMAN: Island counter {0}, visited {1}", currentIslandCounter,
                            visitedTilesCount));
                    messages.Add(string.Format("ROCKETMAN: FINISHED BUILDING ISLANDS!, {0}, {1}, {2}, {3}",
                        islandCounter, visitedTilesCount, passableTiles.Count, currentIslandCounter));
                }
#endif
            }

            [Main.OnTickRare]
            public static void FlushMessages()
            {
                var counter = 0;
                while (messages.Count > 0 && counter++ < 128)
                {
                    var message = messages.Pop();
                    if (message.ToLower().Contains("error"))
                        Log.Error(message);
                    else
                        if (RocketDebugPrefs.Debug) Log.Message(message);
                }
            }

            public static void Initialize()
            {
                lock (locker)
                {
                    world = Find.World;
                    tilesToIsland = new int[Find.WorldGrid.TilesCount];
                    visitedTilesCount = 0;
                    visitedTiles = new HashSet<int>();
                    islandCounter = 1;
                    islands.Clear();
                }

                if (thread == null || !thread.IsAlive)
                {
                    threadStart = StartIslandGeneration;
                    thread = new Thread(threadStart);
                }
                else
                {
                    if (thread.IsAlive) thread.Interrupt();
                    threadStart = StartIslandGeneration;
                    thread = new Thread(threadStart);
                }

                thread.Start();
            }

            public static bool Prefix(ref bool __result, int startTile, int destTile)
            {
                if (RocketPrefs.Enabled)
                {
                    if (world != Find.World)
                    {
                        if (RocketDebugPrefs.Debug)
                            Log.Message("ROCKETMAN: Creating world map cache");
                        Initialize();
                    }

                    if (!finished)
                    {
                        if (RocketDebugPrefs.Debug)
                            Log.Warning("ROCKETMAN: Tried to call WorldReachability while still processing");
                        return true;
                    }

                    if (tilesToIsland[startTile] == 0 || tilesToIsland[destTile] == 0 ||
                        tilesToIsland[startTile] != tilesToIsland[destTile])
                    {
                        if (RocketDebugPrefs.Debug) Log.Message("ROCKETMAN: Not Allowed");
                        __result = false;
                    }

                    if (tilesToIsland[startTile] == tilesToIsland[destTile])
                    {
                        if (RocketDebugPrefs.Debug) Log.Message("ROCKETMAN: Allowed");
                        __result = true;
                    }

                    return false;
                }

                return true;
            }
        }
    }
}