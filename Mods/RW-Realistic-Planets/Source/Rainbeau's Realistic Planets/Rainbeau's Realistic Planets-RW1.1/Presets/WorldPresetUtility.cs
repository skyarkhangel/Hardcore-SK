using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;

namespace Planets_Code.Presets
{
	public static class WorldPresetUtility
	{
		public static IList<WorldPreset> WorldPresets
		{
			get;
		}

		private static WorldPreset WorldPreset_Vanilla()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Vanilla")
				.WorldType(WorldType.Vanilla)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.Normal)
				.Temperature(OverallTemperature.Normal)
				.Population(OverallPopulation.Normal)
				.Build();
		}

		private static WorldPreset WorldPreset_Desert()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Desert")
				.WorldType(WorldType.Dry)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.LittleBitLess)
				.Temperature(OverallTemperature.LittleBitWarmer)
				.Population(OverallPopulation.LittleBitLess)
				.Build();
		}

		private static WorldPreset WorldPreset_Frozen()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Frozen")
				.WorldType(WorldType.VeryDry)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.LittleBitLess)
				.Temperature(OverallTemperature.Cold)
				.Population(OverallPopulation.LittleBitLess)
				.Build();
		}

		private static WorldPreset WorldPreset_Earthlike()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Earthlike")
				.WorldType(WorldType.Earthlike)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.Normal)
				.Temperature(OverallTemperature.Normal)
				.Population(OverallPopulation.Normal)
				.Build();
		}

		private static WorldPreset WorldPreset_Forest()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Forest")
				.WorldType(WorldType.Vanilla)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.LittleBitMore)
				.Temperature(OverallTemperature.LittleBitColder)
				.Population(OverallPopulation.LittleBitMore)
				.Build();
		}

		private static WorldPreset WorldPreset_Iceball()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Iceball")
				.WorldType(WorldType.Vanilla)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.Normal)
				.Temperature(OverallTemperature.VeryCold)
				.Population(OverallPopulation.Little)
				.Build();
		}

		private static WorldPreset WorldPreset_Jungle()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Jungle")
				.WorldType(WorldType.Earthlike)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.LittleBitMore)
				.Temperature(OverallTemperature.LittleBitWarmer)
				.Population(OverallPopulation.High)
				.Build();
		}

		private static WorldPreset WorldPreset_Ocean()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Ocean")
				.WorldType(WorldType.Waterworld)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.Normal)
				.Temperature(OverallTemperature.Normal)
				.Population(OverallPopulation.AlmostNone)
				.Build();
		}

		private static WorldPreset WorldPreset_Custom()
		{
			return new WorldPresetBuilder()
				.Name("Planets.Custom")
				.WorldType(WorldType.Vanilla)
				.AxialTilt(AxialTilt.Normal)
				.RainfallModifier(RainfallModifier.Normal)
				.Temperature(OverallTemperature.Normal)
				.Population(OverallPopulation.Normal)
				.Build();
		}

		static WorldPresetUtility()
		{
			WorldPresets = new WorldPreset[]
			{
				WorldPreset_Vanilla(),
				WorldPreset_Desert(),
				WorldPreset_Frozen(),
				WorldPreset_Earthlike(),
				WorldPreset_Forest(),
				WorldPreset_Iceball(),
				WorldPreset_Jungle(),
				WorldPreset_Ocean(),
				WorldPreset_Custom()
			};
		}
	}
}
