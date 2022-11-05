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
using System.Threading;
using System.Xml;
using System.IO;
using System.Runtime.CompilerServices;

namespace aRandomKiwi.RimThemes
{

    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML)), StaticConstructorOnStartup]
    class LoadedModManager_LoadModXmlPatch_Patch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            LoaderGM.nbCurModsLoaded = 0;
            LoaderGM.nbModsToLoad = LoadedModManager.RunningMods.Count();

            return true;
        }
    }


    [HarmonyPatch(typeof(ModContentPack), nameof(ModContentPack.LoadDefs)), StaticConstructorOnStartup]
    class ModContentPack_LoadDefsPatch_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ModContentPack __instance)
        {
            LoaderGM.nbCurModsLoaded += 1;
            LoaderGM.curStep = LoaderSteps.loadingXML;
            LoaderGM.curLoadedMod = __instance;

            return true;
        }
    }


    [HarmonyPatch(typeof(ModContentPack), "LoadPatches"), StaticConstructorOnStartup]
    class ModContentPack_LoadPatches_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ModContentPack __instance)
        {
            LoaderGM.nbPatches += 1;
            LoaderGM.curPatching = __instance;

            return true;
        }
    }


    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.CombineIntoUnifiedXML)), StaticConstructorOnStartup]
    class LoadedModManager_CombineIntoUnifiedXML_Patch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (LoaderGM.curStep != LoaderSteps.loadingXML)
                return true;

            LoaderGM.curStep = LoaderSteps.CombineXML;
            return true;
        }
    }

    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.ApplyPatches)), StaticConstructorOnStartup]
    class LoadedModManager_ApplyPatches_Patch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (LoaderGM.curStep != LoaderSteps.CombineXML)
                return true;

            LoaderGM.nbPatchesToLoad = LoadedModManager.RunningMods.Count();
            LoaderGM.curStep = LoaderSteps.Patching;

            return true;
        }
    }

    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.ParseAndProcessXML)), StaticConstructorOnStartup]
    class LoadedModManager_ParseAndProcessXML_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(XmlDocument xmlDoc)
        {
            if (LoaderGM.curStep != LoaderSteps.Patching)
                return true;

            LoaderGM.nbDefsToProcess = xmlDoc.DocumentElement.ChildNodes.Count;

            var enumerator = xmlDoc.DocumentElement.ChildNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (((XmlNode)enumerator.Current).NodeType == XmlNodeType.Element)
                {
                    LoaderGM.nbDefsToProcess++;
                }
            }
            LoaderGM.curStep = LoaderSteps.ParsingXML;

            return true;
        }
    }

    [HarmonyPatch(typeof(GenGeneric), "MethodOnGenericType", new Type[] { typeof(Type), typeof(Type), typeof(string) }), StaticConstructorOnStartup]
    class GenGeneric_MethodOnGenericType_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(Type genericParam, string methodName)
        {
            try
            {
                if ((LoaderGM.curStep == LoaderSteps.ParsingXML ||
                         LoaderGM.curStep == LoaderSteps.ResolvingReferences)
                        && genericParam.IsSubclassOf(typeof(Def))
                        && methodName == "ResolveAllReferences")
                {
                    LoaderGM.curDefResolving = genericParam;
                    LoaderGM.nbDefResolving++;   //nbDatabasesReloaded

                    if (LoaderGM.curStep != LoaderSteps.ResolvingReferences)
                    {
                        //Loading themes(only the conf where the loader is not pdisabled can access it)
                        Utils.startLoadingTheme();

                        LoaderGM.nbDefToResolving = typeof(Def).AllSubclasses().Count() - 1; //-1 because Def sub-class def
                        LoaderGM.curStep = LoaderSteps.ResolvingReferences;
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                Themes.LogError("GenGeneric.MethodOnGenericType patch failed : "+e.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(DirectXmlLoader), nameof(DirectXmlLoader.DefFromNode)), StaticConstructorOnStartup]
    class DirectXmlLoader_DefFromNode_Patch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            LoaderGM.nbDefs++;
            return true;
        }
    }

    [HarmonyPatch(typeof(XmlInheritance), nameof(XmlInheritance.TryRegister)), StaticConstructorOnStartup]
    class XmlInheritance_TryRegister_Patch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if (LoaderGM.curStep == LoaderSteps.ParsingXML)
                LoaderGM.nbDefs++;

            return true;
        }
    }

    [HarmonyPatch(typeof(StaticConstructorOnStartupUtility), nameof(StaticConstructorOnStartupUtility.CallAll)), StaticConstructorOnStartup]
    class StaticConstructorOnStartupUtility_CallAll_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        static bool Prefix()
        {
            //If custom loader disabled then loading themes at this loading level
            if (Settings.disableCustomLoader)
                Utils.startLoadingTheme();

            LoaderGM.curStep = LoaderSteps.FinishUp;
            LoaderGM.nbConstructorsToCall = GenTypes.AllTypesWithAttribute<StaticConstructorOnStartup>().Count();

            return true;
        }
    }

    [HarmonyPatch(typeof(RuntimeHelpers), nameof(RuntimeHelpers.RunClassConstructor), new Type[] { typeof(RuntimeTypeHandle) }), StaticConstructorOnStartup]
    class RuntimeHelpers_RunClassConstructor_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        static bool Prefix(RuntimeTypeHandle type)
        {
            try
            {
                //This patch is really sketchy as it's more than possible that this could be called in a million and one places.
                //Need to safeguard as much as is humanly possible.
                if (LoaderGM.curStep != LoaderSteps.FinishUp)
                    return true;
                var t = Type.GetTypeFromHandle(type);
                if (t.TryGetAttribute(out StaticConstructorOnStartup attrib))
                {
                    //We are calling the constructor of a StaticConstructorOnStartup-Annotated class. In theory.
                    LoaderGM.curConstructor = t;
                    LoaderGM.nbConstructorsCalled++;
                }

                return true;
            }
            catch(Exception e)
            {
                Themes.LogError("RuntimeHelpers.RunClassConstructor patch failed : "+e.Message);
                return true;
            }
        }
    }
}
