using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RocketMan;
using Verse;
using Verse.AI;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(JobDriver), nameof(JobDriver.DriverTick))]
    public class JobDriver_DriverTick_Patch
    {
        //private static int deltaT = 0;

        //private static float a1 = 0f;
        //private static float a2 = 0f;
        //private static float a3 = 0f;

        //private static float b1 = 0f;
        //private static float b2 = 0f;
        //private static float b3 = 0f;

        //public static void Prefix(JobDriver __instance)
        //{
        //    if (Context.PartiallyDilatedContext || !RocketPrefs.Enabled || !RocketPrefs.TimeDilation || (deltaT = __instance.pawn.GetTimeDelta()) <= 1 || __instance.pawn != ContextualExtensions.Current)
        //    {
        //        return;
        //    }
        //    if (__instance is JobDriver_CleanFilth clean)
        //    {
        //        a1 = clean.cleaningWorkDone;
        //        a2 = clean.totalCleaningWorkDone;
        //    }
        //    else if (__instance is JobDriver_ClearSnow clearSnow)
        //    {
        //        a1 = clearSnow.workDone;
        //    }
        //    else if (__instance is JobDriver_PlantSow sow)
        //    {
        //        a1 = sow.sowWorkDone;
        //    }
        //    else if (__instance is JobDriver_PlantCut cut)
        //    {
        //        a1 = cut.workDone;
        //    }
        //    else if (__instance is JobDriver_PlantHarvest harvest)
        //    {
        //        a1 = harvest.workDone;
        //    }
        //    else if (__instance is JobDriver_PlantWork plantWork)
        //    {
        //        a1 = plantWork.workDone;
        //    }
        //    else if (__instance is JobDriver_PlantHarvest_Designated harvest_Designated)
        //    {
        //        a1 = harvest_Designated.workDone;
        //    }
        //    else if (__instance is JobDriver_PlantCut_Designated cut_Designated)
        //    {
        //        a1 = cut_Designated.workDone;
        //    }

        //    a3 = __instance.uninstallWorkLeft;
        //}

        //public static void Postfix(JobDriver __instance)
        //{
        //    if (Context.PartiallyDilatedContext || !RocketPrefs.Enabled || !RocketPrefs.TimeDilation || deltaT <= 1 || __instance.pawn != ContextualExtensions.Current)
        //    {
        //        return;
        //    }
        //    if (__instance is JobDriver_CleanFilth clean)
        //    {
        //        b1 = clean.cleaningWorkDone;
        //        b2 = clean.totalCleaningWorkDone; ;

        //        clean.cleaningWorkDone += (b1 - a1) * (deltaT - 1);
        //        clean.totalCleaningWorkDone += (b2 - a2) * (deltaT - 1);
        //    }
        //    else if (__instance is JobDriver_ClearSnow clearSnow)
        //    {
        //        b1 = clearSnow.workDone;

        //        clearSnow.workDone += (b1 - a1) * (deltaT - 1);
        //    }
        //    else if (__instance is JobDriver_PlantSow sow)
        //    {
        //        b1 = sow.sowWorkDone;

        //        sow.sowWorkDone += (b1 - a1) * (deltaT - 1);
        //    }
        //    else if (__instance is JobDriver_PlantCut cut)
        //    {
        //        b1 = cut.workDone;

        //        cut.workDone += (b1 - a1) * (deltaT - 1);
        //    }
        //    else if (__instance is JobDriver_PlantHarvest harvest)
        //    {
        //        b1 = harvest.workDone;

        //        harvest.workDone += (b1 - a1) * (deltaT - 1);
        //    }
        //    else if (__instance is JobDriver_PlantWork plantWork)
        //    {
        //        b1 = plantWork.workDone;

        //        plantWork.workDone += (b1 - a1) * (deltaT - 1);
        //    }
        //    else if (__instance is JobDriver_PlantHarvest_Designated harvest_Designated)
        //    {
        //        b1 = harvest_Designated.workDone;

        //        harvest_Designated.workDone += (b1 - a1) * (deltaT - 1);
        //    }
        //    else if (__instance is JobDriver_PlantCut_Designated cut_Designated)
        //    {
        //        b1 = cut_Designated.workDone;

        //        cut_Designated.workDone += (b1 - a1) * (deltaT - 1);
        //    }

        //    b3 = __instance.uninstallWorkLeft;

        //    __instance.uninstallWorkLeft += (b1 - a1) * (deltaT - 1);
        //}

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator generator)
        {
            var codes = instructions.MethodReplacer(
                AccessTools.Method(typeof(Gen), nameof(Gen.IsHashIntervalTick), new[] { typeof(Thing), typeof(int) }),
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsCustomTickInterval))).ToList();
            var l1 = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn)));
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsBeingThrottled)));
            yield return new CodeInstruction(OpCodes.Brfalse_S, l1);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(JobDriver), nameof(JobDriver.ticksLeftThisToil)));

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn)));
            yield return new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.GetTimeDelta)));
            yield return new CodeInstruction(OpCodes.Ldc_I4, 1);
            yield return new CodeInstruction(OpCodes.Sub);

            yield return new CodeInstruction(OpCodes.Sub);
            yield return new CodeInstruction(OpCodes.Stfld,
                AccessTools.Field(typeof(JobDriver), nameof(JobDriver.ticksLeftThisToil)));

            codes[0].labels.Add(l1);
            foreach (var code in codes)
                yield return code;
        }
    }
}