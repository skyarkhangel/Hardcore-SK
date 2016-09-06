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
    public class PawnPrioritiesTracker : IExposable
    {
        #region Fields

        public WorkFavourite currentFavourite;
        public Pawn pawn;
        private DefMap<WorkGiverDef, bool> _cacheDirty = new DefMap<WorkGiverDef, bool>();
        private DefMap<WorkGiverDef, bool> _timeDependentCache = new DefMap<WorkGiverDef, bool>();
        private Dictionary<WorkGiverDef, string> _timeDependentTipCache = new Dictionary<WorkGiverDef, string>();
        private List<DefMap<WorkGiverDef, int>> priorities = new List<DefMap<WorkGiverDef, int>>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// For scribe only!
        /// </summary>
        public PawnPrioritiesTracker() { }
        
        public PawnPrioritiesTracker( Pawn pawn )
        {
            this.pawn = pawn;

            InitPriorityCache();
        }

        private void InitPriorityCache()
        {
            // initialize from vanilla priorities.
            var vanillaPriorities = Detours_WorkSettings.GetVanillaPriorities( pawn );

            // loop over hours
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {

                // create map for this hour
                priorities.Add( new DefMap<WorkGiverDef, int>() );

                // loop over worktypes
                foreach ( WorkTypeDef worktype in DefDatabase<WorkTypeDef>.AllDefsListForReading )
                {
                    int priority = vanillaPriorities[worktype];

                    // loop over workgivers in type
                    foreach ( WorkGiverDef workgiver in worktype.workGiversByPriority )
                    {
                        priorities[hour][workgiver] = priority;
                    }
                }
            }

            // set partial scheduled cache dirty
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
            try
            {
                return Find.PlaySettings.useWorkPriorities ? priorities[hour][workgiver] : priorities[hour][workgiver] > 0 ? 1 : 0;
            }
            catch ( ArgumentOutOfRangeException )
            {
                // workgiver-priority defmap is really just and ordered list indexed by a dynamically generated workgiver index int
                // if the number of workgivers increases, this means errors.
                Messages.Message( "WorkGiver database corrupted, resetting priorities for " + pawn.NameStringShort + ". Did you add mods during the game?", MessageSound.SeriousAlert );
                priorities = new List<DefMap<WorkGiverDef, int>>();
                InitPriorityCache();

                throw;
            }
        }

        public int GetPriority( WorkTypeDef worktype, int hour = -1 )
        {
            // get current hour if left default
            if ( hour < 0 )
                hour = GenDate.HourOfDay;

            // get workgiver priorities
            var _priorities = worktype.workGiversByPriority.Select( wg => GetPriority( wg, hour ) );

            // if any priority is not 0, select highest (lowest) priority
            if ( _priorities?.Count() > 0 && _priorities.Any( prio => prio != 0 ) )
                return _priorities.Where( prio => prio > 0 ).Min();
            else 
                return 0;
        }

        /// <summary>
        /// returns true if pawn is scheduled for this workgiver, but only if not always active.
        /// </summary>
        /// <param name="workgiver"></param>
        /// <returns></returns>
        public bool IsTimeDependent( WorkGiverDef workgiver )
        {
            if ( _cacheDirty[workgiver] )
            {
                // initialize at false
                _timeDependentCache[workgiver] = false;

                // if there is more than one unique priority, this workgiver is partially scheduled
                int priority = GetPriority( workgiver, 0 );

                for ( int hour = 1; hour < GenDate.HoursPerDay; hour++ )
                {
                    // get prio
                    int curPriority = GetPriority( workgiver, hour );

                    if ( priority != curPriority )
                    {
                        _timeDependentCache[workgiver] = true;
                        break;
                    }
                }

                // mark clean
                _cacheDirty[workgiver] = false;

                // update tip
                CreatePartiallyScheduledTip( workgiver );
            }

            return _timeDependentCache[workgiver];
        }

        public bool IsTimeDependent( WorkTypeDef worktype )
        {
            return worktype.workGiversByPriority.Any( wg => IsTimeDependent( wg ) );
        }

        public string TimeDependentTip( WorkGiverDef workgiver )
        {
            if ( !_timeDependentTipCache.ContainsKey( workgiver ) || _cacheDirty[workgiver] )
                CreatePartiallyScheduledTip( workgiver );

            return _timeDependentTipCache[workgiver];
        }

        public string TimeDependentTip( WorkTypeDef worktype )
        {
            return "FluffyTabs.TimeDependentWorktypeTip".Translate();
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

            // notify pawn that he might have to stop current job
            if ( priority == 0 && hour == GenDate.HourOfDay )
                pawn.mindState.Notify_WorkPriorityDisabled( workgiver.workType );
        }

        public void SetPriority( WorkTypeDef worktype, int priority, int hour )
        {
            foreach ( var workgiver in worktype.workGiversByPriority )
                SetPriority( workgiver, priority, hour );
        }

        public void SetPriority( WorkGiverDef workgiver, int priority )
        {
            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
                SetPriority( workgiver, priority, hour );
        }

        public void SetPriority( WorkTypeDef worktype, int priority )
        {
            // propagate to vanilla
            Detours_WorkSettings.SetVanillaPriority( pawn, worktype, priority );

            foreach ( var workgiver in worktype.workGiversByPriority )
                SetPriority( workgiver, priority );
        }

        public void SetPriority( WorkGiverDef workgiver, int priority, List<int> hours )
        {
            foreach ( int hour in hours )
                SetPriority( workgiver, priority, hour );
        }

        public void SetPriority( WorkTypeDef worktype, int priority, List<int> hours )
        {
            foreach ( var workgiver in worktype.workGiversByPriority )
                SetPriority( workgiver, priority, hours );
        }

        private void CreatePartiallyScheduledTip( WorkGiverDef workgiver )
        {
            string tip = "FluffyTabs.XIsScheduledForY".Translate( pawn.Name.ToStringShort, Settings.WorkgiverLabels[workgiver] );

            int start = -1;
            int priority = -1;

            for ( int hour = 0; hour < GenDate.HoursPerDay; hour++ )
            {
                int curpriority = GetPriority( workgiver, hour );

                // stop condition
                if ( curpriority != priority && start >= 0 )
                {
                    tip += start.FormatHour() + " - " + 0.FormatHour();
                    if ( Find.PlaySettings.useWorkPriorities )
                        tip += " (" + priority + ")";
                    tip += "\n";

                    // reset start & priority
                    start = -1;
                    priority = -1;
                }

                // start condition
                if ( curpriority > 0 && curpriority != priority && start < 0 )
                {
                    priority = curpriority;
                    start = hour;
                }
            }

            // final check for x till midnight
            if ( start > 0 )
            {
                tip += start.FormatHour() + " - " + 0.FormatHour();
                if ( Find.PlaySettings.useWorkPriorities )
                    tip += " (" + priority + ")";
                tip += "\n";
            }

            // add or replace tip cache
            if ( !_timeDependentTipCache.ContainsKey( workgiver ) )
                _timeDependentTipCache.Add( workgiver, tip );
            else
                _timeDependentTipCache[workgiver] = tip;
        }

        #endregion Methods
    }
}