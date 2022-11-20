using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Hospitality.Utilities
{
    public static class ThingCache
    {
        [NotNull]
        private static readonly Dictionary<Map, ThingCacheSet> allCacheSets = new Dictionary<Map, ThingCacheSet>();

        [NotNull]
        public static ThingCacheSet GetSetFor(Map map)
        {
            if (map == null)
            {
                Log.Error($"Hospitality ThingCache: Called with map == null.");
                return new ThingCacheSet();
            }

            if (allCacheSets.TryGetValue(map, out var set)) return set;
            var newSet = new ThingCacheSet();
            allCacheSets.Add(map, newSet);
            return newSet;
        }

        public static void TryRegisterNewThing(Thing thing)
        {
            if (thing?.Map == null) return;
            if (!allCacheSets.ContainsKey(thing.Map))
            {
                allCacheSets.Add(thing.Map, new ThingCacheSet());
            }

            allCacheSets[thing.Map].TryRegister(thing);
        }

        public static void TryDeregister(Thing thing, Map oldMap)
        {
            if (thing == null || oldMap == null) return;
            if (allCacheSets.ContainsKey(oldMap))
            {
                allCacheSets[oldMap].Deregister(thing);
            }
        }

        public class ThingCacheSet
        {
            private readonly List<Thing> vendingMachines = new List<Thing>();
            public IEnumerable<Thing> AllVendingMachines => vendingMachines;

            public void TryRegister(Thing newThing)
            {
                if (newThing is ThingWithComps thingWithComps)
                {
                    if (thingWithComps.TryGetComp<CompVendingMachine>() != null)
                    {
                        vendingMachines.Add(newThing);
                    }
                }
            }

            public void Deregister(Thing thing)
            {
                vendingMachines.Remove(thing);
            }
        }
    }
}
