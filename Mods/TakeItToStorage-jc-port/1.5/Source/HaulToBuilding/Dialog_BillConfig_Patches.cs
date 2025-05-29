using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace HaulToBuilding;

[StaticConstructorOnStartup]
public class Dialog_BillConfig_Patches
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    // ReSharper disable once ConvertToConstant.Local
    // ReSharper disable once InconsistentNaming
    [TweakValue("Interface", 0, 400)] public static int TakeFromSubdialogHeight = 30;

    public static void DoPatches(Harmony harm)
    {
        Log.Message("HaulToBuilding: Dialog_BillConfig_Patches.DoPatches called");

        Utils.TryPatch(harm,
            AccessTools.Method(typeof(Dialog_BillConfig), "DoWindowContents"),
            new HarmonyMethod(typeof(Dialog_BillConfig_Patches), "Transpile"));

        Utils.TryPatch(harm,
            AccessTools.Method(typeof(Bill_Production), nameof(Bill_Production.DoConfigInterface)),
            new HarmonyMethod(typeof(Dialog_BillConfig_Patches), nameof(ConfigTranspile)));
 
         if (ModLister.HasActiveModWithName("Math! (Forked)")) {
            DoMathPatches(harm);
         }


        Dialog_BillConfig.RepeatModeSubdialogHeight = 280; // Make room for new Button
    }

    public static IEnumerable<CodeInstruction> ConfigTranspile(IEnumerable<CodeInstruction> instructions)
    {
        var ctor = AccessTools.Constructor(typeof(WidgetRow), new[] { typeof(float), typeof(float), typeof(UIDirection), typeof(float), typeof(float) });
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (instruction.opcode == OpCodes.Newobj && instruction.OperandIs(ctor))
            {
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(Dialog_BillConfig_Patches), nameof(DoDesButton));
            }
        }
    }

    public static void DoDesButton(WidgetRow row, Bill_Production bill)
    {
        if (HaulToBuildingMod.Settings.DoDes && row.ButtonIcon(TexStorage.SelectStorage, "HaulToBuilding.Des".Translate()))
        {
            Find.Selector.ClearSelection();
            Find.DesignatorManager.Select(new Designator_Storage(bill));
        }
    }

    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        bool debug = true;
        
        var list = instructions.ToList();

        // First replace operation
        var info1 = AccessTools.Field(typeof(Dialog_BillConfig), "StoreModeSubdialogHeight");
        var info5 = AccessTools.Method(typeof(Listing_Standard), "EndSection");
        var info4 = AccessTools.Method(typeof(Dialog_BillConfig_Patches), "DoStoreModeButton");
        list = Utils.PatchRange(
            list,
            info1,
            info5,
            new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, 5),
                new CodeInstruction(OpCodes.Call, info4)
            },
            debug: debug,
            offsetStart: -1
        ).ToList();

        // Second replace operation
        /* var info2 = AccessTools.Method(typeof(Listing), "GetRect");
        var info3 = AccessTools.GetDeclaredMethods(typeof(Widgets)).Where(method => method.Name == "Dropdown")
            .OrderBy(method => method.GetParameters().Length).First()
            .MakeGenericMethod(typeof(Bill_Production), typeof(Zone_Stockpile));
  
        var doStockpileDropdown = AccessTools.Method(typeof(Dialog_BillConfig_Patches), "DoStockpileDropdown");
        list = Utils.PatchRange(
            list,
            info2,
            info3,
            new[]
            {
                new CodeInstruction(OpCodes.Call, doStockpileDropdown)
            },
            debug: debug,
            offsetStart: -1
        ).ToList(); */

        var idx5 = list.FindIndex(ins => ins.Calls(info4));
        var idx6 = list.FindIndex(idx5, ins => ins.Calls(AccessTools.Method(typeof(Listing), "Gap")));
        list.InsertRange(idx6 + 1, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldloc_S, 5),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dialog_BillConfig_Patches), "DoTakeFrom"))
        });
        return list;
    }

    public static void DoTakeFrom(Dialog_BillConfig dialog, Listing_Standard listingStandard)
    {
        var listing = listingStandard.BeginSection(TakeFromSubdialogHeight);
        var extraData = GameComponent_ExtraBillData.Instance.GetData(dialog.bill);
        if (listing.ButtonText(extraData.TakeFromText()))
            Find.WindowStack.Add(HaulToBuildingMod.Settings.UseWindows
                ? new Dialog_Options(() => GenerateTakeFromOptions(dialog, extraData).ToList(), false)
                : new FloatMenu(GenerateTakeFromOptions(dialog, extraData)
                    .Select(ToggleOption.ToFloatMenuOption(false))
                    .ToList()));

        listingStandard.EndSection(listing);
        listingStandard.Gap();
    }

    public static IEnumerable<ToggleOption> GenerateTakeFromOptions(Dialog_BillConfig dialog,
        ExtraBillData extraData)
    {
        yield return new ToggleOption("HaulToBuilding.TakeFromAll".Translate(), !extraData.TakeFrom.Any(), () =>
        {
            extraData.TakeFrom.Clear();
            GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
        }, () => { });
        foreach (var slotGroup in dialog.bill.billStack.billGiver.Map.haulDestinationManager
                     .AllGroupsListInPriorityOrder)
        {
            var valid = slotGroup.parent.ValidTakeFrom(dialog.bill);
            yield return new ToggleOption(valid
                    ? "HaulToBuilding.TakeFromSpecific".Translate(slotGroup.parent
                        .SlotYielderLabel())
                    : $"{"HaulToBuilding.TakeFromSpecific".Translate(slotGroup.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})",
                extraData.TakeFrom.Contains(slotGroup.parent),
                () =>
                {
                    extraData.TakeFrom.Add(slotGroup.parent);
                    GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
                }, () =>
                {
                    extraData.TakeFrom.Remove(slotGroup.parent);
                    GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
                }, valid
            );
        }
    }

    public static void DoStockpileDropdown(Rect inRect, Dialog_BillConfig dialog)
    {
        var extraData = GameComponent_ExtraBillData.Instance.GetData(dialog.bill);
        if (Widgets.ButtonText(inRect, dialog.bill.LookInText(extraData)))
            Find.WindowStack.Add(HaulToBuildingMod.Settings.UseWindows
                ? new Dialog_Options(() => GenerateStockpileInclusion(dialog).ToList(), true)
                : new FloatMenu(GenerateStockpileInclusion(dialog).Select(ToggleOption.ToFloatMenuOption(true))
                    .ToList()));
    }

    public static IEnumerable<ToggleOption> GenerateStockpileInclusion(
        Dialog_BillConfig dialog)
    {
        var extraData = GameComponent_ExtraBillData.Instance.GetData(dialog.bill);
        yield return new ToggleOption("IncludeFromAll".Translate(),
            dialog.bill.includeGroup == null && extraData.LookInStorage == null,
            delegate
            {
                dialog.bill.includeGroup = null;
                extraData.LookInStorage = null;
                GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
            }, () => { });
        foreach (var slotGroup in dialog.bill.billStack.billGiver.Map.haulDestinationManager
                     .AllGroupsListInPriorityOrder)
        {
            var valid = slotGroup.parent.ValidLookIn(dialog.bill);
            var enabled = false;
            switch (slotGroup.parent)
            {
                case Zone_Stockpile stockpile:
                    enabled = dialog.bill.includeGroup == stockpile.slotGroup;
                    break;
                case Building_Storage b:
                    enabled = extraData.LookInStorage == b;
                    break;
            }

            yield return new ToggleOption(
                valid
                    ? "IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel())
                    : $"{"IncludeSpecific".Translate(slotGroup.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})",
                enabled, delegate
                {
                    switch (slotGroup.parent)
                    {
                        case Zone_Stockpile stockpile:
                            dialog.bill.includeGroup = stockpile.slotGroup;
                            extraData.LookInStorage = null;
                            break;
                        case Building_Storage b:
                            extraData.LookInStorage = b;
                            dialog.bill.includeGroup = null;
                            GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
                            break;
                    }
                }, delegate
                {
                    dialog.bill.includeGroup = null;
                    extraData.LookInStorage = null;
                    GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
                }, valid);
        }
    }
    public static void DoStoreModeButton(Dialog_BillConfig dialog, Listing_Standard listingStandard)
    {
        DoStoreModeButtonBase(dialog, listingStandard, Dialog_BillConfig.StoreModeSubdialogHeight);
    }
    public static void DoStoreModeButtonMath(Dialog_BillConfig dialog, Listing_Standard listingStandard)
    {
        Type dialogType = AccessTools.TypeByName("CrunchyDuck.Math.Dialog_MathBillConfig");
        System.Reflection.FieldInfo fieldInfo = AccessTools.Field(dialogType, "StoreModeSubdialogHeight");
        DoStoreModeButtonBase(dialog, listingStandard, (int)fieldInfo.GetValue(null));
    }

    public static void DoStoreModeButtonBase(Dialog_BillConfig dialog, Listing_Standard listingStandard, int sectionHeight)
    {
        var listing = listingStandard.BeginSection(sectionHeight);
        var extraData = GameComponent_ExtraBillData.Instance.GetData(dialog.bill);
        if (extraData.NeedCheck)
        {
            dialog.bill.ValidateSettings();
            extraData.NeedCheck = false;
        }

        var text2 = Utils.TakeToText(dialog.bill, extraData, out var incompatible);
        if (incompatible) Text.Font = GameFont.Tiny;

        if (listing.ButtonText(text2))
        {
            Text.Font = GameFont.Small;
            Find.WindowStack.Add(HaulToBuildingMod.Settings.UseWindows
                ? new Dialog_Options(() => GenerateStoreModeOptions(dialog, extraData).ToList(), true)
                : new FloatMenu(GenerateStoreModeOptions(dialog, extraData)
                    .Select(ToggleOption.ToFloatMenuOption(true)).ToList()));
        }

        Text.Font = GameFont.Small;
        listingStandard.EndSection(listing);
    }

    public static IEnumerable<ToggleOption> GenerateStoreModeOptions(Dialog_BillConfig dialog,
        ExtraBillData extraData)
    {
        foreach (var billStoreModeDef in from bsm in DefDatabase<BillStoreModeDef>.AllDefs
                 orderby bsm.listOrder
                 select bsm)
            if (billStoreModeDef == BillStoreModeDefOf.SpecificStockpile)
            {
                var groups = dialog.bill.billStack.billGiver.Map
                    .haulDestinationManager.AllGroupsListInPriorityOrder;
                foreach (var group in groups)
                    if (group.parent is Zone_Stockpile stockpile)
                    {
                        var valid = stockpile.ValidTakeTo(dialog.bill);
                        yield return new ToggleOption(
                            valid
                                ? string.Format(billStoreModeDef.LabelCap, group.parent.SlotYielderLabel())
                                : $"{string.Format(billStoreModeDef.LabelCap, group.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})",
                            dialog.bill.includeGroup == stockpile.slotGroup, delegate
                            {
                                dialog.bill.SetStoreMode(BillStoreModeDefOf.SpecificStockpile,
	                                stockpile.slotGroup);
                            }, delegate { dialog.bill.SetStoreMode(BillStoreModeDefOf.BestStockpile); }, valid);
                    }
            }
            else if (billStoreModeDef == HaulToBuildingDefOf.StorageBuilding)
            {
                var groups = dialog.bill.billStack.billGiver.Map
                    .haulDestinationManager.AllGroupsListInPriorityOrder;
                foreach (var group in groups)
                    if (group.parent is Building_Storage building)
                    {
                        var valid = building.ValidTakeTo(dialog.bill);
                        yield return new ToggleOption(
                            valid
                                ? string.Format(billStoreModeDef.LabelCap, group.parent.SlotYielderLabel())
                                : $"{string.Format(billStoreModeDef.LabelCap, group.parent.SlotYielderLabel())} ({"IncompatibleLower".Translate()})",
                            extraData.Storage == building, delegate
                            {
                                dialog.bill.SetStoreMode(HaulToBuildingDefOf.StorageBuilding);
                                extraData.Storage = building;
                                GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
                            }, delegate
                            {
                                dialog.bill.SetStoreMode(BillStoreModeDefOf.DropOnFloor);
                                extraData.Storage = null;
                                GameComponent_ExtraBillData.Instance.SetData(dialog.bill, extraData);
                            }, valid);
                    }
            }
            else
                yield return new ToggleOption(billStoreModeDef.LabelCap, dialog.bill.storeMode == billStoreModeDef,
                    delegate { dialog.bill.SetStoreMode(billStoreModeDef); },
                    delegate
                    {
                        dialog.bill.SetStoreMode(new List<BillStoreModeDef>
                                { BillStoreModeDefOf.BestStockpile, BillStoreModeDefOf.DropOnFloor }
                            .First(d => d != billStoreModeDef));
                    });
    }


    //-------------------------------------------------------------------------
    // Patches for Math! (Reworked)
    //-------------------------------------------------------------------------

    public static void DoMathPatches(Harmony harm)
    {
        Log.Message("[HaulToBuilding] Math! (Reworked) mod detected. Patching CrunchyDuck.Math.Dialog_MathBillConfig");
        
           
        Utils.TryPatch(harm,
            AccessTools.Method(AccessTools.TypeByName("CrunchyDuck.Math.Dialog_MathBillConfig"), "RenderStockpileSettings"),
            new HarmonyMethod(typeof(Dialog_BillConfig_Patches), nameof(MathStockpileTranspile)));

        /* Utils.TryPatch(harm, 
            AccessTools.Method(AccessTools.TypeByName("CrunchyDuck.Math.Dialog_MathBillConfig"), "RenderBillSettings"),
            new HarmonyMethod(typeof(Dialog_BillConfig_Patches), nameof(MathBillTranspile))); */

        // Make room for extra button
        Type dialogType = AccessTools.TypeByName("CrunchyDuck.Math.Dialog_MathBillConfig");
        System.Reflection.FieldInfo fieldInfo = AccessTools.Field(dialogType, "RepeatModeSubdialogHeight");
        fieldInfo.SetValue(null, (int)fieldInfo.GetValue(null) - 40);
    }

    public static IEnumerable<CodeInstruction> MathStockpileTranspile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        bool debug = true;
        var list = instructions.ToList();

        var info1 = AccessTools.Field(AccessTools.TypeByName("CrunchyDuck.Math.Dialog_MathBillConfig"), "StoreModeSubdialogHeight");
        var info5 = AccessTools.Method(typeof(Listing_Standard), "EndSection");
        var info4 = AccessTools.Method(typeof(Dialog_BillConfig_Patches), "DoStoreModeButtonMath");

        list = Utils.PatchRange(
            list,
            info1,
            info5,
            new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call, info4)
            },
            debug: debug,
            offsetStart: -1
        ).ToList();

        var idx5 = list.FindIndex(ins => ins.Calls(info4));
        var idx6 = list.FindIndex(idx5, ins => ins.Calls(AccessTools.Method(typeof(Listing), "Gap")));
        list.InsertRange(idx6 + 1, new[]
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldarg_1),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Dialog_BillConfig_Patches), "DoTakeFrom"))
        });
        return list;        
    }

    public static IEnumerable<CodeInstruction> MathBillTranspile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        bool debug = true;
        var list = instructions.ToList();

        var start = AccessTools.Field(typeof(Bill_Production), "includeGroup");
        var end = AccessTools.GetDeclaredMethods(typeof(Widgets)).Where(method => method.Name == "Dropdown")
            .OrderBy(method => method.GetParameters().Length).First()
            .MakeGenericMethod(typeof(Bill_Production), typeof(Zone_Stockpile));
  
        var doStockpileDropdown = AccessTools.Method(typeof(Dialog_BillConfig_Patches), "DoStockpileDropdown");

        /* Log.Message("\n\n\n\n----------------\n\n\n\n");
        foreach(var ins in list.Skip(list.FindIndex(ins => ins.LoadsField(start)) - 7)) {
            Log.Message("    " + ins);
        }
        Log.Message("\n\n\n\n----------------\n\n\n\n"); */

        list = Utils.PatchRange(
            list,
            start,
            end,
            new[]
            {
                // Get the Rect and push it onto the stack
                new CodeInstruction(OpCodes.Ldarg_1), // Load the listing_standard parameter
                new CodeInstruction(OpCodes.Ldc_R4, 30f),
                new CodeInstruction(OpCodes.Ldc_R4, 1f),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Listing_Standard), "GetRect", new[] { typeof(float), typeof(float) })),

                // Load 'this' onto the stack (Dialog_BillConfig instance)
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, doStockpileDropdown)
            },
            debug: debug,
            offsetStart: -6
        ).ToList();

        return list;        
    }
}