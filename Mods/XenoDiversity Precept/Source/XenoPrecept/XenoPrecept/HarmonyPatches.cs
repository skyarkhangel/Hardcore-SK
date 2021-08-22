using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System;
using System.Reflection;
using Verse;

namespace XenoPrecept
{
	[StaticConstructorOnStartup]
	static class HarmonyPatches
	{
		private static readonly Type patchType = typeof(HarmonyPatches);
		// this static constructor runs to create a HarmonyInstance and install a patch.
		static HarmonyPatches()
		{
			var harmony = new Harmony("rimworld.martinbaste.xenoprecept");

			harmony.Patch(AccessTools.Method(typeof(PawnDiedOrDownedThoughtsUtility), name: "AppendThoughts_ForHumanlike"),
				new HarmonyMethod(patchType, nameof(AppendThoughts_ForHumanlike_Prefix)));

			/*harmony.Patch(AccessTools.Method(typeof(PawnDiedOrDownedThoughtsUtility), name: "AppendThoughts_ForHumanlike"),
				postfix: new HarmonyMethod(patchType, nameof(AppendThoughts_ForHumanlike_Prefix)));*/


			// find the FillTab method of the class RimWorld.ITab_Pawn_Character
			/*MethodInfo targetmethod = AccessTools.Method(typeof(RimWorld.PawnDiedOrDownedThoughtsUtility), "AppendThoughts_ForHumanlike");

            // find the static method to call before (i.e. Prefix) the targetmethod
            HarmonyMethod suffixmethod = new HarmonyMethod(typeof(XenoPrecept.HarmonyPatches).GetMethod("AppendThoughts_Relations_Suffix"));

            // patch the targetmethod, by calling prefixmethod before it runs, with no postfixmethod (i.e. null)
            harmony.Patch(targetmethod, postfix: suffixmethod);*/
		}
		private static void AppendThoughts_ForHumanlike_Prefix(Pawn victim, DamageInfo? dinfo, PawnDiedOrDownedThoughtsKind thoughtsKind, List<IndividualThoughtToAdd> outIndividualThoughts, List<ThoughtToAddToAll> outAllColonistsThoughts)
		{
			bool executionDamageFlag = dinfo != null && dinfo.Value.Def.execution;
			if (dinfo != null && dinfo.Value.Def.ExternalViolenceFor(victim) && dinfo.Value.Instigator != null && dinfo.Value.Instigator is Pawn)
			{
				Pawn instigator = (Pawn)dinfo.Value.Instigator;
				if (!instigator.Dead && instigator.needs.mood != null && instigator.story != null && instigator != victim)
				{
					if (instigator.def.label != victim.def.label && thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
					{
						Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.KilledHumanlikeXeno, instigator.Named(HistoryEventArgsNames.Doer)), true);
						/*outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledHumanlikeXeno, instigator, null, 1f, 1f));*/
					}
					/*if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died)
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.KilledHumanlikeBloodlust, instigator, null, 1f, 1f));
					}
					if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && victim.HostileTo(instigator) && victim.Faction != null && PawnUtility.IsFactionLeader(victim) && victim.Faction.HostileTo(instigator.Faction))
					{
						outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.DefeatedHostileFactionLeader, instigator, victim, 1f, 1f));
					}*/
				}
			}
			/*if (thoughtsKind == PawnDiedOrDownedThoughtsKind.Died && !executionDamageFlag)
            {
                foreach (Pawn pawn2 in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
                {
                    if (pawn2 != victim && pawn2.needs != null && pawn2.needs.mood != null && (pawn2.MentalStateDef != MentalStateDefOf.SocialFighting || ((Verse.AI.MentalState_SocialFighting)pawn2.MentalState).otherPawn != victim))
                    {
                        if (ThoughtUtility.Witnessed(pawn2, victim))
                        {
                            if (pawn2.def != victim.def)
                            {
                                outIndividualThoughts.Add(new IndividualThoughtToAdd(ThoughtDefOf.XenoDiversity_WitnessDeath_Abhorrent_Mood, pawn2, null, 1f, 1f));
                            }
                        }
                    }
                }
            }*/
		}
	}
}