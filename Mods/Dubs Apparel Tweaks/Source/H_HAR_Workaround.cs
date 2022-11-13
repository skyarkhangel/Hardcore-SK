using System.Collections.Generic;
using RimWorld;
using Verse;

namespace QuickFast
{
	public static class H_HAR_Workaround
	{
		public static bool run;

		public static void Pre()
		{
			run = true;
		}

		public static void Post()
		{
			run = false;
		}

		public static bool WornApparelPrefix(Pawn_ApparelTracker __instance, ref List<Apparel> __result)
		{
			if (!run) return true;

			var graphics = __instance.pawn?.Drawer?.renderer?.graphics;
			if (graphics != null && !graphics.apparelGraphics.NullOrEmpty())
			{
				__result = new List<Apparel>();
				var coo = graphics.apparelGraphics.Count;
				for (var i = 0; i < coo; i++)
				{
					var apparel = graphics.apparelGraphics[i];
					__result.Add(apparel.sourceApparel);
				}
			}

			return false;
		}
	}
}