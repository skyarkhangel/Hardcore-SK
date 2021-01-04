using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Analyzer.Performance
{
    public class H_ZoomThrottle : PerfPatch
    {
        public static bool Enabled = false;
        public override string Name => "performance.zoom";
        private static float panNorm = 0.0f;
        private static float nMinusCameraScale = 0.0f;

        public override void OnEnabled(Harmony harmony)
        {
            var tickPost = new HarmonyMethod(typeof(H_ZoomThrottle), nameof(TickManager_Postfix));
            var driverDollyPost = new HarmonyMethod(typeof(H_ZoomThrottle), nameof(CameraDriver_Dolly_Postfix));
            var driverOnGuiPost = new HarmonyMethod(typeof(H_ZoomThrottle), nameof(CameraDriver_OnGUI_Postfix));

            harmony.Patch(AccessTools.PropertyGetter(typeof(TickManager), nameof(TickManager.TickRateMultiplier)), null, tickPost);
            harmony.Patch(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.CalculateCurInputDollyVect)), null, driverDollyPost);
            harmony.Patch(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.CameraDriverOnGUI)), null, driverOnGuiPost);
        }

        static void TickManager_Postfix(ref float __result)
        {
            if (!Enabled) return;


            if (Find.CameraDriver.CurrentViewRect.Area != nMinusCameraScale)
            {
                nMinusCameraScale = Find.CameraDriver.CurrentViewRect.Area;

                switch (Find.CameraDriver.CurrentZoom)
                {
                    case CameraZoomRange.Furthest: panNorm += 150f; break;
                    case CameraZoomRange.Far: panNorm += 80f; break;
                    case CameraZoomRange.Middle: panNorm += 40f; break;
                    case CameraZoomRange.Close: panNorm += 5f; break;
                }
            }

            if (panNorm <= 15f) return;

            switch(Find.TickManager.CurTimeSpeed)
            {
                case TimeSpeed.Fast: __result = Mathf.Max(2.0f - panNorm / 128f, 1.0f); break;
                case TimeSpeed.Superfast: __result = Mathf.Max(3.0f - panNorm / 96f, 1.5f); break;
                case TimeSpeed.Ultrafast: __result = Mathf.Max(4.0f - panNorm / 64f, 2.0f); break;
            }
        }        
        
        static void CameraDriver_Dolly_Postfix(ref Vector2 __result)
        {
            if (!Enabled) return;

            panNorm = __result.magnitude;
        }
        
        static void CameraDriver_OnGUI_Postfix(CameraDriver __instance)
        {
            if (!Enabled) return;

            if (Find.Camera == null || Find.TickManager.Paused || Find.TickManager.NotPlaying)
                return;

            var modifer = 0f;

            switch(Find.CameraDriver.CurrentZoom)
            {
                case CameraZoomRange.Furthest: modifer = 150f; break;
                case CameraZoomRange.Far: modifer = 80f; break;
                case CameraZoomRange.Middle: modifer = 40f; break;
                case CameraZoomRange.Close: modifer = 5f; break;
            }

            if (KeyBindingDefOf.MapDolly_Left.IsDown || KeyBindingDefOf.MapDolly_Up.IsDown || KeyBindingDefOf.MapDolly_Right.IsDown || KeyBindingDefOf.MapDolly_Down.IsDown)
            {
                panNorm = modifer;
            }
            else if (KeyBindingDefOf.MapZoom_Out.KeyDownEvent || KeyBindingDefOf.MapZoom_In.KeyDownEvent)
            {
                panNorm = modifer;
            }
        }
    }

}