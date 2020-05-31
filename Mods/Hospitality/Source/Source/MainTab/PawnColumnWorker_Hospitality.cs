using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    public class PawnColumnWorker_Hospitality : PawnColumnWorker_Text
    {
        protected internal float score;

        protected override string GetTextFor(Pawn pawn)
        {
            if (pawn.GetVisitScore(out score))
            {
                return Mathf.Clamp01(score).ToStringPercent();
            }

            return string.Empty;
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return -GetValueToCompare(a).CompareTo(GetValueToCompare(b));
        }

        private static int GetValueToCompare(Pawn pawn)
        {
            // Can't use cache
            return pawn.GetVisitScore(out float s) ? (int) (s * 100) : 0;
        }
    }
}
