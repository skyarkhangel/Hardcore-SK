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
    public class MapComponent_Priorities : MapComponent
    {
        #region Fields

        // making everything static should solve our weird load issue, although it is cheating a bit.
        private static MapComponent_Priorities _instance;
        private static bool _24Hours = true;
        private static bool _dwarfTherapistMode = false;
        private static int _hourOfDay = 0;
        private static bool _schedulerMode = false;
        private static Dictionary<Pawn, PawnPrioritiesTracker> _trackers = new Dictionary<Pawn, PawnPrioritiesTracker>();
        private static List<PawnPrioritiesTracker> _trackersScribeHelper;

        #endregion Fields

        #region Properties

        public static MapComponent_Priorities Instance
        {
            get
            {
                // return null if called too damn early
                if ( Find.Map == null )
                    return null;

                // try to get from game list.
                if ( _instance == null )
                    _instance = Find.Map.components.FirstOrDefault( comp => comp is MapComponent_Priorities ) as MapComponent_Priorities;

                // not found, create new one
                if ( _instance == null )
                {
                    // create instance
                    _instance = new MapComponent_Priorities();

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
                _trackersScribeHelper = _trackers.Values.ToList();
            }

            // do the scribing
            Scribe_Collections.LookList( ref _trackersScribeHelper, "workgiverPriorities", LookMode.Deep );

            // reconstruct the full dict, drop null pawns (these were probably leftovers from killed or otherwise no longer available pawns).
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                _trackers = _trackersScribeHelper.Where( t => t.pawn != null ).ToDictionary( k => k.pawn, v => v );
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

        public void Notify_FavouriteDeleted( WorkFavourite favourite )
        {
            foreach ( var tracker in _trackers )
                if ( tracker.Value.currentFavourite == favourite )
                    tracker.Value.currentFavourite = null;
        }

        public PawnPrioritiesTracker WorkgiverTracker( Pawn pawn )
        {
            if ( !_trackers.ContainsKey( pawn ) )
                _trackers.Add( pawn, new PawnPrioritiesTracker( pawn ) );
            return _trackers[pawn];
        }

        #endregion Methods
    }
}