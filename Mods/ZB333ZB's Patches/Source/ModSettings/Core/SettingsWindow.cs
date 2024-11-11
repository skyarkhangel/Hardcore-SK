using UnityEngine;
using Verse;
using RimWorld;

namespace ZB333ZB_Patches.Core
{
    public class SettingsWindow : Window
    {
        private readonly ModSettingsMod mod;
        private readonly ModSettingsData settings;
        private Vector2 scrollPosition = Vector2.zero;
        private const float buttonSize = 24f;
        private const float shortGapLineSize = 8f;
        private const float gapLineSize = 12f;
        private const float CloseButtonHeight = 35f;
        private const float TitleHeight = 45f;
        private string maxTooltipsWidthBuffer;

        public SettingsWindow(ModSettingsMod mod, ModSettingsData settings)
        {
            this.mod = mod;
            this.settings = settings;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = false;
            preventCameraMotion = false;
            draggable = true;
            resizeable = false;
            layer = WindowLayer.Dialog;
            this.settings.SaveCurrentState();
            maxTooltipsWidthBuffer = settings.maxTooltipsWidth.ToString();
        }

        public override Vector2 InitialSize => new(900f, 700f);

        private float GetTotalHeightAndSizes(out float generalSectionSize, out float wmSectionSize)
        {
            generalSectionSize = ShortGapLinesSize(1f) + ButtonsSize(7f) + GapLinesSize(1f);
            wmSectionSize = ShortGapLinesSize(1f) + ButtonsSize(3f) + GapLinesSize(1f);
            return generalSectionSize + wmSectionSize + shortGapLineSize + 50f;
        }

        private float ButtonsSize(float count) => buttonSize * count;
        private float ShortGapLinesSize(float count) => shortGapLineSize * count;
        private float GapLinesSize(float count) => gapLineSize * count;

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect titleRect = inRect.TopPartPixels(TitleHeight);
            Widgets.Label(titleRect, mod.SettingsCategory());

            Rect contentRect = new(inRect.x, inRect.y + TitleHeight, inRect.width,
                inRect.height - TitleHeight - CloseButtonHeight - 10f);

            Text.Font = GameFont.Small;
            float totalHeight = GetTotalHeightAndSizes(out float generalSectionSize, out float wmSectionSize);

            Rect viewRect = new(0f, 0f, contentRect.width - 30f, totalHeight);
            Widgets.BeginScrollView(contentRect, ref scrollPosition, viewRect);

            Listing_Standard listingStandard = new();
            listingStandard.Begin(viewRect);

            Listing_Standard section = listingStandard.BeginSection(generalSectionSize);

            section.Label("ZB333ZB.Settings.EnablePatches".Translate());
            section.GapLine(shortGapLineSize);

            if (section.ButtonTextLabeled(
                "ZB333ZB.Settings.Reset".Translate(),
                "ZB333ZB.Settings.Default".Translate()))
            {
                settings.ResetToDefaults();
                maxTooltipsWidthBuffer = settings.maxTooltipsWidth.ToString();
            }

            section.CheckboxLabeled(
                "ZB333ZB.Settings.ApplyToAllDoors".Translate(),
                ref settings.applyToAllDoorsEnabled,
                "ZB333ZB.Settings.ApplyToAllDoors.Tooltip".Translate());

            section.CheckboxLabeled(
                "ZB333ZB.Settings.AutoRebuildTerrain".Translate(),
                ref settings.autoRebuildTerrainEnabled,
                "ZB333ZB.Settings.AutoRebuildTerrain.Tooltip".Translate());

            section.CheckboxLabeled(
                "ZB333ZB.Settings.EnhancedDefSelector".Translate(),
                ref settings.enhancedDefSelectorEnabled,
                "ZB333ZB.Settings.EnhancedDefSelector.Tooltip".Translate());

            section.CheckboxLabeled(
                "ZB333ZB.Settings.StorageSelector".Translate(),
                ref settings.storageSelectorEnabled,
                "ZB333ZB.Settings.StorageSelector.Tooltip".Translate());

            section.CheckboxLabeled(
                "ZB333ZB.Settings.ToggleDBHGrids".Translate(),
                ref settings.toggleDBHGridsEnabled,
                "ZB333ZB.Settings.ToggleDBHGrids.Tooltip".Translate());

            section.CheckboxLabeled(
                "ZB333ZB.Settings.ToggleRimefellerGrids".Translate(),
                ref settings.toggleRimefellerGridsEnabled,
                "ZB333ZB.Settings.ToggleRimefellerGrids.Tooltip".Translate());

            listingStandard.EndSection(section);

            listingStandard.Gap(shortGapLineSize);

            section = listingStandard.BeginSection(wmSectionSize);

            section.Label("ZB333ZB.Settings.WhatsMissing".Translate());
            section.GapLine(shortGapLineSize);

            section.CheckboxLabeled(
                "ZB333ZB.Settings.WhatsMissing.HideZeroCountIngredients".Translate(),
                ref settings.hideZeroCountIngredients);

            section.TextFieldNumericLabeled(
                "ZB333ZB.Settings.WhatsMissing.MaxTooltipsWidth".Translate(),
                ref settings.maxTooltipsWidth,
                ref maxTooltipsWidthBuffer);

            listingStandard.EndSection(section);
            listingStandard.End();

            Widgets.EndScrollView();

            Rect closeButtonRect = new(
                (inRect.width - 120f) / 2f,
                inRect.height - CloseButtonHeight,
                120f,
                CloseButtonHeight);

            if (Widgets.ButtonText(closeButtonRect, "CloseButton".Translate()))
            {
                Close();
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            if (settings.SettingsChanged())
            {
                var dialog = new Dialog_MessageBox(
                    "ZB333ZB.Settings.RestartRequiredDesc".Translate(),
                    "ZB333ZB.Settings.DontRestart".Translate(),
                    () =>
                    {
                        settings.RestorePreviousState();
                        base.Close(doCloseSound);
                    },
                    "ZB333ZB.Settings.Restart".Translate(),
                    () =>
                    {
                        settings.Write();
                        GenCommandLine.Restart();
                    },
                    null,
                    false)
                {
                    buttonADestructive = false,
                    buttonCText = "ZB333ZB.Settings.Cancel".Translate(),
                    buttonCAction = () => { }
                };

                Find.WindowStack.Add(dialog);
                return;
            }

            base.Close(doCloseSound);
        }
    }
}
