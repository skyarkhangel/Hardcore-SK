using Verse;

namespace RT_SolarFlareShield
{
	public static class Extensions
	{
		public static MapComponent_ShieldCoordinator GetShieldCoordinator(this Map map)
		{
			MapComponent_ShieldCoordinator coordinator = map.GetComponent<MapComponent_ShieldCoordinator>();
			if (coordinator == null)
			{
				coordinator = new MapComponent_ShieldCoordinator(map);
				map.components.Add(coordinator);
			}
			return coordinator;
		}
	}
}