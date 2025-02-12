using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 
using RimWorld.Planet;
using System.Runtime.CompilerServices;
using PatchOperationWhatHappened;
using SK.Enlighten;
using System.IO;
using Verse.Noise;
using Log = Verse.Log;
using SK;

namespace Minerals
{
    [HarmonyPatch(typeof(World), nameof(World.NaturalRockTypesIn))]
    public static class Minerals_World_NaturalRockTypesIn_Patch
    {
        private static List<ThingDef> allRockDefs;

        [HarmonyPostfix]
        public static IEnumerable<ThingDef> MakeRocksAccordingToBiome(IEnumerable<ThingDef> results, int tile)
        {
            if (allRockDefs.NullOrEmpty())
            {
                allRockDefs = DefDatabase<ThingDef>.AllDefs.Where(x =>
                                                                  (x.building?.buildingTags?.Contains("BaseRock") ?? false) && !x.IsSmoothed)
                                                                  .ToList();
            }
            // Pick a set of random rocks
            Rand.PushState();
            Rand.Seed = tile;
            int num = Rand.RangeInclusive(MineralsMain.Settings.terrainCountRangeSetting.min, MineralsMain.Settings.terrainCountRangeSetting.max);
            if (num > allRockDefs.Count)
            {
                num = allRockDefs.Count;
            }
            List<ThingDef> tempRockDefList = new List<ThingDef>(allRockDefs).Distinct().ToList();
            List<ThingDef> resultList = tempRockDefList.TakeRandom(num).ToList();
            for (int i = 0; i < num; i++)
            {
                yield return resultList[i];
            }
            Rand.PopState();
        }
    }

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        // this static constructor runs to create a HarmonyInstance and install a patch.
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("com.zacharyfoster.minerals");
            // Spawn rocks on map generation
            MethodInfo targetmethod = AccessTools.Method(typeof(GenStep_RockChunks), "Generate");
            HarmonyMethod postfixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("initNewMapRocks"));
            harmony.Patch(targetmethod, null, postfixmethod);
            // Spawn ice after plants
            MethodInfo icetargetmethod = AccessTools.Method(typeof(GenStep_Plants), "Generate");
            HarmonyMethod icepostfixmethod = new HarmonyMethod(typeof(HarmonyPatches).GetMethod("initNewMapIce"));
            harmony.Patch(icetargetmethod, null, icepostfixmethod);
            harmony.PatchAll();
        }

        public static void initNewMapRocks(GenStep_RockChunks __instance, Map map)
        {
            mapBuilder.initRocks(map);
        }

        public static void initNewMapIce(GenStep_RockChunks __instance, Map map)
        {
            mapBuilder.initIce(map);
        }

        [HarmonyPatch(typeof(SK.SkyfallerUtil))]
        [HarmonyPatch("SpawnSkyfaller")]
        [HarmonyPatch(new Type[] { typeof(ThingDef), typeof(IEnumerable<Thing>), typeof(IntVec3), typeof(Map) })]
        static class ImpactPatch
        {
            [HarmonyPrefix]
            public static void Prefix(ref IEnumerable<Thing> things)
            {
                List<Thing> replacementList = new List<Thing>();
                foreach (Thing item in things)
                {
                    Thing toReturn = item;
                    if (item.def.mineable && (!StaticMineral.isMineral(item)))
                    {
                        // check if any of the minerals replace this one 
                        foreach (ThingDef_StaticMineral mineralType in DefDatabase<ThingDef_StaticMineral>.AllDefs)
                        {
                            if (mineralType.ThingsToReplace == null || mineralType.ThingsToReplace.Count == 0)
                            {
                                continue;
                            }

                            if (mineralType.ThingsToReplace.Any(item.def.defName.Equals))
                            {
                                toReturn = (StaticMineral)ThingMaker.MakeThing(mineralType);
                                break;
                            }
                        }

                    }
                    replacementList.Add(toReturn);
                }
                things = replacementList;
            }
        }

        [HarmonyPatch(typeof(GenStep_PreciousLump))]
        [HarmonyPatch("Generate")]
        [HarmonyPatch(new Type[] { typeof(Map), typeof(GenStepParams) })]
        static class GenStep_PreciousLump_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(GenStep_PreciousLump __instance, Map map, GenStepParams parms)
            {
                if (parms.sitePart != null && parms.sitePart.parms.preciousLumpResources != null)
                    __instance.forcedDefToScatter = parms.sitePart.parms.preciousLumpResources;
                else
                    __instance.forcedDefToScatter = __instance.mineables.RandomElement<ThingDef>();

                if (__instance.forcedDefToScatter is ThingDef_StaticMineral)
                {
                    ThingDef_StaticMineral mineral = __instance.forcedDefToScatter as ThingDef_StaticMineral;
                    float averageDropAmount = 0;
                    float averageMarketValue = 0;
                    foreach (var item in mineral.randomlyDropResources)
                    {
                        averageDropAmount += item.DropProbability * item.CountPerDrop;
                        averageMarketValue += (item.DropProbability * item.CountPerDrop) * DefDatabase<ThingDef>.GetNamed(item.ResourceDefName).BaseMarketValue;
                    }
                    int count = mineral.randomlyDropResources.Count;
                    __instance.count = 1;
                    float randomRangeAmount = __instance.totalValueRange.RandomInRange;
                    int minimumLumps = Mathf.Max(Mathf.RoundToInt((averageMarketValue / __instance.totalValueRange.min) * 6), 2);
                    __instance.forcedLumpSize = Mathf.Max(Mathf.RoundToInt(randomRangeAmount /
                        ((averageDropAmount / count) * (averageMarketValue / count))), 1) + Rand.Range(minimumLumps, minimumLumps * 2);

                    float preRoundedValue = randomRangeAmount / (averageDropAmount / count) * (averageMarketValue / count);

                    //Log.Message("Calculation: " + randomRangeAmount + " / (" + averageDropAmount + " / " + count +") * (" + averageMarketValue + " / " + count + ") = " + preRoundedValue);
                    //Log.Message("Spawning Precious Lumps for: " + __instance.forcedDefToScatter.defName + ". Forced Lump size is: " + __instance.forcedLumpSize);

                    GenStep_ScatterLumpsMineable gen = new GenStep_ScatterLumpsMineable
                    {
                        forcedDefToScatter = __instance.forcedDefToScatter,
                        count = __instance.count,
                        forcedLumpSize = __instance.forcedLumpSize
                    };

                    gen.Generate(map, parms);

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CompLongRangeMineralScanner))]
        [HarmonyPatch("SetDefaultTargetMineral")]
        static class SetDefaultTargetMineral_Patch
        {
            [HarmonyPostfix]
            public static void Postfix(CompLongRangeMineralScanner __instance)
            {
                Traverse.Create(__instance).Field("targetMineable").SetValue(ThingDefOfLocal.SolidOreGold);
            }
        }
    }
}