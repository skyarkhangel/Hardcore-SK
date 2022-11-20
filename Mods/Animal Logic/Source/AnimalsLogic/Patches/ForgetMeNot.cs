using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection.Emit;

namespace AnimalsLogic
{
    class ForgetMeNot
    {
        // This function is actually inlined and this patch is not working
        [HarmonyPatch(typeof(TrainableUtility), "TamenessCanDecay", new Type[] { typeof(ThingDef) })]
        static class TrainableUtility_TamenessCanDecay_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    //ldc.r4 0.101
                    if (codes[i].opcode == OpCodes.Ldc_R4) // not checking operand since method is very short
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldsfld, typeof(Settings).GetField(nameof(Settings.wildness_threshold_for_tameness_decay)));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        // this is workaround
        [HarmonyPatch(typeof(Pawn_TrainingTracker), "TrainingTrackerTickRare")]
        static class Pawn_TrainingTracker_TrainingTrackerTickRare_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return PatchTamenessDecay(instructions);
            }
        }
        [HarmonyPatch(typeof(TrainableUtility), "GetWildnessExplanation")]
        static class TrainableUtility_GetWildnessExplanation_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return PatchTamenessDecay(instructions);
            }
        }

        static IEnumerable<CodeInstruction> PatchTamenessDecay(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                //ldc.r4 0.101
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                if (codes[i].opcode == OpCodes.Call && codes[i].operand == typeof(TrainableUtility).GetMethod(nameof(TrainableUtility.TamenessCanDecay)))
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                {
                    codes[i].operand = typeof(ForgetMeNot).GetMethod(nameof(TamenessCanDecay));
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        public static bool TamenessCanDecay(ThingDef def)
        {
            if (def.race.FenceBlocked)
            {
                return false;
            }
            return def.race.wildness > Settings.wildness_threshold_for_tameness_decay;
        }

        [HarmonyPatch(typeof(TrainableUtility), "DegradationPeriodTicks", new Type[] { typeof(ThingDef) })]
        static class TrainableUtility_DegradationPeriodTicks_Patch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    //	IL_001b: mul
                    if (codes[i].opcode == OpCodes.Mul) // not checking operand since method is very short
                    {
                        codes.InsertRange(i,
                            new List<CodeInstruction>() {
                                new CodeInstruction(OpCodes.Ldsfld, typeof(Settings).GetField(nameof(Settings.training_decay_factor))),
                                new CodeInstruction(OpCodes.Div)
                            });
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}
