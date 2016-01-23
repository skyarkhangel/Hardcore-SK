// Manager/Trigger_PawnKind.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-27 16:55

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class Trigger_PawnKind : Trigger
    {
        public Dictionary<Utilities_Livestock.AgeAndSex, int> CountTargets;
        public PawnKindDef pawnKind;

        public Trigger_PawnKind()
        {
            CountTargets = Utilities_Livestock.AgeSexArray.ToDictionary( k => k, v => 5 );
        }

        private Utilities.CachedValue<bool> _state = new Utilities.CachedValue<bool>(); 
        public override bool State
        {
            get
            {
                bool state;
                if ( !_state.TryGetValue( out state ) )
                {
                    state = Utilities_Livestock.AgeSexArray.All( ageSex => CountTargets[ageSex] == pawnKind.GetTame( ageSex ).Count ) && AllTrainingWantedSet();
                    _state.Update( state );
                }
                return state;
            }
        }

        public ManagerJob_Livestock Job
        {
            get
            {
                return Manager.Get.JobStack.FullStack<ManagerJob_Livestock>()
                              .FirstOrDefault( job => job.Trigger == this );
            }
        }

        private bool AllTrainingWantedSet()
        {   
            // loop through all set training targets, then through all animals to see if they're actually set. For the first that is not set, return false.
            // if the loop is completed, everything is set - return true.
            // This is rediculously expensive, and not meant to be called on tick - but as part of the cached Completed routine. 
            foreach ( TrainableDef def in Job.Training.Defs )
            {
                if ( Job.Training[def] )
                {
                    foreach( Utilities_Livestock.AgeAndSex ageSex in Utilities_Livestock.AgeSexArray )
                    {
                        foreach ( Pawn p in pawnKind.GetTame( ageSex ) )
                        {
                            if ( !p.training.GetWanted( def ) &&
                                 !p.training.IsCompleted( def ) )
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public int[] Counts
        {
            get { return Utilities_Livestock.AgeSexArray.Select( ageSex => pawnKind.GetTame( ageSex ).Count ).ToArray(); }
        }

        public override string StatusTooltip
        {
            get
            {
                List<string> tooltipArgs = new List<string>();
                tooltipArgs.Add( pawnKind.LabelCap );
                tooltipArgs.AddRange( Counts.Select( x => x.ToString() ) );
                tooltipArgs.AddRange( CountTargets.Values.Select( v => v.ToString() ) );
                return "FML.ListEntryTooltip".Translate( tooltipArgs.ToArray() );
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.LookDictionary( ref CountTargets, "Targets", LookMode.Value, LookMode.Value );
            Scribe_Defs.LookDef( ref pawnKind, "PawnKind" );
        }

        public override void DrawTriggerConfig( ref Vector2 cur, float width, float entryHeight, bool alt = false ) {}
    }
}