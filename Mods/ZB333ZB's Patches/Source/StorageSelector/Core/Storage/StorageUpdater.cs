using System;
using System.Linq;
using RimWorld;
using Verse;
using StorageSelector.UI.Logging;

namespace StorageSelector.Core.Storage
{
    public static class StorageUpdater
    {
        public static void UpdateBillStorageReferences()
        {
            try
            {
                var storage = ExtendedBillDataStorage.GetStorage();
                if (storage == null)
                {
                    UILogger.LogWarning("Bill storage not available for updating references", "StorageUpdate");
                    return;
                }

                var maps = Find.Maps;
                if (maps == null)
                {
                    UILogger.LogWarning("No maps available for updating storage references", "StorageUpdate");
                    return;
                }

                foreach (var map in maps)
                {
                    if (map == null) continue;

                    UpdateMapStorageReferences(map, storage);
                }
            }
            catch (Exception e)
            {
                UILogger.LogError("Error updating bill storage references", e, "StorageUpdate");
            }
        }

        private static void UpdateMapStorageReferences(Map map, ExtendedBillDataStorage storage)
        {
            try
            {
                var billGivers = map.listerBuildings.allBuildingsColonist
                    .OfType<IBillGiver>()
                    .Where(b => b.BillStack != null);

                foreach (var billGiver in billGivers)
                {
                    foreach (var bill in billGiver.BillStack.Bills.OfType<Bill_Production>())
                    {
                        UpdateBillStorageReference(bill, storage);
                    }
                }
            }
            catch (Exception e)
            {
                UILogger.LogError($"Error updating storage references for map {map.Index}", e, "StorageUpdate");
            }
        }

        private static void UpdateBillStorageReference(Bill_Production bill, ExtendedBillDataStorage storage)
        {
            try
            {
                var inputStorage = storage.GetInputStorage(bill);
                var outputStorage = storage.GetOutputStorage(bill);

                if (inputStorage != null && (!inputStorage.Spawned || !StorageValidator.IsValidStorageBuilding(inputStorage)))
                {
                    storage.SetInputStorage(bill, null);
                    UILogger.LogStateInfo("StorageUpdate", "Cleared invalid input storage", new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "BillLabel", bill.Label },
                        { "StorageSpawned", inputStorage.Spawned },
                        { "StorageValid", StorageValidator.IsValidStorageBuilding(inputStorage) }
                    });
                }

                if (outputStorage != null && (!outputStorage.Spawned || !StorageValidator.IsValidStorageBuilding(outputStorage)))
                {
                    storage.SetOutputStorage(bill, null);
                    UILogger.LogStateInfo("StorageUpdate", "Cleared invalid output storage", new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "BillLabel", bill.Label },
                        { "StorageSpawned", outputStorage.Spawned },
                        { "StorageValid", StorageValidator.IsValidStorageBuilding(outputStorage) }
                    });
                }
            }
            catch (Exception e)
            {
                UILogger.LogError($"Error updating storage reference for bill {bill.Label}", e, "StorageUpdate");
            }
        }
    }
}
