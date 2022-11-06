using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RocketMan;

namespace RocketMan.Tabs
{
    public class TabHolder
    {
        public ITabContent curTab;
        public List<ITabContent> tabs;

        private int curTabIndex;
        private Vector2 scrollPosition = Vector2.zero;
        private Rect tabBarRect;

        private readonly List<TabRecord> tabsRecord;
        private readonly bool useSidebar = true;

        public TabHolder(List<ITabContent> tabs, bool useSidebar = false)
        {
            this.useSidebar = useSidebar;
            this.tabs = tabs;
            if (tabs.Any(i => i.Selected))
            {
                curTab = tabs.First(i => i.Selected);
                curTab.Selected = true;
            }
            else
            {
                curTab = tabs[0];
                curTab.Selected = true;
            }
            tabsRecord = new List<TabRecord>();
            MakeRecords();
        }

        public void DoContent(Rect inRect)
        {
            var selectedFound = false;
            var counter = 0;

            foreach (var tab in tabs)
            {
                if (tab.Selected && tab.ShouldShow)
                {
                    selectedFound = true;
                    curTabIndex = counter;
                    continue;
                }
                if ((tab.Selected && selectedFound) || !tab.ShouldShow)
                    tab.Selected = false;
                counter++;
            }
            if (selectedFound == false)
            {
                curTabIndex = 0;
                tabs[0].Selected = true;
            }
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                curTab = tabs[curTabIndex];
                if (useSidebar)
                {
                    GUIFont.Anchor = TextAnchor.UpperLeft;
                    Rect tabsRect = inRect.LeftPartPixels(50);
                    Rect contentRect = new Rect(inRect);
                    contentRect.xMin += 60;
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        DoSidebar_newtemp(tabsRect);
                    });
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        GUIFont.Font = GUIFontSize.Small;
                        GUIFont.CurFontStyle.fontStyle = FontStyle.Bold;
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        float headerHeight = "RocketMan".GetTextHeight(inRect.width) + 3;
                        Rect rect = contentRect.TopPartPixels(headerHeight);
                        // Create the RocketMan stamp                        
                        Widgets.Label(rect, "RocketMan");
                        // Create the version string
                        rect.xMin += 90;
                        rect.xMax -= 45;
                        //rect.y += 2;
                        GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
                        GUIFont.Font = GUIFontSize.Tiny;
                        Widgets.Label(rect.TopPartPixels(25), $"Version <color=grey>{RocketAssembliesInfo.Version}</color>");
                        // Do the window content
                        contentRect.yMin += headerHeight + 5;
                        Widgets.DrawBoxSolid(contentRect.TopPartPixels(1), Color.grey);
                        contentRect.yMin += 5;
                    });
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        GUIFont.Font = GUIFontSize.Smaller;
                        GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(contentRect.TopPartPixels(25), curTab.Label);
                    });
                    contentRect.yMin += 30;
                    GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        GUIFont.Anchor = TextAnchor.UpperLeft;
                        curTab.DoContent(contentRect);
                    });
                }
                else
                {
                    // TODO fix this API                
                    // inRect.yMin += 40;
                    // var tabRect = new Rect(inRect);
                    // tabRect.height = 0;                
                    // MakeRecords();
                    // TabDrawer.DrawTabs(tabRect, tabsRecord);
                    // curTab.DoContent(inRect);
                    // -----------------------
                    throw new InvalidOperationException("ROCKETMAN: this is an outdated API!");
                }
            }, fallbackAction: null, catchExceptions: false);
        }

        public void AddTab(ITabContent newTab)
        {
            tabs.Add(newTab);
            tabsRecord.Add(new TabRecord(newTab.Label, () => { curTabIndex = tabs.Count; }, false));
        }

        public void RemoveTab(ITabContent tab)
        {
            if (tab.Selected)
            {
                tab.Selected = false;
                tabs.RemoveAll(t => t == tab);
                curTabIndex = 0;
                tabs.First().Selected = true;
            }
            else
            {
                tabs.RemoveAll(t => t == tab);
                for (var i = 0; i < tabs.Count; i++)
                    if (tabs[i].Selected)
                        curTabIndex = i;
            }
        }

        private void MakeRecords()
        {
            tabsRecord.Clear();
            var counter = 0;
            foreach (var tab in tabs)
            {
                var localTab = tab;
                var localCounter = counter;
                tabsRecord.Add(new TabRecord(tab.Label, () =>
                {
                    tab.Selected = true;
                    curTabIndex = localCounter;
                    curTab.Selected = false;
                    curTab = localTab;
                }, tab.Selected));
                counter++;
            }
        }

        private void DoSidebar_newtemp(Rect inRect)
        {
            int active = tabs.Count(t => t.ShouldShow);
            tabBarRect = new Rect(inRect);
            tabBarRect.width -= 2;
            tabBarRect.height = 40 * active + 10 + (active - 1) * 5;
            Widgets.DrawMenuSection(inRect);
            Widgets.BeginScrollView(inRect, ref scrollPosition, tabBarRect.AtZero());
            Rect rect = new Rect(5, 5, 40, 40);
            var counter = 0;
            foreach (var tab in tabs)
            {
                if (!tab.ShouldShow)
                {
                    counter++;
                    continue;
                }
                if (tab.Selected)
                {
                    Widgets.DrawWindowBackgroundTutor(rect);
                }
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.DrawTextureFitted(rect, tab.Icon, 0.85f);
                TooltipHandler.TipRegion(rect, tab.Label);
                ITabContent localTab = tab;
                int localCounter = counter;
                if (!tab.Selected && Widgets.ButtonInvisible(rect))
                {
                    localTab.Selected = true;
                    curTab.Selected = false;
                    curTab = localTab;
                    curTabIndex = localCounter;
                }
                rect.y += rect.height + 5;
                counter++;
            }
            Widgets.EndScrollView();
        }

        private void DoSidebar(Rect rect)
        {
            tabBarRect = rect;
            tabBarRect.width -= 2;
            tabBarRect.height = 30 * tabs.Count(t => t.ShouldShow);
            Widgets.DrawMenuSection(rect);
            Widgets.BeginScrollView(rect, ref scrollPosition, tabBarRect);
            GUIFont.Anchor = TextAnchor.MiddleLeft;
            GUIFont.Font = GUIFontSize.Tiny;
            var curRect = new Rect(rect.xMin + 5, rect.yMin + 5, 160, 30);
            var counter = 0;
            foreach (var tab in tabs)
            {
                if (!tab.ShouldShow)
                {
                    counter++;
                    continue;
                }
                if (tab.Selected)
                {
                    Widgets.DrawWindowBackgroundTutor(curRect);
                }
                Widgets.DrawHighlightIfMouseover(curRect);
                var textRect = new Rect(curRect);
                textRect.xMin += 10;
                Widgets.Label(textRect, tab.Label);
                var localTab = tab;
                var localCounter = counter;
                if (!tab.Selected && Widgets.ButtonInvisible(curRect))
                {
                    localTab.Selected = true;
                    curTab.Selected = false;
                    curTab = localTab;
                    curTabIndex = localCounter;
                }

                curRect.y += 30;
                counter++;
            }
            Widgets.EndScrollView();
        }
    }
}