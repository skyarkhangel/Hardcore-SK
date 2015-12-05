using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Fluffy
{
    class MapComponent_Priorities : MapComponent
    {
        private static List<Saveable_WorkTypeDef>   _priorities         = new List<Saveable_WorkTypeDef>();
        private static List<Saveable_WorkTypeDef>   _XMLPriorities;
        public static bool                          userWarned          = false;

        public override void ExposeData()
        {
            // if we're saving create the current list.
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                _priorities = CreateSaveablePriorities();
            }
            
            // Scribe away!
            Scribe_Collections.LookList(ref _priorities, "Priorities", LookMode.Deep);
            Scribe_Values.LookValue(ref userWarned, "warned");

            // If we're loading, put these into the game.
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                // first make a backup of XML defined priorities.
                CreateXMLPrioritiesIfNotExists();

                // then load the saved priorities.
                LoadPriorities(_priorities);
            }
        }
        
        public static void ResetPriorities()
        {
            LoadPriorities(_XMLPriorities);
        }

        private static void LoadPriorities(List<Saveable_WorkTypeDef> saveableDefs)
        {
            // warning
            if (saveableDefs == null || saveableDefs.Count == 0)
            {
                Log.Warning("Loading priorities from a null / empty list.");
                return;
            }

            // Load the stored stuff, if any, into the game.
            foreach (Saveable_WorkTypeDef workType in saveableDefs )
            {
                WorkTypeDef workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail(workType.defName);
                workTypeDef.naturalPriority = workType.priority;

                foreach (Saveable_WorkGiverDef workGiver in workType.workGivers)
                {
                    DefDatabase<WorkGiverDef>.GetNamedSilentFail(workGiver.defName).priorityInType = workGiver.priority;
                }

                // sort the cache correctly an notify pawns of changed priorities, which also notifies them of the worktype priority changes.
                // normalization of workTypes is handled in the dialog, and is irrelevant outside of the dialog.
                Dialog_Priority.RebuildWorkGiverDefsList(workTypeDef);
            }
        }
        
        private static List<Saveable_WorkTypeDef> CreateSaveablePriorities()
        {
            List<Saveable_WorkTypeDef> list = new List<Saveable_WorkTypeDef>();
            
            foreach(
                WorkTypeDef workTypeDef in
                    DefDatabase<WorkTypeDef>.AllDefsListForReading.OrderByDescending( wt => wt.naturalPriority ) )
            {
                list.Add( new Saveable_WorkTypeDef( workTypeDef ) );
            }

            return list;
        }

        public static void InitMapComponent()
        {
            CreateXMLPrioritiesIfNotExists();
            TryInjectMapComponent();
        }

        private static void TryInjectMapComponent()
        {
            if( Find.Map.GetComponent<MapComponent_Priorities>() == null )
            {
                Log.Message( "Enhanced Tabs :: Injecting map component" );
                // all the relevant bits are static, just create a new one.
                Find.Map.components.Add( new MapComponent_Priorities() );
            }
        }

        private static void CreateXMLPrioritiesIfNotExists()
        {
            if (_XMLPriorities == null || _XMLPriorities.Count == 0)
            {
                _XMLPriorities = CreateSaveablePriorities();
            }
        }
    }
}
