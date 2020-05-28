using System;
using RimWorld.Planet;

namespace Planets_Code
{
	public static class RainfallModifierUtility
	{
		private static int cachedEnumValuesCount = -1;

		public static int EnumValuesCount
		{
			get
			{
				if (RainfallModifierUtility.cachedEnumValuesCount < 0)
				{
					RainfallModifierUtility.cachedEnumValuesCount = Enum.GetNames(typeof(RainfallModifier)).Length;
				}
				return RainfallModifierUtility.cachedEnumValuesCount;
			}
		}	

		public static OverallRainfall GetModifiedRainfall(WorldType worldType, RainfallModifier modifier)
		{
			switch (worldType)
			{
				case WorldType.Waterworld:
					return GetModifiedRainfall_Waterworld(modifier);
				case WorldType.Islands:
					return GetModifiedRainfall_Islands(modifier);
				case WorldType.Earthlike:
					return GetModifiedRainfall_Earthlike(modifier);
				case WorldType.Vanilla:
					return GetModifiedRainfall_Vanilla(modifier);
				case WorldType.Dry:
					return GetModifiedRainfall_Dry(modifier);
				case WorldType.VeryDry:
					return GetModifiedRainfall_VeryDry(modifier);
				case WorldType.Barren:
					return GetModifiedRainfall_Barren(modifier);
				default:
					break;
			}
			throw new ArgumentOutOfRangeException(nameof(worldType));
		
		}

		private static OverallRainfall GetModifiedRainfall_Waterworld(RainfallModifier modifier)
		{
			if (modifier == RainfallModifier.Little) { return OverallRainfall.LittleBitMore; }
			else if (modifier == RainfallModifier.LittleBitLess) { return OverallRainfall.High; }
			else if (modifier == RainfallModifier.Normal) { return OverallRainfall.VeryHigh; }
			else if (modifier == RainfallModifier.LittleBitMore) { return OverallRainfall.VeryHigh; }
			else { return OverallRainfall.VeryHigh; }
		}

		private static OverallRainfall GetModifiedRainfall_Islands(RainfallModifier modifier)
		{
			if (modifier == RainfallModifier.Little) { return OverallRainfall.Normal; }
			else if (modifier == RainfallModifier.LittleBitLess) { return OverallRainfall.LittleBitMore; }
			else if (modifier == RainfallModifier.Normal) { return OverallRainfall.High; }
			else if (modifier == RainfallModifier.LittleBitMore) { return OverallRainfall.VeryHigh; }
			else { return OverallRainfall.VeryHigh; }
		}

		private static OverallRainfall GetModifiedRainfall_Earthlike(RainfallModifier modifier)
		{
			if (modifier == RainfallModifier.Little) { return OverallRainfall.LittleBitLess; }
			else if (modifier == RainfallModifier.LittleBitLess) { return OverallRainfall.Normal; }
			else if (modifier == RainfallModifier.Normal) { return OverallRainfall.LittleBitMore; }
			else if (modifier == RainfallModifier.LittleBitMore) { return OverallRainfall.High; }
			else { return OverallRainfall.VeryHigh; }
		}

		private static OverallRainfall GetModifiedRainfall_Vanilla(RainfallModifier modifier)
		{
			if (modifier == RainfallModifier.Little) { return OverallRainfall.Little; }
			else if (modifier == RainfallModifier.LittleBitLess) { return OverallRainfall.LittleBitLess; }
			else if (modifier == RainfallModifier.LittleBitMore) { return OverallRainfall.LittleBitMore; }
			else if (modifier == RainfallModifier.High) { return OverallRainfall.High; }
			else { return OverallRainfall.Normal; }
		}

		private static OverallRainfall GetModifiedRainfall_Dry(RainfallModifier modifier)
		{
			if (modifier == RainfallModifier.Little) { return OverallRainfall.AlmostNone; }
			else if (modifier == RainfallModifier.LittleBitLess) { return OverallRainfall.Little; }
			else if (modifier == RainfallModifier.Normal) { return OverallRainfall.LittleBitLess; }
			else if (modifier == RainfallModifier.LittleBitMore) { return OverallRainfall.Normal; }
			else { return OverallRainfall.LittleBitMore; }
		}

		private static OverallRainfall GetModifiedRainfall_VeryDry(RainfallModifier modifier)
		{
			if (modifier == RainfallModifier.Little) { return OverallRainfall.AlmostNone; }
			else if (modifier == RainfallModifier.LittleBitLess) { return OverallRainfall.AlmostNone; }
			else if (modifier == RainfallModifier.Normal) { return OverallRainfall.Little; }
			else if (modifier == RainfallModifier.LittleBitMore) { return OverallRainfall.LittleBitLess; }
			else { return OverallRainfall.Normal; }
		}

		private static OverallRainfall GetModifiedRainfall_Barren(RainfallModifier modifier)
		{
			if (modifier == RainfallModifier.Little) { return OverallRainfall.AlmostNone; }
			else if (modifier == RainfallModifier.LittleBitLess) { return OverallRainfall.AlmostNone; }
			else if (modifier == RainfallModifier.Normal) { return OverallRainfall.AlmostNone; }
			else if (modifier == RainfallModifier.LittleBitMore) { return OverallRainfall.Little; }
			else { return OverallRainfall.LittleBitLess; }
		}
	}
}
