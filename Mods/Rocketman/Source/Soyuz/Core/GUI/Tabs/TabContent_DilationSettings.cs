using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using UnityEngine;
using Verse;

namespace Soyuz.Tabs
{
    public class TabContent_DilationSettings : ITabContent
    {
        public override Texture2D Icon => TexTab.Dilation;

        public override bool ShouldShow => RocketPrefs.Enabled;

        public override string Label => "Soyuz.Tab".Translate();

        public static readonly Color ignoredColor = new Color(1f, 0.913f, 0.541f, 0.2f);

        public static readonly Color disabledColor = new Color(0.972f, 0.070f, 0.137f, 0.2f);

        private string buffer1;

        private string buffer2;

        private Vector2 scrollPosition = Vector2.zero;

        private Vector2 jobsScrollPosition = Vector2.zero;

        private RaceSettings curSettings;

        private string searchString = string.Empty;

        public static List<Pair<Color, string>> descriptionBoxes;

        private Listing_Collapsible collapsible = new Listing_Collapsible();

        private Listing_Collapsible collapsible_debug = new Listing_Collapsible();

        private Listing_Collapsible collapsible_selection = new Listing_Collapsible();

        private Listing_Collapsible collapsible_jobs = new Listing_Collapsible();

        private JobSettings curJobSettings = null;

        private IEnumerable<JobDef> jobs;

        public TabContent_DilationSettings()
        {
            if (descriptionBoxes == null)
            {
                descriptionBoxes = new List<Pair<Color, string>>();
                descriptionBoxes.Add(new Pair<Color, string>(Color.red,
                    "Soyuz.Colored.Disabled".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.yellow,
                    "Soyuz.Colored.IgnoredAndDisabled".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.blue,
                    "Soyuz.Colored.IsFastMoving".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.cyan,
                    "Soyuz.Colored.IgnorePlayerFaction".Translate()));
                descriptionBoxes.Add(new Pair<Color, string>(Color.magenta,
                    "Soyuz.Colored.IgnoreAllFaction".Translate()));
            }
            this.jobs = DefDatabase<JobDef>.AllDefs;
        }

        public override void OnDeselect()
        {
            base.OnDeselect();
        }

        public override void OnSelect()
        {
            base.OnSelect();
        }

        public override void DoContent(Rect inRect)
        {
            RocketMan.GUIUtility.StashGUIState();
            collapsible.Begin(inRect, KeyedResources.RocketMan_Settings);
            collapsible.Label(KeyedResources.Soyuz_GeneralTip, invert: true);
            collapsible.CheckboxLabeled(KeyedResources.Soyuz_Enable, ref RocketPrefs.TimeDilation);
            collapsible.Line(1);
            collapsible.Label(KeyedResources.Soyuz_GeneralTip);
            collapsible.Line(1);
            collapsible.Label(KeyedResources.Soyuz_DilationFactor);
            collapsible.Lambda(50, (rect) =>
            {                
                RocketMan.GUIUtility.Row(rect.TopHalf(), new List<Action<Rect>>()
                {
                    (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        Widgets.Label(tempRect, KeyedResources.Soyuz_DilationFactor_OffScreen);
                    },
                    (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        Widgets.Label(tempRect, KeyedResources.Soyuz_DilationFactor_OnScreen);
                    },
                }, drawDivider: false);
                RocketMan.GUIUtility.Row(rect.BottomHalf(), new List<Action<Rect>>()
                {
                     (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        if (buffer1 == null)
                        {
                            buffer1 = $"{Context.Settings.dilationFactorOffscreen}";
                        }
                        Widgets.TextFieldNumeric(tempRect, ref Context.Settings.dilationFactorOffscreen, ref buffer1, 0.2f, 1.2f);
                    },
                    (tempRect) =>
                    {
                        GUIFont.Anchor = TextAnchor.MiddleLeft;
                        GUIFont.Font = GUIFontSize.Tiny;
                        if (buffer2 == null)
                        {
                            buffer2 = $"{Context.Settings.dilationFactorOnscreen}";
                        }
                        Widgets.TextFieldNumeric(tempRect, ref Context.Settings.dilationFactorOnscreen, ref buffer2, 0.2f, 0.8f);
                    },
                }, drawDivider: false);
            }, useMargins: true);
            collapsible.Line(1);
            collapsible.CheckboxLabeled(KeyedResources.Soyuz_EnableTimeWildlife, ref RocketPrefs.TimeDilationWildlife, disabled: !RocketPrefs.TimeDilation);
            // TODO redo this
            // collapsible.CheckboxLabeled(KeyedResources.Soyuz_EnableTimeDilationVisitors, ref RocketPrefs.TimeDilationVisitors, disabled: !RocketPrefs.TimeDilation);
            //if (RocketEnvironmentInfo.IsDevEnv)
            //{
            //collapsible.CheckboxLabeled(KeyedResources.Soyuz_EnableTimeWorldPawns, ref RocketPrefs.TimeDilationWorldPawns, disabled: !RocketPrefs.TimeDilation);
            //collapsible.CheckboxLabeled(KeyedResources.Soyuz_EnableTimeCriticalHediffs, ref RocketPrefs.TimeDilationCriticalHediffs, disabled: !RocketPrefs.TimeDilation);
            //collapsible.CheckboxLabeled(KeyedResources.Soyuz_EnableTimeColonyAnimals, ref RocketPrefs.TimeDilationColonyAnimals, disabled: !RocketPrefs.TimeDilation);
            //collapsible.CheckboxLabeled(KeyedResources.Soyuz_EnableTimeDilationVisitors, ref RocketPrefs.TimeDilationVisitors, disabled: !RocketPrefs.TimeDilation);
            //}
            //if (RocketEnvironmentInfo.IsDevEnv)
            //{
            //    collapsible.Line(1);
            //    collapsible.Label(KeyedResources.RocketMan_Experimental);
            //    collapsible.Gap(3);
            //    collapsible.Label(KeyedResources.Soyuz_EnableTimeColonists_Warning);
            //    collapsible.CheckboxLabeled(KeyedResources.Soyuz_EnableTimeColonists, ref RocketPrefs.TimeDilationColonists, disabled: !RocketPrefs.TimeDilation);
            //}
            collapsible.End(ref inRect);
            if (RocketDebugPrefs.Debug)
            {
                inRect.yMin -= 1;
                collapsible_debug.Begin(inRect, KeyedResources.RocketMan_Settings_Debugging);
                collapsible_debug.CheckboxLabeled(KeyedResources.Soyuz_EnableDataLogging, ref RocketDebugPrefs.LogData, disabled: !RocketPrefs.TimeDilation);
                //collapsible_debug.CheckboxLabeled(KeyedResources.Soyuz_Debug150MTPS, ref RocketDebugPrefs.Debug150MTPS, disabled: !RocketPrefs.TimeDilation);
                collapsible_debug.CheckboxLabeled(KeyedResources.Soyuz_FlashPawns, ref RocketDebugPrefs.FlashDilatedPawns, disabled: !RocketPrefs.TimeDilation);
                collapsible_debug.CheckboxLabeled(KeyedResources.Soyuz_AlwaysDilate, ref RocketDebugPrefs.AlwaysDilating, disabled: !RocketPrefs.TimeDilation);
                collapsible_debug.End(ref inRect);
            }
            inRect.yMin += 5;
            if (RocketPrefs.TimeDilation)
            {
                DoExtras(inRect);
            }
            else RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
            {
                GUIFont.Anchor = TextAnchor.MiddleCenter;
                GUIFont.Font = GUIFontSize.Medium;
                Widgets.DrawMenuSection(inRect);
                Widgets.Label(inRect, "Soyuz.DilationDisabled".Translate());
            });
            RocketMan.GUIUtility.RestoreGUIState();
        }

        private void DoExtras(Rect inRect)
        {
            string oldString = searchString;
            searchString = Widgets.TextField(inRect.TopPartPixels(25), searchString).ToLower();
            if (oldString != searchString)
            {
                scrollPosition = Vector2.zero;
            }
            inRect.yMin += 30;
            if (RocketPrefs.TimeDilation && RocketEnvironmentInfo.IsDevEnv && searchString != null && searchString.StartsWith("secret"))
            {
                string jobSearchString = searchString.Length > 6 ? searchString.Substring(6, searchString.Length - 6).Trim().ToLower() : string.Empty;
                collapsible_jobs.Begin(inRect, "Selection setttings");
                collapsible_jobs.Label("GO AWAY THIS IS SECRET MOFFA LIL PIECE OF SHT!");
                collapsible_jobs.Lambda(20, (rect) =>
                {
                    if (Widgets.ButtonText(rect, "Auto configure secret stuffs"))
                    {
                        SoyuzSettingsUtility.SetRecommendedJobConfig();
                    }
                }, useMargins: true);
                if (curJobSettings != null && curJobSettings.def != null)
                {
                    JobSettings curJobSettings = this.curJobSettings;
                    collapsible_jobs.Line(1);
                    collapsible_jobs.Label(curJobSettings.def.label ?? curJobSettings.def.defName, fontSize: GUIFontSize.Smaller);
                    collapsible_jobs.Gap(2);
                    collapsible_jobs.DropDownMenu("Filter", curJobSettings.throttleFilter,
                        (a) =>
                        {
                            if (a == JobThrottleFilter.All)
                            {
                                return "All";
                            }
                            if (a == JobThrottleFilter.Animals)
                            {
                                return "Animals";
                            }
                            return "Humanlikes";
                        },
                        (a) =>
                         {
                             curJobSettings.throttleFilter = a;
                         },
                        new[] {
                             JobThrottleFilter.All,
                             JobThrottleFilter.Animals,
                         });
                    collapsible_jobs.DropDownMenu("Mode", curJobSettings.throttleMode,
                        (a) =>
                        {
                            if (a == JobThrottleMode.Full)
                            {
                                return "Full";
                            }
                            if (a == JobThrottleMode.Partial)
                            {
                                return "Partial";
                            }
                            return "None";
                        },
                        (a) =>
                        {
                            curJobSettings.throttleMode = a;
                        },
                        new[] {
                            JobThrottleMode.None,
                            JobThrottleMode.Full,
                            JobThrottleMode.Partial
                        });
                }
                collapsible_jobs.End(ref inRect);
                RocketMan.GUIUtility.ScrollView(inRect, ref jobsScrollPosition, this.jobs,
                    (def) =>
                    {
                        if (def == null)
                        {
                            return -1;
                        }
                        if (!jobSearchString.NullOrEmpty() && !(def.label ?? def.defName).ToLower().Contains(jobSearchString))
                        {
                            return -1;
                        }
                        return Context.JobDilationByDef.ContainsKey(def) ? 35 : -1;
                    },
                    (rect, def) =>
                    {
                        GUIFont.Font = GUIFontSize.Tiny;
                        GUIFont.Anchor = TextAnchor.MiddleLeft;

                        var settings = Context.JobDilationByDef[def];

                        if (settings.throttleFilter == JobThrottleFilter.Humanlikes)
                            Widgets.DrawBoxSolid(rect.LeftPartPixels(3), Color.blue);
                        if (settings.throttleMode == JobThrottleMode.None)
                            Widgets.DrawBoxSolid(rect.LeftPartPixels(3), Color.red);
                        if (Widgets.ButtonInvisible(rect))
                            curJobSettings = settings;

                        rect.xMin += 5;
                        Widgets.Label(rect, def.label ?? def.defName);
                    }
                );
            }
            else
            {
                if (curSettings != null)
                {
                    DoRaceSettings(ref inRect);
                    // ------------------
                    // inRect.yMin += 100;
                }
                RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    Rect curRect = inRect.TopPartPixels(60);
                    Widgets.DrawMenuSection(curRect);
                    GUIFont.Font = GUIFontSize.Tiny;
                    RocketMan.GUIUtility.GridView<Pair<Color, string>>(curRect, 2, descriptionBoxes, (rect, pair) =>
                    {
                        RocketMan.GUIUtility.ColorBoxDescription(rect, pair.first, pair.second);
                    }, drawBackground: false);
                });
                inRect.yMin += 60;
                GUIFont.Font = GUIFontSize.Tiny;
                GUIFont.CurFontStyle.fontStyle = FontStyle.Normal;
                RocketMan.GUIUtility.ExecuteSafeGUIAction(() =>
                {
                    Rect tempRect = inRect.TopPartPixels(25);
                    Widgets.DrawMenuSection(tempRect);
                    tempRect.xMin += 10 + 50;
                    tempRect.xMax -= 25;
                    RocketMan.GUIUtility.GridView<Action<Rect>>(tempRect.TopPartPixels(25), 2,
                            new List<Action<Rect>>()
                            {
                        (curRect) =>
                        {
                            Widgets.Label(curRect, KeyedResources.Soyuz_RaceName);
                        },
                        (curRect) =>
                        {
                            Widgets.Label(curRect, KeyedResources.Soyuz_PackageId);
                        }
                            }, (rect, action) => { action.Invoke(rect); }, drawBackground: false);
                });
                inRect.yMin += 25;
                RocketMan.GUIUtility.ScrollView<RaceSettings>(inRect, ref scrollPosition, Context.Settings.AllRaceSettings,
                    heightLambda: (raceSettings) =>
                    {
                        if (searchString?.Trim().NullOrEmpty() ?? true)
                            return raceSettings.def != null ? 35 : -0.1f;
                        return raceSettings.def != null ?
                        (raceSettings.def.label != null && raceSettings.def.label.ToLower().Contains(searchString) ? 45f : -0.1f)
                        : -0.1f;
                    },
                    elementLambda: (rect, raceSettings) =>
                    {
                        if (Widgets.ButtonInvisible(rect))
                            curSettings = raceSettings;
                        bool ignored = IgnoreMeDatabase.ShouldIgnore(raceSettings.def);
                        DoColorBars(ref rect, raceSettings, ignored);
                        Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
                        iconRect = iconRect.ContractedBy(4);
                        iconRect = iconRect.CenteredOnYIn(rect);
                        rect.xMin += rect.height + 5;
                        Widgets.DefIcon(iconRect, raceSettings.def, null, 0.90f);
                        RocketMan.GUIUtility.GridView(rect, 2,
                            elements: new List<Action<Rect>>()
                            {
                            (r) =>
                            {
                                Widgets.Label(r, raceSettings.def.label?.CapitalizeFirst() ?? raceSettings.def.defName);
                            },
                            (r) =>
                            {
                                 Widgets.Label(r, raceSettings.def.modContentPack?.PackageIdPlayerFacing ?? "Unknown");
                            }
                            },
                            cellLambda: (r, f) => f(r),
                            drawBackground: false,
                            drawVerticalDivider: false
                        );
                    }
                );
            }
        }

        private void DoRaceSettings(ref Rect inRect)
        {
            if (curSettings != null)
            {
                collapsible_selection.Expanded = true;
                collapsible_selection.Begin(inRect, KeyedResources.RocketMan_Selection.Formatted(curSettings.def.label?.CapitalizeFirst() ?? curSettings.def.defName), drawIcon: false, drawInfo: false);
                if (!IgnoreMeDatabase.ShouldIgnore(curSettings.def))
                {
                    bool checkOn = Context.DilationEnabled[curSettings.def.index];
                    if (collapsible_selection.CheckboxLabeled("Soyuz.Current.Enable".Translate(), ref checkOn))
                    {
                        Context.DilationEnabled[curSettings.def.index] = checkOn;
                        curSettings.enabled = checkOn;
                        curSettings.Prepare(updating: true);
                    }
                    collapsible_selection.Line(1);
                    collapsible_selection.Label(KeyedResources.Soyuz_Current_Tip);
                    collapsible_selection.Line(1);
                    if (collapsible_selection.CheckboxLabeled("Soyuz.Current.IgnoreAllFactions".Translate(), ref curSettings.ignoreFactions, disabled: !curSettings.enabled))
                    {
                        curSettings.Prepare(updating: true);
                    }
                    if (collapsible_selection.CheckboxLabeled("Soyuz.Current.IgnorePlayerFaction".Translate(), ref curSettings.ignorePlayerFaction, disabled: !curSettings.enabled))
                    {
                        curSettings.Prepare(updating: true);
                    }
                }
                else if (curSettings.isFastMoving)
                {
                    collapsible_selection.Label("<color=yellow>" + "Soyuz.Current.FastPawn".Translate() + "</color>");
                    collapsible_selection.Label("Soyuz.Current.MoveSpeed".Translate().Formatted(curSettings.def.GetStatValueAbstract(StatDefOf.MoveSpeed)));
                }
                else
                {
                    collapsible_selection.Label("<color=yellow>" + "Soyuz.Current.Ignored".Translate() + "</color>");
                    collapsible_selection.Label(IgnoreMeDatabase.Report(curSettings.def));
                }
                curSettings.Prepare(updating: true);
                collapsible_selection.End(ref inRect);
                inRect.yMin += 5;
            }
        }

        private static void DoColorBars(ref Rect inRect, RaceSettings settings, bool ignored = false)
        {
            Widgets.DrawBoxSolid(inRect.LeftPartPixels(3), Color.grey);

            Rect cRect = inRect.LeftPartPixels(2);
            if (ignored == true)
                Widgets.DrawBoxSolid(inRect, ignoredColor);

            if (!settings.enabled)
                Widgets.DrawBoxSolid(cRect, Color.red);
            cRect.x += 2;

            if (settings.isFastMoving)
                Widgets.DrawBoxSolid(cRect, Color.blue);
            cRect.x += 2;

            if (settings.ignorePlayerFaction)
                Widgets.DrawBoxSolid(cRect, Color.cyan);
            cRect.x += 2;

            if (settings.ignoreFactions)
                Widgets.DrawBoxSolid(cRect, Color.magenta);
            cRect.x += 2;

            inRect.xMin += 8;
        }

        [Main.YieldTabContent]
        public static ITabContent YieldTab() => new TabContent_DilationSettings();
    }
}
