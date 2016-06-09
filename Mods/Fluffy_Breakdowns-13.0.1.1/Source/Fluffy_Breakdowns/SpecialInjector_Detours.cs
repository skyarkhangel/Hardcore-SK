using CommunityCoreLibrary;
using RimWorld;
using System.Reflection;

namespace Fluffy_Breakdowns
{
    public class SpecialInjector_Detours : SpecialInjector
    {
        #region Methods

        public override bool Inject()
        {
            // check for breakdown
            MethodInfo Vanilla_CheckForBreakdown = typeof( CompBreakdownable ).GetMethod( "CheckForBreakdown", BindingFlags.Public | BindingFlags.Instance );
            MethodInfo Fluffy_CheckForBreakdown = typeof( CompBreakdownableMaintenance ).GetMethod( "_checkForBreakdown", BindingFlags.Public | BindingFlags.Instance );
            Detours.TryDetourFromTo( Vanilla_CheckForBreakdown, Fluffy_CheckForBreakdown );

            // report string extra
            MethodInfo Vanilla_CompInspectStringExtra = typeof( CompBreakdownable ).GetMethod( "CompInspectStringExtra", BindingFlags.Public | BindingFlags.Instance );
            MethodInfo Fluffy_CompInspectStringExtra = typeof( CompBreakdownableMaintenance ).GetMethod( "_compInspectStringExtra", BindingFlags.Public | BindingFlags.Instance );
            Detours.TryDetourFromTo( Vanilla_CompInspectStringExtra, Fluffy_CompInspectStringExtra );

            return true;
        }

        #endregion Methods
    }
}