using RimWorld;
using System;
using System.Linq;
using Verse;

namespace FluffyManager
{
    public static class ResearchWorkers
    {
        public static void UnlockPowerTab()
        {
            if ( !Manager.Get.ManagerTabs.OfType<ManagerTab_Power>().Any() )
            {
                Manager.Get.ManagerTabs.Add( new ManagerTab_Power() );
                Manager.Get.RefreshTabs();
            }
        }
    }
}