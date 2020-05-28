using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	public class BiomeWorker_RRP_TemperateDesert : BiomeWorker
		{
			public BiomeWorker_RRP_TemperateDesert() { }
			public override float GetScore(Tile tile, int tileID)
			{
				if (tile.WaterCovered)
				{
					return -100f;
				}
				if (tile.rainfall >= 170f)
				{
					return 0f;
				}
				if (tile.temperature > 10f)
				{
					return 0f;
				}
				return tile.temperature + 0.0005f;
			}
		}
	
}
