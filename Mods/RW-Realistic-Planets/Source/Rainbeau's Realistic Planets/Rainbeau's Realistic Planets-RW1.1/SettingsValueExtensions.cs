using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planets_Code
{
	public static class SettingsValueExtensions
	{
		public static void ResetToDefault<T>(this SettingsValue<T> value)
		{
			value.CurrentValue = value.DefaultValue;
		}
	}
}
