using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Analyzer.Profiling
{
    public class CodeInstMethEqual : EqualityComparer<CodeInstruction>
    {
        // Functions primarily to check if two function call CodeInstructions are the same. 
        public override bool Equals(CodeInstruction b1, CodeInstruction b2)
        {
            if (b1.opcode != b2.opcode) return false;

            if (InternalMethodUtility.IsFunctionCall(b1.opcode))
            {
                try
                {
                    return ((MethodInfo)b1.operand).Name == ((MethodInfo)b2.operand).Name;
                }
                catch { }
            }

            return b1.operand == b2.operand;
        }

        public override int GetHashCode(CodeInstruction obj)
        {
            return obj.GetHashCode();
        }
    }


    public static class TranspilerMethodUtility
    {
        public static HarmonyMethod TranspilerProfiler = new HarmonyMethod(typeof(TranspilerMethodUtility), nameof(TranspilerMethodUtility.Transpiler));

        public static List<MethodBase> PatchedMeths = new List<MethodBase>();
        public static CodeInstMethEqual methComparer = new CodeInstMethEqual();

        // Clear the caches which prevent double patching
        public static void ClearCaches()
        {
            PatchedMeths.Clear();
#if DEBUG
            ThreadSafeLogger.Message("[Analyzer] Cleaned up the transpiler methods caches");
#endif
        }

        /* This method takes a method, and computes the different in CodeInstructions,
         * from this difference, it then looks at all 'added' instructions that are either
         * `Call` or `CallVirt` instructions, and attempts to swap the MethodInfo which
         * is called from the instruction, the method it gets switched to is created at 
         * runtime in `MethodTransplating.ReplaceMethodInstruction`
         */

        [HarmonyPriority(Priority.Last)]
        private static IEnumerable<CodeInstruction> Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> inst = PatchProcessor.GetOriginalInstructions(__originalMethod);
            List<CodeInstruction> modInstList = instructions.ToList();

            Myers<CodeInstruction> insts = new Myers<CodeInstruction>(inst.ToArray(), modInstList.ToArray(), methComparer);
            insts.Compute();

            var key = Utility.GetMethodKey(__originalMethod as MethodInfo);
            var index = MethodInfoCache.AddMethod(key, __originalMethod as MethodInfo);

            foreach (var thing in insts.changeSet)
            {
                // We only want added methods
                if (thing.change != ChangeType.Added) continue;
                if (!InternalMethodUtility.IsFunctionCall(thing.value.opcode) || !(thing.value.operand is MethodInfo meth)) continue;

                // swap our instruction
                var replaceInstruction = MethodTransplanting.ReplaceMethodInstruction(
                    thing.value,
                    key,
                    typeof(H_HarmonyTranspilers),
                    index);

                // Find the place it was in our method, and replace the instruction (Optimisation Opportunity to improve this)
                for (int i = 0; i < modInstList.Count; i++)
                {
                    var instruction = modInstList[i];
                    if (!InternalMethodUtility.IsFunctionCall(instruction.opcode)) continue;
                    if (!(instruction.operand is MethodInfo info) || info.Name != meth.Name) continue;


                    if (instruction != replaceInstruction) modInstList[i] = replaceInstruction;
                    break;
                }
            }
    
            return modInstList;
        }


    }
}
