using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.AI.Group;

namespace Analyzer.Profiling
{
    [Entry("entry.update.mapcomponent", Category.Update)]
    internal class H_MapComponentUpdate
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            foreach (var meth in Utility.SubClassNonAbstractImplementationsOf(typeof(MapComponent), t => t.Name == "MapComponentUpdate"))
                yield return meth;

            yield return AccessTools.Method(typeof(SkyManager), nameof(SkyManager.SkyManagerUpdate));
            yield return AccessTools.Method(typeof(PowerNetManager), nameof(PowerNetManager.UpdatePowerNetsAndConnections_First));
            yield return AccessTools.Method(typeof(RegionGrid), nameof(RegionGrid.UpdateClean));
            yield return AccessTools.Method(typeof(RegionAndRoomUpdater), nameof(RegionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms));
            yield return AccessTools.Method(typeof(GlowGrid), nameof(GlowGrid.GlowGridUpdate_First));
            yield return AccessTools.Method(typeof(LordManager), nameof(LordManager.LordManagerUpdate));
            yield return AccessTools.Method(typeof(AreaManager), nameof(AreaManager.AreaManagerUpdate));
            yield return AccessTools.Method(typeof(MapDrawer), nameof(MapDrawer.WholeMapChanged));
            yield return AccessTools.Method(typeof(MapDrawer), nameof(MapDrawer.MapMeshDrawerUpdate_First));
            yield return AccessTools.Method(typeof(MapDrawer), nameof(MapDrawer.DrawMapMesh));
            yield return AccessTools.Method(typeof(DynamicDrawManager), nameof(DynamicDrawManager.DrawDynamicThings));
            yield return AccessTools.Method(typeof(GameConditionManager), nameof(GameConditionManager.GameConditionManagerDraw));
            yield return AccessTools.Method(typeof(DesignationManager), nameof(DesignationManager.DrawDesignations));
            yield return AccessTools.Method(typeof(OverlayDrawer), nameof(OverlayDrawer.DrawAllOverlays));
        }

        public static string GetLabel(object __instance) => __instance.GetType().Name;

    }
}