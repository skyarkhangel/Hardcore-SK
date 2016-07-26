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

            return true;
        }

        #endregion Methods
    }
}