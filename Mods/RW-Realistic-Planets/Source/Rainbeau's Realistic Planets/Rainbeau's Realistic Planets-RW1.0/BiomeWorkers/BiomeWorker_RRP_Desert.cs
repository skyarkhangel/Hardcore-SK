using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	public class BiomeWorker_RRP_Desert : BiomeWorker
		{
			public BiomeWorker_RRP_Desert() { }
			public override float GetScore(Tile tile, int tileID)
			{
				if (tile.WaterCovered)
				{
					return -100f;
				}
				if (tile.temperature < 30 && tile.rainfall >= 600f)
				{
					return 0f;
				}
				return tile.temperature + 0.0001f;
			}
		}
	
}
