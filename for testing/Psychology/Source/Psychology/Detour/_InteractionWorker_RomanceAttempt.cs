using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    // Token: 0x02000328 RID: 808
    internal static class _InteractionWorker_RomanceAttempt
    {

        // Token: 0x06000C81 RID: 3201 RVA: 0x0003E0C0 File Offset: 0x0003C2C0
        internal static void _BreakLoverAndFianceRelations(this InteractionWorker_RomanceAttempt _this, Pawn pawn, out List<Pawn> oldLoversAndFiances)
        {
            oldLoversAndFiances = new List<Pawn>();
            while (true)
            {
                Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                if (firstDirectRelationPawn != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
                    oldLoversAndFiances.Add(firstDirectRelationPawn);
                }
                else
                {
                    Pawn firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                    if (firstDirectRelationPawn2 == null)
                    {
                        break;
                    }
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn2);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn2);
                    oldLoversAndFiances.Add(firstDirectRelationPawn2);
                }
            }
        }

        // Token: 0x06000C7E RID: 3198 RVA: 0x0003DCB8 File Offset: 0x0003BEB8
        internal static float _RandomSelectionWeight(this InteractionWorker_RomanceAttempt _this, Pawn initiator, Pawn recipient)
        {
            if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                return 0f;
            }
            if (LovePartnerRelationUtility.HasAnyLovePartner(initiator) && initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                return 0f;
            }
            float num = initiator.relations.AttractionTo(recipient);
            int num2 = initiator.relations.OpinionOf(recipient);
            if (!initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher))
            {
                if (num < 0.25f)
                {
                    return 0f;
                }
                if (num2 < 5)
                {
                    return 0f;
                }
                if (recipient.relations.OpinionOf(initiator) < 5)
                {
                    return 0f;
                }
            }
            else
            {
                num = 0.25f;
                num2 = 5;
            }
            float num3 = 1f;
            Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
            if (pawn != null)
            {
                float value = (float)initiator.relations.OpinionOf(pawn);
                num3 = Mathf.InverseLerp(50f, -50f, value);
            }
            float num4 = (initiator.gender != Gender.Female) ? 1f : 0.125f;
            float num5 = Mathf.InverseLerp(0.25f, 1f, num);
            float num6 = Mathf.InverseLerp(5f, 100f, (float)num2);
            float num7 = (initiator.story.traits.HasTrait(TraitDefOfPsychology.Lecher)) ? 0.25f : 0f;
            return 1.15f * num4 * num5 * num6 * num3 + num7;
        }

        // Token: 0x06000C80 RID: 3200 RVA: 0x0003DF08 File Offset: 0x0003C108
        internal static void _Interacted(this InteractionWorker_RomanceAttempt _this, Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            if (Rand.Value < _SuccessChance(_this, initiator, recipient))
            {
                List<Pawn> list;
                _BreakLoverAndFianceRelations(_this, initiator, out list);
                List<Pawn> list2;
                _BreakLoverAndFianceRelations(_this, recipient, out list2);
                for (int i = 0; i < list.Count; i++)
                {
                    _TryAddCheaterThought(_this, list[i], initiator);
                }
                for (int j = 0; j < list2.Count; j++)
                {
                    _TryAddCheaterThought(_this, list2[j], recipient);
                }
                initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExLover, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
                TaleRecorder.RecordTale(TaleDefOf.BecameLover, new object[]
                {
                    initiator,
                    recipient
                });
                initiator.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOf.BrokeUpWithMe, recipient);
                recipient.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOf.BrokeUpWithMe, initiator);
                initiator.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, recipient);
                recipient.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
                initiator.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOf.FailedRomanceAttemptOnMe, recipient);
                recipient.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
                if (initiator.IsColonist || recipient.IsColonist)
                {
                    var _SendNewLoversLetter = typeof(InteractionWorker_RomanceAttempt).GetMethod("SendNewLoversLetter", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (_SendNewLoversLetter != null)
                        _SendNewLoversLetter.Invoke(_this, new object[] { initiator, recipient, list, list2 });
                    else
                        Log.ErrorOnce("Unable to reflect InteractionWorker_RomanceAttempt.SendNewLoversLetter!", 305432421);
                }
                extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptAccepted);
                LovePartnerRelationUtility.TryToShareBed(initiator, recipient);
            }
            else
            {
                initiator.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
                extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptRejected);
            }
        }

        // Token: 0x06000C7F RID: 3199 RVA: 0x0003DDB0 File Offset: 0x0003BFB0
        internal static float _SuccessChance(this InteractionWorker_RomanceAttempt _this, Pawn initiator, Pawn recipient)
        {
            float num = 0.6f;
            num *= recipient.relations.AttractionTo(initiator);
            num *= Mathf.InverseLerp(5f, 100f, (float)recipient.relations.OpinionOf(initiator));
            float num2 = 1f;
            Pawn pawn = null;
            if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
            {
                pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
                num2 = recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent) ? 0.0f : 0.6f;
            }
            else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
            {
                pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
                num2 = recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent) ? 0.0f : 0.1f;
            }
            else if (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead)
            {
                pawn = recipient.GetSpouse();
                num2 = recipient.story.traits.HasTrait(TraitDefOfPsychology.Codependent) ? 0.0f : 0.3f;
            }
            if (pawn != null)
            {
                num2 *= Mathf.InverseLerp(100f, 0f, (float)recipient.relations.OpinionOf(pawn));
                num2 *= Mathf.Clamp01(1f - recipient.relations.AttractionTo(pawn));
            }
            num *= num2;
            num += (recipient.story.traits.HasTrait(TraitDefOfPsychology.Lecher) ? 0.5f : 0f);
            return Mathf.Clamp01(num);
        }

        // Token: 0x06000C82 RID: 3202 RVA: 0x0003E16C File Offset: 0x0003C36C
        internal static void _TryAddCheaterThought(this InteractionWorker_RomanceAttempt _this, Pawn pawn, Pawn cheater)
        {
            if (pawn.Dead)
            {
                return;
            }
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.CheatedOnMe, cheater);
            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, cheater);
        }
    }
}
