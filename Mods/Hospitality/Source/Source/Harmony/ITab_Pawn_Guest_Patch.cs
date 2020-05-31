using HarmonyLib;
using Verse;

namespace Hospitality.Harmony
{
    public static class ITab_Pawn_Guest_Patch
    {
        [HarmonyPatch(typeof(RimWorld.ITab_Pawn_Guest))]
        [HarmonyPatch("IsVisible", MethodType.Getter)]
        public static class IsVisible
        {
            private static Traverse selPawnProperty;

            // Added so guests will not show vanilla guest tab
            [HarmonyPostfix]
            public static void Postfix(RimWorld.ITab_Pawn_Guest __instance, ref bool __result)
            {
                if(selPawnProperty == null)
                    selPawnProperty = Traverse.Create(__instance).Property("SelPawn");

                var selPawn = selPawnProperty.GetValue<Pawn>();
                __result &= !selPawn.IsGuest();
            }
        }
    }
}