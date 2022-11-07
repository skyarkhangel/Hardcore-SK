using System.Collections.Generic;
using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.Patches
{

    public class Building_Bed_Patch
    {
        /// <summary>
        /// When the room is made for prisoners, guest beds also switch to prisoner beds
        /// </summary>
        [HarmonyPatch(typeof(Building_Bed))]
        [HarmonyPatch(nameof(Building_Bed.ForPrisoners), MethodType.Setter)]
        public class ForPrisoners
        {
            public static void Postfix(Building_Bed __instance, bool value)
            {
                if (!value || !__instance.ForPrisoners) return;

                if (__instance is Building_GuestBed)
                {
                    Building_GuestBed.Swap(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Building_Bed), nameof(Building_Bed.GetGizmos))]
        public class GetGizmos
        {
            public static void Postfix(Building_Bed __instance, ref IEnumerable<Gizmo> __result)
            {
                __result = Process(__instance, __result);
            }

            private static IEnumerable<Gizmo> Process(Building_Bed __instance, IEnumerable<Gizmo> __result)
            {
                var isPrisonCell = __instance.GetRoom()?.isPrisonCell == true;
                if (!__instance.ForPrisoners && !__instance.Medical && __instance.def.building.bed_humanlike && !isPrisonCell)
                {
                    yield return
                        new Command_Toggle
                        {
                            defaultLabel = "CommandBedSetAsGuestLabel".Translate(),
                            defaultDesc = "CommandBedSetAsGuestDesc".Translate(),
                            icon = ContentFinder<Texture2D>.Get("UI/Commands/AsGuest"),
                            isActive = __instance.IsGuestBed,
                            toggleAction = () => Building_GuestBed.Swap(__instance),
                            hotKey = KeyBindingDefOf.Misc4,
                            disabled = !__instance.def.HasAssignableCompFrom(typeof(CompAssignableToPawn_Bed)),
                            disabledReason = "This bed type is not assignable to pawns."
                        };
                }
                foreach (var gizmo in __result)
                {
                    yield return gizmo;
                }
            }
        }
    }
}