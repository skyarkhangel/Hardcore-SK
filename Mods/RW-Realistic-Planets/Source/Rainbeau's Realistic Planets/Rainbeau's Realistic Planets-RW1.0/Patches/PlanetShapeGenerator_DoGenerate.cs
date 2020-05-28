using Harmony;
using RimWorld.Planet;
using System;
using System.Reflection;

namespace Planets_Code
{
	//
	// PLANET GENERATION CHANGES
	//

	[HarmonyPatch(typeof(PlanetShapeGenerator), "DoGenerate", new Type[] { })]
	public static class PlanetShapeGenerator_DoGenerate {
		[HarmonyPriority(Priority.Low)]
		public static bool Prefix() {
			FieldInfo subdivisionsCount = typeof(PlanetShapeGenerator).GetField("subdivisionsCount", AccessTools.all);
			subdivisionsCount.SetValue(null, Planets_GameComponent.subcount);
			return true;
		}
	}
	
}
