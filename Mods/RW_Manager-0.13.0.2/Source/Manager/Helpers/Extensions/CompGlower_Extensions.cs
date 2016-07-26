using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace FluffyManager
{
    public static class CompGlower_Extensions
    {
        private static FieldInfo _litFI = typeof( CompGlower ).GetField( "glowOnInt", BindingFlags.Instance | BindingFlags.NonPublic );

        public static void SetLit( this CompGlower glower, bool lit = true ) {
            if ( _litFI == null )
                throw new Exception( "Field glowOnInt not found in CompGlower" );

            _litFI.SetValue( glower, lit );
        }
    }
}
