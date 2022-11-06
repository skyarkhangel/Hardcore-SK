using DualWield;
using DualWield.Storage;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace yayoAni
{
    public static class DualWield
    {
        private delegate object GetInstance();

        private static GetInstance DualWieldInstanceGetter;
        private static AccessTools.FieldRef<object, WorldComponent> ExtendedDataStorageField;

        public static void Init()
        {
            DualWieldInstanceGetter = AccessTools.MethodDelegate<GetInstance>(AccessTools.PropertyGetter(typeof(Base), nameof(Base.Instance)));
            ExtendedDataStorageField = AccessTools.FieldRefAccess<WorldComponent>(typeof(Base), "_extendedDataStorage");
        }
        
        public static bool TryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
        {
            result = null;
            if (instance.pawn.HasMissingArmOrHand() || ExtendedDataStorageField(DualWieldInstanceGetter()) is not ExtendedDataStorage store)
            {
                return false;
            }

            foreach (ThingWithComps twc in instance.AllEquipmentListForReading)
            {
                if (store.TryGetExtendedDataFor(twc, out ExtendedThingWithCompsData ext) && ext.isOffHand)
                {
                    result = twc;
                    return true;
                }
            }

            return false;
        }

        public static bool HasMissingArmOrHand(this Pawn instance)
        {
            bool hasMissingHand = false;
            foreach (Hediff_MissingPart missingPart in instance.health.hediffSet.GetMissingPartsCommonAncestors())
            {
                if (missingPart.Part.def == BodyPartDefOf.Hand || missingPart.Part.def == BodyPartDefOf.Arm)
                {
                    hasMissingHand = true;
                }
            }

            return hasMissingHand;
        }

        public static Pawn_StanceTracker GetStancesOffHand(this Pawn instance)
        {
            if (ExtendedDataStorageField(DualWieldInstanceGetter()) is ExtendedDataStorage store)
            {
                return store.GetExtendedDataFor(instance).stancesOffhand;
            }

            return null;
        }
    }
}