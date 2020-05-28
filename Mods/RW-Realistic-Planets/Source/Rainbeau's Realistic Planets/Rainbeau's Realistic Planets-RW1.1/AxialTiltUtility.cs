using System;

namespace Planets_Code
{
	public static class AxialTiltUtility
	{
		private static int cachedEnumValuesCount = -1;

		public static int EnumValuesCount
		{
			get
			{
				if (AxialTiltUtility.cachedEnumValuesCount < 0)
				{
					AxialTiltUtility.cachedEnumValuesCount = Enum.GetNames(typeof(AxialTilt)).Length;
				}
				return AxialTiltUtility.cachedEnumValuesCount;
			}
		}
	}
	
}
