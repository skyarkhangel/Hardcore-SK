using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Hospitality.MainTab
{
    // Copied from AreaAllowedGUI - wish RimWorld had already made this more generic
    public static class AreaGUI
    {
        private static bool dragging;

        // Added passing of Get/Set methods
        public static void DoAllowedAreaSelectors(Rect rect, Pawn p, Func<Pawn, Area> getArea, Action<Pawn, Area> setArea)
        {
            if (Find.CurrentMap == null)
                return;
            List<Area> allAreas = Find.CurrentMap.areaManager.AllAreas;
            int num1 = 1;
            for (int index = 0; index < allAreas.Count; ++index)
            {
                if (allAreas[index].AssignableAsAllowed())
                    ++num1;
            }

            float width = rect.width / num1;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            DoAreaSelector(new Rect(rect.x, rect.y, width, rect.height), p, null, getArea, setArea);
            int num2 = 1;
            for (int index = 0; index < allAreas.Count; ++index)
            {
                if (allAreas[index].AssignableAsAllowed())
                {
                    float num3 = num2 * width;
                    DoAreaSelector(new Rect(rect.x + num3, rect.y, width, rect.height), p, allAreas[index], getArea, setArea);
                    ++num2;
                }
            }

            Text.WordWrap = true;
            Text.Font = GameFont.Small;
        }

        // Added use of get/set methods
        private static void DoAreaSelector(Rect rect, Pawn p, Area area, Func<Pawn, Area> getArea, Action<Pawn, Area> setArea)
        {
            rect = rect.ContractedBy(1f);
            GUI.DrawTexture(rect, area == null ? BaseContent.GreyTex : area.ColorTexture);
            Text.Anchor = TextAnchor.MiddleLeft;
            string label = AreaUtility.AreaAllowedLabel_Area(area);
            Rect rect1 = rect;
            rect1.xMin += 3f;
            rect1.yMin += 2f;
            Widgets.Label(rect1, label);
            if (getArea(p) == area)
                Widgets.DrawBox(rect, 2);
            if (Event.current.rawType == EventType.MouseUp && Event.current.button == 0)
                dragging = false;
            if (!Input.GetMouseButton(0) && Event.current.type != EventType.MouseDown)
                dragging = false;
            if (Mouse.IsOver(rect))
            {
                area?.MarkForDraw();
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    dragging = true;
                if (dragging && getArea(p) != area)
                {
                    setArea(p, area);
                    SoundDefOf.Designate_DragStandard_Changed.PlayOneShotOnCamera(null);
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(rect, (TipSignal) label);
        }
    }
}
