using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.gui.gamecomponent", Category.GUI)]
    public static class H_GameComponentUpdateGUI
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods() => Utility.SubClassNonAbstractImplementationsOf(typeof(GameComponent), t => t.Name == "GameComponentOnGUI");
        public static string GetLabel(GameComponent __instance) => __instance.GetType().Name;
    }
}
