using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.gui.thingoverlay.ongui", Category.GUI)]
    internal class H_ThingOverlaysOnGUI
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods() { yield return AccessTools.Method(typeof(Pawn), nameof(Pawn.DrawGUIOverlay)); }
        public static string GetLabel() => "ThingOverlaysOnGUI";
    }
}