using System;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using RocketMan;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Soyuz.Patches
{
    public static class Pawn_PathFollower_Patch
    {
        private static float remaining = 0f;

        private static float costPaid = 0f;

        private static Pawn pawn;

        private static bool setupNext = false;

        private static void Reset()
        {
            pawn = null;
            costPaid = 0f;
            setupNext = false;
            remaining = 0f;
        }

        [SoyuzPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.PatherTick))]
        public class Pawn_PathFollower_PatherTick
        {
            public static void Prefix(Pawn_PathFollower __instance)
            {
                Pawn_PathFollower_Patch.Reset();

                if (__instance.pawn.IsBeingThrottled())
                {
                    pawn = __instance.pawn;
                }
            }

            public static void Postfix(Pawn_PathFollower __instance)
            {
                if (setupNext && pawn == __instance.pawn)
                {
                    __instance.TryEnterNextPathCell();

                    if (__instance.moving)
                    {
                        if (remaining < __instance.nextCellCostTotal / 450 && remaining > 0)
                        {
                            remaining = __instance.nextCellCostTotal / 450;
                        }
                        __instance.nextCellCostLeft -= remaining;
                    }
                }
                if (__instance.pawn == Context.ProfiledPawn)
                {
                    __instance.pawn.GetPatherModel().AddResult(costPaid);
                }
            }

            public static Exception Finalizer(Exception __exception)
            {
                if (__exception != null)
                {
                    Pawn_PathFollower_Patch.Reset();
                }
                return __exception;
            }
        }

        [SoyuzPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.CostToPayThisTick))]
        public class Pawn_PathFollower_CostToPayThisTick_Patch
        {
            public static void Postfix(Pawn_PathFollower __instance, ref float __result)
            {
                if (__instance.pawn == pawn)
                {
                    float dT = __instance.pawn.GetTimeDelta();
                    float modified = __result * dT;
                    float cost = __instance.nextCellCostLeft;

                    if (modified > cost)
                    {
                        remaining = modified - cost;
                        modified = cost;
                        setupNext = dT > 1 && remaining > 0;
                    }
                    __result = modified;

                }
                costPaid = __result;
            }
        }
    }
}