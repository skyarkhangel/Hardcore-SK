using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;   // Always needed
using RimWorld;      // RimWorld specific functions 
using Verse;         // RimWorld universal objects 

namespace MineralsFrozen
{


    [HarmonyPatch(typeof(WorkGiver_Repair))]
    [HarmonyPatch("HasJobOnThing")]
    public static class RepairIceWalls
    {

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing t, bool forced, WorkGiver_Repair __instance, ref bool __result)
        {
            if (__result == false)
            {
                return;
            }

            if (FrozenWallBase.isFrozenWall(t))
            {
                if (((double)t.HitPoints / t.MaxHitPoints) > ((FrozenWallBase)t).attributes.maxHealHP)
                {
                    __result = forced;
                }
            }
        }
    }


}