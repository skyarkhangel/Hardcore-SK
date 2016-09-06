using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using System.Reflection;

namespace Fluffy_Tabs
{
    public enum SortMode
    {
        Name,
        WorkType,
        Mood,
        Activity,
        Vanilla
    }

    public class MainTabWindow_Work : MainTabWindow_PawnList
    {
        #region Fields

        private float _columnHeaderRowHeight_Workgivers = 150f;
        private float _columnHeaderRowHeight_Worktypes  = 50f;
        private List<int> _focusedHours;
        private int _horizontalScrollPos                = 0;
        private float _margin                           = Settings.Margin;
        private float _schedulerRowHeight               = 40f;
        private bool _sortAscending                     = false;
        private WorkTypeDef _sortedWorkType;
        private SortMode _sortMode                      = SortMode.Vanilla;
        private List<int> _tempFocusedHours;
        private int _tempFocusedHoursFor = -1;
        private bool _tempFocusFullDay = true;
        private float _topRowHeight                     = 25f;
        private List<WorkGiverDef> _workgiversOrdered;
        private List<WorkTypeDef> _worktypesOrdered;

        private MethodInfo _preDrawPawnRow;
        private MethodInfo _postDrawPawnRow;
        private FieldInfo _pawnListDirty;

        #endregion Fields

        public MainTabWindow_Work()
        {
            _preDrawPawnRow = typeof( MainTabWindow_PawnList ).GetMethod( "PreDrawPawnRow", BindingFlags.Instance | BindingFlags.NonPublic );
            _postDrawPawnRow = typeof( MainTabWindow_PawnList ).GetMethod( "PostDrawPawnRow", BindingFlags.Instance | BindingFlags.NonPublic );
            _pawnListDirty = typeof( MainTabWindow_PawnList ).GetField( "pawnListDirty", BindingFlags.Instance | BindingFlags.NonPublic );
        }

        #region Properties

        public float ColumnHeaderRowHeight
        {
            get
            {
                return DwarfTherapistMode ? _columnHeaderRowHeight_Workgivers : _columnHeaderRowHeight_Worktypes;
            }
        }

        public bool DwarfTherapistMode
        {
            get
            {
                return MapComponent_Priorities.Instance.DwarfTherapistMode;
            }
            set
            {
                MapComponent_Priorities.Instance.DwarfTherapistMode = value;
            }
        }

        public List<int> FocusedHours
        {
            get
            {
                if ( _focusedHours == null || _focusedHours.Count == 0 )
                {
                    if ( _tempFocusedHours == null || _tempFocusedHoursFor != GenDate.HourOfDay )
                    {
                        _tempFocusedHours = new List<int>();
                        _tempFocusedHours.Add( GenDate.HourOfDay );
                        if ( TempFocusFullDay )
                        {
                            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
                            {
                                if ( hour != GenDate.HourOfDay )
                                    _tempFocusedHours.Add( hour );
                            }
                        }
                        _tempFocusedHoursFor = GenDate.HourOfDay;
                    }
                    return _tempFocusedHours;
                }
                return _focusedHours;
            }
        }

        public override Vector2 InitialSize
        {
            get
            {
                Vector2 requestedTabSize = RequestedTabSize;
                if ( requestedTabSize.x > (float)Screen.width )
                {
                    // limit to screen width
                    requestedTabSize.x = (float)Screen.width;

                    // add some extra height for horizontal scrollbar
                    requestedTabSize.y += 16f;
                }
                if ( requestedTabSize.y > (float)( Screen.height - 35 ) )
                {
                    // limit to screen height - space for bottom menu bar
                    requestedTabSize.y = (float)( Screen.height - 35 );
                }
                return requestedTabSize;
            }
        }

        public override Vector2 RequestedTabSize
        {
            get
            {
                return new Vector2(
                    ExtraWidth + DesiredWorkAreaWidth,
                    2 * Margin + ColumnHeaderRowHeight + _margin + _topRowHeight + pawns.Count * PawnRowHeight + SchedulerRowHeight );
            }
        }

        public bool SchedulerMode
        {
            get
            {
                return MapComponent_Priorities.Instance.SchedulerMode;
            }
            set
            {
                MapComponent_Priorities.Instance.SchedulerMode = value;
            }
        }

        public float SchedulerRowHeight => SchedulerMode ? _schedulerRowHeight : 0f;

        public bool TempFocusFullDay
        {
            get { return _tempFocusFullDay; }
            set
            {
                _tempFocusFullDay = value;
                _tempFocusedHours = null;
                _tempFocusedHoursFor = -1;
            }
        }

        public IntVec2 VisibleWorkColumns
        {
            get
            {
                if ( windowRect.width >= RequestedTabSize.x )
                {
                    return new IntVec2( 0, WorkColumnCount );
                }
                else
                {
                    return new IntVec2( _horizontalScrollPos, Mathf.Min( _horizontalScrollPos + VisibleWorkColumnsCount, WorkColumnCount ) );
                }
            }
        }

        public int VisibleWorkColumnsCount => Mathf.FloorToInt( ActualWorkAreaWidth / WorkColumnWidth );

        public int WorkColumnCount => DwarfTherapistMode ? WorkgiversOrdered.Count : WorktypesOrdered.Count;

        public List<WorkGiverDef> WorkgiversOrdered
        {
            get
            {
                if ( _workgiversOrdered == null )
                    _workgiversOrdered = DefDatabase<WorkGiverDef>.AllDefsListForReading.OrderByDescending( wg => wg.workType.naturalPriority ).ThenByDescending( wg => wg.priorityInType ).ToList();
                return _workgiversOrdered;
            }
        }

        public List<WorkTypeDef> WorktypesOrdered
        {
            get
            {
                if ( _worktypesOrdered == null )
                    _worktypesOrdered = DefDatabase<WorkTypeDef>.AllDefsListForReading.OrderByDescending( wg => wg.naturalPriority ).ToList();
                return _worktypesOrdered;
            }
        }

        internal bool NumericMode
        {
            get
            {
                return Find.PlaySettings.useWorkPriorities;
            }
            set
            {
                if ( Find.PlaySettings.useWorkPriorities != value )
                    MapComponent_Priorities.NotifyAll_PrioritiesChanged();
                Find.PlaySettings.useWorkPriorities = value;
            }
        }

        private float ActualWorkAreaWidth => windowRect.width - ExtraWidth;

        private float BoxMargin => DwarfTherapistMode ? 2f : 8f;

        private float BoxSize => DwarfTherapistMode ? Settings.WorkgiverBoxSize : Settings.WorktypeBoxSize;

        private float DesiredWorkAreaWidth => WorkColumnCount * WorkColumnWidth;

        private float ExtraWidth => Margin * 2 + NameColumnWidth + StatusColumnWidth + FavouritesColumnWidth + 4 * Settings.Margin;

        private float FavouritesColumnWidth => WorkColumnWidth;

        private string LabelDown => NumericMode ? "FluffyTabs.DecreasePriority".Translate() : "FluffyTabs.ToggleOff".Translate();

        private string LabelUp => NumericMode ? "FluffyTabs.IncreasePriority".Translate() : "FluffyTabs.ToggleOn".Translate();

        private float StatusColumnWidth => 2 * WorkColumnWidth;

        private float WorkColumnWidth => BoxSize + BoxMargin;

        #endregion Properties

        #region Methods

        public override void DoWindowContents( Rect canvas )
        {
            // there's an annoying vanilla bug that sets the default font to tiny when fully zoomed in
            // make sure it's always set to small.
            Text.Font = GameFont.Small;

            // initial setup stuff ( size & position )
            base.DoWindowContents( canvas );

            // set up rects
            Rect topRow = new Rect( canvas );
            topRow.height = _topRowHeight;
            Rect headerRow = new Rect( canvas );
            headerRow.yMin = topRow.yMax + _margin;
            headerRow.height = ColumnHeaderRowHeight;
            Rect contentArea = new Rect( canvas );
            contentArea.yMin = headerRow.yMax;

            // if we required extra horizontal space, we'll need to explicitly make some space for the horizontal scrollbar
            if ( RequestedTabSize.x > windowRect.width )
                contentArea.yMax -= 16f;

            // if we're using the scheduler, make some room for the timebar
            if ( SchedulerMode )
                contentArea.yMax -= SchedulerRowHeight;

            // draw top buttons and priority labels.
            DrawTopButtons( topRow );
            DrawPriorityLabels( topRow );

            // draw column headers
            DrawColumnHeaders( headerRow );
            
            // draw pawn rows
            DrawRows( contentArea );

            // draw horizontal scrollbar if needed
            DrawColumnScrollbar( new Rect( canvas.xMin + NameColumnWidth + StatusColumnWidth, canvas.yMax - 16f - SchedulerRowHeight, ActualWorkAreaWidth, 16f ) );

            // draw scheduler if required
            if ( SchedulerMode )
                DrawScheduler( new Rect( canvas.xMin + NameColumnWidth + StatusColumnWidth, canvas.yMax - SchedulerRowHeight, ActualWorkAreaWidth, SchedulerRowHeight ) );
        }

        /// <summary>
        /// We have to override DrawRows because vanilla's Widgest.BeginScrollView() wrapper around GUI.BeginScrollView() breaks scrollwheel events for some portions of the contained area...
        /// Annoyingly, that also means we have to use reflected calls to Pre- and PostDrawPawnRow.
        /// </summary>
        /// <param name="outRect"></param>
        protected new void DrawRows( Rect outRect )
        {
            // have to use reflection to get to the private dirty field. 
            // we could directly call BuildPawnList in the SortBy method, but that would bypass Notify_PawnListChanged - and ignore changes in colonist count, etc.
            if ( (bool)_pawnListDirty.GetValue( this ) )
                BuildPawnList();

            Rect viewRect = new Rect( 0f, 0f, outRect.width - 16f, (float)pawns.Count * 30f );
            scrollPosition = GUI.BeginScrollView( outRect, scrollPosition, viewRect );
            float num = 0f;
            for ( int i = 0; i < pawns.Count; i++ )
            {
                Pawn p = pawns[i];
                Rect rect = new Rect( 0f, num, viewRect.width, 30f );
                if ( num - scrollPosition.y + 30f >= 0f && num - scrollPosition.y <= outRect.height )
                {
                    GUI.color = new Color( 1f, 1f, 1f, 0.2f );
                    Verse.Widgets.DrawLineHorizontal( 0f, num, viewRect.width );
                    GUI.color = Color.white;
                    PreDrawPawnRow( rect, p );
                    DrawPawnRow( rect, p );
                    PostDrawPawnRow( rect, p );
                }
                num += 30f;
            }
            GUI.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public void PreDrawPawnRow( Rect rect, Pawn p )
        {
            _preDrawPawnRow.Invoke( this, new object[] { rect, p } );
        }

        public void PostDrawPawnRow( Rect rect, Pawn p )
        {
            _postDrawPawnRow.Invoke( this, new object[] { rect, p } );
        }

        public void DrawScheduler( Rect canvas )
        {
            // split the available area into rects. bottom 2/3's are used for 'buttons', with text for times.
            float hourWidth = canvas.width / GenDate.HoursPerDay;
            float barheight = canvas.height * 2/3f;
            float timeIndicatorSize = canvas.height * 2/3f;
            Rect hourRect = new Rect( canvas.xMin, canvas.yMax - barheight, hourWidth, barheight );

            // draw buttons
            Rect buttonRect = new Rect( canvas.xMax + Settings.Margin * 2 + ( FavouritesColumnWidth - timeIndicatorSize ) * 1/2f, canvas.yMin + ( canvas.height - timeIndicatorSize ) * 2/3f, timeIndicatorSize, timeIndicatorSize );

            // something selected, click icon to go to full day
            if ( _focusedHours != null &&
                _focusedHours.Count > 0 &&
                Widgets.ButtonImage( buttonRect, Resources.Priorities_WholeDay, "FluffyTabs.SchedulerWholeDay".Translate() ) )
            {
                _focusedHours = null;
                TempFocusFullDay = true;
            }

            // nothing selected, click icon toggle between full day and current hour
            if ( ( _focusedHours == null || _focusedHours.Count == 0 ) &&
                 TempFocusFullDay == true &&
                 Widgets.ButtonImage( buttonRect, Resources.ClockNow, "FluffyTabs.SchedulerCurrentTimeOnly".Translate() ) )
            {
                TempFocusFullDay = false;
            }
            if ( ( _focusedHours == null || _focusedHours.Count == 0 ) &&
                 TempFocusFullDay == false &&
                 Widgets.ButtonImage( buttonRect, Resources.Priorities_WholeDay, "FluffyTabs.SchedulerWholeDay".Translate() ) )
            {
                TempFocusFullDay = true;
            }

            // draw first tick
            GUI.color = Color.grey;
            Verse.Widgets.DrawLineVertical( hourRect.xMin, hourRect.yMin + hourRect.height * 1/2f, hourRect.height * 1/2f );

            // draw horizontal line ( y - 1 because canvas gets clipped on bottom )
            Verse.Widgets.DrawLineHorizontal( canvas.xMin, canvas.yMax - 1, canvas.width );
            GUI.color = Color.white;

            // label and rect
            string label;
            Rect labelRect;

            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                // print major tick
                GUI.color = Color.grey;
                Verse.Widgets.DrawLineVertical( hourRect.xMax, hourRect.yMin + hourRect.height * 1/2f, hourRect.height * 1/2f );

                // print minor ticks
                Verse.Widgets.DrawLineVertical( hourRect.xMin + hourRect.width * 1/4f, hourRect.yMin + hourRect.height * 3/4f, hourRect.height * 1/4f );
                Verse.Widgets.DrawLineVertical( hourRect.xMin + hourRect.width * 2/4f, hourRect.yMin + hourRect.height * 3/4f, hourRect.height * 1/4f );
                Verse.Widgets.DrawLineVertical( hourRect.xMin + hourRect.width * 3/4f, hourRect.yMin + hourRect.height * 3/4f, hourRect.height * 1/4f );
                GUI.color = Color.white;

                // create and draw labelrect - only for every other tick if we're not in dwarf therapist mode
                if ( DwarfTherapistMode || hour % 2 == 0 )
                {
                    label = hour.FormatHour();
                    labelRect = new Rect( 0f, canvas.yMin + canvas.height * 1/3f, label.NoWrapWidth(), canvas.height * 2/3f );
                    labelRect.x = hourRect.xMin - labelRect.width / 2f;
                    Widgets.Label( labelRect, label, Color.grey, GameFont.Tiny, TextAnchor.UpperCenter );
                }

                // draw hour rect with mouseover + interactions
                Verse.Widgets.DrawHighlightIfMouseover( hourRect );

                // set/remove focus (LMB and any other MB respectively)
                if ( Mouse.IsOver( hourRect ) )
                {
                    if ( Input.GetMouseButton( 0 ) )
                        SetFocusedHour( hour, true );
                    if ( Input.GetMouseButton( 1 ) )
                        SetFocusedHour( hour, false );
                }

                // if this is currently the 'main' timeslot, and not the actual time, draw an eye
                if ( hour == FocusedHours.FirstOrDefault() && hour != GenDate.HourOfDay )
                {
                    Rect eyeRect = new Rect( hourRect.center.x - timeIndicatorSize * 1/2f, hourRect.yMax - timeIndicatorSize - hourRect.height * 1/6f, timeIndicatorSize, timeIndicatorSize );
                    GUI.DrawTexture( eyeRect, Resources.PinEye );
                }

                // also highlight all selected timeslots
                if ( FocusedHours.Contains( hour ) )
                    Verse.Widgets.DrawHighlightSelected( hourRect );

                // advance rect
                hourRect.x += hourRect.width;
            }

            // draw final label
            label = 0.FormatHour();
            labelRect = new Rect( 0f, canvas.yMin + canvas.height * 1/3f, label.NoWrapWidth(), canvas.height * 2/3f );
            labelRect.x = hourRect.xMin - labelRect.width / 2f;
            Widgets.Label( labelRect, label, Color.grey, GameFont.Tiny, TextAnchor.UpperCenter );

            // draw current time indicator
            float curTimeX = GenDate.CurrentDayPercent * canvas.width;
            Rect curTimeRect = new Rect( canvas.xMin + curTimeX - timeIndicatorSize * 1/2f, hourRect.yMax - timeIndicatorSize - hourRect.height * 1/6f, timeIndicatorSize, timeIndicatorSize );
            GUI.DrawTexture( curTimeRect, Resources.PinClock );
        }

        public void Notify_NaturalPrioritiesChanged()
        {
            // reset list so it gets rebuild
            _worktypesOrdered = null;
            _workgiversOrdered = null;

            // notify pawns that they should update
            MapComponent_Priorities.NotifyAll_PrioritiesChanged();
        }

        public void SetFocusedHour( int hour, bool add )
        {
            // copy from temp ( whole day ) if not exists
            if ( _focusedHours == null || _focusedHours.Count == 0 )
                _focusedHours = new List<int>( _tempFocusedHours );

            // adding
            if ( add )
            {
                if ( _focusedHours.Contains( hour ) )
                {
                    if ( _focusedHours.First() != hour )
                    {
                        _focusedHours.Remove( hour );
                        _focusedHours.Insert( 0, hour );
                    }
                }
                else
                {
                    _focusedHours.Add( hour );
                }
            }

            // removing
            else
            {
                if ( _focusedHours.Contains( hour ) )
                    _focusedHours.Remove( hour );
            }
        }

        public void SortBy( SortMode mode, WorkTypeDef worktype )
        {
#if DEBUG
            Log.Message( "Work Tab :: Changing order :: " + _sortMode + " -> " +  mode  );
#endif
            if ( _sortMode != mode )
            {
                _sortMode = mode;
                if ( mode == SortMode.WorkType && worktype != null )
                    _sortedWorkType = worktype;
            }
            else
            {
                if ( mode == SortMode.WorkType )
                {
                    if ( _sortedWorkType == worktype )
                        _sortAscending = !_sortAscending;
                    else
                    {
                        _sortedWorkType = worktype;
                        _sortAscending = false;
                    }
                }
                else
                {
                    _sortAscending = !_sortAscending;
                }
            }
            Notify_PawnsChanged();
        }

        protected override void BuildPawnList()
        {
            _pawnListDirty.SetValue( this, false );
            pawns = Find.MapPawns.FreeColonists.ToList();

            if ( !pawns.Any() )
                return;

            switch ( _sortMode )
            {
                case SortMode.Name:
                    pawns = pawns.OrderBy( p => p.Name.ToStringFull ).ToList();
                    break;

                case SortMode.WorkType:
                    pawns = pawns.OrderBy( p => p.skills.AverageOfRelevantSkillsFor( _sortedWorkType ) ).ToList();
                    break;

                case SortMode.Activity:
                    pawns = pawns.OrderBy( p => p.CurJob?.def.defName ?? "" ).ToList();
                    break;

                case SortMode.Mood:
                    pawns = pawns.OrderByDescending( p => p.needs.mood.CurLevelPercentage ).ToList();
                    break;

                case SortMode.Vanilla:
                    break;

                default:
                    break;
            }

            if ( !_sortAscending )
                pawns.Reverse();
        }

        protected override void DrawPawnRow( Rect canvas, Pawn pawn )
        {
            // name is handled in PreDrawPawnRow

            // set up rects
            Rect row = new Rect( NameColumnWidth, canvas.yMin, DesiredWorkAreaWidth + StatusColumnWidth + FavouritesColumnWidth, canvas.height );
            Rect cell = new Rect( row.xMin, row.yMin, WorkColumnWidth, row.height );
            
            // draw status cells
            Widgets.DrawStatusCell( cell, BoxSize * .8f, pawn );
            cell.x += WorkColumnWidth;
            Widgets.DrawMoodCell( cell, BoxSize * .8f, pawn );
            cell.x += WorkColumnWidth;

            // some margin
            cell.x += Settings.Margin;

            // draw work cells
            for ( int i = VisibleWorkColumns.x; i < VisibleWorkColumns.z; i++ )
            {
                if ( DwarfTherapistMode )
                    Widgets.DrawWorkBoxFor( WorkgiversOrdered[i], cell, pawn, SchedulerMode, FocusedHours );
                else
                    Widgets.DrawWorkBoxFor( WorktypesOrdered[i], cell, pawn, SchedulerMode, FocusedHours );
                cell.x += WorkColumnWidth;
            }

            // some margin
            cell.x += Settings.Margin;

            // draw preset cell
            Widgets.DrawFavouritesCell( cell, BoxSize * .8f, pawn, DwarfTherapistMode );
        }

        private void DrawColumnHeaders( Rect canvas )
        {
            // name header
            Rect nameHeaderRect = new Rect( canvas.xMin, canvas.yMin, NameColumnWidth, canvas.height );
            Widgets.Label( nameHeaderRect, "FluffyTabs.Name".Translate(), TextAnchor.LowerCenter, "FluffyTabs.NameColumnTip".Translate() );
            Verse.Widgets.DrawHighlightIfMouseover( nameHeaderRect );
            if ( Verse.Widgets.ButtonInvisible( nameHeaderRect ) )
                HandleNameColumnInteractions();

            // prepare for drawing icons ( at bottom - icon size )
            Vector2 offset = new Vector2( NameColumnWidth + BoxMargin / 2f, canvas.yMax - BoxSize - BoxMargin / 2f );

            // status column headers
            if ( Widgets.ButtonImage( ref offset, Direction.Right, Resources.Cog, "FluffyTabs.CurrentJobColumnTip".Translate(), BoxSize, BoxMargin ) )
                SortBy( SortMode.Activity, null );
            if ( Widgets.ButtonImage( ref offset, Direction.Right, Resources.MoodContent, "FluffyTabs.CurrentMoodColumnTip".Translate(), BoxSize, BoxMargin ) )
                SortBy( SortMode.Mood, null );

            // set offset y to top
            offset.y = canvas.yMin;
            offset.x += Settings.Margin;

            // draw work headers
            for ( int i = VisibleWorkColumns.x; i < VisibleWorkColumns.z; i++ )
            {
                if ( DwarfTherapistMode )
                    DrawWorkHeader( WorkgiversOrdered[i], offset, WorkColumnWidth, canvas.height, i - VisibleWorkColumns.x );
                else
                    DrawWorkHeader( WorktypesOrdered[i], offset, WorkColumnWidth, canvas.height / 2f, i - VisibleWorkColumns.x );
            }

            // no header for favourites column
            // TODO: Add mass-favourites option?
        }

        private void DrawColumnScrollbar( Rect canvas )
        {
            if ( windowRect.width < RequestedTabSize.x )
            {
                var scrollpos = GUI.HorizontalScrollbar( canvas, _horizontalScrollPos * WorkColumnWidth, ActualWorkAreaWidth, 0f, DesiredWorkAreaWidth );
                _horizontalScrollPos = Mathf.RoundToInt( scrollpos / WorkColumnWidth );

                if ( Mouse.IsOver( canvas ) && Event.current.type == EventType.ScrollWheel )
                {
                    if ( Event.current.delta.y > 0 )
                        _horizontalScrollPos = Mathf.Clamp( _horizontalScrollPos - 1, 0, WorkColumnCount - VisibleWorkColumnsCount );
                    else if ( Event.current.delta.y < 0 )
                        _horizontalScrollPos = Mathf.Clamp( _horizontalScrollPos + 1, 0, WorkColumnCount - VisibleWorkColumnsCount );

                    // stop event bubbling through and zooming game map.
                    Event.current.Use();
                }
            }
        }

        private void DrawPriorityLabels( Rect canvas )
        {
            // set up rects
            Rect loPrio = new Rect( canvas );
            Rect hiPrio = new Rect( canvas );
            loPrio.width = hiPrio.width = canvas.width / 4f;
            hiPrio.x = canvas.width / 4f;
            loPrio.x = canvas.width / 2f;

            // draw labels
            Widgets.Label( hiPrio, "FluffyTabs.HighPriorityLabel".Translate(), Color.grey, GameFont.Tiny, TextAnchor.MiddleCenter );
            Widgets.Label( loPrio, "FluffyTabs.LowPriorityLabel".Translate(), Color.grey, GameFont.Tiny, TextAnchor.MiddleCenter );
        }

        private void DrawTopButtons( Rect canvas )
        {
            // draw buttons in top right
            Vector2 curPos = new Vector2( canvas.xMax, canvas.yMin );

            //// open natural priority changer (left-right ordering)
            //if ( Widgets.ButtonImage( ref curPos, Direction.Left, Resources.Wrench, "FluffyTabs.WorkNaturalPrioritiesTip".Translate() ) )
            //    Find.WindowStack.Add( new Dialog_NaturalPriorities( this ) );

            // button for changing between worktype and workgiver modes
            if ( DwarfTherapistMode )
            {
                if ( Widgets.ButtonImage( ref curPos, Direction.Left, Resources.Priorities_Workgivers, "FluffyTabs.PrioritiesWorktypesTip".Translate() ) )
                    DwarfTherapistMode = false;
            }
            else
            {
                if ( Widgets.ButtonImage( ref curPos, Direction.Left, Resources.Priorities_Worktypes, "FluffyTabs.PrioritiesWorkgiversTip".Translate() ) )
                    DwarfTherapistMode = true;
            }

            // button for toggling scheduler mode
            if ( SchedulerMode )
            {
                if ( Widgets.ButtonImage( ref curPos, Direction.Left, Resources.Priorities_Scheduler, "FluffyTabs.PrioritiesSchedulerTip".Translate() ) )
                    SchedulerMode = false;
            }
            else
            {
                if ( Widgets.ButtonImage( ref curPos, Direction.Left, Resources.Priorities_WholeDay, "FluffyTabs.PrioritiesAllDayTip".Translate() ) )
                    SchedulerMode = true;
            }

            // button for changing between manual and simple workSettings
            if ( NumericMode )
            {
                if ( Widgets.ButtonImage( ref curPos, Direction.Left, Resources.Priorities_Int, "FluffyTabs.PrioritiesToggleTip".Translate() ) )
                    NumericMode = false;
            }
            else
            {
                if ( Widgets.ButtonImage( ref curPos, Direction.Left, Resources.Priorities_Toggle, "FluffyTabs.PrioritiesIntTip".Translate() ) )
                    NumericMode = true;
            }
        }

        private void DrawWorkHeader( WorkTypeDef worktype, Vector2 offset, float width, float height, int i )
        {
            // width is slightly deceptive, it is actually the _center_ of the rect that we want to create that we're interested in.
            Rect rect = new Rect( offset.x, offset.y, worktype.labelShort.NoWrapWidth(), height );

            // move rect to correct x position
            rect.x += ( i + .5f ) * width - rect.width / 2f;

            // move rect to correct (alternating) y position
            rect.y += ( i % 2 == 1 ) ? height : 0f;

            // draw worktype.labelShort
            Widgets.Label(
                rect,
                worktype.labelShort,
                TextAnchor.MiddleCenter,
               "FluffyTabs.WorktypeHeaderTip".Translate( worktype.labelShort, LabelUp, LabelDown, worktype.description ) );

            // handle interactions
            HandleWorkColumnInteractions( worktype, rect );
            Verse.Widgets.DrawHighlightIfMouseover( rect );

            // draw vertical line
            Verse.Widgets.DrawLine( new Vector2( rect.center.x, rect.yMax - 3f ), new Vector2( rect.center.x, offset.y + 2 * height ), Color.gray, 1f );
        }

        private void DrawWorkHeader( WorkGiverDef workgiver, Vector2 offset, float width, float height, int i )
        {
            // create rect
            Rect rect = new Rect( offset.x + width * i, offset.y, width, height );

            // handle interactions
            HandleWorkColumnInteractions( workgiver, rect );
            Widgets.DrawBackground( rect, Settings.WorktypeColors[workgiver.workType], Settings.WorkgiverColorOpacity );
            Verse.Widgets.DrawHighlightIfMouseover( rect );
            TooltipHandler.TipRegion( rect, "FluffyTabs.WorkgiverHeaderTip".Translate(
                Settings.WorkgiverLabels[workgiver],
                LabelUp,
                LabelDown,
                workgiver.workType.gerundLabel.ToLower(),
                Settings.WorkgiverDescriptions[workgiver] ) );

            // draw rotated label
            var oldWW = Text.WordWrap;
            Text.WordWrap = false;
            Widgets.LabelVertical(
                rect,
                // add space to give a tad of margin
                " " + Settings.WorkgiverLabels[workgiver],
                Color.white,
                GameFont.Small,
                TextAnchor.MiddleLeft );
            Text.WordWrap = oldWW;
        }

        private void HandleNameColumnInteractions()
        {
            if ( Event.current.button == 0 )
                SortBy( SortMode.Name, null );
            if ( Event.current.button == 1 )
                SortBy( SortMode.Vanilla, null );
        }

        private void HandleWorkColumnInteractions( WorkGiverDef workgiver, Rect canvas )
        {
            if ( Mouse.IsOver( canvas ) )
            {
                // sorting
                if ( Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Event.current.shift )
                    SortBy( SortMode.WorkType, workgiver.workType );

                // whole column in-/de-crement
                if ( Event.current.shift )
                {
                    if ( ( Event.current.type == EventType.MouseUp && Event.current.button == 0 ) || ( NumericMode && Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f ) )
                    {
                        workgiver.IncrementPriorities( pawns, !NumericMode, SchedulerMode, FocusedHours );
                        Event.current.Use();
                    }
                    if ( ( Event.current.type == EventType.MouseUp && Event.current.button == 1 ) || ( NumericMode && Event.current.type == EventType.ScrollWheel && Event.current.delta.y > 0f ) )
                    {
                        workgiver.DecrementPriorities( pawns, !NumericMode, SchedulerMode, FocusedHours );
                        Event.current.Use();
                    }
                }
            }
        }

        private void HandleWorkColumnInteractions( WorkTypeDef worktype, Rect canvas )
        {
            if ( Mouse.IsOver( canvas ) )
            {
                // sorting
                if ( Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Event.current.shift )
                    SortBy( SortMode.WorkType, worktype );

                // whole column in-/de-crement
                if ( Event.current.shift )
                {
                    if ( ( Event.current.type == EventType.MouseUp && Event.current.button == 0 ) || ( NumericMode && Event.current.type == EventType.ScrollWheel && Event.current.delta.y < 0f ) )
                    {
                        worktype.IncrementPriorities( pawns, !NumericMode, SchedulerMode, FocusedHours );
                        Event.current.Use();
                    }
                    if ( ( Event.current.type == EventType.MouseUp && Event.current.button == 1 ) || ( NumericMode && Event.current.type == EventType.ScrollWheel && Event.current.delta.y > 0f ) )
                    {
                        worktype.DecrementPriorities( pawns, !NumericMode, SchedulerMode, FocusedHours );
                        Event.current.Use();
                    }
                }
            }
        }

        #endregion Methods
    }
}