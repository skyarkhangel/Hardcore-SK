namespace Numbers
{
    using System.Collections.Generic;
    using System.Linq;
    using RimWorld;
    using UnityEngine;
    using Verse;
    using System.Reflection;

    public class PawnColumnWorker_Race : PawnColumnWorker_Text
    {
        private readonly Dictionary<PawnTableDef, float> widthsTables = WorldComponent_Numbers.PrimaryFilter.Keys.ToDictionary(x => x, x => 0f);

        protected override string GetTextFor(Pawn pawn)
        {
            string text = pawn.kindDef.race.LabelCap.Resolve() ?? string.Empty;

            if (Find.WindowStack.currentlyDrawnWindow is MainTabWindow_Numbers numbers)
            {
                float width = Mathf.Max( (int)Text.CalcSize(text).x , widthsTables[numbers.pawnTableDef]);
                widthsTables[numbers.pawnTableDef] = width;
            }

            return text;
        }

        public override int GetMinWidth(PawnTable table)
        {
            if (!(typeof(PawnTable).GetField("def", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(table) is PawnTableDef tableDef))
                throw new System.Exception($"tableDef not found {table}");

            return Mathf.Max(base.GetMinWidth(table), 80, (int)widthsTables[tableDef]);
        }

        public override int Compare(Pawn a, Pawn b) => a.kindDef.race.LabelCap.Resolve().CompareTo(b.kindDef.race.LabelCap.Resolve());
    }
}
