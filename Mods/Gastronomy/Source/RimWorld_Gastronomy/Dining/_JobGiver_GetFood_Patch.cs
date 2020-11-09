using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Gastronomy.Dining
{
	internal static class _JobGiver_GetFood_Patch
	{
		/// <summary>
		/// Replaces regular ingest job with dine job
		/// </summary>
		[HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
		public class TryGiveJob
		{
			[HarmonyPostfix]
			internal static void Postfix(Pawn pawn, ref Job __result)
			{
				if (__result == null) return;
				//Log.Message($"{pawn.NameShortColored} got job {__result.def.label} on {__result.targetA.Thing.Label}.");
				if (__result?.def == JobDefOf.Ingest && __result?.targetA.HasThing == true && __result?.targetA.Thing is DiningSpot)
				{
					//Log.Message($"{pawn.NameShortColored} is now dining instead of ingesting.");
					__result.def = DiningUtility.dineDef;
				}
			}
		}
	}
}
