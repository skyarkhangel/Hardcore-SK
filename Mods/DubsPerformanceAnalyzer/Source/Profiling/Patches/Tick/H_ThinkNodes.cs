using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Analyzer.Profiling
{
    [Entry("entry.tick.thinknodes", Category.Tick)]
    internal static class H_ThinkNodes
    {
        public static bool Active = false;
        public static List<MethodInfo> patched = new List<MethodInfo>();

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            foreach (Type typ in GenTypes.AllTypes)
            {
                if (typeof(ThinkNode_JobGiver).IsAssignableFrom(typ))
                {
                    MethodInfo trygive = AccessTools.Method(typ, nameof(ThinkNode_JobGiver.TryGiveJob));
                    if (!trygive.DeclaringType.IsAbstract && trygive.DeclaringType == typ)
                    {
                        if (!patched.Contains(trygive))
                        {
                            yield return trygive;
                            patched.Add(trygive);
                        }
                    }
                }
                else if (typeof(ThinkNode).IsAssignableFrom(typ))
                {
                    MethodInfo mef = AccessTools.Method(typ, nameof(ThinkNode.TryIssueJobPackage));

                    if (!mef.DeclaringType.IsAbstract && mef.DeclaringType == typ)
                    {
                        if (!patched.Contains(mef))
                        {
                            yield return mef;
                            patched.Add(mef);
                        }
                    }
                }
            }
        }
    }
}