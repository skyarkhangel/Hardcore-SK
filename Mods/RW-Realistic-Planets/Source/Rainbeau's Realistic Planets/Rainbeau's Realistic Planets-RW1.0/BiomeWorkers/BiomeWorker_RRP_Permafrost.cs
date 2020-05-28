using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	public class BiomeWorker_RRP_Permafrost : BiomeWorker
		{
			public BiomeWorker_RRP_Permafrost() { }
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
				float tempAdjust = TempCheck.SeasonalTempChange(tileID);
				if (tile.temperature + tempAdjust > 6f)
				{
					return 0f;
				}
				if (tile.temperature < -18f && tile.temperature > -24f)
				{
					return 100f;
				}
				return 0f;
			}
		}
	
}
