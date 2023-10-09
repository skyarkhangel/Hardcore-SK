using HarmonyLib;
using System;
using UnityEngine;
using Verse;
using System.Collections.Generic;

namespace Minerals
{
    public class MineralsSettings : ModSettings
    {
        public static float maxSetting = 15f;

        public float crystalAbundanceSetting = 1f;
        public float crystalDiversitySetting = 1f;
        public float boulderAbundanceSetting = 1f;
        public float rocksAbundanceSetting = 1f;
        public float mineralGrowthSetting = 1f;
        public float mineralReproductionSetting = 1f;
        public float mineralSpawningSetting = 1f;
        public bool replaceWallsSetting = true;
        public bool replaceChunksSetting = true;
        public bool includeFictionalSetting = true;
        public bool removeStartingChunksSetting = true;
        public bool underwaterMineralsSetting = true;
        public bool mineralsGrowUpWallsSetting = true;
        public bool snowyRockSetting = true;
        public float visualSpreadFactor = 1f;
        public float resourceDropFreqSetting = 1f;
        public float resourceDropAmountSetting = 1f;
        public float miningEffortSetting = 1f;
        public IntRange terrainCountRangeSetting = new IntRange(1, 4);


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref crystalAbundanceSetting, "crystalAbundanceSetting", 1f);
            Scribe_Values.Look(ref crystalDiversitySetting, "crystalDiversitySetting", 1f);
            Scribe_Values.Look(ref boulderAbundanceSetting, "boulderAbundanceSetting", 1f);
            Scribe_Values.Look(ref rocksAbundanceSetting, "rocksAbundanceSetting", 1f);
            Scribe_Values.Look(ref mineralGrowthSetting, "mineralGrowthSetting", 1f);
            Scribe_Values.Look(ref mineralReproductionSetting, "mineralReproductionSetting", 1f);
            Scribe_Values.Look(ref mineralSpawningSetting, "mineralSpawningSetting", 1f);
            Scribe_Values.Look(ref replaceWallsSetting, "replaceWallsSetting", true);
            Scribe_Values.Look(ref replaceChunksSetting, "replaceChunksSetting", true);
            Scribe_Values.Look(ref includeFictionalSetting, "includeFictionalSetting", true);
            Scribe_Values.Look(ref removeStartingChunksSetting, "removeStartingChunksSetting", true);
            Scribe_Values.Look(ref underwaterMineralsSetting, "underwaterMineralsSetting", true);
            Scribe_Values.Look(ref mineralsGrowUpWallsSetting, "mineralsGrowUpWallsSetting", true);
            Scribe_Values.Look(ref snowyRockSetting, "snowyRockSetting", true);
            Scribe_Values.Look(ref visualSpreadFactor, "visualSpreadFactor", 1f);
            Scribe_Values.Look(ref resourceDropFreqSetting, "resourceDropFreqSetting", 1f);
            Scribe_Values.Look(ref resourceDropAmountSetting, "resourceDropAmountSetting", 1f);
            Scribe_Values.Look(ref miningEffortSetting, "miningEffortSetting", 1f);
            Scribe_Values.Look<IntRange>(ref terrainCountRangeSetting, "terrainCountRangeSetting", new IntRange(1, 4), true);
        }


        public void DoWindowContents(Rect inRect)
        {
            const float bigGap = 12f;
            const float smallGap = 8f;
            const float headerSize = 38f;

            var list = new Listing_Standard { ColumnWidth = inRect.width / 2 - 34f };
            list.Begin(inRect);

            list.Label("abundanceSettingsHeader".Translate(), headerSize);

            list.Gap(smallGap);

            list.Label("crystalAbundanceSetting".Translate() + ": " + Math.Round(crystalAbundanceSetting * 100, 3) + "%", -1f);
            crystalAbundanceSetting = list.Slider(crystalAbundanceSetting, 0, maxSetting);

            list.Gap(smallGap);

            list.Label("crystalDiversitySetting".Translate() + ": " + Math.Round(crystalDiversitySetting * 100, 3) + "%", -1f);
            crystalDiversitySetting = list.Slider(crystalDiversitySetting, 0, maxSetting);

            list.Gap(smallGap);

            list.Label("boulderAbundanceSetting".Translate() + ": " + Math.Round(boulderAbundanceSetting * 100, 3) + "%", -1f);
            boulderAbundanceSetting = list.Slider(boulderAbundanceSetting, 0, maxSetting);

            list.Gap(smallGap);

            list.Label("rocksAbundanceSetting".Translate() + ": " + Math.Round(rocksAbundanceSetting * 100, 3) + "%", -1f);
            rocksAbundanceSetting = list.Slider(rocksAbundanceSetting, 0, maxSetting);

            list.Gap(bigGap);

            list.Label("dynamicMineralSettingsHeader".Translate(), headerSize);

            list.Gap(smallGap);

            list.Label("mineralGrowthSetting".Translate() + ": " + Math.Round(mineralGrowthSetting * 100, 3) + "%", -1f);
            mineralGrowthSetting = list.Slider(mineralGrowthSetting, 0, maxSetting);

            list.Gap(smallGap);

            list.Label("mineralReproductionSetting".Translate() + ": " + Math.Round(mineralReproductionSetting * 100, 3) + "%", -1f);
            mineralReproductionSetting = list.Slider(mineralReproductionSetting, 0, maxSetting);

            list.Gap(smallGap);

            list.Label("mineralSpawningSetting".Translate() + ": " + Math.Round(mineralSpawningSetting * 100, 3) + "%", -1f);
            mineralSpawningSetting = list.Slider(mineralSpawningSetting, 0, maxSetting);

            list.Gap(bigGap);

            list.Label("gameplaySettingsHeader".Translate(), headerSize);

            list.Gap(smallGap);

            list.CheckboxLabeled("replaceWallsSetting".Translate(), ref replaceWallsSetting);

            list.Gap(smallGap);

            list.CheckboxLabeled("replaceChunksSetting".Translate(), ref replaceChunksSetting);

            list.Gap(smallGap);

            list.CheckboxLabeled("removeStartingChunksSetting".Translate(), ref removeStartingChunksSetting);

            list.Gap(smallGap);

            list.CheckboxLabeled("includeFictionalSetting".Translate(), ref includeFictionalSetting);

            list.Gap(smallGap);

            list.Label("resourceDropFreqSetting".Translate() + ": " + Math.Round(resourceDropFreqSetting * 100, 3) + "%", -1f);
            resourceDropFreqSetting = list.Slider(resourceDropFreqSetting, 0, maxSetting);

            list.Gap(smallGap);

            list.Label("resourceDropAmountSetting".Translate() + ": " + Math.Round(resourceDropAmountSetting * 100, 3) + "%", -1f);
            resourceDropAmountSetting = list.Slider(resourceDropAmountSetting, 0, maxSetting);

            list.Gap(smallGap);

            list.Label("miningEffortSetting".Translate() + ": " + Math.Round(miningEffortSetting * 100, 3) + "%", -1f);
            miningEffortSetting = list.Slider(miningEffortSetting, 0.1f, maxSetting);

            list.Gap(smallGap);

            list.Label("terrainCountRangeSetting".Translate(), -1f);
            list.IntRange(ref terrainCountRangeSetting, 1, 10);

            list.Gap(bigGap);

            list.Label("graphicalSettingsHeader".Translate(), headerSize);

            list.Gap(smallGap);

            list.CheckboxLabeled("underwaterMineralsSetting".Translate(), ref underwaterMineralsSetting);

            list.Gap(smallGap);

            list.CheckboxLabeled("mineralsGrowUpWallsSetting".Translate(), ref mineralsGrowUpWallsSetting);

            list.Gap(smallGap);

            list.CheckboxLabeled("snowyRockSetting".Translate(), ref snowyRockSetting);

            list.Gap(smallGap);

            list.Label("visualSpreadFactor".Translate() + ": " + Math.Round(visualSpreadFactor * 100, 3) + "%", -1f);
            visualSpreadFactor = list.Slider(visualSpreadFactor, 0, 3);

            list.End();
        }
    }
}