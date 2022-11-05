using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Messaging;
using HarmonyLib;
using RimWorld;
using RocketMan;
using Verse;

namespace Proton
{
    public static class AlertsReadout_Constructor_Patch
    {
        public static MethodBase mAlertsReadout = AccessTools.Constructor(typeof(AlertsReadout));

        public static MethodBase mPostfix = AccessTools.Method(typeof(AlertsReadout_Constructor_Patch), nameof(AlertsReadout_Constructor_Patch.Postfix));

        [Main.OnInitialization]
        public static void Patch()
        {
            Finder.Harmony.Patch(mAlertsReadout, postfix: new HarmonyMethod((MethodInfo)mPostfix));
        }

        public static void Postfix(AlertsReadout __instance)
        {
            bool shouldSave = false;
            int index = 0;
            Context.Alerts = __instance.AllAlerts.ToArray();
            Context.AlertSettingsByIndex = new AlertSettings[Context.Alerts.Length];            
            Context.ReadoutInstance = __instance;
            foreach (Alert alert in Context.Alerts)
            {
                string id = alert.GetType().FullName;
                if (!Context.TypeIdToSettings.TryGetValue(id, out AlertSettings settings))
                {
                    settings = new AlertSettings(id);
                    Context.TypeIdToSettings[id] = settings;
                    shouldSave = true;
                }
                Context.AlertSettingsByIndex[index] = settings;
                Context.AlertToSettings[alert] = settings;
                settings.alert = alert;
                index++;
            }
            if (shouldSave)            
                RocketMod.Instance.WriteSettings();            
        }
    }

    [ProtonPatch(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutOnGUI))]
    public static class AlertsReadout_AlertsReadoutOnGUI_Patch
    {
        private static FieldInfo fEnabled = AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.Enabled));

        private static FieldInfo fAlertThrottling = AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.AlertThrottling));

        private static FieldInfo fAlertsDisabled = AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.DisableAllAlert));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeInstruction[] codes = instructions.ToArray();
            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldsfld, fEnabled);
            yield return new CodeInstruction(OpCodes.Brfalse_S, l1);

            yield return new CodeInstruction(OpCodes.Ldsfld, fAlertThrottling);
            yield return new CodeInstruction(OpCodes.Brfalse_S, l1);

            yield return new CodeInstruction(OpCodes.Ldsfld, fAlertsDisabled);
            yield return new CodeInstruction(OpCodes.Brtrue_S, l2);

            yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(UnityEngine.Time), nameof(UnityEngine.Time.frameCount)));
            yield return new CodeInstruction(OpCodes.Ldc_I4_S, 30);
            yield return new CodeInstruction(OpCodes.Rem);
            yield return new CodeInstruction(OpCodes.Brtrue_S, l1);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutOnGUI_Patch), nameof(AlertsReadout_AlertsReadoutOnGUI_Patch.CheckAllActiveAlerts)));

            if (codes[0].labels == null)
            {
                codes[0].labels = new List<Label>();
            }
            codes[0].labels.Add(l1);
            foreach (CodeInstruction code in codes)
            {
                if (code.opcode == OpCodes.Ret)
                {
                    if (code.labels == null)
                    {
                        code.labels = new List<Label>();
                    }
                    code.labels.Add(l2);
                }
                yield return code;
            }
        }

        private static void CheckAllActiveAlerts(AlertsReadout readoutInstance)
        {
            if (false
                || !RocketPrefs.AlertThrottling
                || !RocketPrefs.Enabled)
                return;
            if (RocketPrefs.DisableAllAlert && readoutInstance.activeAlerts.Count > 0)
            {
                readoutInstance.activeAlerts.Clear();
                foreach (Alert alert in readoutInstance.AllAlerts)
                    alert.cachedActive = false;
                return;
            }
            List<Alert> removalList = new List<Alert>();
            for (int i = 0; i < readoutInstance.activeAlerts.Count; i++)
            {
                Alert alert = readoutInstance.activeAlerts[i];
                if (Context.AlertToSettings.TryGetValue(alert, out AlertSettings settings) && !settings.Enabled)
                {
                    settings.UpdateAlert(removeReadout: false);
                    removalList.Add(alert);
                }
            }
            if (removalList.Count == 0)
                return;
            foreach (Alert alert in removalList)
                readoutInstance.activeAlerts.Remove(alert);
        }
    }

    [ProtonPatch(typeof(AlertsReadout), nameof(AlertsReadout.AlertsReadoutUpdate))]
    public static class AlertsReadout_AlertsReadoutUpdate_Patch
    {
        private static bool toggleBit = false;

        private static FieldInfo fToggleBit = AccessTools.Field(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.toggleBit));

        private static FieldInfo fEnabled = AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.Enabled));

        private static FieldInfo fAlertThrottling = AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.AlertThrottling));

        private static FieldInfo fAlertsDisabled = AccessTools.Field(typeof(RocketPrefs), nameof(RocketPrefs.DisableAllAlert));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            bool finished = false;
            CodeInstruction code;
            CodeInstruction[] codes = instructions.ToArray();
            MethodBase mCheckAddOrRemoveAlert = AccessTools.Method(typeof(AlertsReadout), nameof(AlertsReadout.CheckAddOrRemoveAlert));

            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();
            Label l3 = generator.DefineLabel();
            Label l4 = generator.DefineLabel();
            Label l5 = generator.DefineLabel();
            Label l6 = generator.DefineLabel();
            Label l7 = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Ldsfld, fEnabled);
            yield return new CodeInstruction(OpCodes.Brfalse_S, l7);

            yield return new CodeInstruction(OpCodes.Ldsfld, fAlertThrottling);
            yield return new CodeInstruction(OpCodes.Brfalse_S, l7);

            yield return new CodeInstruction(OpCodes.Ldsfld, fAlertsDisabled);
            yield return new CodeInstruction(OpCodes.Brtrue_S, l6);

            code = codes[0];
            if (code.labels == null)
            {
                code.labels = new List<Label>();
            }
            code.labels.Add(l7);
            for (int i = 0; i < codes.Length; i++)
            {
                code = codes[i];
                if (code.opcode == OpCodes.Ret)
                {
                    if (code.labels == null)
                    {
                        code.labels = new List<Label>();
                    }
                    code.labels.Add(l6);
                }
                if (code.opcode == OpCodes.Ldc_I4_S && code.OperandIs(24))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.GetCount)));
                    continue;
                }
                if (!finished && code.OperandIs(mCheckAddOrRemoveAlert))
                {
                    finished = true;
                    yield return new CodeInstruction(OpCodes.Ldsfld, fEnabled);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, l5);

                    yield return new CodeInstruction(OpCodes.Ldsfld, fAlertThrottling);
                    yield return new CodeInstruction(OpCodes.Brfalse_S, l5);

                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.ShouldUpdate)));

                    yield return new CodeInstruction(OpCodes.Brtrue_S, l2);

                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Br_S, l1);

                    yield return new CodeInstruction(OpCodes.Ldloc_0) { labels = new List<Label>() { l2 } };
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.StartProfiling)));

                    yield return new CodeInstruction(code.opcode, code.operand);

                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlertsReadout_AlertsReadoutUpdate_Patch), nameof(AlertsReadout_AlertsReadoutUpdate_Patch.StopProfiling)));

                    yield return new CodeInstruction(OpCodes.Br_S, l1);

                    yield return new CodeInstruction(code.opcode, code.operand) { labels = new List<Label>() { l5 } };

                    if (codes[i + 1].labels == null)
                    {
                        codes[i + 1].labels = new List<Label>();
                    }
                    codes[i + 1].labels.Add(l1);
                    continue;
                }
                yield return code;
            }
        }

        private static readonly Stopwatch stopwatch = new Stopwatch();

        private static bool ShouldUpdate(int index)
        {
            if (RocketPrefs.DisableAllAlert)
            {
                return false;
            }
            if (index < 0 || index >= Context.Alerts.Length)
            {
                return true;
            }
            AlertSettings settings = Context.AlertSettingsByIndex[index];
            if (settings != null)
            {
                return settings.Enabled && settings.ShouldUpdate;
            }
            return true;
        }

        private static void StartProfiling(int index)
        {
            stopwatch.Restart();
        }

        private static void StopProfiling(int index)
        {
            if (index >= 0 && index < Context.Alerts.Length)
            {
                Context.AlertSettingsByIndex[index]?.UpdatePerformanceMetrics((float)stopwatch.ElapsedTicks * 1000.0f / (float)Stopwatch.Frequency);
                stopwatch.Stop();
            }
        }

        private static int GetCount(AlertsReadout readout) => (int)Math.Max(readout.AllAlerts.Count * 0.75f, 24);
    }
}
