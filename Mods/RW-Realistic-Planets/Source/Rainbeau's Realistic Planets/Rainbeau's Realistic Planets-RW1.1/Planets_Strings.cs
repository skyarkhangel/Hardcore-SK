using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace Planets_Code
{
	public static class Planets_Strings
	{
		// Values to display:
		// Planet name*
		// Seed*
		// Coverage*
		// 
		// World preset
		// Sea level
		// Rainfall
		// Temperature
		// Axial tilt
		// Population
		//
		// *already in vanilla

		// {KEY}: {VALUE}

		public static string GetWorldType()
		{
			switch (Planets_GameComponent.worldType)
			{
				case WorldType.Waterworld:
					return "Planets.SeaLevel_VeryHigh";
				case WorldType.Islands:
					return "Planets.SeaLevel_High";
				case WorldType.Earthlike:
					return "Planets.SeaLevel_SlightlyHigh";
				case WorldType.Vanilla:
					return "Planets.SeaLevel_Normal";
				case WorldType.Dry:
					return "Planets.SeaLevel_SlightlyLow";
				case WorldType.VeryDry:
					return "Planets.SeaLevel_Low";
				case WorldType.Barren:
					return "Planets.SeaLevel_VeryLow";
				default:
					break;
			}
			throw new ArgumentOutOfRangeException(nameof(Planets_GameComponent.worldType));
		}

		public static string GetRainfall()
		{
			switch (Find.World.info.overallRainfall)
			{
				case OverallRainfall.AlmostNone:
					return "Planets.Rainfall_VeryLow";
				case OverallRainfall.Little:
					return "Planets.Rainfall_Low";
				case OverallRainfall.LittleBitLess:
					return "Planets.Rainfall_SlightlyLow";
				case OverallRainfall.Normal:
					return "Planets.Rainfall_Normal";
				case OverallRainfall.LittleBitMore:
					return "Planets.Rainfall_SlightlyHigh";
				case OverallRainfall.High:
					return "Planets.Rainfall_High";
				case OverallRainfall.VeryHigh:
					return "Planets.Rainfall_VeryHigh";
				default:
					break;
			}
			throw new ArgumentOutOfRangeException(nameof(Find.World.info.overallRainfall));
		}

		public static string GetTemperature()
		{
			switch (Find.World.info.overallTemperature)
			{
				case OverallTemperature.VeryCold:
					return "Planets.Temperature_VeryLow";
				case OverallTemperature.Cold:
					return "Planets.Temperature_Low";
				case OverallTemperature.LittleBitColder:
					return "Planets.Temperature_SlightlyLow";
				case OverallTemperature.Normal:
					return "Planets.Temperature_Normal";
				case OverallTemperature.LittleBitWarmer:
					return "Planets.Temperature_SlightlyHigh";
				case OverallTemperature.Hot:
					return "Planets.Temperature_High";
				case OverallTemperature.VeryHot:
					return "Planets.Temperature_VeryHigh";
				default:
					break;
			}
			throw new ArgumentOutOfRangeException(nameof(Find.World.info.overallTemperature));
		}

		public static string GetAxialTilt()
		{
			switch (Planets_GameComponent.axialTilt)
			{
				case AxialTilt.VeryLow:
					return "Planets.AxialTilt_VeryLow";
				case AxialTilt.Low:
					return "Planets.AxialTilt_Low";
				case AxialTilt.Normal:
					return "Planets.AxialTilt_Normal";
				case AxialTilt.High:
					return "Planets.AxialTilt_High";
				case AxialTilt.VeryHigh:
					return "Planets.AxialTilt_VeryHigh";
				default:
					break;
			}
			throw new ArgumentOutOfRangeException(nameof(Planets_GameComponent.axialTilt));
		}

		public static string GetPopulation()
		{
			switch (Find.World.info.overallPopulation)
			{
				case OverallPopulation.AlmostNone:
					return "Planets.Population_VeryLow";
				case OverallPopulation.Little:
					return "Planets.Population_Low";
				case OverallPopulation.LittleBitLess:
					return "Planets.Population_SlightlyLow";
				case OverallPopulation.Normal:
					return "Planets.Population_Normal";
				case OverallPopulation.LittleBitMore:
					return "Planets.Population_SlightlyHigh";
				case OverallPopulation.High:
					return "Planets.Population_High";
				case OverallPopulation.VeryHigh:
					return "Planets.Population_VeryHigh";
				default:
					break;
			}
			throw new ArgumentOutOfRangeException(nameof(Find.World.info.overallPopulation));
		}
			
		public static void Append(this StringBuilder stringBuilder, string key, string value, bool translate = true)
		{
			if (key == null)
				throw new ArgumentNullException(nameof(key));
			if (value == null)
				throw new ArgumentNullException(nameof(value));

			if (translate)
			{
				key = key.Translate();
				value = value.Translate();
			}
			stringBuilder.Append(key);
			stringBuilder.Append(": ");
			stringBuilder.Append(value);
			stringBuilder.Append("\n");
		}
	}
}
