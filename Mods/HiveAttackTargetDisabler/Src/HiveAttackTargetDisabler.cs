using RimWorld;
using Verse;
using HarmonyLib;
using Verse.AI;


namespace HiveAttackTargetDisabler
{
    [HarmonyPatch(typeof(Hive), nameof(Hive.ThreatDisabled))]
    [StaticConstructorOnStartup]
    public class Patch
    {
        static Patch() => new Harmony("HiveAttackTargetDisabler").PatchAll();

        [HarmonyPrefix]
        public static bool ThreatDisabled(IAttackTargetSearcher disabledFor, ref bool __result) => !(__result = true);
    }
}