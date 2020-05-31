using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospitality.MainTab
{
    public class PawnTable_Guests : PawnTable
    {
        public PawnTable_Guests(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : base(def, pawnsGetter, uiWidth, uiHeight) { }
        
        // Removed, so lord groups can be drawn by default (pawns are ordered by lord)
        protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
        {
            return input;
        }
    }
}
