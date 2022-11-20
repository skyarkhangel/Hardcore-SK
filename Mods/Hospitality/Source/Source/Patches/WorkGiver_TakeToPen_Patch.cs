using System.Collections.Generic;
using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
	/// <summary>
	/// Guests try to rope animals of visiting caravans of their own faction. Stop that.
	/// </summary>
	public class WorkGiver_TakeToPen_Patch
	{
		[HarmonyPatch(typeof(WorkGiver_TakeToPen), nameof(WorkGiver_TakeToPen.PotentialWorkThingsGlobal))]
		public class PotentialWorkThingsGlobal
		{
			public static bool Prefix(Pawn pawn, ref IEnumerable<Thing> __result)
			{
				if (!pawn.IsGuest()) return true;

				// Get player pawns instead of guest's faction pawns
				__result = pawn.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
				return false;

			}
		}
	}
}
