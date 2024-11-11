using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;
using StorageSelector.UI.Logging;

namespace StorageSelector.Core.Storage
{
    public static class StorageCapacity
    {
        public static int GetMaxStacks(IStoreSettingsParent storage)
        {
            try
            {
                if (storage == null)
                {
                    UILogger.LogWarning("Null storage passed to GetMaxStacks", "StorageCapacity");
                    return 0;
                }

                if (storage is Building_Storage building)
                {
                    return GetBuildingMaxStacks(building);
                }

                if (storage is Zone_Stockpile zone)
                {
                    return zone.cells?.Count ?? 0;
                }

                UILogger.LogWarning($"Unknown storage type in GetMaxStacks: {storage.GetType().Name}", "StorageCapacity");
                return 0;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error getting max stacks", e, "StorageCapacity");
                return 1;
            }
        }

        private static int GetBuildingMaxStacks(Building_Storage building)
        {
            try
            {
                if (building?.def == null) return 0;

                var deepStorageComp = building.GetComps<ThingComp>()
                    .FirstOrDefault(c =>
                        c.GetType().FullName.Contains("DeepStorage") ||
                        c.GetType().GetInterfaces().Any(i => i.Name == "IHoldMultipleThings") ||
                        c.GetType().BaseType?.Name == "CompDeepStorage");

                if (deepStorageComp == null) return 1;

                var maxStacksField = deepStorageComp.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(f => f.Name.Contains("maxNumberStacks"));

                if (maxStacksField == null)
                {
                    UILogger.LogWarning("Could not find maxNumberStacks field", "StorageCapacity");
                    return 1;
                }

                var maxStacks = maxStacksField.GetValue(deepStorageComp) as int?;
                if (!maxStacks.HasValue)
                {
                    UILogger.LogWarning("maxNumberStacks value is null", "StorageCapacity");
                    return 1;
                }

                var cellCount = building.def.size.x * building.def.size.z;
                return maxStacks.Value * cellCount;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error getting building max stacks", e, "StorageCapacity");
                return 1;
            }
        }

        public static int GetUsedStacks(IStoreSettingsParent storage)
        {
            try
            {
                if (storage == null)
                {
                    UILogger.LogWarning("Null storage passed to GetUsedStacks", "StorageCapacity");
                    return 0;
                }

                if (storage is Building_Storage building)
                {
                    return GetBuildingUsedStacks(building);
                }

                if (storage is Zone_Stockpile zone)
                {
                    return GetZoneUsedStacks(zone);
                }

                UILogger.LogWarning($"Unknown storage type in GetUsedStacks: {storage.GetType().Name}", "StorageCapacity");
                return 0;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error getting used stacks", e, "StorageCapacity");
                return 0;
            }
        }

        private static int GetBuildingUsedStacks(Building_Storage building)
        {
            try
            {
                var positions = GetBuildingPositions(building);
                var usedStacks = 0;

                foreach (var pos in positions)
                {
                    var thingsAtPos = building.Map.thingGrid.ThingsListAtFast(pos);
                    usedStacks += thingsAtPos.Count(t => t.def.category == ThingCategory.Item);
                }

                return usedStacks;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error getting building used stacks", e, "StorageCapacity");
                return 0;
            }
        }

        private static int GetZoneUsedStacks(Zone_Stockpile zone)
        {
            try
            {
                return zone.cells
                    .Sum(cell => zone.Map.thingGrid.ThingsListAtFast(cell)
                        .Count(t => t.def.category == ThingCategory.Item));
            }
            catch (Exception e)
            {
                UILogger.LogError("Error getting zone used stacks", e, "StorageCapacity");
                return 0;
            }
        }

        public static List<IntVec3> GetBuildingPositions(Building_Storage building)
        {
            var positions = new List<IntVec3>();
            var size = building.def.size;
            var rot = building.Rotation;
            var basePos = building.Position;

            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    var offset = new IntVec3(x, 0, z).RotatedBy(rot);
                    positions.Add(basePos + offset);
                }
            }

            return positions;
        }
    }
}
