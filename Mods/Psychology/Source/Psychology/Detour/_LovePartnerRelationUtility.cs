using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;

namespace Psychology
{
    // Token: 0x0200032A RID: 810
    internal static class _LovePartnerRelationUtility
    {
        // Token: 0x06000C9A RID: 3226 RVA: 0x0003EAD8 File Offset: 0x0003CCD8
        internal static float _LovePartnerRelationGenerationChance(Pawn generated, Pawn other, PawnGenerationRequest request, bool ex)
        {
            if (generated.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                return 0f;
            }
            if (other.ageTracker.AgeBiologicalYearsFloat < 14f)
            {
                return 0f;
            }
            if (generated.gender == other.gender && (!other.story.traits.HasTrait(TraitDefOf.Gay) || !other.story.traits.HasTrait(TraitDefOfPsychology.Bisexual) || !request.AllowGay))
            {
                return 0f;
            }
            if (generated.gender != other.gender && other.story.traits.HasTrait(TraitDefOf.Gay))
            {
                return 0f;
            }
            var GetGenerationChanceAgeFactor = typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeFactor", BindingFlags.Static | BindingFlags.NonPublic);
            var GetGenerationChanceAgeGapFactor = typeof(LovePartnerRelationUtility).GetMethod("GetGenerationChanceAgeGapFactor", BindingFlags.Static | BindingFlags.NonPublic);
            if (GetGenerationChanceAgeFactor == null)
            {
                Log.ErrorOnce("Unable to reflect LovePartnerRelationUtility.GetGenerationChanceAgeFactor!", 305432421);
                return 0f;
            }
            if (GetGenerationChanceAgeGapFactor == null)
            {
                Log.ErrorOnce("Unable to reflect LovePartnerRelationUtility.GetGenerationChanceAgeGapFactor!", 305432421);
                return 0f;
            }
            float num = 1f;
            if (ex)
            {
                int num2 = 0;
                List<DirectPawnRelation> directRelations = other.relations.DirectRelations;
                for (int i = 0; i < directRelations.Count; i++)
                {
                    if (LovePartnerRelationUtility.IsExLovePartnerRelation(directRelations[i].def))
                    {
                        num2++;
                    }
                }
                num = Mathf.Pow(0.2f, (float)num2);
            }
            else if (LovePartnerRelationUtility.HasAnyLovePartner(other))
            {
                return 0f;
            }
            float num3 = (generated.gender != other.gender) ? 1f : 0.01f;
            float generationChanceAgeFactor = (float)GetGenerationChanceAgeFactor.Invoke(null, new object[] { generated });
            float generationChanceAgeFactor2 = (float)GetGenerationChanceAgeFactor.Invoke(null, new object[] { other });
            float generationChanceAgeGapFactor = (float)GetGenerationChanceAgeGapFactor.Invoke(null, new object[] { generated, other, ex });
            float num4 = 1f;
            if (generated.GetRelations(other).Any((PawnRelationDef x) => x.familyByBloodRelation))
            {
                num4 = 0.01f;
            }
            float num5;
            if (request.FixedSkinWhiteness.HasValue)
            {
                num5 = ChildRelationUtility.GetSkinSimilarityFactor(request.FixedSkinWhiteness.Value, other.story.skinWhiteness);
            }
            else
            {
                num5 = PawnSkinColors.GetWhitenessCommonalityFactor(other.story.skinWhiteness);
            }
            return num * generationChanceAgeFactor * generationChanceAgeFactor2 * generationChanceAgeGapFactor * num3 * num5 * num4;
        }
    }
}
