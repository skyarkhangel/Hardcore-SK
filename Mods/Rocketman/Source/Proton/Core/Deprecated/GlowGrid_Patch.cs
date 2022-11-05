using System;
using RocketMan;
using Verse;

namespace Proton
{
    public static class GlowGrid_Patch
    {
        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.RecalculateAllGlow))]
        public static class RecalculateAllGlow_Patch
        {
            public static bool Prefix(GlowGrid __instance)
            {
                if (Current.ProgramState != ProgramState.Playing)
                {
                    return false;
                }
                if (__instance.initialGlowerLocs != null)
                {
                    foreach (IntVec3 initialGlowerLoc in __instance.initialGlowerLocs)
                    {
                        __instance.MarkGlowGridDirty(initialGlowerLoc);
                    }
                    __instance.initialGlowerLocs = null;
                }
                __instance.map.GetGlowerCacher().Recalculate();
                return false;
            }
        }

        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.RegisterGlower))]
        public static class RegisterGlower_Patch
        {
            public static void Prefix(GlowGrid __instance, CompGlower newGlow)
            {
                __instance.map?.GetGlowerCacher()?.Register(newGlow);

                if (RocketDebugPrefs.DrawGlowerUpdates)
                    Log.Message($"PROTON: Registering glower {newGlow.parent}");
            }
        }

        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.DeRegisterGlower))]
        public static class DeRegisterGlower_Patch
        {
            public static void Prefix(GlowGrid __instance, CompGlower oldGlow)
            {
                __instance.map?.GetGlowerCacher()?.DeRegister(oldGlow);

                if (RocketDebugPrefs.DrawGlowerUpdates)
                    Log.Message($"PROTON: DeRegistering glower {oldGlow.parent}");
            }
        }

        [ProtonPatch(typeof(GlowGrid), nameof(GlowGrid.MarkGlowGridDirty))]
        public static class MarkGlowGridDirty_Patch
        {
            public static void Prefix(GlowGrid __instance, IntVec3 loc)
            {
                __instance.map?.GetGlowerCacher()?.Notify_DirtyAt(loc);

                if (RocketDebugPrefs.DrawGlowerUpdates)
                    __instance.map.debugDrawer.FlashCell(loc, 1.0f, ">_+_<");
            }
        }
    }
}
