using System;

namespace Planets_Code
{
	public static class RainfallModifierUtility {
		private static int cachedEnumValuesCount = -1;
		public static int EnumValuesCount {
			get {
				if (RainfallModifierUtility.cachedEnumValuesCount < 0) {
					RainfallModifierUtility.cachedEnumValuesCount = Enum.GetNames(typeof(RainfallModifier)).Length;
				}
				return RainfallModifierUtility.cachedEnumValuesCount;
			}
		}	
	}
	
}
