using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Planets_Code
{
	public class BiomeWorker_RRP_SeaIce : BiomeWorker
		{
			private ModuleBase cachedSeaIceAllowedNoise;
			private int cachedSeaIceAllowedNoiseForSeed;
			public BiomeWorker_RRP_SeaIce() { }
			private bool AllowedAt(int tile)
			{
				Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tile);
				Vector3 worldGrid = Find.WorldGrid.viewCenter;
				float single = Vector3.Angle(worldGrid, tileCenter);
				float worldGrid1 = Find.WorldGrid.viewAngle;
				float single1 = Mathf.Min(7.5f, worldGrid1 * 0.12f);
				float single2 = Mathf.InverseLerp(worldGrid1 - single1, worldGrid1, single);
				if (single2 <= 0f)
				{
					return true;
				}
				if (this.cachedSeaIceAllowedNoise == null || this.cachedSeaIceAllowedNoiseForSeed != Find.World.info.Seed)
				{
					this.cachedSeaIceAllowedNoise = new Perlin(0.017000000923872, 2, 0.5, 6, Find.World.info.Seed, QualityMode.Medium);
					this.cachedSeaIceAllowedNoiseForSeed = Find.World.info.Seed;
				}
				float headingFromTo = Find.WorldGrid.GetHeadingFromTo(worldGrid, tileCenter);
				float value = (float)this.cachedSeaIceAllowedNoise.GetValue((double)headingFromTo, 0, 0) * 0.5f + 0.5f;
				return single2 <= value;
			}
			public override float GetScore(Tile tile, int tileID)
			{
				if (!tile.WaterCovered)
				{
					return -100f;
				}
				if (!this.AllowedAt(tileID))
				{
					return -100f;
				}
				float tempAdjust = TempCheck.SeasonalTempChange(tileID);
				if (tile.temperature + tempAdjust > 3f)
				{
					return -100f;
				}
				return BiomeWorker_IceSheet.PermaIceScore(tile) - 23f;
			}
		}
	
}
