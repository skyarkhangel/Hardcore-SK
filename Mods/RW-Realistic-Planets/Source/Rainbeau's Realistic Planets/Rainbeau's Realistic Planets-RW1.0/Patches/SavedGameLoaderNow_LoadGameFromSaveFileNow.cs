using Harmony;
using Verse;

namespace Planets_Code
{
	[HarmonyPatch(typeof(SavedGameLoaderNow), "LoadGameFromSaveFileNow", null)]
	public static class SavedGameLoaderNow_LoadGameFromSaveFileNow {
		public static void Postfix() {
			Planets_TemperatureTuning.SetSeasonalCurve();
		}
	}
	
}
