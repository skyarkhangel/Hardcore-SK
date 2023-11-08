using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout;

/// <summary>
/// Add HP and Quality validator for PickUp weapons
///
/// Versions for MultiLoadout and Standart
/// </summary>
[HarmonyPatch]
public class JobGiver_UpdateLoadout_FindPickup_LambdaValidator_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useHpAndQualityInLoadouts;

    #region temp
    static System.Type innerType = AccessTools.Inner(typeof(JobGiver_UpdateLoadout), "<>c__DisplayClass8_0");
    #endregion

    [UsedImplicitly]
    public static MethodBase TargetMethod()
    {
        string version = Assembly.GetAssembly(typeof(JobGiver_UpdateLoadout)).GetName().Version.ToString();
        Log.Message("Combat Extended ver " + version + " Loaded");
        if (version == "1.1.2.0")
            innerType = AccessTools.Inner(typeof(JobGiver_UpdateLoadout), "<>c__DisplayClass9_0");
        return AccessTools.Method(innerType, "<FindPickup>b__3");
    }

    [HarmonyTranspiler]
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> FindPickup_Validator_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
    {
        var code = instructions.ToList();
        /*
          IL_000e:  ldarg.1
          IL_000f:  ldarg.0
          IL_0010:  ldfld      class ['Assembly-CSharp']Verse.Pawn CombatExtended.JobGiver_UpdateLoadout/'<>c__DisplayClass6_0'::pawn
          IL_0015:  call       bool ['Assembly-CSharp']RimWorld.ForbidUtility::IsForbidden(class ['Assembly-CSharp']Verse.Thing, class ['Assembly-CSharp']Verse.Pawn)
          IL_001a:  brtrue.s   IL_0043
         */
        var isForbidden = AccessTools.Method(typeof(ForbidUtility), nameof(ForbidUtility.IsForbidden), new[] { typeof(Thing), typeof(Pawn) });
        var thisPawn = AccessTools.Field(innerType, "pawn");
        var idx = code.FindIndex(ci => ci.Calls(isForbidden));
        if (idx == -1)
        {
            Log.Error($"Can't find IsForbidden in ce findPickup_validator");
            return code;
        }

        idx += 2; // after brtrue.s

        code.InsertRange(idx, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, thisPawn), // pawn
            new CodeInstruction(OpCodes.Ldarg_1), // thing
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(JobGiver_UpdateLoadout_FindPickup_LambdaValidator_Patch), nameof(AllowEquip))),
            new CodeInstruction(OpCodes.Brfalse_S, code[idx - 1].operand), // read exit label from brtrue.s
        });
#if DEBUG
        File.WriteAllLines("E:\\Validatorbefore.txt", instructions.Select(x => x.ToString()));
        File.WriteAllLines("E:\\Validatorafter.txt", code.Select(x => x.ToString()));
#endif
        return code;
    }

    public static bool AllowEquip(Pawn p, Thing t)
    {
        if (!t.def.IsWeapon)
        {
            return true;
        }

        var loadout = p.GetLoadout();
        if (loadout == null)
        {
            return true;
        }

        if (loadout is Loadout_Multi loadoutMulti)
        {
            loadout = loadoutMulti.FindLoadoutWithThingDef(t.def);
            if (loadout == null)
            {
                return true;
            }
        }

        return loadout.Extended().Allows(t);
    }
}