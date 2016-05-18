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
    public class PawnWorkgiverPrioritiesTracker : IExposable
    {
        #region Fields

        public WorkFavourite currentFavourite;
        public Pawn pawn;
        private DefMap<WorkGiverDef, bool> _cacheDirty = new DefMap<WorkGiverDef, bool>();
        private DefMap<WorkGiverDef, bool> _partiallyScheduledCache = new DefMap<WorkGiverDef, bool>();
        private Dictionary<WorkGiverDef, string> _partiallyScheduledTipCache = new Dictionary<WorkGiverDef, string>();
        private List<DefMap<WorkGiverDef, int>> priorities = new List<DefMap<WorkGiverDef, int>>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// For scribe only!
        /// </summary>
        public PawnWorkgiverPrioritiesTracker() { }

        public PawnWorkgiverPrioritiesTracker( Pawn pawn )
        {
            this.pawn = pawn;

            // start priorities tracker by copying from worktypes
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                priorities.Add( new DefMap<WorkGiverDef, int>() );
                foreach ( var workgiver in DefDatabase<WorkGiverDef>.AllDefsListForReading )
                {
                    priorities[hour][workgiver] = pawn.worktypePriorities().GetPriority( workgiver.workType );
                }
            }

            // mark all cache dirty - just as a precaution
            foreach ( var workgiver in DefDatabase<WorkGiverDef>.AllDefsListForReading )
            {
                _cacheDirty[workgiver] = true;
            }
        }

        #endregion Constructors

        #region Methods

        public void AssignFavourite( WorkFavourite favourite )
        {
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                foreach ( var workgiver in DefDatabase<WorkGiverDef>.AllDefsListForReading )
                {
                    if ( !pawn.story.WorkTypeIsDisabled( workgiver.workType ) )
                        priorities[hour][workgiver] = favourite.workgiverPriorities.priorities[hour][workgiver];
                }
            }
            currentFavourite = favourite;
        }

        public void ExposeData()
        {
            Scribe_References.LookReference( ref pawn, "pawn" );
            Scribe_Collections.LookList( ref priorities, "priorities", LookMode.Deep );
            Scribe_References.LookReference( ref currentFavourite, "currentFavourite" );

            // clear tip cache so it gets rebuild after load
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                foreach ( var workgiver in DefDatabase<WorkGiverDef>.AllDefsListForReading )
                {
                    _cacheDirty[workgiver] = true;
                }
            }
        }

        public int GetPriority( WorkGiverDef workgiver )
        {
            return GetPriority( workgiver, GenDate.HourOfDay );
        }

        public int GetPriority( WorkGiverDef workgiver, int hour )
        {
            return Find.PlaySettings.useWorkPriorities ? priorities[hour][workgiver] : priorities[hour][workgiver] > 0 ? 1 : 0;
        }

        /// <summary>
        /// returns true if pawn is scheduled for this workgiver, but only if not always active.
        /// </summary>
        /// <param name="workgiver"></param>
        /// <returns></returns>
        public bool IsPartiallyScheduledFor( WorkGiverDef workgiver )
        {
            if ( _cacheDirty[workgiver] )
            {
                // initialize at false
                _partiallyScheduledCache[workgiver] = false;

                // if both are true the workgiver is partially scheduled
                bool scheduled = false, notScheduled = false;

                for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
                {
                    // get prio
                    int priority = GetPriority( workgiver, hour );

                    // set flags
                    if ( priority > 0 )
                        scheduled = true;
                    if ( priority == 0 )
                        notScheduled = true;

                    // check flags
                    if ( scheduled && notScheduled )
                    {
                        _partiallyScheduledCache[workgiver] = true;
                        break;
                    }
                }

                // mark clean
                _cacheDirty[workgiver] = false;

                // update tip
                CreatePartiallyScheduledTip( workgiver );
            }

            return _partiallyScheduledCache[workgiver];
        }

        public string PartiallyScheduledTip( WorkGiverDef workgiver )
        {
            if ( !_partiallyScheduledTipCache.ContainsKey( workgiver ) || _cacheDirty[workgiver] )
                CreatePartiallyScheduledTip( workgiver );

            return _partiallyScheduledTipCache[workgiver];
        }

        public void SetPriority( WorkGiverDef workgiver, int priority, int hour )
        {
            if ( pawn.story.WorkTypeIsDisabled( workgiver.workType ) )
                return;

            // change priority
            priorities[hour][workgiver] = priority;

            // notify pawn to recache it's work order
            pawn.workSettings.Notify_UseWorkPrioritiesChanged();

            // mark our partially scheduled cache dirty.
            _cacheDirty[workgiver] = true;

            // clear current favourite
            currentFavourite = null;
        }

        public void SetPriority( WorkGiverDef workgiver, int priority )
        {
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
                SetPriority( workgiver, priority, hour );
        }

        public void SetPriority( WorkGiverDef workgiver, int priority, List<int> hours )
        {
            foreach ( int hour in hours )
                SetPriority( workgiver, priority, hour );
        }

        private void CreatePartiallyScheduledTip( WorkGiverDef workgiver )
        {
            string tip = "FluffyTabs.XIsScheduledForY".Translate( pawn.Name.ToStringShort, Settings.WorkgiverLabels[workgiver] );

            int start = -1;

            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                // start condition
                if ( GetPriority( workgiver, hour ) > 0 && start < 0 )
                    start = hour;

                // stop condition
                if ( GetPriority( workgiver, hour ) == 0 && start >= 0 )
                {
                    tip += start.FormatHour() + " - " + hour.FormatHour() + "\n";

                    // reset start
                    start = -1;
                }
            }

            // final check for x till midnight
            if ( start > 0 )
            {
                tip += start.FormatHour() + " - " + 0.FormatHour() + "\n";
            }

            // add or replace tip cache
            if ( !_partiallyScheduledTipCache.ContainsKey( workgiver ) )
                _partiallyScheduledTipCache.Add( workgiver, tip );
            else
                _partiallyScheduledTipCache[workgiver] = tip;
        }

        #endregion Methods
    }
}