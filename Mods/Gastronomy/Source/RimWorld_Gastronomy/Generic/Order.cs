using Verse;

namespace Gastronomy
{
    public class Order : IExposable
    {
        public Thing consumable;
        public ThingDef consumableDef;
        public Pawn patron;
        public bool hasToBeMade;
        public bool delivered;

        public void ExposeData()
        {
            Scribe_References.Look(ref patron, "patron");
            Scribe_Defs.Look(ref consumableDef, "consumableDef");
            Scribe_References.Look(ref consumable, "consumable");
            Scribe_Values.Look(ref hasToBeMade, "hasToBeMade");
            Scribe_Values.Look(ref delivered, "delivered");
        }
    }
}
