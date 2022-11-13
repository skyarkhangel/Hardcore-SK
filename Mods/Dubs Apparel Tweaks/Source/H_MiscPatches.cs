using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace QuickFast
{
	[StaticConstructorOnStartup]
	public static class H_MiscPatches
	{
		static H_MiscPatches()
		{

		}

		[HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath))]
		public static class H_StartPath
		{
			public static void Postfix(Pawn_PathFollower __instance)
			{
				if (__instance.pawn.Drafted)
				{
					return;
				}

				PatherCheck(__instance.pawn, __instance.nextCell, __instance.lastCell, true);
			}
		}

		[HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.TryEnterNextPathCell))]
		[StaticConstructorOnStartup]
		public static class H_TryEnterNextPathCell
		{
			public static void Postfix(Pawn_PathFollower __instance)
			{
				if (__instance.pawn.Drafted)
				{
					return;
				}

				//foreach (var ap in __instance.pawn.drawer.renderer.graphics.apparelGraphics)
				//{
				//    Log.Warning(ap.sourceApparel.def.apparel.LastLayer + "");
				//}

				PatherCheck(__instance.pawn, __instance.nextCell, __instance.lastCell, false);
			}
		}

		[HarmonyPatch(typeof(Pawn_DraftController))]
		[HarmonyPatch(nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
		public static class H_Drafted
		{
			public static void Postfix(Pawn_DraftController __instance)
			{
                if (__instance.pawn.apparel == null)
                {
					return;
                }
				if (__instance.draftedInt)
				{
					SwitchOutdoors(__instance.pawn);
				}
				else
				{
					if (Settings.DraftedHidingMode || __instance.pawn.Position.UsesOutdoorTemperature(__instance.pawn.MapHeld) is false)
					{
						SwitchIndoors(__instance.pawn);
					}
					else
					{
						SwitchOutdoors(__instance.pawn);
					}
				}
			}
		}


		[TweakValue("DubsApparelTweaks")]
		public static bool TrackApparelHiding = false;

		public static void SwitchIndoors(Pawn pawn)
		{
			if (UnityData.IsInMainThread is false) return;

			pawn.apparel.Notify_ApparelChanged();
			var graphics = pawn?.Drawer?.renderer?.graphics;
			if (graphics == null) return;

			if ((Settings.DraftedHidingMode && pawn.Drafted is false) || (Settings.DraftedHidingMode is false && !pawn.Position.UsesOutdoorTemperature(pawn.Map)))
			{
				bool Matchstick(ApparelGraphicRecord x)
				{
					foreach (var apparelLayerDef in x.sourceApparel.def.apparel.layers)
					{
						if (Settings.LayerVis.Contains(apparelLayerDef.defName))
						{
							if (Settings.hatfilter.Contains(x.sourceApparel.def))
							{
								return false;
							}
							return true;
						}
					}



					return false;
				}

				graphics.apparelGraphics.RemoveAll(Matchstick);

				if (TrackApparelHiding)
				{
					Log.Warning("Apparel hiding check for " + pawn.LabelCap);
				}

				//if (Settings.HideJackets is true)
				//{
				//	if (graphics.apparelGraphics.Any(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin))
				//	{
				//		graphics.apparelGraphics.RemoveAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell);
				//	}
				//}

				//if (Settings.HideEquipment is true)
				//{
				//	graphics.apparelGraphics.RemoveAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt);
				//}

				//if (Settings.HideHats is true)
				//{
				//	bool Match(ApparelGraphicRecord x)
				//	{
				//		return x.sourceApparel.def.apparel.layers.Any(z => z == Overhead) && !Settings.hatfilter.Contains(x.sourceApparel.def);
				//	}

				//	var hidden = graphics.apparelGraphics.RemoveAll(Match);
				//}
			}
		}

		public static void SwitchOutdoors(Pawn pawn)
		{
			if (UnityData.IsInMainThread is false) return;

			pawn.apparel.Notify_ApparelChanged();
			var graphics = pawn?.Drawer?.renderer?.graphics;
			if (graphics == null)
			{
				return;
			}


			//pawn.apparel.Notify_ApparelChanged();
			graphics.ClearCache();
			graphics.apparelGraphics.Clear();
			using (var enumerator = graphics.pawn.apparel.wornApparel.InnerListForReading.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (ApparelGraphicRecordGetter.TryGetGraphicApparel(enumerator.Current,
						graphics.pawn.story.bodyType, out var item))
					{
						graphics.apparelGraphics.Add(item);
					}
				}
			}
			//PortraitsCache.SetDirty(pawn);
		}

		public static void PatherCheck(Pawn pawn, IntVec3 nextCell, IntVec3 lastCell, bool startpath)
		{


			//   if (Settings.HideHats is false && Settings.HideJackets is false) return true;

			if (UnityData.IsInMainThread is false) return;

			// if (!pawn.RaceProps.Humanlike) return false;

			var map = pawn.MapHeld;

			if (map == null) return;

			if (pawn.NonHumanlikeOrWildMan()) return;

			if (!pawn.IsColonist) return;

			if (!nextCell.InBounds(map) || !lastCell.InBounds(map)) return;

			if (startpath)
			{
				if (Settings.DraftedHidingMode)
				{
					if (pawn.Drafted)
					{
						SwitchOutdoors(pawn);
					}
					else
					{
						SwitchIndoors(pawn);
					}
				}
				else
				{
					if (nextCell.UsesOutdoorTemperature(pawn.MapHeld))
					{
						SwitchOutdoors(pawn);
					}
					else
					{
						SwitchIndoors(pawn);
					}
				}

				return;
			}

			if (Settings.DraftedHidingMode)
			{
				return;
			}

			//if (nextCell.UsesOutdoorTemperature(pawn.MapHeld))
			//{
			//    SwitchOutdoors(pawn);
			//}
			//else
			//{
			//    SwitchIndoors(pawn);
			//}

			var last = lastCell.UsesOutdoorTemperature(map);
			var next = nextCell.UsesOutdoorTemperature(map);

			if (last && !next)
			{
				SwitchIndoors(pawn);
			}

			if (!last && next)
			{
				SwitchOutdoors(pawn);
			}
		}

		[HarmonyPatch(typeof(JobDriver_Wear), nameof(JobDriver_Wear.Notify_Starting))]
		public static class h_JobDriver_Wear
		{
			public static void Postfix(JobDriver_Wear __instance)
			{
				if (Settings.ChangeEquipSpeed)
				{
					if (Settings.FlatRate)
					{
						__instance.duration = Settings.EquipModTicks;
					}
					else
					{
						__instance.duration = (int)(__instance.duration * Settings.EquipModPC);
					}
				}
			}
		}


		[HarmonyPatch(typeof(JobDriver_RemoveApparel), nameof(JobDriver_RemoveApparel.Notify_Starting))]
		public static class h_JobDriver_RemoveApparel
		{
			public static void Postfix(JobDriver_Wear __instance)
			{
				if (Settings.ChangeEquipSpeed)
				{
					if (Settings.FlatRate)
					{
						__instance.duration = Settings.EquipModTicks;
					}
					else
					{
						__instance.duration = (int)(__instance.duration * Settings.EquipModPC);
					}
				}
			}
		}
	}
}