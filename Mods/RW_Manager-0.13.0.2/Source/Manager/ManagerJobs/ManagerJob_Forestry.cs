// Manager/ManagerJob_Forestry.cs
//
// Copyright Karel Kroeze, 2015.
//
// Created 2015-11-05 22:41

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class ManagerJob_Forestry : ManagerJob
    {
        #region Fields

        public static bool                ClearWindCells             = true;
        public Dictionary<ThingDef, bool> AllowedTrees;
        public bool                       AllowSaplings;
        public Dictionary<Area, bool>     ClearAreas                 = new Dictionary<Area, bool>();
        public List<Designation>          Designations               = new List<Designation>();
        public History                    History;
        public Area                       LoggingArea;
        public new Trigger_Threshold      Trigger;
        private readonly float            _margin                    = Utilities.Margin;
        private List<bool>                _clearAreas_allowed;
        private List<Area>                _clearAreas_areas;
        private Utilities.CachedValue<int> _designatedWoodCachedValue = new Utilities.CachedValue<int>();

        #endregion Fields

        #region Constructors

        public ManagerJob_Forestry()
        {
            // populate the trigger field, set the root category to wood.
            Trigger = new Trigger_Threshold( this );
            Trigger.ThresholdFilter.SetDisallowAll();
            Trigger.ThresholdFilter.SetAllow( Utilities_Forestry.Wood, true );

            // populate the list of trees from the plants in the biome - allow all by default.
            // A tree is defined as any plant that yields wood or has a wood harvesting tag.
            AllowedTrees =
                Find.Map.Biome.AllWildPlants.Where( pd => pd.plant.harvestTag == "Wood" ||  pd.plant.harvestedThingDef == Utilities_Forestry.Wood )
                    // add harvesttag to allow non-wood yielding woody plants.
                    .ToDictionary( pk => pk, v => true );

            // initialize clearAreas list with current areas
            ClearAreas = Find.AreaManager.AllAreas.Where( area => area.AssignableAsAllowed( AllowedAreaMode.Humanlike ) ).ToDictionary( a => a, v => false );

            History = new History( new[] { "stock", "designated" }, new[] { Color.white, Color.grey } );
        }

        #endregion Constructors

        #region Properties

        public override bool Completed
        {
            get { return !Trigger.State; }
        }

        public override string Label
        {
            get { return "FMF.Forestry".Translate(); }
        }

        public override ManagerTab Tab
        {
            get { return Manager.Get.ManagerTabs.Find( tab => tab is ManagerTab_Forestry ); }
        }

        public override string[] Targets
        {
            get
            {
                return AllowedTrees.Keys.Where( key => AllowedTrees[key] ).Select( tree => tree.LabelCap ).ToArray();
            }
        }

        public override WorkTypeDef WorkTypeDef => WorkTypeDefOf.PlantCutting;

        #endregion Properties

        #region Methods

        public static void DoClearAreaDesignations( IEnumerable<IntVec3> cells, bool allPlants = false )
        {
            foreach ( IntVec3 cell in cells )
            {
                // confirm there is a plant here that it is a tree and that it has no current designation
                Plant plant = cell.GetPlant();
                if ( plant != null &&
                     ( allPlants || plant.def.plant.IsTree ) &&
                     Find.DesignationManager.AllDesignationsOn( plant ).ToList().NullOrEmpty() )
                //DesignationOn( plant, DesignationDefOf.CutPlant ) == null )
                {
                    Find.DesignationManager.AddDesignation( new Designation( plant, DesignationDefOf.CutPlant ) );
                }
            }
        }

        public static void GlobalWork()
        {
            // designate wind cells
            if ( ClearWindCells )
            {
                DoClearAreaDesignations( GetWindCells() );
            }
        }

        public void AddRelevantGameDesignations()
        {
            // get list of game designations not managed by this job that could have been assigned by this job.
            foreach ( Designation des in Find.DesignationManager.DesignationsOfDef( DesignationDefOf.CutPlant )
                                             .Except( Designations )
                                             .Where( des => IsValidForestryTarget( des.target ) ) )
            {
                AddDesignation( des );
            }
        }

        /// <summary>
        /// Remove obsolete designations from the list.
        /// </summary>
        public void CleanDesignations()
        {
            // get the intersection of bills in the game and bills in our list.
            List<Designation> gameDesignations =
                Find.DesignationManager.DesignationsOfDef( DesignationDefOf.HarvestPlant ).ToList();
            Designations = Designations.Intersect( gameDesignations ).ToList();
        }

        public override void CleanUp()
        {
            // clear the list of obsolete designations
            CleanDesignations();

            // cancel outstanding designation
            foreach ( Designation des in Designations )
            {
                des.Delete();
            }

            // clear the list completely
            Designations.Clear();
        }

        public override void DrawListEntry( Rect rect, bool overview = true, bool active = true )
        {
            // (detailButton) | name | (bar | last update)/(stamp) -> handled in Utilities.DrawStatusForListEntry
            int shownTargets = overview ? 4 : 3; // there's more space on the overview

            // set up rects
            Rect labelRect = new Rect( _margin, _margin, rect.width -
                                                         ( active ? StatusRectWidth + 4 * _margin : 2 * _margin ),
                                       rect.height - 2 * _margin ),
                 statusRect = new Rect( labelRect.xMax + _margin, _margin, StatusRectWidth, rect.height - 2 * _margin );

            // create label string
            string text = Label + "\n<i>" +
                          ( Targets.Length < shownTargets ? string.Join( ", ", Targets ) : "<multiple>" )
                          + "</i>";
            string tooltip = string.Join( ", ", Targets );

            // do the drawing
            GUI.BeginGroup( rect );

            // draw label
            Utilities.Label( labelRect, text, tooltip, TextAnchor.MiddleLeft, _margin );

            // if the bill has a manager job, give some more info.
            if ( active )
            {
                this.DrawStatusForListEntry( statusRect, Trigger );
            }
            GUI.EndGroup();
        }

        public override void DrawOverviewDetails( Rect rect )
        {
            History.DrawPlot( rect, Trigger.Count );
        }

        public override void ExposeData()
        {
            // scribe base things
            base.ExposeData();

            // settings
            Scribe_References.LookReference( ref LoggingArea, "LoggingArea" );
            Scribe_Collections.LookDictionary( ref AllowedTrees, "AllowedTrees", LookMode.DefReference, LookMode.Value );
            Scribe_Values.LookValue( ref AllowSaplings, "AllowSaplings", false );
            Scribe_Values.LookValue( ref ClearWindCells, "ClearWindCells", true );

            // clearing areas list
            if ( Scribe.mode == LoadSaveMode.Saving )
            {
                _clearAreas_allowed = new List<bool>( ClearAreas.Values );
                _clearAreas_areas = new List<Area>( ClearAreas.Keys );
            }
            Scribe_Collections.LookList( ref _clearAreas_allowed, "ClearAreas_allowed", LookMode.Value );
            Scribe_Collections.LookList( ref _clearAreas_areas, "ClearAreas_areas", LookMode.MapReference );
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                ClearAreas = new Dictionary<Area, bool>();
                for ( int i = 0; i < _clearAreas_areas.Count; i++ )
                {
                    ClearAreas.Add( _clearAreas_areas[i], _clearAreas_allowed[i] );
                }
            }

            // trigger
            Scribe_Deep.LookDeep( ref Trigger, "Trigger", this );

            if ( Manager.LoadSaveMode == Manager.Modes.Normal )
            {
                // scribe history
                Scribe_Deep.LookDeep( ref History, "History" );
            }
        }

        public int GetWoodInDesignations()
        {
            int count = 0;

            // try get cache
            if ( _designatedWoodCachedValue.TryGetValue( out count ) )
            {
                return count;
            }

            foreach ( Designation des in Designations )
            {
                if ( des.target.HasThing &&
                     des.target.Thing is Plant )
                {
                    Plant plant = des.target.Thing as Plant;
                    count += plant.YieldNow();
                }
            }

            // update cache
            _designatedWoodCachedValue.Update( count );

            return count;
        }

        public override void Tick()
        {
            History.Update( Trigger.CurCount, GetWoodInDesignations() );
        }

        public override bool TryDoJob()
        {
            // keep track if any actual work was done.
            bool workDone = false;

            // remove designations not in zone.
            if ( LoggingArea != null )
            {
                CleanAreaDesignations();
            }

            // clear selected areas
            if ( ClearAreas.Any() )
            {
                DoClearAreas();
            }

            // clean dead designations
            CleanDesignations();

            // add external designations
            AddRelevantGameDesignations();

            // get current lumber count
            int count = Trigger.CurCount + GetWoodInDesignations();

            // get sorted list of loggable trees
            List<Plant> trees = GetLoggableTreesSorted();

            // designate untill we're either out of trees or we have enough designated.
            for ( int i = 0; i < trees.Count && count < Trigger.Count; i++ )
            {
                workDone = true;
                AddDesignation( trees[i], DesignationDefOf.HarvestPlant );
                count += trees[i].YieldNow();
            }

            return workDone;
        }

        public void UpdateClearAreas()
        {
            Dictionary<Area, bool> _clearAreas = new Dictionary<Area, bool>();

            foreach ( Area area in Find.AreaManager.AllAreas.Where( area => area.AssignableAsAllowed( AllowedAreaMode.Humanlike ) ) )
            {
                // add all areas in the game, set to true if area already existed and was true.
                _clearAreas.Add( area, ClearAreas.ContainsKey( area ) && ClearAreas[area] );
            }

            ClearAreas = _clearAreas;
        }

        private static List<IntVec3> GetWindCells()
        {
            return Find.ListerBuildings
                       .AllBuildingsColonistOfClass<Building_WindTurbine>()
                       .SelectMany( turbine => Building_WindTurbine.CalculateWindCells( turbine.Position,
                                                                                        turbine.Rotation,
                                                                                        turbine.RotatedSize ) )
                       .ToList();
        }

        private void AddDesignation( Designation des )
        {
            // add to game
            Find.DesignationManager.AddDesignation( des );

            // add to internal list
            Designations.Add( des );
        }

        private void AddDesignation( Plant p, DesignationDef def = null )
        {
            // create designation
            Designation des = new Designation( p, def );

            // pass to adder
            AddDesignation( des );
        }

        private void CleanAreaDesignations()
        {
            foreach ( Designation des in Designations )
            {
                if ( !des.target.HasThing )
                {
                    des.Delete();
                }
                else if ( !LoggingArea.ActiveCells.Contains( des.target.Thing.Position ) &&
                        ( !IsInWindTurbineArea( des.target.Thing.Position ) || !ClearWindCells ) )
                {
                    des.Delete();
                }
            }
        }

        private void DoClearAreas()
        {
            foreach ( var area in ClearAreas )
            {
                if ( area.Value )
                    DoClearAreaDesignations( area.Key.ActiveCells, true );
            }
        }

        private List<Plant> GetLoggableTreesSorted()
        {
            IntVec3 position = Utilities.GetBaseCenter();

            // get a list of trees that are not designated in the logging grounds and are reachable, sorted by yield / distance * 2
            List<Plant> list = Find.ListerThings.AllThings.Where( p => IsValidForestryTarget( p ) )

                                   // OrderBy defaults to ascending, switch sign on current yield to get descending
                                   .Select( p => p as Plant )
                                   .OrderBy(
                                       p =>
                                           -p.YieldNow() /
                                           ( Math.Sqrt( position.DistanceToSquared( p.Position ) ) * 2 ) )
                                   .ToList();

            return list;
        }

        private bool IsInWindTurbineArea( IntVec3 position )
        {
            return GetWindCells().Contains( position );
        }

        private bool IsValidForestryTarget( TargetInfo t )
        {
            return t.HasThing
                   && IsValidForestryTarget( t.Thing );
        }

        private bool IsValidForestryTarget( Thing t )
        {
            return t is Plant
                   && IsValidForestryTarget( (Plant)t );
        }

        private bool IsValidForestryTarget( Plant p )
        {
            return p.def.plant != null

                   // non-biome trees won't be on the list
                   && AllowedTrees.ContainsKey( p.def )

                   // also filters out non-tree plants
                   && AllowedTrees[p.def]
                   && p.Spawned
                   && Find.DesignationManager.DesignationOn( p ) == null

                   // cut only mature trees, or saplings that yield something right now.
                   && ( ( AllowSaplings && p.YieldNow() > 1 )
                        || p.LifeStage == PlantLifeStage.Mature )
                   && ( LoggingArea == null
                        || LoggingArea.ActiveCells.Contains( p.Position ) )
                   && p.Position.CanReachColony();
        }

        #endregion Methods
    }
}