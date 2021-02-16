using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.lord", Category.Tick)]
    internal class H_GetLord
    {
        public static bool Active = false;

        public static bool Fringe(MethodBase __originalMethod, ThinkNode_Priority __instance, Pawn pawn, JobIssueParams jobParams, ref string __state, ref ThinkResult __result)
        {
            if (Active)
            {
                int count = __instance.subNodes.Count;
                for (int i = 0; i < count; i++)
                {
                    ThinkResult result = ThinkResult.NoJob;
                    Profiler prof = null;
                    try
                    {
                        __state = $"ThinkNode_Priority SubNode [{__instance.subNodes[i].GetType()}]";
                        prof = ProfileController.Start(__state, null, null, null, null, __originalMethod);
                        result = __instance.subNodes[i].TryIssueJobPackage(pawn, jobParams);
                        prof.Stop();
                    }
                    catch (Exception)
                    {
                        prof.Stop();
                    }
                    if (result.IsValid)
                    {
                        __result = result;
                        return false;
                    }
                }
                __result = ThinkResult.NoJob;

                return false;
            }
            return true;
        }

        public static void ProfilePatch()
        {
            Modbase.Harmony.Patch(AccessTools.Method(typeof(ThinkNode_Priority), nameof(ThinkNode_Priority.TryIssueJobPackage)), new HarmonyMethod(typeof(H_GetLord), nameof(Fringe)));

            MethodTransplanting.UpdateMethods(typeof(H_GetLord), Utility.GetTypeMethods(typeof(Lord)).ToList());
        }
    }
}