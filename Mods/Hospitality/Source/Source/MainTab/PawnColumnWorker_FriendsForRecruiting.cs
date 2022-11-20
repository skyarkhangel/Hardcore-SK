using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality.MainTab
{
    public class PawnColumnWorker_FriendsForRecruiting : PawnColumnWorker_Text
    {
        private static readonly string txtRecruit = "Recruit".Translate();

        // Storing it just long enough to use it twice
        protected internal int friendsShortCache;
        protected internal int friendsRequiredShortCache;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!pawn.IsGuest()) return;

            UpdateCache(pawn);

            base.DoCell(rect, pawn, table);

            bool CheckCanRecruit() 
            {
                // Getting comp is expensive
                var comp = pawn.CompGuest();
                var isRoyal = pawn.royalty?.MostSeniorTitle != null;
                var willOnlyJoinByForce = comp.WillOnlyJoinByForce;
                var canNeverRecruit = pawn.Faction?.HasGoodwill == false || isRoyal && willOnlyJoinByForce;
                return !willOnlyJoinByForce && !canNeverRecruit && pawn.MayRecruitRightNow();
            }

            // Use cache - only get comp when we have enough friends
            if (friendsShortCache >= friendsRequiredShortCache && CheckCanRecruit())
            {
                var rect2 = rect;
                rect2.x -= 4;
                if (Widgets.ButtonText(rect2, txtRecruit))
                {
                    ITab_Pawn_Guest.RecruitDialog(pawn, false);
                }
            }
            else base.DoCell(rect, pawn, table);
        }

        public override string GetTextFor(Pawn pawn)
        {
            if (pawn.CompGuest().WillOnlyJoinByForce) return "-";
            // Use cache
            return $"{friendsShortCache}/{friendsRequiredShortCache}";
        }

        public override int Compare(Pawn a, Pawn b)
        {
            return -GetValueToCompare(a).CompareTo(GetValueToCompare(b));
        }

        private int GetValueToCompare(Pawn pawn)
        {
            // Can't use cache here

            // Changed check
            if (!pawn.IsGuest())
            {
                return -2147483648;
            }

            UpdateCache(pawn);

            if (friendsRequiredShortCache == 0) return -2147483648;
            return (int)(100f * friendsShortCache / friendsRequiredShortCache);
        }

        private void UpdateCache(Pawn pawn)
        {
            var isRoyal = pawn.royalty?.MostSeniorTitle != null;
            friendsShortCache = isRoyal ? pawn.GetFriendsSeniorityInColony()/100 : pawn.GetFriendsInColony();
            friendsRequiredShortCache = isRoyal ? (GuestUtility.RoyalFriendsSeniorityRequired(pawn) + pawn.GetRoyalEnemiesSeniorityInColony())/100 : GuestUtility.FriendsRequired(pawn.MapHeld) + pawn.GetEnemiesInColony();
        }
    }
}
