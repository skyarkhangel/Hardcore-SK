using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace Minerals
{
    public static class mapBuilder
    {

        public static void initRocks(Map map)
        {
            // Remove starting chunks
            if (MineralsMain.Settings.removeStartingChunksSetting)
            {
                removeStartingChunks(map);
            }

            List<string> spawned =  new List<string>();

            // Spawn static minerals
            foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs)
            {
                if (mineralType.newMapGenStep == "chunks" && mineralType.GetType() == typeof(ThingDef_StaticMineral) && (! spawned.Contains(mineralType.defName)))
                {
                    mineralType.InitNewMap(map);
                    spawned.Add(mineralType.defName);
                }
            }

            // spawn dynamic minerals
            foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs)
            {
                if (mineralType.newMapGenStep == "chunks" && mineralType.GetType() == typeof(ThingDef_DynamicMineral) && (! spawned.Contains(mineralType.defName)))
                {
                    mineralType.InitNewMap(map);
                    spawned.Add(mineralType.defName);
                }
            }

            // spawn large minerals
            foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs)
            {
                if (mineralType.newMapGenStep == "chunks" && mineralType.GetType() == typeof(ThingDef_BigMineral) && (! spawned.Contains(mineralType.defName)))
                {
                    mineralType.InitNewMap(map);
                    spawned.Add(mineralType.defName);
                }
            }

            // spawn everything else
            foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs)
            {
                if (mineralType.newMapGenStep == "chunks" && (! spawned.Contains(mineralType.defName)))
                {
                    mineralType.InitNewMap(map);
                }
            }
        }

        public static void initIce(Map map)
        {
            foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs)
            {
                if (mineralType.newMapGenStep == "plants")
                {
                    mineralType.InitNewMap(map);
                }
            }
            //Log.Message("Ice spawned");
        }


        public static void removeStartingChunks(Map map)
        {
            string[] toRemove = {"ChunkSandstone", "ChunkGranite", "ChunkLimestone", "ChunkSlate", "ChunkMarble", "ZF_ChunkBasalt", "ChunkClaystone", "Filth_RubbleRock", "AB_ChunkCragstone", "AB_ChunkMudstone", "AB_ChunkObsidian", "GU_ChunkRoseQuartz", "AB_ChunkSlimeStone", "ZF_ChunkMudstone"};
            List<Thing> thingsToCheck = map.listerThings.AllThings;
            for (int i = thingsToCheck.Count - 1; i >= 0; i--)
            {
                if (toRemove.Any(thingsToCheck[i].def.defName.Equals))
                {
                    thingsToCheck[i].Destroy(DestroyMode.Vanish);
                }
            }
        }

    }
}
