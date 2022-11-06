using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using RocketMan;
using RocketMan.Tabs;
using Verse;

namespace Proton
{
    public class AlertSettings : IExposable
    {
        private int counter;
        private float avgT;
        private Stopwatch stopwatch = new Stopwatch();

        public string typeId;
        public Alert alert;
        public bool enabledInt = true;
        public bool ignored;

        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => enabledInt && (avgT < Context.Settings.executionTimeLimit || counter < 15 || ignored);
            set
            {
                enabledInt = value;
                if (!value)
                {
                    UpdateAlert(removeReadout: true);
                }
            }
        }

        public float AverageExecutionTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => avgT;
        }

        public float TimeSinceLastExecution
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)Math.Round((float)(stopwatch?.ElapsedTicks ?? 0) / Stopwatch.Frequency, 3);
        }

        public bool ShouldUpdate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!enabledInt)
                    return false;
                if (counter < 15)
                    return true;
                if (ignored)
                    return true;
                if (avgT >= Context.Settings.executionTimeLimit)
                    return false;
                float elapsedSeconds = ((float)stopwatch.ElapsedTicks / Stopwatch.Frequency);
                if (avgT > 2.5f)
                    return elapsedSeconds > Math.Min(30f * (avgT - 1.5f), 60);
                if (elapsedSeconds <= Context.Settings.minInterval)
                    return false;
                if (elapsedSeconds >= 25f)
                    return true;
                return 10f * avgT <= elapsedSeconds / 4.0f;
            }
        }

        public AlertSettings()
        {
        }

        public AlertSettings(string typeId)
        {
            this.typeId = typeId;
            this.Setup();
        }

        public void UpdatePerformanceMetrics(float t)
        {
            avgT = avgT * 0.9f + 0.1f * t;
            counter++;
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            if (!ignored && counter > 30 && avgT > Context.Settings.executionTimeLimit)
            {
                enabledInt = false;
                UpdateAlert();
            }
            stopwatch.Restart();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref ignored, "ignore", false);
            Scribe_Values.Look(ref typeId, "typeId");
            Scribe_Values.Look(ref enabledInt, "enabled2", true);
            Scribe_Values.Look(ref avgT, "avgT", 0.05f);
        }

        public void UpdateAlert(bool removeReadout = true)
        {
            if (Enabled)
                return;
            if (alert != null)
            {
                if (removeReadout)
                    Context.ReadoutInstance.activeAlerts.Remove(alert);
                alert.cachedActive = false;
                return;
            }
            for (int i = 0; i < Context.Alerts.Length; i++)
            {
                if (Context.AlertSettingsByIndex[i] == this && Context.Alerts[i] != null)
                {
                    Alert alert = Context.Alerts[i];
                    Context.AlertToSettings[alert] = this;
                    if (removeReadout)
                        Context.ReadoutInstance.activeAlerts.Remove(alert);
                    alert.cachedActive = false;
                }
            }
        }

        private void Setup()
        {
            string temp = typeId.ToLower();
            if (temp.Contains("lowfood"))
                ignored = true;
            if (temp.Contains("majororextreem"))
                ignored = true;
            if (temp.Contains("extreem"))
                ignored = true;
            if (temp.Contains("needdoctor"))
                ignored = true;
            if (temp.Contains("starvation"))
                ignored = true;
            if (temp.Contains("lifethreateninghediff"))
                ignored = true;
            if (temp.Contains("hypothermia"))
                ignored = true;
            if (temp.Contains("heatstroke"))
                ignored = true;
        }
    }
}