using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(ModContentPack))]
    [HarmonyPatch("AnyContentLoaded")]
    internal static class AnyContentLoaded_Patch
    {
        [HarmonyPostfix]
        private static void AnyContentLoaded(ModContentPack __instance, ref bool __result )
        {
            try
            {
                string curMod = __instance.RootDir + Path.DirectorySeparatorChar + "RimThemes";
                //Theme folder found
                if (Directory.Exists(curMod))
                {
                    __result = true;
                }
            }
            catch (Exception e)
            {
                Themes.LogError("ModContentPack.AnyContentLoaded patch failed : " + e.Message);
            }
        }
    }
}