using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    [HarmonyPatch]
    internal static class Room_Owners_Transpiler
    {
        public const string targetMethodName = "get_Owners";
        public static MethodBase TargetMethod()
        {
            var classType = typeof(Room).GetNestedTypes(AccessTools.all).First(x => x.Name.Contains(targetMethodName));
            return AccessTools.DeclaredMethod(classType, "MoveNext");
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            for (var i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                if (i > 1 && codes[i - 1].LoadsField(AccessTools.Field(typeof(RoomRoleDefOf), "Bedroom")) && codes[i].opcode == OpCodes.Beq_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Room), "get_Role"));
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(InternalDefOf), nameof(InternalDefOf.GuestRoom)));
                    yield return new CodeInstruction(OpCodes.Beq_S, codes[i].operand);
                }
            }
        }
    }
}
