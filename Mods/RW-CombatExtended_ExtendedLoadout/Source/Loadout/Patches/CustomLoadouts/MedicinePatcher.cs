using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch(typeof(LoadoutGenericDef), "LoadoutGenericDef", MethodType.StaticConstructor)]
class MedicinePatcher
{
    //static AccessTools.FieldRef<LoadoutGenericDef, bool> isRunningRef =
    //    AccessTools.FieldRefAccess<LoadoutGenericDef, bool>("generic");

    /*static bool Prefix(LoadoutGenericDef __instance, ref int ___defaultCount)
    {
        isRunningRef(__instance) = true;
        if (___defaultCount > 100)
            return false;
        ___defaultCount = 5;
        return true;
    }*/

    /*static void Postfix(ref int __defaultCount)
    {
        __defaultCount *= 2;
    }*/

    /*RawFood.DefaultCount = Convert.ToInt32(Math.Floor(num / allDefs.Where((ThingDef td) => generic.lambda(td)).Average((ThingDef td) => td.ingestible.CachedNutrition)))
     *Drugs.DefaultCount = 3
     *Medicine.DefaultCount = 5
     *Ammo.DefaultCount = gun.Magazinesize;
     */

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> CtorTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        int i = 0;
        //Find all the stfld with operand = defalutCount
        List<CodeInstruction> list = instructions.ToList();
        FieldInfo operand = AccessTools.DeclaredField(typeof(LoadoutGenericDef), nameof(LoadoutGenericDef.defaultCount));
        List<int> setValueIndex = (from instruction in list
                                   where instruction.opcode == OpCodes.Stfld && instruction.operand as FieldInfo == operand
                                   orderby list.IndexOf(instruction) descending
                                   select list.IndexOf(instruction)).ToList();
        DbgLog.Msg($"list.count = {list.Count}, num in list = {String.Join(" ", setValueIndex.Select(x => x.ToString()))}");
        List<Label> labels = new List<Label>();
        i = 0;
        foreach (int no in setValueIndex)
        {
            DbgLog.Msg("Init Label");
            labels.Add(generator.DefineLabel());
            DbgLog.Msg($"Adding label {labels[i]} to list[{no}]");
            list[no].labels.Add(labels[i]);
            i++;
        }
        if (setValueIndex.NullOrEmpty())
        {
            DbgLog.Wrn($"Cannot Find Instruction: Field defaultCount in .cctor");
            return instructions;
        }
        File.WriteAllLines("E:/MediAddedlabel.txt", list.Select(x => x.ToString()));
        foreach (int value in setValueIndex)
        {
            DbgLog.Msg($"list[{value}]:{list[value].opcode.ToString() + ' ' + list[value].operand.ToString()}\nlist.count:{list.Count}");
            DbgLog.Msg($"{String.Join("\n", list.GetRange(value - 1, 6).Select(x => x.ToString()))}");
            list.InsertRange(value - 1, new[]
            {
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4_S, 100),
                new CodeInstruction(OpCodes.Blt_S, labels.GetEnumerator().Current),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldc_I4_5)
            });
            labels.GetEnumerator().MoveNext();
            DbgLog.Msg($"{String.Join("\n", list.GetRange(value - 1, 6).Select(x => x.ToString()))}");
        }
        /*for (i = 0; i < setValueIndex.Count; i++)
        {
            *//*
             * Performs this process for each defaultCount: 
             * if (the value on stack < 100) continue; else ldc.i4.5
             * So after the process the stack should look like this:
             * Whatever the value was to be assigned
             * >>dup -> used for comparing
             * >>ldc.i4.s 100
             * >>Blt.s labels[i]
             * >>pop -> get rid of the duplicated value 
             * >>ldc.i4.5
             * labels[i] stfld
             *//*
            DbgLog.Msg($"Inserting IL Code in list[{setValueIndex[i] - 1 + 5 * i}]");
            list.InsertRange(setValueIndex[i] - 1 + 5 * i, new CodeInstruction[]
            {
                //Each iteration also add 5 more instructions, so need to adjust to that
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4_S, 100),
                new CodeInstruction(OpCodes.Blt_S, labels[i]),
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldc_I4_5)
            });
            File.WriteAllLines("E:/Inserting.txt", list.Select(x => x.ToString()));
        }*/

        return list.AsEnumerable();
    }
}
