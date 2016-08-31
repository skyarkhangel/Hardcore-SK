using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    // Token: 0x02000320 RID: 800
    internal static class _InteractionWorker_Breakup
    {
        // Token: 0x06000C67 RID: 3175 RVA: 0x0003CEEC File Offset: 0x0003B0EC
        internal static void _Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
        {
            if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
            {
                initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, recipient);
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.DivorcedMe, initiator);
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
                initiator.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDef(ThoughtDefOf.GotMarried);
                recipient.needs.mood.thoughts.memories.RemoveMemoryThoughtsOfDef(ThoughtDefOf.GotMarried);
                initiator.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOf.HoneymoonPhase, recipient);
                recipient.needs.mood.thoughts.memories.RemoveSocialMemoryThoughts(ThoughtDefOf.HoneymoonPhase, initiator);
            }
            else
            {
                initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOf.BrokeUpWithMe, initiator);
                recipient.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.BrokeUpWithMeCodependent, initiator);
            }
            if (initiator.ownership.OwnedBed != null && initiator.ownership.OwnedBed == recipient.ownership.OwnedBed)
            {
                Pawn pawn = (Rand.Value >= 0.5f) ? recipient : initiator;
                pawn.ownership.UnclaimBed();
            }
            TaleRecorder.RecordTale(TaleDefOf.Breakup, new object[]
            {
                initiator,
                recipient
            });
            if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
            {
                Find.LetterStack.ReceiveLetter("LetterLabelBreakup".Translate(), "LetterNoLongerLovers".Translate(new object[]
                {
                    initiator.LabelShort,
                    recipient.LabelShort
                }), LetterType.BadNonUrgent, initiator, null);
            }
        }

        // Token: 0x06000C66 RID: 3174 RVA: 0x0003CE84 File Offset: 0x0003B084
        internal static float _RandomSelectionWeight(this InteractionWorker_Breakup _this, Pawn initiator, Pawn recipient)
        {
            if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                return 0f;
            }
            else if(initiator.story.traits.HasTrait(TraitDefOfPsychology.Codependent))
            {
                return 0f;
            }
            float num = Mathf.InverseLerp(100f, -100f, (float)initiator.relations.OpinionOf(recipient));
            float num2 = 1f;
            if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
            {
                num2 = 0.4f;
            }
            return 0.02f * num * num2;
        }
    }
}
