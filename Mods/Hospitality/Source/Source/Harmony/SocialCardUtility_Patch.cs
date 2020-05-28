using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.Harmony
{
    public class SocialCardUtility_Patch
    {
        /// <summary>
        /// Highlight pawns that are guests' friends
        /// </summary>
        [HarmonyPatch(typeof(SocialCardUtility), "DrawPawnRow")]
        public class DrawPawnRow
        {
            private static MethodInfo _getRowHeight;
            private static readonly Color HighlightColorFriend = new Color(0.0f, 0.5f, 0.0f, 1f);
            private static readonly Color HighlightColorFriendRelated = new Color(0.0f, 0.75f, 0.0f, 1f);
            private static readonly Color HighlightColorEnemy = new Color(0.5f, 0.0f, 0.0f, 1f);
            private static readonly Color HighlightColorEnemyRelated = new Color(0.75f, 0.0f, 0.0f, 1f);

            [HarmonyPrefix]
            public static bool Prefix(float y, float width,  object entry, Pawn selPawnForSocialInfo)
            {
                if (!selPawnForSocialInfo.IsGuest()) return true;

                Pawn otherPawn = Traverse.Create(entry).Field<Pawn>("otherPawn").Value;
                if (!otherPawn.IsColonist) return true;

                var guest = selPawnForSocialInfo;

                // We don't care about non-royal colonists
                if (guest.royalty?.MostSeniorTitle != null && otherPawn.royalty?.MostSeniorTitle == null) return true;

                //SocialCardUtility.CachedSocialTabEntry
                if (_getRowHeight == null) _getRowHeight = AccessTools.Method(typeof(SocialCardUtility), "GetRowHeight");
                float rowHeight = (float) _getRowHeight.Invoke(null, new[] {entry, width, selPawnForSocialInfo});
                Rect rect = new Rect(0f, y, width, rowHeight);

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
                rect.width *= percent;

                GUI.DrawTexture(rect, TexUI.HighlightTex);

                return true;
            }

        }

        /// <summary>
        /// Show "Colony" in social tab for player pawns
        /// </summary>
        [HarmonyPatch(typeof(SocialCardUtility), "GetPawnSituationLabel")]
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
                if (pawn.kindDef == PawnKindDefOf.Slave)
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