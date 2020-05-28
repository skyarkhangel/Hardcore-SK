using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;

namespace Planets_Code.Presets
{
	public class WorldPresetBuilder
	{
		public string name;
		public WorldType? worldType;
		public AxialTilt? axialTilt;
		public RainfallModifier? rainfallModifier;
		public OverallTemperature? temperature;
		public OverallPopulation? population;

		public WorldPresetBuilder Name(string value)
		{
			this.name = value;
			return this;
		}

		public WorldPresetBuilder WorldType(WorldType value)
		{
			this.worldType = value;
			return this;
		}

		public WorldPresetBuilder AxialTilt(AxialTilt value)
		{
			this.axialTilt = value;
			return this;
		}

		public WorldPresetBuilder RainfallModifier(RainfallModifier value)
		{
			this.rainfallModifier = value;
			return this;
		}

		public WorldPresetBuilder Temperature(OverallTemperature value)
		{
			this.temperature = value;
			return this;
		}

		public WorldPresetBuilder Population(OverallPopulation value)
		{
			this.population = value;
			return this;
		}

		public WorldPreset Build()
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (!worldType.HasValue)
				throw new ArgumentNullException(nameof(worldType));
			if (!axialTilt.HasValue)
				throw new ArgumentNullException(nameof(axialTilt));
			if (!rainfallModifier.HasValue)
				throw new ArgumentNullException(nameof(rainfallModifier));
			if (!temperature.HasValue)
				throw new ArgumentNullException(nameof(temperature));
			if (!population.HasValue)
				throw new ArgumentNullException(nameof(population));
			return new WorldPreset(name, worldType.Value, axialTilt.Value, rainfallModifier.Value, temperature.Value, population.Value);
		}

		public WorldPresetBuilder()
		{

		}
	}
}
