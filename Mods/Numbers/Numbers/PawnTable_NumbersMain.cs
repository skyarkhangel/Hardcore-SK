namespace Numbers
{
    using System;
    using System.Collections.Generic;
    using RimWorld;
    using Verse;

    public class PawnTable_NumbersMain : PawnTable
    {
        private List<PawnColumnDef> _originalColumns;

        public PawnTable_NumbersMain(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : base(def, pawnsGetter, uiWidth, uiHeight)
        {
            PawnTableDef = def;
            _originalColumns = def.columns;

            SetMinMaxSize(def.minWidth, uiWidth, 0, (int)(uiHeight * Numbers_Settings.maxHeight));
            SetDirty();
        }

        public PawnTableDef PawnTableDef { get; protected set; }

        public bool RemoveColumn(PawnColumnDef columnDef)
        {
            return PawnTableDef.columns.Remove(columnDef);
        }

        public int RemoveColumns(Predicate<PawnColumnDef> predicate)
        {
            return PawnTableDef.columns.RemoveAll(predicate);
        }
    }
}
