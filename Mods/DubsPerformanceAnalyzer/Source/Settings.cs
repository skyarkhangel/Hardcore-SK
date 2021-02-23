using Analyzer.Performance;
using Analyzer.Profiling;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine;
using Verse;

namespace Analyzer
{
    public class Settings : ModSettings
    {
        public Color timeColour = new Color32(79, 147, 191, 255);
        public Color callsColour = new Color32(10, 10, 255, 255);
        public Color GraphCol = new Color32(17, 17, 17, 255);

        // Developer settings
        public static string @PathToDnspy = "";
        public static float updatesPerSecond = 2;
        public static bool verboseLogging = false;
        public static bool disableCleanup = false;
        public static bool disableTPSCounter = false;

        // Performance Settings are held in the type which implements the optimisation

        public override void ExposeData()
        {
            base.ExposeData();

            
            Scribe_Values.Look(ref GraphSettings.lineAliasing, "lineAliasing", 7.5f);
            Scribe_Values.Look(ref GraphSettings.showMax, "showMax", false);
            Scribe_Values.Look(ref GraphSettings.showAxis, "showAxis", true);
            Scribe_Values.Look(ref GraphSettings.showGrid, "showGrid", true);


            Scribe_Values.Look(ref timeColour, "timeColour", new Color32(79, 147, 191, 255));
            Scribe_Values.Look(ref callsColour, "callsColour", new Color32(10, 10, 255, 255));
            Scribe_Values.Look(ref GraphCol, "GraphCol", new Color32(17, 17, 17, 255));
            Scribe_Values.Look(ref PathToDnspy, "dnspyPath");
            Scribe_Values.Look(ref updatesPerSecond, "updatesPerSecond", 2);
            Scribe_Values.Look(ref verboseLogging, "verboseLogging", false);
            Scribe_Values.Look(ref disableCleanup, "disableCleanup", false);
            Scribe_Values.Look(ref disableTPSCounter, "disableTPSCounter", false);

            // We save/load all performance-related settings here.
            PerformancePatches.ExposeData();
        }

        public void DoSettings(Rect canvas)
        {
            if (Event.current.type == EventType.Layout) return;

            Panel_Settings.Draw(canvas, true);
        }
    }
}
