using Verse;

namespace Planets_Code
{
	public static class Planets_TemperatureTuning {
        public static SimpleCurve SeasonalTempVariationCurve;
		static Planets_TemperatureTuning() {
        	SetSeasonalCurve();
        }
        public static void SetSeasonalCurve() {
        	if (Planets_GameComponent.axialTilt == AxialTilt.VeryLow) {
				SimpleCurve veryLowTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 0.75f), true },
					{ new CurvePoint(0.1f, 1f), true },
					{ new CurvePoint(1f, 7f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = veryLowTilt;
        	}
        	else if (Planets_GameComponent.axialTilt == AxialTilt.Low) {
				SimpleCurve lowTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 1.5f), true },
					{ new CurvePoint(0.1f, 2f), true },
					{ new CurvePoint(1f, 14f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = lowTilt;
        	}
			else if (Planets_GameComponent.axialTilt == AxialTilt.Normal) {
				SimpleCurve normalTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 3f), true },
					{ new CurvePoint(0.1f, 4f), true },
					{ new CurvePoint(1f, 28f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = normalTilt;
        	}
        	else if (Planets_GameComponent.axialTilt == AxialTilt.High) {
				SimpleCurve highTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 4.5f), true },
					{ new CurvePoint(0.1f, 6f), true },
					{ new CurvePoint(1f, 42f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = highTilt;
        	}
        	else {
				SimpleCurve veryHighTilt = new SimpleCurve() {
					{ new CurvePoint(0f, 6f), true },
					{ new CurvePoint(0.1f, 8f), true },
					{ new CurvePoint(1f, 56f), true }
				};
				Planets_TemperatureTuning.SeasonalTempVariationCurve = veryHighTilt;
        	}
		}
	}
	
}
