using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	public class BiomeWorker_RRP_TropicalSwamp : BiomeWorker
		{
			public BiomeWorker_RRP_TropicalSwamp() { }
			public override float GetScore(Tile tile, int tileID)
			{
				if (tile.WaterCovered)
				{
					return -100f;
				}
				if (tile.temperature < 15f)
				{
					return 0f;
				}
				if (tile.rainfall < 2000f)
				{
					return 0f;
				}
				if (tile.swampiness < 0.5f)
				{
					return 0f;
				}
				float tempAdjust = TempCheck.SeasonalTempChange(tileID);
				if (tile.temperature - tempAdjust > 42f)
				{
					return 0f;
				}
				return 28f + (tile.temperature - 20f) * 1.5f + (tile.rainfall - 600f) / 165f + tile.swampiness * 3f;
			}
		}
	
}
