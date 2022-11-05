using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace RocketMan
{
    public static class TranspilerUtility
    {
        private static readonly Vector2 one2 = Vector2.one;
        private static readonly Vector3 one3 = Vector3.one;
        private static readonly Vector2 zero2 = Vector2.zero;
        private static readonly Vector3 zero3 = Vector3.zero;

        private static readonly MethodInfo pOne3 = AccessTools.PropertyGetter(typeof(Vector3), nameof(Vector3.one));
        private static readonly MethodInfo pOne2 = AccessTools.PropertyGetter(typeof(Vector2), nameof(Vector2.one));
        private static readonly MethodInfo pZero3 = AccessTools.PropertyGetter(typeof(Vector3), nameof(Vector3.zero));
        private static readonly MethodInfo pZero2 = AccessTools.PropertyGetter(typeof(Vector2), nameof(Vector2.zero));

        private static readonly Quaternion identity = Quaternion.identity;

        private static readonly MethodInfo pIdentity =
            AccessTools.PropertyGetter(typeof(Quaternion), nameof(Quaternion.identity));

        public static IEnumerable<CodeInstruction> FixConsts(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].OperandIs(pZero3))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(TranspilerUtility), "zero3")) {labels = codes[i].labels};
                    continue;
                }

                if (codes[i].OperandIs(pZero2))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(TranspilerUtility), "zero2")) {labels = codes[i].labels};
                    continue;
                }

                if (codes[i].OperandIs(pOne3))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(TranspilerUtility), "one3")) {labels = codes[i].labels};
                    continue;
                }

                if (codes[i].OperandIs(pOne2))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(TranspilerUtility), "one2")) {labels = codes[i].labels};
                    continue;
                }

                if (codes[i].OperandIs(pIdentity))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld,
                        AccessTools.Field(typeof(TranspilerUtility), "identity")) {labels = codes[i].labels};
                    continue;
                }

                yield return codes[i];
            }
        }
    }
}