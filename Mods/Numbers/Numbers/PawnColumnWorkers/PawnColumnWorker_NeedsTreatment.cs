namespace Numbers
{
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_NeedsTreatment : PawnColumnWorker_Icon
    {
        protected override Texture2D GetIconFor(Pawn pawn)
            => pawn.health.HasHediffsNeedingTendByPlayer()
                ? StaticConstructorOnGameStart.IconTendedNeed
                : StaticConstructorOnGameStart.IconTendedWell;

        public override int Compare(Pawn a, Pawn b)
            => a.health.hediffSet.GetHediffsTendable().Count()
                .CompareTo(b.health.hediffSet.GetHediffsTendable().Count());

        protected override string GetIconTip(Pawn pawn)
            => pawn.health.hediffSet.GetHediffsTendable()
                .Select(x => x.LabelCap).ToCommaList();

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}
