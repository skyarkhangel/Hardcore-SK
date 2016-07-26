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
    public class PawnWorktypePrioritiesTracker : IExposable
    {
        #region Fields

        public WorkFavourite currentFavourite;
        public Pawn pawn;
        private DefMap<WorkTypeDef, bool> _cacheDirty = new DefMap<WorkTypeDef, bool>();
        private DefMap<WorkTypeDef, bool> _partiallyScheduledCache = new DefMap<WorkTypeDef, bool>();
        private Dictionary<WorkTypeDef, string> _partiallyScheduledTipCache = new Dictionary<WorkTypeDef, string>();
        private List<DefMap<WorkTypeDef, int>> priorities = new List<DefMap<WorkTypeDef, int>>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// For scribe only!
        /// </summary>
        public PawnWorktypePrioritiesTracker() { }

        public PawnWorktypePrioritiesTracker( Pawn pawn )
        {
            this.pawn = pawn;

            // create list of work priorities, and initialize with default settings (logic lifted from Pawn_WorkSettings.EnableAndInitialize() )
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                priorities.Add( new DefMap<WorkTypeDef, int>() );

                // for up to six 'best' worktypes, enable
                int count = 1;
                foreach ( var worktype in DefDatabase<WorkTypeDef>
                    .AllDefsListForReading
                    .Where( wtd => !wtd.alwaysStartActive && !pawn.story.WorkTypeIsDisabled( wtd ) )
                    .OrderByDescending( wtd => pawn.skills.AverageOfRelevantSkillsFor( wtd ) ) )
                {
                    priorities[hour][worktype] = 3;
                    if ( count++ > 6 )
                        break;
                }

                // enable all always enabled types
                foreach ( var worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading.Where( wtd => wtd.alwaysStartActive ) )
                {
                    priorities[hour][worktype] = 3;
                }

                // disable story-disabled types
                foreach ( var worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading.Where( wtd => pawn.story.WorkTypeIsDisabled( wtd ) ) )
                {
                    priorities[hour][worktype] = 0;
                }
            }

            // set partial scheduled cache dirty
            foreach ( var worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading )
            {
                _cacheDirty[worktype] = true;
            }
        }

        #endregion Constructors

        #region Methods

        public void AssignFavourite( WorkFavourite favourite )
        {
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                foreach ( var worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading )
                {
                    if ( !pawn.story.WorkTypeIsDisabled( worktype ) )
                        priorities[hour][worktype] = favourite.worktypePriorities.priorities[hour][worktype];
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
                foreach ( var worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading )
                {
                    _cacheDirty[worktype] = true;
                }
            }
        }

        public int GetPriority( WorkTypeDef worktype )
        {
            return GetPriority( worktype, GenDate.HourOfDay );
        }

        public int GetPriority( WorkTypeDef worktype, int hour )
        {
            return Find.PlaySettings.useWorkPriorities ? priorities[hour][worktype] : priorities[hour][worktype] > 0 ? 1 : 0;
        }

        /// <summary>
        /// returns true if pawn is scheduled for this workgiver, but only if not always active.
        /// </summary>
        /// <param name="worktype"></param>
        /// <returns></returns>
        public bool IsPartiallyScheduledFor( WorkTypeDef worktype )
        {
            if ( _cacheDirty[worktype] )
            {
                // initialize at false
                _partiallyScheduledCache[worktype] = false;

                // if both are true the workgiver is partially scheduled
                bool scheduled = false, notScheduled = false;

                for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
                {
                    // get prio
                    int priority = GetPriority( worktype, hour );

                    // set flags
                    if ( priority > 0 )
                        scheduled = true;
                    if ( priority == 0 )
                        notScheduled = true;

                    // check flags
                    if ( scheduled && notScheduled )
                    {
                        _partiallyScheduledCache[worktype] = true;
                        break;
                    }
                }

                // mark clean
                _cacheDirty[worktype] = false;

                // update tip
                CreatePartiallyScheduledTip( worktype );
            }

            return _partiallyScheduledCache[worktype];
        }

        public string PartiallyScheduledTip( WorkTypeDef worktype )
        {
            if ( !_partiallyScheduledTipCache.ContainsKey( worktype ) || _cacheDirty[worktype] )
                CreatePartiallyScheduledTip( worktype );

            return _partiallyScheduledTipCache[worktype];
        }

        public void SetPriority( WorkTypeDef worktype, int priority, int hour )
        {
            if ( pawn.story.WorkTypeIsDisabled( worktype ) )
                return;

            // set priority
            priorities[hour][worktype] = priority;

            // notify pawn that it should update
            pawn.workSettings.Notify_UseWorkPrioritiesChanged();

            // mark our partial assignment cache dirty
            _cacheDirty[worktype] = true;

            // clear current favourite
            currentFavourite = null;
        }

        public void SetPriority( WorkTypeDef worktype, int priority )
        {
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
                SetPriority( worktype, priority, hour );
        }

        public void SetPriority( WorkTypeDef worktype, int priority, List<int> hours )
        {
            foreach ( int hour in hours )
                SetPriority( worktype, priority, hour );
        }

        private void CreatePartiallyScheduledTip( WorkTypeDef worktype )
        {
            string tip = "FluffyTabs.XIsScheduledForY".Translate( pawn.Name.ToStringShort, worktype.labelShort );

            int start = -1;

            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                // start condition
                if ( GetPriority( worktype, hour ) > 0 && start < 0 )
                    start = hour;

                // stop condition
                if ( GetPriority( worktype, hour ) == 0 && start >= 0 )
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
            if ( !_partiallyScheduledTipCache.ContainsKey( worktype ) )
                _partiallyScheduledTipCache.Add( worktype, tip );
            else
                _partiallyScheduledTipCache[worktype] = tip;
        }

        #endregion Methods
    }
}