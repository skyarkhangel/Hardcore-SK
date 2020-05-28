using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Planets_Code
{

	[HarmonyPatch(typeof(TileFinder), "RandomSettlementTileFor", null)]
	public static class TileFinder_RandomSettlementTileFor {
		[HarmonyPriority(Priority.High)]
		public static bool Prefix(Faction faction, ref int __result, bool mustBeAutoChoosable = false, Predicate<int> extraValidator = null) {
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				return true;
			}
			float minTemp = -500f;
			float maxTemp = 500f;
			if (faction != null) {
				if (!faction.IsPlayer) {
					if (faction.leader != null) {
						minTemp = faction.leader.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null);
						maxTemp = faction.leader.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax, null);
					}
				}
			}
			int num;
			for (int i = 0; i < 2500; i++) {
				if ((
				from _ in Enumerable.Range(0, 100)
				select Rand.Range(0, Find.WorldGrid.TilesCount)).TryRandomElementByWeight<int>((int x) => {
					Tile item = Find.WorldGrid[x];
					if (!item.biome.canBuildBase || !item.biome.implemented || item.hilliness == Hilliness.Impassable) {
						return 0f;
					}
					if (mustBeAutoChoosable && !item.biome.canAutoChoose) {
						return 0f;
					}
					if (extraValidator != null && !extraValidator(x)) {
						return 0f;
					}
					if (Controller.Settings.checkTemp.Equals(true)) {
						if (i < 1000) {
							if (item.temperature < (minTemp-45) || item.temperature > (maxTemp+45)) {
								return 0f;
							}
							if (item.temperature < (minTemp-36) || item.temperature > (maxTemp+36)) {
								if (Rand.Value > 0.1f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-28) || item.temperature > (maxTemp+28)) {
								if (Rand.Value > 0.2f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-21) || item.temperature > (maxTemp+21)) {
								if (Rand.Value > 0.3f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-15) || item.temperature > (maxTemp+15)) {
								if (Rand.Value > 0.4f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-10) || item.temperature > (maxTemp+10)) {
								if (Rand.Value > 0.5f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-6) || item.temperature > (maxTemp+6)) {
								if (Rand.Value > 0.6f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-3) || item.temperature > (maxTemp+3)) {
								if (Rand.Value > 0.7f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-1) || item.temperature > (maxTemp+1)) {
								if (Rand.Value > 0.8f) {
									return 0f;
								}
							}
							if (item.temperature < minTemp || item.temperature > maxTemp) {
								if (Rand.Value > 0.9f) {
									return 0f;
								}
							}
						}
						else if (i < 1500) {
							if (item.temperature < (minTemp-45) || item.temperature > (maxTemp+45)) {
								if (Rand.Value > 0.2f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-36) || item.temperature > (maxTemp+36)) {
								if (Rand.Value > 0.4f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-28) || item.temperature > (maxTemp+28)) {
								if (Rand.Value > 0.6f) {
									return 0f;
								}
							}
							if (item.temperature < (minTemp-21) || item.temperature > (maxTemp+21)) {
								if (Rand.Value > 0.8f) {
									return 0f;
								}
							}
						}
					}
					return item.biome.settlementSelectionWeight;
				}, out num)) {
					if (TileFinder.IsValidTileForNewSettlement(num, null)) {
						if (faction == null || faction.def.hidden.Equals(true) || faction.def.isPlayer.Equals(true)) {
							__result = num;
							return false;
						}
						else if (Controller.factionCenters.ContainsKey(faction)) { 
							float test = Find.WorldGrid.ApproxDistanceInTiles(Controller.factionCenters[faction],num);
							if (faction.def.defName == "Pirate" || faction.def.defName == "TribalRaiders") {
								if (test < (Controller.maxFactionSprawl*3)) {
									__result = num;
									return false;
								}
							}
							else {	
								if (test < Controller.maxFactionSprawl) { 
									__result = num;
									return false;
								}
							}
						}
						else {
							bool locationOK = true;
							foreach (KeyValuePair<Faction,int> factionCenter in Controller.factionCenters) {
								float test = Find.WorldGrid.ApproxDistanceInTiles(factionCenter.Value,num);
								if (test < Controller.minFactionSeparation) {
									locationOK = false;
								}
							}
							if (locationOK.Equals(true)) {
								__result = num;
								Controller.factionCenters.Add(faction, num);
								return false;
							}
						}
					}
				}
			}
			Log.Warning(string.Concat("Failed to find faction base tile for ", faction));
			if (Controller.failureCount.ContainsKey(faction)) {
				Controller.failureCount[faction]++;
				if (Controller.failureCount[faction] == 10) {
					Controller.failureCount.Remove(faction);
					if (Controller.factionCenters.ContainsKey(faction)) {
						Controller.factionCenters.Remove(faction);
						Log.Warning("  Relocating faction center.");
					}
				}
			}
			else {
				Log.Warning("  Retrying.");
				Controller.failureCount.Add(faction,1);
			}
			__result = 0;
			return false;
		}
	}
	
}
