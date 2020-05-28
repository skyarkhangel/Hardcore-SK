using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Profile;
using Verse.Sound;

namespace Planets_Code
{
	public class Planets_CreateWorldParams : Page {
		private bool initialized;
		private string seedString;
		private float planetCoverage;
		private string worldPreset;
		private RainfallModifier rainfallMod;
		private OverallRainfall rainfall;
		private OverallTemperature temperature;
		private readonly static float[] PlanetCoverages;
		private readonly static string[] WorldPresets;
		public override string PageTitle {
			get { return "CreateWorld".Translate(); }
		}
		static Planets_CreateWorldParams() {
			Planets_CreateWorldParams.PlanetCoverages = new float[] { 
				0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 
				0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f
			};
			Planets_CreateWorldParams.WorldPresets = new string[] { 
				"Planets.Vanilla", 
				"Planets.Desert",
				"Planets.Frozen",
				"Planets.Earthlike",
				"Planets.Forest",
				"Planets.Iceball", 
				"Planets.Jungle",
				"Planets.Ocean", 
				"Planets.Custom"
			};
		}
		public Planets_CreateWorldParams() { }
		protected override bool CanDoNext() {
			if (!base.CanDoNext()) {
				return false;
			}
			if (Planets_GameComponent.worldType == WorldType.Waterworld) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.High; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.VeryHigh; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.VeryHigh; }
				else { this.rainfall = OverallRainfall.VeryHigh; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Islands) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.Normal; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.High; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.VeryHigh; }
				else { this.rainfall = OverallRainfall.VeryHigh; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Earthlike) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.LittleBitLess; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.Normal; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.High; }
				else { this.rainfall = OverallRainfall.VeryHigh; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Vanilla) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.Little; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.LittleBitLess; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.LittleBitMore; }
				else if (this.rainfallMod == RainfallModifier.High) { this.rainfall = OverallRainfall.High; }
				else { this.rainfall = OverallRainfall.Normal; }
			}
			else if (Planets_GameComponent.worldType == WorldType.Dry) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.Little; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.LittleBitLess; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.Normal; }
				else { this.rainfall = OverallRainfall.LittleBitMore; }
			}
			else if (Planets_GameComponent.worldType == WorldType.VeryDry) {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.Little; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.LittleBitLess; }
				else { this.rainfall = OverallRainfall.Normal; }
			}
			else {
				if (this.rainfallMod == RainfallModifier.Little) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitLess) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.Normal) { this.rainfall = OverallRainfall.AlmostNone; }
				else if (this.rainfallMod == RainfallModifier.LittleBitMore) { this.rainfall = OverallRainfall.Little; }
				else { this.rainfall = OverallRainfall.LittleBitLess; }
			}
			Planets_TemperatureTuning.SetSeasonalCurve();
			string generationString = "GeneratingWorld";
			if (Controller.Settings.randomPlanet.Equals(true)) {
				generationString = "Planets.GeneratingRandom";
				Controller.Settings.randomPlanet = false;
			}
			LongEventHandler.QueueLongEvent(() => {
				Find.GameInitData.ResetWorldRelatedMapInitData();
				Current.Game.World = WorldGenerator.GenerateWorld(this.planetCoverage, this.seedString, this.rainfall, this.temperature);
				LongEventHandler.ExecuteWhenFinished(() => {
					if (this.next != null) {
						Find.WindowStack.Add(this.next);
					}
					MemoryUtility.UnloadUnusedUnityAssets();
					Find.World.renderer.RegenerateAllLayersNow();
					this.Close(true);
				});
			}, generationString, true, null);
			return false;
		}
		public override void DoWindowContents(Rect rect) {
			base.DrawPageTitle(rect);
			GUI.BeginGroup(base.GetMainRect(rect, 0f, false));
			Text.Font = GameFont.Small;
			//
			// PLANET SEED
			//
			float single = 0f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "WorldSeed".Translate());
			Rect rect1 = new Rect(200f, single, 200f, 30f);
			this.seedString = Widgets.TextField(rect1, this.seedString);
			single += 40f;
			Rect rect2 = new Rect(200f, single, 200f, 30f);
			if (Widgets.ButtonText(rect2, "RandomizeSeed".Translate(), true, false, true)) {
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);
				this.seedString = GenText.RandomSeedString();
			}
			//
			// PLANET SIZE (IF "MY LITTLE PLANET" IS IN USE)
			//
			if (ModsConfig.ActiveModsInLoadOrder.Any(mod => mod.Name.Contains("My Little Planet"))) {
				single += 80f;
				Widgets.Label(new Rect(0f, single, 200f, 30f), "MLPWorldPlanetSize".Translate());
				Rect rect7 = new Rect(200f, single, 200f, 30f);
				Planets_GameComponent.subcount = Mathf.RoundToInt(Widgets.HorizontalSlider(rect7, Planets_GameComponent.subcount, 6f, 10f, true, null, "MLPWorldTiny".Translate(), "MLPWorldDefault".Translate(), 1f));
			}
			//
			// PLANET COVERAGE
			//
			single += 80f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetCoverage".Translate());
			Rect rect3 = new Rect(200f, single, 200f, 30f);
			if (Widgets.ButtonText(rect3, this.planetCoverage.ToStringPercent(), true, false, true)) {
				List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
				float[] singleArray = Planets_CreateWorldParams.PlanetCoverages;
				for (int i = 0; i < (int)singleArray.Length; i++) {
					float single1 = singleArray[i];
					string stringPercent = single1.ToStringPercent();
					FloatMenuOption floatMenuOption = new FloatMenuOption(stringPercent, () => {
						if (this.planetCoverage != single1) {
							this.planetCoverage = single1;
							if (this.planetCoverage < 0.15f) {
								Messages.Message("Planets.MessageLowPlanetCoverageWarning".Translate(), MessageTypeDefOf.CautionInput, false);
							}
							if (this.planetCoverage > 0.7f) {
								Messages.Message("MessageMaxPlanetCoveragePerformanceWarning".Translate(), MessageTypeDefOf.CautionInput, false);
							}
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
					floatMenuOptions.Add(floatMenuOption);
				}
				Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
			}
			TooltipHandler.TipRegion(new Rect(0f, single, rect3.xMax, rect3.height), "PlanetCoverageTip".Translate());
			//
			// WORLD TYPE PRESETS
			//
			single += 80f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.WorldPresets".Translate());
			Rect rect8 = new Rect(200f, single, 200f, 30f);
			if (Widgets.ButtonText(rect8, this.worldPreset.Translate(), true, false, true)) {
				List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
				string[] singleArray = Planets_CreateWorldParams.WorldPresets;
				for (int i = 0; i < (int)singleArray.Length; i++) {
					string single1 = singleArray[i];
					string single1Translated = single1.Translate();
					FloatMenuOption floatMenuOption = new FloatMenuOption(single1Translated, () => {
						if (this.worldPreset != single1) {
							this.worldPreset = single1;
							if (this.worldPreset == "Planets.Vanilla") {
								Planets_GameComponent.worldType = WorldType.Vanilla;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.Normal;
							}
							else if (this.worldPreset == "Planets.Desert") {
								Planets_GameComponent.worldType = WorldType.Dry;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitLess;
								this.temperature = OverallTemperature.LittleBitWarmer;
							}
							else if (this.worldPreset == "Planets.Frozen") {
								Planets_GameComponent.worldType = WorldType.VeryDry;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitLess;
								this.temperature = OverallTemperature.Cold;
							}
							else if (this.worldPreset == "Planets.Earthlike") {
								Planets_GameComponent.worldType = WorldType.Earthlike;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.Normal;
							}
							else if (this.worldPreset == "Planets.Forest") {
								Planets_GameComponent.worldType = WorldType.Vanilla;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitMore;
								this.temperature = OverallTemperature.LittleBitColder;
							}
							else if (this.worldPreset == "Planets.Iceball") {
								Planets_GameComponent.worldType = WorldType.Vanilla;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.VeryCold;
							}
							else if (this.worldPreset == "Planets.Jungle") {
								Planets_GameComponent.worldType = WorldType.Earthlike;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.LittleBitMore;
								this.temperature = OverallTemperature.LittleBitWarmer;
							}
							else if (this.worldPreset == "Planets.Ocean") {
								Planets_GameComponent.worldType = WorldType.Waterworld;
								Planets_GameComponent.axialTilt = AxialTilt.Normal;
								this.rainfallMod = RainfallModifier.Normal;
								this.temperature = OverallTemperature.Normal;
							}
							else { }
						}
					}, MenuOptionPriority.Default, null, null, 0f, null, null);
					floatMenuOptions.Add(floatMenuOption);
				}
				Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
			}
			TooltipHandler.TipRegion(new Rect(0f, single, rect8.xMax, rect8.height), "Planets.WorldPresetsTip".Translate());
			//
			// OCEAN SLIDER
			//
			single += 80f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.OceanType".Translate());
			Rect rect6 = new Rect(200f, single, 200f, 30f);
			WorldType worldTypeCheck = Planets_GameComponent.worldType;
			Planets_GameComponent.worldType = (WorldType)Mathf.RoundToInt(Widgets.HorizontalSlider(rect6, (float)Planets_GameComponent.worldType, 0f, (float)(WorldTypeUtility.EnumValuesCount - 1), true, "Planets.OceanType_Earthlike".Translate(), "Planets.OceanType_Waterworld".Translate(), "Planets.OceanType_Barren".Translate(), 1f));
			if (Planets_GameComponent.worldType != worldTypeCheck) { this.worldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect6.xMax, rect6.height), "Planets.OceanTypeTip".Translate());
			//
			// RAINFALL SLIDER
			//
			single += 40f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetRainfall".Translate());
			Rect rect4 = new Rect(200f, single, 200f, 30f);
			RainfallModifier rainfallModCheck = this.rainfallMod;
			this.rainfallMod = (RainfallModifier)Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, (float)this.rainfallMod, 0f, (float)(RainfallModifierUtility.EnumValuesCount - 1), true, "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(), "PlanetRainfall_High".Translate(), 1f));
			if (this.rainfallMod != rainfallModCheck) { this.worldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect4.xMax, rect4.height), "Planets.RainfallTip".Translate());
			//
			// TEMPERATURE SLIDER
			//
			single += 40f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetTemperature".Translate());
			Rect rect5 = new Rect(200f, single, 200f, 30f);
			OverallTemperature temperatureCheck = this.temperature;
			this.temperature = (OverallTemperature)Mathf.RoundToInt(Widgets.HorizontalSlider(rect5, (float)this.temperature, 0f, (float)(OverallTemperatureUtility.EnumValuesCount - 1), true, "PlanetTemperature_Normal".Translate(), "PlanetTemperature_Low".Translate(), "PlanetTemperature_High".Translate(), 1f));
			if (this.temperature != temperatureCheck) { this.worldPreset = "Planets.Custom"; }
			//
			// AXIAL TILT
			//
			single += 40f;
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.AxialTilt".Translate());
			Rect rect9 = new Rect(200f, single, 200f, 30f);
			AxialTilt axialTiltCheck = Planets_GameComponent.axialTilt;
			Planets_GameComponent.axialTilt = (AxialTilt)Mathf.RoundToInt(Widgets.HorizontalSlider(rect9, (float)Planets_GameComponent.axialTilt, 0f, (float)(AxialTiltUtility.EnumValuesCount - 1), true, "Planets.AxialTilt_Normal".Translate(), "Planets.AxialTilt_Low".Translate(), "Planets.AxialTilt_High".Translate(), 1f));
			if (Planets_GameComponent.axialTilt != axialTiltCheck) { this.worldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect9.xMax, rect9.height), "Planets.AxialTiltTip".Translate());
			GUI.EndGroup();
			//
			// BOTTOM BUTTONS
			//
			base.DoBottomButtons(rect, "WorldGenerate".Translate(), "Planets.Random".Translate(), new Action(this.Randomize), true, true);
		}
		public override void PostOpen() {
			base.PostOpen();
			TutorSystem.Notify_Event("PageStart-CreateWorldParams");
		}
		public override void PreOpen() {
			base.PreOpen();
			if (!this.initialized) {
				this.Reset();
				this.initialized = true;
			}
		}
		public void Reset() {
			this.seedString = GenText.RandomSeedString();
			this.planetCoverage = 0.3f;
			this.worldPreset = "Planets.Vanilla";
			Planets_GameComponent.axialTilt = AxialTilt.Normal;
			Planets_GameComponent.worldType = WorldType.Vanilla;
			Planets_GameComponent.subcount = 10;
			this.rainfallMod = RainfallModifier.Normal;
			this.temperature = OverallTemperature.Normal;
		}
		public void Randomize() {
			this.seedString = GenText.RandomSeedString();
			float randTilt = Rand.Value;
			if (randTilt > 0.8) { Planets_GameComponent.axialTilt = AxialTilt.VeryLow; }
			else if (randTilt > 0.6) { Planets_GameComponent.axialTilt = AxialTilt.Low; }
			else if (randTilt > 0.4) { Planets_GameComponent.axialTilt = AxialTilt.Normal; }
			else if (randTilt > 0.2) { Planets_GameComponent.axialTilt = AxialTilt.High; }
			else { Planets_GameComponent.axialTilt = AxialTilt.VeryHigh; }
			float randType = Rand.Value;
			if (randType > 0.86) { Planets_GameComponent.worldType = WorldType.Waterworld; }
			else if (randType > 0.72) { Planets_GameComponent.worldType = WorldType.Islands; }
			else if (randType > 0.58) { Planets_GameComponent.worldType = WorldType.Earthlike; }
			else if (randType > 0.42) { Planets_GameComponent.worldType = WorldType.Vanilla; }
			else if (randType > 0.28) { Planets_GameComponent.worldType = WorldType.Dry; }
			else if (randType > 0.14) { Planets_GameComponent.worldType = WorldType.VeryDry; }
			else { Planets_GameComponent.worldType = WorldType.Barren; }
			float randRain = Rand.Value;
			if (randRain > 0.8) { this.rainfallMod = RainfallModifier.Little; }
			else if (randRain > 0.6) { this.rainfallMod = RainfallModifier.LittleBitLess; }
			else if (randRain > 0.4) { this.rainfallMod = RainfallModifier.Normal; }
			else if (randRain > 0.2) { this.rainfallMod = RainfallModifier.LittleBitMore; }
			else { this.rainfallMod = RainfallModifier.High; }
			float randTemp = Rand.Value;
			if (randTemp > 0.86) { this.temperature = OverallTemperature.VeryCold; }
			else if (randTemp > 0.72) { this.temperature = OverallTemperature.Cold; }
			else if (randTemp > 0.58) { this.temperature = OverallTemperature.LittleBitColder; }
			else if (randTemp > 0.42) { this.temperature = OverallTemperature.Normal; }
			else if (randTemp > 0.28) { this.temperature = OverallTemperature.LittleBitWarmer; }
			else if (randTemp > 0.14) { this.temperature = OverallTemperature.Hot; }
			else { this.temperature = OverallTemperature.VeryHot; }
			Controller.Settings.randomPlanet = true;
			if (this.CanDoNext()) {
				this.DoNext();
			}
		}
	}
	
}
