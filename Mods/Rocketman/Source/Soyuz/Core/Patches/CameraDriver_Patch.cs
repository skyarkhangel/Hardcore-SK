using HarmonyLib;
using RocketMan;
using Verse;

namespace Soyuz.Patches
{
    [SoyuzPatch(typeof(CameraDriver), nameof(CameraDriver.Update))]
    public class CameraDriver_Patch
    {
        public static void Postfix(CameraDriver __instance)
        {
            Context.ZoomRange = __instance.CurrentZoom;
            Context.CurViewRect = __instance.CurrentViewRect;
            if (RocketDebugPrefs.Debug && RocketDebugPrefs.StatLogging)
                Log.Message($"SOYUZ: Zoom range is {Context.ZoomRange}");
        }
    }
}