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
    public class SpecialInjector_DetourWorkSettings : SpecialInjector
    {
        #region Methods

        public override bool Inject()
        {
            MethodInfo cachePriorities_Vanilla = typeof( Pawn_WorkSettings ).GetMethod( "CacheWorkGiversInOrder", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo cachePriorities_Fluffy = typeof( Detours_WorkSettings ).GetMethod( "_CacheWorkGiversInOrder", BindingFlags.Instance | BindingFlags.Public );
            Detours.TryDetourFromTo( cachePriorities_Vanilla, cachePriorities_Fluffy );

            MethodInfo getPriority_Vanilla = typeof( Pawn_WorkSettings ).GetMethod( "GetPriority", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo getPriority_Fluffy = typeof( Detours_WorkSettings ).GetMethod( "_GetPriority", BindingFlags.Instance | BindingFlags.Public );
            Detours.TryDetourFromTo( getPriority_Vanilla, getPriority_Fluffy );

            MethodInfo setPriority_Vanilla = typeof( Pawn_WorkSettings ).GetMethod( "SetPriority", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo setPriority_Fluffy = typeof( Detours_WorkSettings ).GetMethod( "_SetPriority", BindingFlags.Instance | BindingFlags.Public );
            Detours.TryDetourFromTo( setPriority_Vanilla, setPriority_Fluffy );

            return true;
        }

        #endregion Methods
    }
}