using HarmonyLib;
using RimWorld;
using Verse;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;

namespace StorageSelector.Patches.Storage
{
    public static class BuildingLifecyclePatch
    {
        [HarmonyPatch(typeof(Building), nameof(Building.DeSpawn))]
        public static class Building_DeSpawn_Patch
        {
            public static void Prefix(Building __instance)
            {
                try
                {
                    if (__instance is Building_Storage)
                    {
                        UILogger.LogStateInfo("BuildingLifecycle", "Storage building despawning", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "BuildingType", __instance.GetType().Name },
                            { "Position", __instance.Position }
                        });

                        StorageUtility.UpdateBillStorageReferences();
                    }
                }
                catch (System.Exception e)
                {
                    UILogger.LogError("Error during storage building despawn", e, "BuildingLifecycle");
                }
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.Destroy))]
        public static class Building_Destroy_Patch
        {
            public static void Prefix(Building __instance)
            {
                try
                {
                    if (__instance is Building_Storage)
                    {
                        UILogger.LogStateInfo("BuildingLifecycle", "Storage building being destroyed", new System.Collections.Generic.Dictionary<string, object>
                        {
                            { "BuildingType", __instance.GetType().Name },
                            { "Position", __instance.Position }
                        });

                        StorageUtility.UpdateBillStorageReferences();
                    }
                }
                catch (System.Exception e)
                {
                    UILogger.LogError("Error during storage building destruction", e, "BuildingLifecycle");
                }
            }
        }
    }
}
