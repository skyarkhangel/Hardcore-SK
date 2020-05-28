using Harmony;
using RimWorld;
using System;
using Verse;

namespace Planets_Code
{
	//
	// FACTION GENERATION CHANGES
	//

	[HarmonyPatch(typeof(FactionGenerator), "GenerateFactionsIntoWorld", null)]
	public static class FactionGenerator_GenerateFactionsIntoWorld {
		[HarmonyPriority(Priority.High)]
		public static bool Prefix() {
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				return true;
			}
			Controller.factionCenters.Clear();
			int actualFactionCount = 0;
			foreach (FactionDef allDef in DefDatabase<FactionDef>.AllDefs) {
				if (!allDef.hidden) {
					actualFactionCount += allDef.requiredCountAtGameStart;
				}
			}
			Controller.minFactionSeparation = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*2);
			if (Controller.Settings.factionGrouping < 1) {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount);
			}
			else if (Controller.Settings.factionGrouping < 2) {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*1.5);
			}
			else if (Controller.Settings.factionGrouping < 3) {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*2.25);
			}
			else {
				Controller.maxFactionSprawl = Math.Sqrt(Find.WorldGrid.TilesCount)/(Math.Sqrt(actualFactionCount)*3);
			}
			return true;
		}
	}
	
}
