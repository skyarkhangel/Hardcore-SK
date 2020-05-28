using RimWorld;
using RimWorld.Planet;

namespace Planets_Code
{
	public class BiomeWorker_RRP_IceSheet : BiomeWorker
		{
			public override float GetScore(Tile tile, int tileID)
			{
				if (tile.WaterCovered)
				{
					return -100f;
				}
				float tempAdjust = TempCheck.SeasonalTempChange(tileID);
				if (tile.temperature + tempAdjust > 3f)
				{
					return 0f;
				}
				return BiomeWorker_IceSheet.PermaIceScore(tile);
			}
		}
	
}
