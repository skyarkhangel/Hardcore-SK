using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using StorageSelector.UI.Logging;

namespace StorageSelector.Core.Storage
{
    public static class StorageTooltip
    {
        public static string GetStorageTooltip(IStoreSettingsParent storage)
        {
            try
            {
                if (storage == null)
                {
                    UILogger.LogWarning("Null storage passed to GetStorageTooltip", "StorageTooltip");
                    return "";
                }

                var maxStacks = StorageCapacity.GetMaxStacks(storage);
                var usedStacks = StorageCapacity.GetUsedStacks(storage);

                string storageLabel = GetStorageDisplayLabel(storage);
                var tooltip = BuildBaseTooltip(storageLabel, usedStacks, maxStacks);
                var storedItems = GetStoredItems(storage);

                return BuildFullTooltip(tooltip, storedItems);
            }
            catch (Exception e)
            {
                UILogger.LogError("Error generating storage tooltip", e, "StorageTooltip");
                return "";
            }
        }

        private static string GetStorageDisplayLabel(IStoreSettingsParent storage)
        {
            if (storage is Zone_Stockpile zoneStorage)
                return zoneStorage.label;
            if (storage is Building_Storage buildingStorage)
                return GetStorageLabel(buildingStorage);
            return "Unknown";
        }

        public static string GetStorageLabel(Building_Storage storage)
        {
            try
            {
                if (storage == null)
                {
                    UILogger.LogWarning("Null storage passed to GetStorageLabel", "StorageLabel");
                    return "";
                }
                return storage.Label ?? storage.def?.label ?? "";
            }
            catch (Exception e)
            {
                UILogger.LogError("Error getting storage label", e, "StorageLabel");
                return "";
            }
        }

        private static string BuildBaseTooltip(string label, int usedStacks, int maxStacks)
        {
            return "ZB333ZB.StorageSelector.StorageTooltip".Translate(
                label,
                $"{usedStacks}/{maxStacks}"
            );
        }

        private static Dictionary<ThingDef, int> GetStoredItems(IStoreSettingsParent storage)
        {
            var storedItems = new Dictionary<ThingDef, int>();
            var positions = new List<IntVec3>();

            if (storage is Building_Storage building)
            {
                positions = StorageCapacity.GetBuildingPositions(building);
            }
            else if (storage is Zone_Stockpile zoneStockpile)
            {
                positions.AddRange(zoneStockpile.cells);
            }

            foreach (var pos in positions)
            {
                var map = storage is Thing t ? t.Map : (storage as Zone_Stockpile)?.Map;
                if (map == null) continue;

                var thingsAtPos = map.thingGrid.ThingsListAtFast(pos);
                foreach (var thing in thingsAtPos)
                {
                    if (thing.def.category != ThingCategory.Item) continue;

                    if (!storedItems.ContainsKey(thing.def))
                        storedItems[thing.def] = 0;
                    storedItems[thing.def] += thing.stackCount;
                }
            }

            return storedItems;
        }

        private static string BuildFullTooltip(string baseTooltip, Dictionary<ThingDef, int> storedItems)
        {
            var tooltip = baseTooltip + "\n\n" + "ZB333ZB.StorageSelector.StoredItems".Translate() + ":";

            if (!storedItems.Any())
            {
                tooltip += $"\n  - {"ZB333ZB.StorageSelector.Nothing".Translate()}";
            }
            else
            {
                foreach (var item in storedItems)
                {
                    tooltip += $"\n  - {item.Key.label} (x{item.Value})";
                }
            }

            return tooltip;
        }
    }
}
