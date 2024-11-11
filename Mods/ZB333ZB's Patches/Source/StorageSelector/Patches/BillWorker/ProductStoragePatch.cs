using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;
using StorageSelector.Core;

namespace StorageSelector.Patches.BillWorker
{
    [HarmonyPatch(typeof(Toils_Recipe))]
    [HarmonyPatch("FinishRecipeAndStartStoringProduct")]
    public static class ProductStoragePatch
    {
        public static void Postfix(Toil __instance)
        {
            try
            {
                var job = __instance?.actor?.CurJob;
                if (job?.bill == null)
                {
                    UILogger.LogWarning("Invalid job or bill in product storage", "ProductStorage");
                    return;
                }

                var storage = ExtendedBillDataStorage.GetStorage();
                if (storage == null)
                {
                    UILogger.LogWarning("Storage not available during product storage", "ProductStorage");
                    return;
                }

                IStoreSettingsParent targetStorage = GetTargetStorage(job.bill, storage);
                if (targetStorage == null)
                {
                    return;
                }

                var workTable = job.GetTarget(TargetIndex.A).Thing as Building_WorkTable;
                if (workTable == null)
                {
                    UILogger.LogWarning("Work table not found", "ProductStorage");
                    return;
                }

                ProcessProducts(workTable, job.bill, targetStorage, __instance.actor);
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error storing products", e, "ProductStorage");
            }
        }

        private static IStoreSettingsParent GetTargetStorage(Bill bill, ExtendedBillDataStorage storage)
        {
            try
            {
                var outputStorage = storage.GetOutputStorage(bill);
                if (outputStorage != null && StorageUtility.IsValidStorageBuilding(outputStorage))
                {
                    return outputStorage;
                }

                if (bill is Bill_Production billProd)
                {
                    var storeMode = billProd.GetStoreMode();
                    if (storeMode == BillStoreModeDefOf.SpecificStockpile)
                    {
                        return billProd.GetStoreZone();
                    }
                }

                return null;
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error getting target storage", e, "ProductStorage");
                return null;
            }
        }

        private static void ProcessProducts(Building_WorkTable workTable, Bill bill, IStoreSettingsParent targetStorage, Pawn actor)
        {
            try
            {
                var products = workTable.Map.thingGrid.ThingsListAt(workTable.Position)
                    .Where(t => t.def == bill.recipe.ProducedThingDef)
                    .ToList();

                if (!products.Any())
                {
                    UILogger.LogWarning("No products found at workbench position after bill completion", "ProductStorage");
                    return;
                }

                UILogger.LogStateInfo("ProductStorage", "Found products", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "ProductCount", products.Count },
                    { "ProductType", bill.recipe.ProducedThingDef?.defName }
                });

                if (targetStorage is Thing storeThing &&
                    !actor.Map.reachability.CanReach(workTable.Position, storeThing,
                        PathEndMode.Touch, TraverseParms.For(actor)))
                {
                    StorageMessages.ShowStorageUnreachableMessage(targetStorage);
                    return;
                }

                var successfulHauls = 0;
                foreach (var product in products)
                {
                    if (TryHaulProduct(product, targetStorage, actor))
                    {
                        successfulHauls++;
                    }
                }

                if (successfulHauls > 0)
                {
                    StorageMessages.ShowProductsCreatedMessage(successfulHauls, targetStorage);
                }

                UILogger.LogStateInfo("ProductStorage", "Completed hauling", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "SuccessfulHauls", successfulHauls },
                    { "TotalProducts", products.Count }
                });
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error processing products", e, "ProductStorage");
            }
        }

        private static bool TryHaulProduct(Thing product, IStoreSettingsParent targetStorage, Pawn actor)
        {
            try
            {
                if (!StorageUtility.CanAcceptThing(targetStorage, product, out string errorMessage))
                {
                    HandleStorageError(targetStorage, product, errorMessage);
                    return false;
                }

                if (targetStorage is Building_Storage buildingStorage)
                {
                    var haulJob = JobMaker.MakeJob(Core.JobDefOf.StorageHaul, product, buildingStorage);
                    actor.jobs.StartJob(haulJob, JobCondition.InterruptForced);
                    return true;
                }

                return false;
            }
            catch (System.Exception e)
            {
                UILogger.LogError($"Error hauling product {product?.def?.defName ?? "null"}", e, "ProductStorage");
                return false;
            }
        }

        private static void HandleStorageError(IStoreSettingsParent targetStorage, Thing product, string errorMessage)
        {
            switch (errorMessage)
            {
                case "StorageFull":
                    StorageMessages.ShowStorageFullMessage(targetStorage);
                    break;
                case "StorageNotAllowed":
                    StorageMessages.ShowStorageDisallowedMessage(targetStorage, product);
                    break;
                case "StorageNearlyFull":
                    var maxStacks = StorageUtility.GetMaxStacks(targetStorage);
                    var usedStacks = StorageUtility.GetUsedStacks(targetStorage);
                    var percentRemaining = ((maxStacks - usedStacks) / (float)maxStacks) * 100f;
                    StorageMessages.ShowStorageNearlyFullMessage(targetStorage, percentRemaining);
                    break;
            }

            UILogger.LogStateInfo("ProductStorage", "Storage error", new System.Collections.Generic.Dictionary<string, object>
            {
                { "ErrorType", errorMessage },
                { "ProductType", product?.def?.defName },
                { "StorageType", targetStorage?.GetType().Name }
            });
        }
    }
}
