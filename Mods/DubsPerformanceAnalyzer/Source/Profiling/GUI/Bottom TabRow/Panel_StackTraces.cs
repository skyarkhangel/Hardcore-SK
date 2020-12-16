using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    class Panel_StackTraces
    {
        public static bool currentlyTracking = false;
        public static int currentTrackedStacktraces = 0;
        public static int currentGoalTrackedTraces = 100_000;
        public static string currentTrace = "";
        public static MethodInfo currentMethod = null;
        public static MethodInfo postfix = AccessTools.Method(typeof(Panel_StackTraces), nameof(StacktracePostfix));

        private static void Reset(MethodInfo oldMethod)
        {
            if (oldMethod == null) return; 

            currentTrace = "";
            StackTraceRegex.Reset();
            currentTrackedStacktraces = 0;
            Modbase.Harmony.CreateProcessor(oldMethod).Unpatch(postfix);
            currentlyTracking = false;
        }

        public static void Draw(Rect rect, GeneralInformation? info)
        {
            if (info == null || info.Value.method == null) return;
            var method = info.Value.method as MethodInfo;

            if (currentMethod != method)
            {
                Reset(currentMethod);
                currentMethod = method;
            }

            var enableBox = rect.TopPartPixels(30.0f);

            var checkBoxWidth = DrawCheckbox(ref enableBox, method);
            enableBox.AdjustHorizonallyBy(checkBoxWidth);

            var unpatchWidth = DrawUnpatch(ref enableBox, method);
            enableBox.AdjustHorizonallyBy(unpatchWidth);

            var strings = DrawSelectStacktrace(ref enableBox, method);

            rect.AdjustVerticallyBy(30f);

            StringBuilder sb = new StringBuilder();

            foreach(var str in strings)
            {
                sb.AppendLine(str);
            }

            Widgets.Label(rect, sb.ToString().TrimEndNewlines());
        }

        private static float DrawCheckbox(ref Rect rect, MethodInfo meth)
        {
            var checkBoxWidth = 30.0f + $"Enable for {meth.Name}".GetWidthCached();
            var checkBox = rect.LeftPartPixels(checkBoxWidth);

            if (DubGUI.Checkbox(checkBox, $"Enable for {meth.Name}", ref currentlyTracking))
            {
                if (currentlyTracking)
                {
                    Modbase.Harmony.Patch(meth, postfix: new HarmonyMethod(postfix));
                    StackTraceRegex.Reset();
                    currentTrace = "";
                    currentTrackedStacktraces = 0;
                }
                else
                {
                    Modbase.Harmony.CreateProcessor(meth).Unpatch(postfix);
                }
            }

            return checkBoxWidth;
        }

        private static float DrawUnpatch(ref Rect rect, MethodInfo meth)
        {
            var width = "   Disable   ".GetWidthCached();
            var box = rect.LeftPartPixels(width);

            if (Widgets.ButtonInvisible(box))
            {
                Modbase.Harmony.CreateProcessor(meth).Unpatch(postfix);
                currentlyTracking = false;
            }

            Widgets.DrawHighlightIfMouseover(box);

            var anch = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(box, "Disable");
            Text.Anchor = anch;

            return width;
        }

        private static string[] DrawSelectStacktrace(ref Rect rect, MethodInfo info)
        {
            if (StackTraceRegex.traces.Count == 0) return new string[] {""};

            if (Widgets.ButtonInvisible(rect))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                foreach (var trace in StackTraceRegex.traces.OrderBy(p => p.Value.Count).Reverse())
                {
                    var digestibleKey = trace.Value.TranslatedArr().First();

                    options.Add(new FloatMenuOption($"{digestibleKey} : {trace.Value.Count}", () =>
                    {
                        currentTrace = trace.Key;
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            Widgets.DrawHighlightIfMouseover(rect);
            var anch = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, "Select Stack Trace");
            Text.Anchor = anch;

            if (currentTrace == "")
                return StackTraceRegex.traces.Values
                    .OrderBy(p => p.Count)
                    .Last()
                    .TranslatedArr();

            return StackTraceRegex.traces[currentTrace].TranslatedArr();
        }


        
        // This will be added as a Postfix to the method which we want to gather stack trace information for
        // it will only effect one method, so we can skip the check, and it will not slow down other profilers
        // because it will only be patched onto one method. There can be extra checks and flexibility in how
        // many frames are grabbed p/s etc. These are to be done when the GUI decisions have been made.

        public static void StacktracePostfix(MethodBase __originalMethod)
        {
            if (++currentTrackedStacktraces < currentGoalTrackedTraces) StackTraceRegex.Add(new StackTrace(2, false));
            else currentlyTracking = false;
        }

    }
}
