using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace aRandomKiwi.HFM
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.race != null && thingDef.race.Animal)
                {
                    thingDef.comps.Add(new CompProps_Hunting());
                }
            }
            var inst = new Harmony("rimworld.randomKiwi.hfm");
            inst.PatchAll(Assembly.GetExecutingAssembly());

            Utils.setAllowAllToHuntState();
        }

        public static FieldInfo MapFieldInfo;
    }
}
