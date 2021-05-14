using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.gamecomponent", Category.Update)]
    public static class H_GameComponentUpdate
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods() => Utility.SubClassNonAbstractImplementationsOf(typeof(GameComponent), t => t.Name == "GameComponentUpdate");
        public static string GetLabel(GameComponent __instance) => __instance.GetType().Name;
    }

}