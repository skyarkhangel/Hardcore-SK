namespace Numbers
{
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    [StaticConstructorOnStartup]
    public class PawnColumnWorker_DiseaseProgression : PawnColumnWorker
    {
        private static readonly Color SeverePainColor = new Color(0.9f, 0.5f, 0f);

        //[TweakValue("AAAADiseaseProgression")] //assumes a perfectly square icon.
        public static float MaxIconSize = 22f;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (pawn.Dead || pawn.health?.hediffSet == null || !pawn.health.hediffSet.HasImmunizableNotImmuneHediff())
                return;

            HediffWithComps mostSevere = FindMostSevereHediff(pawn);

            if (mostSevere == null)
                return;

            float deltaSeverity = GetTextFor(mostSevere);

            GUI.color = GetPrettyColorFor(deltaSeverity);

            Rect rect2 = new Rect(rect.x, rect.y, rect.width, Mathf.Min(rect.height, 30f));
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Widgets.Label(rect2, GetTextFor(mostSevere).ToStringPercent());
            Text.WordWrap = true;
            Text.Anchor = TextAnchor.UpperLeft;
            string tip = GetTip(pawn, mostSevere);
            if (!tip.NullOrEmpty())
            {
                TooltipHandler.TipRegion(rect2, tip);
            }

            float severityChangePerDayFromImmu = (float)AccessTools.Method(typeof(HediffComp_Immunizable), "SeverityChangePerDay")
                                                    .Invoke(mostSevere.TryGetComp<HediffComp_Immunizable>(), null);

            float severityChangePerDayFromTendD = 0f;

            if (mostSevere.TryGetComp<HediffComp_TendDuration>()?.IsTended ?? false)
            {
                severityChangePerDayFromTendD =
                    mostSevere.TryGetComp<HediffComp_TendDuration>().TProps.severityPerDayTended *
                    mostSevere.TryGetComp<HediffComp_TendDuration>().tendQuality;
            }

            float immunityPerDay = 0f;

            ImmunityRecord immunityRecord = pawn.health.immunity.GetImmunityRecord(mostSevere.def);
            if (immunityRecord != null)
                immunityPerDay = immunityRecord.ImmunityChangePerTick(pawn, true, mostSevere) * GenDate.TicksPerDay;

            GUI.color = Color.white;

            bool redFlag = !(severityChangePerDayFromTendD + severityChangePerDayFromImmu > immunityPerDay);

            Texture2D texture2D = redFlag ? StaticConstructorOnGameStart.SortingIcon : StaticConstructorOnGameStart.SortingDescendingIcon;
            GUI.color = redFlag ? HealthUtility.GoodConditionColor : HealthUtility.RedColor;
            Rect position2 = new Rect(rect.xMax - texture2D.width - 1f, rect.yMax - texture2D.height - 1f, texture2D.width, texture2D.height);
            GUI.DrawTexture(position2, texture2D);
            GUI.color = Color.white;
        }

        public override void DoHeader(Rect rect, PawnTable table)
        {
            Texture2D skull = StaticConstructorOnGameStart.IconDead;
            Texture2D immune = StaticConstructorOnGameStart.IconImmune;

            //[   [x]   |   []   ]
            float oneFourthLeftCenteredOnIconWidth = (rect.width / 6) - MaxIconSize / 2;
            //[   []   |   [x]   ]
            float oneFourthRightCenteredOnIconWidth = (rect.width - (rect.width / 6)) - MaxIconSize / 2; 

            //skull vs immunity icon. One left, one right.
            Rect skullPosition = new Rect(rect.x + oneFourthLeftCenteredOnIconWidth, rect.yMax - MaxIconSize, MaxIconSize, MaxIconSize);
            GUI.DrawTexture(skullPosition, skull);

            Rect immunePosition = new Rect(rect.x + oneFourthRightCenteredOnIconWidth, rect.yMax - MaxIconSize, MaxIconSize, MaxIconSize);
            GUI.DrawTexture(immunePosition.ScaledBy(0.8f), immune);

            Rect rect2 = rect;
            rect2.y += 3f;
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rect2, "vs");
            Text.Anchor = TextAnchor.UpperLeft;

            //rest is copypasta
            if (table.SortingBy == def)
            {
                Texture2D texture2D = (!table.SortingDescending) ? StaticConstructorOnGameStart.SortingIcon : StaticConstructorOnGameStart.SortingDescendingIcon;
                Rect position2 = new Rect(rect.xMax - texture2D.width - 1f, rect.yMax - texture2D.height - 1f, texture2D.width, texture2D.height);
                GUI.DrawTexture(position2, texture2D);
            }
            if (def.HeaderInteractable)
            {
                Rect interactableHeaderRect = GetInteractableHeaderRect(rect, table);
                Widgets.DrawHighlightIfMouseover(interactableHeaderRect);
                if (interactableHeaderRect.Contains(Event.current.mousePosition))
                {
                    string headerTip = GetHeaderTip(table);
                    if (!headerTip.NullOrEmpty())
                    {
                        TooltipHandler.TipRegion(interactableHeaderRect, headerTip);
                    }
                }
                if (Widgets.ButtonInvisible(interactableHeaderRect))
                {
                    HeaderClicked(rect, table);
                }
            }
        }

        public override int GetMinWidth(PawnTable table)
            => def.width;

        private string GetTip(Pawn pawn, HediffWithComps severe)
            => severe.LabelCap + ": " + severe.SeverityLabel + "\n" + severe.TipStringExtra;

        private float GetTextFor(HediffWithComps hediff)
            => hediff?.TryGetComp<HediffComp_Immunizable>().Immunity - hediff?.Severity ?? -1f; //nullcheck for Comparer.

        private Color GetPrettyColorFor(float deltaSeverity)
        {
            if (deltaSeverity <= -0.4f)
                return HealthUtility.RedColor;

            if (deltaSeverity <= -0.2f)
                return SeverePainColor;

            if (deltaSeverity <= -0.1f)
                return HealthUtility.ImpairedColor;

            if (deltaSeverity <= -0.05f)
                return HealthUtility.SlightlyImpairedColor;

            if (deltaSeverity <= 0.05f)
                return Color.gray;

            return HealthUtility.GoodConditionColor;
        }

        private HediffWithComps FindMostSevereHediff(Pawn pawn)
        {
            IEnumerable<HediffWithComps> tmplist =
                pawn.health.hediffSet.hediffs.Where(x => x.Visible && x is HediffWithComps && !x.FullyImmune()).Cast<HediffWithComps>();

            float delta = float.MinValue;
            HediffWithComps mostSevereHediff = null;

            foreach (HediffWithComps hediff in tmplist)
            {
                HediffComp_Immunizable hediffCompImmunizable = hediff.TryGetComp<HediffComp_Immunizable>();

                if (hediffCompImmunizable == null)
                    continue;

                if (hediffCompImmunizable.Immunity - hediff.Severity > delta)
                {
                    delta = hediffCompImmunizable.Immunity - hediff.Severity;
                    mostSevereHediff = hediff;
                }
            }

            return mostSevereHediff;
        }

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(def.LabelCap.Resolve().WordWrapAt(GetMinWidth(table))).y);

        public override int Compare(Pawn a, Pawn b)
            => GetTextFor(FindMostSevereHediff(a)).CompareTo(GetTextFor(FindMostSevereHediff(b)));
    }
}
