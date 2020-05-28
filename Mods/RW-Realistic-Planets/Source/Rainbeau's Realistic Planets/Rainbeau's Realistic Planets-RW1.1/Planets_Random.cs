using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace Planets_Code
{
	public static class Planets_Random
	{
		public static AxialTilt GetRandomAxialTilt()
		{
			float randTilt = Rand.Value;
			if (randTilt > 0.8) { return AxialTilt.VeryLow; }
			else if (randTilt > 0.6) { return AxialTilt.Low; }
			else if (randTilt > 0.4) { return AxialTilt.Normal; }
			else if (randTilt > 0.2) { return AxialTilt.High; }
			else { return AxialTilt.VeryHigh; }
		}

		public static WorldType GetRandomWorldType()
		{
			float randType = Rand.Value;
			if (randType > 0.86) { return WorldType.Waterworld; }
			else if (randType > 0.72) { return WorldType.Islands; }
			else if (randType > 0.58) { return WorldType.Earthlike; }
			else if (randType > 0.42) { return WorldType.Vanilla; }
			else if (randType > 0.28) { return WorldType.Dry; }
			else if (randType > 0.14) { return WorldType.VeryDry; }
			else { return WorldType.Barren; }
		}

		public static RainfallModifier GetRandomRainfallModifier()
		{
			float randRain = Rand.Value;
			if (randRain > 0.8) { return RainfallModifier.Little; }
			else if (randRain > 0.6) { return RainfallModifier.LittleBitLess; }
			else if (randRain > 0.4) { return RainfallModifier.Normal; }
			else if (randRain > 0.2) { return RainfallModifier.LittleBitMore; }
			else { return RainfallModifier.High; }
		}

		public static OverallTemperature GetRandomTemperature()
		{
			float randTemp = Rand.Value;
			if (randTemp > 0.86) { return OverallTemperature.VeryCold; }
			else if (randTemp > 0.72) { return OverallTemperature.Cold; }
			else if (randTemp > 0.58) { return OverallTemperature.LittleBitColder; }
			else if (randTemp > 0.42) { return OverallTemperature.Normal; }
			else if (randTemp > 0.28) { return OverallTemperature.LittleBitWarmer; }
			else if (randTemp > 0.14) { return OverallTemperature.Hot; }
			else { return OverallTemperature.VeryHot; }
		}

		public static OverallPopulation GetRandomPopulation()
		{
			float randPop = Rand.Value;
			if (randPop > 0.86) { return OverallPopulation.AlmostNone; }
			else if (randPop > 0.72) { return OverallPopulation.Little; }
			else if (randPop > 0.58) { return OverallPopulation.LittleBitLess; }
			else if (randPop > 0.42) { return OverallPopulation.Normal; }
			else if (randPop > 0.28) { return OverallPopulation.LittleBitMore; }
			else if (randPop > 0.14) { return OverallPopulation.High; }
			else { return OverallPopulation.VeryHigh; }
		}
	}
}
