using RimWorld;
using UnityEngine;
using Verse;

namespace ZhentarFix
{
	class GaydarFix : InteractionWorker_RomanceAttempt
	{
		[DetourMember]
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
			{
				return 0f;
			}
			float num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
			if (num < 0.25f)
			{
				return 0f;
			}
			int num2 = initiator.relations.OpinionOf(recipient);
			if (num2 < 5)
			{
				return 0f;
			}
			if (recipient.relations.OpinionOf(initiator) < 5)
			{
				return 0f;
			}
			float num3 = 1f;
			Pawn pawn = LovePartnerRelationUtility.ExistingMostLikedLovePartner(initiator, false);
			if (pawn != null)
			{
				float value = initiator.relations.OpinionOf(pawn);
				num3 = Mathf.InverseLerp(50f, -50f, value);
			}
			float num4 = (!initiator.story.traits.HasTrait(TraitDefOf.Gay)) ? ((initiator.gender != Gender.Female) ? 1f : 0.15f) : 1f;
			float num5 = Mathf.InverseLerp(0.25f, 1f, num);
			float num6 = Mathf.InverseLerp(5f, 100f, num2);
			float num7 = initiator.gender != recipient.gender || ( initiator.story.traits.HasTrait(TraitDefOf.Gay) && recipient.story.traits.HasTrait(TraitDefOf.Gay)) ? 1f : 0.15f;
			return 1.15f * num4 * num5 * num6 * num3 * num7;
		}
	}
}
