using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace CombatExtended.ExtendedLoadout;

/*
 * .method private hidebysig static 
		void SyncedAddLoadout (
			class ['Assembly-CSharp']Verse.Pawn p
		) cil managed 
	{
		.custom instance void CombatExtended.Compatibility.Multiplayer/SyncMethodAttribute::.ctor() = (
			01 00 00 00
		)
		// Method begins at RVA 0x29184
		// Header size: 12
		// Code size: 57 (0x39)
		.maxstack 2
		.locals init (
			[0] class CombatExtended.Loadout,
			[1] bool
		)

		// {
		IL_0000: nop
		// Loadout loadout = p.GenerateLoadoutFromPawn();
		IL_0001: ldarg.0
		IL_0002: call class CombatExtended.Loadout CombatExtended.Utility_Loadouts::GenerateLoadoutFromPawn(class ['Assembly-CSharp']Verse.Pawn)
		IL_0007: stloc.0
		// LoadoutManager.AddLoadout(loadout);
		IL_0008: ldloc.0
		IL_0009: call void CombatExtended.LoadoutManager::AddLoadout(class CombatExtended.Loadout)
		// p.SetLoadout(loadout);
		IL_000e: nop
		IL_000f: ldarg.0
		IL_0010: ldloc.0
		IL_0011: call void CombatExtended.Utility_Loadouts::SetLoadout(class ['Assembly-CSharp']Verse.Pawn, class CombatExtended.Loadout)
		// if (Multiplayer.IsExecutingCommandsIssuedBySelf)
		IL_0016: nop
		IL_0017: call bool CombatExtended.Compatibility.Multiplayer::get_IsExecutingCommandsIssuedBySelf()
		IL_001c: stloc.1
		IL_001d: ldloc.1
		IL_001e: brfalse.s IL_0038

		// Find.WindowStack.Add(new Dialog_ManageLoadouts(p.GetLoadout()));
		IL_0020: nop
		IL_0021: call class ['Assembly-CSharp']Verse.WindowStack ['Assembly-CSharp']Verse.Find::get_WindowStack()
		IL_0026: ldarg.0
		IL_0027: call class CombatExtended.Loadout CombatExtended.Utility_Loadouts::GetLoadout(class ['Assembly-CSharp']Verse.Pawn)
		IL_002c: newobj instance void CombatExtended.Dialog_ManageLoadouts::.ctor(class CombatExtended.Loadout)
		IL_0031: callvirt instance void ['Assembly-CSharp']Verse.WindowStack::Add(class ['Assembly-CSharp']Verse.Window)
		// (no C# code)
		IL_0036: nop
		// }
		IL_0037: nop

		IL_0038: ret
	} // end of method ITab_Inventory::SyncedAddLoadout

	ldarg.0 Verse.Pawn p
	ldloc.0 class CombatExtended.Loadout
 */
/// <summary>
/// Fix 'Make loadout' button in pawn inventory
/// </summary>
[HarmonyPatch(typeof(ITab_Inventory))]
public static class ITab_Inventory_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ITab_Inventory.SyncedAddLoadout))]
    public static IEnumerable<CodeInstruction> SyncedAddLoadout_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> list = instructions.ToList();
        MethodInfo getLoadout = AccessTools.Method(typeof(Utility_Loadouts), "GetLoadout");
        MethodInfo setLoadout = AccessTools.Method(typeof(Utility_Loadouts), "SetLoadout");
        MethodInfo ExtendedsetLoadout = AccessTools.Method(typeof(LoadoutMulti_Manager), "SetLoadout"); // takes 3 arguments Pawn p, Loadout loadout, int index
        int num = list.FindIndex((CodeInstruction c) => c.opcode == OpCodes.Call && c.operand as MethodInfo == setLoadout);
        if (num == -1)
        {
            Log.Error("Can't find SetLoadout in ITab_Inventory.SyncedAddLoadout");
            return instructions;
        }
        OpCode ldarg = list[num - 2].opcode;
        OpCode ldloc = list[num - 1].opcode;
        list.Insert(num++, new CodeInstruction(OpCodes.Ldc_I4_0));
        list[num].operand = ExtendedsetLoadout;// stack trace : index , loadout, pawn
        /*
		 * Find p.GetLoadout in Find.WindowStack.Add(new Dialog_ManageLoadouts(p.GetLoadout()))
		 *	IL_0020: nop
			IL_0021: call class ['Assembly-CSharp']Verse.WindowStack ['Assembly-CSharp']Verse.Find::get_WindowStack()
			IL_0026: ldarg.0
			IL_0027: call class CombatExtended.Loadout CombatExtended.Utility_Loadouts::GetLoadout(class ['Assembly-CSharp']Verse.Pawn)
			IL_002c: newobj instance void CombatExtended.Dialog_ManageLoadouts::.ctor(class CombatExtended.Loadout)
			IL_0031: callvirt instance void ['Assembly-CSharp']Verse.WindowStack::Add(class ['Assembly-CSharp']Verse.Window)
		 */
        int num2 = list.FindLastIndex((CodeInstruction c) => c.opcode == OpCodes.Call && c.operand as MethodInfo == getLoadout) - 1;
        if (num2 != -1)
        {
            // num2 should point to IL_0026
            List<CodeInstruction> list2 = list.GetRange(num2, 2);
            if (list2[0].opcode == OpCodes.Ldarg_0
                // Verse.Pawn
                && list2[1].Is(OpCodes.Call, getLoadout)
                // call class CombatExtended.Loadout CombatExtended.Utility_Loadouts::GetLoadout(class ['Assembly-CSharp']Verse.Pawn)
                )
            {
                list.RemoveRange(num2, 2);
                list.Insert(num2, new CodeInstruction(OpCodes.Ldloc_0));
                //File.WriteAllLines("E:/after.txt", list.Select(x => x.ToString()));
                return list;
            }
        }

        Log.Error($"Outdated transpiler ITab_Inventory.FillTab");
        return instructions;
    }
}