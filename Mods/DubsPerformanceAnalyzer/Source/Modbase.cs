using Analyzer.Performance;
using Analyzer.Profiling;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Verse;

namespace Analyzer 
{
    public class Modbase : Mod
    {
        public const int TIME_SINCE_CLOSE_FOR_CLEANUP = 30;

        public static Settings Settings;
        private static Harmony harmony = null;
        public static Harmony Harmony => harmony ??= new Harmony("Dubwise.DubsProfiler");
        private static Harmony staticHarmony = null;
        public static Harmony StaticHarmony => staticHarmony ??= new Harmony("Dubswise.PerformanceAnalyzer");

        // Major - Reworked functionality
        // Minor - New feature
        // Build - Change Existing Feature
        // Revision - Hotfix

        private static readonly Version analyzerVersion = new Version(1, 1, 2, 3);

        public static bool isPatched = false;

        public Modbase(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();

            ThreadSafeLogger.Message($"[Analyzer] Loaded version {analyzerVersion.Major}.{analyzerVersion.Minor}.{analyzerVersion.Build} rev {analyzerVersion.Revision}");


            { // Profiling
                ModInfoCache.PopulateCache(Content.Name);

                GUIController.InitialiseTabs();

                // GUI needs to be initialised before xml (the tabs need to exist for entries to be inserted into them)
                XmlParser.CollectXmlData();
            }

            { // Always Running
                StaticHarmony.Patch(AccessTools.Method(typeof(GlobalControlsUtility), nameof(GlobalControlsUtility.DoTimespeedControls)),
                    prefix: new HarmonyMethod(typeof(GUIElement_TPS), nameof(GUIElement_TPS.Prefix)));
            }

            { // Performance Patches
                PerformancePatches.InitialisePatches();
            }

#if DEBUG
            ThreadSafeLogger.Warning("==========================================================================");
            ThreadSafeLogger.Warning("                          Analyzer Running In Debug Mode                  ");
            ThreadSafeLogger.Warning("==========================================================================");
#endif

        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettings(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            // Any patches we had pending closing are now going to get closed
            PerformancePatches.ClosePatches();
        }

        public override string SettingsCategory()
        {
            return "Dubs Performance Analyzer";
        }
    }
}