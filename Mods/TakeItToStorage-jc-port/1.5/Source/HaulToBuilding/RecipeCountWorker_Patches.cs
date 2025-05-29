using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HaulToBuilding;

public class RecipeCountWorker_Patches
{
    public static void DoPatches(Harmony harm)
    {
        harm.Patch(AccessTools.Method(typeof(RecipeWorkerCounter), "CountProducts"),
            transpiler: new HarmonyMethod(typeof(RecipeCountWorker_Patches), nameof(Transpiler)));
        if (ModLister.HasActiveModWithName("Better Workbench Management"))
        {
            harm.Patch(
                AccessTools.Method(AccessTools.TypeByName("ImprovedWorkbenches.RecipeWorkerCounter_CountProducts_Detour"), "CountAdditionalProducts"),
                transpiler: new HarmonyMethod(typeof(RecipeCountWorker_Patches), nameof(Transpiler)));
            harm.Patch(
                AccessTools.Method(AccessTools.TypeByName("ImprovedWorkbenches.RecipeWorkerCounter_CountProducts_Detour"), "Postfix"),
                transpiler: new HarmonyMethod(typeof(RecipeCountWorker_Patches), nameof(Transpiler_Postfix)));
        }
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator, MethodBase original)
    {
        var isCore = original.DeclaringType == typeof(RecipeWorkerCounter);
        var list = instructions.ToList();
        // var info1 = AccessTools.Field(typeof(Bill_Production), "includeGroup");
        var info1 =  AccessTools.Method(typeof(Bill_Production), "GetIncludeSlotGroup");
        var idx1 = list.FindIndex(ins => ins.Calls(info1));
        
        var label1 = (Label)list[idx1 + 1].operand;
        list.InsertRange(idx1 + 2, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RecipeCountWorker_Patches), nameof(HasBuilding))),
            new CodeInstruction(OpCodes.Brtrue, label1)
        });
        var idx2 = list.FindIndex(idx1 + 1, ins => ins.Calls(info1));

        Label? label2 = null;
        var idx2b = list.FindIndex(idx2, ins => ins.Branches(out label2));
        if(label2 == null) {
            Log.Error($"Failed to find branch label after get include slot group");
            return list;
        }

        // var label2 = (Label)list[idx2 + 1].operand;
        var idx3 = list.FindIndex(ins => ins.labels.Contains((Label)label2));
        var ins = list[idx3 - (isCore ? 1 : 5)];
        if (ins.opcode != (isCore ? OpCodes.Br_S : OpCodes.Leave_S))
            throw new Exception("Unexpected instruction when searching for label");
        var label3 = (Label)ins.operand;
        list.InsertRange(idx2 + 2, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_1),
            isCore ? new CodeInstruction(OpCodes.Ldloc_1) : new CodeInstruction(OpCodes.Ldloc_S, 5),
            new CodeInstruction(OpCodes.Ldloca_S, isCore ? 2 : 3),
            new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(RecipeCountWorker_Patches), nameof(GetContentsOfBuilding))),
            new CodeInstruction(OpCodes.Brtrue, label3)
        });
        return list;
    }

    public static IEnumerable<CodeInstruction> Transpiler_Postfix(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var list = instructions.ToList();
        var info1 =  AccessTools.Method(typeof(Bill_Production), "GetIncludeSlotGroup");
        var idx1 = list.FindIndex(ins => ins.Calls(info1));
        var label1 = (Label)list[idx1 + 1].operand;
        list.InsertRange(idx1 + 2, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_2),
            new CodeInstruction(OpCodes.Ldind_Ref),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RecipeCountWorker_Patches), nameof(HasBuilding))),
            new CodeInstruction(OpCodes.Brtrue, label1)
        });
        return list;
    }

    public static bool HasBuilding(Bill_Production bill) => GameComponent_ExtraBillData.Instance.GetData(bill).LookInStorage != null;

    public static bool GetContentsOfBuilding(RecipeWorkerCounter counter, Bill_Production bill, ThingDef def, ref int num)
    {
        var storage = GameComponent_ExtraBillData.Instance.GetData(bill).LookInStorage;
        if (storage == null) return false;
        num += storage.slotGroup.HeldThings
           .Where(outerThing => counter.CountValidThing(outerThing.GetInnerIfMinified(), bill, def))
           .Sum(outerThing => outerThing.GetInnerIfMinified().stackCount);
        return true;
    }
}
