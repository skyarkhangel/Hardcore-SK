using HarmonyLib;
using Verse;
using Verse.AI.Group;

namespace Hospitality.Patches
{
    /// <summary>
    /// To prevent errors when this function is called and there are no ownedPawns left or no destination is set
    /// </summary>
    public class TransitionAction_EnsureHaveExitDestination_Patch
    {
        [HarmonyPatch(typeof(TransitionAction_EnsureHaveExitDestination), nameof(TransitionAction_EnsureHaveExitDestination.DoAction))]
        public class DoAction
        {
            [HarmonyPrefix]
            public static bool Prefix(Transition trans)
            {
                var lordToilTravel = (LordToil_Travel) trans.target;
                lordToilTravel.lord.ownedPawns.RemoveAll(p => p == null || !p.SpawnedOrAnyParentSpawned || p.Dead);
                if (lordToilTravel.HasDestination()) return false;
                return lordToilTravel.lord.ownedPawns.Any();
            }
        }
    }
}