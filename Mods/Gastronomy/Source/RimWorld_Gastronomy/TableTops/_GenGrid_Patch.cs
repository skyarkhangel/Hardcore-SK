using HarmonyLib;
using Verse;

namespace Gastronomy.TableTops
{
    /// <summary>
    /// So pawns won't eat where the register is placed
    /// </summary>
    internal static class _GenGrid_Patch
    {
        [HarmonyPatch(typeof(GenGrid), nameof(GenGrid.HasEatSurface))]
        public class HasEatSurface
        {
            [HarmonyPostfix]
            internal static void Postfix(IntVec3 c, Map map, ref bool __result)
            {
                if (!__result) return;

                if (c.GetFirstThing<Building_TableTop>(map) != null) __result = false;
            }
        }
    }
}