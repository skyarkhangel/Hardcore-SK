using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	public class BiomeWorker_RRP_BorealForest : BiomeWorker
		{
			public BiomeWorker_RRP_BorealForest() { }
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
				if (tile.rainfall < 600f)
				{
					return 0f;
				}
				float tempAdjust = TempCheck.SeasonalTempChange(tileID);
				if (tile.temperature > 5f && (tile.temperature + tempAdjust >= 18f))
				{
					return 0f;
				}
				if (tile.temperature + tempAdjust < 12f)
				{
					return 0f;
				}
				return 15f;
			}
		}
	
}
