namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Bleedrate : PawnColumnWorker_Text
    {
        private static readonly Color MediumPainColor = new Color(0.9f, 0.9f, 0f);

        private static readonly Color SeverePainColor = new Color(0.9f, 0.5f, 0f);

        public override int Compare(Pawn a, Pawn b)
            => a.health.hediffSet.BleedRateTotal.CompareTo(b.health.hediffSet.BleedRateTotal);

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            GUI.color = FindPrettyColourForBleedingPawn(pawn);
            base.DoCell(rect, pawn, table);
            GUI.color = Color.white;
        }

        protected override string GetTextFor(Pawn pawn)
            => pawn.health.hediffSet.BleedRateTotal.ToStringPercent();

        protected override string GetTip(Pawn pawn)
        {
            int ticksUntilDeathDueToBloodLoss = HealthUtility.TicksUntilDeathDueToBloodLoss(pawn);

            if (ticksUntilDeathDueToBloodLoss < GenDate.TicksPerDay)
                return "TimeToDeath".Translate(ticksUntilDeathDueToBloodLoss.ToStringTicksToPeriod());

            return "WontBleedOutSoon".Translate();
        }

        private Color FindPrettyColourForBleedingPawn(Pawn pawn)
        {
            float isThisMyBlood = pawn.health.hediffSet.BleedRateTotal;

            if (isThisMyBlood <= 0f)
                return HealthUtility.GoodConditionColor;

            if (isThisMyBlood < 0.8f)
                return Color.gray;

            if (isThisMyBlood < 1f && !Mathf.Approximately(isThisMyBlood, 1f))
                return MediumPainColor;

            return Mathf.Approximately(isThisMyBlood, 1f)
                ? SeverePainColor
                : HealthUtility.RedColor;
        }
    }
}
