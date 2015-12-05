using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorldIconMod
{
    class Dialog_IconModSettings : Layer_Window
    {

        public string page = "main";
        
        
        public Layer_Window optionsDialog = null;
        public bool closeButtonClicked = true;
        
    public Dialog_IconModSettings()
    {
      this.drawPriority = LayerDrawPriorities.GameDialog;
      this.closeOnEscapeKey = false;
      this.doCloseButton = false;
      this.doCloseX = true;
      this.absorbAllInput = false;
      this.forcePause = false;
      this.reorderable = false;
      
    }

    private void DoHeading(Listing_Standard listing,String translator_key, bool translate = true)
    {
        Text.Font = GameFont.Medium;
        listing.DoLabel(translate?Translator.Translate(translator_key):translator_key);
        Text.Font = GameFont.Small;
    }

    protected void fillPageMain(Listing_Standard listing)
    {        
        //listing.DoHeading("General settings");

        if (listing.DoTextButton( Translator.Translate("PSI.Settings.IconSet") + PSI.settings.iconSet))
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (string setname in PSI.iconSets)
            {
                options.Add(new FloatMenuOption(setname, () =>
                {
                    PSI.settings.iconSet = setname;                    
                    PSI.materials = new Materials(setname);
                    PSI.materials.reloadTextures(true);
                }));
            }
            Find.LayerStack.Add((Layer)new Layer_FloatMenu(options, false));
        }

        if (listing.DoTextButton(Translator.Translate("PSI.Settings.LoadPresetButton")))
        {
            string[] presetList = {};            
            String path2 = GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Presets/Complete/";
            if (Directory.Exists(path2)) presetList = Directory.GetFiles(path2, "*.cfg");            

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (string setname in presetList)
            {
                options.Add(new FloatMenuOption(Path.GetFileNameWithoutExtension(setname), () =>
                {
                    try
                    {
                        PSI.settings = XmlLoader.ItemFromXmlFile<ModSettings>(setname, true);
                        PSI.saveSettings();
                        PSI.reinit();
                    }
                    catch (IOException) { Log.Error(Translator.Translate("PSI.Settings.LoadPreset.UnableToLoad") + setname); }
                }));
            }
            Find.LayerStack.Add((Layer)new Layer_FloatMenu(options, false));
        }

        listing.DoGap();        
        DoHeading(listing,"PSI.Settings.Advanced");

        if (listing.DoTextButton(Translator.Translate("PSI.Settings.VisibilityButton"))) page = "showhide";
        if (listing.DoTextButton(Translator.Translate("PSI.Settings.ArrangementButton"))) page = "arrange";
        if (listing.DoTextButton(Translator.Translate("PSI.Settings.SensitivityButton"))) page = "limits";
        
    }

    protected void fillPageLimits(Listing_Standard listing)
    {
        DoHeading(listing, "PSI.Settings.Sensitivity.Header");

        if (listing.DoTextButton(Translator.Translate("PSI.Settings.LoadPresetButton")))
        {
            string[] presetList = { };
            String path2 = GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Presets/Sensitivity/";
            if (Directory.Exists(path2)) presetList = Directory.GetFiles(path2, "*.cfg"); 

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (string setname in presetList)
            {
                options.Add(new FloatMenuOption(Path.GetFileNameWithoutExtension(setname), () =>
                {
                    try
                    {
                        ModSettings settings = XmlLoader.ItemFromXmlFile<ModSettings>(setname, true);
                        PSI.settings.limit_bleedMult = settings.limit_bleedMult;
                        PSI.settings.limit_diseaseLess = settings.limit_diseaseLess;
                        PSI.settings.limit_EfficiencyLess = settings.limit_EfficiencyLess;
                        PSI.settings.limit_FoodLess = settings.limit_FoodLess;
                        PSI.settings.limit_MoodLess = settings.limit_MoodLess;
                        PSI.settings.limit_RestLess = settings.limit_RestLess;
                        PSI.settings.limit_apparelHealthLess = settings.limit_apparelHealthLess;
                        PSI.settings.limit_tempComfortOffset = settings.limit_tempComfortOffset;                        
                    }
                    catch (IOException) { Log.Error(Translator.Translate("PSI.Settings.LoadPreset.UnableToLoad") + setname); }
                }));
            }
            Find.LayerStack.Add((Layer)new Layer_FloatMenu(options, false));
        }

        listing.DoGap();

        listing.DoLabel(Translator.Translate("PSI.Settings.Sensitivity.Bleeding") + Translator.Translate("PSI.Settings.Sensitivity.Bleeding." + Math.Round( PSI.settings.limit_bleedMult - 0.25 )));
        PSI.settings.limit_bleedMult = listing.DoSlider(PSI.settings.limit_bleedMult, 0.5f, 5.0f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Sensitivity.Injured") + ((int)(PSI.settings.limit_EfficiencyLess * 100)) + "%");
        PSI.settings.limit_EfficiencyLess = listing.DoSlider(PSI.settings.limit_EfficiencyLess, 0.01f, 0.99f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Sensitivity.Food") + ((int)(PSI.settings.limit_FoodLess * 100)) + "%");
        PSI.settings.limit_FoodLess = listing.DoSlider(PSI.settings.limit_FoodLess, 0.01f, 0.99f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Sensitivity.Mood") + ((int)(PSI.settings.limit_MoodLess * 100)) + "%");
        PSI.settings.limit_MoodLess = listing.DoSlider(PSI.settings.limit_MoodLess, 0.01f, 0.99f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Sensitivity.Rest") + ((int)(PSI.settings.limit_RestLess * 100)) + "%");
        PSI.settings.limit_RestLess = listing.DoSlider(PSI.settings.limit_RestLess, 0.01f, 0.99f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Sensitivity.ApparelHealth") + ((int)(PSI.settings.limit_apparelHealthLess * 100)) + "%");
        PSI.settings.limit_apparelHealthLess = listing.DoSlider(PSI.settings.limit_apparelHealthLess, 0.01f, 0.99f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Sensitivity.Temperature") + ((int)(PSI.settings.limit_tempComfortOffset)) + "C");
        PSI.settings.limit_tempComfortOffset = listing.DoSlider(PSI.settings.limit_tempComfortOffset, -10f, 10f);

        if (listing.DoTextButton(Translator.Translate("PSI.Settings.ReturnButton"))) page = "main";
    }

    protected void fillPageShowHide(Listing_Standard listing)
    {

        listing.OverrideColumnWidth = 230;
        DoHeading(listing, "PSI.Settings.Visibility.Header");
        listing.OverrideColumnWidth = 95;

        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.TargetPoint"), ref PSI.settings.showTargetPoint);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Aggressive"), ref PSI.settings.showAggressive);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Dazed"), ref PSI.settings.showDazed);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Leave"), ref PSI.settings.showLeave);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Draft"), ref PSI.settings.showDraft);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Idle"), ref PSI.settings.showIdle);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Unarmed"), ref PSI.settings.showUnarmed);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Hungry"), ref PSI.settings.showHungry);      
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Sad"), ref PSI.settings.showSad);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Tired"), ref PSI.settings.showTired);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Disease"), ref PSI.settings.showDisease);

        listing.OverrideColumnWidth = 230;

        if (listing.DoTextButton(Translator.Translate("PSI.Settings.ReturnButton"))) page = "main";
        listing.OverrideColumnWidth = 95;
        listing.NewColumn();

        DoHeading(listing, " ", false);
        DoHeading(listing, " ", false);

        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Injury"), ref PSI.settings.showEffectiveness);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Bloodloss"), ref PSI.settings.showBloodloss);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Hot"), ref PSI.settings.showHot);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Cold"), ref PSI.settings.showCold);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Naked"), ref PSI.settings.showNaked);

        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Drunk"), ref PSI.settings.showDrunk);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.ApparelHealth"), ref PSI.settings.showApparelHealth);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Visibility.Pacific"), ref PSI.settings.showPacific);
        
    }

    protected void fillPageArrangement(Listing_Standard listing)
    {
        DoHeading(listing, "PSI.Settings.Arrangement.Header");

        if (listing.DoTextButton(Translator.Translate("PSI.Settings.LoadPresetButton")))
        {
            string[] presetList = { };
            String path2 = GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Presets/Position/";
            if (Directory.Exists(path2)) presetList = Directory.GetFiles(path2, "*.cfg"); 

            List<FloatMenuOption> options = new List<FloatMenuOption>();
            foreach (string setname in presetList)
            {
                options.Add(new FloatMenuOption(Path.GetFileNameWithoutExtension(setname), () =>
                {
                    try
                    {
                        ModSettings settings = XmlLoader.ItemFromXmlFile<ModSettings>(setname, true);
                        PSI.settings.iconDistanceX = settings.iconDistanceX;
                        PSI.settings.iconDistanceY = settings.iconDistanceY;
                        PSI.settings.iconOffsetX = settings.iconOffsetX;
                        PSI.settings.iconOffsetY = settings.iconOffsetY;
                        PSI.settings.iconsHorizontal = settings.iconsHorizontal;
                        PSI.settings.iconsScreenScale = settings.iconsScreenScale;
                        PSI.settings.iconsInColumn = settings.iconsInColumn;
                        PSI.settings.iconSize = settings.iconSize;                        
                    }
                    catch (IOException) { Log.Error(Translator.Translate("PSI.Settings.LoadPreset.UnableToLoad") + setname); }
                }));
            }
            Find.LayerStack.Add((Layer)new Layer_FloatMenu(options, false));
        }

        int labelNum = (int)(PSI.settings.iconSize * 4.5f);
        if (labelNum > 8) labelNum = 8;
        else if (labelNum < 0) labelNum = 0;

        listing.DoLabel(Translator.Translate("PSI.Settings.Arrangement.IconSize") + Translator.Translate("PSI.Settings.SizeLabel."+labelNum));
        PSI.settings.iconSize = listing.DoSlider(PSI.settings.iconSize, 0.5f, 2.0f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Arrangement.IconPosition") + (int)(PSI.settings.iconDistanceX * 100) + " , " + (int)(PSI.settings.iconDistanceY * 100));
        PSI.settings.iconDistanceX = listing.DoSlider(PSI.settings.iconDistanceX, -2.0f, 2.0f);
        PSI.settings.iconDistanceY = listing.DoSlider(PSI.settings.iconDistanceY, -2.0f, 2.0f);

        listing.DoLabel(Translator.Translate("PSI.Settings.Arrangement.IconOffset") + (int)(PSI.settings.iconOffsetX * 100) + " , " + (int)(PSI.settings.iconOffsetY * 100));
        PSI.settings.iconOffsetX = listing.DoSlider(PSI.settings.iconOffsetX, -2.0f, 2.0f);
        PSI.settings.iconOffsetY = listing.DoSlider(PSI.settings.iconOffsetY, -2.0f, 2.0f);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Arrangement.Horizontal"), ref PSI.settings.iconsHorizontal);
        listing.DoLabelCheckbox(Translator.Translate("PSI.Settings.Arrangement.ScreenScale"), ref PSI.settings.iconsScreenScale);

        listing.DoLabel(Translator.Translate("PSI.Settings.Arrangement.IconsPerColumn") + PSI.settings.iconsInColumn);
        PSI.settings.iconsInColumn = (int)listing.DoSlider(PSI.settings.iconsInColumn, 1.0f, 9.0f);

        if (listing.DoTextButton(Translator.Translate("PSI.Settings.ReturnButton"))) page = "main";
    }

        protected override void FillWindow(UnityEngine.Rect inRect)
        {
            if (optionsDialog == null) return;
            //base.FillWindow(inRect);

            Rect optionsRect = optionsDialog.winRect;
            this.winRect = new Rect(optionsRect.xMax - 240, optionsRect.yMin, 240, optionsRect.height);
            Listing_Standard listing = new Listing_Standard(inRect);

            DoHeading(listing, "Pawn State Icons", false);

            listing.OverrideColumnWidth = this.winRect.width;

            //if (page == "main") fillPageMain(listing);
            if (page == "showhide") fillPageShowHide(listing);
            else if (page == "arrange") fillPageArrangement(listing);
            else if (page == "limits") fillPageLimits(listing);
            else fillPageMain(listing);
                
            listing.End();
        }

        public override void PreRemove()
        {
            PSI.saveSettings();
            PSI.reinit();
            closeButtonClicked = true;
            base.PreRemove();
            
        }
    }
}
