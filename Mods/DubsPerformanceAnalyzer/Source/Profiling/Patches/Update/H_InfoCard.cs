using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.infocard", Category.Update)]
    internal class H_InfoCard
    {
        public static bool Active = false;

        public static IEnumerable<MethodInfo> GetPatchMethods()
        {
            yield return AccessTools.Method(typeof(Dialog_InfoCard), nameof(Dialog_InfoCard.DoWindowContents));
            yield return AccessTools.Method(typeof(Dialog_InfoCard), nameof(Dialog_InfoCard.FillCard));
            yield return AccessTools.Method(typeof(Dialog_InfoCard), nameof(Dialog_InfoCard.DefsToHyperlinks), new[] { typeof(IEnumerable<ThingDef>) });
            yield return AccessTools.Method(typeof(Dialog_InfoCard), nameof(Dialog_InfoCard.DefsToHyperlinks), new[] { typeof(IEnumerable<DefHyperlink>) });
            yield return AccessTools.Method(typeof(Dialog_InfoCard), nameof(Dialog_InfoCard.TitleDefsToHyperlinks));
            yield return AccessTools.Method(typeof(ThingDef), nameof(ThingDef.SpecialDisplayStats));

            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.StatsToDraw), new[] { typeof(Def), typeof(ThingDef) });
            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.StatsToDraw), new[] { typeof(RoyalTitleDef), typeof(Faction) });
            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.StatsToDraw), new[] { typeof(Faction) });
            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.StatsToDraw), new[] { typeof(AbilityDef) });
            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.StatsToDraw), new[] { typeof(Thing) });
            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.StatsToDraw), new[] { typeof(WorldObject) });

            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.DrawStatsWorker));
            yield return AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.FinalizeCachedDrawEntries));
        }

        public static bool FUUUCK(Rect rect, Thing thing)
        {
            if (!Active) return true;

            Profiler prof = null;
            if (StatsReportUtility.cachedDrawEntries.NullOrEmpty<StatDrawEntry>())
            {
                prof = ProfileController.Start("SpecialDisplayStats");
                StatsReportUtility.cachedDrawEntries.AddRange(thing.def.SpecialDisplayStats(StatRequest.For(thing)));
                prof.Stop();

                prof = ProfileController.Start("StatsToDraw");
                StatsReportUtility.cachedDrawEntries.AddRange(StatsReportUtility.StatsToDraw(thing).Where(s => s.ShouldDisplay));
                prof.Stop();

                prof = ProfileController.Start("RemoveAll");
                StatsReportUtility.cachedDrawEntries.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
                prof.Stop();

                prof = ProfileController.Start("FinalizeCachedDrawEntries");
                StatsReportUtility.FinalizeCachedDrawEntries(StatsReportUtility.cachedDrawEntries);
                prof.Stop();
            }
            prof = ProfileController.Start("DrawStatsWorker");
            StatsReportUtility.DrawStatsWorker(rect, thing, null);
            prof.Stop();

            return false;
        }

        public static void ProfilePatch()
        {
            MethodTransplanting.PatchMethods(typeof(H_InfoCard));

            Modbase.Harmony.Patch( AccessTools.Method(typeof(StatsReportUtility), nameof(StatsReportUtility.DrawStatsReport), new[] { typeof(Rect), typeof(Thing) }),
                new HarmonyMethod(typeof(H_InfoCard), nameof(FUUUCK)));
        }
    }
}