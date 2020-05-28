using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using Verse;
using UnityEngine;

namespace Planets_Code.Patches
{
	/*
	Add the following the the Planet tab description:
	- world preset
	- world type
	- rainfall
	- temperature
	- axial tilt
	- population

	Also increase height of tab to fit contents.
	*/
	[HarmonyPatch(typeof(WITab_Planet))]
	[HarmonyPatch("Desc", MethodType.Getter)]
	public static class WITab_Planet_Desc
	{
		/*
		Had to move increase of planet tab win size to HarmonyPrepare().
		This is because the static constructor of Planets_Initializer is called after
		the constructor of WITab_Planet, which is called in WorldInspectPane.
		*/
		[HarmonyPrepare]
		public static bool Prepare()
		{
			var winSizeField = AccessTools.Field(typeof(WITab_Planet), "WinSize");
			var winSizeValue = (Vector2)winSizeField.GetValue(obj: null);

			// Increase size of window to fit added contents of desc.
			if (winSizeValue.y <= 200f)
			{
				winSizeValue.y *= 1.5f;
				winSizeField.SetValue(obj: null, winSizeValue);
			}
			return true;
		}

		[HarmonyPostfix]
		public static void Postfix(ref string __result)
		{
			var sb = new StringBuilder(__result);
			{
				var preset = Planets_GameComponent.worldPreset;
				var worldType = Planets_Strings.GetWorldType();
				var rainfall = Planets_Strings.GetRainfall();
				var temperature = Planets_Strings.GetTemperature();
				var axialTilt = Planets_Strings.GetAxialTilt();
				var population = Planets_Strings.GetPopulation();

				sb.Append("Planets.WorldPresets", preset);
				sb.Append("Planets.OceanType", worldType);
				sb.Append("PlanetRainfall", rainfall);
				sb.Append("PlanetTemperature", temperature);
				sb.Append("Planets.AxialTilt", axialTilt);
				sb.Append("PlanetPopulation", population);
			}
			__result = sb.ToString();
		}
	}
}
