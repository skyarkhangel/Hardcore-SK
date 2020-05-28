using Verse;

namespace Infused
{
    public class SpecialThingFilterWorker_InfusedApparel : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t) => CanEverMatch(t.def) && WantsInfused == CompInfused.TryGetInfusedComp(t, out CompInfused comp);

        public override bool CanEverMatch(ThingDef def) => def.IsApparel;

        public virtual bool WantsInfused => true;
    }

    public class SpecialThingFilterWorker_NonInfusedApparel : SpecialThingFilterWorker_InfusedApparel
    {
        public override bool WantsInfused => false;
    }

    public class SpecialThingFilterWorker_InfusedWeapons : SpecialThingFilterWorker_InfusedApparel
    {
        public override bool CanEverMatch(ThingDef def) => def.IsWeapon;
    }

    public class SpecialThingFilterWorker_NonInfusedWeapons : SpecialThingFilterWorker_InfusedWeapons
    {
        public override bool WantsInfused => false;
    }
}
