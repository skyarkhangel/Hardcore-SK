using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;

namespace StorageSelector.Core
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly bool IsBWMActive = ModsConfig.ActiveModsInLoadOrder.Any(m =>
            m.PackageId.Equals("falconne.bwm", StringComparison.OrdinalIgnoreCase));

        static HarmonyPatches()
        {
            try
            {
                var harmony = new Harmony("ZB333ZB.StorageSelector");

                UILogger.LogMessage("Starting patch initialization", "Startup");
                UILogger.LogMessage($"Better Workbench Management (BWM) is {(IsBWMActive ? "active" : "not active")}", "Startup");

                harmony.PatchAll(Assembly.GetExecutingAssembly());

                var patchedMethods = harmony.GetPatchedMethods().ToList();
                UILogger.LogMessage($"Successfully patched {patchedMethods.Count} methods", "Startup");

                foreach (var method in patchedMethods)
                {
                    var info = Harmony.GetPatchInfo(method);
                    UILogger.LogStateInfo("Patches", $"{method.DeclaringType?.Name}.{method.Name}", new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "Prefixes", info.Prefixes.Count },
                        { "Postfixes", info.Postfixes.Count },
                        { "Transpilers", info.Transpilers.Count }
                    });
                }

                CheckModIntegrations();

                if (Current.Game?.Maps != null)
                {
                    ValidateGameState();
                }

                UILogger.LogMessage("Initialization completed successfully", "Startup");
            }
            catch (Exception e)
            {
                UILogger.LogError("Failed to initialize patches", e, "Startup");

                if (e is HarmonyException he && he.InnerException != null)
                {
                    UILogger.LogError("Inner exception details", he.InnerException, "Startup");
                }
            }
        }

        private static void CheckModIntegrations()
        {
            try
            {
                var hasDeepStorage = AccessTools.TypeByName("LWM.DeepStorage.CompDeepStorage") != null;
                if (hasDeepStorage)
                {
                    UILogger.LogMessage("LWM's Deep Storage integration is active", "ModIntegration");
                }
            }
            catch (Exception e)
            {
                UILogger.LogError("Error checking mod integrations", e, "ModIntegration");
            }
        }

        private static void ValidateGameState()
        {
            try
            {
                foreach (var map in Current.Game.Maps)
                {
                    ValidateMapStorage(map);
                    ValidateBillAssignments(map);
                }
            }
            catch (Exception e)
            {
                UILogger.LogError("Error validating game state", e, "GameState");
            }
        }

        private static void ValidateMapStorage(Map map)
        {
            try
            {
                var storageBuildings = map.listerBuildings.allBuildingsColonist
                    .Where(b => b is Building_Storage)
                    .Cast<Building_Storage>();

                UILogger.LogStateInfo("Storage", $"Map {map.Index}", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "StorageBuildingCount", storageBuildings.Count() }
                });

                foreach (var storage in storageBuildings)
                {
                    var slotGroup = storage.GetSlotGroup();
                    var cellCount = slotGroup?.CellsList?.Count ?? 0;
                    var maxStacks = StorageUtility.GetMaxStacks(storage);
                    var usedStacks = StorageUtility.GetUsedStacks(storage);

                    UILogger.LogStateInfo("Storage", storage.Label, new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "Valid", StorageUtility.IsValidStorageBuilding(storage) },
                        { "CellCount", cellCount },
                        { "MaxStacks", maxStacks },
                        { "UsedStacks", usedStacks },
                        { "Position", storage.Position },
                        { "HasSettings", storage.GetSlotGroup()?.Settings != null },
                        { "IsReachable", storage.Map.reachability.CanReach(storage.Position, storage.Position, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)) }
                    });
                }
            }
            catch (Exception e)
            {
                UILogger.LogError($"Error validating storage for map {map.Index}", e, "Storage");
            }
        }

        private static void ValidateBillAssignments(Map map)
        {
            try
            {
                var billStorage = ExtendedBillDataStorage.GetStorage();
                if (billStorage == null)
                {
                    UILogger.LogWarning("Bill storage not available", "BillAssignments");
                    return;
                }

                var billGivers = map.listerBuildings.allBuildingsColonist
                    .OfType<IBillGiver>()
                    .Where(b => b.BillStack != null);

                foreach (var billGiver in billGivers)
                {
                    foreach (var bill in billGiver.BillStack)
                    {
                        var inputStorage = billStorage.GetInputStorage(bill);
                        var outputStorage = billStorage.GetOutputStorage(bill);

                        if (inputStorage != null || outputStorage != null)
                        {
                            UILogger.LogStateInfo("BillAssignments", bill.Label, new System.Collections.Generic.Dictionary<string, object>
                            {
                                { "InputStorage", inputStorage?.Label ?? "None" },
                                { "OutputStorage", outputStorage?.Label ?? "None" }
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UILogger.LogError($"Error validating bill assignments for map {map.Index}", e, "BillAssignments");
            }
        }
    }
}
