using Analyzer.Performance;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Analyzer.Fixes
{
    internal class H_DrawNamesFix : PerfPatch
    {
        public static bool Enabled = false;
        public override PerformanceCategory Category => PerformanceCategory.Overrides;
        private static readonly Color hwite = new Color(1f, 1f, 1f, 1f);
        public override string Name => "performance.drawlabelsfix";

        public override void OnEnabled(Harmony harmony)
        {
            var skiff = AccessTools.Method(typeof(PawnUIOverlay), nameof(PawnUIOverlay.DrawPawnGUIOverlay));
            harmony.Patch(skiff, new HarmonyMethod(typeof(H_DrawNamesFix), nameof(Prefix)));
        }

        public static bool Prefix(PawnUIOverlay __instance)
        {
            if (!Enabled) return true;

            if (!__instance.pawn.Spawned)
            {
                return false;
            }

            if (!__instance.pawn.RaceProps.Humanlike)
            {
                switch (Prefs.AnimalNameMode)
                {
                    case AnimalNameDisplayMode.None:
                        return false;
                    case AnimalNameDisplayMode.TameNamed:
                        if (__instance.pawn.Name == null || __instance.pawn.Name.Numerical)
                        {
                            return false;
                        }

                        break;
                    case AnimalNameDisplayMode.TameAll:
                        if (__instance.pawn.Name == null)
                        {
                            return false;
                        }

                        break;
                }
            }

            var pawn = __instance.pawn;

            var pos = GenMapUI.LabelDrawPosFor(pawn, -0.6f);


            Text.Font = GameFont.Tiny;
            var pawnLabel =  pawn.Name.ToStringShort;
            var pawnLabelNameWidth = pawnLabel.GetWidthCached();

            var bgRect = new Rect(pos.x - pawnLabelNameWidth / 2f - 4f, pos.y, pawnLabelNameWidth + 8f, 12f);
            if (!pawn.RaceProps.Humanlike)
            {
                bgRect.y -= 4f;
            }

            GUI.color = hwite;

            var summaryHealthPercent = pawn.health.summaryHealth.SummaryHealthPercent;

            GUI.DrawTexture(bgRect, TexUI.GrayTextBG);

            if (summaryHealthPercent < 0.999f)
            {
                Widgets.FillableBar(bgRect.ContractedBy(1f), summaryHealthPercent, GenMapUI.OverlayHealthTex, BaseContent.ClearTex, false);
            }

            GUI.color = PawnNameColorUtility.PawnNameColorOf(pawn);

            Text.Anchor = TextAnchor.UpperCenter;
            var rect = new Rect(bgRect.center.x - pawnLabelNameWidth / 2f, bgRect.y - 2f, pawnLabelNameWidth, 100f);

            Widgets.Label(rect, pawnLabel);
            if (pawn.Drafted)
            {
                Widgets.DrawLineHorizontal(bgRect.center.x - pawnLabelNameWidth / 2f, bgRect.y + 11f, pawnLabelNameWidth);
            }

            GUI.color = hwite;
            Text.Anchor = TextAnchor.UpperLeft;

            if (pawn.CanTradeNow)
            {
                __instance.pawn.Map.overlayDrawer.DrawOverlay(pawn, OverlayTypes.QuestionMark);
            }

            var lord = pawn.GetLord();
            lord?.CurLordToil?.DrawPawnGUIOverlay(pawn);
            return false;
        }
    }
}