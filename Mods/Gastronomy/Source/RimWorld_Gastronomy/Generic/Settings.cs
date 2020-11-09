using HugsLib.Settings;
using Verse;

namespace Gastronomy
{
	internal class Settings
	{
		public static SettingHandle<bool> showAlertNoDedicatedWaiter;

		public Settings(ModSettingsPack settings)
		{
			showAlertNoDedicatedWaiter = settings.GetHandle("showAlertNoDedicatedWaiter", "ShowAlertNoDedicatedWaiter".Translate(), "ShowAlertNoDedicatedWaiterDesc".Translate(), true);
		}

		// Make sure that it still works when referenced settings are null!
		//private static SettingHandle.ValueIsValid WorkSkillLimits { get { return AtLeast(() => disableLimits?.Value != false ? 0 : 6); } }
		//private static SettingHandle.ValueIsValid MaxIncidentsPer3DaysLimitsMin { get { return AtLeast(() => 1); } }
		//private static SettingHandle.ValueIsValid GroupSizeLimitsMin { get { return Between(() => 1, () => maxGuestGroupSize?.Value ?? DefaultMaxGroupSize); } }
		//private static SettingHandle.ValueIsValid GroupSizeLimitsMax { get { return AtLeast(() => Mathf.Max(minGuestGroupSize?.Value ?? 0, disableLimits?.Value != false ? 1 : 8)); } }
		//
		//private static SettingHandle.ValueIsValid Between(Func<int> amountMin, Func<int> amountMax)
		//{
		//	return value => int.TryParse(value, out var actual) && actual >= amountMin() && actual <= amountMax();
		//}
		//
		//private static SettingHandle.ValueIsValid AtLeast(Func<int> amount)
		//{
		//	return value => int.TryParse(value, out var actual) && actual >= amount();
		//}
		//
		//private static SettingHandle.ValueIsValid AtMost(Func<int> amount)
		//{
		//	return value => int.TryParse(value, out var actual) && actual <= amount();
		//}
	}
}
