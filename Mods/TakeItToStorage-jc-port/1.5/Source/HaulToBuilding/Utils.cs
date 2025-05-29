using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HaulToBuilding
{
    public static class Utils
    {
        public static Zone_Stockpile FakeStockpile(this ISlotGroupParent parent)
        {
            var pile = new Zone_Stockpile {settings = parent.GetStoreSettings()};
            return pile;
        }

        public static string TakeFromText(this ExtraBillData extraData) => !extraData.TakeFrom.Any()
            ? "HaulToBuilding.TakeFromAll".Translate()
            : "HaulToBuilding.TakeFromSpecific".Translate(
                extraData.TakeFrom.Count == 1
                    ? extraData.TakeFrom.First().SlotYielderLabel()
                    : "HaulToBuilding.Places".Translate(extraData.TakeFrom.Count).ToString());

        public static bool ValidTakeFrom(this ISlotGroupParent parent, Bill bill) => bill.recipe.ingredients.Any(ing =>
            ing.IsFixedIngredient
                ? parent.GetStoreSettings().AllowedToAccept(ing.FixedIngredient)
                : ing.filter.AllowedThingDefs.Any(def => parent.GetStoreSettings().AllowedToAccept(def)));

        public static string LookInText(this Bill_Production bill, ExtraBillData extraData) => bill.includeGroup == null && extraData.LookInStorage == null
            ? "IncludeFromAll".Translate()
            : "IncludeSpecific".Translate(
                ((ISlotGroupParent) bill.includeGroup ?? extraData.LookInStorage)
                .SlotYielderLabel());

        public static bool ValidLookIn(this ISlotGroupParent parent, Bill_Production bill) => bill.recipe.WorkerCounter.CanPossiblyStore(bill,
            (parent as Zone_Stockpile ?? parent.FakeStockpile()).slotGroup);

        public static string TakeToText(Bill_Production bill, ExtraBillData extraData, out bool incompatible)
        {
            var text = string.Format(bill.GetStoreMode().LabelCap,
                bill.storeGroup is SlotGroup sg ? sg.parent.SlotYielderLabel() :
                extraData.Storage != null ? extraData.Storage.SlotYielderLabel() : "");
            incompatible = bill.storeGroup != null &&
                !bill.recipe.WorkerCounter.CanPossiblyStore(bill, bill.storeGroup) || extraData.Storage != null &&
                !bill.recipe.WorkerCounter.CanPossiblyStore(bill,
                    extraData.Storage.FakeStockpile().slotGroup);
            if (incompatible) text += $" ({"IncompatibleLower".Translate()})";
            return text;
        }

        public static bool ValidTakeTo(this ISlotGroupParent parent, Bill_Production bill)
        {
            return parent switch
            {
                Building_Storage building => bill.recipe.WorkerCounter.CanPossiblyStore(bill, building.FakeStockpile().slotGroup),
                Zone_Stockpile stockpile => bill.recipe.WorkerCounter.CanPossiblyStore(bill, stockpile.slotGroup),
                _ => false
            };
        }

        public static bool TryPatch(Harmony harm, System.Reflection.MethodInfo targetMethod, HarmonyMethod transpilerMethod) {
            try
            {
                harm.Patch(targetMethod, transpiler: transpilerMethod);
            }
            catch (Exception e)
            {
                Log.Error($"Got error while patching {targetMethod}: {e.Message}\n{e.StackTrace}\n{e.InnerException?.Message}\n{e.InnerException?.StackTrace}");
                return false;
            }
            return true; 
        }

        public static IEnumerable<CodeInstruction> PatchRange(
            List<CodeInstruction> instructions,
            object startSubject,
            object endSubject,
            IEnumerable<CodeInstruction> replacementInstructions,
            string operationName = null,
            bool debug = false,
            int offsetStart = 0,
            int offsetEnd = 0)
        {


            operationName ??= $"{startSubject} -> {endSubject}";

            Predicate<CodeInstruction> CreatePredicate(object subject)
            {
                return subject switch
                {
                    System.Reflection.MethodInfo method => (CodeInstruction ins) => ins.Calls(method),
                    System.Reflection.FieldInfo field => (CodeInstruction ins) => ins.LoadsField(field),
                    OpCode opCode => (CodeInstruction ins) => ins.opcode == opCode,
                    _ => throw new ArgumentException($"Unsupported subject type: {subject.GetType()}")
                };
            }

            var startPredicate = CreatePredicate(startSubject);
            var endPredicate = CreatePredicate(endSubject);

            int startIndex = instructions.FindIndex(startPredicate);
            if (startIndex == -1)
            {
                if (debug) Log.Error($"HaulToBuilding: Could not find start of {operationName}");
                return instructions;
            }
            startIndex += offsetStart;

            int endIndex = instructions.FindIndex(startIndex + 1, endPredicate);
            if (endIndex == -1)
            {
                if (debug) Log.Error($"HaulToBuilding: Could not find end of {operationName}");
                return instructions;
            }
            endIndex += offsetEnd;

            int removeCount = endIndex - startIndex + 1;
            if (removeCount <= 0)
            {
                if (debug) Log.Error($"HaulToBuilding: Invalid replace range for {operationName}: startIndex={startIndex}, endIndex={endIndex}");
                return instructions;
            }

            var replacementList = replacementInstructions.ToList();

            if (debug) Log.Message($"HaulToBuilding: Replacing range for {operationName}: startIndex={startIndex}, endIndex={endIndex}, removeCount={removeCount}, insertCount={replacementList.Count}");

            instructions.RemoveRange(startIndex, removeCount);
            instructions.InsertRange(startIndex, replacementList);

            return instructions;
        }
    }
}