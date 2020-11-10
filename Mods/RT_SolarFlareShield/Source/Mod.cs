using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace RT_SolarFlareShield
{
	internal class Mod : Verse.Mod
	{
		public Mod(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("io.github.ratysz.rt_solarflareshield");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[StaticConstructorOnStartup]
	public static class Resources
	{
		public static Material rotatorTexture
			= MaterialPool.MatFrom("RT_Buildings/Building_RTMagneticShield_Top", ShaderDatabase.Cutout);
	}
}