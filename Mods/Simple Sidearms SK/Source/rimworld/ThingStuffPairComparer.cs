using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleSidearms.rimworld
{
    public class ThingDefStuffDefComparer : IEqualityComparer<ThingDefStuffDefPair>
    {
        public bool Equals(ThingDefStuffDefPair x, ThingDefStuffDefPair y)
        {
            if (x == y)
                return true;
            else
                return false;
        }

        public int GetHashCode(ThingDefStuffDefPair obj)
        {
            return obj.GetHashCode();
        }
    }
}
