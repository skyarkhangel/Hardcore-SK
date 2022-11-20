using HarmonyLib;
using Hospitality.Utilities;
using Verse;
using Verse.AI;

namespace Hospitality.Patches
{
	public class PathFinder_Patch
	{
		/// <summary>
		/// So guests don't path outside of their area
		/// </summary>
		[HarmonyPatch(typeof(PathFinder), nameof(PathFinder.GetAllowedArea))]
		public class GetAllowedArea
		{
			[HarmonyPrefix]
			public static bool Replacement(Pawn pawn, ref Area __result)
			{
				if (!pawn.IsGuest()) return true;
				__result = pawn.GetGuestArea();
				return false;
			}
		}
	}
}
