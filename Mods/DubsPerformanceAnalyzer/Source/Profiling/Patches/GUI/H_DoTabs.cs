using System;
using System.Collections;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.gui.dotabs", Category.GUI)]
    internal class H_DoTabs
    {
        public static bool Active = false;

        public static string GetLabel() => "InspectPaneUtility - DoTabs";

        public static void ProfilePatch()
        {
            Modbase.Harmony.Patch(AccessTools.Method(typeof(InspectPaneUtility), "<DoTabs>g__Do|17_0"), new HarmonyMethod(typeof(H_DoTabs), "Prefix"), new HarmonyMethod(typeof(H_DoTabs), "Postfix"));

        }

        public static bool Prefix(MethodBase __originalMethod, InspectTabBase tab, ref string __state)
        {
            if (!Active) return true;

            __state = string.Empty;
            if (tab is InspectTabBase f)
            {
                __state = string.Intern($"{tab.GetType()} {tab.labelKey}");
            }
            else
            {
                __state = string.Intern($"{tab.GetType()}");
            }

            ProfileController.Start(__state, null, tab.GetType(), null, null, __originalMethod);

            return true;
        }

        public static void Postfix(string __state)
        {
            if (Active)
            {
                ProfileController.Stop(__state);
            }
        }
    }
}