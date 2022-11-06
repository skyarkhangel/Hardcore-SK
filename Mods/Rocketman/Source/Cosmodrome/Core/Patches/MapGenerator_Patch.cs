using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public static class MapGenerator_Patch
    {
        [RocketPatch(typeof(MapGenerator), nameof(MapGenerator.GenerateMap))]
        public static class GenerateMap_Patch
        {
            public static void Prefix(ref IntVec3 mapSize, MapParent parent)
            {
                try
                {
                    if ((Find.Maps?.Any(m => m.IsPlayerHome) ?? false) && Faction.OfPlayerSilentFail != null)
                    {
                        World world = Find.World;
                        WorldInfoComponent comp = world?.GetComponent<WorldInfoComponent>() ?? null;
                        if (comp != null && comp.useCustomMapSizes)
                        {
                            Vector3 vector = comp.IntialMapSize;
                            mapSize.x = (int)vector.x;
                            mapSize.y = (int)vector.y;
                            mapSize.z = (int)vector.z;

                            comp.useCustomMapSizes = false;
                            Log.Message($"ROCKETMAN: Applied custom map size for new settelment/map");
                        }
                    }
                }
                catch (Exception er)
                {
                    Logger.Debug("ROCKETMAN: Caught error while generating map", er);
                    if (RocketEnvironmentInfo.IsDevEnv && Prefs.DevMode)
                        throw er;
                }
            }
        }
    }
}
