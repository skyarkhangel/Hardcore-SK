using Harmony;
using RimWorld.Planet;
using Verse;

namespace Planets_Code
{
	[HarmonyPatch(typeof(OverallTemperatureUtility), "GetTemperatureCurve", null)]
	public static class OverallTemperatureUtility_GetTemperatureCurve {
		public static void Postfix(this OverallTemperature overallTemperature, ref SimpleCurve __result) {
			if (overallTemperature == OverallTemperature.VeryCold) {
				SimpleCurve Curve_VeryCold_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-50f, -90f), true },
					{ new CurvePoint(-40f, -75f), true },
					{ new CurvePoint(0f, -50f), true },
					{ new CurvePoint(20f, -43f), true },
					{ new CurvePoint(25f, -33f), true },
					{ new CurvePoint(30f, -23.5f), true },
					{ new CurvePoint(50f, -22f), true }
				};
				__result = Curve_VeryCold_Revised;
			}
			if (overallTemperature == OverallTemperature.Cold) {
				SimpleCurve Curve_Cold_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-50f, -78f), true },
					{ new CurvePoint(-25f, -48f), true },
					{ new CurvePoint(-20f, -33f), true },
					{ new CurvePoint(-13f, -23f), true },
					{ new CurvePoint(0f, -20f), true },
					{ new CurvePoint(30f, -11f), true },
					{ new CurvePoint(60f, 17f), true }
				};
				__result = Curve_Cold_Revised;
			}
			if (overallTemperature == OverallTemperature.LittleBitColder) {
				SimpleCurve Curve_LittleBitColder_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-20f, -25f), true },
					{ new CurvePoint(-15f, -18f), true },
					{ new CurvePoint(-5f, -16f), true },
					{ new CurvePoint(40f, 27f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_LittleBitColder_Revised;
			}
			if (overallTemperature == OverallTemperature.LittleBitWarmer) {
				SimpleCurve Curve_LittleBitWarmer_Revised = new SimpleCurve {
					{ new CurvePoint(-9999f, -9999f), true },
					{ new CurvePoint(-45f, -32f), true },
					{ new CurvePoint(40f, 53f), true },
					{ new CurvePoint(120f, 123f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_LittleBitWarmer_Revised;
			}
			if (overallTemperature == OverallTemperature.Hot) {
				SimpleCurve Curve_Hot_Revised = new SimpleCurve {
					{ new CurvePoint(-45f, -14f), true },
					{ new CurvePoint(-25f, -4f), true },
					{ new CurvePoint(-22f, 10f), true },
					{ new CurvePoint(-10f, 33f), true },
					{ new CurvePoint(40f, 65f), true },
					{ new CurvePoint(120f, 128f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_Hot_Revised;
			}
			if (overallTemperature == OverallTemperature.VeryHot) {
				SimpleCurve Curve_VeryHot_Revised = new SimpleCurve {
					{ new CurvePoint(-45f, 40f), true },
					{ new CurvePoint(0f, 55f), true },
					{ new CurvePoint(33f, 95f), true },
					{ new CurvePoint(40f, 103f), true },
					{ new CurvePoint(120f, 135f), true },
					{ new CurvePoint(9999f, 9999f), true }
				};
				__result = Curve_VeryHot_Revised;
			}
		}
	}
	
}
