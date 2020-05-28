using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;
using Verse;

namespace Planets_Code.Patches
{
	/*
	Adds the following to the world inspect pane:
	- Stone types
	- Growing quadrums
	- Average disease frequency
	*/
	[HarmonyPatch(typeof(WorldInspectPane))]
	[HarmonyPatch("TileInspectString", MethodType.Getter)]
	public static class WorldInspectPane_TileInspectString
	{
		private static void Append_StoneTypes(this StringBuilder stringBuilder)
		{
			int selTileID = Find.WorldSelector.selectedTile;
			var selTile = Find.WorldGrid[selTileID];

			if (selTile.biome.canBuildBase)
			{
				string stoneTypesHereLabel = "StoneTypesHere".Translate();
				var stoneTypes = Find.World.NaturalRockTypesIn(selTileID);

				Func<ThingDef, string> stoneTypesLabelsFunc = (ThingDef x) => { return x.label; };

				stringBuilder.Append(stoneTypesHereLabel, stoneTypes.Select(stoneTypesLabelsFunc).ToCommaList(true).CapitalizeFirst(), translate: false);
			}
		}

		private static void Append_GrowingQuadrums(this StringBuilder stringBuilder)
		{
			int selTileID = Find.WorldSelector.selectedTile;
			stringBuilder.Append("OutdoorGrowingPeriod".Translate(), Zone_Growing.GrowingQuadrumsDescription(selTileID), translate: false);
		}

		private static void Append_DiseaseFrequency(this StringBuilder stringBuilder)
		{

			int selTileID = Find.WorldSelector.selectedTile;
			var selTile = Find.WorldGrid[selTileID];
			string key = "AverageDiseaseFrequency".Translate();
			string value = String.Format("{0} {1}", (60f / selTile.biome.diseaseMtbDays).ToString("F1"), "PerYear".Translate());

			stringBuilder.Append(key, value, translate: false);
		}

		[HarmonyPostfix]
		public static void Postfix(ref string __result)
		{
			var sb = new StringBuilder(__result);
			{
				sb.AppendLine();
				// Stone types
				if (Controller.Settings.showStoneTypes.CurrentValue)
				{
					sb.Append_StoneTypes();
				}
				// Growing quadrums description
				if (Controller.Settings.showGrowingPeriod.CurrentValue)
				{
					sb.Append_GrowingQuadrums();
				}
				// Average disease frequency
				if (Controller.Settings.showDiseaseFrequency.CurrentValue)
				{
					sb.Append_DiseaseFrequency();
				}
			}
			__result = sb.ToString();
		}
	}
}
