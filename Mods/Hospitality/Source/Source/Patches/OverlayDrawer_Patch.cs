using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches 
{
    /// <summary>
    /// Don't draw the trader question mark on guests.
    /// </summary>
    public class OverlayDrawer_Patch
    {
        [HarmonyPatch(typeof(OverlayDrawer), nameof(OverlayDrawer.DrawOverlay))]
        public class DrawOverlay
        {
            [HarmonyPrefix]
            public static bool Prefix(Thing t, OverlayTypes overlayType)
            {
                if (!(t is Pawn pawn)) return true;
                var tryingToDrawQuestionMarkOnGuest = overlayType == OverlayTypes.QuestionMark && pawn.IsGuest();
                return !tryingToDrawQuestionMarkOnGuest;
            }
        }
    }
}
