namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Stat : PawnColumnWorker_Text
    {
        public const int minWidthBasedOnNarrowestColumnThatColumnBeingMass = 69;
        public const int maxWidthBasedOnColumnsWithALongAssNameLikeThisInt = 150;
        public const int margin = 5;

        protected override string GetTextFor(Pawn pawn)
        {
            Thing thing = pawn;

            if (pawn.ParentHolder is Corpse tmpCorpse  && this.def.Ext().stat != StatDefOf.LeatherAmount) //this is dumb, but corpses don't seem to have leather.
                thing = tmpCorpse;

            return def.Ext().stat.Worker.IsDisabledFor(thing) ? null
                 : def.Ext().stat.Worker.ValueToString(thing.GetStatValue(def.Ext().stat), true);
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(minWidthBasedOnNarrowestColumnThatColumnBeingMass,
                         Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, maxWidthBasedOnColumnsWithALongAssNameLikeThisInt)).x))
                    + margin;

        public override int GetMinHeaderHeight(PawnTable table)
            => Mathf.CeilToInt(Text.CalcSize(Numbers_Utility.WordWrapAt(def.LabelCap, GetMinWidth(table))).y); //not messy at all.

        public override int Compare(Pawn a, Pawn b)
            => (def.Ext().stat.Worker.IsDisabledFor(a) ? 0 : a.GetStatValue(def.Ext().stat)).CompareTo
               (def.Ext().stat.Worker.IsDisabledFor(b) ? 0 : b.GetStatValue(def.Ext().stat));

    }
}
 