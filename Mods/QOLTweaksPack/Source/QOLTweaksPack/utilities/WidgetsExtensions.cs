using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace QOLTweaksPack.utilities
{
    class WidgetsExtensions
    {
        private static Color WindowBGFillColor = new ColorInt(21, 25, 29).ToColor;
        private static Color WindowBGBorderColor = new ColorInt(97, 108, 122).ToColor;

        public static void DrawWindowBackgroundTransparent(Rect rect, float alpha)
        {
            GUI.color = WindowBGFillColor;
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
            GUI.DrawTexture(rect, BaseContent.WhiteTex);
            GUI.color = WindowBGBorderColor;
            Widgets.DrawBox(rect, 1);
            GUI.color = Color.white;
        }

        public static void DrawHorizontalDivider(Rect rect, float yPos)
        {
            GUI.color = WindowBGBorderColor;
            GUI.DrawTexture(new Rect(rect.x, rect.y + yPos, rect.width, 1), BaseContent.WhiteTex);
            GUI.color = Color.white;
        }

        public static void DrawVerticalDivider(Rect rect, float xPos)
        {
            GUI.color = WindowBGBorderColor;
            GUI.DrawTexture(new Rect(rect.x + xPos, rect.y, 1, rect.height), BaseContent.WhiteTex);
            GUI.color = Color.white;
        }

        public static void DrawGizmoLabel(string labelText, Rect gizmoRect)
        {
            var labelHeight = Text.CalcHeight(labelText, gizmoRect.width);
            labelHeight -= 2f;
            var labelRect = new Rect(gizmoRect.x, gizmoRect.yMax - labelHeight + 12f, gizmoRect.width, labelHeight);
            GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        public static void DrawGadgetWindowLabel(string labelText, Rect windowRect, Color color, out float bottom)
        {
            var labelHeight = Text.CalcHeight(labelText, windowRect.width);
            labelHeight -= 2f;
            var labelRect = new Rect(windowRect.x, windowRect.y, windowRect.width, labelHeight);
            //GUI.DrawTexture(labelRect, TexUI.GrayTextBG);
            GUI.color = color;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(labelRect, labelText);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            bottom = labelHeight;
        }
    }
}
