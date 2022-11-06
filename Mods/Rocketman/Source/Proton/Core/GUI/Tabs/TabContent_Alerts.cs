using System;
using System.Collections.Generic;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Proton
{
    public class TabContent_Alerts : ITabContent
    {
        private Vector2 scrollPosition = Vector2.zero;

        private Alert curAlert;

        private AlertSettings curSettings;

        public override Texture2D Icon => TexTab.Alerts;

        private string searchString = string.Empty;

        public override bool ShouldShow => RocketPrefs.Enabled;

        public override string Label => "Proton.Tab".Translate();

        public static readonly Color warningColor = new Color(1f, 0.913f, 0.541f, 0.2f);

        public static readonly Color dangerColor = new Color(0.972f, 0.070f, 0.137f, 0.2f);

        private string buffer1;

        private string buffer2;

        private Listing_Collapsible collapsible = new Listing_Collapsible();

        private Listing_Collapsible selection_collapsible = new Listing_Collapsible();

        public static List<Pair<Color, string>> descriptionBoxes;

        public TabContent_Alerts()
        {
            if (descriptionBoxes == null)
            {
                descriptionBoxes = new List<Pair<Color, string>>();
                descriptionBoxes.Add(new Pair<Color, string>(Color.green, "Proton.Colored.ActiveNow".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.yellow, "Proton.Colored.IgnoredAndBad".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.red, "Proton.Colored.DisabledOrBad".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.blue, "Proton.Colored.Ignored".Translate()));
            }
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }

        public override void OnSelect()
        {
            base.OnSelect();
        }

        public override void DoContent(Rect rect)
        {
            collapsible.Begin(rect, KeyedResources.RocketMan_Settings);
            collapsible.Label(KeyedResources.Proton_DisalbeAllInfo, invert: true);
            collapsible.CheckboxLabeled(KeyedResources.Proton_Enable, ref RocketPrefs.AlertThrottling);
            collapsible.Line(1);
            collapsible.Label(KeyedResources.Proton_DisalbeAllInfo);
            collapsible.Line(1);
            if (collapsible.CheckboxLabeled(KeyedResources.Proton_AlertsDisabled, ref RocketPrefs.DisableAllAlert, disabled: !RocketPrefs.AlertThrottling)
                && RocketPrefs.DisableAllAlert)
            {
                foreach (Alert alert in Context.Alerts)
                {
                    alert.cachedActive = false;
                    alert.cachedLabel = string.Empty;
                }
            }
            collapsible.End(ref rect);
            rect.yMin += 5;
            float max = rect.yMax;
            rect.yMax -= 65;
            if (RocketPrefs.AlertThrottling && !RocketPrefs.DisableAllAlert)
            {
                DoScrollView(rect);
            }
            else
            {
                RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    GUIFont.Anchor = TextAnchor.MiddleCenter;
                    GUIFont.Font = GUIFontSize.Medium;
                    Widgets.DrawMenuSection(rect);
                    Widgets.Label(rect, RocketPrefs.DisableAllAlert ? "Proton.Disabled".Translate() : "Proton.AlertsDisabled".Translate());
                });
            }
            rect.yMin = max - 60;
            rect.yMax = max;
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect curRect = rect;
                Widgets.DrawMenuSection(curRect);
                curRect.yMax -= 5;
                curRect.xMin += 15;
                curRect = curRect.ContractedBy(3);
                RocketMan.GUIUtility.Row(curRect.TopHalf(), new List<Action<Rect>>()
                {
                    (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        Widgets.Label(tempRect, "Proton.MaxIn".Translate() + " <color=green>MS</color>");
                    },
                    (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        Widgets.Label(tempRect, "Proton.MinUpdate".Translate() +" <color=green>MS</color>");
                    },
                }, drawDivider: false);
                RocketMan.GUIUtility.Row(curRect.BottomHalf(), new List<Action<Rect>>()
                {
                     (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        if (buffer1 == null)
                        {
                            buffer1 = $"{Context.Settings.executionTimeLimit}";
                        }
                        Widgets.TextFieldNumeric(tempRect, ref Context.Settings.executionTimeLimit, ref buffer1, 1.0f, 100.0f);
                    },
                    (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        if (buffer2 == null)
                        {
                            buffer2 = $"{Context.Settings.minInterval}";
                        }
                        Widgets.TextFieldNumeric(tempRect, ref Context.Settings.minInterval, ref buffer2, 0.5f, 25f);
                    },
                }, drawDivider: false);
                rect.yMin += 63;
            });
        }

        private void DoScrollView(Rect inRect)
        {
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                string tempSearchString = Widgets.TextField(inRect.TopPartPixels(25), searchString).ToLower();
                if (tempSearchString != searchString)
                {
                    scrollPosition = Vector2.zero;
                    searchString = tempSearchString;
                }
                inRect.yMin += 30;
                if (curAlert != null)
                {
                    RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                    {
                        selection_collapsible.Expanded = true;
                        selection_collapsible.Begin(inRect, KeyedResources.RocketMan_Selection.Formatted(curAlert.GetName()), drawIcon: false, drawInfo: false);
                        if (selection_collapsible.CheckboxLabeled("Proton.Enabled".Translate() + "</color>", ref curSettings.enabledInt))
                        {
                            curSettings.UpdateAlert(true);
                            RocketMod.Instance.WriteSettings();
                        }
                        if (selection_collapsible.CheckboxLabeled("Proton.IgnoreThis".Translate(), ref curSettings.ignored))
                        {
                            curSettings.UpdateAlert(true);
                            RocketMod.Instance.WriteSettings();
                        }
                        selection_collapsible.End(ref inRect);
                    });
                    inRect.y += 5;                  
                }
            });
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect curRect = inRect.TopPartPixels(45);
                Widgets.DrawMenuSection(curRect);
                GUIFont.Font = GUIFontSize.Tiny;
                RocketMan.GUIUtility.GridView<Pair<Color, string>>(curRect, 2, descriptionBoxes, (rect, pair) =>
                {
                    RocketMan.GUIUtility.ColorBoxDescription(rect, pair.first, pair.second);
                }, drawBackground: false);
            });
            inRect.yMin += 45;
            RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Rect tempRect = inRect.TopPartPixels(25);
                Widgets.DrawMenuSection(tempRect);
                tempRect.xMin += 10;
                tempRect.xMax -= 25;
                RocketMan.GUIUtility.GridView<Action<Rect>>(tempRect.TopPartPixels(25), 3,
                        new List<Action<Rect>>()
                        {
                        (curRect) =>
                        {
                            Widgets.Label(curRect, "Proton.AlertName".Translate());
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, "Proton.Avg".Translate());
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, "Proton.TimeSinceLast".Translate());
                        }
                        }, (rect, action) => { action.Invoke(rect); }, drawBackground: false);
            });
            inRect.yMin += 25;
            RocketMan.GUIUtility.ScrollView(inRect, ref scrollPosition, Context.ReadoutInstance.AllAlerts,
            heightLambda: (alert) =>
            {
                if (alert == null)
                    return -1.0f;
                if (!Context.AlertToSettings.TryGetValue(alert, out _))
                    return -1.0f;
                if (searchString == null || searchString.NullOrEmpty())
                    return 35;
                return alert.GetNameLower().Contains(searchString) ? 40f : -1.0f;
            },
            elementLambda: (rect, alert) =>
            {
                AlertSettings settings = Context.AlertToSettings[alert];
                if (settings.AverageExecutionTime > Context.Settings.executionTimeLimit)
                {
                    if (settings.ignored) Widgets.DrawBoxSolid(rect, warningColor);
                    else Widgets.DrawBoxSolid(rect, dangerColor);
                }
                if (Widgets.ButtonInvisible(rect))
                {
                    curAlert = alert;
                    curSettings = settings;
                }
                Widgets.DrawBoxSolid(rect.LeftPartPixels(3), !settings.ignored ? (settings.enabledInt ? (alert.cachedActive ? Color.green : Color.grey) : Color.red) : Color.blue);
                RocketMan.GUIUtility.GridView<Action<Rect>>(rect, 3,
                    new List<Action<Rect>>()
                    {
                        (curRect) =>
                        {
                            curRect.xMin += 3;
                            Widgets.Label(curRect, $"{alert.GetName()}");
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, $"{Math.Round(settings.AverageExecutionTime, 3)} MS");
                        },
                        (curRect) =>
                        {
                             Widgets.Label(curRect, $"{Math.Round(settings.TimeSinceLastExecution, 3)} Seconds");
                        }
                    }, (tempRect, action) => { action.Invoke(tempRect); }, drawBackground: false);
            });
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_Alerts();
    }
}
