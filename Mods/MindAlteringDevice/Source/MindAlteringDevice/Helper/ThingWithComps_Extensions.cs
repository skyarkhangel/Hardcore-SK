using System.Reflection;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{

    public static class ThingWithComps_Extensions
    {
        #region easier on the eyes if I do it this way. Google says I should be carefull with extentions... oh well

        public static void SetComps(this ThingWithComps thingWithComps, List<ThingComp> comps)
        {
            // Hate this bit of code. thank fuck for extentions
            typeof(ThingWithComps).GetField("comps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(thingWithComps, comps);
        }


        public static List<ThingComp> GetComps(this ThingWithComps thingWithComps)
        {
            // useless bit... but now I have a nice looking Get and Set ...
            return thingWithComps.AllComps;
        }

        #endregion

    }

}