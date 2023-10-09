using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                    ThingWithComps newThing = (ThingWithComps)ThingMaker.MakeThing(thing.def);
                    newThing.stackCount = thing.stackCount;

                    float progress = thing.TryGetComp<CompRottable>().RotProgress;

                    thing.Destroy();
                    CompProperties_EggLayer foundRace = DefDatabase<ThingDef>.AllDefsListForReading.Find(d => d.comps != null
                        && d.GetCompProperties<CompProperties_EggLayer>() != null && d.GetCompProperties<CompProperties_EggLayer>().eggFertilizedDef == newThing.def).GetCompProperties<CompProperties_EggLayer>();
                    //Log.Message("unfertilised egg is: " + foundRace != null ? foundRace.eggUnfertilizedDef.defName : "Null");
                    if (foundRace == null || foundRace.eggUnfertilizedDef == null || (foundRace.eggFertilizedDef != null && foundRace.eggFertilizedDef == foundRace.eggUnfertilizedDef))
                        newThing.def = DefDatabase<ThingDef>.GetNamed("EggChickenUnfertilized");
                    else
                        newThing.def = foundRace.eggUnfertilizedDef;
                    //CompProperties_Rottable rottable = newThing.GetCompProperties<CompProperties_Rottable>();
                    //if (rottable != null)

                    GenPlace.TryPlaceThing(newThing, pos, map, ThingPlaceMode.Near);
                    //GenSpawn.Spawn(newThing, pos, map);

                    foreach (var item in pos.GetThingList(map))
                    {
                        CompRottable rot = null;
                        if (item.def == newThing.def)
                            rot = item.TryGetComp<CompRottable>();
                        if (progress > 0 && rot != null && rot.RotProgress < 30)
                            rot.RotProgress = progress;
                    }
                }
            }
        }
    }
}
