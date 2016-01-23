// Manager/ManagerJob_Hunting.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-05 22:30

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class ManagerJob_Hunting : ManagerJob
    {
        private Utilities.CachedValue<int>   _corpseCachedValue     = new Utilities.CachedValue<int>();
        private Utilities.CachedValue<int>   _designatedCachedValue = new Utilities.CachedValue<int>();
        private readonly float               _margin                = Utilities.Margin;
        public Dictionary<PawnKindDef, bool> AllowedAnimals         = new Dictionary<PawnKindDef, bool>();
        public List<Designation>             Designations           = new List<Designation>();
        public Area                          HuntingGrounds;
        public new Trigger_Threshold         Trigger;
        public bool                          UnforbidCorpses        = true;
        public History                       History;

        public override bool Completed
        {
            get { return !Trigger.State; }
        }

        public override ManagerTab Tab
        {
            get { return Manager.Get.ManagerTabs.Find( tab => tab is ManagerTab_Hunting ); }
        }

        public override string Label
        {
            get { return "FMH.Hunting".Translate(); }
        }

        public override string[] Targets
        {
            get
            {
                return AllowedAnimals.Keys.Where( key => AllowedAnimals[key] ).Select( pk => pk.LabelCap ).ToArray();
            }
        }

        public List<Corpse> Corpses
        {
            get
            {
                List<Corpse> corpses = Find.ListerThings.ThingsInGroup( ThingRequestGroup.Corpse ).ConvertAll( thing => thing as Corpse);
                return corpses.Where( thing => AllowedAnimals.ContainsKey(thing.innerPawn.kindDef) && AllowedAnimals[thing.innerPawn.kindDef] ).ToList();
            }
        }

        public override WorkTypeDef WorkTypeDef => WorkTypeDefOf.Hunting;

        public ManagerJob_Hunting()
        {
            // populate the trigger field, set the root category to meats and allow all but human meat.
            Trigger = new Trigger_Threshold( this );
            Trigger.ThresholdFilter.SetDisallowAll();
            Trigger.ThresholdFilter.SetAllow( Utilities_Hunting.RawMeat, true );
            Trigger.ThresholdFilter.SetAllow( Utilities_Hunting.HumanMeat, false );

            // populate the list of animals from the animals in the biome - allow all by default.
            AllowedAnimals = Find.Map.Biome.AllWildAnimals.ToDictionary( pk => pk, v => true );

            History = new History( new[] { "stock", "corpses", "designated" },
                                   new Color[] { Color.white, new Color( .7f, .7f, .7f ), new Color( .4f, .4f, .4f ) } );
        }

        private void AddRelevantGameDesignations()
        {
            foreach (
                Designation des in
                    Find.DesignationManager.DesignationsOfDef( DesignationDefOf.Hunt )
                        .Except( Designations )
                        .Where( des => IsValidHuntingTarget( des.target ) ) )
            {
                AddDesignation( des );
            }
        }

        private bool IsValidHuntingTarget( TargetInfo t )
        {
            return t.HasThing
                   && t.Thing is Pawn
                   && IsValidHuntingTarget( (Pawn)t.Thing );
        }

        private bool IsValidHuntingTarget( Pawn p )
        {
            return p.RaceProps.Animal
                   && !p.health.Dead
                   && p.SpawnedInWorld

                // wild animals only
                   && p.Faction == null

                // non-biome animals won't be on the list
                   && AllowedAnimals.ContainsKey( p.kindDef )
                   && AllowedAnimals[p.kindDef]
                   && Find.DesignationManager.DesignationOn( p ) == null
                   && ( HuntingGrounds == null
                        || HuntingGrounds.ActiveCells.Contains( p.Position ) )
                   && p.Position.CanReachColony();
        }

        public override void ExposeData()
        {
            // scribe base things
            base.ExposeData();

            // settings
            Scribe_References.LookReference( ref HuntingGrounds, "HuntingGrounds" );
            Scribe_Collections.LookDictionary( ref AllowedAnimals, "AllowedAnimals", LookMode.DefReference,
                                               LookMode.Value );

            // human meat is saved in trigger's thingfilter.
            Scribe_Values.LookValue( ref UnforbidCorpses, "UnforbidCorpses", true );

            // trigger
            Scribe_Deep.LookDeep( ref Trigger, "Trigger", this );

            if ( Manager.LoadSaveMode == Manager.Modes.Normal )
            {
                // scribe history
                Scribe_Deep.LookDeep( ref History, "History" );
            }
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

        /// <summary>
        /// Remove obsolete designations from the list.
        /// </summary>
        public void CleanDesignations()
        {
            // get the intersection of bills in the game and bills in our list.
            List<Designation> GameDesignations =
                Find.DesignationManager.DesignationsOfDef( DesignationDefOf.Hunt ).ToList();
            Designations = Designations.Intersect( GameDesignations ).ToList();
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

        public override bool TryDoJob()
        {
            // did we do any work?
            bool workDone = false;

            // clean designations not in area
            CleanAreaDesignations();

            // clean dead designations
            CleanDesignations();

            // add designations that could have been handed out by us
            AddRelevantGameDesignations();
            
            // get the total count of meat in storage, expected meat in corpses and expected meat in designations.
            int totalCount = Trigger.CurCount + GetMeatInCorpses() + GetMeatInDesignations();

            // get a list of huntable animals sorted by distance (ignoring obstacles) and expected meat count.
            // note; attempt to balance cost and benefit, current formula: value = meat / ( distance ^ 2)
            List<Pawn> huntableAnimals = GetHuntableAnimalsSorted();

            // while totalCount < count AND we have animals that can be designated, designate animal.
            for ( int i = 0; i < huntableAnimals.Count && totalCount < Trigger.Count; i++ )
            {
                AddDesignation( huntableAnimals[i] );
                totalCount += huntableAnimals[i].EstimatedMeatCount();
                workDone = true;
            }

            // unforbid if required
            if( UnforbidCorpses )
            {
                DoUnforbidCorpses( ref workDone );
            }
        
            return workDone;
        }
        
        private void CleanAreaDesignations()
        {
            // huntinggrounds of null denotes unrestricted
            if ( HuntingGrounds != null )
            {
                foreach ( Designation des in Designations )
                {
                    if ( des.target.HasThing &&
                         !HuntingGrounds.ActiveCells.Contains( des.target.Thing.Position ) )
                    {
                        des.Delete();
                    }
                }
            }
        }

        private void AddDesignation( Designation des )
        {
            // add to game
            Find.DesignationManager.AddDesignation( des );

            // add to internal list
            Designations.Add( des );
        }

        private void AddDesignation( Pawn p )
        {
            // create designation
            Designation des = new Designation( p, DesignationDefOf.Hunt );

            // pass to adder
            AddDesignation( des );
        }

        private List<Pawn> GetHuntableAnimalsSorted()
        {
            // get the 'home' position
            IntVec3 position = Utilities.GetBasePosition();

            // get a list of alive animals that are not designated in the hunting grounds and are reachable, sorted by meat / distance * 2
            List<Pawn> list = Find.ListerPawns.AllPawns.Where( p => IsValidHuntingTarget( p ) )

                // OrderBy defaults to ascending, switch sign on estimated meat count to get descending
                                  .OrderBy(
                                      p =>
                                          - p.EstimatedMeatCount() /
                                          ( Math.Sqrt( position.DistanceToSquared( p.Position ) ) * 2 ) ).ToList();

            return list;
        }

        // copypasta from autohuntbeacon by Carry
        // https://ludeon.com/forums/index.php?topic=8930.0
        private void DoUnforbidCorpses( ref bool workDone )
        {
            foreach ( Corpse corpse in Corpses )
            {
                
                // don't unforbid corpses in storage - we're going to assume they were manually set.
                if ( corpse != null &&
                     !corpse.IsInAnyStorage() &&
                     corpse.IsForbidden( Faction.OfColony ) )
                {
                    // only fresh corpses
                    CompRottable comp = corpse.GetComp<CompRottable>();
                    if ( comp != null &&
                         comp.Stage == RotStage.Fresh )
                    {
                        // unforbid
                        workDone = true;
                        corpse.SetForbidden( false, false );
                    }
                }
            }
        }

        public int GetMeatInCorpses()
        {
            // get current count + corpses in storage that is not a grave + designated count
            // current count in storage
            int count = 0;

            // try get cached value
            if ( _corpseCachedValue.TryGetValue( out count ) )
            {
                return count;
            }

            // corpses not buried / forbidden
            foreach ( Thing current in Corpses )
            {
                // make sure it's a real corpse. (I dunno, poke it?)
                // and that it's not forbidden (anymore) and can be reached.
                Corpse corpse = current as Corpse;
                if ( corpse != null &&
                     !corpse.IsForbidden( Faction.OfColony ) &&
                     corpse.Position.CanReachColony() )
                {
                    // check to see if it's buried.
                    bool buried = false;
                    SlotGroup slotGroup = Find.SlotGroupManager.SlotGroupAt( corpse.Position );
                    if ( slotGroup != null )
                    {
                        Building_Storage building_Storage = slotGroup.parent as Building_Storage;

                        // Sarcophagus inherits grave, here's to hoping Ty and modders stick to that in the future.
                        if ( building_Storage != null &&
                             building_Storage.def == ThingDefOf.Grave )
                        {
                            buried = true;
                        }
                    }

                    // get the rottable comp and check how far gone it is.
                    CompRottable rottable = corpse.TryGetComp<CompRottable>();

                    if ( !buried && rottable?.Stage == RotStage.Fresh )
                    {
                        count += corpse.GetMeatCount();
                    }
                }
            }

            // set cache
            _corpseCachedValue.Update( count );

            return count;
        }

        public int GetMeatInDesignations()
        {
            int count = 0;

            // try get cache
            if ( _designatedCachedValue.TryGetValue( out count ) )
            {
                return count;
            }

            // designated animals
            foreach ( Designation des in Find.DesignationManager.DesignationsOfDef( DesignationDefOf.Hunt ) )
            {
                // make sure target is a pawn, is an animal, is not forbidden and somebody can reach it.
                // note: could be rolled into a fancy LINQ chain, but this is probably clearer.
                Pawn target = des.target.Thing as Pawn;
                if ( target != null &&
                     target.RaceProps.Animal &&
                     !target.IsForbidden( Faction.OfColony ) &&
                     target.Position.CanReachColony() )
                {
                    count += target.EstimatedMeatCount();
                }
            }

            // update cache
            _designatedCachedValue.Update( count );

            return count;
        }

        public override void Tick()
        {
            History.Update( Trigger.CurCount, GetMeatInCorpses(), GetMeatInDesignations() );
        }

        private List<ThingDef> _humanLikeMeatDefs;

        public List<ThingDef> HumanLikeMeatDefs
        {
            get
            {
                if ( _humanLikeMeatDefs == null )
                {
                    List<ThingDef> humanLikeRaces =
                        DefDatabase<ThingDef>.AllDefsListForReading.Where( def => def.category == ThingCategory.Pawn &&
                                                                                  def.race.Humanlike && 
                                                                                  def.race.isFlesh )
                                             .ToList();

                    _humanLikeMeatDefs = new List<ThingDef>();
                    foreach ( ThingDef sourceDef in humanLikeRaces )
                    {
                        _humanLikeMeatDefs.Add( DefDatabase<ThingDef>.GetNamed( sourceDef.defName + "_Meat" ) );
                    }
                }
                return _humanLikeMeatDefs;
            }
        } 

        public void AllowHumanLikeMeat( bool allow )
        {
            foreach ( ThingDef meatDef in HumanLikeMeatDefs )
            {
                Trigger.ThresholdFilter.SetAllow(meatDef, allow);
            }
        }
    }
}