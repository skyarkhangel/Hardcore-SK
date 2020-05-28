using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Profile;
using Verse.Sound;
using Planets_Code.Presets;

namespace Planets_Code
{
	public class Planets_CreateWorldParams : Page
	{
		private bool initialized;
		private string seedString;
		private float planetCoverage;
		private string CurrentWorldPreset
		{
			get
			{
				return Planets_GameComponent.worldPreset;
			}
			set
			{
				Planets_GameComponent.worldPreset = value;
			}
		}
		private RainfallModifier rainfallMod;
		private OverallRainfall rainfall;
		private OverallTemperature temperature;

		private OverallPopulation population;

		private readonly static float[] PlanetCoverages;
		private readonly static WorldPreset[] WorldPresets;
		private readonly static string[] WorldPresetStrings;

		public override string PageTitle
		{
			get { return "CreateWorld".Translate(); }
		}

		static Planets_CreateWorldParams()
		{
			Planets_CreateWorldParams.PlanetCoverages = new float[] { 
				0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f, 
				0.55f, 0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 0.95f, 1f
			};
			WorldPresets = WorldPresetUtility.WorldPresets.ToArray();

			WorldPresetStrings = new string[WorldPresets.Length];
			for (int i = 0; i < WorldPresets.Length; i++)
			{
				var worldPreset = WorldPresets[i];
				WorldPresetStrings[i] = worldPreset.Name;
			}
		}

		public Planets_CreateWorldParams()
		{
		}

		protected override bool CanDoNext()
		{
			if (!base.CanDoNext())
			{
				return false;
			}
			this.rainfall = RainfallModifierUtility.GetModifiedRainfall(Planets_GameComponent.worldType, this.rainfallMod);
			Planets_TemperatureTuning.SetSeasonalCurve();
			string generationString = "GeneratingWorld";
			if (Controller.Settings.randomPlanet.Equals(true)) {
				generationString = "Planets.GeneratingRandom";
				Controller.Settings.randomPlanet = false;
			}
			LongEventHandler.QueueLongEvent(() => {
				Find.GameInitData.ResetWorldRelatedMapInitData();

				Current.Game.World = WorldGenerator.GenerateWorld(this.planetCoverage, this.seedString, this.rainfall, this.temperature, this.population);
				LongEventHandler.ExecuteWhenFinished(() => {
					if (this.next != null) {
						Find.WindowStack.Add(this.next);
					}
					MemoryUtility.UnloadUnusedUnityAssets();
					Find.World.renderer.RegenerateAllLayersNow();
					this.Close(true);
				});
			}, generationString, doAsynchronously: true, exceptionHandler: null);
			return false;
		}

		public override void DoWindowContents(Rect rect)
		{
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
			DoWorldPresetsSelection(single);


			{
				single += 80f;
				DoSeaLevelSlider(single);

				single += 40f;
				DoRainfallSlider(single);

				single += 40f;
				DoTemperatureSlider(single);

				single += 40f;
				DoAxialTiltSlider(single);

				// Faction Control will override population, so disable this slider if Faction Control is enabled
				if (Controller.FactionControlSettingsMI == null)
				{
					single += 40f;
					DoPopulationSlider(single);
				}
			}
			GUI.EndGroup();

			//
			// BOTTOM BUTTONS
			//
			// Add Faction Control button
			if (Controller.FactionControlSettingsMI != null)
			{
				Text.Font = GameFont.Small;
				if (Widgets.ButtonText(new Rect(0, rect.yMax - 78f, 150, 32), "RFC.FactionControlName".Translate()))
				{
					Controller.FactionControlSettingsMI.Invoke(null, null);
				}
			}
			base.DoBottomButtons(rect, "WorldGenerate".Translate(), "Planets.Random".Translate(), new Action(this.Randomize), true, true);
		}

		private void ApplyWorldPreset(WorldPreset worldPreset)
		{
			this.CurrentWorldPreset = worldPreset.Name;

			if (this.CurrentWorldPreset != "Planets.Custom")
			{
				Planets_GameComponent.worldType = worldPreset.WorldType;
				Planets_GameComponent.axialTilt = worldPreset.AxialTilt;
				this.rainfallMod = worldPreset.RainfallModifier;
				this.temperature = worldPreset.Temperature;
				this.population = worldPreset.Population;
			}
		}

		private void DoWorldPresetsSelection(float single)
		{
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.WorldPresets".Translate());
			Rect rect8 = new Rect(200f, single, 200f, 30f);
			if (Widgets.ButtonText(rect8, this.CurrentWorldPreset.Translate(), true, false, true))
			{
				List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
				string[] singleArray = WorldPresetStrings;
				for (int i = 0; i < (int)singleArray.Length; i++)
				{
					string single1 = singleArray[i];
					string single1Translated = single1.Translate();
					var worldPreset = WorldPresets[i];
					FloatMenuOption floatMenuOption = new FloatMenuOption(single1Translated, () =>
					{
						if (this.CurrentWorldPreset != single1)
						{
							ApplyWorldPreset(worldPreset);
						}
					}, MenuOptionPriority.Default, mouseoverGuiAction: null, revalidateClickTarget: null, extraPartWidth: 0f, extraPartOnGUI: null, revalidateWorldClickTarget: null);
					floatMenuOptions.Add(floatMenuOption);
				}
				Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
			}
			TooltipHandler.TipRegion(new Rect(0f, single, rect8.xMax, rect8.height), "Planets.WorldPresetsTip".Translate());
		}

		private void DoSeaLevelSlider(float single)
		{
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.OceanType".Translate());
			Rect rect = new Rect(200f, single, 200f, 30f);
			WorldType worldTypeCheck = Planets_GameComponent.worldType;

			var lowString = "Planets.SeaLevel_Low".Translate();
			var normalString = "Planets.SeaLevel_Normal".Translate();
			var highString = "Planets.SeaLevel_High".Translate();

			float slider = Widgets.HorizontalSlider(rect, (float)Planets_GameComponent.worldType, WorldTypeUtility.EnumValuesCount - 1, 0f, true, normalString, lowString, highString, 1f);
			Planets_GameComponent.worldType = (WorldType)Mathf.RoundToInt(slider);
			if (Planets_GameComponent.worldType != worldTypeCheck)
			{
				this.CurrentWorldPreset = "Planets.Custom";
			}
			TooltipHandler.TipRegion(new Rect(0f, single, rect.xMax, rect.height), "Planets.OceanTypeTip".Translate());
		}

		private void DoRainfallSlider(float single)
		{
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetRainfall".Translate());
			Rect rect = new Rect(200f, single, 200f, 30f);
			RainfallModifier rainfallModCheck = this.rainfallMod;
			this.rainfallMod = (RainfallModifier)Mathf.RoundToInt(Widgets.HorizontalSlider(rect, (float)this.rainfallMod, 0f, (float)(RainfallModifierUtility.EnumValuesCount - 1), true, "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(), "PlanetRainfall_High".Translate(), 1f));
			if (this.rainfallMod != rainfallModCheck) { this.CurrentWorldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect.xMax, rect.height), "Planets.RainfallTip".Translate());
		}

		private void DoTemperatureSlider(float single)
		{
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetTemperature".Translate());
			Rect rect = new Rect(200f, single, 200f, 30f);
			OverallTemperature temperatureCheck = this.temperature;
			this.temperature = (OverallTemperature)Mathf.RoundToInt(Widgets.HorizontalSlider(rect, (float)this.temperature, 0f, (float)(OverallTemperatureUtility.EnumValuesCount - 1), true, "PlanetTemperature_Normal".Translate(), "PlanetTemperature_Low".Translate(), "PlanetTemperature_High".Translate(), 1f));
			if (this.temperature != temperatureCheck) { this.CurrentWorldPreset = "Planets.Custom"; }
		}

		private void DoAxialTiltSlider(float single)
		{
			Widgets.Label(new Rect(0f, single, 200f, 30f), "Planets.AxialTilt".Translate());
			Rect rect = new Rect(200f, single, 200f, 30f);
			AxialTilt axialTiltCheck = Planets_GameComponent.axialTilt;
			Planets_GameComponent.axialTilt = (AxialTilt)Mathf.RoundToInt(Widgets.HorizontalSlider(rect, (float)Planets_GameComponent.axialTilt, 0f, (float)(AxialTiltUtility.EnumValuesCount - 1), true, "Planets.AxialTilt_Normal".Translate(), "Planets.AxialTilt_Low".Translate(), "Planets.AxialTilt_High".Translate(), 1f));
			if (Planets_GameComponent.axialTilt != axialTiltCheck) { this.CurrentWorldPreset = "Planets.Custom"; }
			TooltipHandler.TipRegion(new Rect(0f, single, rect.xMax, rect.height), "Planets.AxialTiltTip".Translate());
		}

		private void DoPopulationSlider(float single)
		{
			Widgets.Label(new Rect(0f, single, 200f, 30f), "PlanetPopulation".Translate());
			Rect rect = new Rect(200f, single, 200f, 30f);
			var populationCheck = this.population;
			this.population = (OverallPopulation)Mathf.RoundToInt(Widgets.HorizontalSlider(rect, (float)this.population, 0f, (float)(OverallPopulationUtility.EnumValuesCount - 1), true, "PlanetPopulation_Normal".Translate(), "PlanetPopulation_Low".Translate(), "PlanetPopulation_High".Translate(), 1f));
			if (this.population != populationCheck)
			{
				this.CurrentWorldPreset = "Planets.Custom";
			}
		}

		public override void PostOpen()
		{
			base.PostOpen();
			TutorSystem.Notify_Event("PageStart-CreateWorldParams");
		}

		public override void PreOpen()
		{
			base.PreOpen();
			if (!this.initialized)
			{
				this.Reset();
				this.initialized = true;
			}
		}

		public void Reset()
		{
			this.seedString = GenText.RandomSeedString();
			this.planetCoverage = 0.3f;
			this.CurrentWorldPreset = "Planets.Vanilla";
			Planets_GameComponent.axialTilt = AxialTilt.Normal;
			Planets_GameComponent.worldType = WorldType.Vanilla;
			Planets_GameComponent.subcount = 10;
			this.rainfallMod = RainfallModifier.Normal;
			this.temperature = OverallTemperature.Normal;
			this.population = OverallPopulation.Normal;
		}

		public void Randomize()
		{
			this.seedString = GenText.RandomSeedString();

			Planets_GameComponent.axialTilt = Planets_Random.GetRandomAxialTilt();
			Planets_GameComponent.worldType = Planets_Random.GetRandomWorldType();
			this.rainfallMod = Planets_Random.GetRandomRainfallModifier();
			this.temperature = Planets_Random.GetRandomTemperature();
			this.population = Planets_Random.GetRandomPopulation();
			this.CurrentWorldPreset = "Planets.Custom";

			Controller.Settings.randomPlanet = true;

			if (this.CanDoNext())
			{
				this.DoNext();
			}
		}
	}
	
}
