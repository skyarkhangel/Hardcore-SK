using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace RocketMan.Optimizations
{
    [RocketPatch(typeof(PlayLog), nameof(PlayLog.ReduceToCapacity))]
    public class PlayLog_ReduceToCapacity_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var code in instructions)
            {
                if (code.OperandIs(150))
                    code.operand = 75;
                yield return code;
            }
        }
    }
}