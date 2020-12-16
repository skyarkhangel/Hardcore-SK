using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.Profiling
{
    public static class MethodInfoCache
    {
        private static Dictionary<string, int> nameToKey = new Dictionary<string, int>();
        private static List<MethodInfo[]> internalArrays = new List<MethodInfo[]>();

        private static int currentIndex = 0;
        private static object indexLock = new object();

        public static FieldInfo internalArray = AccessTools.Field(typeof(MethodInfoCache), nameof(internalArrays));
        public static MethodInfo accessList = AccessTools.Method(typeof(List<MethodInfo[]>), "get_Item");

        static MethodInfoCache()
        {
            internalArrays.Add(new MethodInfo[0x80]);
        }

        public static MethodInfo Get(int index)
        {
            // basically, [index / 128][index % 128]
            return internalArrays[index >> 7][index & 0x7f];
        }

        public static List<CodeInstruction> GetInlineIL(int index)
        {
            return new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldsfld, internalArray),
                new CodeInstruction(OpCodes.Ldc_I4, index),
                new CodeInstruction(OpCodes.Ldc_I4_7),
                new CodeInstruction(OpCodes.Shr),
                new CodeInstruction(OpCodes.Callvirt, accessList),
                new CodeInstruction(OpCodes.Ldc_I4, index),
                new CodeInstruction(OpCodes.Ldc_I4, 127),
                new CodeInstruction(OpCodes.And),
                new CodeInstruction(OpCodes.Ldelem_Ref)
            };
        }

        public static int AddMethod(string key, MethodInfo method)
        {
            int index = 0;

            lock (indexLock)
            {
                if (nameToKey.TryGetValue(key, out var i))
                {
                    return i;
                }

                index = currentIndex++;

                if ((index & 0x7f) == 127)
                {
                    internalArrays.Add(new MethodInfo[0x80]);
#if DEBUG
                    ThreadSafeLogger.Message("[Analyzer] Adding new internal array to the MethodInfoCache");
#endif
                }

                nameToKey.Add(key, index);
            }

            internalArrays[index >> 7][index & 0x7f] = method;

            return index;
        }

        public static void ClearCache()
        {
            lock (indexLock)
            {
                currentIndex = 0;

                internalArrays.Clear();
                nameToKey.Clear();
            }
            internalArrays.Add(new MethodInfo[0x80]);
#if DEBUG
            ThreadSafeLogger.Message("[Analyzer] Cleaned up the MethodInfoCache");
#endif
        }
    }
}

