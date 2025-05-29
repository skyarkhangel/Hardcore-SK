using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace HaulToBuilding;

internal class Toils_Recipe_Patches
{
    public static void DoPatches(Harmony harm)
    {
        try
        {
            harm.Patch(AccessTools.Method(AccessTools.Inner(typeof(Toils_Recipe), "<>c__DisplayClass3_0"), "<FinishRecipeAndStartStoringProduct>b__1"),
                transpiler: new HarmonyMethod(typeof(Toils_Recipe_Patches), nameof(Transpiler)));
        }
        catch (Exception e)
        {
            Log.Error(
                $"Got error while patching Toils_Recipe.<>c_DisplayClass3_0.<FinishRecipeAndStartStoringProduct>b__1: {e.Message}\n{e.StackTrace}");
        }

        harm.Patch(AccessTools.Method(typeof(Toils_Recipe), nameof(Toils_Recipe.FinishRecipeAndStartStoringProduct)),
            postfix: new HarmonyMethod(typeof(Toils_Recipe_Patches), nameof(AddDebugInfo)));
    }

    public static void AddDebugInfo(Toil __result)
    {
        __result.AddPreInitAction(() => { Log.Message($"storeMode={__result.actor.CurJob.bill.GetStoreMode()}"); });
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        var list = instructions.ToList();
        var info1 = AccessTools.Field(typeof(BillStoreModeDefOf), "SpecificStockpile");
        var idx = list.FindIndex(ins => ins.LoadsField(info1));
        var label1 = (Label)list[idx + 1].operand;
        var idx2 = list.FindIndex(ins => ins.labels.Contains(label1));
        var label2 = (Label)list[idx2 - 1].operand;
        list[idx2].labels.Remove(label1);
        var list2 = new[]
        {
            new CodeInstruction(OpCodes.Ldloc_0).WithLabels(label1),
            new CodeInstruction(OpCodes.Ldloc_S, 6),
            new CodeInstruction(OpCodes.Ldloca_S, 9),
            new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(Toils_Recipe_Patches), "FindCell")),
            new CodeInstruction(OpCodes.Brtrue_S, label2)
        };
        list.InsertRange(idx2, list2);
        return list;
    }

    public static bool FindCell(Pawn pawn, List<Thing> things, ref IntVec3 cell)
    {
        if (pawn.CurJob.bill.GetStoreMode() == HaulToBuildingDefOf.StorageBuilding)
        {
            StoreUtility.TryFindBestBetterStoreCellForIn(things[0], pawn, pawn.Map, 0, pawn.Faction,
                GameComponent_ExtraBillData.Instance.GetData(pawn.CurJob.bill).Storage.GetSlotGroup(), out cell);
            return true;
        }

        if (pawn.CurJob.bill.GetStoreMode() == HaulToBuildingDefOf.Nearest)
        {
            foreach (var slotGroup in pawn.Map.haulDestinationManager.AllGroupsListForReading.Where(group => group.parent.Accepts(things[0]))
                        .OrderBy(group => group.CellsList.Any() ? group.CellsList.Select(c => c.DistanceToSquared(pawn.Position)).Min() : float.MaxValue))
                if (StoreUtility.TryFindBestBetterStoreCellForIn(things[0], pawn, pawn.Map, 0, pawn.Faction,
                        slotGroup, out cell))
                    break;
            return true;
        }

        return false;
    }
}
