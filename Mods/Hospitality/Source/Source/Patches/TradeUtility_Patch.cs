using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
	/// <summary>
	/// So the player can sell food to guests
	/// </summary>
	public class TradeUtility_Patch
	{
		[HarmonyPatch(typeof (TradeUtility), nameof(TradeUtility.PlayerSellableNow))]
		public class PlayerSellableNow
		{
			public static void Postfix(ref bool __result, Thing t, ITrader trader)
			{
				if (!__result && t != null && trader.MayPurchaseThing(t))
				{
					__result = true;
				}
			}
		}
	}
}
