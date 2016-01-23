// Manager/ManagerTab_Overview.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:23

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Resources = FluffyManager.Resources;

namespace FluffyManager
{
    internal class ManagerTab_Overview : ManagerTab
    {
        public const float Margin = Utilities.Margin,
                           OverviewWidthRatio = .6f,
                           RowHeight = Utilities.LargeListEntryHeight,
                           RowHeightPawnOverview = 30f,
                           IconSize = 30f;
        
        private Vector2 _overviewScrollPosition = Vector2.zero;
        private ManagerJob _selectedJob;
        private SkillDef _skillDef;
        private Vector2 _workersScrollPosition = Vector2.zero;
        private WorkTypeDef _workType;
        public float OverviewHeight = 9999f;
        private List<Pawn> Workers = new List<Pawn>();

        public static List<ManagerJob> Jobs
        {
            get { return Manager.Get.JobStack.FullStack(); }
        }

        public override Texture2D Icon
        {
            get { return Resources.IconOverview; }
        }

        public override IconAreas IconArea
        {
            get { return IconAreas.Left; }
        }

        public override string Label { get; } = "FM.Overview".Translate();

        public override ManagerJob Selected
        {
            get { return _selectedJob; }
            set
            {
                _selectedJob = value;
                WorkTypeDef = _selectedJob?.WorkTypeDef ?? Utilities.WorkTypeDefOf_Managing;
                SkillDef = _selectedJob?.SkillDef;
            }
        }

        private WorkTypeDef WorkTypeDef
        {
            get
            {
                if ( _workType == null )
                {
                    _workType = Utilities.WorkTypeDefOf_Managing;
                }
                return _workType;
            }
            set
            {
                _workType = value;
                RefreshWorkers();
            }
        }

        private SkillDef SkillDef
        {
            get { return _skillDef; }
            set { _skillDef = value; }
        }

        /// <summary>
        /// Draw a square group of ordering buttons for a job in rect.
        /// This is an LOCAL method that within the specified job type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rect"></param>
        /// <param name="job"></param>
        /// <returns></returns>
        public static bool DrawOrderButtons<T>( Rect rect, T job ) where T : ManagerJob
        {
            bool ret = false;

            float width = rect.width / 2,
                  height = rect.height / 2;

            Rect upRect = new Rect( rect.xMin, rect.yMin, width, height ).ContractedBy( 1f ),
                 downRect = new Rect( rect.xMin, rect.yMin + height, width, height ).ContractedBy( 1f ),
                 topRect = new Rect( rect.xMin + width, rect.yMin, width, height ).ContractedBy( 1f ),
                 bottomRect = new Rect( rect.xMin + width, rect.yMin + height, width, height ).ContractedBy( 1f );

            List<T> jobsOfType = Jobs.OfType<T>().OrderBy( j => j.Priority ).ToList();

            bool top = jobsOfType.IndexOf( job ) == 0,
                 bottom = jobsOfType.IndexOf( job ) == jobsOfType.Count - 1;

            if ( !top )
            {
                DrawOrderTooltips( upRect, topRect );
                if ( Widgets.ImageButton( topRect, Resources.ArrowTop ) )
                {
                    Manager.Get.JobStack.TopPriority( job );
                    ret = true;
                }

                if ( Widgets.ImageButton( upRect, Resources.ArrowUp ) )
                {
                    Manager.Get.JobStack.IncreasePriority( job );
                    ret = true;
                }
            }

            if ( !bottom )
            {
                DrawOrderTooltips( downRect, bottomRect, false );
                if ( Widgets.ImageButton( downRect, Resources.ArrowDown ) )
                {
                    Manager.Get.JobStack.DecreasePriority( job );
                    ret = true;
                }

                if ( Widgets.ImageButton( bottomRect, Resources.ArrowBottom ) )
                {
                    Manager.Get.JobStack.BottomPriority( job );
                    ret = true;
                }
            }
            return ret;
        }

        public static void DrawOrderTooltips( Rect step, Rect max, bool up = true )
        {
            if ( up )
            {
                TooltipHandler.TipRegion( step, "FM.OrderUp".Translate() );
                TooltipHandler.TipRegion( max, "FM.OrderTop".Translate() );
            }
            else
            {
                TooltipHandler.TipRegion( step, "FM.OrderDown".Translate() );
                TooltipHandler.TipRegion( max, "FM.OrderBottom".Translate() );
            }
        }

        public override void DoWindowContents( Rect canvas )
        {
            Rect overviewRect = new Rect( 0f, 0f, OverviewWidthRatio * canvas.width, canvas.height );
            Rect sideRectUpper = new Rect( overviewRect.xMax + Margin, 0f,
                                           ( 1 - OverviewWidthRatio ) * canvas.width - Margin,
                                           ( canvas.height - Margin ) / 2 );
            Rect sideRectLower = new Rect( overviewRect.xMax + Margin, sideRectUpper.yMax + Margin, sideRectUpper.width,
                                           sideRectUpper.height - 1 );

            // draw the listing of current jobs.
            Widgets.DrawMenuSection( overviewRect );
            DrawOverview( overviewRect );

            // draw the selected job's details
            Widgets.DrawMenuSection( sideRectUpper );
            if ( _selectedJob != null )
            {
                _selectedJob.DrawOverviewDetails( sideRectUpper );
            }

            // overview of managers & pawns (capable of) doing this job.
            Widgets.DrawMenuSection( sideRectLower );
            DrawPawnOverview( sideRectLower );
        }

        private void RefreshWorkers()
        {
            IEnumerable<Pawn> temp =
                Find.ListerPawns.FreeColonistsSpawned.Where( pawn => !pawn.story.WorkTypeIsDisabled( WorkTypeDef ) );

            // sort by either specific skill def or average over job - depending on which is known.
            temp = SkillDef != null
                ? temp.OrderByDescending( pawn => pawn.skills.GetSkill( SkillDef ).level )
                : temp.OrderByDescending( pawn => pawn.skills.AverageOfRelevantSkillsFor( WorkTypeDef ) );

            Workers = temp.ToList();
        }

        public void DrawPawnOverview( Rect rect )
        {
            // table body viewport
            Rect tableOutRect = new Rect( 0f, RowHeightPawnOverview, rect.width, rect.height - RowHeightPawnOverview );
            Rect tableViewRect = new Rect( 0f, RowHeightPawnOverview, rect.width, Workers.Count * RowHeightPawnOverview );
            if ( tableViewRect.height > tableOutRect.height )
            {
                tableViewRect.width -= 16f;
            }

            // column width
            float colWidth = tableViewRect.width / 4 - Margin;

            // column headers
            Rect nameColumnHeaderRect = new Rect( colWidth * 0, 0f, colWidth, RowHeightPawnOverview );
            Rect activityColumnHeaderRect = new Rect( colWidth * 1, 0f, colWidth * 2.5f, RowHeightPawnOverview );
            Rect priorityColumnHeaderRect = new Rect( colWidth * 3.5f, 0f, colWidth * .5f, RowHeightPawnOverview );

            // label for priority column
            string workLabel = Find.Map.playSettings.useWorkPriorities
                ? "FM.Priority".Translate()
                : "FM.Enabled".Translate();

            // begin drawing
            GUI.BeginGroup( rect );

            // draw labels
            Utilities.Label( nameColumnHeaderRect, WorkTypeDef.pawnLabel + "s", null, TextAnchor.LowerCenter );
            Utilities.Label( activityColumnHeaderRect, "FM.Activity".Translate(), null, TextAnchor.LowerCenter );
            Utilities.Label( priorityColumnHeaderRect, workLabel, null, TextAnchor.LowerCenter );

            // begin scrolling area
            Widgets.BeginScrollView( tableOutRect, ref _workersScrollPosition, tableViewRect );
            GUI.BeginGroup( tableViewRect );

            // draw pawn rows
            Vector2 cur = Vector2.zero;
            for ( int i = 0; i < Workers.Count; i++ )
            {
                Rect row = new Rect( cur.x, cur.y, tableViewRect.width, RowHeightPawnOverview );
                if ( i % 2 == 0 )
                {
                    Widgets.DrawAltRect( row );
                }
                try
                {
                    DrawPawnOverviewRow( Workers[i], row );
                }
                catch // pawn death, etc.
                {
                    // rehresh the list and skip drawing untill the next GUI tick.
                    RefreshWorkers();
                    return;
                }
                cur.y += RowHeightPawnOverview;
            }

            // end scrolling area
            GUI.EndGroup();
            Widgets.EndScrollView();

            // done!
            GUI.EndGroup();
        }

        private void DrawPawnOverviewRow( Pawn pawn, Rect rect )
        {
            // column width
            float colWidth = rect.width / 4 - Margin;

            // cell rects
            Rect nameRect = new Rect( colWidth * 0, rect.yMin, colWidth, RowHeightPawnOverview );
            Rect activityRect = new Rect( colWidth * 1, rect.yMin, colWidth * 2.5f, RowHeightPawnOverview );
            Rect priorityRect = new Rect( colWidth * 3.5f, rect.yMin, colWidth * .5f, RowHeightPawnOverview );

            // name
            Widgets.DrawHighlightIfMouseover( nameRect );

            // on click select and jump to location
            if ( Widgets.InvisibleButton( nameRect ) )
            {
                Find.MainTabsRoot.EscapeCurrentTab();
                Find.CameraMap.JumpTo( pawn.PositionHeld );
                Find.Selector.ClearSelection();
                if ( pawn.SpawnedInWorld )
                {
                    Find.Selector.Select( pawn );
                }
            }
            Utilities.Label( nameRect, pawn.NameStringShort, "FM.ClickToJumpTo".Translate( pawn.LabelCap ),
                             TextAnchor.MiddleLeft, Margin );

            // current activity (if curDriver != null)
            string activityString = pawn.jobs.curDriver?.GetReport() ?? "FM.NoCurJob".Translate();
            Utilities.Label( activityRect, activityString, pawn.jobs.curDriver?.GetReport(), TextAnchor.MiddleCenter,
                             Margin, font: GameFont.Tiny );

            // priority button
            Rect priorityPosition = new Rect( 0f, 0f, 24f, 24f ).CenteredOnXIn( priorityRect )
                                                                .CenteredOnYIn( priorityRect );
            Text.Font = GameFont.Medium;
            WidgetsWork.DrawWorkBoxFor( new Vector2( priorityPosition.xMin, priorityPosition.yMin ), pawn, WorkTypeDef );
            Text.Font = GameFont.Small;
        }

        public void DrawOverview( Rect rect )
        {
            if ( Jobs.NullOrEmpty() )
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.grey;
                Widgets.Label( rect, "FM.NoJobs".Translate() );
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            else
            {
                Rect viewRect = rect;
                Rect contentRect = viewRect.AtZero();
                contentRect.height = OverviewHeight;
                if ( OverviewHeight > viewRect.height )
                {
                    contentRect.width -= 16f;
                }

                GUI.BeginGroup( viewRect );
                Widgets.BeginScrollView( viewRect, ref _overviewScrollPosition, contentRect );

                Vector2 cur = Vector2.zero;

                for ( int i = 0; i < Jobs.Count; i++ )
                {
                    Rect row = new Rect( cur.x, cur.y, contentRect.width, 50f );

                    // highlights
                    if ( i % 2 == 1 )
                    {
                        Widgets.DrawAltRect( row );
                    }
                    if ( Jobs[i] == Selected )
                    {
                        Widgets.DrawHighlightSelected( row );
                    }

                    // go to job icon
                    Rect iconRect = new Rect( Margin, row.yMin + ( RowHeight - IconSize ) / 2, IconSize, IconSize );
                    if ( Widgets.ImageButton( iconRect, Jobs[i].Tab.Icon ) )
                    {
                        MainTabWindow_Manager.GoTo( Jobs[i].Tab, Jobs[i] );
                    }

                    // order buttons
                    DrawOrderButtons( new Rect( row.xMax - 50f, row.yMin, 50f, 50f ), Jobs[i] );

                    // job specific overview.
                    Rect jobRect = row;
                    jobRect.width -= RowHeight + IconSize + 2 * Margin; // - (a + b)?
                    jobRect.x += IconSize + 2 * Margin;
                    Jobs[i].DrawListEntry( jobRect, true, true );
                    Widgets.DrawHighlightIfMouseover( row );
                    if ( Widgets.InvisibleButton( jobRect ) )
                    {
                        Selected = Jobs[i];
                    }

                    cur.y += 50f;
                }

                GUI.EndScrollView();
                GUI.EndGroup();

                OverviewHeight = cur.y;
            }
        }

        #region Overrides of ManagerTab

        public override void PreOpen()
        {
            RefreshWorkers();
        }

        #endregion
    }
}