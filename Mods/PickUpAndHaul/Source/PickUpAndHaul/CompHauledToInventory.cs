using System.Collections.Generic;
using Verse;

namespace PickUpAndHaul
{
    public class CompHauledToInventory : ThingComp
    {
        private HashSet<Thing> TakenToInventory = new HashSet<Thing>();

        public HashSet<Thing> GetHashSet()
        {
            TakenToInventory.RemoveWhere(x => x == null);
            return TakenToInventory;
        }
        
        public void RegisterHauledItem(Thing thing)
        {
            this.TakenToInventory.Add(thing);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look<Thing>(ref this.TakenToInventory, "ThingsHauledToInventory", LookMode.Reference);
        }
    }
}
