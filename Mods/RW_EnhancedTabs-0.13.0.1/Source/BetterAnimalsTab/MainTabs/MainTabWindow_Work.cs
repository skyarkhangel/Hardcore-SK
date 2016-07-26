using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Fluffy
{
    public class MainTabWindow_Work : MainTabWindow_PawnList
    {
        //private const float TopAreaHeight = 40f;
        protected const float LabelRowHeight = 50f;
        private float _workColumnSpacing = -1f;

        private static List<WorkTypeDef> _visibleWorkTypeDefsInPriorityOrder;

        private static readonly Texture2D CopyTex = ContentFinder<Texture2D>.Get("UI/Buttons/Copy");
        private static readonly Texture2D PasteTex = ContentFinder<Texture2D>.Get("UI/Buttons/Paste");
        private static readonly Texture2D CancelTex = ContentFinder<Texture2D>.Get("UI/Buttons/cancel");

        private WorkTypeDef _workOrder;

        public enum Order
        {
            Work,
            Name,
            Default
        }

        public Order OrderBy = Order.Default;
        private bool _asc;
        private List<int> _copy;
        public static Pawn Copied;

        private int _UItick;

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(1050f, 90f + PawnsCount * 30f + 65f);
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            Reinit();
        }

        public static void Reinit()
        {
            _visibleWorkTypeDefsInPriorityOrder = (from def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder
                                                                     where def.visible
                                                                     select def).ToList();
        }

        protected void Copy(Pawn p)
        {
            _copy = (from def in _visibleWorkTypeDefsInPriorityOrder
                    select p.story.WorkTypeIsDisabled(def) ? -1 : p.workSettings.GetPriority(def)).ToList();
            Copied = p;

#if DEBUG
            var temp = "Priorities: ";
            for (int i = 0; i < _copy.Count; i++)
            {
                temp += ", " + _copy[i].ToString();
            }
            Log.Message(temp);
#endif
        }

        protected void Paste(Pawn p)
        {
            for (int i = 0; i < _visibleWorkTypeDefsInPriorityOrder.Count; i++)
            {
#if DEBUG
                Log.Message("Attempting paste...");
#endif
                if (p.story != null && !p.story.WorkTypeIsDisabled(_visibleWorkTypeDefsInPriorityOrder[i]) && _copy[i] >= 0)
                {
#if DEBUG
                    Log.Message( _visibleWorkTypeDefsInPriorityOrder[i].LabelCap);
#endif
                    p.workSettings.SetPriority(_visibleWorkTypeDefsInPriorityOrder[i], _copy[i]);
                }
            }
        }

        protected void ClearCopied()
        {
            _copy = null;
            Copied = null;
        }

        protected override void BuildPawnList()
        {
            Pawns.Clear();
            IEnumerable<Pawn> sorted;

            switch (OrderBy)
            {
                case Order.Work:
                    sorted = from p in Find.MapPawns.FreeColonists
                             orderby (p.story == null || p.story.WorkTypeIsDisabled(_workOrder)),
                                      p.skills.AverageOfRelevantSkillsFor(_workOrder) descending
                             select p;
                    break;
                case Order.Name:
                    sorted = from p in Find.MapPawns.FreeColonists
                             orderby p.LabelCap ascending
                             select p;
                    break;
                default:
                    sorted = Find.MapPawns.FreeColonists;
                    break;
            }

            Pawns = sorted.ToList();
            if (_asc && Pawns.Count > 1)
            {
                Pawns.Reverse();
            }
        }

        protected void IncrementJobPriority(WorkTypeDef work, bool toggle)
        {
            int start = toggle ? 3 : 4;
            bool max = Pawns.All(p => (p.workSettings.GetPriority(work) == 1 || (p.story == null || p.story.WorkTypeIsDisabled(work))));

            foreach (Pawn t in Pawns)
            {
                if (!(t.story == null || t.story.WorkTypeIsDisabled(work)))
                {
                    int cur = t.workSettings.GetPriority(work);
                    if (cur > 1)
                    {
                        t.workSettings.SetPriority(work, cur - 1);
                    }
                    if (cur == 0)
                    {
                        if (toggle) SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                        t.workSettings.SetPriority(work, start);
                    }
                    if (toggle && max)
                    {
                        SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                        t.workSettings.SetPriority(work, 0);
                    }
                }
            }
        }

        protected void DecrementJobPriority(WorkTypeDef work, bool toggle)
        {
            bool min = Pawns.All(p => (p.workSettings.GetPriority(work) == 0 || (p.story == null || p.story.WorkTypeIsDisabled(work))));

            foreach (Pawn p in Pawns)
            {
                if (!(p.story == null || p.story.WorkTypeIsDisabled(work)))
                {
                    int cur = p.workSettings.GetPriority(work);
                    if (!toggle && cur > 0 && cur < 4)
                    {
                        p.workSettings.SetPriority(work, cur + 1);
                    }
                    if (cur == 4 || (toggle && cur == 1))
                    {
                        p.workSettings.SetPriority(work, 0);
                        if (toggle) SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                    }
                    if (min && toggle)
                    {
                        p.workSettings.SetPriority(work, 3);
                        SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                    }
                }
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            // catch priority changes
            if (Widgets_Work.prioritiesDirty)
            {
                Reinit();
                Widgets_Work.prioritiesDirty = false;
            }

            // update the list every 5 seconds (workaround to catch external pawn list changes)
            _UItick++;
            if (_UItick%300 == 0)
            {
                BuildPawnList();
            }
            
            base.DoWindowContents(rect);
            Rect position = new Rect(0f, 0f, rect.width, 40f);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            float num3 = 175f;

            Text.Anchor = TextAnchor.UpperLeft;

            float headerWidth = (position.width - 25f) / 4;

            // use manual toggle
            Rect rect2 = new Rect(5f, 5f, headerWidth, 30f);
            bool useWorkPriorities = Find.Map.playSettings.useWorkPriorities;
            Widgets.LabelCheckbox(rect2, "ManualPriorities".Translate(), ref Find.Map.playSettings.useWorkPriorities);
            if (useWorkPriorities != Find.Map.playSettings.useWorkPriorities)
            {
                foreach (Pawn current in Find.MapPawns.FreeColonists)
                {
                    current.workSettings.Notify_UseWorkPrioritiesChanged();
                }
            }

            // priorities detail button
            Rect detailRect = new Rect(3 * (headerWidth + 5f) + 5f, 5f, headerWidth, 30f);
            if (Widgets.TextButton(detailRect, "Fluffy.WorkPrioritiesDetails".Translate()))
            {
                Find.WindowStack.Add( new Dialog_Priority());
            }

            // high/low priority labels
            Rect rect3 = new Rect(headerWidth + 10f, 5f, headerWidth, 30f);
            Rect rect4 = new Rect(2 * (headerWidth + 5f) + 5f, 5f, headerWidth, 30f);
            GUI.color = new Color( 1f, 1f, 1f, 0.5f );
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            Widgets.Label( rect3, "<= " + "HigherPriority".Translate() );
            Widgets.Label( rect4, "LowerPriority".Translate() + " =>" );
            Text.Font = GameFont.Small;

            GUI.EndGroup();
            Rect position2 = new Rect(0f, 40f, rect.width, rect.height - 40f);
            GUI.BeginGroup(position2);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.LowerCenter;
            GUI.color = Color.white;
            Rect rectname = new Rect(0f, 20f, num3, 33f);
            Widgets.Label(rectname, "Fluffy.Name".Translate());
            Widgets.DrawHighlightIfMouseover(rectname);
            if (Widgets.InvisibleButton(rectname))
            {
                if (Event.current.button == 0)
                {
                    if (OrderBy == Order.Name)
                    {
                        _asc = !_asc;
                    }
                    else
                    {
                        OrderBy = Order.Name;
                        _asc = false;
                    }
                }
                else if (Event.current.button == 1)
                {
                    if (OrderBy != Order.Default)
                    {
                        OrderBy = Order.Default;
                        _asc = false;
                    }
                }
                if (_asc)
                {
                    SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                }
                else
                {
                    SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                }
                BuildPawnList();
            }
            TooltipHandler.TipRegion(rectname, "Fluffy.SortByNameWork".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Rect outRect = new Rect(0f, 50f, position2.width, position2.height - 50f);
            _workColumnSpacing = (position2.width - 16f - 225f) / _visibleWorkTypeDefsInPriorityOrder.Count;
            int num4 = 0;
            foreach (WorkTypeDef current2 in _visibleWorkTypeDefsInPriorityOrder)
            {
                Vector2 vector = Text.CalcSize(current2.labelShort);
                float num5 = num3 + 15f;
                Rect rect5 = new Rect(num5 - vector.x / 2f, 0f, vector.x, vector.y);
                if (num4 % 2 == 1)
                {
                    rect5.y += 20f;
                }
                if (Mouse.IsOver(rect5))
                {
                    Widgets.DrawHighlight(rect5);
                }
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect5, current2.labelShort);
                WorkTypeDef localDef = current2;
                var tiptext = new StringBuilder();
                if (useWorkPriorities)
                {
                    tiptext.AppendLine("Fluffy.LeftSortBySkill".Translate());
                    tiptext.AppendLine("Fluffy.ShiftToChangePriority".Translate());
                }
                else
                {
                    tiptext.AppendLine("Fluffy.SortBySkill".Translate());
                    tiptext.AppendLine("Fluffy.ShiftToToggle".Translate());
                }
                tiptext.AppendLine().AppendLine(localDef.gerundLabel).
                        AppendLine().Append(localDef.description);
                TooltipHandler.TipRegion(rect5, new TipSignal(() => tiptext.ToString(), localDef.GetHashCode()));
                if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect5))
                {
                    if (Event.current.shift)
                    {
                        if (Event.current.button == 0)
                        {
                            if (useWorkPriorities)
                            {
                                SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
                                IncrementJobPriority(current2, false);
                            }
                            else
                            {
                                IncrementJobPriority(current2, true);
                            }
                        }
                        else if (Event.current.button == 1)
                        {
                            if (useWorkPriorities)
                            {
                                SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
                                DecrementJobPriority(current2, false);
                            }
                            else
                            {
                                DecrementJobPriority(current2, true);
                            }
                        }
                    }
                    else
                    {
                        //Log.Message("Clicked on " + current2.LabelCap);
                        if (OrderBy == Order.Work && _workOrder == current2)
                        {
                            _asc = !_asc;
                        }
                        else
                        {
                            OrderBy = Order.Work;
                            _workOrder = current2;
                            _asc = false;
                        }
                        if( _asc )
                        {
                            SoundDefOf.CheckboxTurnedOff.PlayOneShotOnCamera();
                        }
                        else
                        {
                            SoundDefOf.CheckboxTurnedOn.PlayOneShotOnCamera();
                        }
                        BuildPawnList();
                        //Log.Message("Sort order is now " + order.LabelCap + ", " + (asc ? "asc" : "desc"));
                    }
                    Event.current.Use();
                }
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
                Widgets.DrawLineVertical(num5, rect5.yMax - 3f, 50f - rect5.yMax + 3f);
                Widgets.DrawLineVertical(num5 + 1f, rect5.yMax - 3f, 50f - rect5.yMax + 3f);
                GUI.color = Color.white;
                num3 += _workColumnSpacing;
                num4++;
            }
            DrawRows(outRect);
            GUI.EndGroup();
        }

        protected override void DrawPawnRow(Rect rect, Pawn p)
        {
            float num = 175f;
            Text.Font = GameFont.Medium;
            foreach (WorkTypeDef workTypeDef in _visibleWorkTypeDefsInPriorityOrder)
            {
                Vector2 topLeft = new Vector2(num, rect.y + 2.5f);
                Widgets_Work.DrawWorkBoxFor(topLeft, p, workTypeDef);
                Rect rect2 = new Rect(topLeft.x, topLeft.y, 25f, 25f);
                TooltipHandler.TipRegion(rect2, Widgets_Work.TipForPawnWorker(p, workTypeDef));
                num += _workColumnSpacing;
            }

            if (Copied == p)
            {
                Rect rectCancel = new Rect(num + 17f, rect.y + 6f, 16f, 16f);
                if (Widgets.ImageButton(rectCancel, CancelTex))
                {
                    SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    ClearCopied();
                }
                TooltipHandler.TipRegion(rectCancel, "Fluffy.ClearCopy".Translate());
            }
            else
            {
                Rect rectCopy = new Rect(num + 6f, rect.y + 6f, 16f, 16f);
                if (Widgets.ImageButton(rectCopy, CopyTex))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    Copy(p);
                }
                TooltipHandler.TipRegion(rectCopy, "Fluffy.Copy".Translate());
            }
            if (_copy != null && Copied != p)
            {
                Rect rectPaste = new Rect(num + 28f, rect.y + 6f, 16f, 16f);
                if (Widgets.ImageButton(rectPaste, PasteTex))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    Paste(p);
                }
                TooltipHandler.TipRegion(rectPaste, "Fluffy.Paste".Translate());
            }
        }
    }
}

