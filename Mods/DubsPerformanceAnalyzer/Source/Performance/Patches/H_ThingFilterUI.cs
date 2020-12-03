using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Analyzer.Performance
{
    internal class H_ThingFilterUI : PerfPatch
    {
        public static bool Enabled = false;
        public override string Name => "performance.fixthingfilter";
        public override PerformanceCategory Category => PerformanceCategory.Optimizes;

        public override void OnEnabled(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(ThingFilterUI), nameof(ThingFilterUI.DoThingFilterConfigWindow)),
                new HarmonyMethod(typeof(H_ThingFilterUI), nameof(Prefix)));

            harmony.Patch(
                AccessTools.Method(typeof(Listing_TreeThingFilter), nameof(Listing_TreeThingFilter.DoCategoryChildren)),
                new HarmonyMethod(typeof(H_ThingFilterUI), nameof(DoCategoryChildren)));
        }

        private static Rect viewFrustum;



        public void DoCategoryChildren(Listing_TreeThingFilter __instance, TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool isRoot = false)
        {
            if (isRoot)
            {
                foreach (SpecialThingFilterDef sfDef in node.catDef.ParentsSpecialThingFilterDefs)
                {
                    if (__instance.Visible_NewTemp(sfDef, node))
                    {
                        __instance.DoSpecialFilter(sfDef, indentLevel);
                    }
                }
            }
            List<SpecialThingFilterDef> childSpecialFilters = node.catDef.childSpecialFilters;
            foreach (var t in childSpecialFilters)
            {
                if (__instance.Visible_NewTemp(t, node))
                {
                    __instance.DoSpecialFilter(t, indentLevel);
                }
            }
            foreach (TreeNode_ThingCategory node2 in node.ChildCategoryNodes)
            {
                if (__instance.Visible(node2))
                {
                    __instance.DoCategory(node2, indentLevel, openMask, map);
                }
            }
            foreach (ThingDef thingDef in node.catDef.childThingDefs.OrderBy(n => n.label))
            {
                if (__instance.Visible(thingDef))
                {
                    __instance.DoThingDef(thingDef, indentLevel, map);
                }
            }
        }



        public static bool Prefix(Rect rect, ref Vector2 scrollPosition, ThingFilter filter,
            ThingFilter parentFilter = null, int openMask = 1, IEnumerable<ThingDef> forceHiddenDefs = null,
            IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null, bool forceHideHitPointsConfig = false,
            List<ThingDef> suppressSmallVolumeTags = null, Map map = null)
        {
            if (!Enabled) return true;

            if (Event.current.type == EventType.Layout)
            {
                return false;
            }

            Widgets.DrawMenuSection(rect);
            Text.Font = GameFont.Tiny;
            var num = rect.width - 2f;
            var rect2 = new Rect(rect.x + 1f, rect.y + 1f, num / 2f, 24f);
            var exceptedDefs = forceHiddenDefs as ThingDef[] ?? forceHiddenDefs.ToArray();
            var specialThingFilterDefs = forceHiddenFilters as SpecialThingFilterDef[] ?? forceHiddenFilters.ToArray();
            if (rect2.Overlaps(viewFrustum) && Widgets.ButtonText(rect2, "ClearAll".Translate()))
            {
                filter.SetDisallowAll(exceptedDefs, specialThingFilterDefs);
                SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
            }

            var inline = new Rect(rect2.xMax + 1f, rect2.y, rect.xMax - 1f - (rect2.xMax + 1f), 24f);
            if (inline.Overlaps(viewFrustum) && Widgets.ButtonText(inline, "AllowAll".Translate()))
            {
                filter.SetAllowAll(parentFilter);
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            }

            Text.Font = GameFont.Small;
            rect.yMin = rect2.yMax;
            var node = ThingCategoryNodeDatabase.RootNode;
            var flag = true;
            var flag2 = true;
            if (parentFilter != null)
            {
                node = parentFilter.DisplayRootCategory;
                flag = parentFilter.allowedHitPointsConfigurable;
                flag2 = parentFilter.allowedQualitiesConfigurable;
            }

            var viewRect = new Rect(0f, 0f, rect.width - 16f, ThingFilterUI.viewHeight);

            viewFrustum = rect.AtZero(); // Our view frustum starts at 0,0 from the rect we are given
            viewFrustum.y += scrollPosition.y; // adjust our view frustum vertically based on the scroll position

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            var num2 = 2f;
            if (flag && !forceHideHitPointsConfig)
            {
                ThingFilterUI.DrawHitPointsFilterConfig(ref num2, viewRect.width, filter);
            }

            if (flag2)
            {
                ThingFilterUI.DrawQualityFilterConfig(ref num2, viewRect.width, filter);
            }

            var num3 = num2;
            var rect3 = new Rect(0f, num2, viewRect.width, 9999f);
            var listing_TreeThingFilter = new Listing_TreeThingFilter(filter, parentFilter, exceptedDefs, specialThingFilterDefs, suppressSmallVolumeTags);
            listing_TreeThingFilter.Begin(rect3);
            listing_TreeThingFilter.DoCategoryChildren(node, 0, openMask, map, true);
            listing_TreeThingFilter.End();
           // if (Event.current.type == EventType.Layout)
            {
                ThingFilterUI.viewHeight = num3 + listing_TreeThingFilter.CurHeight + 90f;
            }

            Widgets.EndScrollView();


            return false;
        }
    }
}