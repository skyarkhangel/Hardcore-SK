using System;
using Analyzer.Performance;
using Analyzer.Profiling;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Analyzer
{
    [StaticConstructorOnStartup]
    internal static class H_KeyPresses
    {
        private static KeyBindingDef key;
        private static KeyBindingDef restartkey;
        private static KeyBindingDef alertKey;

        static H_KeyPresses()
        {
            key = KeyBindingDef.Named("DubsOptimizerKey");
            restartkey = KeyBindingDef.Named("DubsOptimizerRestartKey");
            alertKey = KeyBindingDef.Named("dpa_ToggleAlertBlock");
            PatchMe(Modbase.StaticHarmony);
        }

        public static void PatchMe(Harmony harmony)
        {
            var biff = new HarmonyMethod(typeof(H_KeyPresses).GetMethod(nameof(OnGUI)));
            var skiff = typeof(UIRoot_Entry).GetMethod(nameof(UIRoot_Entry.UIRootOnGUI));
            harmony.Patch(skiff, biff);

            skiff = typeof(UIRoot_Play).GetMethod(nameof(UIRoot_Play.UIRootOnGUI));
            harmony.Patch(skiff, biff);
        }

        public static void OnGUI()
        {
            if (restartkey.KeyDownEvent)
            {
                GenCommandLine.Restart();
            }

            try
            {
                if (key != null && key.KeyDownEvent)
                {
                    if (Find.WindowStack.WindowOfType<Window_Analyzer>() != null)
                    {
                        Find.WindowStack.RemoveWindowsOfType(typeof(Window_Analyzer));
                    }
                    else
                    {
                        Find.WindowStack.Add(new Window_Analyzer());
                    }
                }
            }
            catch (Exception e)
            {
                ThreadSafeLogger.Error(e.ToString());
            }

            if (alertKey.KeyDownEvent)
            {
                H_AlertsReadoutUpdate.DisableAlerts = !H_AlertsReadoutUpdate.DisableAlerts;
            }
        }
    }
}
