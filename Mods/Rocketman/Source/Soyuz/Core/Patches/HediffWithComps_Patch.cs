using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Soyuz.Patches
{
    // [SoyuzPatch(typeof(HediffWithComps), nameof(HediffWithComps.PostTick))]
    // public static class HediffWithComps_PostTick_Patch
    // {
    //     public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
    //         ILGenerator generator)
    //     {
    //         var codes = instructions.ToList();
    //         var finished = false;
    //     
    //         var loc1 = generator.DeclareLocal(typeof(bool));
    //     
    //         var l1 = generator.DefineLabel();
    //         var l2 = generator.DefineLabel();
    //     
    //         yield return new CodeInstruction(OpCodes.Ldc_I4_0);
    //         yield return new CodeInstruction(OpCodes.Stloc_S, loc1.LocalIndex);
    //     
    //         yield return new CodeInstruction(OpCodes.Ldarg_0);
    //         yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Hediff), nameof(Hediff.pawn)));
    //         yield return new CodeInstruction(OpCodes.Call,
    //             AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsValidWildlifeOrWorldPawn)));
    //         yield return new CodeInstruction(OpCodes.Brfalse_S, l1);
    //     
    //         yield return new CodeInstruction(OpCodes.Ldarg_0);
    //         yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Hediff), nameof(Hediff.pawn)));
    //         yield return new CodeInstruction(OpCodes.Call,
    //             AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.IsSkippingTicks)));
    //         yield return new CodeInstruction(OpCodes.Brfalse_S, l1);
    //     
    //         yield return new CodeInstruction(OpCodes.Ldc_I4_1);
    //         yield return new CodeInstruction(OpCodes.Stloc_S, loc1.LocalIndex);
    //     
    //         codes[0].labels = new List<Label> {l1};
    //         for (var i = 0; i < codes.Count; i++)
    //         {
    //             if (!finished)
    //                 if (codes[i + 4].opcode == OpCodes.Ldloc_0 && codes[i + 5].opcode == OpCodes.Add)
    //                 {
    //                     finished = true;
    //                     yield return codes[i];
    //     
    //                     yield return new CodeInstruction(OpCodes.Ldloc_S, loc1.LocalIndex);
    //                     yield return new CodeInstruction(OpCodes.Brfalse_S, l2);
    //     
    //                     yield return new CodeInstruction(OpCodes.Ldloc_0);
    //                     yield return new CodeInstruction(OpCodes.Ldarg_0);
    //                     yield return new CodeInstruction(OpCodes.Ldfld,
    //                         AccessTools.Field(typeof(Hediff), nameof(Hediff.pawn)));
    //                     yield return new CodeInstruction(OpCodes.Call,
    //                         AccessTools.Method(typeof(ContextualExtensions), nameof(ContextualExtensions.GetDeltaT)));
    //                     yield return new CodeInstruction(OpCodes.Conv_R4);
    //                     yield return new CodeInstruction(OpCodes.Mul);
    //                     yield return new CodeInstruction(OpCodes.Stloc_0);
    //     
    //                     codes[i + 1].labels.Add(l2);
    //                     continue;
    //                 }
    //     
    //             yield return codes[i];
    //         }
    //     }
    // }
}