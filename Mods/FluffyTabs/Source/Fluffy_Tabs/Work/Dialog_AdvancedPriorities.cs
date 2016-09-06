//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Mime;
//using System.Text;
//using UnityEngine;
//using Verse;
//using Verse.Noise;

//namespace Fluffy_Tabs
//{
//    public class Dialog_NaturalPriorities : Window
//    {
//        #region Fields

//        private static bool         _workGiverMoved             = false;
//        private Vector2             _buttonSize                 = new Vector2(16f, 16f);
//        private float               _entryHeight                = 30f;
//        private bool                _first                      = true;
//        private float               _height                     = 550f;
//        private MainTabWindow_Work  _mainTab;
//        private float               _margin                     = 6f;
//        private WorkGiverDef        _selectedWorkGiverDef;
//        private WorkTypeDef         _selectedWorkTypeDef;
//        private float               _width                      = 350f;
//        private float               _workGiverListHeight        = 999f;
//        private Vector2             _workGiverScrollPosition    = Vector2.zero;
//        private List<WorkTypeDef>   _workTypeDefs;
//        private float               _workTypeListHeight         = 9999f;
//        private bool                _workTypeMoved              = false;
//        private Vector2             _workTypeScrollPosition     = Vector2.zero;

//        #endregion Fields

//        #region Constructors

//        public Dialog_NaturalPriorities()
//        {
//        }

//        public Dialog_NaturalPriorities( MainTabWindow_Work mainTab )
//        {
//            _mainTab = mainTab;
//        }

//        #endregion Constructors

//        #region Properties

//        public override Vector2 InitialSize
//        {
//            get
//            {
//                return new Vector2( _width, _height );
//            }
//        }

//        public List<WorkTypeDef> WorkTypeDefs
//        {
//            get
//            {
//                if ( _workTypeDefs == null )
//                {
//                    RebuildWorkTypeDefsList();
//                }
//                return _workTypeDefs;
//            }
//        }

//        #endregion Properties

//        #region Methods

//        public override void DoWindowContents( Rect inRect )
//        {
//            // Close if work tab closed
//            if ( Find.WindowStack.WindowOfType<MainTabWindow_Work>() == null )
//            {
//                Find.WindowStack.TryRemove( this );
//            }

//            float areaHeight = ( inRect.height - 50f - _margin )/2f;

//            // the space reserved on the UI
//            Rect headerRect = new Rect( inRect.xMin, _margin, inRect.width, 50f - _margin );
//            Rect workTypeListRect = new Rect( inRect.xMin, 50f, inRect.width, areaHeight );
//            Rect workGiverListRect = new Rect( inRect.xMin, workTypeListRect.yMax + _margin, inRect.width, areaHeight );

//            // leave room for buttons
//            workGiverListRect.width -= _buttonSize.x + _margin;
//            workTypeListRect.width -= _buttonSize.x + _margin;

//            // button areas
//            Rect workTypeSortRect = new Rect( workTypeListRect.xMax + _margin, workTypeListRect.yMin, _buttonSize.x, areaHeight );
//            Rect workGiverSortRect = new Rect( workGiverListRect.xMax + _margin, workGiverListRect.yMin, _buttonSize.x, areaHeight );
//            Rect resetRect = new Rect( inRect.width - 27f, 13f - _margin / 2, 24f, 24f );

//            // draw backgrounds
//            Verse.Widgets.DrawMenuSection( workTypeListRect );
//            Verse.Widgets.DrawMenuSection( workGiverListRect );

//            // leave a tiny margin around the scrollbar if necessary
//            if ( _workTypeListHeight > workTypeListRect.height )
//            {
//                workTypeListRect.xMax -= 2f;
//            }
//            if ( _workGiverListHeight > workGiverListRect.height )
//            {
//                workGiverListRect.xMax -= 2f;
//            }

//            // the canvas for the (scrollable) lists
//            Rect workTypeListContent = new Rect( workTypeListRect.AtZero() );
//            Rect workGiverListContent = new Rect( workGiverListRect.AtZero() );

//            // set height to calculated height after first (every) pass.
//            workTypeListContent.height = _workTypeListHeight;
//            workGiverListContent.height = _workGiverListHeight;

//            // leave room for scrollbar if necessary.
//            workTypeListContent.width -= _workTypeListHeight > workTypeListRect.height ? 16f : 0f;
//            workGiverListContent.width -= _workGiverListHeight > workGiverListRect.height ? 16f : 0f;

//            // header
//            Text.Font = GameFont.Medium;
//            Widgets.Label( headerRect, "FluffyTabs.WorkPrioritiesDetails".Translate() );
//            Text.Font = GameFont.Small;

//            // reset button
//            if ( Widgets.ButtonImage( resetRect, Resources.ResetButton, "FluffyTabs.ResetPrioritiesTip".Translate() ) )
//            {
//                MapComponent_NaturalPriorities.ResetPriorities();

//                // rebuild def list (also notifies work tab and pawns of change)
//                RebuildWorkTypeDefsList();
//            }

//            // worktype lister
//            GUI.BeginGroup( workTypeListRect );
//            Verse.Widgets.BeginScrollView( workTypeListRect.AtZero(), ref _workTypeScrollPosition, workTypeListContent );

//            // keep track of position
//            Vector2 cur = Vector2.zero;

//            // draw the listings
//            foreach ( WorkTypeDef workType in WorkTypeDefs )
//            {
//                // move with selected when reordering
//                if ( _workTypeMoved && workType == _selectedWorkTypeDef )
//                {
//                    _workTypeScrollPosition.y = cur.y - 2 * _entryHeight;
//                    _workTypeMoved = false;
//                }
//                if ( DrawEntry( ref cur, workTypeListContent, workType == _selectedWorkTypeDef, workType ) )
//                {
//                    _selectedWorkTypeDef = workType;
//                    _selectedWorkGiverDef = null;
//                }
//            }

//            // set the actual height after having drawn everything
//            _workTypeListHeight = cur.y;

//            Verse.Widgets.EndScrollView();
//            GUI.EndGroup();

//            // draw buttons
//            DrawSortButtons( workTypeSortRect, _selectedWorkTypeDef != null, _selectedWorkTypeDef );

//            // START WORKGIVERS
//            if ( _selectedWorkTypeDef != null )
//            {
//                // workgiver lister
//                GUI.BeginGroup( workGiverListRect );
//                Verse.Widgets.BeginScrollView( workGiverListRect.AtZero(), ref _workGiverScrollPosition, workGiverListContent );

//                // keep track of position
//                cur = Vector2.zero;

//                // draw the listings
//                foreach ( WorkGiverDef workGiver in _selectedWorkTypeDef.workGiversByPriority )
//                {
//                    // move with selected when reordering
//                    if ( _workGiverMoved && workGiver == _selectedWorkGiverDef )
//                    {
//                        _workGiverScrollPosition.y = cur.y - 2 * _entryHeight;
//                        _workGiverMoved = false;
//                    }
//                    if ( DrawEntry( ref cur, workGiverListContent, workGiver == _selectedWorkGiverDef, _selectedWorkTypeDef, workGiver ) )
//                    {
//                        _selectedWorkGiverDef = workGiver;
//                    }
//                }

//                _workGiverListHeight = cur.y;

//                Verse.Widgets.EndScrollView();
//                GUI.EndGroup();

//                // draw buttons
//                DrawSortButtons( workGiverSortRect, _selectedWorkGiverDef != null, _selectedWorkTypeDef, _selectedWorkGiverDef );
//            }
//        }

//        public bool DrawEntry( ref Vector2 cur, Rect view, bool selected, WorkTypeDef worktype, WorkGiverDef workgiver = null )
//        {
//            // set some convenience variables
//            float width = view.width - cur.x - _margin;
//            float height = _entryHeight;
//            string label;
//            string tooltip = string.Empty;

//            // indent with the margin
//            cur.x += _margin;

//            // set label / tooltip
//            if ( workgiver == null )
//            {
//                label = worktype.labelShort;
//            }
//            else
//            {
//                label = Settings.WorkgiverLabels[workgiver];
//                tooltip = Settings.WorkgiverDescriptions[workgiver];
//            }

//            // decrease text size if label grows too big (probably will never happen).
//            if ( Text.CalcHeight( label, width ) > _entryHeight )
//            {
//                Text.Font = GameFont.Tiny;
//                float height2 = Text.CalcHeight( label, width );
//                height = Mathf.Max( height, height2 );
//            }

//            // draw the label
//            Text.Anchor = TextAnchor.MiddleLeft;
//            Rect labelRect = new Rect( cur.x, cur.y, width, height );
//            Widgets.Label( labelRect, label );
//            Text.Anchor = TextAnchor.UpperLeft;
//            Text.Font = GameFont.Small;

//            // set buttonRect for highlighting
//            Rect buttonRect = new Rect( view.xMin, cur.y, view.width, height );

//            Verse.Widgets.DrawHighlightIfMouseover( buttonRect );

//            // highlight if selected
//            if ( selected )
//            {
//                Verse.Widgets.DrawHighlightSelected( buttonRect );
//            }

//            // set a tooltip, since workgivers do not necessarily have unique labels
//            if ( tooltip != string.Empty )
//            {
//                TooltipHandler.TipRegion( buttonRect, tooltip );
//            }

//            // draw a line at the bottom to separate entries
//            cur.y += height;
//            GUI.color = Color.grey;
//            Verse.Widgets.DrawLineHorizontal( view.xMin, cur.y, view.width );
//            GUI.color = Color.white;

//            // reset cur.x
//            cur.x -= _margin;

//            // return click for selecting stuff.
//            return Verse.Widgets.ButtonInvisible( buttonRect );
//        }

//        public void DrawSortButtons( Rect rect, bool active, WorkTypeDef workType, WorkGiverDef workGiver = null )
//        {
//            bool top = false, bottom = false, isWorkType = false;

//            // no workType, should not be possible, but catch it nonetheless
//            if ( workType == null )
//            {
//                active = false;
//            }

//            // is this a workGiver, or a workType?
//            if ( workGiver == null )
//            {
//                isWorkType = true;
//            }

//            // for worktypes
//            if ( isWorkType )
//            {
//                int index = WorkTypeDefs.IndexOf( workType );
//                int count = WorkTypeDefs.Count;
//                if ( index == 0 )
//                    top = true;
//                if ( index == count - 1 )
//                    bottom = true;
//            }
//            else
//            // for workgivers
//            {
//                int index = workType.workGiversByPriority.IndexOf( workGiver );
//                int count = workType.workGiversByPriority.Count;
//                if ( index == 0 )
//                    top = true;
//                if ( index == count - 1 )
//                    bottom = true;
//            }

//            GUI.BeginGroup( rect );
//            Rect buttonRect = new Rect( 0f, 0f, _buttonSize.x, _buttonSize.y );
//            Rect topRect = buttonRect;
//            Rect upRect = buttonRect;
//            upRect.y = _buttonSize.y + _margin;
//            Rect downRect = buttonRect;
//            downRect.y = rect.height - _buttonSize.y * 2 - _margin;
//            Rect bottomRect = buttonRect;
//            bottomRect.y = rect.height - _buttonSize.y;

//            if ( active && !top )
//            {
//                if ( Widgets.ButtonImage( topRect, Resources.TopArrow, "FluffyTabs.SortTop".Translate() ) )
//                {
//                    if ( isWorkType )
//                    {
//                        Top( workType );
//                    }
//                    else
//                    {
//                        Top( workType, workGiver );
//                    }
//                }
//                if ( Widgets.ButtonImage( upRect, Resources.UpArrow, "FluffyTabs.SortUp".Translate() ) )
//                {
//                    if ( isWorkType )
//                    {
//                        Up( workType );
//                    }
//                    else
//                    {
//                        Up( workType, workGiver );
//                    }
//                }
//            }
//            else
//            {
//                GUI.color = Color.grey;
//                //Widgets.DrawBox( topRect );
//                //Widgets.DrawBox( upRect );
//                GUI.DrawTexture( topRect, Resources.TopArrow );
//                GUI.DrawTexture( upRect, Resources.UpArrow );
//                GUI.color = Color.white;
//            }

//            if ( active && !bottom )
//            {
//                if ( Widgets.ButtonImage( downRect, Resources.DownArrow, "FluffyTabs.SortDown".Translate() ) )
//                {
//                    if ( isWorkType )
//                    {
//                        Down( workType );
//                    }
//                    else
//                    {
//                        Down( workType, workGiver );
//                    }
//                }
//                if ( Widgets.ButtonImage( bottomRect, Resources.BottomArrow, "FluffyTabs.SortBottom".Translate() ) )
//                {
//                    if ( isWorkType )
//                    {
//                        Bottom( workType );
//                    }
//                    else
//                    {
//                        Bottom( workType, workGiver );
//                    }
//                }
//            }
//            else
//            {
//                GUI.color = Color.grey;
//                //Widgets.DrawBox( downRect );
//                //Widgets.DrawBox( bottomRect );
//                GUI.DrawTexture( downRect, Resources.DownArrow );
//                GUI.DrawTexture( bottomRect, Resources.BottomArrow );
//                GUI.color = Color.white;
//            }
//            GUI.EndGroup();
//        }

//        public override void PreOpen()
//        {
//            base.PreOpen();

//            // normalize priorities first time the tab is opened
//            // initialize the map component, making sure we have a valid backup and the map component is injected into the game
//            // finally, show a disclaimer the first time the tab is opened.
//            if ( _first )
//            {
//                _first = false;
//                RebuildWorkTypeDefsList();

//                foreach ( WorkTypeDef workTypeDef in _workTypeDefs )
//                {
//                    RebuildWorkGiverDefsList( workTypeDef );
//                }

//                MapComponent_NaturalPriorities.InitMapComponent();

//                if ( !MapComponent_NaturalPriorities.userWarned )
//                {
//                    MapComponent_NaturalPriorities.userWarned = true;
//                    Find.WindowStack.Add( new Dialog_Message( "FluffyTabs.WorkPrioritiesDisclaimer".Translate(), "FluffyTabs.WorkPrioritiesDisclaimerTitle".Translate() ) );
//                    Find.WindowStack.WindowOfType<Dialog_Message>().layer = WindowLayer.Super;
//                }
//            }
//        }

//        public void RebuildWorkGiverDefsList( WorkTypeDef workType )
//        {
//            // first reset the list, since core uses a cache
//            // normalization is irrelevant for order
//            workType.workGiversByPriority = workType.workGiversByPriority.OrderByDescending( wg => wg.priorityInType ).ToList();

//            // normalize
//            int max = workType.workGiversByPriority.Count;

//            for ( int i = 0, j = max; i < max; i++, j-- )
//            {
//                workType.workGiversByPriority[i].priorityInType = j;
//            }

//            // notify drawer to change scrollPosition
//            _workGiverMoved = true;

//            // notify all pawns to recache their internal priorities
//            Notify_PrioritiesChanged();
//        }

//        private void Bottom( WorkTypeDef workType )
//        {
//            // set to top, i.e. 0
//            workType.naturalPriority = 0;

//            // rebuild def list (also notifies work tab and pawns of change)
//            RebuildWorkTypeDefsList();
//        }

//        private void Bottom( WorkTypeDef workType, WorkGiverDef workGiver )
//        {
//            // set to bottom, i.e. 0
//            workGiver.priorityInType = 0;

//            // rebuild the list to ensure everything stays normalized and is displayed correctly.
//            RebuildWorkGiverDefsList( workType );
//        }

//        private void Down( WorkTypeDef workType )
//        {
//            // down, so decrease priority
//            // we actually switch priorities with the next lowest
//            WorkTypeDef next =
//                DefDatabase<WorkTypeDef>.AllDefs.OrderByDescending( wtd => wtd.naturalPriority )
//                    .First( wtd => wtd.naturalPriority < workType.naturalPriority );

//            // bumping by one works because we normalized priorities.
//            workType.naturalPriority -= 1;
//            next.naturalPriority += 1;

//            // rebuild def list (also notifies work tab and pawns of change)
//            RebuildWorkTypeDefsList();
//        }

//        private void Down( WorkTypeDef workType, WorkGiverDef workGiver )
//        {
//            // down, so decrease priority
//            // we actually switch priorities with the next highest
//            // workGivers come pre-sorted in descending order
//            WorkGiverDef next = workType.workGiversByPriority.First( wgd => wgd.priorityInType < workGiver.priorityInType );

//            // bumping by one works because we normalized priorities.
//            workGiver.priorityInType -= 1;
//            next.priorityInType += 1;

//            // rebuild the list to ensure everything stays normalized and is displayed correctly.
//            RebuildWorkGiverDefsList( workType );
//        }

//        private void Notify_PrioritiesChanged()
//        {
//            _mainTab.Notify_NaturalPrioritiesChanged();
//        }

//        private void RebuildWorkTypeDefsList()
//        {
//            _workTypeDefs = DefDatabase<WorkTypeDef>.AllDefs.OrderByDescending( wt => wt.naturalPriority ).ToList();
//            int max = _workTypeDefs.Count;

//            for ( int i = 0, j = max; i < max; i++, j-- )
//            {
//                _workTypeDefs[i].naturalPriority = j;
//            }

//            // notify drawer to change scrollPosition
//            _workTypeMoved = true;

//            // notify all pawns to recache their internal priorities
//            Notify_PrioritiesChanged();
//        }

//        private void Top( WorkTypeDef workType )
//        {
//            // set to top, i.e. count + 1
//            workType.naturalPriority = WorkTypeDefs.Count + 1;

//            // rebuild def list (also notifies work tab and pawns of change)
//            RebuildWorkTypeDefsList();
//        }

//        private void Top( WorkTypeDef workType, WorkGiverDef workGiver )
//        {
//            // set to top, i.e. count + 1
//            workGiver.priorityInType = workType.workGiversByPriority.Count + 1;

//            // rebuild the list to ensure everything stays normalized and is displayed correctly.
//            RebuildWorkGiverDefsList( workType );
//        }

//        private void Up( WorkTypeDef workType )
//        {
//            // up, so increase priority
//            // we actually switch priorities with the next highest
//            WorkTypeDef next =
//                DefDatabase<WorkTypeDef>.AllDefs.OrderBy( wtd => wtd.naturalPriority )
//                    .First( wtd => wtd.naturalPriority > workType.naturalPriority );

//            // bumping by one works because we normalized priorities.
//            workType.naturalPriority += 1;
//            next.naturalPriority -= 1;

//            // rebuild def list (also notifies work tab and pawns of change)
//            RebuildWorkTypeDefsList();
//        }

//        private void Up( WorkTypeDef workType, WorkGiverDef workGiver )
//        {
//            // up, so increase priority
//            // we actually switch priorities with the next highest
//            WorkGiverDef next = workType.workGiversByPriority.OrderBy( wgd => wgd.priorityInType ).First( wgd => wgd.priorityInType > workGiver.priorityInType );

//            // bumping by one works because we normalized priorities.
//            workGiver.priorityInType += 1;
//            next.priorityInType -= 1;

//            // rebuild the list to ensure everything stays normalized and is displayed correctly.
//            RebuildWorkGiverDefsList( workType );
//        }

//        #endregion Methods
//    }
//}