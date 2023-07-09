using HarmonyLib;
using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{

    [HarmonyPatch(typeof(Alert_HunterLacksRangedWeapon), "HuntersWithoutRangedWeapon")]
    [HarmonyPatch(MethodType.Getter)]
    public static class Alert_HunterLacksRangedWeapon_HuntersWithoutRangedWeapon_Postfix
    {
        [HarmonyPostfix]
        public static void HuntersWithoutRangedWeapon(Alert_HunterLacksRangedWeapon __instance, ref List<Pawn> __result)
        {
            List<Pawn> editedList = new List<Pawn>();
            foreach (Pawn pawn in __result)
            {
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (pawnMemory != null && pawn.IsValidSidearmsCarrier() && pawnMemory.IsUsingAutotool(true, true))
                {
                    continue;
                }
                //if I implement default-melee ranged hunting, the check goes here
                editedList.Add(pawn);
            }
            __result = editedList;
        }
    }
}
