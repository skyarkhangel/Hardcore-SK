using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace RocketMan.Patches
{
    [RocketStartupPatch(typeof(TickManager), nameof(TickManager.DoSingleTick))]
    public static class TickManager_DoSingleTick_Patch
    {
        private static FieldInfo ftickListNormal = AccessTools.Field(typeof(TickManager), nameof(TickManager.tickListNormal));

        private static MethodBase mTickList_Tick = AccessTools.Method(typeof(TickList), nameof(TickList.Tick));

        private static MethodBase mMain_Tick = AccessTools.Method(typeof(Main), nameof(Main.Tick));

        [HarmonyPriority(int.MaxValue)]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = instructions.ToList();
            var finished = false;

            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction code = codes[i];
                if (!finished)
                {
                    if (code.opcode == OpCodes.Ldarg_0
                        && codes[i + 1].opcode == OpCodes.Ldfld
                        && codes[i + 1].OperandIs(ftickListNormal)
                        && codes[i + 2].opcode == OpCodes.Callvirt
                        && codes[i + 2].OperandIs(mTickList_Tick))
                    {
                        finished = true;
                        yield return new CodeInstruction(OpCodes.Call, mMain_Tick) { labels = code.labels };
                        code.labels = new List<Label>();
                    }
                }
                yield return code;
            }
        }
    }
}