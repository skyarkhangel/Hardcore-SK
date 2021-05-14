using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    public static class GraphDrawer
    {
        public static Color gray = new Color(0.5f, 0.5f, 0.5f, .7f);

        public static void DrawGraph(Panel_Graph instance, Rect rect, int entries)
        {
            GUI.BeginGroup(rect);
            rect = rect.AtZero();

            AdjustDimensions(instance, rect, entries);

            if (Event.current.type != EventType.Repaint) 
            {
                GUI.EndGroup();
                return;
            }

            DrawBackground(rect, GraphSettings.showGrid, GraphSettings.showAxis);

            var primaryEntry = instance.times.visible ? instance.times : instance.calls;

            if (GraphSettings.showMax)
            {
                DrawMaxLine(rect, primaryEntry.absMax, primaryEntry.max, instance.times.visible ? "ms" : "calls");
            }

            if (GraphSettings.showAxis)
            {
                DrawAxis(rect, entries, (int)instance.settings.offsets.x, primaryEntry.max, instance.times.visible ? "ms" : "calls");

                rect.x += 25;
                rect.width -= 25;
                rect.height -= Text.LineHeight;

                Text.Font = GameFont.Small;

                GUI.BeginGroup(rect);
                rect = rect.AtZero();
            }

            var calls = instance.calls;
            var times = instance.times;

            DrawEntries(rect, calls, times, entries, GraphSettings.lineAliasing);

            if(GraphSettings.showAxis) GUI.EndGroup();

            GUI.EndGroup();
        }

        internal static void DrawEntries(Rect rect, GraphEntry calls, GraphEntry times, int entries, float aliasing)
        {
            var xIncrement = rect.width / (entries - 1.0f);

            var timeCutoff = 0.0f;
            var callsCutoff = 0.0f;

            if (aliasing != 0)
            {
                timeCutoff = (calls.max / rect.height) / aliasing;
                callsCutoff = (times.max / rect.height) / aliasing;
            }

            int i = 1, timesIndex = 0, callsIndex = 0;
                
            for (; i < entries; i++)
            {
                if (calls.visible)
                {
                    if (Mathf.Abs(calls.entries[callsIndex] - calls.entries[i]) > callsCutoff || i == entries - 1) // We need to draw a line, enough of a difference
                    {
                        DrawLine(ref calls, callsIndex, i, rect.height, xIncrement, Modbase.Settings.callsColour);
                        
                        callsIndex = i;
                    }
                }

                if (times.visible)
                {
                    if (Mathf.Abs(times.entries[timesIndex] - times.entries[i]) > timeCutoff || i == entries - 1)
                    {
                        DrawLine(ref times, timesIndex, i, rect.height, xIncrement, Modbase.Settings.timeColour);

                        timesIndex = i;
                    }
                }

            }
        }

        internal static void DrawLine(ref GraphEntry value, int prevIndex, int nextIndex, float rectHeight, float xIncrement, Color color)
        {
            var prevY = GetAdjustedY(value.entries[prevIndex], value.max);
            var nextY = GetAdjustedY(value.entries[nextIndex], value.max);
             
            if (prevIndex != nextIndex - 1) // We have aliased a point (or multiple) we need to draw two lines.
            {
                DubGUI.DrawLine(new Vector2(prevIndex * xIncrement, prevY), new Vector2((nextIndex - 1) * xIncrement, prevY), color, 1f, true);
                DubGUI.DrawLine(new Vector2((nextIndex - 1) * xIncrement, prevY), new Vector2(nextIndex * xIncrement, nextY), color, 1f, true);
            }
            else
            {
                DubGUI.DrawLine(new Vector2(prevIndex * xIncrement, prevY), new Vector2(nextIndex * xIncrement, nextY), color, 1f, true);
            }

            float GetAdjustedY(float y, float max)
            {
                return rectHeight - (rectHeight*.95f) * (y / max);
            }
        }

        internal static void AdjustDimensions(Panel_Graph instance, Rect rect, int entries)
        {
            if (Input.GetMouseButtonDown(0) && Mouse.IsOver(rect) && !instance.settings.dragging)
            {
                instance.settings.dragging = true;
                instance.settings.dragAnchor = Event.current.mousePosition;
            }

            if (instance.settings.dragging)
            {
                var mousePos = Event.current.mousePosition;

                var deltaX = mousePos.x - instance.settings.dragAnchor.x;

                if (Mathf.Abs(deltaX) > 1)
                {
                    instance.settings.offsets.x += deltaX;
                    instance.settings.offsets.x = Mathf.Clamp(instance.settings.offsets.x, 0, (Analyzer.GetCurrentLogCount - entries)-1);

                    instance.settings.dragAnchor.x = mousePos.x;
                } 
            }

            if (Input.GetMouseButtonUp(0)) instance.settings.dragging = false;
        }

        internal static void DrawMaxLine(Rect rect, float absMax, float maxValue, string suffix)
        {
            var y = rect.height - (rect.height *.95f) * (absMax / maxValue);

            var col = GUI.color;
            GUI.color = Color.red;
            Widgets.DrawLineHorizontal(0, y, rect.width);
            GUI.color = col;

            var str = $" Max: {absMax}" + suffix + "  ";
            var width = str.GetWidthCached();

            var labelRect = rect.TopPartPixels(Text.LineHeight + (rect.height * .05f));
            labelRect = labelRect.BottomPartPixels(Text.LineHeight);
            labelRect = labelRect.RightPartPixels(width);

            Widgets.Label(labelRect, str);
        }

        internal static void DrawBackground(Rect rect, bool drawGridLines, bool axis)
        {
            Widgets.DrawRectFast(rect, Modbase.Settings.GraphCol);
            if (!drawGridLines) return;

            var xIncrement = rect.width / 3.0f;
            var yIncrement = rect.height / 3.0f;

            for (int i = 1; i < 3; i++)
            {
                // Horizontal lines
                Widgets.DrawLine(new Vector2(axis ? 25 : 0, i * yIncrement), new Vector2(rect.width, i * yIncrement), gray, 1f);

                // Vertical
                Widgets.DrawLine(new Vector2(i * xIncrement, 0), new Vector2(i * xIncrement, rect.height - (axis ? Text.LineHeight : 0)), gray, 1f);
            }
        }

        internal static void DrawAxis(Rect rect, int entries, int xOffset, float yAxis, string suffix)
        {
            var xIncrement = rect.width / 3.0f;
            var yIncrement = rect.height / 3.0f;

            Text.Font = GameFont.Tiny;

            for (int i = 1; i < 3; i++)
            {
                // Horizontal lines
                Widgets.Label(new Rect(0, (i * yIncrement) - 12.5f, 25f, Text.LineHeight * 2), Mathf.Round((3 - i) * (yAxis / 3.0f) * 100)/100 + "\n" + suffix);

                var xAxisValue = ((3 - i) * (entries / 3.0f)) + xOffset;
                // Vertical
                Widgets.Label(new Rect((i * xIncrement) - 12.5f, rect.height - Text.LineHeight, 40f, 999f), Mathf.CeilToInt(xAxisValue).ToString());
            }
        }
    }
}