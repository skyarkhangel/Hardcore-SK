using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalsLogic
{
    /*
     * Transforms any ruined egg into unfertilized chicken egg.
     */

    class RuinedEggs
    {
        [HarmonyPatch(typeof(CompTemperatureRuinable), "DoTicks", new Type[] { typeof(int) })]
        static class CompTemperatureRuinable_DoTicks_Patch
        {
            static bool Prefix(ref bool __state, ref CompTemperatureRuinable __instance)
            {
                __state = __instance.Ruined;
                return true;
            }

            static void Postfix(ref bool __state, ref CompTemperatureRuinable __instance)
            {
                if (Settings.convert_ruined_eggs && !__state && __instance.Ruined) // Thing is ruined after this tick
                {
                    ThingWithComps thing = __instance.parent;
                    Map map = thing.Map;
                    foreach (var item in thing.AllComps)
                    {
                        if (item.props.GetType() == typeof(CompProperties_Hatcher))
                        {
                            thing.DeSpawn();
                            thing.def = DefDatabase<ThingDef>.GetNamed("EggChickenUnfertilized");
                            thing.AllComps.Remove(__instance);
                            thing.AllComps.Remove(item);
                            thing.SpawnSetup(map, true);
                            break;
                        }
                    }
                }
            }
        }
    }
}
