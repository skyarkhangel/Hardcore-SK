using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
	public class GenGuest_Patch
	{
		/// <summary>
		/// So guests creating slaves don't claim them for their faction
		/// </summary>
		[HarmonyPatch(typeof(GenGuest), nameof(GenGuest.EnslavePrisoner))]
		public class EnslavePrisoner
		{
			[HarmonyPostfix]
			public static void Postfix(Pawn warden, Pawn prisoner)
			{
				if (warden.IsGuest())
				{
					prisoner.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Slave);
				}
			}
		}
	}
}
