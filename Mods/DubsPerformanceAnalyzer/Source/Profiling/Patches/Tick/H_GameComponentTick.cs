using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.gamecomponent", Category.Tick)]
    public static class H_GameComponent
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods() => Utility.SubClassNonAbstractImplementationsOf(typeof(GameComponent), t => t.Name == "GameComponentTick");
        public static string GetLabel(GameComponent __instance) => __instance.GetType().Name;
    }
}

