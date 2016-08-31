using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    // Token: 0x0200031F RID: 799
    internal static class _ChildRelationUtility
    {
        // Token: 0x06000C59 RID: 3161 RVA: 0x0003C700 File Offset: 0x0003A900
        internal static float _ChanceOfBecomingChildOf(Pawn child, Pawn father, Pawn mother, PawnGenerationRequest? childGenerationRequest, PawnGenerationRequest? fatherGenerationRequest, PawnGenerationRequest? motherGenerationRequest)
        {
            if (father != null && father.gender != Gender.Male)
            {
                Log.Warning("Tried to calculate chance for father with gender \"" + father.gender + "\".");
                return 0f;
            }
            if (mother != null && mother.gender != Gender.Female)
            {
                Log.Warning("Tried to calculate chance for mother with gender \"" + mother.gender + "\".");
                return 0f;
            }
            if (father != null && child.GetFather() != null && child.GetFather() != father)
            {
                return 0f;
            }
            if (mother != null && child.GetMother() != null && child.GetMother() != mother)
            {
                return 0f;
            }
            if (mother != null && father != null && !LovePartnerRelationUtility.LovePartnerRelationExists(mother, father) && !LovePartnerRelationUtility.ExLovePartnerRelationExists(mother, father))
            {
                return 0f;
            }
            var GetSkinWhiteness = typeof(ChildRelationUtility).GetMethod("GetSkinWhiteness", BindingFlags.Static | BindingFlags.NonPublic);
            var GetSkinColorFactor = typeof(ChildRelationUtility).GetMethod("GetSkinColorFactor", BindingFlags.Static | BindingFlags.NonPublic);
            var GetParentAgeFactor = typeof(ChildRelationUtility).GetMethod("GetParentAgeFactor", BindingFlags.Static | BindingFlags.NonPublic);
            var NumberOfChildrenFemaleWantsEver = typeof(ChildRelationUtility).GetMethod("NumberOfChildrenFemaleWantsEver", BindingFlags.Static | BindingFlags.NonPublic);
            if (GetSkinWhiteness == null)
            {
                Log.ErrorOnce("Unable to reflect ChildRelationUtility.GetSkinWhiteness!", 305432421);
                return 0f;
            }
            if (GetSkinColorFactor == null)
            {
                Log.ErrorOnce("Unable to reflect ChildRelationUtility.GetSkinColorFactor!", 305432421);
                return 0f;
            }
            if (GetParentAgeFactor == null)
            {
                Log.ErrorOnce("Unable to reflect ChildRelationUtility.GetParentAgeFactor!", 305432421);
                return 0f;
            }
            if (NumberOfChildrenFemaleWantsEver == null)
            {
                Log.ErrorOnce("Unable to reflect ChildRelationUtility.NumberOfChildrenFemaleWantsEver!", 305432421);
                return 0f;
            }
            float? skinWhiteness = (float?)GetSkinWhiteness.Invoke(null,new object[] { child, childGenerationRequest });
            float? skinWhiteness2 = (float?)GetSkinWhiteness.Invoke(null,new object[] { father, fatherGenerationRequest });
            float? skinWhiteness3 = (float?)GetSkinWhiteness.Invoke(null,new object[] { mother, motherGenerationRequest });
            bool fatherIsNew = father != null && child.GetFather() != father;
            bool motherIsNew = mother != null && child.GetMother() != mother;
            float skinColorFactor = (float)GetSkinColorFactor.Invoke(null, new object[] { skinWhiteness, skinWhiteness2, skinWhiteness3, fatherIsNew, motherIsNew});
            if (skinColorFactor <= 0f)
            {
                return 0f;
            }
            float num = 1f;
            float num2 = 1f;
            float num3 = 1f;
            float num4 = 1f;
            if (father != null && child.GetFather() == null)
            {
                num = (float)GetParentAgeFactor.Invoke(null, new object[] { father, child, 14f, 30f, 50f });
                if (num == 0f)
                {
                    return 0f;
                }
                if (father.story.traits.HasTrait(TraitDefOf.Gay))
                {
                    num4 = 0.1f;
                }
                if (father.story.traits.HasTrait(TraitDefOfPsychology.Bisexual))
                {
                    num4 = 0.6f;
                }
            }
            if (mother != null && child.GetMother() == null)
            {
                num2 = (float)GetParentAgeFactor.Invoke(null, new object[] { mother, child, 16f, 27f, 45f });
                if (num2 == 0f)
                {
                    return 0f;
                }
                int num5 = (int)NumberOfChildrenFemaleWantsEver.Invoke(null, new object[] { mother });
                if (mother.relations.ChildrenCount >= num5)
                {
                    return 0f;
                }
                num3 = 1f - (float)mother.relations.ChildrenCount / (float)num5;
                if (mother.story.traits.HasTrait(TraitDefOf.Gay))
                {
                    num4 = 0.1f;
                }
            }
            float num6 = 1f;
            if (mother != null)
            {
                Pawn firstDirectRelationPawn = mother.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
                if (firstDirectRelationPawn != null && firstDirectRelationPawn != father)
                {
                    num6 *= 0.15f;
                }
            }
            if (father != null)
            {
                Pawn firstDirectRelationPawn2 = father.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse, null);
                if (firstDirectRelationPawn2 != null && firstDirectRelationPawn2 != mother)
                {
                    num6 *= 0.15f;
                }
            }
            return skinColorFactor * num * num2 * num3 * num6 * num4;
        }
    }
}
