using HarmonyLib;
using MorePlanning.Designators;
using MorePlanning.Plan;
using UnityEngine;
using Verse;
using Resources = MorePlanning.Common.Resources;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeTypeModifiers

namespace MorePlanning.Patches
{
    [HarmonyPatch(typeof(Designation))]
    [HarmonyPatch("DesignationDraw")]
    class DesignationPlanningDraw
    {
        static bool Prefix(Designation __instance)
        {
            if (__instance.def is PlanDesignationDef == false)
            {
                return true;
            }

            if (VisibilityDesignator.PlanningVisibility == false)
            {
                return false;
            }

            int colorId = 1;

            if (__instance is PlanDesignation designation)
            {
                colorId = designation.Color;
            }

            Vector3 position = __instance.target.Cell.ToVector3ShiftedWithAltitude(__instance.DesignationDrawAltitude);
            Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, Resources.PlanMatColor[colorId], 0);

            return false;
        }
    }
}