using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Linq;
using Verse;

namespace Planets_Code.Factions
{
	[HarmonyPatch(typeof(FactionGenerator), "NewGeneratedFaction", new Type[] { typeof(FactionDef) })]
	public static class FactionGenerator_NewGeneratedFaction {
		[HarmonyPriority(Priority.High)]
		public static bool Prefix(FactionDef facDef, ref Faction __result) {
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				return true;
			}
			Faction faction = new Faction();
			faction.def = facDef;
			faction.loadID = Find.UniqueIDsManager.GetNextFactionID();
			faction.colorFromSpectrum = FactionGenerator.NewRandomColorFromSpectrum(faction);
			if (!facDef.isPlayer) {
				if (facDef.fixedName == null) {
					faction.Name = NameGenerator.GenerateName(facDef.factionNameMaker, 
					from fac in Find.FactionManager.AllFactionsVisible
					select fac.Name, false, null);
				}
				else {
					faction.Name = facDef.fixedName;
				}
			}
			faction.centralMelanin = Rand.Value;
			foreach (Faction allFactionsListForReading in Find.FactionManager.AllFactionsListForReading) {
				faction.TryMakeInitialRelationsWith(allFactionsListForReading);
			}
			faction.GenerateNewLeader();
			if (!facDef.hidden && !facDef.isPlayer) {
				Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				settlement.SetFaction(faction);
				settlement.Tile = TileFinder.RandomSettlementTileFor(faction, false, null);
				settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement, null);
				Find.WorldObjects.Add(settlement);
			}
			__result = faction;
			return false;
		}
	}
	
}
