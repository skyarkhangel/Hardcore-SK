using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality
{
    public static class BedStatsDrawer
    {
        private static readonly Color StackElementBackgroundDisabled = new Color(1f, 1f, 1f, 0.03f);

        private const float DistFromMouse = 26f;

        public const float WindowPadding = 18f;

        private const float LineHeight = 23f;

        private const float SpaceBetweenLines = 2f;

        private const float IconSize = 20f;

        public static Rect GetWindowRect()
        {
            Rect result = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 416f, 200);

            result.x += DistFromMouse;
            result.y += DistFromMouse;
            if (result.xMax > UI.screenWidth)
            {
                result.x -= result.width + 2*DistFromMouse;
            }

            if (result.yMax > UI.screenHeight)
            {
                result.y -= result.height + 2*DistFromMouse;
            }

            return result;
        }

        public static void DoBedInfos(Rect windowRect, Building_GuestBed bed)
        {
            Rect rect = new Rect(WindowPadding, 18, windowRect.width - 2*WindowPadding, 100f);
            GUI.color = Color.white;
            Widgets.Label(rect, "BedSatisfiedTitles".Translate());
            rect.y += LineHeight+SpaceBetweenLines;
            var curY = rect.y;
            DrawTitles(rect, ref curY, bed);
            rect.y = curY;
            Widgets.Label(rect, bed.Stats.textNextTitleReq);
        }

        private static void DrawTitles(Rect rectTotal, ref float curY, Building_GuestBed bed)
        {
            var stackElements = from titleDef in DefDatabase<RoyalTitleDef>.AllDefsListForReading
                orderby titleDef.seniority
                let titleLabel = titleDef.GetLabelCapForBothGenders()
                let possibleFactions = GuestUtility.DistinctFactions.Where(f => f.def.RoyalTitlesAllInSeniorityOrderForReading.Contains(titleDef)).ToArray()
                let accepted = bed.Stats.metRoyalTitles.Contains(titleDef)
                where possibleFactions.Length > 0
                select new GenUI.AnonymousStackElement
                {
                    drawer = delegate(Rect r) {
                        Color guiColor = GUI.color;
                        Rect rect = new Rect(r.x, r.y, r.width, r.height);
                        GUI.color = accepted ? CharacterCardUtility.StackElementBackground : StackElementBackgroundDisabled;
                        GUI.DrawTexture(rect, BaseContent.WhiteTex);
                        GUI.color = guiColor;
                        if (Mouse.IsOver(rect))
                        {
                            Widgets.DrawHighlight(rect);
                        }

                        Rect labelRect = new Rect(r.x, r.y, r.width + 2f + IconSize * possibleFactions.Length, r.height);
                        Rect positionIcon = new Rect(r.x + 1f, r.y + 1f, IconSize, IconSize);
                        foreach (var faction in possibleFactions)
                        {
                            GUI.color = accepted ? faction.Color : CharacterCardUtility.StackElementBackground;
                            GUI.DrawTexture(positionIcon, faction.def.FactionIcon);
                            positionIcon.x += IconSize;
                        }

                        GUI.color = accepted ? guiColor : Color.gray;
                        Widgets.Label(new Rect(labelRect.x + labelRect.height + 5f, labelRect.y, labelRect.width - 10f, labelRect.height), titleLabel);
                        GUI.color = guiColor;

                    },
                    width = Text.CalcSize(titleLabel).x + 15f + IconSize * possibleFactions.Length
                };
            var stackList = stackElements.ToList();

            curY += GenUI.DrawElementStack(
                new Rect(15f, curY, rectTotal.width - 5f, 50f), 
                22f, 
                stackList, 
                delegate(Rect r, GenUI.AnonymousStackElement obj) { obj.drawer(r); }, 
                obj => obj.width, 4, 5, false).height;

            if (stackList.Any())
            {
                curY += 10f;
            }
        }
    }
}
