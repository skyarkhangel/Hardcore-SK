using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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
                    IntVec3 pos = thing.Position;
                    Map map = thing.Map;
                    ThingDef newThing = thing.def;
                    
                    thing.Destroy();
                    CompProperties_EggLayer foundRace = DefDatabase<ThingDef>.AllDefsListForReading.Find(d => d.comps != null
                        && d.GetCompProperties<CompProperties_EggLayer>() != null && d.GetCompProperties<CompProperties_EggLayer>().eggFertilizedDef == newThing).GetCompProperties<CompProperties_EggLayer>();
                    Log.Message("unfertilised egg is: " + foundRace != null ? foundRace.eggUnfertilizedDef.defName : "Null");
                    if (foundRace == null || foundRace.eggFertilizedDef == foundRace.eggUnfertilizedDef)
                        newThing = DefDatabase<ThingDef>.GetNamed("EggChickenUnfertilized");
                    else
                        newThing = foundRace.eggUnfertilizedDef;
                    GenSpawn.Spawn(newThing, pos, map);

                }
            }
        }
    }
}
