using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using System.IO;
using System.Linq;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch]
public class Dialog_ManageLoadouts_Patch
{
    [HarmonyTargetMethod]
    public static IEnumerable<MethodBase> TargetMethods()
    {
        // 'Make loadout' button in Pawn inventory. Disabled as it's literally a one-use method
        //yield return AccessTools.Method(typeof(ITab_Inventory), nameof(ITab_Inventory.SyncedAddLoadout));
        // 'Manage loadouts' button in Assign tab
        yield return AccessTools.Method(typeof(PawnColumnWorker_Loadout), nameof(PawnColumnWorker_Loadout.DoHeader));
        // method replaced by PawnColumnWorker_Loadout_Multi
        yield return AccessTools.Method(typeof(PawnColumnWorker_Loadout), nameof(PawnColumnWorker_Loadout.DoCell));
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> CtorReplacer_Transpiler(MethodBase __originalMethod, IEnumerable<CodeInstruction> instructions)
    {
        //File.WriteAllText("E:\\CrotReplacerBefore.txt", string.Concat(instructions.Select(x=>x.ToString())));
        var dialog_ManageLoadoutsCtor = AccessTools.Constructor(typeof(Dialog_ManageLoadouts), new[] { typeof(Loadout) });
        var dialog_ManageLoadouts_ExtendedCtor = AccessTools.Constructor(typeof(Dialog_ManageLoadouts_Extended), new[] { typeof(Loadout) });

        int i = 0;
        foreach (var ci in instructions)
        {
#pragma warning disable 252,253
            if (ci.opcode == OpCodes.Newobj && ci.operand == dialog_ManageLoadoutsCtor)
#pragma warning restore 252,253
            {
                yield return new CodeInstruction(OpCodes.Newobj, dialog_ManageLoadouts_ExtendedCtor);
                i++;
                DbgLog.Msg($"Dialog_ManageLoadouts Constructor replaced in method: {__originalMethod.DeclaringType.FullName}:{__originalMethod.Name}");
            }
            else
            {
                yield return ci;
            }
        }

        if (i == 0)
        {
            Log.Error($"Can't find Dialog_ManageLoadouts Constructor in method: {__originalMethod.DeclaringType.FullName}:{__originalMethod.Name}");
        }
    }
}