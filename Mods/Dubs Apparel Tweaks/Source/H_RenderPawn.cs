using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using HarmonyLib;

using RimWorld;

using UnityEngine;

using Verse;

namespace QuickFast
{
	public static class H_RenderPawn
	{
		public static Type displayClassType = AccessTools.TypeByName("Verse.PawnRenderer+<>c__DisplayClass54_0");
		public static System.Reflection.FieldInfo displayClassPrField = AccessTools.Field(displayClassType, "<>4__this");

		public static Dictionary<Mesh, Mesh> scalers = new Dictionary<Mesh, Mesh>();

		public static Mesh MeshScaler(PawnRenderer pr, Mesh mesh)
		{
			if (!Settings.ShowHairUnderHats || Math.Abs(Settings.hairMeshScale - 1f) < 0.001f)
			{
				return mesh;
			}
			//HairGotFiltered = false;
			if (!ShouldRenderHair(pr))
			{
				//HairGotFiltered = true;
				return mesh;
			}

			try
			{
				return scalers[mesh];
			}
			catch
			{
				scalers[mesh] = bs.MeshHead(mesh, Settings.hairMeshScale);
				return scalers[mesh];
			}
		}

		public static Mesh MeshScalerDC(object pawnRendererDisplayClass, Mesh mesh)
		{
			var pr = (PawnRenderer) displayClassPrField.GetValue(pawnRendererDisplayClass);
			return MeshScaler(pr, mesh);
		}

		public static bool InjectedHairToggly(PawnRenderer pr, bool HatDrawn)
		{
			if (Settings.AltHairRenderMode || bs.CEGetHeadMesh != null) return HatDrawn;

			if (HatDrawn is true)
			{
				return !ShouldRenderHair(pr);
			}

			return false;
		}

		public static bool ShouldRenderHair(PawnRenderer pr)
		{
			if (Settings.hairfilter.Contains(pr.pawn.story.hairDef))
			{
				return false;
			}

			var ag = pr.graphics.apparelGraphics;
			var hairset = Settings.HatHairCombo.FirstOrDefault(x => x.Hair == pr.pawn.story.hairDef.defName);
			if (hairset != null && hairset.Hats.Count > 0)
			{
				for (int k = 0; k < ag.Count; k++)
				{
					if (ag[k].sourceApparel.def.apparel.LastLayer == bs.Overhead)
					{
						//HatDrawn;
						if (hairset.Hats.Contains(ag[k].sourceApparel.def.defName))
						{
							return false;
						}
					}
				}
			}

			return true;
		}

		//public static Vector3 offset(Vector3 vec)
		//{
		//	if (HairGotFiltered)
		//	{
		//		HairGotFiltered = false;
		//		return vec;
		//	}

		//	vec.y += -0.0036f;
		//	return vec;
		//}

		public static bool oploc(this CodeInstruction obj, OpCode oc, int ind)
		{
			return obj.opcode == oc && obj.operand is LocalBuilder i && i.LocalIndex == ind;
		}

		public static bool loc(this CodeInstruction obj, int ind)
		{
			return obj.operand is LocalBuilder i && i.LocalIndex == ind;
		}

		public static bool op(this CodeInstruction obj, OpCode oc)
		{
			return obj.opcode == oc;
		}

		public static IEnumerable<CodeInstruction> DrawHeadgearTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var f_scaler = false;
			var ins_l = instructions.ToList();

			for (var i = 0; i < ins_l.Count - 1; i++)
			{
				var ins = ins_l[i];
				if (ins.op(OpCodes.Stloc_0) && ins_l[i + 1].op(OpCodes.Ldarg_1))
				{
					f_scaler = true;
				}
			}

			if (f_scaler is false)
			{
				Log.Warning("Failed inject m_MeshScaler - hair under hats wont work F");
			}

			if (!f_scaler)
			{
				foreach (var codeInstruction in ins_l) yield return codeInstruction;
			}
			else
			{
				f_scaler = false;
				for (var i = 0; i < ins_l.Count; i++)
				{
					var ins = ins_l[i];
					if (f_scaler is false && ins.op(OpCodes.Stloc_0) && ins_l[i + 1].op(OpCodes.Ldarg_1))
					{
						f_scaler = true;
						yield return ins;
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Ldloc_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(MeshScalerDC)));
						yield return new CodeInstruction(OpCodes.Stloc_0);
					}
					else
					{
						yield return ins;
					}
				}
			}
		}

		public static IEnumerable<CodeInstruction> DrawHeadHairTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var f_shouldrender = false;
			//var f_offset = false;

			var ins_l = instructions.ToList();

			//check its possible before bothering

			for (var i = 0; i < ins_l.Count - 1; i++)
			{
				var ins = ins_l[i];
				if (ins.op(OpCodes.Ldloc_2))
				{
					f_shouldrender = true;
				}
				else if (ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
				{
					//f_offset = true;
				}
			}

			if (f_shouldrender is false)
			{
				Log.Warning("Failed inject HairGotFiltered - hair under hats wont work F");
			}

			//if (f_offset is false)
			//{
			//	Log.Warning("Failed inject m_offset - hair under hats wont work F");
			//}

			if (!f_shouldrender)
			{
				foreach (var codeInstruction in ins_l) yield return codeInstruction;
			}
			else
			{
				f_shouldrender = false;
				//f_offset = false;

				for (var i = 0; i < ins_l.Count; i++)
				{
					var ins = ins_l[i];

					if (f_shouldrender is false && ins.op(OpCodes.Ldloc_2))
					{
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Ldloc_2);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(InjectedHairToggly)));
						// yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(H_RenderPawn), nameof(HairGotFiltered)));
						yield return new CodeInstruction(OpCodes.Stloc_2);
						yield return ins;
						f_shouldrender = true;
					}
					//else if (f_offset is false && ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
					//{
					//	f_offset = true;
					//	yield return ins;
					//	yield return new CodeInstruction(OpCodes.Ldloc_S, 13);
					//	yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(offset)));
					//	yield return new CodeInstruction(OpCodes.Stloc_S, 13);
					//}
					else
					{
						yield return ins;
					}
				}
			}
		}
	}
}
