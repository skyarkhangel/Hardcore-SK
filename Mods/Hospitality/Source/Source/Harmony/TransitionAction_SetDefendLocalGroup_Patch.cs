using HarmonyLib;
using Verse;
using Verse.AI.Group;

namespace Hospitality.Harmony
{
    /// <summary>
    /// To prevent errors when this function is called and there are no ownedPawns left
    /// </summary>
    public class TransitionAction_SetDefendLocalGroup_Patch
    {
        [HarmonyPatch(typeof(TransitionAction_SetDefendLocalGroup), "DoAction")]
        public class DoAction
        {
            [HarmonyPrefix]
            public static bool Prefix(Transition trans)
            {
                var target = trans.target;
                return target.lord.ownedPawns.Any();
            }
        }
    }
}