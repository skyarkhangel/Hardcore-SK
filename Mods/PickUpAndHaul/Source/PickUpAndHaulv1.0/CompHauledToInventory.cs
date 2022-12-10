namespace PickUpAndHaul
{
    using System.Collections.Generic;
    using Verse;

    public class CompHauledToInventory : ThingComp
    {
        private HashSet<Thing> takenToInventory = new HashSet<Thing>();

        public HashSet<Thing> GetHashSet()
        {
            takenToInventory.RemoveWhere(x => x == null);
            return takenToInventory;
        }
        
        public void RegisterHauledItem(Thing thing)
        {
            this.takenToInventory.Add(thing);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref takenToInventory, "ThingsHauledToInventory", LookMode.Reference);
        }
    }
}
