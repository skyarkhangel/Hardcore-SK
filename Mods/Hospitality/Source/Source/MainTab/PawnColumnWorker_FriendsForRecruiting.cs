using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.MainTab
{
    public class PawnColumnWorker_FriendsForRecruiting : PawnColumnWorker_Text
    {
        private Color enoughFriendsColor = new Color(0, 1, 0, 0.2f);
        private static readonly string txtRecruit = "Recruit".Translate();

        // Storing it just long enough to use it twice
        protected internal int friendsShortCache;
        protected internal int friendsRequiredShortCache;

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!pawn.IsGuest()) return;

            // Store cache
            friendsShortCache = pawn.GetFriendsInColony();
            friendsRequiredShortCache = GuestUtility.FriendsRequired(pawn.MapHeld) + pawn.GetEnemiesInColony();

            base.DoCell(rect, pawn, table);

            // Use cache - only get comp when we have enough friends
            if (friendsShortCache >= friendsRequiredShortCache && MayRecruitAtAll(pawn))
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

        private static bool MayRecruitAtAll(Pawn pawn)
        {
            var comp = pawn.CompGuest();
            var mayRecruitAtAll = !pawn.InMentalState && comp.arrived;
            return mayRecruitAtAll;
        }

        protected override string GetTextFor(Pawn pawn)
        {
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

            friendsShortCache = pawn.GetFriendsInColony();
            friendsRequiredShortCache = GuestUtility.FriendsRequired(pawn.MapHeld) + pawn.GetEnemiesInColony();

            if (friendsRequiredShortCache == 0) return -2147483648;
            return (int)(100f * friendsShortCache / friendsRequiredShortCache);
        }
    }
}
