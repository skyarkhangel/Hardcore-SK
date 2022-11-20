using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
	public class SmoothableWallUtility_Patch
	{
		/// <summary>
		/// So guests smoothing walls don't claim them for their faction
		/// </summary>
		[HarmonyPatch(typeof(SmoothableWallUtility), nameof(SmoothableWallUtility.SmoothWall))]
		public class SmoothWall
		{
			[HarmonyPostfix]
			public static void Postfix(Pawn smoother, Thing __result)
			{
				if (smoother.HostFaction != smoother.Faction)
				{
					__result.SetFaction(smoother.HostFaction);
				}
			}
		}
	}
}
