using HarmonyLib;
using Verse;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeModifiers

namespace MorePlanning.Patches
{
    [HarmonyPatch(typeof(Designation))]
    [HarmonyPatch("ExposeData")]
    class DesignationPlanningExposeData
    {
        static bool Prefix(Designation __instance)
        {
            if (__instance is PlanDesignation designation)
            {
                Scribe_Values.Look(ref designation.Color, "Color", 0, true);
            }
            
            return true;
        }
    }
}