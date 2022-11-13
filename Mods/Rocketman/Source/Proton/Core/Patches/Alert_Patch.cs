using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RocketMan;
using Verse;

namespace Proton
{
    [ProtonPatch]
    public class Alert_Constructor_Patch
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (Type type in typeof(Alert).AllLeafSubclasses())
            {
                if (!(type == typeof(Alert_Custom)) && !(type == typeof(Alert_CustomCritical)))
                {
                    MethodBase target = AccessTools.Constructor(type);
                    if (target.IsValidTarget())
                        yield return target;
                }
            }
        }

        public static void Postfix(Alert __instance)
        {
            if (Context.TypeIdToSettings.TryGetValue(__instance.GetType().FullName, out AlertSettings settings))
            {
                settings.alert = __instance;
            }
        }
    }
}
