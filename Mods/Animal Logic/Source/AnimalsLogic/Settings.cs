using UnityEngine;
using Verse;
using System;

namespace AnimalsLogic
{
    class Settings : ModSettings
    {
        public static bool prevent_eating_stuff = true;
        public static bool hostile_predators = true;
        public static bool hostile_vermins = true;
        public static bool convert_ruined_eggs = true;
        public static bool tastes_like_chicken = false;
        public static bool medical_alerts = true;
        public static bool trade_tags = true;
        public static bool use_dispenser = true;
        public static bool extra_display_stats = true;
        public static bool always_show_relations = false;
        public static bool taming_age_factor = true;

        public static float wildness_threshold_for_tameness_decay = 0.101f;
        public static float training_decay_factor = 1.0f;
        public static float haul_mtb = 1.5f;
        public static float toxic_buildup_rot = 1.0f;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);

            listing_Standard.CheckboxLabeled("ALConfigCuriosityEatingLabel".Translate(), ref prevent_eating_stuff, "ALConfigCuriosityEatingTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigHostilePredatorsLabel".Translate(), ref hostile_predators, "ALConfigHostilePredatorsTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigEggConversionLabel".Translate(), ref convert_ruined_eggs, "ALConfigEggConversionTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigMeatConversionLabel".Translate(), ref tastes_like_chicken, "ALConfigMeatConversionTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigMedicalAlertsLabel".Translate(), ref medical_alerts, "ALConfigMedicalAlertsTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigTradeHintsLabel".Translate(), ref trade_tags, "ALConfigTradeHintsTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigAnimalsCanUseFoodDispenserLabel".Translate(), ref use_dispenser, "ALConfigAnimalsCanUseFoodDispenserTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigShowExtraDisplayStatsLabel".Translate(), ref extra_display_stats, "ALConfigShowExtraDisplayStatsTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigAlwaysShowRelationsLabel".Translate(), ref always_show_relations, "ALConfigAlwaysShowRelationsTooltip".Translate());
            listing_Standard.CheckboxLabeled("ALConfigTamingAgeFactorLabel".Translate(), ref taming_age_factor, "ALConfigTamingAgeFactorTooltip".Translate());

            listing_Standard.Label("ALConfigTamenessDecayThresholdLabel".Translate(((float)Math.Round(wildness_threshold_for_tameness_decay, 3) * 100).ToString()), tooltip: "ALConfigTamenessDecayThresholdTooltip".Translate());
            wildness_threshold_for_tameness_decay = listing_Standard.Slider(wildness_threshold_for_tameness_decay, 0f, 1f);

            listing_Standard.Label("ALConfigTrainingDecaySpeedFactorLabel".Translate(((float)Math.Round(training_decay_factor, 3)).ToStringPercent()), tooltip: "ALConfigTrainingDecaySpeedFactorTooltip".Translate());
            training_decay_factor = listing_Standard.Slider(training_decay_factor, 0.01f, 2f);
            
            listing_Standard.Label("ALConfigHaulingMTBLabel".Translate(((float)Math.Round(Math.Round(haul_mtb * 4) / 4f, 2))), tooltip: "ALConfigHaulingMTBTooltip".Translate());
            haul_mtb = listing_Standard.Slider(haul_mtb, 0.0f, 3f);

            listing_Standard.Label("ALConfigToxicBuildupRotLabel".Translate(((float)Math.Round(toxic_buildup_rot, 3)).ToStringPercent()), tooltip: "ALConfigToxicBuildupRotTooltip".Translate());
            toxic_buildup_rot = listing_Standard.Slider(toxic_buildup_rot, 0.00f, 1f);

            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref prevent_eating_stuff, "prevent_eating_stuff", true, false);
            Scribe_Values.Look<bool>(ref hostile_predators, "hostile_predators", true, false);
            Scribe_Values.Look<bool>(ref convert_ruined_eggs, "convert_ruined_eggs", true, false);
            Scribe_Values.Look<bool>(ref tastes_like_chicken, "tastes_like_chicken", false, false);
            Scribe_Values.Look<bool>(ref medical_alerts, "medical_alerts", true, false);
            Scribe_Values.Look<bool>(ref trade_tags, "trade_tags", true, false);
            Scribe_Values.Look<bool>(ref use_dispenser, "use_dispenser", true, false);
            Scribe_Values.Look<bool>(ref extra_display_stats, "extra_display_stats", true, false);
            Scribe_Values.Look<bool>(ref always_show_relations, "always_show_relations", false, false);
            Scribe_Values.Look<bool>(ref taming_age_factor, "taming_age_factor", true, false);

            Scribe_Values.Look<float>(ref wildness_threshold_for_tameness_decay, "wildness_threshold_for_tameness_decay", 0.101f, false);
            Scribe_Values.Look<float>(ref training_decay_factor, "training_decay_factor", 1.0f, false);
            Scribe_Values.Look<float>(ref haul_mtb, "haul_mtb", 1.5f, false);
            Scribe_Values.Look<float>(ref toxic_buildup_rot, "toxic_buildup_rot", 1.0f, false);
        }
    }
}
