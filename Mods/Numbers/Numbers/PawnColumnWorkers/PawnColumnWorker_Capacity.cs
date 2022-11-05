namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Capacity : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
            => pawn.health.capacities.GetLevel(def.Ext().capacity).ToStringPercent();

        protected override string GetTip(Pawn pawn)
            => HealthCardUtility.GetPawnCapacityTip(pawn, def.Ext().capacity);

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            GUI.color = HealthCardUtility.GetEfficiencyLabel(pawn, def.Ext().capacity).Second;
            base.DoCell(rect, pawn, table);
            GUI.color = Color.white;
        }

        public override int GetMinWidth(PawnTable table)
            => base.GetMinWidth(table) + 8; //based on Sight column.

        public override int Compare(Pawn a, Pawn b)
            => a.health.capacities.GetLevel(def.Ext().capacity)
            .CompareTo(b.health.capacities.GetLevel(def.Ext().capacity));

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y);
    }
}
