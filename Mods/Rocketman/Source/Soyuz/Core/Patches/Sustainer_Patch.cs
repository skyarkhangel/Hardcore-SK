using System;
using Verse;
using Verse.Sound;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(Sustainer), nameof(Sustainer.SustainerUpdate))]
    public class Sustainer_Update_Patch
    {
        public static void Prefix(Sustainer __instance)
        {
            if (!(__instance?.info.Maker.HasThing ?? false) || __instance.info.Maintenance != MaintenanceType.PerTick)
                return;

            if (__instance.info.Maker.Thing is Pawn pawn && !pawn.Destroyed && pawn.Spawned && pawn.IsBeingThrottled())
            {
                float deltaT = pawn.GetTimeDelta();
                if (deltaT > 1 && deltaT < 22)
                    __instance.lastMaintainTick = GenTicks.TicksGame;
            }
        }
    }
}
