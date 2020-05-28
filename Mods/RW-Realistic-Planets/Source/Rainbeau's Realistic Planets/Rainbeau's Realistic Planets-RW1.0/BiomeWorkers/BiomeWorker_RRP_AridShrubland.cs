using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	//
	// REPLACEMENT BIOMEWORKERS FOR VANILLA BIOMES
	//
	public class BiomeWorker_RRP_AridShrubland : BiomeWorker
		{
			public BiomeWorker_RRP_AridShrubland() { }
			public override float GetScore(Tile tile, int tileID)
			{
				if (tile.WaterCovered)
				{
					return -100f;
				}
				if (tile.rainfall < 170f)
				{
					return 0f;
				}
				if (tile.temperature > 0f && (tile.rainfall / tile.temperature < 16f))
				{
					return 0f;
				}
				return tile.temperature + 0.0002f;
			}
		}
	
}
