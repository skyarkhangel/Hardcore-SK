using System;
using System.Collections.Generic;
using System.IO;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Gagarin
{
    public class TabContent_Gagarin : ITabContent
    {
        private List<FilterMode> optionsFilter;

        private List<Action<Rect>> columnsFilter;

        private Listing_Collapsible collapsible = new Listing_Collapsible(expanded: true);

        public override Texture2D Icon => TexTab.Gagarin;

        public override bool ShouldShow => true;

        public override string Label => KeyedResources.Gagarin_Tab;

        public TabContent_Gagarin()
        {
            optionsFilter = new List<FilterMode>() {
                FilterMode.Bilinear,
                FilterMode.Trilinear
            };
            this.columnsFilter = new List<Action<Rect>>()
            {
                (rect) =>
                {
                    GUIFont.Anchor = TextAnchor.MiddleLeft;

                    Widgets.Label(rect, KeyedResources.Gagarin_FilterMode);
                },
                (rect) =>
                {
                    if (Widgets.ButtonText(rect, (FilterMode)GagarinPrefs.FilterMode  == FilterMode.Bilinear ? "Bilinear" : "Trilinear"))
                    {
                        FloatMenuUtility.MakeMenu(optionsFilter,
                            (mode) =>
                            {
                                if(mode == FilterMode.Bilinear)
                                    return "Bilinear";
                                if(mode == FilterMode.Trilinear)
                                    return "Trilinear";
                                return "";
                            },
                            (mode)=>
                            {
                                return () => {
                                    GagarinPrefs.FilterMode = (int)mode;
                                    GagarinSettings.WriteSettings();
                                    ClearCache();
                                };
                            }
                        );
                    }
                }
            };
        }

        public override void DoContent(Rect rect)
        {
            collapsible.Begin(rect, KeyedResources.RocketMan_Settings);
            collapsible.Label(KeyedResources.RocketMan_EnableGagarin_Tip);
            if (collapsible.CheckboxLabeled(KeyedResources.RocketMan_EnableGagarin, ref GagarinPrefs.Enabled) && !GagarinPrefs.Enabled)
            {
                if (File.Exists(GagarinEnvironmentInfo.UnifiedXmlFilePath))
                    File.Delete(GagarinEnvironmentInfo.UnifiedXmlFilePath);
                if (File.Exists(GagarinEnvironmentInfo.ModListFilePath))
                    File.Delete(GagarinEnvironmentInfo.ModListFilePath);
                if (Directory.Exists(GagarinEnvironmentInfo.TexturesFolderPath))
                    Directory.Delete(GagarinEnvironmentInfo.TexturesFolderPath, recursive: true);
            }
            if (GagarinPrefs.Enabled)
            {
                collapsible.Line(1);
                collapsible.Label(KeyedResources.Gagarin_Expiry.Formatted(3 - DateTime.Now.Subtract(GagarinPrefs.CacheCreationTime).Days));
                collapsible.Gap(4);
                collapsible.Label(KeyedResources.Gagarin_Tip);
                collapsible.Label(KeyedResources.Gagarin_Tip, invert: true);
                collapsible.Line(1);
                collapsible.Label(KeyedResources.Gagarin_ClearCache_Description);
                collapsible.Lambda(25, (rect) =>
                {
                    if (Widgets.ButtonText(rect, label: KeyedResources.Gagarin_ClearCache))
                    {
                        ClearCache();
                        GagarinSettings.WriteSettings();
                    }
                }, useMargins: true);
                collapsible.Line(1);
                collapsible.Label(KeyedResources.Gagarin_EnableTextureCaching_Description);
                if (collapsible.CheckboxLabeled(KeyedResources.Gagarin_EnableTextureCaching, ref GagarinPrefs.TextureCachingEnabled))
                {
                    ClearCache();
                    GagarinSettings.WriteSettings();
                }
                collapsible.Line(1);
                collapsible.Label(KeyedResources.Gagarin_AdvancedSettings, fontSize: GUIFontSize.Smaller);
                collapsible.Gap(3);
                collapsible.Label(KeyedResources.Gagarin_AdvancedSettings_Description);
                collapsible.Line(1);
                collapsible.Columns(20, columnsFilter, useMargins: true);
            }
            collapsible.End(ref rect);
        }

        private static void ClearCache()
        {
            if (File.Exists(GagarinEnvironmentInfo.UnifiedXmlFilePath))
                File.Delete(GagarinEnvironmentInfo.UnifiedXmlFilePath);
            if (File.Exists(GagarinEnvironmentInfo.ModListFilePath))
                File.Delete(GagarinEnvironmentInfo.ModListFilePath);
            if (File.Exists(GagarinEnvironmentInfo.UnifiedPatchedOriginalXmlPath))
                File.Delete(GagarinEnvironmentInfo.UnifiedPatchedOriginalXmlPath);
            if (Directory.Exists(GagarinEnvironmentInfo.TexturesFolderPath))
                Directory.Delete(GagarinEnvironmentInfo.TexturesFolderPath, recursive: true);
        }

        public override void OnSelect()
        {
            base.OnSelect();

            GagarinSettings.WriteSettings();
        }

        public override void OnDeselect()
        {
            base.OnDeselect();

            GagarinSettings.WriteSettings();
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Gagarin();
    }
}
