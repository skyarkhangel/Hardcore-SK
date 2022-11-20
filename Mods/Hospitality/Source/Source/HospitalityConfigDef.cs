using System.Collections.Generic;
using Verse;

namespace Hospitality
{
    public class HospitalityConfigDef : Def
    {
        public List<ThingDef> vendingMachines;

        public static HospitalityConfigDef Config => DefDatabase<HospitalityConfigDef>.GetNamed("MainConfig");
    }
}
