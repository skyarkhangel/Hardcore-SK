using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeteTimesSix.SimpleSidearms.UI
{
    using global::Verse;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    namespace Verse
    {
        // Token: 0x020003D0 RID: 976
        public static class CurveEditorPublic
        {
            public static void DoCurveEditor(Rect screenRect, SimpleCurve curve, int decimalPlaces = 0, int displayMult = 100, string valueSuffix = "%", Action onChange = null)
            {
                Widgets.DrawMenuSection(screenRect);
                SimpleCurveDrawer.DrawCurve(screenRect, curve, null, null, default(Rect));
                Vector2 mousePosition = Event.current.mousePosition - screenRect.position;
                Vector2 mouseCurveCoords = SimpleCurveDrawer.ScreenToCurveCoords(screenRect, curve.View.rect, mousePosition);
                if (Mouse.IsOver(screenRect))
                {
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        var clampedCoords = mouseCurveCoords;
                        clampedCoords.x = Mathf.Clamp(Mathf.Round(clampedCoords.x), 0, 20);
                        clampedCoords.y = Mathf.Clamp((float)Math.Round(clampedCoords.y, 2), 0, 1);
                        List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                        if (!curve.Any(point => point.x == clampedCoords.x))
                        {
                            list2.Add(new FloatMenuOption($"Add point at [{clampedCoords.x:F0} - {(clampedCoords.y * displayMult).ToString($"F{decimalPlaces}")}{valueSuffix}]", () =>
                            {
                                curve.Add(new CurvePoint(clampedCoords), true);
                                onChange?.Invoke();
                            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                        }
                        else 
                        {
                            var existingPoint = curve.First(point => point.x == clampedCoords.x);

                            list2.Add(new FloatMenuOption($"Move point at [{existingPoint.x:F0} - {(existingPoint.y * displayMult).ToString($"F{decimalPlaces}")}{valueSuffix}] to [{clampedCoords.x:F0} - {(clampedCoords.y * displayMult).ToString($"F{decimalPlaces}")}{valueSuffix}]", () =>
                            {
                                curve.RemovePointNear(existingPoint);
                                curve.Add(new CurvePoint(clampedCoords), true);
                                onChange?.Invoke();
                            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));

                            if (Mathf.RoundToInt(existingPoint.x) != 0 && Mathf.RoundToInt(existingPoint.x) != 20)
                            {
                                list2.Add(new FloatMenuOption($"Remove point at [{existingPoint.x:F0} - {(existingPoint.y * displayMult).ToString($"F{decimalPlaces}")}{valueSuffix}]", () =>
                                {
                                    curve.RemovePointNear(existingPoint);
                                    onChange?.Invoke();
                                }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                            }
                        }
                        Find.WindowStack.Add(new FloatMenu(list2));
                        Event.current.Use();
                    }
                }
            }

            // Token: 0x06001E03 RID: 7683 RVA: 0x000BCB34 File Offset: 0x000BAD34
            private static IEnumerable<int> PointsNearMouse(Rect screenRect, SimpleCurve curve, Vector2 mousePosition)
            {
                GUI.BeginGroup(screenRect);
                try
                {
                    int num;
                    for (int i = 0; i < curve.PointsCount; i = num + 1)
                    {
                        if ((SimpleCurveDrawer.CurveToScreenCoordsInsideScreenRect(screenRect, curve.View.rect, curve[i].Loc) - mousePosition).sqrMagnitude < 49f)
                        {
                            yield return i;
                        }
                        num = i;
                    }
                }
                finally
                {
                    GUI.EndGroup();
                }
                yield break;
            }
        }
    }

}
