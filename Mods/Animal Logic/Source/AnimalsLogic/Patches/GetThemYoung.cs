using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using System.Reflection.Emit;

namespace AnimalsLogic.Patches
{
    /**
     * Ties animal wildness toanimal age, making taming and training young animals much easier.
     * 
     * TODO: make animals remember trainer and build up rapport with them.
     */
    class GetThemYoung
    {
        public static void Patch()
        {
            AnimalsLogic.harmony.Patch(
                typeof(InteractionWorker_RecruitAttempt).GetMethod("Interacted"),
                transpiler: new HarmonyMethod(typeof(GetThemYoung).GetMethod(nameof(Interacted_Transpiler)))
                );

            // TODO: the same but method takes TargetIndex as target and needs a bit different code for that
            //AnimalsLogic.harmony.Patch(
            //    typeof(Toils_Interpersonal).GetMethod("TryTrain"),
            //    transpiler: new HarmonyMethod(typeof(GetThemYoung).GetMethod(nameof(TryTrain_Transpiler)))
            //    );
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Interacted_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                // ldfld float32 Verse.RaceProperties::wildness
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                if (codes[i].opcode == OpCodes.Ldfld && codes[i].operand == typeof(RaceProperties).GetField("wildness"))
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                {
                    codes.InsertRange(i + 1,
                        new List<CodeInstruction>() {
                                new CodeInstruction(OpCodes.Ldarg_2), // put recipient Pawn on stack
                                new CodeInstruction(OpCodes.Call, typeof(GetThemYoung).GetMethod(nameof(WildnessFactor)))
                        });
                    break;
                }
            }

            return codes.AsEnumerable();
        }

        public static float WildnessFactor(float wildness, Pawn recipient)
        {
            if (!Settings.taming_age_factor)
                return wildness;

            LifeStageAge matureAge = recipient?.def?.race?.lifeStageAges?.FirstOrFallback(
                    p => p.def.reproductive || p.def.milkable || p.def.shearable,
                    recipient.def.race.lifeStageAges.Last()
                    );

            if (matureAge == null)
                return wildness;

            float ageFactor = Math.Min(recipient.ageTracker.AgeBiologicalYearsFloat / matureAge.minAge, 1);
            ageFactor *= (float) Math.Pow(ageFactor, 0.33f);
            return wildness * ageFactor;
        }
    }
}
