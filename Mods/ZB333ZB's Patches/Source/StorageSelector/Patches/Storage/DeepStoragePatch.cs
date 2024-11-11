using System;
using System.Reflection;
using HarmonyLib;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;

namespace StorageSelector.Patches.Storage
{
    [HarmonyPatch]
    public static class DeepStoragePatch
    {
        private static readonly Type CompDeepStorageType = AccessTools.TypeByName("LWM.DeepStorage.CompDeepStorage");
        private static readonly Type IHoldMultipleThingsType = AccessTools.TypeByName("IHoldMultipleThings.IHoldMultipleThings");

        public static bool Prepare()
        {
            var hasDeepStorage = CompDeepStorageType != null && IHoldMultipleThingsType != null;

            if (hasDeepStorage)
            {
                UILogger.LogMessage("Deep Storage integration initialized", "DeepStorage");
            }
            else
            {
                UILogger.LogMessage("Deep Storage not found, skipping integration", "DeepStorage");
            }

            return hasDeepStorage;
        }

        public static MethodBase TargetMethod()
        {
            var method = CompDeepStorageType?.GetMethod("PostDeSpawn");

            if (method == null)
            {
                UILogger.LogWarning("Could not find Deep Storage PostDeSpawn method", "DeepStorage");
            }

            return method;
        }

        public static void Prefix()
        {
            try
            {
                UILogger.LogStateInfo("DeepStorage", "Deep storage component despawning");
                StorageUtility.UpdateBillStorageReferences();
            }
            catch (Exception e)
            {
                UILogger.LogError("Error during deep storage despawn", e, "DeepStorage");
            }
        }
    }
}
