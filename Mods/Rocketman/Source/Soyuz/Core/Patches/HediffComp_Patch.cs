using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Soyuz.Patches
{
    public class HediffComp_Patch
    {
        [SoyuzPatch]
        public static class HediffComp_GenHashInterval_Replacement
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                MethodBase method;
                yield return AccessTools.Method(typeof(Hediff), nameof(Hediff.Tick));
                foreach (var type in typeof(Hediff).AllSubclassesNonAbstract())
                {
                    method = type.GetMethod("Tick");
                    if (method != null && method.HasMethodBody()) yield return method;
                }

                yield return AccessTools.Method(typeof(HediffComp), nameof(HediffComp.CompPostTick));
                foreach (var type in typeof(HediffComp).AllSubclassesNonAbstract())
                {
                    method = type.GetMethod("CompPostTick");
                    if (method != null && method.HasMethodBody()) yield return method;
                }
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return instructions.MethodReplacer(
                    AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) }),
                    AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval)));
            }
        }

        [SoyuzPatch(typeof(HediffComp_ChanceToRemove), nameof(HediffComp_ChanceToRemove.CompPostTick))]
        public static class HediffComp_ChanceToRemove_Patch
        {
            public static void Prefix(HediffComp_ChanceToRemove __instance)
            {
                if (__instance.parent.pawn.IsBeingThrottled())
                    __instance.currentInterval -= __instance.parent.pawn.GetTimeDelta();
            }
        }

        [SoyuzPatch(typeof(HediffComp_ChangeNeed), nameof(HediffComp_ChangeNeed.CompPostTick))]
        public static class HediffComp_ChangeNeed_Patch
        {
            public static void Prefix(HediffComp_ChangeNeed __instance)
            {
                if (__instance.Need != null && __instance.parent.pawn.IsBeingThrottled())
                    __instance.Need.CurLevelPercentage += __instance.Props.percentPerDay / 60000f * (__instance.parent.pawn.GetTimeDelta() - 1);
            }
        }

        [SoyuzPatch(typeof(HediffComp_Disappears), nameof(HediffComp_Disappears.CompPostTick))]
        public static class HediffComp_Disappears_Patch
        {
            public static void Prefix(HediffComp_Disappears __instance)
            {
                if (__instance.parent.pawn.IsBeingThrottled())
                    __instance.ticksToDisappear -= (__instance.parent.pawn.GetTimeDelta() - 1);
            }
        }

        [SoyuzPatch(typeof(HediffComp_Discoverable), nameof(HediffComp_Discoverable.CompPostTick))]
        public static class HediffComp_Discoverable_Patch
        {
            public static void Prefix(HediffComp_Discoverable __instance)
            {
                if (__instance.parent.pawn.IsBeingThrottled() && GenTicks.TicksGame % 250 == 0)
                    __instance.CheckDiscovered();
            }
        }

        [SoyuzPatch(typeof(HediffComp_HealPermanentWounds), nameof(HediffComp_HealPermanentWounds.CompPostTick))]
        public static class HediffComp_HealPermanentWounds_Patch
        {
            public static void Prefix(HediffComp_HealPermanentWounds __instance)
            {
                if (__instance.parent.pawn.IsBeingThrottled())
                    __instance.ticksToHeal -= (__instance.parent.pawn.GetTimeDelta() - 1);
            }
        }

        [SoyuzPatch(typeof(HediffComp_Infecter), nameof(HediffComp_Infecter.CompPostTick))]
        public static class HediffComp_Infecter_Patch
        {
            public static void Prefix(HediffComp_Infecter __instance)
            {
                if (__instance.parent.pawn.IsBeingThrottled() && __instance.ticksUntilInfect > 0)
                {
                    __instance.ticksUntilInfect -= (__instance.parent.pawn.GetTimeDelta() - 1);
                    if (__instance.ticksUntilInfect <= 0)
                        __instance.ticksUntilInfect = 1;
                }
            }
        }

        [SoyuzPatch(typeof(HediffComp_SelfHeal), nameof(HediffComp_SelfHeal.CompPostTick))]
        public static class HediffComp_SelfHeal_Patch
        {
            public static void Prefix(HediffComp_SelfHeal __instance, ref float severityAdjustment)
            {
                if (__instance.parent.pawn.IsBeingThrottled())
                {
                    int dT = __instance.parent.pawn.GetTimeDelta() - 1;
                    int dT_hediff = __instance.Props.healIntervalTicksStanding;

                    __instance.ticksSinceHeal += dT;
                    if (__instance.ticksSinceHeal > dT_hediff && dT_hediff != 0)
                        severityAdjustment -= __instance.Props.healAmount * Mathf.Max((float)__instance.ticksSinceHeal / dT_hediff - 1f, 0f);
                }
            }
        }

        [SoyuzPatch(typeof(HediffComp_TendDuration), nameof(HediffComp_TendDuration.CompPostTick))]
        public static class HediffComp_TendDuration_Patch
        {
            public static void Prefix(HediffComp_TendDuration __instance)
            {
                if (__instance.parent.pawn.IsBeingThrottled() && __instance.TProps.TendIsPermanent == false)
                    __instance.tendTicksLeft -= (__instance.parent.pawn.GetTimeDelta() - 1);
            }
        }

        [SoyuzPatch(typeof(HediffComp_KillAfterDays), nameof(HediffComp_KillAfterDays.CompPostTick))]
        public static class HediffComp_HediffComp_KillAfterDays_Patch
        {
            public static void Prefix(HediffComp_KillAfterDays __instance)
            {
                if (__instance.parent.pawn.IsBeingThrottled())
                    __instance.ticksLeft -= (__instance.parent.pawn.GetTimeDelta() - 1);
            }
        }
    }
}