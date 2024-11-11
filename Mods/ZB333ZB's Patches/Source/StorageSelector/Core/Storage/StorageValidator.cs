using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using StorageSelector.UI.Logging;

namespace StorageSelector.Core.Storage
{
    public static class StorageValidator
    {
        public static bool IsValidStorageBuilding(Building_Storage building)
        {
            try
            {
                if (building == null)
                {
                    UILogger.LogWarning("Null building passed to IsValidStorageBuilding", "StorageValidation");
                    return false;
                }

                if (building.def == null)
                {
                    UILogger.LogWarning($"Building {building} has null def", "StorageValidation");
                    return false;
                }

                var isValid = building.def.thingClass == typeof(Building_Storage) &&
                            building.def.building != null &&
                            building.def.building.fixedStorageSettings != null;

                if (!isValid)
                {
                    UILogger.LogStateInfo("StorageValidation", "Invalid storage building", new Dictionary<string, object>
                    {
                        { "BuildingType", building.def.thingClass?.Name },
                        { "HasBuilding", building.def.building != null },
                        { "HasSettings", building.def.building?.fixedStorageSettings != null }
                    });
                }

                return isValid;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error validating storage building", e, "StorageValidation");
                return false;
            }
        }

        public static bool IsValidStorage(IStoreSettingsParent storage)
        {
            try
            {
                if (storage == null)
                {
                    UILogger.LogWarning("Null storage passed to IsValidStorage", "StorageValidation");
                    return false;
                }

                if (storage is Building_Storage building)
                    return IsValidStorageBuilding(building);

                if (storage is Zone_Stockpile zone)
                {
                    var isValid = zone.Map != null && !zone.Map.zoneManager.AllZones.Contains(zone);
                    if (!isValid)
                    {
                        UILogger.LogStateInfo("StorageValidation", "Invalid stockpile zone", new Dictionary<string, object>
                        {
                            { "HasMap", zone.Map != null },
                            { "InZoneManager", zone.Map?.zoneManager.AllZones.Contains(zone) }
                        });
                    }
                    return isValid;
                }

                UILogger.LogWarning($"Unknown storage type: {storage.GetType().Name}", "StorageValidation");
                return false;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error validating storage", e, "StorageValidation");
                return false;
            }
        }

        public static bool CanAcceptThing(IStoreSettingsParent storage, Thing thing, out string errorMessage)
        {
            try
            {
                if (storage == null)
                {
                    UILogger.LogWarning("Null storage in CanAcceptThing", "StorageValidation");
                    errorMessage = "InvalidStorage";
                    return false;
                }

                if (thing == null)
                {
                    UILogger.LogWarning("Null thing in CanAcceptThing", "StorageValidation");
                    errorMessage = "InvalidThing";
                    return false;
                }

                var storeSettings = storage.GetStoreSettings();
                if (storeSettings == null)
                {
                    UILogger.LogWarning("Null store settings", "StorageValidation");
                    errorMessage = "InvalidStorage";
                    return false;
                }

                if (!storeSettings.AllowedToAccept(thing))
                {
                    UILogger.LogStateInfo("StorageValidation", "Thing not allowed", new Dictionary<string, object>
                    {
                        { "ThingDef", thing.def.defName },
                        { "StorageType", storage.GetType().Name }
                    });
                    errorMessage = "StorageNotAllowed";
                    return false;
                }

                var maxStacks = StorageCapacity.GetMaxStacks(storage);
                var usedStacks = StorageCapacity.GetUsedStacks(storage);

                if (usedStacks >= maxStacks)
                {
                    UILogger.LogStateInfo("StorageValidation", "Storage full", new Dictionary<string, object>
                    {
                        { "UsedStacks", usedStacks },
                        { "MaxStacks", maxStacks }
                    });
                    errorMessage = "StorageFull";
                    return false;
                }

                if (usedStacks >= maxStacks * 0.9f)
                {
                    UILogger.LogStateInfo("StorageValidation", "Storage nearly full", new Dictionary<string, object>
                    {
                        { "UsedStacks", usedStacks },
                        { "MaxStacks", maxStacks },
                        { "PercentFull", (usedStacks / (float)maxStacks) * 100 }
                    });
                    errorMessage = "StorageNearlyFull";
                    return true;
                }

                errorMessage = null;
                return true;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error checking if storage can accept thing", e, "StorageValidation");
                errorMessage = "Error";
                return false;
            }
        }
    }
}
