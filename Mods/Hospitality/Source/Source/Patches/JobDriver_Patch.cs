using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse.AI;

namespace Hospitality.Patches
{
    /// <summary>
    /// Works together with ForbidUtility_Patch to prevent guests from forbidding items during work
    /// </summary>
    public class JobDriver_Patch
    {
        [HarmonyPatch(typeof(JobDriver), nameof(JobDriver.DriverTick))]
        public class DriverTick
        {
            private static readonly FieldInfo pawnField = AccessTools.Field(typeof(JobDriver), nameof(JobDriver.pawn));
            private static readonly FieldInfo ourTransplant = AccessTools.Field(typeof(ForbidUtility_Patch), nameof(ForbidUtility_Patch.currentToilWorker));

            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, pawnField);
                yield return new CodeInstruction(OpCodes.Stsfld, ourTransplant);

                foreach(var inst in insts)
                {
                    yield return inst;
                }
            }
        }
    }
}