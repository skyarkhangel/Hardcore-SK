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



    /* Replace slate with basalt in the Alpha biomes Pyroclastic Conflagration biome*/
    [HarmonyPatch(typeof(World))]
    [HarmonyPatch("NaturalRockTypesIn")]
    public static class Minerals_World_NaturalRockTypesIn_Patch
    {

        [HarmonyPostfix]
        public static void MakeRocksAccordingToBiome(int tile, ref World __instance, ref IEnumerable<ThingDef> __result)
        {
            if (__instance.grid.tiles[tile].biome.defName == "AB_PyroclasticConflagration")
            {
                List<ThingDef> replacedList = new List<ThingDef>();
                ThingDef item = DefDatabase<ThingDef>.GetNamed("AB_Obsidianstone");
                replacedList.Add(item);
                replacedList.Add(DefDatabase<ThingDef>.GetNamed("ZF_BasaltBase"));

                __result = replacedList;
            }
            else if (__instance.grid.tiles[tile].biome.defName == "AB_OcularForest" || __instance.grid.tiles[tile].biome.defName == "AB_GallatrossGraveyard" || __instance.grid.tiles[tile].biome.defName == "AB_GelatinousSuperorganism" || __instance.grid.tiles[tile].biome.defName == "AB_MechanoidIntrusion" || __instance.grid.tiles[tile].biome.defName == "AB_RockyCrags") {
                return;
            }
            else
            {
                // Pick a set of random rocks
                Rand.PushState();
                Rand.Seed = tile;

                // Disabled since found great impact on tps. Skyarkhangel. 01.04.2024.
                // Made list of stones hardcoded, until not found solution.

                //List<ThingDef> list = (from d in DefDatabase<ThingDef>.AllDefs
                //                       where d.category == ThingCategory.Building && d.building.isNaturalRock && !d.building.isResourceRock &&
                //                       !d.IsSmoothed && d.defName != "GU_RoseQuartz" && d.defName != "AB_SlimeStone" &&
                //                       d.defName != "GU_AncientMetals" && d.defName != "AB_Cragstone" && d.defName != "AB_Obsidianstone" &&
                //                       d.defName != "BiomesIslands_CoralRock" && d.defName != "LavaRock" && d.defName != "AB_Mudstone"
                //                       select d).ToList<ThingDef>();

                List<ThingDef> list = new List<ThingDef>
                {
                    ThingDefOf.Sandstone,
                    ThingDefOf.Granite,
                    ThingDef.Named("Slate"),
                    ThingDef.Named("Limestone"),
                    ThingDef.Named("Marble"),
                    ThingDef.Named("ZF_BasaltBase"),
                    ThingDef.Named("ZF_ClaystoneBase"),
                    ThingDef.Named("ZF_MudstoneBase")
                };

                int num = Rand.RangeInclusive(MineralsMain.Settings.terrainCountRangeSetting.min, MineralsMain.Settings.terrainCountRangeSetting.max);
                if (num > list.Count)
                {
                    num = list.Count;
                }
                List<ThingDef> list2 = new List<ThingDef>();
                for (int i = 0; i < num; i++)
                {
                    ThingDef item = list.RandomElement<ThingDef>();
                    list.Remove(item);
                    list2.Add(item);
                }
                Rand.PopState();
                __result = list2;
            }
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
        [HarmonyPatch(new Type[] {typeof(ThingDef), typeof(IEnumerable<Thing>), typeof(IntVec3), typeof(Map) })]
        static class ImpactPatch
        {
            [HarmonyPrefix]
            public static void Prefix(ref IEnumerable<Thing> things)
            {
                List<Thing> replacementList = new List<Thing>();
                foreach (Thing item in things)
                {
                    Thing toReturn = item;
                    if (item.def.mineable && (! StaticMineral.isMineral(item)))
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
                                toReturn = (StaticMineral) ThingMaker.MakeThing(mineralType);
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
        [HarmonyPatch(new Type[] { typeof(Map), typeof(GenStepParams)})]
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
                    int minimumLumps = Mathf.Max(Mathf.RoundToInt((averageMarketValue / __instance.totalValueRange.min) * 6),2);
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

                    gen.Generate(map,parms);

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
                Traverse.Create(__instance).Field("targetMineable").SetValue(DefDatabase<ThingDef>.GetNamed("SolidOreGold"));
            }
        }
    }
}