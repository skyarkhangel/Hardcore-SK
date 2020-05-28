using System;

namespace Planets_Code
{
	public static class WorldTypeUtility
	{
		private static int cachedEnumValuesCount = -1;

		public static int EnumValuesCount
		{
			get
			{
				if (WorldTypeUtility.cachedEnumValuesCount < 0)
				{
					WorldTypeUtility.cachedEnumValuesCount = Enum.GetNames(typeof(WorldType)).Length;
				}
				return WorldTypeUtility.cachedEnumValuesCount;
			}
		}	
	}
}
