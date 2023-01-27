using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
static class PlaySettings_DoPlaySettingsGlobalControls_Patch
{
    static void Postfix(WidgetRow row, bool worldView)
    {
        if (worldView)
            return;

        row.ToggleableIcon(ref JobGiver_OptimizeApparel_TryGiveJob_Patch.optimizeApparel, Textures.OptimizeApparel, Strings.OptimizeApparelDesc, SoundDefOf.Mouseover_ButtonToggle, null);
    }
}

[HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
static class JobGiver_OptimizeApparel_TryGiveJob_Patch
{
    public static bool optimizeApparel = false;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    static void Prefix(Pawn pawn)
    {
        if (!optimizeApparel)
            return;

        pawn.mindState.nextApparelOptimizeTick = -1;
    }
}