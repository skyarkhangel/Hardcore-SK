using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Planets_Code
{
	public class BiomeWorker_RRP_Oasis_Mod : BiomeWorker
		{
			public BiomeWorker_RRP_Oasis_Mod() { }
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
				float num = tile.rainfall / tile.temperature;
				if (num < 4 || num > 16)
				{
					return 0f;
				}
				if (num > 12)
				{
					if (Rand.Value > .008)
					{
						return 0f;
					}
					return tile.temperature + 5;
				}
				else if (num > 8)
				{
					if (Rand.Value > .004)
					{
						return 0f;
					}
					return tile.temperature + 5;
				}
				else
				{
					if (Rand.Value > .001)
					{
						return 0f;
					}
					return tile.temperature + 5;
				}
			}
		}
	
}
