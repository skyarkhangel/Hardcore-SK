using System.Collections.Generic;
using Verse;

namespace RT_SolarFlareShield
{
	public class MapComponent_ShieldCoordinator : MapComponent
	{
		public MapComponent_ShieldCoordinator(Map map) : base(map)
		{
		}

		public List<CompRTSolarFlareShield> shields = new List<CompRTSolarFlareShield>();

		public bool HasActiveShield()
		{
			foreach (var shield in shields)
			{
				if (shield.Active)
				{
					return true;
				}
			}
			return false;
		}
	}
}