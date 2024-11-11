using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;
using StorageSelector.Core;

namespace StorageSelector.Patches.BillWorker
{
    [HarmonyPatch(typeof(WorkGiver_DoBill))]
    [HarmonyPatch("TryFindBestBillIngredientsInSet")]
    public static class IngredientSearchPatch
    {
        public static void Prefix(List<Thing> availableThings, Bill bill)
        {
            try
            {
                if (bill == null)
                {
                    UILogger.LogWarning("Bill is null in ingredient search", "IngredientSearch");
                    return;
                }

                if (availableThings == null)
                {
                    UILogger.LogWarning("Available things list is null", "IngredientSearch");
                    return;
                }

                var storage = ExtendedBillDataStorage.GetStorage();
                if (storage == null)
                {
                    UILogger.LogWarning("Storage not available during ingredient search", "IngredientSearch");
                    return;
                }

                var inputStorage = storage.GetInputStorage(bill);
                if (inputStorage == null || !StorageUtility.IsValidStorageBuilding(inputStorage))
                {
                    return;
                }

                var validCells = new HashSet<IntVec3>(inputStorage.GetSlotGroup()?.CellsList ?? new List<IntVec3>());
                var initialCount = availableThings.Count;
                availableThings.RemoveAll(t => !validCells.Contains(t.Position));

                UILogger.LogStateInfo("IngredientSearch", "Filtered ingredients", new Dictionary<string, object>
                {
                    { "InitialCount", initialCount },
                    { "FilteredCount", availableThings.Count },
                    { "ValidCellsCount", validCells.Count }
                });
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error filtering ingredients", e, "IngredientSearch");
            }
        }
    }
}
