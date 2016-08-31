using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    // Token: 0x02000341 RID: 833
    internal static class _PawnRelationWorker_Sibling
    {
        // Token: 0x06000CF2 RID: 3314 RVA: 0x00040A6C File Offset: 0x0003EC6C
        internal static Pawn _GenerateParent(Pawn generatedChild, Pawn existingChild, Gender genderToGenerate, PawnGenerationRequest childRequest, bool newlyGeneratedParentsWillBeSpousesIfNotGay)
        {
            float ageChronologicalYearsFloat = generatedChild.ageTracker.AgeChronologicalYearsFloat;
            float ageChronologicalYearsFloat2 = existingChild.ageTracker.AgeChronologicalYearsFloat;
            float num = (genderToGenerate != Gender.Male) ? 16f : 14f;
            float num2 = (genderToGenerate != Gender.Male) ? 45f : 50f;
            float num3 = (genderToGenerate != Gender.Male) ? 27f : 30f;
            float num4 = Mathf.Max(ageChronologicalYearsFloat, ageChronologicalYearsFloat2) + num;
            float maxChronologicalAge = num4 + (num2 - num);
            float midChronologicalAge = num4 + (num3 - num);
            float value;
            float value2;
            float value3;
            string last;
            GenerateParentParams(num4, maxChronologicalAge, midChronologicalAge, num, generatedChild, existingChild, childRequest, out value, out value2, out value3, out last);
            bool allowGay = true;
            if (newlyGeneratedParentsWillBeSpousesIfNotGay && last.NullOrEmpty() && Rand.Value < 0.8f)
            {
                if (genderToGenerate == Gender.Male && existingChild.GetMother() != null && !existingChild.GetMother().story.traits.HasTrait(TraitDefOf.Gay) && !existingChild.GetMother().story.traits.HasTrait(TraitDefOfPsychology.Bisexual))
                {
                    last = ((NameTriple)existingChild.GetMother().Name).Last;
                    allowGay = false;
                }
                else if (genderToGenerate == Gender.Female && existingChild.GetFather() != null && !existingChild.GetFather().story.traits.HasTrait(TraitDefOf.Gay) && !existingChild.GetMother().story.traits.HasTrait(TraitDefOfPsychology.Bisexual))
                {
                    last = ((NameTriple)existingChild.GetFather().Name).Last;
                    allowGay = false;
                }
            }
            Faction faction = existingChild.Faction;
            if (faction == null || faction.IsPlayer)
            {
                bool tryMedievalOrBetter = faction != null && faction.def.techLevel >= TechLevel.Medieval;
                Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter);
            }
            float? fixedChronologicalAge = new float?(value2);
            Gender? fixedGender = new Gender?(genderToGenerate);
            float? fixedSkinWhiteness = new float?(value3);
            PawnGenerationRequest request = new PawnGenerationRequest(existingChild.kindDef, faction, PawnGenerationContext.NonPlayer, true, false, true, true, false, false, 1f, false, allowGay, null, new float?(value), fixedChronologicalAge, fixedGender, fixedSkinWhiteness, last);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            if (!Find.WorldPawns.Contains(pawn))
            {
                Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Keep);
            }
            return pawn;
        }

        // Token: 0x06000CF3 RID: 3315 RVA: 0x00040C84 File Offset: 0x0003EE84
        internal static void GenerateParentParams(float minChronologicalAge, float maxChronologicalAge, float midChronologicalAge, float minBioAgeToHaveChildren, Pawn generatedChild, Pawn existingChild, PawnGenerationRequest childRequest, out float biologicalAge, out float chronologicalAge, out float skinWhiteness, out string lastName)
        {
            chronologicalAge = Rand.GaussianAsymmetric(midChronologicalAge, (midChronologicalAge - minChronologicalAge) / 2f, (maxChronologicalAge - midChronologicalAge) / 2f);
            chronologicalAge = Mathf.Clamp(chronologicalAge, minChronologicalAge, maxChronologicalAge);
            biologicalAge = Rand.Range(minBioAgeToHaveChildren, Mathf.Min(existingChild.RaceProps.lifeExpectancy, chronologicalAge));
            if (existingChild.GetFather() != null)
            {
                skinWhiteness = ParentRelationUtility.GetRandomSecondParentSkinColor(existingChild.GetFather().story.skinWhiteness, existingChild.story.skinWhiteness, childRequest.FixedSkinWhiteness);
            }
            else if (existingChild.GetMother() != null)
            {
                skinWhiteness = ParentRelationUtility.GetRandomSecondParentSkinColor(existingChild.GetMother().story.skinWhiteness, existingChild.story.skinWhiteness, childRequest.FixedSkinWhiteness);
            }
            else if (!childRequest.FixedSkinWhiteness.HasValue)
            {
                skinWhiteness = PawnSkinColors.GetRandomSkinColorSimilarTo(existingChild.story.skinWhiteness, 0f, 1f);
            }
            else
            {
                float num = Mathf.Min(childRequest.FixedSkinWhiteness.Value, existingChild.story.skinWhiteness);
                float num2 = Mathf.Max(childRequest.FixedSkinWhiteness.Value, existingChild.story.skinWhiteness);
                if (Rand.Value < 0.5f)
                {
                    skinWhiteness = PawnSkinColors.GetRandomSkinColorSimilarTo(num, 0f, num);
                }
                else
                {
                    skinWhiteness = PawnSkinColors.GetRandomSkinColorSimilarTo(num2, num2, 1f);
                }
            }
            lastName = null;
            if (!ChildRelationUtility.DefinitelyHasNotBirthName(existingChild) && ChildRelationUtility.ChildWantsNameOfAnyParent(existingChild))
            {
                if (existingChild.GetMother() == null && existingChild.GetFather() == null)
                {
                    if (Rand.Value < 0.5f)
                    {
                        lastName = ((NameTriple)existingChild.Name).Last;
                    }
                }
                else
                {
                    string last = ((NameTriple)existingChild.Name).Last;
                    string b = null;
                    if (existingChild.GetMother() != null)
                    {
                        b = ((NameTriple)existingChild.GetMother().Name).Last;
                    }
                    else if (existingChild.GetFather() != null)
                    {
                        b = ((NameTriple)existingChild.GetFather().Name).Last;
                    }
                    if (last != b)
                    {
                        lastName = last;
                    }
                }
            }
        }
    }
}
