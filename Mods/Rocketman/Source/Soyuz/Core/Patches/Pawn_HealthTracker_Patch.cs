using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RocketMan;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HealthTick))]
    public class Pawn_HealthTracker_Tick_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            return instructions.MethodReplacer(
                AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) }),
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval)));
        }
    }

    //[SoyuzPatch(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.ExposeData))]
    //public class Pawn_HealthTracker_ExposeData_Patch
    //{
    //    public static void Postfix(Pawn_HealthTracker __instance)
    //    {
    //        //var tracker = __instance.pawn.GetHediffTracker();
    //        //if (tracker != null)
    //        //{
    //        //    tracker.ExposeData();
    //        //}
    //    }
    //}
    //
    //[SoyuzPatch]
    //public class Pawn_HealthTracker_AddHediff_Patch
    //{
    //    public static IEnumerable<MethodBase> TargetMethods()
    //    {
    //        yield return AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(HediffDef), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) });
    //        yield return AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) });
    //    }

    //    public static void Postfix(Pawn_HealthTracker __instance)
    //    {
    //        if (Finder.timeDilationCriticalHediffs) return;

    //        if (__instance.hediffSet.HasHediff(HediffDefOf.Pregnant))
    //        {
    //            __instance.pawn.GetHediffTracker().Pregnant = true;
    //            if (RocketDebugPrefs.debug) Log.Message(string.Format("Pawn is pregnant: {0}", __instance.pawn));
    //        }
    //    }
    //}
    //
    //[SoyuzPatch]
    //public class Pawn_HealthTracker_RemoveHediff_Patch
    //{
    //    public static IEnumerable<MethodBase> TargetMethods()
    //    {
    //        yield return AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.RemoveHediff));
    //    }

    //    public static void Prefix(Pawn_HealthTracker __instance, Hediff hediff)
    //    {
    //        if (Finder.timeDilationCriticalHediffs) return;

    //        if (hediff.def == HediffDefOf.Pregnant)
    //        {
    //            __instance.pawn.GetHediffTracker().Pregnant = false;
    //            if (RocketDebugPrefs.debug) Log.Message(string.Format("Pawn is not pregnant anymore: {0}", __instance.pawn));
    //        }
    //    }
    //}
}