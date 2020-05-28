using UnityEngine;
using Verse;

namespace Planets_Code
{
	public class Settings : ModSettings
	{
		// Biome
		public bool otherGrassland = false;
		public bool otherSavanna = false;
		public bool otherOasis = false;

		// Generation
		public bool usingMLP = false;
		public bool usingFactionControl = false;
		public bool randomPlanet = false;

		// Factions
		public SettingsValue<bool> checkTemp = new SettingsValue<bool>(defaultValue: true, name: "checkTemp");
		public SettingsValue<float> factionGrouping = new SettingsValue<float>(defaultValue: 2.5f, name: "factionGrouping");

		// Descriptions
		public SettingsValue<bool> showStoneTypes = new SettingsValue<bool>(defaultValue: false, name: "showStoneTypes");
		public SettingsValue<bool> showGrowingPeriod = new SettingsValue<bool>(defaultValue: true, name: "showGrowingPeriod");
		public SettingsValue<bool> showDiseaseFrequency = new SettingsValue<bool>(defaultValue: false, name: "showDiseaseFrequency");

		public void DoWindowContents(Rect canvas)
		{
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = canvas.width;
			list.Begin(canvas);
			list.Gap(24);
			// Faction settings
			if (Controller.Settings.usingFactionControl.Equals(true)) {
				list.Label("Planets.SettingsDisabled".Translate());
			}
			else {
				list.CheckboxLabeled( "Planets.CheckTemp".Translate(), ref checkTemp.CurrentValue, "Planets.CheckTempTip".Translate() );
				list.Gap(24);
				factionGrouping.CurrentValue = list.Slider(factionGrouping.CurrentValue, 0, 3.99f);
				if (factionGrouping.CurrentValue < 1) {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingNone".Translate());
				}
				else if (factionGrouping.CurrentValue < 2 ) {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingLoose".Translate());
				}
				else if (factionGrouping.CurrentValue < 3) {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingTight".Translate());
				}
				else {
					list.Label("Planets.FactionGrouping".Translate()+"  "+"Planets.FactionGroupingVeryTight".Translate());
				}
			}
			// World inspect pane information
			const float wipInfoGap = 12f;

			list.Gap(wipInfoGap);
			list.CheckboxLabeled("Planets.ShowStoneTypes".Translate(), ref showStoneTypes.CurrentValue, "Planets.ShowStoneTypesTip".Translate());

			list.Gap(wipInfoGap);
			list.CheckboxLabeled("Planets.ShowGrowingPeriod".Translate(), ref showGrowingPeriod.CurrentValue, "Planets.ShowGrowingPeriodTip".Translate());

			list.Gap(wipInfoGap);
			list.CheckboxLabeled("Planets.ShowDiseaseFrequency".Translate(), ref showDiseaseFrequency.CurrentValue, "Planets.ShowDiseaseFrequencyTip".Translate());

			list.Gap(24);
			if (list.ButtonText("Planets.ResetToDefault".Translate()))
			{
				this.checkTemp.ResetToDefault();
				this.factionGrouping.ResetToDefault();
				this.showStoneTypes.ResetToDefault();
				this.showGrowingPeriod.ResetToDefault();
				this.showDiseaseFrequency.ResetToDefault();
			}
			list.End();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Planets_Scribe_Values.Look(checkTemp);
			Planets_Scribe_Values.Look(factionGrouping);
			Planets_Scribe_Values.Look(showStoneTypes);
			Planets_Scribe_Values.Look(showGrowingPeriod);
			Planets_Scribe_Values.Look(showDiseaseFrequency);
		}
	}
}
