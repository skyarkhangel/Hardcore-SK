using Harmony;
using Verse;

namespace Planets_Code
{
	[HarmonyPatch(typeof(GenTemperature), "SeasonalShiftAmplitudeAt", null)]
	public static class GenTemperature_SeasonalShiftAmplitudeAt {
		public static void Postfix(int tile, ref float __result) {
			if (Find.WorldGrid.LongLatOf(tile).y >= 0f) {
				__result = Planets_TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
				return;
			}
			__result = -Planets_TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
		}
	}
	
}
