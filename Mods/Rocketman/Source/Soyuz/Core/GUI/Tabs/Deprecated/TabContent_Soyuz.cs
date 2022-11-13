using System;
using System.Linq;
using System.Runtime.InteropServices;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_Soyuz : ITabContent
    {
        private Listing_Standard standard = new Listing_Standard();
        private Listing_Standard standard_extras = new Listing_Standard();

        private static Vector2 scrollPosition = Vector2.zero;
        private static Rect viewRect = Rect.zero;
        private static string searchString;
        private static RaceSettings curSelection;

        public override string Label => "Time dilation controls";
        public override bool ShouldShow => RocketPrefs.Enabled;

        public override void DoContent(Rect rect)
        {
            standard.Begin(rect.TopPartPixels(95 + (RocketDebugPrefs.Debug ? 120 : 0)));
            var font = GUIFont.Font;
            GUIFont.Font = GameFont.Tiny;
            standard.CheckboxLabeled("Enable time dilation", ref RocketPrefs.TimeDilation, "Experimental.");
            standard.GapLine();
            // TODO: remake this
            //standard.CheckboxLabeled("[<color=red>EXPERIMENTAL</color>] Enable time dilation for fire (Can cause issues)", ref RocketPrefs.TimeDilationFire, "Disable this in case your caravans are consuming food too quickly.");
            RocketPrefs.TimeDilationCaravans = false;
            standard.CheckboxLabeled("Enable time dilation for visitor pawns.", ref RocketPrefs.TimeDilationVisitors, "Experimental: Can cause a lot of bugs.");
            standard.CheckboxLabeled("Enable time dilation for world pawns", ref RocketPrefs.TimeDilationWorldPawns, "Throttle ticking for world pawns.");
            if (RocketDebugPrefs.Debug)
            {
                standard.CheckboxLabeled("Enable data logging", ref RocketDebugPrefs.DogData, "For debugging only.");
                standard.CheckboxLabeled("Set tick multiplier to 150", ref RocketDebugPrefs.Debug150MTPS, "Dangerous!");
                standard.GapLine();
                standard.CheckboxLabeled("Enable flashing dilated pawns",
                    ref RocketDebugPrefs.FlashDilatedPawns);
                standard.CheckboxLabeled("Simulate offscreen behavior", ref RocketDebugPrefs.AlwaysDilating);
            }
            GUIFont.Font = font;
            standard.End();
            rect.yMin += 85 + (RocketDebugPrefs.Debug ? 120 : 0);
            DoExtras(rect);
        }

        public void DoExtras(Rect rect)
        {
            var stage = 0;
            rect.yMin += 5;
            GUIFont.CurFontStyle.fontStyle = FontStyle.Bold;
            Widgets.Label(rect.TopPartPixels(25), "Dilated races");
            GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
            if (Context.Settings == null || Find.Selector == null)
            {
                return;
            }
            var font = GUIFont.Font;
            var anchor = Text.Anchor;
            GUIFont.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;
            rect.yMin += 25;
            var searchRect = rect.TopPartPixels(25);
            string oldSearchString = searchString;
            searchString = Widgets.TextField(searchRect, searchString)?.ToLower()?.Trim() ?? string.Empty;
            if (oldSearchString != searchString)
                scrollPosition = Vector2.zero;
            rect.yMin += 30;
            if (curSelection != null)
            {
                var height = 128 + (RocketDebugPrefs.Debug ? 75 : 0);
                var selectionRect = rect.TopPartPixels(height);
                Widgets.DrawMenuSection(selectionRect);
                GUIFont.Font = GameFont.Tiny;
                Widgets.DefLabelWithIcon(selectionRect.TopPartPixels(54), curSelection.pawnDef);
                if (Widgets.ButtonImage(selectionRect.RightPartPixels(30).TopPartPixels(30).ContractedBy(5),
                    TexButton.CloseXSmall))
                {
                    curSelection = null;
                    return;
                }
                selectionRect.yMin += 54;
                standard_extras.Begin(selectionRect.ContractedBy(3));
                GUIFont.Font = GameFont.Tiny;
                if (!IgnoreMeDatabase.ShouldIgnore(curSelection.pawnDef))
                {
                    standard_extras.CheckboxLabeled($"Enable dilation for {curSelection.pawnDef?.label ?? "_"}", ref curSelection.enabled, tooltip: "Used to control which races are dilated/throttled in case of a problem.");
                    standard_extras.CheckboxLabeled($"Disable dilation for all factions except the player faction", ref curSelection.ignoreFactions);
                    standard_extras.CheckboxLabeled($"Disable dilation for the player faction", ref curSelection.ignorePlayerFaction);
                }
                else
                {
                    standard_extras.Label($"This race will be ignored because: <color=red>{ IgnoreMeDatabase.Report(curSelection.pawnDef) }</color>");
                }
                if (RocketDebugPrefs.Debug)
                {
                    standard_extras.GapLine();
                    standard_extras.Label($"This race modContentPack is { curSelection.pawnDef.modContentPack?.PackageId ?? "UNKNOWN" }");
                    standard_extras.Label($"This race defName is { curSelection.pawnDef.defName ?? "UNKNOWN" }");
                    if (curSelection.pawnDef.StatBaseDefined(StatDefOf.MoveSpeed))
                        standard_extras.Label($"Base race move speed is {curSelection.pawnDef.GetStatValueAbstract(StatDefOf.MoveSpeed)}:{Context.DilationFastMovingRace[curSelection.pawnDef.index]}");
                    else standard_extras.Label("Base race move speed is not defined");
                }
                standard_extras.End();
                rect.yMin += height + 8;
            }
            else if (Find.Selector.selected.Count == 1 && Find.Selector.selected.First() is Pawn pawn && pawn != null && searchString.NullOrEmpty())
            {
                var height = 128;
                var selectionRect = rect.TopPartPixels(height);
                var model = pawn.GetPerformanceModel();
                if (RocketDebugPrefs.Debug) Log.Message($"SOYUZ: UI stage is {stage}:{1}");
                if (model != null)
                {
                    model.DrawGraph(selectionRect, 2000);
                    rect.yMin += height + 8;
                }
            }
            Widgets.DrawMenuSection(rect);
            rect = rect.ContractedBy(2);
            viewRect.size = rect.size;
            viewRect.height = 60 * Context.Settings.raceSettings.Count;
            viewRect.width -= 15;
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect.AtZero());
            Rect curRect = viewRect.TopPartPixels(58);
            curRect.width -= 15;
            var counter = 0;
            foreach (var element in Context.Settings.raceSettings)
            {
                string defLabel = element?.pawnDef?.label?.ToLower();
                if (defLabel == null)
                    continue;
                if (!searchString.NullOrEmpty() && !(defLabel?.Contains(searchString) ?? false))
                    continue;
                counter++;
                if (counter % 2 == 0)
                    Widgets.DrawBoxSolid(curRect, new Color(0.2f, 0.2f, 0.2f));
                Widgets.DefLabelWithIcon(curRect, element.pawnDef);
                if (Widgets.ButtonInvisible(curRect))
                {
                    curSelection = element;
                    break;
                }
                curRect.y += 58;
            }
            Widgets.EndScrollView();
            GUIFont.Font = font;
            Text.Anchor = anchor;
            Finder.Mod.WriteSettings();
            SoyuzSettingsUtility.CacheSettings();
        }

        public override void OnSelect()
        {
        }

        public override void OnDeselect()
        {
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Soyuz();
    }
}
