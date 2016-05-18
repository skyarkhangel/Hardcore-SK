using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    internal class ManagerTab_Foraging : ManagerTab
    {
        #region Fields

        private const float EntryHeight = 30f;
        private const float Margin = Utilities.Margin;
        private readonly float _topAreaHeight = 30f;
        private Vector2 _button = new Vector2( 200f, 40f );
        private Vector2 _contentScrollPosition = Vector2.zero;
        private List<ManagerJob_Foraging> _jobs;
        private float _leftRowHeight = 9999f;
        private Vector2 _scrollPosition = Vector2.zero;
        private ManagerJob_Foraging _selected = new ManagerJob_Foraging();

        #endregion Fields

        #region Properties

        public override Texture2D Icon => Resources.IconForaging;

        public override IconAreas IconArea => IconAreas.Middle;

        public override string Label => "FMG.Foraging".Translate();

        public override ManagerJob Selected
        {
            get { return _selected; }
            set { _selected = (ManagerJob_Foraging)value; }
        }

        #endregion Properties

        #region Methods

        public void DoContent( Rect rect )
        {
            // layout: settings | trees
            // draw background
            Widgets.DrawMenuSection( rect );

            // some variables
            float width = rect.width;
            float height = rect.height - _topAreaHeight - _button.y - Margin;
            int cols = 2;
            float colWidth = width / cols - Margin;
            List<Rect> colRects = new List<Rect>();
            List<Rect> colTitleRects = new List<Rect>();
            Rect buttonRect = new Rect( rect.width - _button.x, rect.height - _button.y, _button.x - Margin,
                                        _button.y - Margin );

            // set up rects
            for ( int j = 0; j < cols; j++ )
            {
                colRects.Add( new Rect( j * colWidth + j * Margin + Margin / 2, _topAreaHeight, colWidth, height ) );
                colTitleRects.Add( new Rect( j * colWidth + j * Margin + Margin * 2.5f, 0f, colWidth, _topAreaHeight ) );
            }

            // keep track of location
            Vector2 cur;

            // begin window
            GUI.BeginGroup( rect );

            // settings.
            Text.Anchor = TextAnchor.LowerLeft;
            Text.Font = GameFont.Tiny;
            Widgets.Label( colTitleRects[0], "FMG.Options".Translate() );
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.DrawTexture( colRects[0], Resources.SlightlyDarkBackground );
            GUI.BeginGroup( colRects[0] );
            cur = Vector2.zero;

            // trigger config (1)
            int currentCount = _selected.Trigger.CurCount;
            int designatedCount = _selected.CurrentDesignatedCount;
            int targetCount = _selected.Trigger.Count;
            _selected.Trigger.DrawTriggerConfig( ref cur, colRects[0].width, EntryHeight, true,
                "FMG.TargetCount".Translate( currentCount, designatedCount, targetCount ),
                "FMG.TargetCountTooltip".Translate( currentCount, designatedCount, targetCount ) );

            // Force mature plants only (2)
            Rect forceMatureRect = new Rect( cur.x, cur.y, colWidth, EntryHeight );
            Utilities.DrawToggle( forceMatureRect, "FMG.ForceMature".Translate(), ref _selected.ForceFullyMature );
            cur.y += EntryHeight;

            // Foraging area (3)
            Rect foragingAreaTitleRect = new Rect( cur.x, cur.y, colWidth, EntryHeight );
            Widgets.DrawAltRect( foragingAreaTitleRect );
            Utilities.Label( foragingAreaTitleRect, "FMG.ForagingArea".Translate(), anchor: TextAnchor.MiddleLeft,
                             lrMargin: Margin );
            cur.y += EntryHeight;

            Rect foragingAreaRect = new Rect( cur.x, cur.y, colWidth, EntryHeight );
            Widgets.DrawAltRect( foragingAreaRect );
            AreaAllowedGUI.DoAllowedAreaSelectors( foragingAreaRect, ref _selected.ForagingArea, lrMargin: Margin );
            cur.y += EntryHeight;

            GUI.EndGroup();

            // plantdefs.
            Text.Anchor = TextAnchor.LowerLeft;
            Text.Font = GameFont.Tiny;
            Widgets.Label( colTitleRects[1], "FMG.Plants".Translate() );
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.DrawTexture( colRects[1], Resources.SlightlyDarkBackground );
            GUI.BeginGroup( colRects[1] );
            cur = Vector2.zero;

            Rect outRect = colRects[1].AtZero().ContractedBy( 1f );
            Rect viewRect = new Rect( 0f, 0f, outRect.width, _selected.AllowedPlants.Count * EntryHeight );
            if ( viewRect.height > outRect.height )
            {
                viewRect.width -= 16f;
            }

            // start scrolling view
            Widgets.BeginScrollView( outRect, ref _contentScrollPosition, viewRect );

            // list of keys in allowed trees list (all plans that yield wood in biome, static)
            List<ThingDef> plantDefs = new List<ThingDef>( _selected.AllowedPlants.Keys );

            // toggle all
            Rect toggleAllRect = new Rect( cur.x, cur.y, colWidth, EntryHeight );
            Widgets.DrawAltRect( toggleAllRect );
            Utilities.DrawToggle( toggleAllRect, "<i>" + "FM.All".Translate() + "</i>",
                                  _selected.AllowedPlants.Values.All( v => v ), delegate
                                  {
                                      foreach ( ThingDef def in plantDefs )
                                      {
                                          _selected.AllowedPlants[def] = true;
                                      }
                                  }, delegate
                                  {
                                      foreach ( ThingDef def in plantDefs )
                                      {
                                          _selected.AllowedPlants[def] = false;
                                      }
                                  } );

            cur.y += EntryHeight;

            // toggle for each plant
            int i = 1;

            foreach ( ThingDef def in plantDefs )
            {
                Rect toggleRect = new Rect( cur.x, cur.y, colWidth, EntryHeight );

                // highlight alternate rows
                if ( i++ % 2 == 0 )
                {
                    Widgets.DrawAltRect( toggleRect );
                }

                // draw the toggle
                Utilities.DrawToggle( toggleRect, def.LabelCap, _selected.AllowedPlants[def],
                                      delegate
                                      { _selected.AllowedPlants[def] = !_selected.AllowedPlants[def]; } );

                // update current position
                cur.y += EntryHeight;
            }

            // close scrolling view
            Widgets.EndScrollView();

            // close tree list
            GUI.EndGroup();

            // do the button
            if ( !_selected.Managed )
            {
                if ( Widgets.TextButton( buttonRect, "FM.Manage".Translate() ) )
                {
                    // activate job, add it to the stack
                    _selected.Managed = true;
                    Manager.Get.JobStack.Add( _selected );

                    // refresh source list
                    Refresh();
                }
            }
            else
            {
                if ( Widgets.TextButton( buttonRect, "FM.Delete".Translate() ) )
                {
                    // inactivate job, remove from the stack.
                    Manager.Get.JobStack.Delete( _selected );

                    // remove content from UI
                    _selected = null;

                    // refresh source list
                    Refresh();
                }
            }

            // close window
            GUI.EndGroup();
        }

        public void DoLeftRow( Rect rect )
        {
            Widgets.DrawMenuSection( rect );

            // content
            float height = _leftRowHeight;
            Rect scrollView = new Rect( 0f, 0f, rect.width, height );
            if ( height > rect.height )
            {
                scrollView.width -= 16f;
            }

            Widgets.BeginScrollView( rect, ref _scrollPosition, scrollView );
            Rect scrollContent = scrollView;

            GUI.BeginGroup( scrollContent );
            Vector2 cur = Vector2.zero;
            int i = 0;

            foreach ( ManagerJob_Foraging job in _jobs )
            {
                Rect row = new Rect( 0f, cur.y, scrollContent.width, Utilities.LargeListEntryHeight );
                Widgets.DrawHighlightIfMouseover( row );
                if ( _selected == job )
                {
                    Widgets.DrawHighlightSelected( row );
                }

                if ( i++ % 2 == 1 )
                {
                    Widgets.DrawAltRect( row );
                }

                Rect jobRect = row;

                if ( ManagerTab_Overview.DrawOrderButtons( new Rect( row.xMax - 50f, row.yMin, 50f, 50f ), job ) )
                {
                    Refresh();
                }
                jobRect.width -= 50f;

                job.DrawListEntry( jobRect, false );
                if ( Widgets.InvisibleButton( jobRect ) )
                {
                    _selected = job;
                }

                cur.y += Utilities.LargeListEntryHeight;
            }

            // row for new job.
            Rect newRect = new Rect( 0f, cur.y, scrollContent.width, Utilities.LargeListEntryHeight );
            Widgets.DrawHighlightIfMouseover( newRect );

            if ( i % 2 == 1 )
            {
                Widgets.DrawAltRect( newRect );
            }

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label( newRect, "<" + "FMG.NewForagingJob".Translate() + ">" );
            Text.Anchor = TextAnchor.UpperLeft;

            if ( Widgets.InvisibleButton( newRect ) )
            {
                Selected = new ManagerJob_Foraging();
            }

            TooltipHandler.TipRegion( newRect, "FMG.NewForagingJobTooltip".Translate() );

            cur.y += Utilities.LargeListEntryHeight;

            _leftRowHeight = cur.y;
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        public override void DoWindowContents( Rect canvas )
        {
            // set up rects
            Rect leftRow = new Rect( 0f, 0f, DefaultLeftRowSize, canvas.height );
            Rect contentCanvas = new Rect( leftRow.xMax + Margin, 0f, canvas.width - leftRow.width - Margin,
                                           canvas.height );

            // draw overview row
            DoLeftRow( leftRow );

            // draw job interface if something is selected.
            if ( Selected != null )
            {
                DoContent( contentCanvas );
            }
        }

        public override void PreOpen()
        {
            Refresh();
        }

        public void Refresh()
        {
            _jobs = Manager.Get.JobStack.FullStack<ManagerJob_Foraging>();
        }

        #endregion Methods
    }
}