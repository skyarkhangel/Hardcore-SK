using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;

namespace Planets_Code.Presets
{
	public class WorldPreset
	{
		public string Name { get; }
		public WorldType WorldType { get; }
		public AxialTilt AxialTilt { get; }
		public RainfallModifier RainfallModifier { get; }
		public OverallTemperature Temperature { get; }
		public OverallPopulation Population { get; }

		public WorldPreset(string name, WorldType worldType, AxialTilt axialTilt, RainfallModifier rainfallModifier, OverallTemperature temperature, OverallPopulation population)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			WorldType = worldType;
			AxialTilt = axialTilt;
			RainfallModifier = rainfallModifier;
			Temperature = temperature;
			Population = population;
		}
	}
}
