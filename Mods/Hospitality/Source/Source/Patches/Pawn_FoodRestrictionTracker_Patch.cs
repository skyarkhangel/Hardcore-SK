using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;

namespace Hospitality.Patches
{
	public class Pawn_FoodRestrictionTracker_Patch
	{
		/// <summary>
		/// So the player can't access the guest's food restrictions via the health tab
		/// </summary>
		[HarmonyPatch(typeof(Pawn_FoodRestrictionTracker))]
		[HarmonyPatch(nameof(Pawn_FoodRestrictionTracker.Configurable), MethodType.Getter)]
		public class Configurable
		{
			public static bool Prefix(Pawn_FoodRestrictionTracker __instance, ref bool __result)
			{
				if (__instance.pawn.IsGuest())
				{
					__result = false;
					return false;
				}

				return true;
			}
		}
	}
}
