using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;
using Verse;

namespace SimpleSidearms.rimworld
{
    public struct ThingDefStuffDefPair : IExposable, IEquatable<ThingDefStuffDefPair>
    {
        public ThingDef thing;
        public ThingDef stuff;

        public bool cachedThingStuffPairLoaded;
        public ThingStuffPair? cachedThingStuffPair;

        public ThingStuffPair? EquivalentThingStuffPair 
        {
            get 
            {
                if (!cachedThingStuffPair.HasValue) 
                {
                    var thisRef = this;
                    cachedThingStuffPair = ThingStuffPair.AllWith(t => t == thisRef.thing).Where(t => t.stuff == thisRef.stuff).FirstOrDefault();
                }
                return cachedThingStuffPair;
            }
        }

        public float Price
        {
            get
            {
                if (EquivalentThingStuffPair.HasValue)
                    return EquivalentThingStuffPair.Value.Price;
                else
                    return 0;
            }
        }

        public float Commonality
        {
            get
            {
                if (EquivalentThingStuffPair.HasValue)
                    return EquivalentThingStuffPair.Value.Commonality;
                else
                    return 0;
            }
        }

        public ThingDefStuffDefPair(ThingDef thing, ThingDef stuff = null)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));

            this.thing = thing;
            this.stuff = stuff;
            cachedThingStuffPairLoaded = false;
            this.cachedThingStuffPair = null;
        }

        public ThingDefStuffDefPair(ThingStuffPair thingStuffPair) : this()
        {
            if (thingStuffPair == null) throw new ArgumentNullException(nameof(thingStuffPair));
            if (thingStuffPair.thing == null) throw new ArgumentException(nameof(thingStuffPair)+".thing");


            this.thing = thingStuffPair.thing;
            this.stuff = thingStuffPair.stuff;
            this.cachedThingStuffPairLoaded = true;
            this.cachedThingStuffPair = thingStuffPair;
        }

        public void ExposeData()
        {
            Scribe_Defs.Look<ThingDef>(ref this.thing, "thing");
            Scribe_Defs.Look<ThingDef>(ref this.stuff, "stuff");
        }

        public static bool operator ==(ThingDefStuffDefPair a, ThingDefStuffDefPair b)
        {
            return a.thing == b.thing && a.stuff == b.stuff;
        }

        public static bool operator !=(ThingDefStuffDefPair a, ThingDefStuffDefPair b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            int num = 0;
            if (this.thing != null)
            {
                num = Gen.HashCombineInt(num, (int)this.thing.shortHash);
            }
            else
            {
                Log.Warning("There is a ThingDefStuffDefPair with null thing!");
            }
            if (this.stuff != null)
            {
                num = Gen.HashCombineInt(num, (int)this.stuff.shortHash);
            }
            return num;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            if (obj is ThingDefStuffDefPair)
                return this == (ThingDefStuffDefPair)obj;
            return false;
        }

        public bool Equals(ThingDefStuffDefPair other)
        {
            return this == other;
        }
    }
}
