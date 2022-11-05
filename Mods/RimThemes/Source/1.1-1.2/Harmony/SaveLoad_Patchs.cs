using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using HarmonyLib;
using System.Collections;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(World), nameof(World.ExposeData)), StaticConstructorOnStartup]
    class World_ExposeData_Patch
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            LoaderGM.curStep = LoaderSteps.LoadWorldMap;
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GenerateFromScribe)), StaticConstructorOnStartup]
    class WorldGenerator_GenerateFromScribe_Patch
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            LoaderGM.curStep = LoaderSteps.FillWorldMap;
            if (WorldGenerator.GenStepsInOrder != null)
                LoaderGM.nbWorldGenToRun = WorldGenerator.GenStepsInOrder.Count() - 2;
            else
                LoaderGM.nbWorldGenToRun = 0;
        }
    }

    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GenerateWithoutWorldData)), StaticConstructorOnStartup]
    class WorldGenerator_GenerateWithoutWorldData_Patch
    {
        [HarmonyPrefix]
        static void Prefix(string seedString)
        {
            LoaderGM.curStep = LoaderSteps.FillWorldMap;
            if (WorldGenerator.GenStepsInOrder != null)
                LoaderGM.nbWorldGenToRun = WorldGenerator.GenStepsInOrder.Count() - 2;
            else
                LoaderGM.nbWorldGenToRun = 0;
        }
    }



    [HarmonyPatch(typeof(WorldGenStep_Terrain), nameof(WorldGenStep_Terrain.GenerateFromScribe)), StaticConstructorOnStartup]
    public class WorldGenStep_Terrain_GenerateFromScribe_Patch
    {
        [HarmonyPostfix]
        public static void Prefix(WorldGenStep __instance, string seed)
        {
            LoaderGM.nbWorldGenRun++;
            LoaderGM.curWorldGenStep = __instance;
        }
    }

    [HarmonyPatch(typeof(WorldGenStep_Terrain), nameof(WorldGenStep_Terrain.GenerateFromScribe)), StaticConstructorOnStartup]
    public class WorldGenStep_Terrain_GenerateWithoutWorldData_Patch
    {
        [HarmonyPostfix]
        public static void Prefix(WorldGenStep __instance, string seed)
        {
            LoaderGM.nbWorldGenRun++;
            LoaderGM.curWorldGenStep = __instance;
        }
    }

    [HarmonyPatch(typeof(WorldGenStep_Components), nameof(WorldGenStep_Components.GenerateFromScribe)), StaticConstructorOnStartup]
    public class WorldGenStep_Components_GenerateFromScribe_Patch
    {
        [HarmonyPostfix]
        public static void Prefix(WorldGenStep __instance, string seed)
        {
            LoaderGM.nbWorldGenRun++;
            LoaderGM.curWorldGenStep = __instance;
        }
    }

    [HarmonyPatch(typeof(WorldGenStep_Components), nameof(WorldGenStep_Components.GenerateWithoutWorldData)), StaticConstructorOnStartup]
    public class WorldGenStep_Components_GenerateWithoutWorldData_Patch
    {
        [HarmonyPostfix]
        public static void Prefix(WorldGenStep __instance, string seed)
        {
            LoaderGM.nbWorldGenRun++;
            LoaderGM.curWorldGenStep = __instance;
        }
    }


    [HarmonyPatch(typeof(World), nameof(World.FinalizeInit)), StaticConstructorOnStartup]
    public class World_FinalizeInit_Patch
    {
        public static void Prefix()
        {
            //If previous finalizeWOrld when loading a map
            if (LoaderGM.curStep == LoaderSteps.FillWorldMap)
                LoaderGM.curStep = LoaderSteps.FinalizeWorld;
            else
                LoaderGM.curStep = LoaderSteps.CreateWorldFinalizeWorld;
        }
    }



    [HarmonyPatch(typeof(Map), nameof(Map.ExposeData)), StaticConstructorOnStartup]
    public class Map_ExposeData_Patch
    {
        public static void Prefix(Map __instance)
        {
            if (LoaderGM.curStep >= LoaderSteps.FinalizeWorld &&
               LoaderGM.curStep <= LoaderSteps.MapsLoadData)
            {
                LoaderGM.curStep = LoaderSteps.MapsInitComps;
            }
        }
    }

    [HarmonyPatch(typeof(Map), "ExposeComponents"), StaticConstructorOnStartup]
    public class Map_ExposeComponents_Patch
    {
        public static void Prefix()
        {
            if (LoaderGM.curStep == LoaderSteps.MapsInitComps)
            {
                LoaderGM.curStep = LoaderSteps.MapsLoadComps;
            }
        }
    }

    [HarmonyPatch(typeof(MapFileCompressor), nameof(MapFileCompressor.ExposeData)), StaticConstructorOnStartup]
    public class MapLoadCompressedPatch
    {
        public static void Prefix()
        {
            if (LoaderGM.curStep == LoaderSteps.MapsLoadComps)
            {
                LoaderGM.curStep = LoaderSteps.MapsLoadData;
            }
        }
    }

    [HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.Expose)), StaticConstructorOnStartup]
    public class CameraDriver_Expose_Patch
    {
        public static void Prefix()
        {
            if ((LoaderGM.curStep == LoaderSteps.MapsLoadData))
            {
                LoaderGM.curStep = LoaderSteps.SetCamera;
            }
        }
    }


    [HarmonyPatch(typeof(ScribeLoader), nameof(ScribeLoader.FinalizeLoading)), StaticConstructorOnStartup]
    public class ScribeLoader_FinalizeLoading_Patch
    {
        public static void Prefix()
        {
            if (LoaderGM.curStep != LoaderSteps.FillWorldMap)
                return;

            LoaderGM.curStep = LoaderSteps.ResolveSaveFileCrossReferences;

        }
    }


    [HarmonyPatch(typeof(Map), nameof(Map.FinalizeLoading)), StaticConstructorOnStartup]
    public class MapFinalizeLoadPatch
    {
        public static void Prefix(Map __instance)
        {
            LoaderGM.curStep = LoaderSteps.SpawnThings;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit)), StaticConstructorOnStartup]
    public class GameFinalizeInitPatch
    {
        public static void Prefix()
        {
            LoaderGM.curStep = LoaderSteps.FinalizeLoad;
        }
    }





    // World generation screen 
    [HarmonyPatch(typeof(LongEventHandler), nameof(LongEventHandler.QueueLongEvent), new Type[] { typeof(IEnumerable), typeof(string), typeof(Action<Exception>), typeof(bool) }), StaticConstructorOnStartup]
    public class LongEventHandler_QueueLongEvent_Patch
    {
        public static void Prefix(IEnumerable action, string textKey, Action<Exception> exceptionHandler = null, bool showExtraUIInfo = true)
        {
            if (textKey == "GeneratingPlanet")
                LoaderGM.curStep = LoaderSteps.GeneratingPlanet;
        }
    }

    [HarmonyPatch(typeof(LongEventHandler), nameof(LongEventHandler.QueueLongEvent), new Type[] { typeof(Action), typeof(string), typeof(string), typeof(bool), typeof(Action<Exception>), typeof(bool) }), StaticConstructorOnStartup]
    public class LongEventHandler_QueueLongEvent2_Patch
    {
        public static void Prefix(Action preLoadLevelAction, string levelToLoad, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler, bool showExtraUIInfo = true)
        {
            if (textKey == "GeneratingWorld")
                LoaderGM.curStep = LoaderSteps.CreateWorldGeneratingWorld;
            else if (textKey == "SavingLongEvent")
                LoaderGM.curStep = LoaderSteps.InitSaveSaving;
            else if (textKey == "GeneratingMap")
                LoaderGM.curStep = LoaderSteps.InitGeneratingMap;
            else if (textKey == "GeneratingMapForNewEncounter")
                LoaderGM.curStep = LoaderSteps.InitGeneratingMapForNewEncounter;
            else if (textKey == "Autosaving")
                LoaderGM.autosave = true;
        }
    }

    [HarmonyPatch(typeof(LongEventHandler), nameof(LongEventHandler.QueueLongEvent), new Type[] { typeof(Action), typeof(string), typeof(bool), typeof(Action<Exception>), typeof(bool) }), StaticConstructorOnStartup]
    public class LongEventHandler_QueueLongEvent3_Patch
    {
        public static void Prefix(Action action, string textKey, bool doAsynchronously, Action<Exception> exceptionHandler, bool showExtraUIInfo = true)
        {
            if (textKey == "Autosaving")
                LoaderGM.autosave = true;
        }
    }

    [HarmonyPatch(typeof(WorldRenderer), "RegenerateDirtyLayersNow_Async"), StaticConstructorOnStartup]
    public class WorldRenderer_RegenerateDirtyLayersNow_Async_Patch
    {
        public static void Prefix()
        {
            LoaderGM.curStep = LoaderSteps.FinalizeGeneratingPlanet;
        }
    }


    [HarmonyPatch(typeof(GameDataSaveLoader), nameof(GameDataSaveLoader.SaveGame)), StaticConstructorOnStartup]
    class ScribeSaver_FinalizeSaving_Patch
    {
        [HarmonyPostfix]
        static void Postfix(string fileName)
        {
            LoaderGM.curStep = LoaderSteps.FinalizeSaveSaving;
        }
    }


}