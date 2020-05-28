using System;
using Verse;

namespace Planets_Code
{
	public static class TempCheck {
		public static float SeasonalTempChange(int tile) {
			return Math.Abs(Planets_TemperatureTuning.SeasonalTempVariationCurve.Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile)));
		}
	}
	
}
