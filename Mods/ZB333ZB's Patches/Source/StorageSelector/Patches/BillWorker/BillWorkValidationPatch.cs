using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;
using StorageSelector.Core;

namespace StorageSelector.Patches.BillWorker
{
    [HarmonyPatch(typeof(Bill))]
    [HarmonyPatch(nameof(Bill.Notify_BillWorkStarted))]
    public static class BillWorkValidationPatch
    {
        public static void Prefix(Bill __instance, Pawn billDoer)
        {
            try
            {
                if (__instance == null)
                {
                    UILogger.LogWarning("Bill is null in work validation", "BillValidation");
                    return;
                }

                if (billDoer == null)
                {
                    UILogger.LogWarning("Bill doer is null", "BillValidation");
                    return;
                }

                var storage = ExtendedBillDataStorage.GetStorage();
                if (storage == null)
                {
                    UILogger.LogWarning("Storage not available during work validation", "BillValidation");
                    return;
                }

                ValidateInputStorage(__instance, billDoer, storage);
                ValidateOutputStorage(__instance, storage);
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error validating bill work", e, "BillValidation");
            }
        }

        private static void ValidateInputStorage(Bill bill, Pawn billDoer, ExtendedBillDataStorage storage)
        {
            try
            {
                var inputStorage = storage.GetInputStorage(bill);
                if (inputStorage != null)
                {
                    if (!StorageUtility.IsValidStorageBuilding(inputStorage))
                    {
                        storage.SetInputStorage(bill, null);
                        StorageMessages.ShowStorageInvalidMessage(bill.Label, true, inputStorage);

                        UILogger.LogStateInfo("BillValidation", "Invalid input storage", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "BillLabel", bill.Label },
                            { "StorageType", inputStorage.GetType().Name }
                        });
                    }
                    else if (!billDoer.Map.reachability.CanReach(billDoer.Position, inputStorage, PathEndMode.Touch,
                        TraverseParms.For(billDoer)))
                    {
                        StorageMessages.ShowStorageUnreachableMessage(inputStorage);

                        UILogger.LogStateInfo("BillValidation", "Unreachable input storage", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "BillLabel", bill.Label },
                            { "PawnPosition", billDoer.Position },
                            { "StoragePosition", inputStorage.Position }
                        });
                    }
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error validating input storage", e, "BillValidation");
            }
        }

        private static void ValidateOutputStorage(Bill bill, ExtendedBillDataStorage storage)
        {
            try
            {
                var outputStorage = storage.GetOutputStorage(bill);
                if (outputStorage != null)
                {
                    if (!StorageUtility.IsValidStorageBuilding(outputStorage))
                    {
                        storage.SetOutputStorage(bill, null);
                        StorageMessages.ShowStorageInvalidMessage(bill.Label, false, outputStorage);

                        UILogger.LogStateInfo("BillValidation", "Invalid output storage", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "BillLabel", bill.Label },
                            { "StorageType", outputStorage.GetType().Name }
                        });
                    }
                    else
                    {
                        var recipe = bill as Bill_Production;
                        if (recipe?.recipe.ProducedThingDef != null)
                        {
                            if (!outputStorage.GetSlotGroup()?.Settings?.AllowedToAccept(recipe.recipe.ProducedThingDef) ?? false)
                            {
                                StorageMessages.ShowCheckStorageSettingsMessage(outputStorage);

                                UILogger.LogStateInfo("BillValidation", "Storage settings mismatch", new System.Collections.Generic.Dictionary<string, object>
                                {
                                    { "BillLabel", bill.Label },
                                    { "ProductType", recipe.recipe.ProducedThingDef.defName },
                                    { "StorageType", outputStorage.GetType().Name }
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error validating output storage", e, "BillValidation");
            }
        }
    }
}
