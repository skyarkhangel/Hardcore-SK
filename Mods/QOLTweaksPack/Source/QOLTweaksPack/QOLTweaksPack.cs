using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using QOLTweaksPack.hugsLibSettings;
using QOLTweaksPack.rimworld;
using QOLTweaksPack.tweaks;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace QOLTweaksPack
{
    public class QOLTweaksPack : ModBase
    {
        public override string ModIdentifier { get { return "QOLTweaksPack"; } }

        public enum SurgeryEstimateMode { AlwaysAccurate, AccurateIfDoctor, AccurateIfGoodDoctor, AccurateIfAmazingDoctor, NeverAccurate }

        public static SavedData savedData;
         
        internal static SettingHandle<bool> ButcherSpace;
        internal static SettingHandle<bool> DeadlockProtection;

        internal static SettingHandle<bool> CookingEquivalency;
        internal static SettingHandle<MealSetHandler> MealSelection;

        internal static SettingHandle<bool> MoveOrder;
        internal static SettingHandle<bool> PickupDropOrders;

        internal static SettingHandle<bool> TradingStockpiles;

        internal static SettingHandle<bool> SurgeryEstimates;
        internal static SettingHandle<SurgeryEstimateMode> SurgeryEstimationMode;
        internal static SettingHandle<bool> SurgeryEstimateAccountForTraits;
        internal static SettingHandle<bool> BruiseWalkingOff;

        private static Color noHighlight = new Color(0, 0, 0, 0);
        private static Color highlight1 = new Color(0.5f, 0, 0, 0.1f);
        private static Color highlight2 = new Color(0, 0.5f, 0, 0.1f);
        private static Color highlight3 = new Color(0, 0, 0.5f, 0.1f);
        private static Color highlight4 = new Color(0.5f, 0, 0.5f, 0.1f);
        private static Color highlight5 = new Color(0.5f, 0.5f, 0, 0.1f);
        private static Color highlight6 = new Color(0, 0.5f, 0.5f, 0.1f);

        public override void DefsLoaded()
        {
            base.DefsLoaded(); 

            ButcherSpace = Settings.GetHandle<bool>("ButcherSpace", "ButcherSpace_title".Translate(), "ButcherSpace_desc".Translate(), true);
            DeadlockProtection = Settings.GetHandle<bool>("DeadlockProtection", "DeadlockProtection_title".Translate(), "DeadlockProtection_desc".Translate(), true);
            DeadlockProtection.VisibilityPredicate = delegate { return ButcherSpace.Value == true; };
            ButcherSpace.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(ButcherSpace, rect, highlight1); };
            DeadlockProtection.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(DeadlockProtection, rect, highlight1); };
             
            CookingEquivalency = Settings.GetHandle<bool>("CookingEquivalency", "CookingEquivalency_title".Translate(), "CookingEquivalency_desc".Translate(), true);
            CookingEquivalency.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(CookingEquivalency, rect, highlight2); };
            MealSelection = Settings.GetHandle<MealSetHandler>("MealSelection", "MealSelection_title".Translate(), "MealSelection_desc".Translate(), null);
            MealSelection.VisibilityPredicate = delegate { return CookingEquivalency.Value == true; }; 
            MealSelection.CustomDrawer = rect => { return SettingUIs.CustomDrawer_MatchingMeals_active(rect, MealSelection, highlight2, "SelectedMeals".Translate(), "UnselectedMeals".Translate()); };
            MealSelection.Value = new MealSetHandler();

            MoveOrder = Settings.GetHandle<bool>("MoveOrder", "MoveOrder_title".Translate(), "MoveOrder_desc".Translate(), true);
            MoveOrder.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(MoveOrder, rect, highlight3); };
            PickupDropOrders = Settings.GetHandle<bool>("PickupDropOrders", "PickupDropOrders_title".Translate(), "PickupDropOrders_desc".Translate(), true);
            PickupDropOrders.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(PickupDropOrders, rect, highlight3); };

            TradingStockpiles = Settings.GetHandle<bool>("TradingStockpiles", "TradingStockpiles_title".Translate(), "TradingStockpiles_desc".Translate(), true);
            TradingStockpiles.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(TradingStockpiles, rect, highlight4); };

            SurgeryEstimates = Settings.GetHandle<bool>("SurgeryEstimates", "SurgeryEstimates_title".Translate(), "SurgeryEstimates_desc".Translate(), true);
            SurgeryEstimates.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(SurgeryEstimates, rect, highlight5); };
            SurgeryEstimationMode = Settings.GetHandle<SurgeryEstimateMode>("SurgeryEstimationMode", "SurgeryEstimationMode_title".Translate(), "SurgeryEstimationMode_desc".Translate(), SurgeryEstimateMode.AccurateIfGoodDoctor, null, "SurgeryEstimationMode_option_");
            SurgeryEstimationMode.CustomDrawer = rect => {
                string[] names = Enum.GetNames(SurgeryEstimationMode.Value.GetType());
                float[] forcedWidths = new float[names.Length];
                return SettingUIs.CustomDrawer_Enumlist(SurgeryEstimationMode, rect, names, forcedWidths, true, SettingUIs.ExpansionMode.Vertical, highlight5);
                    };
            SurgeryEstimateAccountForTraits = Settings.GetHandle<bool>("SurgeryEstimateAccountForTraits", "SurgeryEstimateAccountForTraits_title".Translate(), "SurgeryEstimateAccountForTraits_desc".Translate(), true);
            SurgeryEstimateAccountForTraits.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(SurgeryEstimateAccountForTraits, rect, highlight5); };

            BruiseWalkingOff = Settings.GetHandle<bool>("BruiseWalkingOff", "BruiseWalkingOff_title".Translate(), "BruiseWalkingOff_desc".Translate(), true);
            BruiseWalkingOff.CustomDrawer = rect => { return SettingUIs.HugsDrawerRebuild_Checkbox(BruiseWalkingOff, rect, highlight5); };

        }

        public override void WorldLoaded()
        {
            savedData = UtilityWorldObjectManager.GetUtilityWorldObject<SavedData>();
        }

        public override void Update()
        {
            base.Update();
            if (GenScene.InPlayScene)
            {
                UIRoot_Play_UIRootOnGUI_Prefix.Validate();
            }
        }
    }
}
