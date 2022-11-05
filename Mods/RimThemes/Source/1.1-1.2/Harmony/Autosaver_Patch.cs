using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace aRandomKiwi.RimThemes
{
    [HarmonyPatch(typeof(Autosaver), nameof(Autosaver.DoAutosave)), StaticConstructorOnStartup]
    public class Autosaver_DoAutosave_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            LoaderGM.autosave = false;
        }
    }
}
