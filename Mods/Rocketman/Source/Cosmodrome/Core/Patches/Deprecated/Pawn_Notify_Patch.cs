using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RocketMan.Patches
{
    public static class Pawn_Notify_Dirty
    {
        [RocketPatch()]
        public static class Pawn_ApparelTracker_Dirty
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Pawn_ApparelTracker),
                    nameof(Pawn_ApparelTracker.Notify_ApparelAdded));
                yield return AccessTools.Method(typeof(Pawn_ApparelTracker),
                    nameof(Pawn_ApparelTracker.Notify_ApparelRemoved));
                yield return AccessTools.Method(typeof(Pawn_ApparelTracker),
                    nameof(Pawn_ApparelTracker.Notify_LostBodyPart));
                //yield return AccessTools.Method(typeof(Pawn_ApparelTracker),
                //    nameof(Pawn_ApparelTracker.));
                yield return AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear));
                yield return AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove));
            }

            [HarmonyPriority(int.MaxValue)]
            public static void Postfix(Pawn_ApparelTracker __instance)
            {
                __instance.pawn.Notify_Dirty();
            }
        }

        [RocketPatch()]
        public static class Pawn_EquipmentTracker_Dirty
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Notify_EquipmentAdded));
                yield return AccessTools.Method(typeof(Pawn_EquipmentTracker), nameof(Pawn_EquipmentTracker.Notify_EquipmentRemoved));
            }

            [HarmonyPriority(int.MaxValue)]
            public static void Postfix(Pawn_EquipmentTracker __instance)
            {
                __instance.pawn.Notify_Dirty();
            }
        }

        [RocketPatch()]
        public static class Pawn_Dirty
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Pawn), nameof(Pawn.Destroy));
                yield return AccessTools.Method(typeof(Pawn), nameof(Pawn.Notify_Equipped));
                yield return AccessTools.Method(typeof(Pawn), nameof(Pawn.Notify_Teleported));
                yield return AccessTools.Method(typeof(Pawn), nameof(Pawn.Notify_UsedWeapon));
                yield return AccessTools.Method(typeof(Pawn), nameof(Pawn.Notify_BulletImpactNearby));
            }

            [HarmonyPriority(int.MaxValue)]
            public static void Postfix(Pawn __instance)
            {
                __instance.Notify_Dirty();
            }
        }

        [RocketPatch()]
        public static class Pawn_HealthTracker_Dirty
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_HediffChanged));
                yield return AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_UsedVerb));
            }

            [HarmonyPriority(int.MaxValue)]
            public static void Postfix(Pawn_HealthTracker __instance)
            {
                __instance.pawn.Notify_Dirty();
            }
        }
    }
}