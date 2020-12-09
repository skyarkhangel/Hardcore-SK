using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace Analyzer.Profiling
{
    public static class InternalMethodUtility
    {
        public static HarmonyMethod InternalProfiler = new HarmonyMethod(typeof(InternalMethodUtility), nameof(InternalMethodUtility.Transpiler));

        public static HashSet<MethodInfo> PatchedInternals = new HashSet<MethodInfo>();

        public static void ClearCaches()
        {
            PatchedInternals.Clear();

#if DEBUG
            ThreadSafeLogger.Message("[Analyzer] Cleaned up the internal method caches");
#endif
        }

        public static bool IsFunctionCall(OpCode instruction)
        {
            return (instruction == OpCodes.Call || instruction == OpCodes.Callvirt);// || instruction == OpCodes.Calli);
        }

        private static IEnumerable<CodeInstruction> Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> codeInstructions)
        {
            try
            {
                List<CodeInstruction> instructions = new List<CodeInstruction>(codeInstructions);

                for (int i = 0; i < instructions.Count(); i++)
                {
                    if (!IsFunctionCall(instructions[i].opcode)) continue;
                    if (!(instructions[i].operand is MethodInfo meth)) continue;

                    // Check for constrained
                    if (i != 0 && instructions[i - 1].opcode == OpCodes.Constrained) continue;
                    // Make sure it is not an analyzer profiling method
                    if (meth.DeclaringType.FullName.Contains("Analyzer.Profiling")) continue;

                    var key = Utility.GetMethodKey(meth);
                    var index = MethodInfoCache.AddMethod(key, meth);

                    var inst = MethodTransplanting.ReplaceMethodInstruction(
                        instructions[i],
                        key,
                        GUIController.types[__originalMethod.DeclaringType + ":" + __originalMethod.Name + "-int"],
                        index);

                    instructions[i] = inst;
                }

                return instructions;
            }
            catch (Exception e)
            {

#if DEBUG
                ThreadSafeLogger.Error($"Failed to patch the internal method {__originalMethod.DeclaringType.FullName}:{__originalMethod.Name}, failed with the error " + e.Message);
#else
                if(Settings.verboseLogging)
                    ThreadSafeLogger.Warning($"Failed to patch the internal method {__originalMethod.DeclaringType.FullName}:{__originalMethod.Name}, failed with the error " + e.Message);
#endif

                return codeInstructions;
            }
        }


    }
}
