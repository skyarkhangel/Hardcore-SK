using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality
{
    public class StatWorker_ForcedRelationshipDamage : StatWorker_RelationshipDamage
    {
        private static int penaltyPer10PercentMissing = 10;

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (!req.HasThing) return 0;
            if (!(req.Thing is Pawn pawn)) return 0;
            return base.GetValueUnfinalized(req, applyPostProcess) + GetExtraPenalty(pawn);
        }

        private static int GetExtraPenalty(Pawn pawn)
        {
            return Mathf.RoundToInt(GetMissingPercentage(pawn) * penaltyPer10PercentMissing * 10);
        }

        private static float GetMissingPercentage(Pawn pawn)
        {
            var friends = pawn.GetFriendsInColony();
            var friendsRequired = GuestUtility.FriendsRequired(pawn.MapHeld) + pawn.GetEnemiesInColony();
            if (friendsRequired <= 0 || friends >= friendsRequired) return 0;
            return 1 - 1f * friends / friendsRequired;
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            var stringBuilder = new StringBuilder(base.GetExplanationUnfinalized(req, numberSense));
            if (!req.HasThing || !(req.Thing is Pawn pawn)) return stringBuilder.ToString();

            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"{"StatsReport_MissingFriends".Translate()}: +{GetMissingPercentage(pawn).ToStringPercent()} x{penaltyPer10PercentMissing}");
            return stringBuilder.ToString();
        }
    }
}
