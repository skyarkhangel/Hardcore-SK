using Harmony;
using RimWorld.Planet;
using Verse;

namespace Planets_Code
{
	[HarmonyPatch(typeof(WorldGenStep_Terrain), "SetupElevationNoise", null)]
	public static class WorldGenStep_Terrain_SetupElevationNoise {
		public static bool Prefix(ref FloatRange ___ElevationRange) {
			if (Planets_GameComponent.worldType == WorldType.Waterworld) {
				___ElevationRange = new FloatRange(-2100f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Islands) {
				___ElevationRange = new FloatRange(-1500f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Earthlike) {
				___ElevationRange = new FloatRange(-1000f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Vanilla) {
				___ElevationRange = new FloatRange(-600f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.Dry) {
				___ElevationRange = new FloatRange(-300f, 5000f);
			}
			else if (Planets_GameComponent.worldType == WorldType.VeryDry) {
				___ElevationRange = new FloatRange(-100f, 5000f);
			}
			else {
				___ElevationRange = new FloatRange(0f, 5000f);
			}
			return true;
		}
	}
	
}
