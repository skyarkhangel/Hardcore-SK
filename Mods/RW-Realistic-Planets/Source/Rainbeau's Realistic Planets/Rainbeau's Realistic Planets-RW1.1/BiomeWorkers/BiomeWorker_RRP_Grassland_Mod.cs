using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	//
	// REPLACEMENT BIOMEWORKERS FOR OTHER MODS' BIOMES
	//

	public class BiomeWorker_RRP_Grassland_Mod : BiomeWorker
		{
			public BiomeWorker_RRP_Grassland_Mod() { }
			public override float GetScore(Tile tile, int tileID)
			{
				if (tile.WaterCovered)
				{
					return -100f;
				}
				if (tile.temperature < -10f)
				{
					return 0f;
				}
				if (tile.rainfall < 170f)
				{
					return 0f;
				}
				float tempAdjust = TempCheck.SeasonalTempChange(tileID);
				if (tile.temperature + tempAdjust < 6f)
				{
					return 0f;
				}
				if (tile.rainfall >= 600f && (tile.temperature + tempAdjust >= 12f))
				{
					return 0f;
				}
				if (tile.temperature > 0f && (tile.rainfall / tile.temperature < 28f))
				{
					return 0f;
				}
				if (tile.temperature < 0f && (tile.rainfall / -tile.temperature < 50f))
				{
					return 0f;
				}
				if (tile.temperature < 0f)
				{
					return -tile.temperature + 0.0003f;
				}
				return tile.temperature + 0.0003f;
			}
		}
	
}
