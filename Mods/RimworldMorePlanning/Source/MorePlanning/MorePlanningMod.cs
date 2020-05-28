using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using MorePlanning.Designators;
using MorePlanning.Legacy;
using MorePlanning.Plan;
using MorePlanning.Settings;
using UnityEngine;
using Verse;
using ModSettings = MorePlanning.Settings.ModSettings;
using Resources = MorePlanning.Common.Resources;

namespace MorePlanning
{

    internal partial class MorePlanningMod : ModBase
    {
        private static MorePlanningMod _instance;
        public static MorePlanningMod Instance => _instance ?? (_instance = new MorePlanningMod());

        public const string Identifier = "com.github.alandariva.moreplanning";

        public int SelectedColor = 0;

        private static List<PlanDesignationDef> _planDesDefs = new List<PlanDesignationDef>();

        public WorldSettings WorldSettings;

        public ModSettings ModSettings;

        public static List<PlanDesignationDef> PlanDesDefs
        {
            get
            {
                if (_planDesDefs.Count == 0)
                {
                    LoadPlanDesDefs();
                }
                return _planDesDefs;
            }
        }

        public override string ModIdentifier => Identifier;

        public bool OverrideColors
        {
            get
            {
                return (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) == ModSettings.ShiftKeyForOverride;
            }
        }

        private MorePlanningMod()
        {
            _instance = this;
        }

        public override void DefsLoaded()
        {
            LoadPlanDesDefs();
            ModSettings = ModSettings.CreateModSettings(Settings);
            PlanColorManager.Load(Settings);

            SettingsChanged();

            DesignationCategoryDef desCatDef = DefDatabase<DesignationCategoryDef>.GetNamed("Planning");

            if (desCatDef == null)
                throw new Exception("Planning designation category not found");

            FieldInfo designatorsFi = typeof(DesignationCategoryDef).GetField("resolvedDesignators", BindingFlags.NonPublic | BindingFlags.Instance);
            var designators = designatorsFi.GetValue(desCatDef) as List<Designator>;

            for (int i = 0; i < PlanColorManager.NumPlans; i++)
            {
                designators.Add(new SelectColorDesignator(i));
            }
        }

        public static void LogError(string text)
        {
            Instance.Logger.Error(text);
        }

        public static void LogMessage(string text)
        {
            Instance.Logger.Message(text);
        }

        private void UpdatePlanningDefsSetting()
        {
            var planningDefs = DefDatabase<PlanDesignationDef>.AllDefs;
            foreach (var planningDef in planningDefs)
            {
                planningDef.removeIfBuildingDespawned = this.ModSettings.RemoveIfBuildingDespawned;
            }
        }

        public override void SettingsChanged()
        {
            UpdatePlanningDefsSetting();
            UpdatePlanOpacity();
        }

        private void UpdatePlanOpacity()
        {
            foreach (var mat in Resources.PlanMatColor)
            {
                Color color = mat.color;
                color.a = ModSettings.PlanOpacity / 100f;
                mat.color = color;
            }
        }

        public override void WorldLoaded()
        {
            WorldSettings = UtilityWorldObjectManager.GetUtilityWorldObject<WorldSettings>();
            
            // Fix compatibilities with older saves
            UpdateLegacy.Update();

            VisibilityDesignator.PlanningVisibility = WorldSettings.PlanningVisibility;
            OpacityDesignator.Opacity = ModSettings.PlanOpacity;
        }

        private static void LoadPlanDesDefs()
        {
            _planDesDefs.Clear();
            _planDesDefs.AddRange(DefDatabase<PlanDesignationDef>.AllDefsListForReading);

            Resources.PlanDesignationDef = DefDatabase<PlanDesignationDef>.GetNamed("Plan");
        }

    }
}
