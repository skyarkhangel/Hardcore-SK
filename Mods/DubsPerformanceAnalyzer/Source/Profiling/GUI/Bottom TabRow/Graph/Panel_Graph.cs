using ColourPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    internal struct GraphEntry
    {
        public float max;
        public float absMax;
        public bool visible;
        public float[] entries;
    }

    internal class GraphSettings
    {
        public static bool showAxis = true;
        public static bool showGrid = true;
        public static bool showMax = false;
        public static float lineAliasing = 7.5f; // Tweak if lines are merging too aggressively


        public bool dragging = false;
        public Vector2 dragAnchor = new Vector2();
        public Vector2 offsets = new Vector2(0,0);
    }


    public class Panel_Graph
    {
        internal GraphEntry calls = new GraphEntry { entries = new float[Profiler.RECORDS_HELD] };
        internal GraphEntry times = new GraphEntry { entries = new float[Profiler.RECORDS_HELD], visible = true };
        internal GraphSettings settings = new GraphSettings();

        private int entryCount = 300;


        // value = 0 - time, 1 - calls, 2 - background
        public static void DisplayColorPicker(Rect rect, int value)
        {
            Color32 col = new Color32();
            if (value == 0) col = Modbase.Settings.timeColour;
            else if (value == 1) col = Modbase.Settings.callsColour;
            else col = Modbase.Settings.GraphCol;

            Widgets.DrawBoxSolid(rect, col);

            if (!Widgets.ButtonInvisible(rect, true)) return;

            if (Find.WindowStack.WindowOfType<colourPicker>() != null)
            {
                Find.WindowStack.RemoveWindowsOfType(typeof(colourPicker));
                return;
            }

            colourPicker cp = new colourPicker();
            if (value == 0) cp.Setcol = () => Modbase.Settings.timeColour = colourPicker.CurrentCol;
            else if (value == 1) cp.Setcol = () => Modbase.Settings.callsColour = colourPicker.CurrentCol;
            else cp.Setcol = () => Modbase.Settings.GraphCol = colourPicker.CurrentCol;

            cp.SetColor(col);

            Find.WindowStack.Add(cp);
        }

        private static void DrawButton(Panel_Graph instance, Rect rect, string str, int idx)
        {
            var iconRect = rect.LeftPartPixels(20f);
            iconRect.height = 10;
            iconRect.x += 5;
            iconRect.width -= 10;
            iconRect.y += (rect.height / 2.0f) - 5f;
            rect.AdjustHorizonallyBy(20f);

            DisplayColorPicker(iconRect, idx);

            if (idx == 0 && !instance.times.visible) GUI.color = Color.grey;
            else if(idx == 1 && !instance.calls.visible) GUI.color = Color.grey;
            
            Widgets.Label(rect, str);

            GUI.color = Color.white;

            if (idx == 2) return;

            if (Widgets.ButtonInvisible(rect))
            {
                if (idx == 0) instance.times.visible = !instance.times.visible;
                else instance.calls.visible = !instance.calls.visible;
            }

            Widgets.DrawHighlightIfMouseover(rect);
        }

        private static void DrawSettings(Panel_Graph instance, ref Rect position)
        {
            // [ - Calls ] [ - Times ] [ Lines ] [ Entries ------ ] [ - Bg Col ]

            var width = position.width;
            var currentHeight = 32;
            var currentSlice = position.TopPartPixels(currentHeight);
            position.AdjustVerticallyBy(currentHeight);

            Text.Anchor = TextAnchor.MiddleCenter;

            // [ - Times ]
            var str = " Times ";
            var contentWidth = 20 + str.GetWidthCached();
            var rect = currentSlice.LeftPartPixels(contentWidth);
            currentSlice.AdjustHorizonallyBy(contentWidth);

            DrawButton(instance, rect, " Times ", 0);

            // [ - Calls ]
            str = " Calls ";
            contentWidth = 20 + str.GetWidthCached();
            rect = currentSlice.LeftPartPixels(contentWidth);
            currentSlice.AdjustHorizonallyBy(contentWidth);
            
            DrawButton(instance, rect, " Calls ", 1);

            // [ - Background ]
            str = " Background ";
            contentWidth = 20 + str.GetWidthCached();
            rect = currentSlice.LeftPartPixels(contentWidth);
            currentSlice.AdjustHorizonallyBy(contentWidth);
            
            DrawButton(instance, rect, " Background ", 2);

            Text.Anchor = TextAnchor.UpperLeft;

            // [ - Entries ] 
            contentWidth = 150;
            if (currentSlice.width < contentWidth)
            {
                currentSlice = position.TopPartPixels(currentHeight);
                position.AdjustVerticallyBy(currentHeight);
            }

            rect = currentSlice.LeftPartPixels(contentWidth);
            instance.entryCount = (int)Widgets.HorizontalSlider(rect.BottomPartPixels(30f), instance.entryCount, 10, 2000, true, string.Intern($"{instance.entryCount} Entries  "));

            currentSlice.AdjustHorizonallyBy(contentWidth);


            Text.Anchor = TextAnchor.MiddleCenter;

            // [ - Show Axis ] 
            str = " Axis";
            contentWidth = str.GetWidthCached() + 30;
            if (currentSlice.width < contentWidth)
            {
                currentSlice = position.TopPartPixels(currentHeight);
                position.AdjustVerticallyBy(currentHeight);
            }

            rect = currentSlice.LeftPartPixels(contentWidth);
            DubGUI.Checkbox(rect, str, ref GraphSettings.showAxis);
            currentSlice.AdjustHorizonallyBy(contentWidth);

            // [ - Show Grid ] 
            str = " Grid " + 30;
            contentWidth = str.GetWidthCached();
            if (currentSlice.width < contentWidth)
            {
                currentSlice = position.TopPartPixels(currentHeight);
                position.AdjustVerticallyBy(currentHeight);
            }

            rect = currentSlice.LeftPartPixels(contentWidth);
            DubGUI.Checkbox(rect, str, ref GraphSettings.showGrid);
            currentSlice.AdjustHorizonallyBy(contentWidth);

            // [ - Show Max ] 
            str = " Max ";
            contentWidth = str.GetWidthCached() + 30;
            if (currentSlice.width < contentWidth)
            {
                currentSlice = position.TopPartPixels(currentHeight);
                position.AdjustVerticallyBy(currentHeight);
            }

            rect = currentSlice.LeftPartPixels(contentWidth);
            DubGUI.Checkbox(rect, str, ref GraphSettings.showMax);
            currentSlice.AdjustHorizonallyBy(contentWidth);

            // [ - Aliasing ] 
            str = " Aliasing: " + (GraphSettings.lineAliasing == 0 ? "none" : GraphSettings.lineAliasing.ToString()) + " ";
            contentWidth = str.GetWidthCached();
            if (currentSlice.width < contentWidth)
            {
                currentSlice = position.TopPartPixels(currentHeight);
                position.AdjustVerticallyBy(currentHeight);
            }

            rect = currentSlice.LeftPartPixels(contentWidth);
            if(Widgets.ButtonText(rect, str, false))
            {
                GraphSettings.lineAliasing = GraphSettings.lineAliasing switch
                {
                    7.5f => 12.5f, 
                    12.5f => 0.0f,
                    0.0f => 5.0f,
                    5.0f => 7.5f,
                    _ => 0.0f
                };
            }

            currentSlice.AdjustHorizonallyBy(contentWidth);

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public void Draw(Rect rect)
        {
            if (GUIController.CurrentProfiler == null) return;

            DrawSettings(this, ref rect);

            DrawGraph(rect);
        }

        internal int SetupArrays()
        {
            var entries = Mathf.Min(Analyzer.GetCurrentLogCount, entryCount);
            settings.offsets.x = Mathf.Clamp(settings.offsets.x, 0, entryCount);


            int i = entries;
            var prof = GUIController.CurrentProfiler;
            uint arrayIndex = prof.currentIndex;

            // arrayIndex = 300
            // offset = 400
            // correct starting position = (RecordsHeld) - (offset - arrayIndex)

            // arrayIndex = 1700
            // offset = 1600
            // correct starting position = arrayIndex - offset

            if (arrayIndex < settings.offsets.x) arrayIndex = (Profiler.RECORDS_HELD) - ((uint)settings.offsets.x - arrayIndex);
            else arrayIndex -= (uint)settings.offsets.x;
            

            var callsMax = 0;
            var timesMax = 0.0f;

            while (i > 0)
            {
                var timeEntry = (float)prof.times[arrayIndex];
                var hitsEntry = GUIController.CurrentEntry.type == typeof(H_HarmonyTranspilers) ? 0 : prof.hits[arrayIndex];

                calls.entries[i-1] = hitsEntry;
                times.entries[i-1] = timeEntry;

                if (callsMax < hitsEntry) callsMax = hitsEntry;
                if (timesMax < timeEntry) timesMax = timeEntry;

                i--;
                arrayIndex--;
                if (arrayIndex > Profiler.RECORDS_HELD) arrayIndex = Profiler.RECORDS_HELD - 1;
            }

            if (calls.max > callsMax) calls.max -= (calls.max - callsMax) / 120.0f;
            else calls.max = callsMax;

            calls.absMax = callsMax;

            if (times.max > timesMax) times.max -= (times.max - timesMax) / 120.0f;
            else times.max = timesMax;

            times.absMax = timesMax;

            return entries;
        }

        private void DrawGraph(Rect rect)
        {
            var count = SetupArrays();


            GraphDrawer.DrawGraph(this, rect, count);
        }

    }
}
