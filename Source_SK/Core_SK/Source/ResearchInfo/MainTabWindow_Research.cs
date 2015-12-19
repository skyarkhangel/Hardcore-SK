using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Fluffy
{
    public class MainTabWindow_Research : MainTabWindow
    {
        private Texture2D _sortByNameTex = ContentFinder<Texture2D>.Get("UI/Buttons/A");

        private Texture2D _sortByCostTex = ContentFinder<Texture2D>.Get("UI/Buttons/Research");

        private const float LeftAreaWidth = 330f;

        private const int ModeSelectButHeight = 40;

        private const float ProjectTitleHeight = 50f;

        private const float ProjectTitleLeftMargin = 20f;

        private const int ProjectIntervalY = 25;

        protected ResearchProjectDef SelectedProject;

        private enum ShowResearch
        {
            All,
            Completed,
            Available
        }

        private ShowResearch _showResearchedProjects = ShowResearch.Available;

        private Vector2 _projectListScrollPosition = default(Vector2);

        private bool _noBenchWarned;

        private static readonly Texture2D BarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

        private static readonly Texture2D BarBgTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

        private IEnumerable<ResearchProjectDef> _source;

        public enum SortOptions
        {
            Name,
            Cost
        }

        private SortOptions _sortBy = SortOptions.Cost;

        private bool _asc = true;

        private string _filter = "";

        private string _oldFilter;

        public override float TabButtonBarPercent
        {
            get
            {
                ResearchProjectDef currentProj = Find.ResearchManager.currentProj;
                if (currentProj == null)
                {
                    return 0f;
                }
                return currentProj.PercentComplete;
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            SelectedProject = Find.ResearchManager.currentProj;
            _filter = "";
            _oldFilter = "";
            RefreshSource();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            if (!_noBenchWarned)
            {
                if (!Find.ListerBuildings.ColonistsHaveBuilding(ThingDefOf.ResearchBench))
                {
                    Find.WindowStack.Add(new Dialog_Message("ResearchMenuWithoutBench".Translate()));
                }
                _noBenchWarned = true;
            }
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 300f), "Research".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            Rect sidebar = new Rect(0f, 75f, 330f, inRect.height - 75f);
            Rect content = new Rect(sidebar.xMax + 10f, 45f, inRect.width - sidebar.width - 10f, inRect.height - 45f);
            Widgets.DrawMenuSection(sidebar, false);
            Widgets.DrawMenuSection(content);

            // plop in extra row for input + sort buttons
            Rect sortFilterRow = sidebar.ContractedBy(10f);
            sortFilterRow.height = 30f;

            Rect filterRect = new Rect(sortFilterRow);
            filterRect.width = sortFilterRow.width - 110f;
            Rect deleteFilter = new Rect(filterRect.xMax + 6f, filterRect.yMin + 3f, 24f, 24f);
            Rect sortByName = new Rect(deleteFilter.xMax + 6f, filterRect.yMin + 3f, 24f, 24f);
            Rect sortByCost = new Rect(sortByName.xMax + 6f, filterRect.yMin + 3f, 24f, 24f);
            TooltipHandler.TipRegion(filterRect, "RI.filterTooltip".Translate());
            if (_filter != "") TooltipHandler.TipRegion(deleteFilter, "RI.deleteFilterTooltip".Translate());
            TooltipHandler.TipRegion(sortByName, "RI.sortByNameTooltip".Translate());
            TooltipHandler.TipRegion(sortByCost, "RI.sortByCostTooltip".Translate());


            // filter options
            _filter = Widgets.TextField(filterRect, _filter);
            if (_oldFilter != _filter)
            {
                _oldFilter = _filter;
                RefreshSource();
            }
            if (_filter != "")
            {
                if (Widgets.ImageButton(deleteFilter, Widgets.CheckboxOffTex))
                {
                    _filter = "";
                    RefreshSource();
                }
            }

            // sort options
            if (Widgets.ImageButton(sortByName, _sortByNameTex))
            {
                if (_sortBy != SortOptions.Name)
                {
                    _sortBy = SortOptions.Name;
                    _asc = false;
                    RefreshSource();
                }
                else
                {
                    _asc = !_asc;
                    RefreshSource();
                }
            }
            if (Widgets.ImageButton(sortByCost, _sortByCostTex))
            {
                if (_sortBy != SortOptions.Cost)
                {
                    _sortBy = SortOptions.Cost;
                    _asc = true;
                    RefreshSource();
                }
                else
                {
                    _asc = !_asc;
                    RefreshSource();
                }
            }

            // contract sidebar area
            Rect sidebarInner = sidebar.ContractedBy(10f);
            sidebarInner.yMin += 30f;
            sidebarInner.height -= 30f;
            float height = 25 * _source.Count() + 100;
            Rect sidebarContent = new Rect(0f, 0f, sidebarInner.width - 16f, height);
            Widgets.BeginScrollView(sidebarInner, ref _projectListScrollPosition, sidebarContent);
            Rect position = sidebarContent.ContractedBy(10f);
            GUI.BeginGroup(position);
            int num = 0;

            foreach (ResearchProjectDef current in from rp in _source
                                                   select rp)
            {
                Rect sidebarRow = new Rect(0f, num, position.width, 25f);
                if (SelectedProject == current)
                {
                    GUI.DrawTexture(sidebarRow, TexUI.HighlightTex);
                }

                string text = current.LabelCap + " (" + current.totalCost.ToString("F0") + ")";
                Rect sidebarRowInner = new Rect(sidebarRow);
                sidebarRowInner.x += 6f;
                sidebarRowInner.width -= 6f;
                float num2 = Text.CalcHeight(text, sidebarRowInner.width);
                if (sidebarRowInner.height < num2)
                {
                    sidebarRowInner.height = num2 + 3f;
                }
                // give the label a colour if we're in the all tab.
                Color textColor;
                if (_showResearchedProjects == ShowResearch.All)
                {
                    if (current.IsFinished)
                    {
                        textColor = new Color(1f, 1f, 1f);
                    }
                    else if (!current.PrereqsFulfilled)
                    {
                        textColor = new Color(.6f, .6f, .6f);
                    }
                    else
                    {
                        textColor = new Color(.8f, .85f, 1f);
                    }
                }
                else
                {
                    textColor = new Color(.8f, .85f, 1f);
                }
                if (Widgets.TextButton(sidebarRowInner, text, false, true, textColor))
                {
                    SoundDefOf.Click.PlayOneShotOnCamera();
                    SelectedProject = current;
                }
                num += 25;
            }
            GUI.EndGroup();
            Widgets.EndScrollView();
            List<TabRecord> list = new List<TabRecord>();
            TabRecord item = new TabRecord("RI.All".Translate(), delegate
            {
                this._showResearchedProjects = ShowResearch.All;
                RefreshSource();
            }, _showResearchedProjects == ShowResearch.All);
            list.Add(item);
            TabRecord item2 = new TabRecord("Researched".Translate(), delegate
            {
                this._showResearchedProjects = ShowResearch.Completed;
                RefreshSource();
            }, _showResearchedProjects == ShowResearch.Completed);
            list.Add(item2);
            TabRecord item3 = new TabRecord("RI.Available".Translate(), delegate
            {
                this._showResearchedProjects = ShowResearch.Available;
                RefreshSource();
            }, _showResearchedProjects == ShowResearch.Available);
            list.Add(item3);
            TabDrawer.DrawTabs(sidebar, list);
            Rect position2 = content.ContractedBy(20f);
            GUI.BeginGroup(position2);
            if (SelectedProject != null)
            {
                Text.Font = GameFont.Medium;
                GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
                Rect rect6 = new Rect(20f, 0f, position2.width - 20f, 50f);
                Widgets.Label(rect6, SelectedProject.LabelCap);
                GenUI.ResetLabelAlign();
                Text.Font = GameFont.Small;
                Rect rect7 = new Rect(0f, 50f, position2.width, position2.height - 50f);
                string desc = SelectedProject.description;

                // select prerequisites
                desc += ".\n\n";
                string[] prereqs = SelectedProject.prerequisites.Select(def => def.LabelCap).ToArray();
                desc += "RI.Prerequisites".Translate() + ": ";
                if (prereqs.Length == 0)
                {
                    desc += "RI.none".Translate();
                }
                else
                {
                    desc += String.Join(", ", prereqs);
                }
                desc += ".\n\n";

                // select follow-ups
                string[] follow = DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(rpd => rpd.prerequisites.Contains(SelectedProject)).Select(rpd => rpd.LabelCap).ToArray();
                desc += "RI.LeadsTo".Translate() + ": ";
                if (!follow.Any())
                {
                    desc += "RI.none".Translate();
                }
                else
                {
                    desc += String.Join(", ", follow);
                }
                desc += ".\n\n";

                //// find all unlocks
                //desc += "Unlocks: ";
                //string[] unlocks = getUnlocks(selectedProject);
                //if (unlocks == null || unlocks.Count() == 0)
                //{
                //    desc += "none";
                //}
                //else
                //{
                //    desc += String.Join(", ", unlocks);
                //}
                //desc += ".\n\n";


                Widgets.Label(rect7, desc);
                Rect rect8 = new Rect(position2.width / 2f - 50f, 300f, 100f, 50f);
                if (SelectedProject.IsFinished)
                {
                    Widgets.DrawMenuSection(rect8);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect8, "Finished".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if (SelectedProject == Find.ResearchManager.currentProj)
                {
                    Widgets.DrawMenuSection(rect8);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect8, "InProgress".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else if (!SelectedProject.PrereqsFulfilled)
                {
                    Widgets.DrawMenuSection(rect8);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect8, "RI.PreReqLocked".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else
                {
                    if (Widgets.TextButton(rect8, "Research".Translate()))
                    {
                        SoundDef.Named("ResearchStart").PlayOneShotOnCamera();
                        Find.ResearchManager.currentProj = SelectedProject;
                    }
                    if (Prefs.DevMode)
                    {
                        Rect rect9 = rect8;
                        rect9.x += rect9.width + 4f;
                        if (Widgets.TextButton(rect9, "Debug Insta-finish"))
                        {
                            Find.ResearchManager.currentProj = SelectedProject;
                            Find.ResearchManager.InstantFinish(SelectedProject);
                        }
                    }
                }
                Rect rect10 = new Rect(15f, 450f, position2.width - 30f, 35f);
                Widgets.FillableBar(rect10, SelectedProject.PercentComplete, BarFillTex, BarBgTex, true);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect10, SelectedProject.ProgressNumbersString);
                Text.Anchor = TextAnchor.UpperLeft;
            }
            GUI.EndGroup();
        }

        private void RefreshSource()
        {
            if (_showResearchedProjects == ShowResearch.All)
            {
                _source = from proj in DefDatabase<ResearchProjectDef>.AllDefs
                          where !proj.prerequisites.Contains(proj)
                          select proj;
            }
            else if (_showResearchedProjects == ShowResearch.Completed)
            {
                _source = from proj in DefDatabase<ResearchProjectDef>.AllDefs
                          where proj.IsFinished && proj.PrereqsFulfilled
                          select proj;
            }
            else
            {
                _source = from proj in DefDatabase<ResearchProjectDef>.AllDefs
                          where !proj.IsFinished && proj.PrereqsFulfilled
                          select proj;
            }

            if (_filter != "")
            {
                _source = _source.Where(rpd => rpd.label.ToUpper().Contains(_filter.ToUpper()));
            }

            switch (_sortBy)
            {
                case SortOptions.Cost:
                    _source = _source.OrderBy(rpd => rpd.totalCost);
                    break;
                case SortOptions.Name:
                    _source = _source.OrderBy(rpd => rpd.LabelCap);
                    break;
                default:
                    break;
            }

            if (_asc) _source = _source.Reverse();
        }


        //// too many options, different ways, dll unlocking. This won't work.
        //public string[] getUnlocks(ResearchProjectDef def)
        //{
        //    string[] recipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where(rd => re)
        //}
    }
}
