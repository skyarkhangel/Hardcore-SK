using System;
using System.Collections.Generic;

using RimWorld;

using UnityEngine;

using Verse;

namespace QuickFast
{
	public class DubsApparelTweaks : Mod
	{
		public static Settings Settings;

		public DubsApparelTweaks(ModContentPack content) : base(content)
		{
			Settings = GetSettings<Settings>();
		}

		public override void DoSettingsWindowContents(Rect canvas)
		{
			Settings.DoWindowContents(canvas);
		}

		public override string SettingsCategory()
		{
			return "Dubs Apparel Tweaks";
		}
	}

	public class Settings : ModSettings
	{
		private static List<ThingDef> _hatfilter;
		private static List<HairDef> _hairfilter;

		public static float hairMeshScale = 1.1f;
		public static float EquipModPC = 0.2f;
		public static int EquipModTicks = 10;
		public static bool FlatRate = true;
		public static bool HatsSleeping = true;
		public static bool HideHats = true;
		public static bool HideEquipment = true;
		public static bool HideJackets = true;
		//public static bool EquipmentOnlyWhileDrafted;
		public static bool DraftedHidingMode;
		//public static bool JacketsOnlyWhileDrafted;
		public static bool ShowHairUnderHats;
		public static bool ChangeEquipSpeed = true;

		public static bool AltHairRenderMode = false;
		public static float AltHairRenderLayer = 0f;

		public static HashSet<string> LayerVis = new HashSet<string>();

		public class HairHatSet : IExposable
		{
			public string Hair;

			public HashSet<string> Hats = new HashSet<string>();

			public void ExposeData()
			{
				Scribe_Values.Look(ref Hair, "Hair");
				Scribe_Collections.Look(ref Hats, "Hats", LookMode.Value);
				if (Scribe.mode == LoadSaveMode.PostLoadInit)
				{
					if (Hats == null)
					{
						Hats = new HashSet<string>();
					}
				}
			}
		}

		public static List<HairHatSet> HatHairCombo = new List<HairHatSet>();


		public static List<string> HatDefToStrings = new List<string>();

		public static List<string> DefToStrings = new List<string>();
		private string buf;
		private Listing_Standard lis;

		public static List<ThingDef> hatfilter
		{
			get
			{
				if (_hatfilter == null)
				{
					_hatfilter = new List<ThingDef>();
					if (HatDefToStrings == null)
					{
						HatDefToStrings = new List<string>();
					}

					foreach (var HatDefToStrings in HatDefToStrings)
					{
						var foo = DefDatabase<ThingDef>.GetNamed(HatDefToStrings);
						if (foo != null)
						{
							_hatfilter.Add(foo);
						}
					}
				}

				return _hatfilter;
			}
		}

		public static List<HairDef> hairfilter
		{
			get
			{
				if (_hairfilter == null)
				{
					_hairfilter = new List<HairDef>();
					if (DefToStrings == null)
					{
						DefToStrings = new List<string>();
					}

					foreach (var defToString in DefToStrings)
					{
						var foo = DefDatabase<HairDef>.GetNamed(defToString);
						if (foo != null)
						{
							_hairfilter.Add(foo);
						}
					}
				}

				return _hairfilter;
			}
		}


		public void DoWindowContents(Rect canvas)
		{
			var nifta = canvas;
			lis = new Listing_Standard();
			lis.ColumnWidth = (nifta.width - 40f) / 2f;

			lis.Begin(canvas);

			Text.Font = GameFont.Medium;
			lis.Label("Apparel_equip_speed".Translate());
			Text.Font = GameFont.Small;
			lis.CheckboxLabeled("Change_equip_speeds".Translate(), ref ChangeEquipSpeed);
			if (ChangeEquipSpeed)
			{
				lis.CheckboxLabeled("Same_speed_for_all_apparel".Translate(), ref FlatRate);

				if (FlatRate)
				{
					lis.LabelDouble("Equip_speed_Ticks".Translate(), $"{EquipModTicks} ticks");
					lis.IntEntry(ref EquipModTicks, ref buf);
				}
				else
				{
					lis.LabelDouble("Equip_duration".Translate(), $"{EquipModPC.ToStringPercent()}");
					EquipModPC = lis.Slider(EquipModPC, 0f, 1f);
				}
			}

			lis.GapLine();
			Text.Font = GameFont.Medium;
			lis.Label("Apparel_visibility".Translate());
			Text.Font = GameFont.Small;

			lis.CheckboxLabeled("Hide_hats_when_sleeping".Translate(), ref HatsSleeping);

			lis.GapLine();

			GUI.color = Color.green;
			lis.Label("HatFiltersTip".Translate());
			GUI.color = Color.white;

			if (lis.RadioButton("IndoorHidingMode".Translate(), DraftedHidingMode is false, 10))
			{
				DraftedHidingMode = false;
			}

			if (lis.RadioButton("DraftedHidingMode".Translate(), DraftedHidingMode is true, 10))
			{
				DraftedHidingMode = true;
			}

			//	lis.CheckboxLabeled("Hide_hats_when_indoors".Translate(), ref HideHats);
			//lis.CheckboxLabeled("Hide_jackets_when_indoors".Translate(), ref HideJackets);
			//			lis.CheckboxLabeled("Hide_equipment_when_indoors".Translate(), ref HideEquipment);

			//	lis.GapLine(100);

			Scrollerball(lis, ref graoner1, nifta.height - lis.CurHeight, ref scrollPosition1, DefDatabase<ApparelLayerDef>.AllDefsListForReading);


			//lis.CheckboxLabeled("Hats_only_while_drafted".Translate(), ref DraftedHidingMode);
			//lis.CheckboxLabeled("Jackets_only_while_drafted".Translate(), ref JacketsOnlyWhileDrafted);
			//lis.CheckboxLabeled("Equipment_only_while_drafted".Translate(), ref EquipmentOnlyWhileDrafted);

			lis.NewColumn();
			Text.Font = GameFont.Medium;
			lis.Label("Hair_visibility".Translate());
			Text.Font = GameFont.Small;
			var jim = ShowHairUnderHats;
			lis.CheckboxLabeled("Show_hair_under_hats".Translate(), ref ShowHairUnderHats);
			if (jim != ShowHairUnderHats)
			{
				if (ShowHairUnderHats)
				{
					bs.ApplyTrans();
				}
				else
				{
					bs.RemoveTrans();
				}
			}

			//if (bs.CEdrawhair != null)
			//{
			//	GUI.color = Color.red;
			//	lis.Label("Combat Extended is loaded and has its own hair visibility system");
			//	GUI.color = Color.white;
			//}

			if (ShowHairUnderHats)
			{
				var gilf = AltHairRenderMode;
				lis.CheckboxLabeled("Alternate hair draw mode (for conflicts)", ref AltHairRenderMode);
				if (gilf != AltHairRenderMode)
				{
					hairScale_Changed();
				}

				lis.LabelDouble("Hair layer offset", $"{AltHairRenderLayer}");
				var tamwy = decimal.Round((decimal)lis.Slider(AltHairRenderLayer, -0.1f, 0.1f), 3);
				if (tamwy != (decimal)AltHairRenderLayer)
				{
					AltHairRenderLayer = (float)tamwy;
					hairScale_Changed();
				}


				lis.LabelDouble("HatScaling".Translate(), $"{hairMeshScale}");
				var tamw = decimal.Round((decimal)lis.Slider(hairMeshScale, 0.9f, 1.2f), 3);
				if (tamw != (decimal)hairMeshScale)
				{
					hairMeshScale = (float)tamw;
					hairScale_Changed();
				}

				if (lis.ButtonText("HairScaleReset".Translate()))
				{
					//  hairScaleNarrow = 1.4f;
					hairMeshScale = 1.1f;
					AltHairRenderLayer = 0;
					hairScale_Changed();
				}


				GUI.color = Color.green;
				lis.Label("HairFiltersTip".Translate());
				lis.Label("HatHairFiltersTip".Translate());
				GUI.color = Color.white;
				lis.GapLine();

				booger(lis, ref graoner2, nifta.height - lis.CurHeight, ref scrollPosition2, Settings.HatHairCombo);

			}


			lis.End();
		}


		public static float graoner1 = 50f;
		public Vector2 scrollPosition1;

		public static void booger(Listing_Standard listing, ref float groaner, float height, ref Vector2 scrolpos, List<HairHatSet> list)
		{
			var rect = listing.GetRect(height);
			rect.width = 300;
			Text.Font = GameFont.Small;

			var innyrek = rect;
			innyrek.width -= 16f;
			innyrek.height = groaner;

			Widgets.BeginScrollView(rect, ref scrolpos, innyrek);

			GUI.BeginGroup(innyrek);
			var lineHeight = Text.LineHeight;
			float y = 0;
			foreach (var t in list)
			{
				foreach (var tHat in t.Hats)
				{
					var rec = new Rect(0, y, innyrek.width, lineHeight);
					Widgets.DrawHighlightIfMouseover(rec);
					Widgets.Label(rec, $"[{t.Hair}] + {tHat}");

					y += lineHeight + 3f;
				}
			}

			GUI.EndGroup();
			groaner = y + 25f;

			Widgets.EndScrollView();

			listing.Gap(listing.verticalSpacing);
		}


		public static float graoner2 = 50f;
		public Vector2 scrollPosition2;

		public static void Scrollerball(Listing_Standard listing, ref float groaner, float height, ref Vector2 scrolpos, List<ApparelLayerDef> list)
		{
			var rect = listing.GetRect(height);
			rect.width = 300;
			Text.Font = GameFont.Small;

			var innyrek = rect;
			innyrek.width -= 16f;
			innyrek.height = groaner;

			Widgets.BeginScrollView(rect, ref scrolpos, innyrek);

			GUI.BeginGroup(innyrek);
			var lineHeight = Text.LineHeight;
			float y = 0;
			foreach (var t in list)
			{
				var biff = LayerVis.Contains(t.defName);
				var jiff = biff;
				var rec = new Rect(0, y, innyrek.width, lineHeight);
				Widgets.DrawHighlightIfMouseover(rec);
				Widgets.CheckboxLabeled(rec, t.LabelCap, ref biff);

				if (jiff != biff)
				{
					if (biff)
					{
						LayerVis.Add(t.defName);
					}
					else
					{
						LayerVis.Remove(t.defName);
					}
					hairScale_Changed();
				}

				y += lineHeight + 3f;
			}

			GUI.EndGroup();
			groaner = y + 25f;

			Widgets.EndScrollView();

			listing.Gap(listing.verticalSpacing);
		}



		public static void hairScale_Changed()
		{
			H_RenderPawn.scalers.Clear();
			if (Find.CurrentMap != null)
			{
				foreach (var p in Find.CurrentMap.mapPawns.FreeColonists)
				{
					p.apparel?.Notify_ApparelChanged();
					H_MiscPatches.PatherCheck(p, p.pather.nextCell, p.pather.lastCell, true);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref AltHairRenderMode, "AltHairRenderMode", false);
			Scribe_Values.Look(ref AltHairRenderLayer, "AltHairRenderLayer", 0f);

			Scribe_Values.Look(ref hairMeshScale, "hairMeshScale", 1.06f);
			Scribe_Values.Look(ref ChangeEquipSpeed, "ChangeEquipSpeed");
			Scribe_Values.Look(ref DraftedHidingMode, "DraftedHidingMode");
			Scribe_Values.Look(ref ShowHairUnderHats, "ShowHairUnderHats");
			Scribe_Values.Look(ref FlatRate, "FlatRate");
			Scribe_Values.Look(ref HideHats, "HatsIndoors");
			Scribe_Values.Look(ref HatsSleeping, "HatsSleeping");
			Scribe_Values.Look(ref EquipModPC, "EquipModPC", 0.2f);
			Scribe_Values.Look(ref EquipModTicks, "EquipModTicks", 10);
			Scribe_Values.Look(ref HideJackets, "HideJackets");
			Scribe_Values.Look(ref HideEquipment, "HideEquipment");
			Scribe_Collections.Look(ref DefToStrings, "hairFilter", LookMode.Value);
			Scribe_Collections.Look(ref HatDefToStrings, "hatFilter", LookMode.Value);
			Scribe_Collections.Look(ref LayerVis, "LayerVis", LookMode.Value);
			Scribe_Collections.Look(ref HatHairCombo, "HatHairCombo", LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (LayerVis == null)
				{
					LayerVis = new HashSet<string>();
				}
				if (HatHairCombo == null)
				{
					HatHairCombo = new List<HairHatSet>();
				}
			}

			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				HatHairCombo.RemoveAll(x => string.IsNullOrEmpty(x.Hair));
			}
		}
	}
}