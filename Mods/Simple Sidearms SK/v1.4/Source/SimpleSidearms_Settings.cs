using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

using static PeteTimesSix.SimpleSidearms.Utilities.Enums;
using static PeteTimesSix.SimpleSidearms.UI.SidearmsSpecificExtensions;
using static PeteTimesSix.SimpleSidearms.UI.ListingExtensions;
using PeteTimesSix.SimpleSidearms.Utilities;
using SimpleSidearms.rimworld;
using System.Linq;
using RimWorld;
using PeteTimesSix.SimpleSidearms.UI.Verse;

namespace PeteTimesSix.SimpleSidearms
{
    public static class InferredValues
    {
        public static float maxWeightMelee;
        public static float maxWeightRanged;
        public static float maxWeightTotal;
        public static float maxCapacity = 35f;

        public static void Init()
        {
            GettersFilters.getHeaviestWeapons(out maxWeightMelee, out maxWeightRanged);
            maxWeightMelee += 1;
            maxWeightRanged += 1;
            maxWeightTotal = Math.Max(maxWeightMelee, maxWeightRanged);
        }
    }

    public class SimpleSidearms_Settings : ModSettings
    {
        public bool NeedsResaving = false;

        public OptionsTab ActiveTab = OptionsTab.Presets;

        public bool SettingsEverOpened = false;

        public SettingsPreset ActivePreset = SettingsPreset.NoneApplied;

        public bool ToolAutoSwitch;
        public bool OptimalMelee;
        public bool CQCAutoSwitch;
        public bool CQCTargetOnly;
        public bool RangedCombatAutoSwitch;
        public bool RangedNonCombatAutoSwitch;

        public float RangedCombatAutoSwitchMaxWarmup;

        public float SpeedSelectionBiasMelee;
        public float SpeedSelectionBiasRanged;

        public bool AllowBlockedWeaponUse;

        public bool SeparateModes;
        public LimitModeSingleSidearm LimitModeSingle;
        public LimitModeAmountOfSidearms LimitModeAmount;

        public LimitModeSingleSidearm LimitModeSingleMelee;
        public LimitModeAmountOfSidearms LimitModeAmountMelee;
        public LimitModeSingleSidearm LimitModeSingleRanged;
        public LimitModeAmountOfSidearms LimitModeAmountRanged;
        public LimitModeAmountOfSidearms LimitModeAmountTotal;

        #region LimitModeSingle
        public float LimitModeSingle_AbsoluteMass;
        public float LimitModeSingle_RelativeMass;
        public HashSet<ThingDef> LimitModeSingle_Selection;
        private HashSet<string> LimitModeSingle_Selection_AsStringCache;

        private HashSet<ThingDef> LimitModeSingle_Match_Cache;
        #endregion
        #region LimitModeAmount
        public float LimitModeAmount_AbsoluteMass;
        public float LimitModeAmount_RelativeMass;
        public int LimitModeAmount_Slots;
        #endregion

        #region LimitModeSingleMelee
        public float LimitModeSingleMelee_AbsoluteMass;
        public float LimitModeSingleMelee_RelativeMass;
        public HashSet<ThingDef> LimitModeSingleMelee_Selection;
        private HashSet<string> LimitModeSingleMelee_Selection_AsStringCache;

        private HashSet<ThingDef> LimitModeSingleMelee_Match_Cache;
        #endregion
        #region LimitModeAmountMelee
        public float LimitModeAmountMelee_AbsoluteMass;
        public float LimitModeAmountMelee_RelativeMass;
        public int LimitModeAmountMelee_Slots;
        #endregion
        #region LimitModeSingleRanged
        public float LimitModeSingleRanged_AbsoluteMass;
        public float LimitModeSingleRanged_RelativeMass;
        public HashSet<ThingDef> LimitModeSingleRanged_Selection;
        private HashSet<string> LimitModeSingleRanged_Selection_AsStringCache;

        private HashSet<ThingDef> LimitModeSingleRanged_Match_Cache;
        #endregion
        #region LimitModeAmountRanged
        public float LimitModeAmountRanged_AbsoluteMass;
        public float LimitModeAmountRanged_RelativeMass;
        public int LimitModeAmountRanged_Slots;
        #endregion
        #region LimitModeAmountTotal
        public float LimitModeAmountTotal_AbsoluteMass;
        public float LimitModeAmountTotal_RelativeMass;
        public int LimitModeAmountTotal_Slots;
        #endregion

        public float SidearmSpawnChance;
        public float SidearmSpawnChanceDropoff;
        public float SidearmBudgetMultiplier;
        public float SidearmBudgetDropoff;

        public PrimaryWeaponMode ColonistDefaultWeaponMode;
        public PrimaryWeaponMode NPCDefaultWeaponMode;

        public FumbleModeOptionsEnum FumbleMode;
        public SimpleCurve FumbleRecoveryChance;

        private static CurvePoint[] defaultFumbleRecoveryChancePoints = { new CurvePoint(0, 0), new CurvePoint(2, 0.15f), new CurvePoint(5, 0.85f), new CurvePoint(7, 1), new CurvePoint(20, 1) };
        public bool ReEquipOutOfCombat;
        public bool ReEquipBest;
        public bool ReEquipInCombat;

        public bool SkipDangerousWeapons;
        public bool SkipEMPWeapons;

        public bool PreserveInventoryInCaravans;
        public bool HideSidearmsInCaravanDialogs;

        public bool ShowAlertsMissingSidearm;

        public bool GizmoPerformanceMode;

        public void StartupChecks()
        {
            if (LimitModeSingle_Selection == null)
                LimitModeSingle_Selection = new HashSet<ThingDef>();
            if (LimitModeSingleMelee_Selection == null)
                LimitModeSingleMelee_Selection = new HashSet<ThingDef>();
            if (LimitModeSingleRanged_Selection == null)
                LimitModeSingleRanged_Selection = new HashSet<ThingDef>();
            if (ActivePreset == SettingsPreset.NoneApplied) 
            {
                SettingsEverOpened = false;
                ApplyPreset(SettingsPreset.Basic);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref SettingsEverOpened, "SettingsEverOpened", defaultValue: false);
            Scribe_Values.Look(ref ActivePreset, "ActivePreset", defaultValue: SettingsPreset.NoneApplied);

            Scribe_Values.Look(ref ToolAutoSwitch, "ToolAutoSwitch", defaultValue: true);
            Scribe_Values.Look(ref OptimalMelee, "OptimalMelee", defaultValue: true);
            Scribe_Values.Look(ref CQCAutoSwitch, "CQCAutoSwitch", defaultValue: true);
            Scribe_Values.Look(ref CQCTargetOnly, "CQCTargetOnly", defaultValue: false);
            Scribe_Values.Look(ref RangedCombatAutoSwitch, "RangedCombatAutoSwitch", defaultValue: true);
            Scribe_Values.Look(ref RangedNonCombatAutoSwitch, "RangedNonCombatAutoSwitch", defaultValue: true);

            Scribe_Values.Look(ref RangedCombatAutoSwitchMaxWarmup, "RangedCombatAutoSwitchMaxWarmup", defaultValue: 0.75f);

            Scribe_Values.Look(ref SpeedSelectionBiasMelee, "SpeedSelectionBiasMelee", defaultValue: 1f);
            Scribe_Values.Look(ref SpeedSelectionBiasRanged, "SpeedSelectionBiasRanged", defaultValue: 1.1f);

            Scribe_Values.Look(ref AllowBlockedWeaponUse, "AllowBlockedWeaponUse", defaultValue: false);

            Scribe_Values.Look(ref SeparateModes, "SeparateModes", defaultValue: false);

            Scribe_Values.Look(ref LimitModeSingle, "LimitModeSingle", defaultValue: LimitModeSingleSidearm.None);
            Scribe_Values.Look(ref LimitModeAmount, "LimitModeAmount", defaultValue: LimitModeAmountOfSidearms.None);

            Scribe_Values.Look(ref LimitModeSingleMelee, "LimitModeSingleMelee", defaultValue: LimitModeSingleSidearm.None);
            Scribe_Values.Look(ref LimitModeAmountMelee, "LimitModeAmountMelee", defaultValue: LimitModeAmountOfSidearms.None);
            Scribe_Values.Look(ref LimitModeSingleRanged, "LimitModeSingleRanged", defaultValue: LimitModeSingleSidearm.None);
            Scribe_Values.Look(ref LimitModeAmountRanged, "LimitModeAmountRanged", defaultValue: LimitModeAmountOfSidearms.None);
            Scribe_Values.Look(ref LimitModeAmountTotal, "LimitModeAmountTotal", defaultValue: LimitModeAmountOfSidearms.None);

            Scribe_Values.Look(ref LimitModeSingle_AbsoluteMass, "LimitModeSingle_AbsoluteMass", defaultValue: 1.9f);
            Scribe_Values.Look(ref LimitModeSingle_RelativeMass, "LimitModeSingle_RelativeMass", defaultValue: 0.25f);

            Scribe_Values.Look(ref LimitModeAmount_AbsoluteMass, "LimitModeAmount_AbsoluteMass", defaultValue: 10f);
            Scribe_Values.Look(ref LimitModeAmount_RelativeMass, "LimitModeAmount_RelativeMass", defaultValue: 0.5f);
            Scribe_Values.Look(ref LimitModeAmount_Slots, "LimitModeAmount_Slots", defaultValue: 2);

            Scribe_Values.Look(ref LimitModeSingleMelee_AbsoluteMass, "LimitModeSingleMelee_AbsoluteMass", defaultValue: 1.9f);
            Scribe_Values.Look(ref LimitModeSingleMelee_RelativeMass, "LimitModeSingleMelee_RelativeMass", defaultValue: 0.25f);

            Scribe_Values.Look(ref LimitModeAmountMelee_AbsoluteMass, "LimitModeAmountMelee_AbsoluteMass", defaultValue: 10f);
            Scribe_Values.Look(ref LimitModeAmountMelee_RelativeMass, "LimitModeAmountMelee_RelativeMass", defaultValue: 0.5f);
            Scribe_Values.Look(ref LimitModeAmountMelee_Slots, "LimitModeAmountMelee_Slots", defaultValue: 2);

            Scribe_Values.Look(ref LimitModeSingleRanged_AbsoluteMass, "LimitModeSingleRanged_AbsoluteMass", defaultValue: 2.55f);
            Scribe_Values.Look(ref LimitModeSingleRanged_RelativeMass, "LimitModeSingleRanged_RelativeMass", defaultValue: 0.25f);

            Scribe_Values.Look(ref LimitModeAmountRanged_AbsoluteMass, "LimitModeAmountRanged_AbsoluteMass", defaultValue: 10f);
            Scribe_Values.Look(ref LimitModeAmountRanged_RelativeMass, "LimitModeAmountRanged_RelativeMass", defaultValue: 0.5f);
            Scribe_Values.Look(ref LimitModeAmountRanged_Slots, "LimitModeAmountRanged_Slots", defaultValue: 2);

            Scribe_Values.Look(ref LimitModeAmountTotal_AbsoluteMass, "LimitModeAmountTotal_AbsoluteMass", defaultValue: 10f);
            Scribe_Values.Look(ref LimitModeAmountTotal_RelativeMass, "LimitModeAmountTotal_RelativeMass", defaultValue: 0.5f);
            Scribe_Values.Look(ref LimitModeAmountTotal_Slots, "LimitModeAmountTotal_Slots", defaultValue: 4);

            //avoid errors on removed defs
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var singlesAsStrings = LimitModeSingle_Selection.Where(t => t != null).Select(t => t.defName).ToList();
                var singleMeleeAsStrings = LimitModeSingleMelee_Selection.Where(t => t != null).Select(t => t.defName).ToList();
                var singleRangedAsStrings = LimitModeSingleRanged_Selection.Where(t => t != null).Select(t => t.defName).ToList();

                Scribe_Collections.Look(ref singlesAsStrings, "LimitModeSingle_Selection_defNames", LookMode.Value);
                Scribe_Collections.Look(ref singleMeleeAsStrings, "LimitModeSingleMelee_Selection_defNames", LookMode.Value);
                Scribe_Collections.Look(ref singleRangedAsStrings, "LimitModeSingleRanged_Selection_defNames", LookMode.Value);

                HashSet<ThingDef> emptyCol = new HashSet<ThingDef>();

                Scribe_Collections.Look(ref emptyCol, "LimitModeSingle_Selection", LookMode.Def);
                Scribe_Collections.Look(ref emptyCol, "LimitModeSingleMelee_Selection", LookMode.Def);
                Scribe_Collections.Look(ref emptyCol, "LimitModeSingleRanged_Selection", LookMode.Def);
            }
            else
            {
                Scribe_Collections.Look(ref LimitModeSingle_Selection, "LimitModeSingle_Selection", LookMode.Def);
                Scribe_Collections.Look(ref LimitModeSingleMelee_Selection, "LimitModeSingleMelee_Selection", LookMode.Def);
                Scribe_Collections.Look(ref LimitModeSingleRanged_Selection, "LimitModeSingleRanged_Selection", LookMode.Def);

                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    Scribe_Collections.Look(ref LimitModeSingle_Selection_AsStringCache, "LimitModeSingle_Selection_defNames", LookMode.Value);
                    Scribe_Collections.Look(ref LimitModeSingleMelee_Selection_AsStringCache, "LimitModeSingleMelee_Selection_defNames", LookMode.Value);
                    Scribe_Collections.Look(ref LimitModeSingleRanged_Selection_AsStringCache, "LimitModeSingleRanged_Selection_defNames", LookMode.Value);
                    if (LimitModeSingle_Selection_AsStringCache == null) 
                        LimitModeSingle_Selection_AsStringCache = new HashSet<string>();
                    if (LimitModeSingleMelee_Selection_AsStringCache == null)
                        LimitModeSingleMelee_Selection_AsStringCache = new HashSet<string>();
                    if (LimitModeSingleRanged_Selection_AsStringCache == null)
                        LimitModeSingleRanged_Selection_AsStringCache = new HashSet<string>();
                }
                else if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    if ((LimitModeSingle_Selection != null && LimitModeSingle_Selection.Any()) ||
                        (LimitModeSingleMelee_Selection != null && LimitModeSingleMelee_Selection.Any()) ||
                        (LimitModeSingleRanged_Selection != null && LimitModeSingleRanged_Selection.Any()))
                    {
                        Log.Warning($"SS: LimitMode collection contained defs. This will need a one-time migration resave.");
                        NeedsResaving = true;
                    }

                    foreach (var defName in LimitModeSingle_Selection_AsStringCache) 
                    {
                        var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                        if(def == null) 
                        {
                            //Log.Warning($"SS: LimitModeSingle_Selection contained unknown def {defName}. Removing.");
                            NeedsResaving = true;
                            continue;
                        }
                        LimitModeSingle_Selection.Add(def);
                    }
                    foreach (var defName in LimitModeSingleMelee_Selection_AsStringCache)
                    {
                        var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                        if (def == null)
                        {
                            //Log.Warning($"SS: LimitModeSingleMelee_Selection contained unknown def {defName}. Removing.");
                            NeedsResaving = true;
                            continue;
                        }
                        LimitModeSingleMelee_Selection.Add(def);
                    }
                    foreach (var defName in LimitModeSingleRanged_Selection_AsStringCache)
                    {
                        var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                        if (def == null)
                        {
                            //Log.Warning($"SS: LimitModeSingleRanged_Selection contained unknown def {defName}. Removing.");
                            NeedsResaving = true;
                            continue;
                        }
                        LimitModeSingleRanged_Selection.Add(def);
                    }

                    LimitModeSingle_Selection_AsStringCache = null;
                    LimitModeSingleMelee_Selection_AsStringCache = null;
                    LimitModeSingleRanged_Selection_AsStringCache = null;
                }
            } 

            Scribe_Values.Look(ref SidearmSpawnChance, "SidearmSpawnChance", defaultValue: 0.5f);
            Scribe_Values.Look(ref SidearmSpawnChanceDropoff, "SidearmSpawnChanceDropoff", defaultValue: 0.25f);
            Scribe_Values.Look(ref SidearmBudgetMultiplier, "SidearmBudgetMultiplier", defaultValue: 0.5f);
            Scribe_Values.Look(ref SidearmBudgetDropoff, "SidearmBudgetDropoff", defaultValue: 0.25f);

            Scribe_Values.Look(ref ColonistDefaultWeaponMode, "ColonistDefaultWeaponMode", defaultValue: PrimaryWeaponMode.BySkill);
            Scribe_Values.Look(ref NPCDefaultWeaponMode, "NPCDefaultWeaponMode", PrimaryWeaponMode.ByGenerated);

            Scribe_Values.Look(ref FumbleMode, "FumbleMode", defaultValue: FumbleModeOptionsEnum.InDistress);
            Scribe_Values.Look(ref ReEquipOutOfCombat, "ReEquipOutOfCombat", defaultValue: true);
            Scribe_Values.Look(ref ReEquipBest, "ReEquipBest", defaultValue: true);
            Scribe_Values.Look(ref ReEquipInCombat, "ReEquipInCombat", defaultValue: true);

            Scribe_Values.Look(ref SkipDangerousWeapons, "SkipDangerousWeapons", defaultValue: true);
            Scribe_Values.Look(ref SkipEMPWeapons, "SkipEMPWeapons", defaultValue: false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var temp = FumbleRecoveryChance.ToList();
                Scribe_Collections.Look(ref temp, "FumbleRecoveryChance");
            }
            else
            {
                List<CurvePoint> temp = null;
                Scribe_Collections.Look(ref temp, "FumbleRecoveryChance");
                FumbleRecoveryChance = temp != null ? new SimpleCurve(temp) : new SimpleCurve(defaultFumbleRecoveryChancePoints);
            }

            Scribe_Values.Look(ref PreserveInventoryInCaravans, "PreserveInventoryInCaravans", defaultValue: true);
            Scribe_Values.Look(ref HideSidearmsInCaravanDialogs, "HideSidearmsInCaravanDialogs", defaultValue: true);

            Scribe_Values.Look(ref ShowAlertsMissingSidearm, "ShowAlertsMissingSidearm", defaultValue: true);
            Scribe_Values.Look(ref GizmoPerformanceMode, "GizmoPerformanceMode", defaultValue: false);
        }

        Vector2 scrollPosition = new Vector2(0, 0);
        float cachedScrollHeight = 0;

        internal void DoSettingsWindowContents(Rect outerRect)
        {
            SettingsEverOpened = true;

            bool change = false;
            Action onChange = () => { change = true; };

            Color colorSave = GUI.color;
            TextAnchor anchorSave = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            var headerRect = outerRect.TopPartPixels(50);
            var restOfRect = new Rect(outerRect);
            restOfRect.y += 50;
            restOfRect.height -= 50;

            Listing_Standard prelist = new Listing_Standard();
            prelist.Begin(headerRect);

            prelist.EnumSelector("ActiveTab_title".Translate(), ref ActiveTab, "ActiveTab_option_", valueTooltipPostfix: null, tooltip: "ActiveTab_desc".Translate());
            prelist.GapLine();

            prelist.End();


            bool needToScroll = cachedScrollHeight > outerRect.height;
            var viewRect = new Rect(restOfRect);
            if (needToScroll)
            {
                viewRect.width -= 20f;
                viewRect.height = cachedScrollHeight;
                Widgets.BeginScrollView(restOfRect, ref scrollPosition, viewRect);
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            listingStandard.Begin(viewRect);

            float initialY = listingStandard.GetRect(0).y;
            float maxWidth = listingStandard.ColumnWidth;

            //public enum OptionsTab { Presets, Automation, Allowances, Spawning, Misc }


            switch (ActiveTab) 
            {
                case OptionsTab.Presets:
                    {
                        if (listingStandard.RadioButton("PresetCustom_title".Translate(), active: ActivePreset == SettingsPreset.Custom, tooltip: "PresetCustom_desc".Translate()))
                            ApplyPreset(SettingsPreset.Custom);
                        if (listingStandard.RadioButton("Preset0_title".Translate(), active: ActivePreset == SettingsPreset.Disabled, tooltip: "Preset0_desc".Translate()))
                            ApplyPreset(SettingsPreset.Disabled);
                        if (listingStandard.RadioButton("Preset1_title".Translate(), active: ActivePreset == SettingsPreset.LoadoutOnly, tooltip: "Preset1_desc".Translate()))
                            ApplyPreset(SettingsPreset.LoadoutOnly);
                        if (listingStandard.RadioButton("Preset1half_title".Translate(), active: ActivePreset == SettingsPreset.Lite, tooltip: "Preset1half_desc".Translate()))
                            ApplyPreset(SettingsPreset.Lite);
                        if (listingStandard.RadioButton("Preset2_title".Translate(), active: ActivePreset == SettingsPreset.Basic, tooltip: "Preset2_desc".Translate()))
                            ApplyPreset(SettingsPreset.Basic);
                        if (listingStandard.RadioButton("Preset3_title".Translate(), active: ActivePreset == SettingsPreset.Advanced, tooltip: "Preset3_desc".Translate()))
                            ApplyPreset(SettingsPreset.Advanced);
                        if (listingStandard.RadioButton("Preset4_title".Translate(), active: ActivePreset == SettingsPreset.Excessive, tooltip: "Preset4_desc".Translate()))
                            ApplyPreset(SettingsPreset.Excessive);
                        if (listingStandard.RadioButton("Preset5_title".Translate(), active: ActivePreset == SettingsPreset.Brawler, tooltip: "Preset5_desc".Translate()))
                            ApplyPreset(SettingsPreset.Brawler);
                    }
                    break;
                case OptionsTab.Automation:
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.CheckboxLabeled("ToolAutoSwitch_title".Translate(), ref ToolAutoSwitch, "ToolAutoSwitch_desc".Translate(), onChange: onChange);
                        subsection.CheckboxLabeled("OptimalMelee_title".Translate(), ref OptimalMelee, "OptimalMelee_desc".Translate(), onChange: onChange);
                        subsection.CheckboxLabeled("CQCAutoSwitch_title".Translate(), ref CQCAutoSwitch, "CQCAutoSwitch_desc".Translate(), onChange: onChange);
                        subsection.CheckboxLabeled("CQCTargetOnly_title".Translate(), ref CQCTargetOnly, "CQCTargetOnly_desc".Translate(), onChange: onChange);

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.CheckboxLabeled("RangedCombatAutoSwitch_title".Translate(), ref RangedCombatAutoSwitch, "RangedCombatAutoSwitch_desc".Translate(), onChange);
                        if (RangedCombatAutoSwitch)
                        {
                            subsection.SliderLabeled("RangedCombatAutoSwitchMaxWarmup_title".Translate(), ref RangedCombatAutoSwitchMaxWarmup, 0, 1, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        }
                        subsection.CheckboxLabeled("RangedNonCombatAutoSwitch_title".Translate(), ref RangedNonCombatAutoSwitch, "RangedNonCombatAutoSwitch_desc".Translate(), onChange);

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.SliderSpeedBias("SpeedSelectionBiasMelee_title".Translate(), ref SpeedSelectionBiasMelee, 0.5f, 1f, 2f, true, displayMult: 100, valueSuffix: "%", onChange: onChange);

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.SliderSpeedBias("SpeedSelectionBiasRanged_title".Translate(), ref SpeedSelectionBiasRanged, 0.5f, 1f, 2f, false, displayMult: 100, valueSuffix: "%", onChange: onChange);

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    break;
                case OptionsTab.Allowances:
                    {
                        listingStandard.CheckboxLabeled("AllowBlockedWeaponUse_title".Translate(), ref AllowBlockedWeaponUse, "AllowBlockedWeaponUse_desc".Translate(), onChange: onChange);

                        var valBefore = SeparateModes;
                        listingStandard.CheckboxLabeled("SeparateModes_title".Translate(), ref SeparateModes, "SeparateModes_desc".Translate(), onChange: onChange);
                        if (valBefore != SeparateModes)
                        {
                            LimitModeSingle_Match_Cache = null;
                            LimitModeSingleMelee_Match_Cache = null;
                            LimitModeSingleRanged_Match_Cache = null;
                        }
                    }
                    if (!SeparateModes)
                    {
                        Limits(listingStandard, WeaponListKind.Both, onChange: onChange);
                    }
                    else
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        Limits(subsection, WeaponListKind.Melee, onChange: onChange);

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        Limits(subsection, WeaponListKind.Ranged, onChange: onChange);

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);

                        listingStandard.GapLine();

                        listingStandard.EnumSelector("LimitModeAmountTotal_title".Translate(), ref LimitModeAmountTotal, "LimitModeAmount_option_", valueTooltipPostfix: null, tooltip: "LimitModeAmountTotal_desc".Translate(), onChange: onChange);
                        switch (LimitModeAmountTotal)
                        {
                            case LimitModeAmountOfSidearms.AbsoluteWeight:
                                listingStandard.SliderLabeled("MaximumMassAmountAbsolute_title".Translate(), ref LimitModeAmountTotal_AbsoluteMass, 0, InferredValues.maxCapacity, decimalPlaces: 1, valueSuffix: " kg", onChange: onChange);
                                break;
                            case LimitModeAmountOfSidearms.RelativeWeight:
                                listingStandard.SliderLabeled("MaximumMassAmountRelative_title".Translate(), ref LimitModeAmountTotal_RelativeMass, 0, 1, displayMult: 100, valueSuffix: "%", onChange: onChange);
                                break;
                            case LimitModeAmountOfSidearms.Slots:
                                listingStandard.Spinner("MaximumSlots_title".Translate(), ref LimitModeAmountTotal_Slots, min: 1, tooltip: "MaximumSlots_desc".Translate(), onChange: onChange);
                                break;
                            case LimitModeAmountOfSidearms.None:
                                break;
                        }
                    }
                    {
                        Color save = GUI.color;
                        GUI.color = Color.gray;
                        listingStandard.Label("LimitCarryInfo_title".Translate());
                        GUI.color = save;
                    }
                    break;
                case OptionsTab.Spawning:
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.SliderLabeled("SidearmSpawnChance_title".Translate(), ref SidearmSpawnChance, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmSpawnChance_desc".Translate(), onChange: onChange);
                        subsection.SliderLabeled("SidearmSpawnChanceDropoff_title".Translate(), ref SidearmSpawnChanceDropoff, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmSpawnChanceDropoff_desc".Translate(), onChange: onChange);

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.SliderLabeled("SidearmBudgetMultiplier_title".Translate(), ref SidearmBudgetMultiplier, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmBudgetMultiplier_desc".Translate(), onChange: onChange);
                        subsection.SliderLabeled("SidearmBudgetDropoff_title".Translate(), ref SidearmBudgetDropoff, 0, 1, displayMult: 100, valueSuffix: "%", tooltip: "SidearmBudgetDropoff_desc".Translate(), onChange: onChange);

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    break;
                case OptionsTab.Misc:
                    {
                        var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                        subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                        subsection.EnumSelector("ColonistDefaultWeaponMode_title".Translate(), ref ColonistDefaultWeaponMode, "PrimaryWeaponMode_option_", valueTooltipPostfix: null, tooltip: "ColonistDefaultWeaponMode_desc".Translate(), onChange: onChange);
                        subsection.EnumSelector("NPCDefaultWeaponMode_title".Translate(), ref NPCDefaultWeaponMode, "PrimaryWeaponMode_option_", valueTooltipPostfix: null, tooltip: "NPCDefaultWeaponMode_desc".Translate(), onChange: onChange);

                        subsection.EnumSelector("DropMode_title".Translate(), ref FumbleMode, "DropMode_option_", valueTooltipPostfix: null, tooltip: "DropMode_desc".Translate(), onChange: onChange);
                        Text.Anchor = TextAnchor.MiddleLeft;
                        subsection.Label("FumbleRecoveryChance_title".Translate());
                        Text.Anchor = TextAnchor.MiddleCenter;
                        var rect = subsection.GetRect(100f);
                        Widgets.DrawBoxSolid(rect, Color.black);
                        var innerRect = rect.ContractedBy(2f);
                        CurveEditorPublic.DoCurveEditor(innerRect, FumbleRecoveryChance, displayMult: 100, valueSuffix: "%", onChange: onChange);

                        Color save = GUI.color;
                        GUI.color = Color.gray;
                        subsection.Label("FumbleRecoveryChance_hint".Translate());
                        GUI.color = save;

                        subsection.NewHiddenColumn(ref subsectionHeight);

                        subsection.CheckboxLabeled("ReEquipOutOfCombat_title".Translate(), ref ReEquipOutOfCombat, "ReEquipOutOfCombat_desc".Translate(), onChange: onChange);
                        if (ReEquipOutOfCombat)
                        {
                            subsection.CheckboxLabeled("ReEquipBest_title".Translate(), ref ReEquipBest, "ReEquipBest_desc".Translate(), onChange: onChange);
                            subsection.CheckboxLabeled("ReEquipInCombat_title".Translate(), ref ReEquipInCombat, "ReEquipInCombat_desc".Translate(), onChange: onChange);
                        }

                        subsection.GapLine();

                        subsection.CheckboxLabeled("SkipDangerousWeapons_title".Translate(), ref SkipDangerousWeapons, "SkipDangerousWeapons_desc".Translate(), onChange: onChange);
                        subsection.CheckboxLabeled("SkipEMPWeapons_title".Translate(), ref SkipEMPWeapons, "SkipEMPWeapons_desc".Translate(), onChange: onChange);

                        subsection.GapLine();

                        subsection.CheckboxLabeled("PreserveInventoryInCaravans_title".Translate(), ref PreserveInventoryInCaravans, "PreserveInventoryInCaravans_desc".Translate(), onChange: onChange);
                        subsection.CheckboxLabeled("HideSidearmsInCaravanDialogs_title".Translate(), ref HideSidearmsInCaravanDialogs, "HideSidearmsInCaravanDialogs_desc".Translate(), onChange: onChange);

                        subsection.GapLine();

                        subsection.CheckboxLabeled("ShowAlertsMissingSidearm_title".Translate(), ref ShowAlertsMissingSidearm, "ShowAlertsMissingSidearm_desc".Translate(), onChange: onChange);
                        subsection.CheckboxLabeled("GizmoPerformanceMode_title".Translate(), ref GizmoPerformanceMode, "GizmoPerformanceMode_desc".Translate(), onChange: onChange);

                        listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    }
                    break;
            }

            cachedScrollHeight = listingStandard.CurHeight;
            listingStandard.End();

            if(needToScroll)
            {
                Widgets.EndScrollView();
            }

            //GUI.EndGroup();

            if(change)
                ApplyPreset(SettingsPreset.Custom);

            GUI.color = colorSave;
            Text.Anchor = anchorSave;
        }

        public void ApplyBaseSettings() 
        {
            ToolAutoSwitch = false;
            OptimalMelee = true;
            CQCAutoSwitch = true;
            CQCTargetOnly = false;
            RangedCombatAutoSwitch = true;

            RangedCombatAutoSwitchMaxWarmup = 0.75f;

            SpeedSelectionBiasMelee = 1f;
            SpeedSelectionBiasRanged = 1.1f;

            SeparateModes = false;
            LimitModeSingle = LimitModeSingleSidearm.None;
            LimitModeAmount = LimitModeAmountOfSidearms.None;

            LimitModeSingleMelee = LimitModeSingleSidearm.None;
            LimitModeAmountMelee = LimitModeAmountOfSidearms.None;
            LimitModeSingleRanged = LimitModeSingleSidearm.None;
            LimitModeAmountRanged = LimitModeAmountOfSidearms.None;
            LimitModeAmountTotal = LimitModeAmountOfSidearms.None;

            #region LimitModeSingle
            LimitModeSingle_AbsoluteMass = 1.9f;
            LimitModeSingle_RelativeMass = 0.25f;
            LimitModeSingle_Selection = GettersFilters.getValidWeaponsThingDefsOnly().Where(w => w.GetStatValueAbstract(StatDefOf.Mass) <= LimitModeSingle_AbsoluteMass).ToHashSet();
            #endregion
            #region LimitModeAmount
            LimitModeAmount_AbsoluteMass = 10f;
            LimitModeAmount_RelativeMass = 0.5f;
            LimitModeAmount_Slots = 2;
            #endregion

            #region LimitModeSingleMelee
            LimitModeSingleMelee_AbsoluteMass = 1.9f;
            LimitModeSingleMelee_RelativeMass = 0.25f;
            LimitModeSingleMelee_Selection = GettersFilters.filterForWeaponKind(GettersFilters.getValidWeaponsThingDefsOnly(), WeaponSearchType.Melee).Where(w => w.GetStatValueAbstract(StatDefOf.Mass) <= LimitModeSingleMelee_AbsoluteMass).ToHashSet();
            #endregion
            #region LimitModeAmountMelee
            LimitModeAmountMelee_AbsoluteMass = 10f;
            LimitModeAmountMelee_RelativeMass = 0.5f;
            LimitModeAmountMelee_Slots = 2;
            #endregion

            #region LimitModeSingleRanged
            LimitModeSingleRanged_AbsoluteMass = 2.55f;
            LimitModeSingleRanged_RelativeMass = 0.25f;
            LimitModeSingleRanged_Selection = GettersFilters.filterForWeaponKind(GettersFilters.getValidWeaponsThingDefsOnly(), WeaponSearchType.Ranged).Where(w => w.GetStatValueAbstract(StatDefOf.Mass) <= LimitModeSingleRanged_AbsoluteMass).ToHashSet();
            #endregion
            #region LimitModeAmountRanged
            LimitModeAmountRanged_AbsoluteMass = 10f;
            LimitModeAmountRanged_RelativeMass = 0.5f;
            LimitModeAmountRanged_Slots = 2;
            #endregion

            #region LimitModeAmountTotal
            LimitModeAmountTotal_AbsoluteMass = 10f;
            LimitModeAmountTotal_RelativeMass = 0.5f;
            LimitModeAmountTotal_Slots = 4;
            #endregion

            SidearmSpawnChance = 0.5f;
            SidearmSpawnChanceDropoff = 0.25f;
            SidearmBudgetMultiplier = 0.5f;
            SidearmBudgetDropoff = 0.25f;

            ColonistDefaultWeaponMode = PrimaryWeaponMode.BySkill;
            NPCDefaultWeaponMode = PrimaryWeaponMode.ByGenerated;

            FumbleMode = FumbleModeOptionsEnum.InDistress;
            FumbleRecoveryChance = new SimpleCurve(defaultFumbleRecoveryChancePoints);
            ReEquipOutOfCombat = true;
            ReEquipBest = true;
            ReEquipInCombat = true;

            PreserveInventoryInCaravans = true;
            HideSidearmsInCaravanDialogs = true;
        }

        public void ApplyPreset(SettingsPreset preset)
        {
            if (preset == SettingsPreset.NoneApplied)
                throw new InvalidOperationException("SettingsPreset.NoneApplied should never be assigned!");

            ActivePreset = preset;
            if (preset == SettingsPreset.Custom) //setting the preset TO custom does nothing
                return;

            ApplyBaseSettings();

            switch (preset)
            {
                case SettingsPreset.NoneApplied:
                    break;
                case SettingsPreset.Disabled:
                    ToolAutoSwitch = false;
                    OptimalMelee = false;
                    CQCAutoSwitch = false;
                    RangedCombatAutoSwitch = false;

                    SidearmSpawnChance = 0.0f;

                    LimitModeAmount = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmount_Slots = 1;

                    FumbleMode = FumbleModeOptionsEnum.Never;
                    ReEquipOutOfCombat = false;
                    ReEquipBest = false;
                    ReEquipInCombat = false;

                    PreserveInventoryInCaravans = false;
                    HideSidearmsInCaravanDialogs = false;
                    break;
                case SettingsPreset.LoadoutOnly:
                    ToolAutoSwitch = false;
                    OptimalMelee = false;
                    CQCAutoSwitch = false;
                    RangedCombatAutoSwitch = false;
                    LimitModeSingle = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingle_AbsoluteMass = 4.75f;
                    LimitModeAmount = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmount_Slots = 3;

                    SidearmSpawnChance = 0.0f;

                    FumbleMode = FumbleModeOptionsEnum.Never;
                    ReEquipOutOfCombat = false;
                    ReEquipBest = false;
                    ReEquipInCombat = false;
                    break;
                case SettingsPreset.Lite:
                    SeparateModes = true;
                    LimitModeSingleMelee = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingleMelee_AbsoluteMass = 0.6f;
                    LimitModeSingleRanged = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingleRanged_AbsoluteMass = 1.6f;
                    LimitModeAmountMelee = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmountMelee_Slots = 2;
                    LimitModeAmountRanged = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmountRanged_Slots = 2;
                    LimitModeAmountTotal = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmountTotal_Slots = 2;

                    SidearmSpawnChanceDropoff = 1.0f;
                    break;
                case SettingsPreset.Basic:
                    SeparateModes = true;
                    LimitModeSingleMelee = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingleMelee_AbsoluteMass = 1.9f;
                    LimitModeSingleRanged = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingleRanged_AbsoluteMass = 2.7f;
                    LimitModeAmountMelee = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmountMelee_Slots = 2;
                    LimitModeAmountRanged = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmountRanged_Slots = 2;
                    LimitModeAmountTotal = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmountTotal_Slots = 3;
                    break;
                case SettingsPreset.Advanced:
                    SeparateModes = true;
                    LimitModeSingleMelee = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingleMelee_AbsoluteMass = 2.25f;
                    LimitModeSingleRanged = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingleRanged_AbsoluteMass = 5.0f;
                    LimitModeAmountTotal = LimitModeAmountOfSidearms.AbsoluteWeight;
                    LimitModeAmountTotal_AbsoluteMass = 10;
                    break;
                case SettingsPreset.Excessive:
                    SidearmSpawnChance = 0.75f;
                    SidearmSpawnChanceDropoff = 0.5f;
                    SidearmBudgetMultiplier = 0.75f;
                    SidearmBudgetDropoff = 0.5f;
                    break;
                case SettingsPreset.Brawler:
                    SeparateModes = true;
                    LimitModeSingleMelee = LimitModeSingleSidearm.AbsoluteWeight;
                    LimitModeSingleMelee_AbsoluteMass = 4f;
                    LimitModeAmountMelee = LimitModeAmountOfSidearms.AbsoluteWeight;
                    LimitModeAmountMelee_AbsoluteMass = 10f;
                    LimitModeAmountRanged = LimitModeAmountOfSidearms.Slots;
                    LimitModeAmountRanged_Slots = 0;
                    break;
                default:
                    return;
            }

            RebuildCache(ref LimitModeSingle_Match_Cache, WeaponListKind.Both);
            RebuildCache(ref LimitModeSingleMelee_Match_Cache, WeaponListKind.Melee);
            RebuildCache(ref LimitModeSingleRanged_Match_Cache, WeaponListKind.Ranged);
        }

        private void Limits(Listing_Standard listing, WeaponListKind listType, Action onChange)
        {
            ref var limitModeSingle = ref LimitModeSingle;
            ref var limitModeSingle_Match_Cache = ref LimitModeSingle_Match_Cache;
            ref var limitModeSingle_AbsoluteMass = ref LimitModeSingle_AbsoluteMass;
            ref var limitModeSingle_RelativeMass = ref LimitModeSingle_RelativeMass;
            ref var limitModeSingle_Selection = ref LimitModeSingle_Selection;
            ref var limitModeAmount = ref LimitModeAmount;
            ref var limitModeAmount_AbsoluteMass = ref LimitModeAmount_AbsoluteMass;
            ref var limitModeAmount_RelativeMass = ref LimitModeAmount_RelativeMass;
            ref var limitModeAmount_Slots = ref LimitModeAmount_Slots;

            var limitModeSingleLabel = "LimitModeSingle_title";
            var limitModeSingleTooltip = "LimitModeSingle_desc";
            var limitModeAmountLabel = "LimitModeAmount_title";
            var limitModeAmountTooltip = "LimitModeAmount_desc";

            switch (listType)
            {
                case WeaponListKind.Both:
                    break;
                case WeaponListKind.Melee:
                    limitModeSingle = ref LimitModeSingleMelee;
                    limitModeSingle_Match_Cache = ref LimitModeSingleMelee_Match_Cache;
                    limitModeSingle_AbsoluteMass = ref LimitModeSingleMelee_AbsoluteMass;
                    limitModeSingle_RelativeMass = ref LimitModeSingleMelee_RelativeMass;
                    limitModeSingle_Selection = ref LimitModeSingleMelee_Selection;
                    limitModeAmount = ref LimitModeAmountMelee;
                    limitModeAmount_AbsoluteMass = ref LimitModeAmountMelee_AbsoluteMass;
                    limitModeAmount_RelativeMass = ref LimitModeAmountMelee_RelativeMass;
                    limitModeAmount_Slots = ref LimitModeAmountMelee_Slots;

                    limitModeSingleLabel = "LimitModeSingleMelee_title";
                    limitModeSingleTooltip = "LimitModeSingleMelee_desc";
                    limitModeAmountLabel = "LimitModeAmountMelee_title";
                    limitModeAmountTooltip = "LimitModeAmountMelee_desc";
                    break;
                case WeaponListKind.Ranged:
                    limitModeSingle = ref LimitModeSingleRanged;
                    limitModeSingle_Match_Cache = ref LimitModeSingleRanged_Match_Cache;
                    limitModeSingle_AbsoluteMass = ref LimitModeSingleRanged_AbsoluteMass;
                    limitModeSingle_RelativeMass = ref LimitModeSingleRanged_RelativeMass;
                    limitModeSingle_Selection = ref LimitModeSingleRanged_Selection;
                    limitModeAmount = ref LimitModeAmountRanged;
                    limitModeAmount_AbsoluteMass = ref LimitModeAmountRanged_AbsoluteMass;
                    limitModeAmount_RelativeMass = ref LimitModeAmountRanged_RelativeMass;
                    limitModeAmount_Slots = ref LimitModeAmountRanged_Slots;

                    limitModeSingleLabel = "LimitModeSingleRanged_title";
                    limitModeSingleTooltip = "LimitModeSingleRanged_desc";
                    limitModeAmountLabel = "LimitModeAmountRanged_title";
                    limitModeAmountTooltip = "LimitModeAmountRanged_desc";
                    break;
                default:
                    throw new ArgumentException();
            }

            listing.GapLine();
            {
                var valBefore = limitModeSingle;
                listing.EnumSelector(limitModeSingleLabel.Translate(), ref limitModeSingle, "LimitModeSingle_option_", valueTooltipPostfix: null, tooltip: limitModeSingleTooltip.Translate(), onChange: onChange);
                if (valBefore != limitModeSingle)
                    limitModeSingle_Match_Cache = null;
            }
            switch (limitModeSingle)
            {
                case LimitModeSingleSidearm.AbsoluteWeight:
                    {
                        var valBefore = limitModeSingle_AbsoluteMass;
                        listing.SliderLabeled("MaximumMassSingleAbsolute_title".Translate(), ref limitModeSingle_AbsoluteMass, 0, InferredValues.maxWeightTotal, decimalPlaces: 1, valueSuffix: " kg", onChange: onChange);
                        if (valBefore != limitModeSingle_AbsoluteMass || limitModeSingle_Match_Cache == null)
                            RebuildCache(ref limitModeSingle_Match_Cache, listType);
                        listing.WeaponList(limitModeSingle_Match_Cache);
                    }
                    break;
                case LimitModeSingleSidearm.RelativeWeight:
                    {
                        var valBefore = limitModeSingle_RelativeMass;
                        listing.SliderLabeled("MaximumMassSingleRelative_title".Translate(), ref limitModeSingle_RelativeMass, 0, 1, displayMult: 100, valueSuffix: "%", onChange: onChange);
                        if (valBefore != limitModeSingle_RelativeMass || limitModeSingle_Match_Cache == null)
                            RebuildCache(ref limitModeSingle_Match_Cache, listType);
                        Color save = GUI.color;
                        GUI.color = Color.gray;
                        listing.Label("MaximumMassAmountRelative_hint".Translate());
                        GUI.color = save;
                        listing.WeaponList(limitModeSingle_Match_Cache);
                    }
                    break;
                case LimitModeSingleSidearm.Selection:
                    {
                        var matchingSidearms = GettersFilters.filterForWeaponKind(GettersFilters.getValidWeaponsThingDefsOnly(), MiscUtils.LimitTypeToListType(listType));
                        listing.WeaponSelector(matchingSidearms, limitModeSingle_Selection, "ConsideredSidearms".Translate(), "NotConsideredSidearms".Translate(), onChange: onChange);
                    }
                    break;
                case LimitModeSingleSidearm.None:
                    break;
            }

            listing.GapLine();
            {
                listing.EnumSelector(limitModeAmountLabel.Translate(), ref limitModeAmount, "LimitModeAmount_option_", valueTooltipPostfix: null, tooltip: limitModeAmountTooltip.Translate(), onChange: onChange);
            }
            switch (limitModeAmount)
            {
                case LimitModeAmountOfSidearms.AbsoluteWeight:
                    listing.SliderLabeled("MaximumMassAmountAbsolute_title".Translate(), ref limitModeAmount_AbsoluteMass, 0, InferredValues.maxCapacity, decimalPlaces: 1, valueSuffix: " kg", onChange: onChange);
                    break;
                case LimitModeAmountOfSidearms.RelativeWeight:
                    listing.SliderLabeled("MaximumMassAmountRelative_title".Translate(), ref limitModeAmount_RelativeMass, 0, 1, displayMult: 100, valueSuffix: "%", onChange: onChange);
                    break;
                case LimitModeAmountOfSidearms.Slots:
                    listing.Spinner("MaximumSlots_title".Translate(), ref limitModeAmount_Slots, min: 1, tooltip: "MaximumSlots_desc".Translate(), onChange: onChange);
                    break;
                case LimitModeAmountOfSidearms.None:
                    break;
            }
        }

        private void RebuildCache(ref HashSet<ThingDef> cache, WeaponListKind listType)
        {
            IEnumerable<ThingDef> validSidearms = GettersFilters.getValidWeaponsThingDefsOnly();

            //Log.Message($"(list type: {listType}) valid weapons ({validSidearms.Count()}):{String.Join(", ", validSidearms.Select(w => w.defName))}");

            List<ThingDef> matchingSidearms = GettersFilters.filterForWeaponKind(validSidearms, MiscUtils.LimitTypeToListType(listType)).ToList();

            //Log.Message($"candidate weapons ({matchingSidearms.Count()}):{String.Join(", ", matchingSidearms.Select(w => w.defName))}");

            LimitModeSingleSidearm limitMode;
            float limitModeSingle_AbsoluteMass;
            float limitModeSingle_RelativeMass;
            switch (listType) 
            {
                case WeaponListKind.Both:
                    limitMode = LimitModeSingle;
                    limitModeSingle_AbsoluteMass = LimitModeSingle_AbsoluteMass;
                    limitModeSingle_RelativeMass = LimitModeSingle_RelativeMass;
                    break;
                case WeaponListKind.Melee:
                    limitMode = LimitModeSingleMelee;
                    limitModeSingle_AbsoluteMass = LimitModeSingleMelee_AbsoluteMass;
                    limitModeSingle_RelativeMass = LimitModeSingleMelee_RelativeMass;
                    break;
                case WeaponListKind.Ranged:
                    limitMode = LimitModeSingleRanged;
                    limitModeSingle_AbsoluteMass = LimitModeSingleRanged_AbsoluteMass;
                    limitModeSingle_RelativeMass = LimitModeSingleRanged_RelativeMass;
                    break;
                default:
                    throw new ArgumentException();
            }

            switch (limitMode)
            {
                case LimitModeSingleSidearm.AbsoluteWeight:
                    matchingSidearms = matchingSidearms.Where(w => w.GetStatValueAbstract(StatDefOf.Mass) <= limitModeSingle_AbsoluteMass).OrderBy(t => t.GetStatValueAbstract(StatDefOf.Mass)).ToList();
                    break;
                case LimitModeSingleSidearm.RelativeWeight:
                    matchingSidearms = matchingSidearms.Where(w => w.GetStatValueAbstract(StatDefOf.Mass) <= limitModeSingle_RelativeMass * InferredValues.maxCapacity).OrderBy(t => t.GetStatValueAbstract(StatDefOf.Mass)).ToList();
                    break;
                case LimitModeSingleSidearm.Selection:
                    matchingSidearms = LimitModeSingle_Selection.ToList();
                    break;
                case LimitModeSingleSidearm.None:
                    break;
            }

            //Log.Message($"(result weapons ({matchingSidearms.Count()}):{String.Join(", ", matchingSidearms.Select(w => w.defName))}");

            cache = matchingSidearms.ToHashSet();
        }
    }
}