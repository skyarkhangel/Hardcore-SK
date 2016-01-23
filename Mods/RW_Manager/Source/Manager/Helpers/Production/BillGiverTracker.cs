// Manager/BillGiverTracker.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:30

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public enum AssignedBillGiverOptions
    {
        All,
        Count,
        Specific
    }

    public class BillGiverTracker : IExposable
    {
        private bool _assignedBillGiversInitialized = true;
        private Dictionary<Bill_Production, Building_WorkTable> _assignedBills;
        private List<string> _assignedBillsScribeID;
        private List<string> _assignedWorkersScribeID;
        private readonly ManagerJob_Production _job;
        public Area AreaRestriction;
        public AssignedBillGiverOptions BillGiverSelection = AssignedBillGiverOptions.All;
        public List<Building_WorkTable> SpecificBillGivers;
        public int UserBillGiverCount;

        public List<string> AssignedBillsScribe
        {
            get { return _assignedBills.Keys.Select( b => b.GetUniqueLoadID() ).ToList(); }
            set { _assignedBillsScribeID = value; }
        }

        public List<string> AssignedWorkersScribe
        {
            get { return _assignedBills.Values.Select( b => b.GetUniqueLoadID() ).ToList(); }
            set { _assignedWorkersScribeID = value; }
        }

        public RecipeDef Recipe { get; }

        /// <summary>
        ///     All potential billgivers count
        /// </summary>
        public int AllBillGiverCount => BillGiverDefs.Count;

        /// <summary>
        ///     Currently allowed billgivers count (these do not necessarily actually have the bill)
        /// </summary>
        public int CurBillGiverCount => SelectedBillGivers.Count;

        /// <summary>
        ///     All billgiver defs (by recipe).
        /// </summary>
        public List<ThingDef> BillGiverDefs => Recipe.GetRecipeUsers();

        /// <summary>
        ///     Get workstations that can perform the current bill/recipe (nothwithstanding area/count restrictions etc).
        /// </summary>
        public List<Building_WorkTable> PotentialBillGivers => Recipe.CurrentRecipeUsers();

        /// <summary>
        ///     Get workstations that can perform the current bill/recipe, and meet selection criteria set by player.
        /// </summary>
        /// <returns></returns>
        public List<Building_WorkTable> SelectedBillGivers
        {
            get
            {
                List<Building_WorkTable> list = Recipe.CurrentRecipeUsers();

                switch ( BillGiverSelection )
                {
                    case AssignedBillGiverOptions.Count:
                        if ( AreaRestriction != null )
                        {
                            list = list.Where( bw => AreaRestriction.ActiveCells.Contains( bw.Position ) ).ToList();
                        }
                        list = list.Take( UserBillGiverCount ).ToList();
                        break;

                    case AssignedBillGiverOptions.Specific:
                        list = SpecificBillGivers;
                        break;

                    case AssignedBillGiverOptions.All:
                    default:
                        break;
                }

                return list;
            }
        }

        public Dictionary<Bill_Production, Building_WorkTable> AssignedBillGiversAndBillsDictionary
        {
            get
            {
                if ( !_assignedBillGiversInitialized )
                {
                    InitializeAssignedBillGivers();
                }

                return _assignedBills;
            }
        }

        /// <summary>
        ///     Get workstations to which a bill was actually assigned
        /// </summary>
        public List<Building_WorkTable> AssignedBillGivers
        {
            get { return AssignedBillGiversAndBillsDictionary.Values.ToList(); }
        }

        public WindowBillGiverDetails DetailsWindow
        {
            get
            {
                WindowBillGiverDetails window = new WindowBillGiverDetails
                {
                    Job = _job,
                    closeOnClickedOutside = true,
                    draggable = true
                };
                return window;
            }
        }

        public BillGiverTracker( ManagerJob_Production job )
        {
            Recipe = job.Bill.recipe;
            _job = job;
            _assignedBills = new Dictionary<Bill_Production, Building_WorkTable>();
            SpecificBillGivers = new List<Building_WorkTable>();
        }

        public void ExposeData()
        {
            if ( Scribe.mode == LoadSaveMode.Saving )
            {
                _assignedWorkersScribeID = _assignedBills.Values.Select( b => b.GetUniqueLoadID() ).ToList();
                _assignedBillsScribeID = _assignedBills.Keys.Select( b => b.GetUniqueLoadID() ).ToList();
            }

            Scribe_Values.LookValue( ref BillGiverSelection, "BillGiverSelection" );
            Scribe_Values.LookValue( ref UserBillGiverCount, "UserBillGiverCount" );
            Scribe_References.LookReference( ref AreaRestriction, "AreaRestriction" );
            Scribe_Collections.LookList( ref _assignedBillsScribeID, "AssignedBills", LookMode.Value );
            Scribe_Collections.LookList( ref _assignedWorkersScribeID, "AssignedWorkers", LookMode.Value );
            Scribe_Collections.LookList( ref SpecificBillGivers, "SpecificBillGivers", LookMode.MapReference );

            // rather complicated post-load workaround to find buildings by unique ID, since the scribe won't do things the simple way.
            // i.e. scribing dictionary with reference keys and values does not appear to work.
            // since buildings dont appear in the standard finding methods at this point, set a flag to initialize assignedbillgivers the next time Assigned bill givers is called.
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                _assignedBillGiversInitialized = false;
            }
        }

        private void InitializeAssignedBillGivers()
        {
            bool error = false;
            _assignedBills = new Dictionary<Bill_Production, Building_WorkTable>();
            for ( int i = 0; i < _assignedBillsScribeID.Count; i++ )
            {
#if DEBUG_SCRIBE
                        Log.Message( "Trying to find " + _assignedWorkersScribeID[i] + " | " + _assignedBillsScribeID[i] );
                        if ( Recipe != null )
                        {
                            Log.Message( "Recipe: " + Recipe.label );
                        }
                        Log.Message( Recipe.CurrentRecipeUsers().Count.ToString() );
#endif
                try
                {
                    Building_WorkTable worker = Recipe.CurrentRecipeUsers().DefaultIfEmpty( null )
                                                      .FirstOrDefault(
                                                          b =>
                                                              b.GetUniqueLoadID() == _assignedWorkersScribeID[i] );
                    Bill_Production bill = null;
                    if ( worker == null )
                    {
                        throw new Exception( "worker not found" );
                    }
                    if ( worker.billStack == null )
                    {
                        throw new Exception( "Billstack not initialized" );
                    }
                    foreach ( Bill current in worker.billStack )
                    {
                        if ( current.GetUniqueLoadID() == _assignedBillsScribeID[i] )
                        {
                            bill = (Bill_Production)current;
                        }
                    }
                    if ( bill == null )
                    {
                        throw new Exception( "Bill not found" );
                    }
                    _assignedBills.Add( bill, worker );
                }

                // ReSharper disable once UnusedVariable
                catch ( Exception e )
                {
                    error = true;
#if DEBUG_SCRIBE
                            Log.Warning( e.ToString() );
#endif
                }
            }

            if ( !error )
            {
                _assignedBillGiversInitialized = true;
            }
        }

        /// <summary>
        ///     Draw billgivers info + details button
        /// </summary>
        public void DrawBillGiverConfig( ref Vector2 cur, float width, float entryHeight, bool alt = false )
        {
            // target threshold
            string potentialString = string.Join( "\n", PotentialBillGivers.Select( b => b.LabelCap ).ToArray() );
            string selectedString = string.Join( "\n", SelectedBillGivers.Select( b => b.LabelCap ).ToArray() );
            string assignedString = string.Join( "\n", AssignedBillGivers.Select( b => b.LabelCap ).ToArray() );
            string billgiverTooltip = "FMP.BillGiversTooltip".Translate( potentialString, selectedString, assignedString );

            Rect billgiverLabelRect = new Rect( cur.x, cur.y, width, entryHeight );
            if ( alt )
            {
                Widgets.DrawAltRect( billgiverLabelRect );
            }
            Widgets.DrawHighlightIfMouseover( billgiverLabelRect );
            Utilities.Label( billgiverLabelRect,
                             "FMP.BillGiversCount".Translate( PotentialBillGivers.Count, SelectedBillGivers.Count,
                                                              AssignedBillGivers.Count ),
                             billgiverTooltip,
                             TextAnchor.MiddleLeft,
                             Utilities.Margin );
            if ( Widgets.InvisibleButton( billgiverLabelRect ) )
            {
                Find.WindowStack.Add( DetailsWindow );
            }

            // add a little icon to mark interactivity
            Rect searchIconRect = new Rect( billgiverLabelRect.xMax - Utilities.Margin - entryHeight, cur.y, entryHeight,
                                            entryHeight );
            if ( searchIconRect.height > Utilities.SmallIconSize )
            {
                // center it.
                searchIconRect = searchIconRect.ContractedBy( ( searchIconRect.height - Utilities.SmallIconSize ) / 2 );
            }
            GUI.DrawTexture( searchIconRect, Resources.Search );

            cur.y += entryHeight;
        }
    }
}