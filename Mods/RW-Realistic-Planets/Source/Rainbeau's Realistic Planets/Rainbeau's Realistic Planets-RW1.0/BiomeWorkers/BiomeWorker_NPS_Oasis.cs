using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Planets_Code
{
	public class BiomeWorker_NPS_Oasis : BiomeWorker
		{
			public BiomeWorker_NPS_Oasis() { }
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
				if (tile.temperature < 10)
				{
					return 0f;
				}
				if (tile.rainfall / tile.temperature >= 16f)
				{
					return 0f;
				}
				if (Rand.Value > .006)
				{
					return 0f;
				}
				return tile.temperature + 20;
			}
		}
	
}
