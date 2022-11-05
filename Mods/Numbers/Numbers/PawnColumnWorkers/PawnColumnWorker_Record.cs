namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Record : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            RecordDef recordDef = def.Ext().record;
            string recordValue;

            if (recordDef.type == RecordType.Time)
                recordValue = pawn.records.GetAsInt(recordDef).ToStringTicksToPeriod();
            else
                recordValue = pawn.records.GetValue(recordDef).ToString("0.##");

            return recordValue;
        }

        protected override string GetTip(Pawn pawn) => def.Ext().record.description;

        public override int GetMinWidth(PawnTable table) => Mathf.Max(50, Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, 150)).x));

        public override int GetMinHeaderHeight(PawnTable table) => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y); //not messy at all.

        public override int Compare(Pawn a, Pawn b)
        {
            RecordDef recordDef = def.Ext().record;

            if (recordDef.type == RecordType.Time)
                return a.records.GetAsInt(recordDef).CompareTo(b.records.GetAsInt(recordDef));
            return a.records.GetValue(recordDef).CompareTo(b.records.GetValue(recordDef));
        }

    }
}
