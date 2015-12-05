using System.Collections.Generic;
using System.Linq;

using Verse;
using RimWorld;

namespace SK_Enviro.Seeds
{
    internal class StockGenerator_Seeds : StockGenerator
    {
        private IntRange thingDefCountRange = IntRange.one;

        public override IEnumerable<Thing> GenerateThings()
        {
            var seeds = DefDatabase<ThingDef>.AllDefs.OfType<ThingDef_PlantSeedItem>().ToList();
            var count = thingDefCountRange.RandomInRange;

            for (int l1 = 0; l1 < count; l1++)
            {
                var seed = seeds.RandomElement();
                var item = ThingMaker.MakeThing(seed);
                item.stackCount = RandomCountOf(seed);
                yield return item;
            }
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef is ThingDef_PlantSeedItem;
        }
    }
}

