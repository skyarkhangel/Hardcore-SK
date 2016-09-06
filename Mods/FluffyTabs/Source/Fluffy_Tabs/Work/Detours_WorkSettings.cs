using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Tabs
{
    public class Detours_WorkSettings : Pawn_WorkSettings
    {
        #region Fields

        public static FieldInfo pawnField = typeof( Pawn_WorkSettings ).GetField( "pawn", BindingFlags.Instance | BindingFlags.NonPublic );
        public static FieldInfo prioritiesField = typeof( Pawn_WorkSettings ).GetField( "priorities", BindingFlags.Instance | BindingFlags.NonPublic );
        public static FieldInfo workgiversDirtyField = typeof( Pawn_WorkSettings ).GetField( "workGiversDirty", BindingFlags.Instance | BindingFlags.NonPublic );
        public static FieldInfo workgiversEmergencyField = typeof( Pawn_WorkSettings ).GetField( "workGiversInOrderEmerg", BindingFlags.Instance | BindingFlags.NonPublic );
        public static FieldInfo workgiversNormalField = typeof( Pawn_WorkSettings ).GetField( "workGiversInOrderNormal", BindingFlags.Instance | BindingFlags.NonPublic );

        #endregion Fields

        #region Methods

        /// <summary>
        /// This method is used by vanilla to check if a pawns is assigned to a specific job for the right click menu.
        /// Don't ask me why, since the right click menu iterates over workgiversInOrder, which ONLY includes enabled workgivers/types...
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public int _GetPriority( WorkTypeDef w )
        {
            Pawn pawn = pawnField.GetValue( this ) as Pawn;

            return pawn.Priorities()?.GetPriority( w ) ?? 0; // return 0 if no tracker was found
        }

        /// <summary>
        /// This detours the vanilla SetPriority method to the Work Tabs priorities, thus ensuring that rogue vanilla or modded code is applied to this mod's priorities. 
        /// </summary>
        /// <param name="w"></param>
        /// <param name="priority"></param>
        public void _SetPriority( WorkTypeDef w, int priority )
        {
            Pawn pawn = pawnField.GetValue( this ) as Pawn;
            
            // set priority in work tab's priority list
            if (pawn.Priorities() != null)
                pawn.Priorities().SetPriority( w, priority );
            // if not available, make sure that changes ARE propagated to vanilla
            else
                SetVanillaPriority( pawn, w, priority );
        }

        internal static void SetVanillaPriority( Pawn pawn, WorkTypeDef workTypeDef, int priority )
        {
            // get value
            DefMap<WorkTypeDef, int> priorities = GetVanillaPriorities( pawn );

            // cop out on issues
            if ( priorities == null )
            {
                Log.Warning( "Vanilla priorities for " + pawn.LabelShort + " not found." );
                return;
            }

            // update
            priorities[workTypeDef] = priority;
        }

        internal static DefMap<WorkTypeDef, int> GetVanillaPriorities( Pawn pawn )
        {
            if ( pawn?.workSettings == null )
                return null;
            return prioritiesField.GetValue( pawn.workSettings ) as DefMap<WorkTypeDef, int>;
        }

        /// <summary>
        /// This method deviates from vanilla in that it also allows sorting pawns by player set workGIVER priorities
        /// </summary>
        public void _CacheWorkGiversInOrder()
        {
            var allWorkgivers = DefDatabase<WorkGiverDef>.AllDefsListForReading.Select( wgd => wgd.Worker );
            List<WorkGiver> normalWorkgivers = new List<WorkGiver>();
            List<WorkGiver> emergencyWorkgivers = new List<WorkGiver>();
            Pawn pawn = pawnField.GetValue( this ) as Pawn;

            // order workgivers
            if ( MapComponent_Priorities.Instance.DwarfTherapistMode )
            {
                // sort by player set workgiver priorities => worktype order => workgiver order
                allWorkgivers = allWorkgivers.Where( wg => pawn.Priorities().GetPriority( wg.def ) > 0 );

                if ( allWorkgivers.Any() )
                {
                    allWorkgivers = allWorkgivers
                        .OrderBy( wg => pawn.Priorities().GetPriority( wg.def ) )
                        .ThenByDescending( wg => wg.def.workType.naturalPriority )
                        .ThenByDescending( wg => wg.def.priorityInType ).ToList();

                    // lowest priority non-emergency job
                    int maxEmergPrio = allWorkgivers.Where( wg => !wg.def.emergency ).Min( wg => pawn.Priorities().GetPriority( wg.def ) );

                    // create lists of workgivers
                    normalWorkgivers = allWorkgivers.Where( wg => !wg.def.emergency || pawn.Priorities().GetPriority( wg.def ) > maxEmergPrio ).ToList();
                    emergencyWorkgivers = allWorkgivers.Where( wg => wg.def.emergency && pawn.Priorities().GetPriority( wg.def ) <= maxEmergPrio ).ToList();
                }
            }
            else
            {
                // sort by player set worktype priorities => worktype order => workgiver order
                allWorkgivers = allWorkgivers.Where( wg => pawn.Priorities().GetPriority( wg.def.workType ) > 0 );

                if ( allWorkgivers.Any() )
                {
                    allWorkgivers = allWorkgivers
                        .OrderBy( wg => pawn.Priorities().GetPriority( wg.def.workType ) )
                        .ThenByDescending( wg => wg.def.workType.naturalPriority )
                        .ThenByDescending( wg => wg.def.priorityInType ).ToList();

                    // lowest priority non-emergency job
                    int maxEmergPrio = allWorkgivers.Where( wg => !wg.def.emergency ).Min( wg => pawn.Priorities().GetPriority( wg.def.workType ) );

                    // create lists of workgivers
                    normalWorkgivers = allWorkgivers.Where( wg => !wg.def.emergency || pawn.Priorities().GetPriority( wg.def.workType ) > maxEmergPrio ).ToList();
                    emergencyWorkgivers = allWorkgivers.Where( wg => wg.def.emergency && pawn.Priorities().GetPriority( wg.def.workType ) <= maxEmergPrio ).ToList();
                }
            }

            // update cached lists of workgivers
            workgiversNormalField.SetValue( this, normalWorkgivers );
            workgiversEmergencyField.SetValue( this, emergencyWorkgivers );
            workgiversDirtyField.SetValue( this, false );
        }

        #endregion Methods
    }
}