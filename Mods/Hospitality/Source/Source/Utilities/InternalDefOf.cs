using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming
// ReSharper disable UnassignedField.Global

namespace Hospitality.Utilities
{
    [RimWorld.DefOf]
    public static class InternalDefOf
    {
        [MayRequire("CETeam.CombatExtended")]
        public static ThingDef Apparel_Backpack;

        [MayRequire("VanillaExpanded.VMemesE")]
        public static PreceptDef VME_Anonymity_Required;

        [MayRequire("Orion.Gastronomy")]
        public static ThingDef Gastronomy_DiningSpot;

        public static JobDef VendingMachine_EmptyVendingMachine;
        public static JobDef ClaimGuestBed;
        public static JobDef ScroungeFood;
        public static JobDef SwipeFood;
        public static RoomRoleDef GuestRoom;
        public static SpecialThingFilterDef AllowRotten;
        public static JoyGiverDef BuyFood;
        public static ConceptDef GuestWork;
        public static ThoughtDef GuestExpensiveFood;
        public static ThoughtDef GuestCheapFood;
        public static ThoughtDef GuestCantAffordBed;
        public static ThoughtDef GuestHasNoFood;
    }
}
