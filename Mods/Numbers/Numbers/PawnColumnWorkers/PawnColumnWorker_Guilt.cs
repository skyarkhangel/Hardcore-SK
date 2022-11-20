namespace Numbers
{
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class PawnColumnWorker_Guilt : PawnColumnWorker_Icon
    {
        protected override string GetIconTip(Pawn pawn)
        {
            return pawn.guilt?.Tip;
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return a.guilt?.TicksUntilInnocent.CompareTo(b.guilt?.TicksUntilInnocent) ?? base.Compare(a, b);
        }

        protected override Texture2D GetIconFor(Pawn pawn)
        {
            return (pawn.guilt?.IsGuilty ?? false) ? TexUI.GuiltyTex : null;
        }
    }
}
