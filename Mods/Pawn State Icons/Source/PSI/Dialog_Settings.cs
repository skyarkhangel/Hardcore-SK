using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace PSI
{
    // ReSharper disable once InconsistentNaming
    internal class Dialog_Settings : Window
    {
        public string Page = "main";
        public bool CloseButtonClicked = true;
        public Window OptionsDialog;

        public Dialog_Settings()
        {
            closeOnEscapeKey = false;
            doCloseButton = false;
            doCloseX = true;
            absorbInputAroundWindow = false;
            forcePause = false;
        }

        private void DoHeading(Listing_Standard listing, string translatorKey, bool translate = true)
        {
            Text.Font = GameFont.Medium;
            listing.DoLabel(translate ? translatorKey.Translate() : translatorKey);
            Text.Font = GameFont.Small;
        }

        private void FillPageMain(Listing_Standard listing)
        {
            if (listing.DoTextButton("PSI.Settings.IconSet".Translate() + PSI.Settings.IconSet))
            {
                var options = new List<FloatMenuOption>();
                foreach (var str in PSI.IconSets)
                {
                    var setname = str;
                    options.Add(new FloatMenuOption(setname, () =>
                    {
                        PSI.Settings.IconSet = setname;
                        PSI.Materials = new Materials(setname);
                        PSI.Materials.ReloadTextures(true);
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            if (listing.DoTextButton("PSI.Settings.LoadPresetButton".Translate()))
            {
                var strArray = new string[0];
                var path = GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Presets/Complete/";
                if (Directory.Exists(path))
                    strArray = Directory.GetFiles(path, "*.cfg");
                var options = new List<FloatMenuOption>();
                foreach (var str in strArray)
                {
                    var setname = str;
                    options.Add(new FloatMenuOption(Path.GetFileNameWithoutExtension(setname), () =>
                    {
                        try
                        {
                            PSI.Settings = XmlLoader.ItemFromXmlFile<ModSettings>(setname);
                            PSI.SaveSettings();
                            PSI.Reinit();
                        }
                        catch (IOException)
                        {
                            Log.Error("PSI.Settings.LoadPreset.UnableToLoad".Translate() + setname);
                        }
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            listing.DoGap();

            DoHeading(listing, "PSI.Settings.Advanced");

            if (listing.DoTextButton("PSI.Settings.VisibilityButton".Translate()))
                Page = "showhide";

            if (listing.DoTextButton("PSI.Settings.OpacityAndColorButton".Translate()))
                Page = "opacityandcolor";

            if (listing.DoTextButton("PSI.Settings.ArrangementButton".Translate()))
                Page = "arrange";

            if (!listing.DoTextButton("PSI.Settings.SensitivityButton".Translate()))
                return;

            Page = "limits";
        }

        private void FillPageLimits(Listing_Standard listing)
        {
            DoHeading(listing, "PSI.Settings.Sensitivity.Header");
            if (listing.DoTextButton("PSI.Settings.LoadPresetButton".Translate()))
            {
                var strArray = new string[0];
                var path = GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Presets/Sensitivity/";
                if (Directory.Exists(path))
                    strArray = Directory.GetFiles(path, "*.cfg");
                var options = new List<FloatMenuOption>();
                foreach (var str in strArray)
                {
                    var setname = str;
                    options.Add(new FloatMenuOption(Path.GetFileNameWithoutExtension(setname), () =>
                    {
                        try
                        {
                            var settings = XmlLoader.ItemFromXmlFile<ModSettings>(setname);
                            PSI.Settings.LimitBleedMult = settings.LimitBleedMult;
                            PSI.Settings.LimitDiseaseLess = settings.LimitDiseaseLess;
                            PSI.Settings.LimitEfficiencyLess = settings.LimitEfficiencyLess;
                            PSI.Settings.LimitFoodLess = settings.LimitFoodLess;
                            PSI.Settings.LimitMoodLess = settings.LimitMoodLess;
                            PSI.Settings.LimitRestLess = settings.LimitRestLess;
                            PSI.Settings.LimitApparelHealthLess = settings.LimitApparelHealthLess;
                            PSI.Settings.LimitTempComfortOffset = settings.LimitTempComfortOffset;
                        }
                        catch (IOException)
                        {
                            Log.Error("PSI.Settings.LoadPreset.UnableToLoad".Translate() + setname);
                        }
                    }));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }

            listing.DoGap();

            listing.DoLabel("PSI.Settings.Sensitivity.Bleeding".Translate() + ("PSI.Settings.Sensitivity.Bleeding." + Math.Round(PSI.Settings.LimitBleedMult - 0.25)).Translate());
            PSI.Settings.LimitBleedMult = listing.DoSlider(PSI.Settings.LimitBleedMult, 0.5f, 5f);

            listing.DoLabel("PSI.Settings.Sensitivity.Injured".Translate() + (int)(PSI.Settings.LimitEfficiencyLess * 100.0) + "%");
            PSI.Settings.LimitEfficiencyLess = listing.DoSlider(PSI.Settings.LimitEfficiencyLess, 0.01f, 0.99f);

            listing.DoLabel("PSI.Settings.Sensitivity.Food".Translate() + (int)(PSI.Settings.LimitFoodLess * 100.0) + "%");
            PSI.Settings.LimitFoodLess = listing.DoSlider(PSI.Settings.LimitFoodLess, 0.01f, 0.99f);

            listing.DoLabel("PSI.Settings.Sensitivity.Mood".Translate() + (int)(PSI.Settings.LimitMoodLess * 100.0) + "%");
            PSI.Settings.LimitMoodLess = listing.DoSlider(PSI.Settings.LimitMoodLess, 0.01f, 0.99f);

            listing.DoLabel("PSI.Settings.Sensitivity.Rest".Translate() + (int)(PSI.Settings.LimitRestLess * 100.0) + "%");
            PSI.Settings.LimitRestLess = listing.DoSlider(PSI.Settings.LimitRestLess, 0.01f, 0.99f);

            listing.DoLabel("PSI.Settings.Sensitivity.ApparelHealth".Translate() + (int)(PSI.Settings.LimitApparelHealthLess * 100.0) + "%");
            PSI.Settings.LimitApparelHealthLess = listing.DoSlider(PSI.Settings.LimitApparelHealthLess, 0.01f, 0.99f);

            listing.DoLabel("PSI.Settings.Sensitivity.Temperature".Translate() + (int)PSI.Settings.LimitTempComfortOffset + "C");
            PSI.Settings.LimitTempComfortOffset = listing.DoSlider(PSI.Settings.LimitTempComfortOffset, -10f, 10f);

            if (!listing.DoTextButton("PSI.Settings.ReturnButton".Translate()))
                return;

            Page = "main";
        }

        private void FillPageOpacityAndColor(Listing_Standard listing)
        {
            DoHeading(listing, "PSI.Settings.IconOpacityAndColor.Header");
            listing.DoLabel("PSI.Settings.IconOpacityAndColor.Opacity".Translate());
            PSI.Settings.IconOpacity = listing.DoSlider(PSI.Settings.IconOpacity, 0.05f, 1f);

            listing.DoLabel("PSI.Settings.IconOpacityAndColor.OpacityCritical".Translate());
            PSI.Settings.IconOpacityCritical = listing.DoSlider(PSI.Settings.IconOpacityCritical, 0f, 1f);

            listing.DoLabelCheckbox("PSI.Settings.IconOpacityAndColor.UseColoredTarget".Translate(), ref PSI.Settings.UseColoredTarget);


            listing.DoLabel("Custom color settings coming from CCL in future");

            if (listing.DoTextButton("PSI.Settings.ReturnButton".Translate()))
                Page = "main";
        }

        private void FillPageShowHide(Listing_Standard listing)
        {
            listing.OverrideColumnWidth = 230f;
            DoHeading(listing, "PSI.Settings.Visibility.Header");
            listing.OverrideColumnWidth = 95f;
            listing.DoLabelCheckbox("PSI.Settings.Visibility.TargetPoint".Translate(), ref PSI.Settings.ShowTargetPoint);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Aggressive".Translate(), ref PSI.Settings.ShowAggressive);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Dazed".Translate(), ref PSI.Settings.ShowDazed);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Leave".Translate(), ref PSI.Settings.ShowLeave);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Draft".Translate(), ref PSI.Settings.ShowDraft);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Idle".Translate(), ref PSI.Settings.ShowIdle);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Unarmed".Translate(), ref PSI.Settings.ShowUnarmed);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Hungry".Translate(), ref PSI.Settings.ShowHungry);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Sad".Translate(), ref PSI.Settings.ShowSad);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Tired".Translate(), ref PSI.Settings.ShowTired);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Sickness".Translate(), ref PSI.Settings.ShowDisease);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.NightOwl".Translate(), ref PSI.Settings.ShowNightOwl);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Greedy".Translate(), ref PSI.Settings.ShowGreedy);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Jealous".Translate(), ref PSI.Settings.ShowJealous);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Lovers".Translate(), ref PSI.Settings.ShowLovers);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Prosthophile".Translate(), ref PSI.Settings.ShowProsthophile);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Prosthophobe".Translate(), ref PSI.Settings.ShowProsthophobe);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.RoomStatus".Translate(), ref PSI.Settings.ShowRoomStatus);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Bedroom".Translate(), ref PSI.Settings.ShowBedroom);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Pain".Translate(), ref PSI.Settings.ShowPain);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Hralth".Translate(), ref PSI.Settings.ShowHealth);

            // to do: sort, cleanup + translation

            listing.OverrideColumnWidth = 230f;
            if (listing.DoTextButton("PSI.Settings.ReturnButton".Translate()))
                Page = "main";
            listing.OverrideColumnWidth = 95f;
            listing.NewColumn();
            DoHeading(listing, " ", false);
            DoHeading(listing, " ", false);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Injury".Translate(), ref PSI.Settings.ShowEffectiveness);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Bloodloss".Translate(), ref PSI.Settings.ShowBloodloss);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Hot".Translate(), ref PSI.Settings.ShowHot);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Cold".Translate(), ref PSI.Settings.ShowCold);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Naked".Translate(), ref PSI.Settings.ShowNaked);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Drunk".Translate(), ref PSI.Settings.ShowDrunk);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.ApparelHealth".Translate(), ref PSI.Settings.ShowApparelHealth);
            listing.DoLabelCheckbox("PSI.Settings.Visibility.Pacific".Translate(), ref PSI.Settings.ShowPacific);
        }

        private void FillPageArrangement(Listing_Standard listing)
        {
            DoHeading(listing, "PSI.Settings.Arrangement.Header");

            if (listing.DoTextButton("PSI.Settings.LoadPresetButton".Translate()))
            {
                var strArray = new string[0];
                var path = GenFilePaths.CoreModsFolderPath + "/Pawn State Icons/Presets/Position/";
                if (Directory.Exists(path))
                    strArray = Directory.GetFiles(path, "*.cfg");
                var options = new List<FloatMenuOption>();
                foreach (var str in strArray)
                {
                    var setname = str;
                    options.Add(new FloatMenuOption(Path.GetFileNameWithoutExtension(setname), () =>
                    {
                        try
                        {
                            var settings = XmlLoader.ItemFromXmlFile<ModSettings>(setname);
                            PSI.Settings.IconDistanceX = settings.IconDistanceX;
                            PSI.Settings.IconDistanceY = settings.IconDistanceY;
                            PSI.Settings.IconOffsetX = settings.IconOffsetX;
                            PSI.Settings.IconOffsetY = settings.IconOffsetY;
                            PSI.Settings.IconsHorizontal = settings.IconsHorizontal;
                            PSI.Settings.IconsScreenScale = settings.IconsScreenScale;
                            PSI.Settings.IconsInColumn = settings.IconsInColumn;
                            PSI.Settings.IconSize = settings.IconSize;
                            PSI.Settings.IconOpacity = settings.IconOpacity;
                            PSI.Settings.IconOpacity = settings.IconOpacityCritical;
                        }
                        catch (IOException)
                        {
                            Log.Error("PSI.Settings.LoadPreset.UnableToLoad".Translate() + setname);
                        }


                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            var num = (int)(PSI.Settings.IconSize * 4.5);

            if (num > 8)
                num = 8;
            else if (num < 0)
                num = 0;

            listing.DoLabel("PSI.Settings.Arrangement.IconSize".Translate() + ("PSI.Settings.SizeLabel." + num).Translate());
            PSI.Settings.IconSize = listing.DoSlider(PSI.Settings.IconSize, 0.5f, 2f);

            listing.DoLabel(string.Concat("PSI.Settings.Arrangement.IconPosition".Translate(), (int)(PSI.Settings.IconDistanceX * 100.0), " , ", (int)(PSI.Settings.IconDistanceY * 100.0)));
            PSI.Settings.IconDistanceX = listing.DoSlider(PSI.Settings.IconDistanceX, -2f, 2f);
            PSI.Settings.IconDistanceY = listing.DoSlider(PSI.Settings.IconDistanceY, -2f, 2f);

            listing.DoLabel(string.Concat("PSI.Settings.Arrangement.IconOffset".Translate(), (int)(PSI.Settings.IconOffsetX * 100.0), " , ", (int)(PSI.Settings.IconOffsetY * 100.0)));
            PSI.Settings.IconOffsetX = listing.DoSlider(PSI.Settings.IconOffsetX, -2f, 2f);
            PSI.Settings.IconOffsetY = listing.DoSlider(PSI.Settings.IconOffsetY, -2f, 2f);

            listing.DoLabelCheckbox("PSI.Settings.Arrangement.Horizontal".Translate(), ref PSI.Settings.IconsHorizontal);

            listing.DoLabelCheckbox("PSI.Settings.Arrangement.ScreenScale".Translate(), ref PSI.Settings.IconsScreenScale);

            listing.DoLabel("PSI.Settings.Arrangement.IconsPerColumn".Translate() + PSI.Settings.IconsInColumn);

            PSI.Settings.IconsInColumn = (int)listing.DoSlider(PSI.Settings.IconsInColumn, 1f, 7f);

            if (!listing.DoTextButton("PSI.Settings.ReturnButton".Translate()))
                return;

            Page = "main";
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (OptionsDialog == null)
                return;

            var rect = OptionsDialog.currentWindowRect;

            currentWindowRect = new Rect(rect.xMax - 240f, rect.yMin, 240f, rect.height);

            var listing = new Listing_Standard(inRect);

            DoHeading(listing, "Pawn State Icons", false);

            listing.OverrideColumnWidth = currentWindowRect.width;

            if (Page == "showhide")
                FillPageShowHide(listing);
            else if (Page == "opacityandcolor")
                FillPageOpacityAndColor(listing);
            else if (Page == "arrange")
                FillPageArrangement(listing);
            else if (Page == "limits")
                FillPageLimits(listing);
            else
                FillPageMain(listing);

            listing.End();
        }

        public override void PreClose()
        {
            PSI.SaveSettings();
            PSI.Reinit();
            CloseButtonClicked = true;
            base.PreClose();
        }
    }
}
