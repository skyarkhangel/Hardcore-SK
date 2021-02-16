using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analyzer.Fixes;
using UnityEngine;
using Verse;

namespace Analyzer
{
    using Analyzer.Performance;
    using Profiling;

    public static class Panel_Settings
    {
        public static Listing_Standard listing = new Listing_Standard();
        private static Vector2 scrollPos;
        private static int currentTab = 0;

        public static void Draw(Rect rect, bool settingsPage = false)
        {
            if (settingsPage)
            {
                currentTab = 0;
            }
            else
            {
                rect.y += 35;
                List<TabRecord> list = new List<TabRecord>();
                list.Add(new TabRecord("settings.performance".Translate(), delegate
                {
                    currentTab = 0;
                    Modbase.Settings.Write();
                }, currentTab == 0));
                list.Add(new TabRecord("settings.developer".Translate(), delegate
                {
                    currentTab = 1;
                    Modbase.Settings.Write();
                }, currentTab == 1));

                TabDrawer.DrawTabs(rect, list, 500f);
            }


            Rect view = rect.AtZero();
            view.height = rect.height;

            Widgets.BeginScrollView(rect, ref scrollPos, view, false);
            GUI.BeginGroup(view);
            view.height = 9999;
            listing.Begin(view.ContractedBy(10f));

            // Draw the github and discord textures / Icons

            Rect rec = listing.GetRect(24f);
            Rect lrec = rec.LeftHalf();
            rec = rec.RightHalf();
            Widgets.DrawTextureFitted(lrec.LeftPartPixels(40f), ResourceCache.GUI.Support, 1f);
            lrec.x += 40;
            if (Widgets.ButtonText(lrec.LeftPartPixels(ResourceCache.Strings.settings_wiki.GetWidthCached()), ResourceCache.Strings.settings_wiki, false, true))
            {
                Application.OpenURL("https://github.com/Dubwise56/Dubs-Performance-Analyzer/wiki");
            }

            Widgets.DrawTextureFitted(rec.RightPartPixels(40f), ResourceCache.GUI.disco, 1f);
            rec.width -= 40;
            if (Widgets.ButtonText(rec.RightPartPixels(ResourceCache.Strings.settings_discord.GetWidthCached()), ResourceCache.Strings.settings_discord, false, true))
            {
                Application.OpenURL("https://discord.gg/Az5CnDW");
            }


            listing.GapLine(6f);

            switch (currentTab)
            {
                case 0:
                    PerformancePatches.Draw(ref listing);
                    FixPatches.Draw(listing);
                    break;
                case 1:
                    DrawDevOptions(rect.height);
                    break;
            }

            listing.End();
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        /* For Dev Tools */

        public static void DrawDevOptions(float height)
        {
            DubGUI.InputField(listing.GetRect(Text.LineHeight), ResourceCache.Strings.settings_dnspy, ref Settings.PathToDnspy, ShowName: true);
            DubGUI.LabeledSliderFloat(listing, ResourceCache.Strings.settings_updates_per_second, ref Settings.updatesPerSecond, 1.0f, 20.0f);
            DubGUI.Checkbox(ResourceCache.Strings.settings_logging, listing, ref Settings.verboseLogging);
            DubGUI.Checkbox(ResourceCache.Strings.settings_disable_tps_counter, listing, ref Settings.disableTPSCounter);

            var s = ResourceCache.Strings.settings_disable_cleanup;
            var size = Mathf.CeilToInt(s.GetWidthCached() / listing.ColumnWidth) * Text.LineHeight;
            var rect = listing.GetRect(size);
            DubGUI.Checkbox(rect, s, ref Settings.disableCleanup);
            TooltipHandler.TipRegion(rect, ResourceCache.Strings.settings_disable_cleanup_desc);

            listing.GapLine();

            Panel_DevOptions.Draw(listing, height);
        }
    }
}