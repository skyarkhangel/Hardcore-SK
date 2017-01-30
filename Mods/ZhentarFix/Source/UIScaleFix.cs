using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace ZhentarFix.Source
{
	[StaticConstructorOnStartup]
	class UIScaleFix
	{
		static UIScaleFix()
		{
			var assembly = typeof(GameInitData).Assembly;
			var debugCellType = assembly.GetType("Verse.BlackScreenFixer");
			if (!DoDetour(debugCellType, typeof(UIScaleFix), "Start")) Log.Error("BlackScreenFixer detour failed");
		}

		private static bool DoDetour(Type rimworld, Type mod, string method)
		{
			MethodInfo RimWorld_A = rimworld.GetMethod(method, Detours.UniversalBindingFlags);
			MethodInfo ModTest_A = mod.GetMethod(method, Detours.UniversalBindingFlags);
			if (!Detours.TryDetourFromToInt(RimWorld_A, ModTest_A))
				return false;
			return true;
		}

		private void Start()
		{
			Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);
		}
	}
}
