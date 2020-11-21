using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;

namespace RT_SolarFlareShield
{
	[HarmonyPatch(typeof(PowerNet))]
	[HarmonyPatch(nameof(PowerNet.PowerNetTick))]
	internal static class Patch_PowerNetTick
	{
		public static IEnumerable<CodeInstruction> Transpiler(
			MethodBase original,
			IEnumerable<CodeInstruction> instructions)
		{
			return instructions.MethodReplacer(
				AccessTools.PropertyGetter(typeof(GameConditionManager), nameof(GameConditionManager.ElectricityDisabled)),
				AccessTools.Method(typeof(Patch_PowerNetTick), nameof(Patch_PowerNetTick.ElectricityDisabled)));
		}

		public static bool ElectricityDisabled(this GameConditionManager instance)
		{
			return instance.ElectricityDisabled && !instance.ownerMap.GetShieldCoordinator().HasActiveShield();
		}
	}
}