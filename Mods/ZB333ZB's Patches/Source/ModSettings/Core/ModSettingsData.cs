using Verse;

namespace ZB333ZB_Patches
{
    public class ModSettingsData : ModSettings
    {
        public bool applyToAllDoorsEnabled = true;
        public bool autoRebuildTerrainEnabled = true;
        public bool enhancedDefSelectorEnabled = true;
        public bool storageSelectorEnabled = false;
        public bool toggleDBHGridsEnabled = true;
        public bool toggleRimefellerGridsEnabled = true;

        public bool hideZeroCountIngredients = true;
        public int maxTooltipsWidth = 260;

        private bool prevApplyToAllDoorsEnabled;
        private bool prevAutoRebuildTerrainEnabled;
        private bool prevEnhancedDefSelectorEnabled;
        private bool prevStorageSelectorEnabled;
        private bool prevToggleDBHGridsEnabled;
        private bool prevToggleRimefellerGridsEnabled;
        private bool prevHideZeroCountIngredients;
        private int prevMaxTooltipsWidth;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref applyToAllDoorsEnabled, "applyToAllDoorsEnabled", true);
            Scribe_Values.Look(ref autoRebuildTerrainEnabled, "autoRebuildTerrainEnabled", true);
            Scribe_Values.Look(ref enhancedDefSelectorEnabled, "enhancedDefSelectorEnabled", true);
            Scribe_Values.Look(ref storageSelectorEnabled, "storageSelectorEnabled", false);
            Scribe_Values.Look(ref toggleDBHGridsEnabled, "toggleDBHGridsEnabled", true);
            Scribe_Values.Look(ref toggleRimefellerGridsEnabled, "toggleRimefellerGridsEnabled", true);
            Scribe_Values.Look(ref hideZeroCountIngredients, "hideZeroCountIngredients", true);
            Scribe_Values.Look(ref maxTooltipsWidth, "maxTooltipsWidth", 260);
        }

        public void SaveCurrentState()
        {
            prevApplyToAllDoorsEnabled = applyToAllDoorsEnabled;
            prevAutoRebuildTerrainEnabled = autoRebuildTerrainEnabled;
            prevEnhancedDefSelectorEnabled = enhancedDefSelectorEnabled;
            prevStorageSelectorEnabled = storageSelectorEnabled;
            prevToggleDBHGridsEnabled = toggleDBHGridsEnabled;
            prevToggleRimefellerGridsEnabled = toggleRimefellerGridsEnabled;
            prevHideZeroCountIngredients = hideZeroCountIngredients;
            prevMaxTooltipsWidth = maxTooltipsWidth;
        }

        public void RestorePreviousState()
        {
            applyToAllDoorsEnabled = prevApplyToAllDoorsEnabled;
            autoRebuildTerrainEnabled = prevAutoRebuildTerrainEnabled;
            enhancedDefSelectorEnabled = prevEnhancedDefSelectorEnabled;
            storageSelectorEnabled = prevStorageSelectorEnabled;
            toggleDBHGridsEnabled = prevToggleDBHGridsEnabled;
            toggleRimefellerGridsEnabled = prevToggleRimefellerGridsEnabled;
            hideZeroCountIngredients = prevHideZeroCountIngredients;
            maxTooltipsWidth = prevMaxTooltipsWidth;
            Write();
        }

        public bool SettingsChanged()
        {
            return prevApplyToAllDoorsEnabled != applyToAllDoorsEnabled
                || prevAutoRebuildTerrainEnabled != autoRebuildTerrainEnabled
                || prevEnhancedDefSelectorEnabled != enhancedDefSelectorEnabled
                || prevStorageSelectorEnabled != storageSelectorEnabled
                || prevToggleDBHGridsEnabled != toggleDBHGridsEnabled
                || prevToggleRimefellerGridsEnabled != toggleRimefellerGridsEnabled
                || prevHideZeroCountIngredients != hideZeroCountIngredients
                || prevMaxTooltipsWidth != maxTooltipsWidth;
        }

        public void ResetToDefaults()
        {
            applyToAllDoorsEnabled = true;
            autoRebuildTerrainEnabled = true;
            enhancedDefSelectorEnabled = true;
            storageSelectorEnabled = false;
            toggleDBHGridsEnabled = true;
            toggleRimefellerGridsEnabled = true;
            hideZeroCountIngredients = true;
            maxTooltipsWidth = 260;
        }
    }
}
