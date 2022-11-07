using System.Linq;
using System.Reflection;
using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality.Patches
{
    public class SocialCardUtility_Patch
    {
        /// <summary>
        /// Highlight pawns that are guests' friends
        /// </summary>
        [HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.DrawPawnRow))]
        public class DrawPawnRow
        {
            private static readonly Color HighlightColorFriend = new Color(0.0f, 0.5f, 0.0f, 1f);
            private static readonly Color HighlightColorFriendRelated = new Color(0.0f, 0.75f, 0.0f, 1f);
            private static readonly Color HighlightColorEnemy = new Color(0.5f, 0.0f, 0.0f, 1f);
            private static readonly Color HighlightColorEnemyRelated = new Color(0.75f, 0.0f, 0.0f, 1f);
            private static readonly Color HighlightColorSlave = new Color(0.35f, 0.35f, 0.35f, 1f);

            [HarmonyPrefix]
            public static bool Prefix(float y, float width,  SocialCardUtility.CachedSocialTabEntry entry, Pawn selPawnForSocialInfo)
            {
                if (!selPawnForSocialInfo.IsGuest()) return true;

                Pawn otherPawn = entry.otherPawn; 
                if (!otherPawn.IsColonist) return true;

                var guest = selPawnForSocialInfo;

                // We don't care about non-royal colonists
                if (guest.royalty?.MostSeniorTitle != null && otherPawn.royalty?.MostSeniorTitle == null) return true;

                //SocialCardUtility.CachedSocialTabEntry
                float rowHeight = SocialCardUtility.GetRowHeight(entry, width, selPawnForSocialInfo);
                Rect rect = new Rect(0f, y, width, rowHeight);

                if (otherPawn.IsSlave)
                {
                    return true;
                }

                float requiredOpinion = guest.GetMinRecruitOpinion();

                float opinion = guest.relations.OpinionOf(otherPawn);
                var related = guest.relations.RelatedPawns.Any(rel => rel == otherPawn);

                float percent;
                if (opinion > 0)
                {
                    percent = opinion / requiredOpinion;
                    GUI.color = related ? HighlightColorFriendRelated : HighlightColorFriend;
                }
                else
                {
                    percent = opinion / GuestUtility.MaxOpinionForEnemy;
                    GUI.color = related ? HighlightColorEnemyRelated : HighlightColorEnemy;
                }

                DrawBar(rect, percent);
                return true;
            }

            private static void DrawBar(Rect rect, float percent)
            {
                rect.width *= percent;
                GUI.DrawTexture(rect, TexUI.HighlightTex);
            }
        }

        /// <summary>
        /// Show "Colony" in social tab for player pawns
        /// </summary>
        [HarmonyPatch(typeof(SocialCardUtility), nameof(SocialCardUtility.GetPawnSituationLabel))]
        public class GetPawnSituationLabel
        {
            [HarmonyPrefix]
            public static bool Replacement(ref string __result, Pawn pawn, Pawn fromPOV)
            {
                if (pawn.Dead)
                {
                    __result = "Dead".Translate();
                    return false;
                }
                if (pawn.Destroyed)
                {
                    __result = "Missing".Translate();
                    return false;
                }
                if (PawnUtility.IsKidnappedPawn(pawn))
                {
                    __result = "Kidnapped".Translate();
                    return false;
                }
                if (pawn.kindDef == PawnKindDefOf.Slave || pawn.IsSlave)
                {
                    __result = "Slave".Translate();
                    return false;
                }
                if (PawnUtility.IsFactionLeader(pawn))
                {
                    __result = "FactionLeader".Translate();
                    return false;
                }
                Faction faction = pawn.Faction;
                if (faction == fromPOV.Faction)
                {
                    __result = string.Empty;
                    return false;
                }
                if (faction == null || fromPOV.Faction == null)
                {
                    __result = "Neutral".Translate();
                    return false;
                }

                #region ADDED
                if (faction == Faction.OfPlayer)
                {
                    __result = "Colony".Translate();
                    return false;
                }
                #endregion

                switch (faction.RelationKindWith(fromPOV.Faction))
                {
                    case FactionRelationKind.Hostile:
                        __result = "Hostile".Translate() + ", " + faction.Name;
                        return false;
                    case FactionRelationKind.Neutral:
                        __result = "Neutral".Translate() + ", " + faction.Name;
                        return false;
                    case FactionRelationKind.Ally:
                        __result = "Ally".Translate() + ", " + faction.Name;
                        return false;
                    default:
                        __result = "";
                        return false;
                }
            }
        }
    }
}