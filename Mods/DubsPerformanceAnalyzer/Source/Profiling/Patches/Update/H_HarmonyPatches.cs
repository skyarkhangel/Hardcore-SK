using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.harmonypatches", Category.Update)]
    internal class H_HarmonyPatches
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            foreach (MethodBase mode in Harmony.GetAllPatchedMethods().ToList())
            {
                Patches patchInfo = Harmony.GetPatchInfo(mode);
                foreach (var fix in patchInfo.Prefixes.Concat(patchInfo.Postfixes).Where(f => Utility.IsNotAnalyzerPatch(f.owner)))
                {
                    yield return fix.PatchMethod;
                }  
            }
        }

        public static void ProfilePatch()
        {
            try
            {
                MethodTransplanting.PatchMethods(typeof(H_HarmonyPatches));
            }
            catch(Exception e)
            {
                ThreadSafeLogger.Error("Patching HarmonyPatches failed, errored with the message" + e.Message);
            }
        }
    }
}