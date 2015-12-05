using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fluffy
{
    public abstract class MainTabWindow_PawnList : MainTabWindow
    {
        public const float PawnRowHeight = 30f;

        protected const float NameColumnWidth = 175f;

        protected const float NameLeftMargin = 15f;

        protected Vector2 ScrollPosition = Vector2.zero;

        protected List<Pawn> Pawns = new List<Pawn>();

        protected int PawnsCount
        {
            get
            {
                return Pawns.Count;
            }
        }

        protected abstract void DrawPawnRow(Rect r, Pawn p);

        public override void PreOpen()
        {
            base.PreOpen();
            BuildPawnList();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            currentWindowRect.size = InitialWindowSize;
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            currentWindowRect.size = InitialWindowSize;
        }

        protected virtual void BuildPawnList()
        {
            Pawns.Clear();
            Pawns.AddRange(Find.ListerPawns.FreeColonists);
        }

        public void Notify_PawnsChanged()
        {
            BuildPawnList();
        }

        protected void DrawRows(Rect outRect)
        {
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, Pawns.Count * 30f);
            Widgets.BeginScrollView(outRect, ref ScrollPosition, viewRect);
            float num = 0f;
            foreach (Pawn p in Pawns)
            {
                Rect rect = new Rect(0f, num, viewRect.width, 30f);
                if (num - ScrollPosition.y + 30f >= 0f && num - ScrollPosition.y <= outRect.height)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    Widgets.DrawLineHorizontal(0f, num, viewRect.width);
                    GUI.color = Color.white;
                    PreDrawPawnRow(rect, p);
                    DrawPawnRow(rect, p);
                    PostDrawPawnRow(rect, p);
                }
                num += 30f;
            }
            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void PreDrawPawnRow(Rect rect, Pawn p)
        {
            Rect rect2 = new Rect(0f, rect.y, rect.width, 30f);
            if (Mouse.IsOver(rect2) || MainTabWindow_Work.Copied == p)
            {
                GUI.DrawTexture(rect2, TexUI.HighlightTex);
            }
            Rect rect3 = new Rect(0f, rect.y, 175f, 30f);
            Rect position = rect3.ContractedBy(3f);
            if (p.health.summaryHealth.SummaryHealthPercent < 0.99f)
            {
                Rect rect4 = new Rect(rect3);
                rect4.xMin -= 4f;
                rect4.yMin += 4f;
                rect4.yMax -= 6f;
                Widgets.FillableBar(rect4, p.health.summaryHealth.SummaryHealthPercent, PawnUIOverlay.HealthTex, BaseContent.ClearTex, false);
            }
            if (Mouse.IsOver(rect3))
            {
                GUI.DrawTexture(position, TexUI.HighlightTex);
            }
            string label;
            if (!p.RaceProps.Humanlike && p.Name != null && !p.Name.Numerical)
            {
                label = p.Name.ToStringShort.CapitalizeFirst() + ", " + p.KindLabel;
            }
            else
            {
                label = p.LabelCap;
            }
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Rect rect5 = new Rect(rect3);
            rect5.xMin += 15f;
            Widgets.Label(rect5, label);
            Text.WordWrap = true;
            if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect3))
            {
                if (Event.current.button == 0)
                {
                    Find.MainTabsRoot.EscapeCurrentTab();
                    Find.CameraMap.JumpTo(p.PositionHeld);
                    Find.Selector.ClearSelection();
                    if (p.SpawnedInWorld)
                    {
                        Find.Selector.Select(p);
                    }
                }
                if (Event.current.button == 1 && !p.RaceProps.Humanlike && p.Name != null && !p.Name.Numerical)
                {
                    Find.WindowStack.Add(new Dialog_RenamePet(p));
                }
                Event.current.Use();
            }

            TipSignal tooltip = p.GetTooltip();
            string temp = tooltip.text;
            tooltip.text = "Fluffy.ClickToJump".Translate();
            if (!p.RaceProps.Humanlike && p.Name != null && !p.Name.Numerical)
            {
                tooltip.text += "\n" + "Fluffy.RightClickToRename".Translate();
            }
            tooltip.text += "\n\n" + temp;
            TooltipHandler.TipRegion(rect3, tooltip);
        }

        private void PostDrawPawnRow(Rect rect, Pawn p)
        {
            if (p.Downed)
            {
                GUI.color = new Color(1f, 0f, 0f, 0.5f);
                Widgets.DrawLineHorizontal(rect.x, rect.center.y, rect.width);
                GUI.color = Color.white;
            }
        }
    }
}
