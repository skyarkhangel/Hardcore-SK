using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    /// <summary>
    /// Prevent guests from binging
    /// </summary>
    public class AddictionUtility_Patch
    {
        [HarmonyPatch(typeof (AddictionUtility), nameof(AddictionUtility.CanBingeOnNow))]
        public class CanBingeOnNow
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn pawn)
            {
                if (!pawn.IsGuest()) return true;
                
                __result = false;
                return false;
            }
        }
    }
}

    
