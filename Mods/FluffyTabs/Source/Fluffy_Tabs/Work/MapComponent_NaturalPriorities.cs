//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Verse;

//namespace Fluffy_Tabs
//{
//    public class MapComponent_NaturalPriorities : MapComponent
//    {
//        #region Fields

//        public static bool                          userWarned          = false;
//        private static List<Saveable_WorkTypeDef>   _priorities         = new List<Saveable_WorkTypeDef>();
//        private static List<Saveable_WorkTypeDef>   _XMLPriorities;

//        #endregion Fields

//        #region Methods

//        public static void InitMapComponent()
//        {
//            CreateXMLPrioritiesIfNotExists();
//            TryInjectMapComponent();
//        }

//        public static void ResetPriorities()
//        {
//            LoadPriorities( _XMLPriorities );
//        }

//        public override void ExposeData()
//        {
//            // if we're saving create the current list.
//            if ( Scribe.mode == LoadSaveMode.Saving )
//            {
//                _priorities = CreateSaveablePriorities();
//            }

//            // Scribe away!
//            Scribe_Collections.LookList( ref _priorities, "Priorities", LookMode.Deep );
//            Scribe_Values.LookValue( ref userWarned, "warned" );

//            // If we're loading, put these into the game.
//            if ( Scribe.mode == LoadSaveMode.ResolvingCrossRefs )
//            {
//                // first make a backup of XML defined priorities.
//                CreateXMLPrioritiesIfNotExists();

//                // then load the saved priorities.
//                LoadPriorities( _priorities );
//            }
//        }

//        private static List<Saveable_WorkTypeDef> CreateSaveablePriorities()
//        {
//            List<Saveable_WorkTypeDef> list = new List<Saveable_WorkTypeDef>();

//            foreach (
//                WorkTypeDef workTypeDef in
//                    DefDatabase<WorkTypeDef>.AllDefsListForReading.OrderByDescending( wt => wt.naturalPriority ) )
//            {
//                list.Add( new Saveable_WorkTypeDef( workTypeDef ) );
//            }

//            return list;
//        }

//        private static void CreateXMLPrioritiesIfNotExists()
//        {
//            if ( _XMLPriorities == null || _XMLPriorities.Count == 0 )
//            {
//                _XMLPriorities = CreateSaveablePriorities();
//            }
//        }

//        private static void LoadPriorities( List<Saveable_WorkTypeDef> saveableDefs )
//        {
//            // warning
//            if ( saveableDefs == null || saveableDefs.Count == 0 )
//            {
//                Log.Warning( "Loading priorities from a null / empty list." );
//                return;
//            }

//            // Load the stored stuff, if any, into the game.
//            foreach ( Saveable_WorkTypeDef workType in saveableDefs )
//            {
//                WorkTypeDef workTypeDef = DefDatabase<WorkTypeDef>.GetNamedSilentFail( workType.defName );
//                workTypeDef.naturalPriority = workType.priority;

//                foreach ( Saveable_WorkGiverDef workGiver in workType.workGivers )
//                {
//                    DefDatabase<WorkGiverDef>.GetNamedSilentFail( workGiver.defName ).priorityInType = workGiver.priority;
//                }
//            }
//        }

//        private static void TryInjectMapComponent()
//        {
//            if ( Find.Map.GetComponent<MapComponent_NaturalPriorities>() == null )
//            {
//                Log.Message( "Enhanced Tabs :: Injecting map component" );
//                Find.Map.components.Add( new MapComponent_NaturalPriorities() );
//            }
//        }

//        #endregion Methods
//    }

//    public class Saveable_WorkGiverDef : IExposable
//    {
//        #region Fields

//        public string defName;
//        public int priority;

//        #endregion Fields

//        #region Constructors

//        public Saveable_WorkGiverDef()
//        {
//            // empty constructor for scribe.
//        }

//        public Saveable_WorkGiverDef( WorkGiverDef def )
//        {
//            defName = def.defName;
//            priority = def.priorityInType;
//        }

//        #endregion Constructors

//        #region Methods

//        public void ExposeData()
//        {
//            Scribe_Values.LookValue( ref defName, "defName" );
//            Scribe_Values.LookValue( ref priority, "priority" );
//        }

//        #endregion Methods
//    }

//    public class Saveable_WorkTypeDef : IExposable
//    {
//        #region Fields

//        public string defName;
//        public int priority;
//        public List<Saveable_WorkGiverDef> workGivers;

//        #endregion Fields

//        #region Constructors

//        // empty constructor for scribe
//        public Saveable_WorkTypeDef()
//        {
//            // tralalalalala, in the morning!
//        }

//        public Saveable_WorkTypeDef( WorkTypeDef def )
//        {
//            defName = def.defName;
//            priority = def.naturalPriority;
//            workGivers = def.workGiversByPriority.Select( wg => new Saveable_WorkGiverDef( wg ) ).ToList();
//        }

//        #endregion Constructors

//        #region Methods

//        public void ExposeData()
//        {
//            Scribe_Values.LookValue( ref defName, "defName" );
//            Scribe_Values.LookValue( ref priority, "priority" );
//            Scribe_Collections.LookList( ref workGivers, "workGivers", LookMode.Deep );
//        }

//        #endregion Methods
//    }
//}