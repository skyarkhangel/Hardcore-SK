using System;
using RimWorld;

namespace Soyuz.Patches
{
    public static class SkillRecord_Patch
    {
        [SoyuzPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
        public static class Learn_Patch
        {
            public static void Prefix(SkillRecord __instance, ref float xp)
            {
                if (__instance.pawn.IsBeingThrottled())
                    xp = __instance.pawn.GetTimeDelta() * xp;
            }
        }
    }
}
