using RimWorld;
using Verse;

namespace StorageSelector.Core.Storage
{
    public static class StorageUtility
    {
        public static bool IsValidStorageBuilding(Building_Storage building) =>
            StorageValidator.IsValidStorageBuilding(building);

        public static bool IsValidStorage(IStoreSettingsParent storage) =>
            StorageValidator.IsValidStorage(storage);

        public static bool CanAcceptThing(IStoreSettingsParent storage, Thing thing, out string errorMessage) =>
            StorageValidator.CanAcceptThing(storage, thing, out errorMessage);

        public static int GetMaxStacks(IStoreSettingsParent storage) =>
            StorageCapacity.GetMaxStacks(storage);

        public static int GetUsedStacks(IStoreSettingsParent storage) =>
            StorageCapacity.GetUsedStacks(storage);

        public static System.Collections.Generic.List<IntVec3> GetBuildingPositions(Building_Storage building) =>
            StorageCapacity.GetBuildingPositions(building);

        public static string GetStorageLabel(Building_Storage storage) =>
            StorageTooltip.GetStorageLabel(storage);

        public static string GetStorageTooltip(IStoreSettingsParent storage) =>
            StorageTooltip.GetStorageTooltip(storage);

        public static void UpdateBillStorageReferences() =>
            StorageUpdater.UpdateBillStorageReferences();
    }
}
