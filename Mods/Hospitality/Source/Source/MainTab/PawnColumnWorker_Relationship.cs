using RimWorld;
using UnityEngine;
using Verse;
using static Hospitality.RelationUtility;

namespace Hospitality.MainTab
{
    [StaticConstructorOnStartup]
    public class PawnColumnWorker_Relationship : PawnColumnWorker_Icon
    {
        private static readonly Texture2D Icon = ContentFinder<Texture2D>.Get("UI/Tab/Relationship");
        private bool mayDrawLordGroups;

        public override Texture2D GetIconFor(Pawn pawn)
        {
            if (pawn == null) return null;
            return GetRelationInfo(pawn).hasRelationship ? Icon : null;
        }

        public override string GetIconTip(Pawn pawn)
        {
            return GetRelationInfo(pawn).tooltip;
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            base.DoCell(rect, pawn, table);

            // As this is the first column shown we can watch from here
            // Move accordingly
            if (Multiplayer.IsRunning) {
                var compGuest = pawn.CompGuest();
                if (compGuest != null) {
                    Multiplayer.guestFields?.Watch(compGuest);
                }
            }

            // Only run once
            if (!mayDrawLordGroups) return;
            mayDrawLordGroups = false;

            MainTabUtility.DrawLordGroups(rect, table, pawn);
        }

        public override void DoHeader(Rect rect, PawnTable table)
        {
            base.DoHeader(rect, table);
            mayDrawLordGroups = true;
        }
    }
}
