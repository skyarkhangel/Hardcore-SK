using System.Collections.Generic;
using RimWorld;
using Verse;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;

namespace StorageSelector.Core
{
    public class ExtendedBillDataStorage : GameComponent
    {
        private Dictionary<int, string> inputStorageThingIDs = new();
        private Dictionary<int, string> outputStorageThingIDs = new();

        public ExtendedBillDataStorage(Game game) : base()
        {
            try
            {
                UILogger.LogMessage("Initializing bill data storage", "Storage");
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error initializing bill data storage", e, "Storage");
            }
        }

        public override void ExposeData()
        {
            try
            {
                base.ExposeData();

                Scribe_Collections.Look(ref inputStorageThingIDs, "inputStorageThingIDs", LookMode.Value, LookMode.Value);
                Scribe_Collections.Look(ref outputStorageThingIDs, "outputStorageThingIDs", LookMode.Value, LookMode.Value);

                if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    inputStorageThingIDs ??= new Dictionary<int, string>();
                    outputStorageThingIDs ??= new Dictionary<int, string>();

                    UILogger.LogStateInfo("Storage", "Data loaded", new Dictionary<string, object>
                    {
                        { "InputStorageCount", inputStorageThingIDs.Count },
                        { "OutputStorageCount", outputStorageThingIDs.Count }
                    });
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error exposing bill data", e, "Storage");
                inputStorageThingIDs ??= new Dictionary<int, string>();
                outputStorageThingIDs ??= new Dictionary<int, string>();
            }
        }

        private Building_Storage RestoreBuildingReference(string thingID)
        {
            try
            {
                if (string.IsNullOrEmpty(thingID))
                {
                    UILogger.LogWarning("Attempted to restore null or empty thing ID", "Storage");
                    return null;
                }

                foreach (var map in Current.Game.Maps)
                {
                    var storageBuildings = map.listerBuildings.AllBuildingsColonistOfClass<Building_Storage>();
                    foreach (var storage in storageBuildings)
                    {
                        if (storage.ThingID == thingID)
                        {
                            if (!StorageUtility.IsValidStorageBuilding(storage))
                            {
                                UILogger.LogWarning($"Found storage building {thingID} but it's invalid", "Storage");
                                return null;
                            }
                            return storage;
                        }
                    }
                }

                UILogger.LogWarning($"Storage building {thingID} not found", "Storage");
                return null;
            }
            catch (System.Exception e)
            {
                UILogger.LogError($"Error restoring building reference for {thingID}", e, "Storage");
                return null;
            }
        }

        public void SetInputStorage(Bill bill, Building_Storage storage)
        {
            try
            {
                if (bill == null)
                {
                    UILogger.LogWarning("Attempted to set input storage for null bill", "Storage");
                    return;
                }

                var billID = bill.GetHashCode();
                if (storage == null)
                {
                    inputStorageThingIDs.Remove(billID);
                    UILogger.LogStateInfo("Storage", "Cleared input storage", new Dictionary<string, object>
                    {
                        { "BillLabel", bill.Label },
                        { "BillID", billID }
                    });
                }
                else
                {
                    inputStorageThingIDs[billID] = storage.ThingID;
                    UILogger.LogStateInfo("Storage", "Set input storage", new Dictionary<string, object>
                    {
                        { "BillLabel", bill.Label },
                        { "BillID", billID },
                        { "StorageID", storage.ThingID },
                        { "StorageLabel", storage.Label }
                    });
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error setting input storage", e, "Storage");
            }
        }

        public void SetOutputStorage(Bill bill, Building_Storage storage)
        {
            try
            {
                if (bill == null)
                {
                    UILogger.LogWarning("Attempted to set output storage for null bill", "Storage");
                    return;
                }

                var billID = bill.GetHashCode();
                if (storage == null)
                {
                    outputStorageThingIDs.Remove(billID);
                    UILogger.LogStateInfo("Storage", "Cleared output storage", new Dictionary<string, object>
                    {
                        { "BillLabel", bill.Label },
                        { "BillID", billID }
                    });
                }
                else
                {
                    outputStorageThingIDs[billID] = storage.ThingID;
                    UILogger.LogStateInfo("Storage", "Set output storage", new Dictionary<string, object>
                    {
                        { "BillLabel", bill.Label },
                        { "BillID", billID },
                        { "StorageID", storage.ThingID },
                        { "StorageLabel", storage.Label }
                    });
                }
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error setting output storage", e, "Storage");
            }
        }

        public Building_Storage GetInputStorage(Bill bill)
        {
            try
            {
                if (bill == null)
                {
                    UILogger.LogWarning("Attempted to get input storage for null bill", "Storage");
                    return null;
                }

                var billID = bill.GetHashCode();
                if (!inputStorageThingIDs.TryGetValue(billID, out var thingID))
                {
                    return null;
                }

                var storage = RestoreBuildingReference(thingID);
                if (storage == null && thingID != null)
                {
                    inputStorageThingIDs.Remove(billID);
                    UILogger.LogWarning($"Removed invalid input storage reference for bill {bill.Label}", "Storage");
                }

                return storage;
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error getting input storage", e, "Storage");
                return null;
            }
        }

        public Building_Storage GetOutputStorage(Bill bill)
        {
            try
            {
                if (bill == null)
                {
                    UILogger.LogWarning("Attempted to get output storage for null bill", "Storage");
                    return null;
                }

                var billID = bill.GetHashCode();
                if (!outputStorageThingIDs.TryGetValue(billID, out var thingID))
                {
                    return null;
                }

                var storage = RestoreBuildingReference(thingID);
                if (storage == null && thingID != null)
                {
                    outputStorageThingIDs.Remove(billID);
                    UILogger.LogWarning($"Removed invalid output storage reference for bill {bill.Label}", "Storage");
                }

                return storage;
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error getting output storage", e, "Storage");
                return null;
            }
        }

        public static ExtendedBillDataStorage GetStorage()
        {
            try
            {
                var storage = Current.Game?.GetComponent<ExtendedBillDataStorage>();
                if (storage == null)
                {
                    UILogger.LogWarning("Bill data storage component not found", "Storage");
                }
                return storage;
            }
            catch (System.Exception e)
            {
                UILogger.LogError("Error getting bill data storage", e, "Storage");
                return null;
            }
        }
    }
}
