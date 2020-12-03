using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Analyzer.Profiling;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Analyzer.Performance
{
    internal class H_AlertsReadoutUpdate : PerfPatch
    {
        public static bool Enabled = true;

        public override PerformanceCategory Category => PerformanceCategory.Removes;
        // need this so it doesn't error when searching
        // and so that the `OnEnabled` function is called.

        public static bool OverrideAlerts = false;
        public static bool DisableAlerts = false;

        private static Dictionary<Type, bool> alertFilter = new Dictionary<Type, bool>();
        public static Dictionary<Type, bool> AlertFilter => alertFilter ??= new Dictionary<Type, bool>();

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref alertFilter, "alertfilter");
            Scribe_Values.Look(ref OverrideAlerts, "overrideAlerts");
            Scribe_Values.Look(ref DisableAlerts, "disableAlerts");
        }

        public override void Draw(Listing_Standard listing)
        {
            var name = "performance.alertsoverride".TranslateSimple();
            var tooltip = ("performance.alertsoverride" + ".tooltip").TranslateSimple();

            var height = Mathf.CeilToInt((name.GetWidthCached() + 30) / listing.ColumnWidth) * Text.LineHeight;
            var rect = listing.GetRect(height);

            DubGUI.Checkbox(rect, name, ref OverrideAlerts);
            TooltipHandler.TipRegion(rect, tooltip);

            name = "performance.alertsdisable".TranslateSimple();
            tooltip = ("performance.alertsdisable" + ".tooltip").TranslateSimple();
            height = Mathf.CeilToInt((name.GetWidthCached() + 30) / listing.ColumnWidth) * Text.LineHeight;
            rect = listing.GetRect(height);

            DubGUI.Checkbox(rect, name, ref DisableAlerts);
            TooltipHandler.TipRegion(rect, tooltip);
        }

        public override void OnEnabled(Harmony harmony)
        {
            var biff = new HarmonyMethod(typeof(H_AlertsReadoutUpdate), nameof(CheckAddOrRemoveAlert));
            var skiff = AccessTools.Method(typeof(AlertsReadout), nameof(AlertsReadout.CheckAddOrRemoveAlert));
            harmony.Patch(skiff, biff);

            var skiff2 = AccessTools.Method(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutOnGUI));
            harmony.Patch(skiff2, new HarmonyMethod(typeof(H_AlertsReadoutUpdate), nameof(AlertsReadoutOnGUI)));
        }

        [Entry("entry.update.alert", Profiling.Category.Update)]
        public class AlertRecalculate
        {
            public static bool Active = false;
            public static MethodInfo recalculate = AccessTools.Method(typeof(Alert), nameof(Alert.Recalculate));

            public static IEnumerable<MethodInfo> GetPatchMethods() 
            {
                yield return recalculate;
            }

            public static string GetName(Alert __instance) => __instance.GetType().Name;
            public static string GetLabel(Alert __instance) => __instance.GetType().FullName;
            public static Type GetType(Alert __instance) => __instance.GetType();

            public static void Checkbox(ProfileLog log)
            {
                if (AlertFilter.TryGetValue(log.type, out var value)) AlertFilter[log.type] = !value;
                else AlertFilter.Add(log.type, true);
            }

            public static bool Selected(Profiler _, ProfileLog log) => !(AlertFilter.TryGetValue(log.type, out var active) && active);
        }


        public static bool CheckAddOrRemoveAlert(AlertsReadout __instance, Alert alert, bool forceRemove)
        {
            if (DisableAlerts) return false;
            if (!OverrideAlerts) return true;

            try
            {
                var alertActive = false;
                var alertType = alert.GetType();

                if (AlertFilter.TryGetValue(alertType, out var active))
                {
                    if (!active)
                    {
                        alert.Recalculate();
                        alertActive = alert.Active;
                    }
                    else // We ensure alerts show up so you can re-enable disabled alerts.
                    {
                        if (AlertRecalculate.Active)
                        {
                            var prof = ProfileController.Start(alertType.Name, () => alertType.FullName, alertType, null, null, AlertRecalculate.recalculate);

                            prof?.Stop();
                        }
                    }
                }
                else
                {
                    alert.Recalculate();
                    alertActive = alert.Active;
                }

                if (!forceRemove && alertActive)
                {
                    if (!__instance.activeAlerts.Contains(alert))
                    {
                        __instance.activeAlerts.Add(alert);
                        alert.Notify_Started();
                    }
                }
                else
                {
                    __instance.activeAlerts.Remove(alert);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("Exception processing alert " + alert.ToString() + ": " + ex.ToString(), 743575, false);
                __instance.activeAlerts.Remove(alert);
            }

            return false;
        }

        public static bool AlertsReadoutOnGUI(AlertsReadout __instance)
        {

            if (DisableAlerts) return false;
            if (!OverrideAlerts) return true;

            if (Event.current.type == EventType.Layout || Event.current.type == EventType.MouseDrag) return false;
            if (__instance.activeAlerts.Count == 0) return false;

            Alert alert = null;
            var alertPriority = AlertPriority.Critical;
            var flag = false;
            var num = Find.LetterStack.LastTopY - __instance.activeAlerts.Count * 28f;
            var rect = new Rect(UI.screenWidth - 154f, num, 154f, __instance.lastFinalY - num);
            var num2 = GenUI.BackgroundDarkAlphaForText();
            if (num2 > 0.001f)
            {
                GUI.color = new Color(1f, 1f, 1f, num2);
                Widgets.DrawShadowAround(rect);
                GUI.color = Color.white;
            }

            var num3 = num;
            if (num3 < 0f)
            {
                num3 = 0f;
            }

            for (var i = 0; i < __instance.PriosInDrawOrder.Count; i++)
            {
                var alertPriority2 = __instance.PriosInDrawOrder[i];
                for (var j = 0; j < __instance.activeAlerts.Count; j++)
                {
                    var alert2 = __instance.activeAlerts[j];
                    if (alert2.Priority == alertPriority2)
                    {
                        if (!flag)
                        {
                            alertPriority = alertPriority2;
                            flag = true;
                        }

                        var key = alert2.GetType();
                        var rect2 = alert2.DrawAt(num3, alertPriority2 != alertPriority);

                        if (Mouse.IsOver(rect2))
                        {
                            alert = alert2;
                            __instance.mouseoverAlertIndex = j;
                        }

                        num3 += rect2.height;
                    }
                }
            }

            __instance.lastFinalY = num3;
            UIHighlighter.HighlightOpportunity(rect, "Alerts");
            if (alert != null)
            {
                alert.DrawInfoPane();
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Alerts, KnowledgeAmount.FrameDisplayed);
                __instance.CheckAddOrRemoveAlert(alert, false);
            }

            return false;
        }

    }
}