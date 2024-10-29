using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace AutoRebuildTerrainOnSeismicActivity
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            new Harmony("com.ZB333ZB.AutoRebuildTerrainOnSeismicActivity").PatchAll();
        }
    }

    [HarmonyPatch(typeof(SK.Events.WeatherEvent_Tremor))]
    [HarmonyPatch("CreateFaultCell")]
    [HarmonyPatch(new Type[] { typeof(IntVec3) })]
    public static class Patch_WeatherEvent_Tremor_CreateFaultCell
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].Calls(typeof(TerrainGrid).GetMethod("RemoveTopLayer")))
                {
                    codes[i] = new CodeInstruction(OpCodes.Call, 
                        typeof(Patch_WeatherEvent_Tremor_CreateFaultCell)
                        .GetMethod(nameof(RemoveTerrainWithNotify)));
                }
            }
            
            return codes;
        }

        public static void RemoveTerrainWithNotify(TerrainGrid terrainGrid, IntVec3 cell, bool doLeavings)
        {
            TerrainDef terrain = terrainGrid.TerrainAt(cell);
            if (terrain != null && terrain.Removable)
            {
                terrainGrid.Notify_TerrainDestroyed(cell);
            }
        }
    }
}