using RimWorld;
using Verse;
using StorageSelector.Core.Storage;

namespace StorageSelector.Core
{
    public static class StorageMessages
    {
        public static void ShowStorageFullMessage(IStoreSettingsParent storage)
        {
            string label = storage is Building_Storage building ?
                StorageUtility.GetStorageLabel(building) :
                "Unknown";

            Messages.Message(
                "ZB333ZB.StorageSelector.StorageFull".Translate(label),
                MessageTypeDefOf.RejectInput
            );
        }

        public static void ShowStorageNearlyFullMessage(IStoreSettingsParent storage, float percentRemaining)
        {
            string label = storage is Building_Storage building ?
                StorageUtility.GetStorageLabel(building) :
                "Unknown";

            Messages.Message(
                "ZB333ZB.StorageSelector.StorageNearlyFull".Translate(label, percentRemaining.ToString("F0")),
                MessageTypeDefOf.CautionInput
            );
        }

        public static void ShowStorageDisallowedMessage(IStoreSettingsParent storage, Thing thing)
        {
            string label = storage is Building_Storage building ?
                StorageUtility.GetStorageLabel(building) :
                "Unknown";

            Messages.Message(
                "ZB333ZB.StorageSelector.StorageDisallowed".Translate(thing.Label, label),
                MessageTypeDefOf.RejectInput
            );
        }

        public static void ShowStorageUnreachableMessage(IStoreSettingsParent storage)
        {
            string label = storage is Building_Storage building ?
                StorageUtility.GetStorageLabel(building) :
                "Unknown";

            Messages.Message(
                "ZB333ZB.StorageSelector.StorageUnreachable".Translate(label),
                MessageTypeDefOf.RejectInput
            );
        }

        public static void ShowStorageInvalidMessage(string billLabel, bool isInput, IStoreSettingsParent storage)
        {
            string label = storage is Building_Storage building ?
                StorageUtility.GetStorageLabel(building) :
                "Unknown";

            Messages.Message(
                "ZB333ZB.StorageSelector.StorageInvalid".Translate(
                    billLabel,
                    isInput ? "input" : "output",
                    label
                ),
                MessageTypeDefOf.NegativeEvent
            );
        }

        public static void ShowCheckStorageSettingsMessage(IStoreSettingsParent storage)
        {
            string label = storage is Building_Storage building ?
                StorageUtility.GetStorageLabel(building) :
                "Unknown";

            Messages.Message(
                "ZB333ZB.StorageSelector.CheckStorageSettings".Translate(label),
                MessageTypeDefOf.CautionInput
            );
        }

        public static void ShowProductsCreatedMessage(int count, IStoreSettingsParent storage)
        {
            string label = storage is Building_Storage building ?
                StorageUtility.GetStorageLabel(building) :
                "Unknown";

            Messages.Message(
                "ZB333ZB.StorageSelector.ProductsCreated".Translate(count, label),
                MessageTypeDefOf.TaskCompletion
            );
        }
    }
}
