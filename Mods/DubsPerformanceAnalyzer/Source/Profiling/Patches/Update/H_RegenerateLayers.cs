using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Analyzer.Profiling
{
    [StaticConstructorOnStartup]
    internal class H_RegenerateLayers
    {
        public static Entry p = Entry.Create("entry.update.sections", Category.Update, typeof(H_RegenerateLayers), false);

        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods() => typeof(SectionLayer).AllSubclasses().Select(sl => sl.GetMethod("Regenerate"));
        public static string GetLabel(SectionLayer __instance) => __instance.GetType().FullName;
    }
}