using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    public static class ITab_Pawn_Gear_Patch
    {
        // This is so the player can't force visitors to drop items. The button remains, though, until fixed by Ludeon. Thus, never?
        [HarmonyPatch(typeof(ITab_Pawn_Gear), "InterfaceDrop")]
        public class InterfaceDrop
        {
            [HarmonyPrefix]
            public static bool Prefix(ITab_Pawn_Gear __instance, Thing t)
            {
                var SelPawnForGear = Traverse.Create(__instance).Property("SelPawnForGear").GetValue<Pawn>();

                var preventDrop = SelPawnForGear.HostFaction == Faction.OfPlayer && !SelPawnForGear.IsPrisoner;
                return !preventDrop;
            }
        }
    }
}
