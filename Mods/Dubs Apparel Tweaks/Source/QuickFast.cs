using System;
using System.Reflection;
using System.Windows.Forms;

using HarmonyLib;

using RimWorld;

using UnityEngine;

using Verse;
using Verse.AI;

namespace QuickFast
{

	[StaticConstructorOnStartup]
	public static class bs
	{

		//[DebugAction("Memory", "Leak test", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
		//public static void LeakTesting()
		//{
		//	void bimleaker(Type allType)
		//	{
		//		if (allType.IsAbstract)
		//		{
		//			return;
		//		}

		//		if (allType.IsGenericType)
		//		{
		//			return;
		//		}


		//		int counts = 0;


		//		try
		//		{
		//			counts = Mesh.FindObjectsOfType(allType).Length;
		//		}
		//		catch (Exception e)
		//		{

		//		}
		//		;
		//		typepcounter.Add(allType, counts);
		//	}
		//	if (typepcounter == null)
		//	{
		//		typepcounter = new Dictionary<Type, int>();
		//		//foreach (var allType in GenTypes.AllTypes.ToList())
		//		//{
		//		//	try
		//		//	{
		//		//		bimleaker(allType);
		//		//	}
		//		//	catch (Exception e)
		//		//	{

		//		//	}
		//		//}

		//		foreach (var allActiveAssembly in ModLister.AllInstalledMods)
		//		{
		//			foreach (var VARIABLE in allActiveAssembly.source.)
		//			{

		//			}
		//			foreach (var type in allActiveAssembly.GetTypes())
		//			{
		//				bimleaker(type);
		//			}
		//		}

		//		bimleaker(typeof(string));
		//		bimleaker(typeof(Mesh));
		//		bimleaker(typeof(Texture));
		//		bimleaker(typeof(Material));
		//		bimleaker(typeof(Texture2D));
		//		Log.Warning(typepcounter.Count + " cached types");
		//	}

		//	var strang = "";


		//	foreach (var allType in typepcounter.Keys.ToList())
		//	{
		//		int counts = 0;

		//		try
		//		{
		//			counts = Mesh.FindObjectsOfType(allType).Length;
		//		}
		//		catch (Exception e)
		//		{

		//		}
		//		;

		//		if (counts > typepcounter[allType])
		//		{
		//			strang += $"\n+{counts - typepcounter[allType]} {allType.Name}";
		//		}

		//		try
		//		{
		//			typepcounter[allType] = counts;
		//		}
		//		catch (Exception e)
		//		{
		//			//typepcounter.Add(allType, counts);
		//		}

		//	}
		//	TextEditor te = new TextEditor();
		//	te.text = strang;
		//	te.OnFocus();
		//	te.Copy();
		//	Log.Warning(strang);

		//}

		//public static Dictionary<Type, int> typepcounter = null;



		//[HarmonyPatch(typeof(PawnCacheRenderer), nameof(PawnCacheRenderer.RenderPawn))]
		//public static class H_PawnCacheRendererRenderPawn
		//{
		//	private static void Prefix(Pawn pawn, ref bool renderBody, ref bool renderHeadgear, ref bool renderClothes, bool portrait)
		//	{
		//		if (portrait)
		//		{
		//			return;
		//		}

		//		if (pawn.CurJob == null)
		//		{
		//			return;
		//		}

		//		if (renderBody == false && !pawn.Awake() && Settings.HatsSleeping)
		//		{
		//			renderHeadgear = false;
		//		}

		//		if (renderBody == true && !pawn.Position.UsesOutdoorTemperature(pawn.Map))
		//		{
		//			//renderHeadgear = false;
		//		}
		//	}
		//}

		//[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.ShellFullyCoversHead))]
		//public static class H_ShellFullyCoversHead
		//{
		//	private static void Postfix(PawnRenderer __instance, PawnRenderFlags flags, ref bool __result)
		//	{
		//		if (Settings.hairfilter.Contains(__instance.pawn.story.hairDef))
		//		{
		//			__result = false;
		//		}
		//		else
		//		{
		//			__result = false;
		//		}
		//	}
		//}

		public static Harmony harmony;

		public static ApparelLayerDef MiddleHead;

		private static MethodInfo DrawHeadHair = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawHeadHair));
		private static MethodInfo g__DrawApparel = AccessTools.Method("Verse.PawnRenderer+<>c__DisplayClass54_0:<DrawHeadHair>g__DrawApparel|2");

		private static HarmonyMethod DrawHeadHairTranspiler = new HarmonyMethod(typeof(H_RenderPawn).GetMethod(nameof(H_RenderPawn.DrawHeadHairTranspiler)));
		private static HarmonyMethod DrawHeadgearTranspiler = new HarmonyMethod(typeof(H_RenderPawn).GetMethod(nameof(H_RenderPawn.DrawHeadgearTranspiler)));

		public static readonly string harmonyID = "Quickfast";


		public static MethodInfo CEdrawhair = null;

		public static MethodInfo CEGetHeadMesh = null;

		static bs()
		{
			harmony = new Harmony(harmonyID);
			harmony.PatchAll();

			MethodInfo b__0 = AccessTools.Method("RimWorld.Toils_LayDown+<>c__DisplayClass6_0:<LayDown>b__0");
			MethodInfo b__2 = AccessTools.Method("RimWorld.Toils_LayDown+<>c__DisplayClass6_0:<LayDown>b__2");

			if (b__0 != null && b__2 != null)
			{
				HarmonyMethod Prefix = new HarmonyMethod(typeof(bs).GetMethod(nameof(fix_0)));
				harmony.Patch(b__0, null, Prefix);

				Prefix = new HarmonyMethod(typeof(bs).GetMethod(nameof(fix_2)));
				harmony.Patch(b__2, null, Prefix);
			}
			else
			{
				Log.Warning("Couldn't find the LayDown toils, bad news for hat hiding while sleeping.");
			}

			if (Settings.ShowHairUnderHats)
			{
				ApplyTrans();
			}



			// fixes for mods

			MiddleHead = DefDatabase<ApparelLayerDef>.GetNamedSilentFail("MiddleHead");


			//CEdrawhair = AccessTools.Method("Harmony_PawnRenderer_DrawHeadHair:DrawHeadApparel");
			CEGetHeadMesh = AccessTools.Method("Harmony_PawnRenderer:GetHeadMesh");

			if (CEGetHeadMesh != null)
			{
				harmony.Patch(CEGetHeadMesh, null, new HarmonyMethod(typeof(bs).GetMethod(nameof(CE_GetHeadMesh))));
			}

			//if (CEdrawhair != null)
			//{
			//harmony.Patch(CEdrawhair, null, new HarmonyMethod(typeof(bs).GetMethod(nameof(killme))));
			//	}

			//trick har so it loops the cached visible gear rather than getting all worn apparel
			var meth = AccessTools.Method("AlienRace.HarmonyPatches:DrawAddons");
			if (meth != null)
			{
				// Log.Message("Dubs Apparel Tweaks found HAR");
				HarmonyMethod pre = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.Pre)));
				HarmonyMethod post = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.Post)));
				harmony.Patch(meth, pre, post);

				pre = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.WornApparelPrefix)));
				meth = AccessTools.Method(typeof(Pawn_ApparelTracker), "get_WornApparel");
				harmony.Patch(meth, pre);
			}
		}

		public static ApparelLayerDef Overhead => MiddleHead ?? ApparelLayerDefOf.Overhead;

		public static void ApplyTrans()
		{
			//if (CEdrawhair != null)
			//{
			//	Log.Warning("Apparel tweaks found Combat Extended, hair under hats will not work, use CE's built in hair under hats feature instead.");
			//	return;
			//}
			harmony.Patch(DrawHeadHair, transpiler: DrawHeadHairTranspiler);
			harmony.Patch(g__DrawApparel, transpiler: DrawHeadgearTranspiler);
			//Log.Warning("Applied transpiler to RenderPawnInternal to show hair under hats and rescale hats");
		}

		public static void RemoveTrans()
		{
			harmony.Unpatch(DrawHeadHair, HarmonyPatchType.Transpiler, harmonyID);
			harmony.Unpatch(g__DrawApparel, HarmonyPatchType.Transpiler, harmonyID);
		}

		public static Mesh MeshHead(Mesh originalMesh, float s)
		{
			var clonedMesh = new Mesh();

			clonedMesh.name = "clone";

			var trash = originalMesh.vertices;

			clonedMesh.vertices = originalMesh.vertices;
			clonedMesh.triangles = originalMesh.triangles;
			clonedMesh.normals = originalMesh.normals;
			clonedMesh.uv = originalMesh.uv;

			for (var index = 0; index < trash.Length; index++)
			{
				var vertex = trash[index];
				vertex.x *= s;
				//vertex.y *= s;
				vertex.z *= s;
				trash[index] = vertex;
			}

			clonedMesh.vertices = trash;
			clonedMesh.RecalculateNormals();
			clonedMesh.RecalculateBounds();

			return clonedMesh;
		}











		// CE BULLLSHIIIIT


		[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawHeadHair))]
		public static class H_DrawHeadHair
		{
			//private static bool Prepare()
			//{
			//	return true;

			//	if (AccessTools.Method("Harmony_PawnRenderer_DrawHeadHair:DrawHeadApparel") != null)
			//	{
			//		Log.Warning("Apparel tweaks detected CE - Applying hair drawing patch");
			//		return true;
			//	}

			//	if (Settings.AltHairRenderMode)
			//	{
			//		return true;
			//	}

			//	return false;
			//}


			private static void Prefix(PawnRenderer __instance, Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
			{
				if (!Settings.ShowHairUnderHats)
				{
					return;
				}

				if (!Settings.AltHairRenderMode && CEGetHeadMesh == null) return;

				bool flag11 = bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump);
				if (!flag11) return;

				if (!H_RenderPawn.ShouldRenderHair(__instance))
				{
					return;
				}

				if (__instance.ShellFullyCoversHead(flags))
				{
					return;
				}

				var vector = rootLoc + headOffset;
				vector.y += 0.0289575271f;
				vector.y += Settings.AltHairRenderLayer;
				var quat = Quaternion.AngleAxis(angle, Vector3.up);
				var mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
				var material4 = __instance.graphics.HairMatAt(headFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
				if (material4 != null)
				{
					GenDraw.DrawMeshNowOrLater(mesh4, vector, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
				}


			}
		}


		public static void CE_GetHeadMesh(PawnRenderFlags renderFlags, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics, ref Mesh __result)
		{
			if (__result == null)
			{
				__result = pawn.drawer.renderer.graphics.HairMeshSet.MeshAt(headFacing);
			}
			__result = H_RenderPawn.MeshScaler(pawn.drawer.renderer, __result);
		}

		//public static void killme(PawnRenderer renderer, ref bool hideHair)
		//{
		//	if (Settings.ShowHairUnderHats)
		//	{
		//		hideHair = false;// Settings.hairfilter.Contains(renderer.pawn.story.hairDef);
		//	}
		//}
		//













		public static void fix_0(object __instance)
		{
			if (!Settings.HatsSleeping)
			{
				return;
			}

			if (AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) is Toil toil)
			{
				toil?.actor?.apparel?.Notify_ApparelChanged();

				var bed = toil?.actor.CurrentBed();
				if (bed != null && toil.actor.RaceProps.Humanlike && !bed.def.building.bed_showSleeperBody)
				{
					toil.actor.Drawer.renderer.graphics.ClearCache();
					toil.actor.Drawer.renderer.graphics.apparelGraphics.Clear();
				}
			}
		}

		public static void fix_2(object __instance)
		{
			if (!Settings.HatsSleeping)
			{
				return;
			}

			if (AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) is Toil toil && toil.actor.RaceProps.Humanlike)
			{
				toil?.actor?.apparel?.Notify_ApparelChanged();
				toil.actor.Drawer.renderer.graphics.ResolveApparelGraphics();
				PortraitsCache.SetDirty(toil.actor);
			}
		}
	}
}
