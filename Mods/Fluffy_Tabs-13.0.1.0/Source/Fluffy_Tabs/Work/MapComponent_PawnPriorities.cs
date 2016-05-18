using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Tabs
{
    public class MapComponent_PawnPriorities : MapComponent
    {
        #region Fields

        private static MapComponent_PawnPriorities _instance;
        private bool _24Hours = true;
        private bool _dwarfTherapistMode = false;
        private int _hourOfDay = 0;
        private bool _schedulerMode = false;
        private Dictionary<Pawn, PawnWorkgiverPrioritiesTracker> _workgiverTrackers = new Dictionary<Pawn, PawnWorkgiverPrioritiesTracker>();
        private List<PawnWorkgiverPrioritiesTracker> _workgiverTrackersScribe;
        private Dictionary<Pawn, PawnWorktypePrioritiesTracker> _worktypeTrackers = new Dictionary<Pawn, PawnWorktypePrioritiesTracker>();
        private List<PawnWorktypePrioritiesTracker> _worktypeTrackersScribe;

        #endregion Fields

        #region Properties

        public static MapComponent_PawnPriorities Instance
        {
            get
            {
                // try to get from game list.
                if ( _instance == null )
                    _instance = Find.Map.components.FirstOrDefault( comp => comp is MapComponent_PawnPriorities ) as MapComponent_PawnPriorities;

                // not found, create new one
                if ( _instance == null )
                {
                    // create instance
                    _instance = new MapComponent_PawnPriorities();

                    // inject into game
                    Find.Map.components.Add( _instance );
                }

                return _instance;
            }
        }

        public bool DwarfTherapistMode
        {
            get { return _dwarfTherapistMode; }
            set { _dwarfTherapistMode = value; }
        }

        /// <summary>
        /// Note that time assignment is _always_ used internally, it is really only relevant from a UI perspective.
        /// </summary>
        public bool SchedulerMode
        {
            get { return _schedulerMode; }
            set { _schedulerMode = value; }
        }

        public bool TwentyFourHourMode
        {
            get { return _24Hours; }
            set { _24Hours = value; }
        }

        #endregion Properties

        #region Methods

        public static void NotifyAll_PrioritiesChanged()
        {
            foreach ( var pawn in Find.MapPawns.FreeColonistsSpawned )
                pawn.workSettings.Notify_UseWorkPrioritiesChanged();
        }

        public override void ExposeData()
        {
            // scribe per-game settings
            Scribe_Values.LookValue( ref _dwarfTherapistMode, "DwarfTherapistMode", false );
            Scribe_Values.LookValue( ref _24Hours, "TwentyFourHourClock", true );
            Scribe_Values.LookValue( ref _schedulerMode, "SchedulerMode", false );

            // scribe only the actual trackers, since pawns don't want to be saved in dicts
            if ( Scribe.mode == LoadSaveMode.Saving )
            {
                _workgiverTrackersScribe = _workgiverTrackers.Values.ToList();
                _worktypeTrackersScribe = _worktypeTrackers.Values.ToList();
            }

            // do the scribing
            Scribe_Collections.LookList( ref _workgiverTrackersScribe, "workgiverPriorities", LookMode.Deep );
            Scribe_Collections.LookList( ref _worktypeTrackersScribe, "worktypePriorities", LookMode.Deep );

            // reconstruct the full dict, drop null pawns (these were probably leftovers from killed or otherwise no longer available pawns).
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                _workgiverTrackers = _workgiverTrackersScribe.Where( t => t.pawn != null ).ToDictionary( k => k.pawn, v => v );
                _worktypeTrackers = _worktypeTrackersScribe.Where( t => t.pawn != null ).ToDictionary( k => k.pawn, v => v );
            }
        }

        public override void MapComponentTick()
        {
            // if the hour changed, make all pawns update their priorities.
            if ( _hourOfDay != GenDate.HourOfDay )
            {
                _hourOfDay = GenDate.HourOfDay;
                NotifyAll_PrioritiesChanged();
            }
        }

        public PawnWorkgiverPrioritiesTracker WorkgiverTracker( Pawn pawn )
        {
            if ( !_workgiverTrackers.ContainsKey( pawn ) )
                _workgiverTrackers.Add( pawn, new PawnWorkgiverPrioritiesTracker( pawn ) );
            return _workgiverTrackers[pawn];
        }

        public PawnWorktypePrioritiesTracker WorktypeTracker( Pawn pawn )
        {
            if ( !_worktypeTrackers.ContainsKey( pawn ) )
                _worktypeTrackers.Add( pawn, new PawnWorktypePrioritiesTracker( pawn ) );
            return _worktypeTrackers[pawn];
        }

        #endregion Methods
    }
}