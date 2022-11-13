using System;
using System.Linq;
using System.Runtime.InteropServices;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;
using static RocketMan.Listing_Collapsible;

namespace Soyuz.Tabs
{
    public class TabContent_Grapher : ITabContent
    {
        public override Texture2D Icon => TexTab.Graphing;

        private Pawn currentPawn;

        private Listing_Collapsible.Group_Collapsible group = new Listing_Collapsible.Group_Collapsible();

        private Listing_Collapsible standard_Content = new Listing_Collapsible();

        public override string Label => "Grapher";

        public override bool ShouldShow => RocketPrefs.Enabled;

        public override void DoContent(Rect rect)
        {
            GUI.color = Color.red;
            standard_Content.Begin(rect.TopPartPixels(350), "Information and controls");
            GUI.color = Color.white;
            standard_Content.CheckboxLabeled("Enable time dilation", ref RocketPrefs.TimeDilation, "Experimental.");
            standard_Content.CheckboxLabeled("Flash dilated pawns", ref RocketDebugPrefs.FlashDilatedPawns, "Experimental.");
            standard_Content.End(ref rect);
            DoExtras(rect.ExpandedBy(1));
        }

        private void DoExtras(Rect rect)
        {
            var anchor = GUIFont.Anchor;
            var font = GUIFont.Font;
            var style = GUIFont.CurFontStyle.fontStyle;
            Widgets.DrawMenuSection(rect.ContractedBy(1));
            if (Find.Selector.selected.Count == 0 || !(Find.Selector.selected.First() is Pawn pawn))
            {
                GUIFont.Font = GUIFontSize.Medium;
                GUIFont.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "Please select a pawn");
            }
            else DoExtras_Internal(rect.ContractedBy(3));
            GUIFont.CurFontStyle.fontStyle = style;
            GUIFont.Font = font;
            GUIFont.Anchor = anchor;
        }

        private void DoExtras_Internal(Rect rect)
        {
            var pawn = Find.Selector.selected.First() as Pawn;
            if (currentPawn != pawn)
            {
                Context.ProfiledPawn = pawn;
                currentPawn = pawn;
                group = new Group_Collapsible();
            }
            var needs = pawn.needs.needs;
            var hediffs = pawn.health.hediffSet.hediffs;
            var needsModel = pawn.GetNeedModels();
            var performanceModel = pawn.GetPerformanceModel();
            if (performanceModel.grapher.Group != group)
                performanceModel.grapher.Group = group;
            performanceModel.DrawGraph(ref rect);
            var patherModel = pawn.GetPatherModel();
            if (patherModel.grapher.Group != group)
                patherModel.grapher.Group = group;
            patherModel.DrawGraph(ref rect);
            foreach (var need in needs)
            {
                if (needsModel.TryGetValue(need.GetType(), out var model))
                {
                    if (model.grapher.Group != group)
                        model.grapher.Group = group;
                    model.DrawGraph(ref rect);
                }
            }
            var hediffsModel = pawn.GetHediffModels();
            foreach (var hediff in hediffs)
            {
                if (hediffsModel.TryGetValue(hediff, out var model))
                {
                    if (model.grapher.Group != group)
                        model.grapher.Group = group;
                    model.DrawGraph(ref rect);
                }
            }
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }

        public override void OnSelect()
        {
            base.OnSelect();
            RocketDebugPrefs.LogData = true;
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Grapher();
    }
}
