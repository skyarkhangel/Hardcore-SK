using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Gastronomy
{
	/// <summary>
	/// This is also added to VFE Core. Keeping it for now.
	/// </summary>
	internal static class _DrugPolicy_ExposeData_Patch
	{
		[HarmonyPatch(typeof(DrugPolicy), nameof(DrugPolicy.ExposeData))]
		public class ExposeData
		{
			[HarmonyPostfix]
			internal static void Prefix(DrugPolicy __instance, List<DrugPolicyEntry> ___entriesInt)
			{
				if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

				var allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
				foreach (var t in allDefsListForReading)
				{
					if (t.category == ThingCategory.Item && t.IsDrug && !___entriesInt.Exists(e=>e.drug == t))
					{
						DrugPolicyEntry drugPolicyEntry = new DrugPolicyEntry {drug = t, allowedForAddiction = true};
						___entriesInt.Add(drugPolicyEntry);
						Log.Message($"Added {t.label} to drug policy {__instance.label}.");
					}
				}
				___entriesInt.SortBy(e => e.drug.GetCompProperties<CompProperties_Drug>().listOrder);
			}
		}
	}
}
