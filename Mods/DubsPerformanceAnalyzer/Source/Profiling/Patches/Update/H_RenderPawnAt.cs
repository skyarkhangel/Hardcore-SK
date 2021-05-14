using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.pawnrenderer", Category.Update)]
    internal class H_RenderPawnAt
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt), new Type[] { typeof(Vector3), typeof(RotDrawMode), typeof(bool), typeof(bool)});
        }
        public static string GetLabel(PawnRenderer __instance) => $"{__instance.pawn.Label} - {__instance.pawn.ThingID}";
        public static string GetName(PawnRenderer __instance) => __instance.pawn.GetHashCode().ToString();
    }
}