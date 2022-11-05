using RimWorld;
using UnityEngine;
using Verse;

namespace Numbers
{
    public class PawnColumnWorker_Meditation : PawnColumnWorker_Text
    {
        protected override string GetTextFor(Pawn pawn)
        {
            return MeditationUtility.FocusTypesAvailableForPawnString(pawn);
        }

        public override int GetMinWidth(PawnTable table)
            => Mathf.Max(base.GetMinWidth(table), 200);
    }
}
