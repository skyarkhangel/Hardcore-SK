using System;
using System.Diagnostics;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan
{
    public class Alert_StatsLearning : Alert
    {
        private bool active = false;

        private float activeSeconds;
        private float activeLastTick;

        private string explanation = string.Empty;

        private readonly Stopwatch updateGUIStopwatch = new Stopwatch();

        // Start color is Green
        // 0.329, 0.964, 0.015
        private const float R0 = 0.329f;
        private const float G0 = 0.964f;
        private const float B0 = 0.015f;

        // End color is Yellow
        // 0.964, 0.811, 0.015
        private const float R1 = 0.964f;
        private const float G1 = 0.811f;
        private const float B1 = 0.015f;

        private const float MaxActiveMinutes = 45;

        private float MinutesActive
        {
            get => activeSeconds / 60f;
        }

        private float SecondsSinceGUIUpdate
        {
            get => updateGUIStopwatch.ElapsedMilliseconds / 1000f;
        }

        private Color color = Color.green;

        public override Color BGColor
        {
            get => color;
        }

        public override AlertPriority Priority
        {
            get => AlertPriority.Medium;
        }

        public Alert_StatsLearning() : base()
        {
            base.defaultLabel = KeyedResources.RocketMan_Alert_StatsLearning_Label;
            base.defaultExplanation = KeyedResources.RocketMan_Alert_StatsLearning_Explanation;
        }

        public override void OnClick()
        {
            base.OnClick();
            if (Find.WindowStack.WindowOfType<Window_Main>() == null)
            {
                Find.WindowStack.Add(Finder.RocketManWindow == null ? Finder.RocketManWindow = new Window_Main() : Finder.RocketManWindow);
            }
        }

        public override AlertReport GetReport()
        {
            if (!RocketPrefs.Learning || !RocketPrefs.LearningAlertEnabled)
            {
                if (active)
                {
                    activeSeconds = 0;
                    activeLastTick = -1;
                    updateGUIStopwatch.Reset();
                    active = false;
                }
                return AlertReport.Inactive;
            }
            if (!active)
            {
                active = true;
                activeSeconds = 0;
                activeLastTick = GenTicks.TicksGame;
                UpdateGUI();
                updateGUIStopwatch.Restart();
            }
            activeSeconds += ((float)GenTicks.TicksGame - activeLastTick) / 60f;
            activeLastTick = GenTicks.TicksGame;
            if (SecondsSinceGUIUpdate > 21f)
            {
                UpdateGUI();
                updateGUIStopwatch.Restart();
            }
            if (MinutesActive > MaxActiveMinutes)
            {
                RocketPrefs.Learning = false;
                RocketMod.Instance.WriteSettings();
                return AlertReport.Inactive;
            }
            return AlertReport.Active;
        }

        public override TaggedString GetExplanation()
        {
            return explanation;
        }

        private void UpdateGUI()
        {
            float progress = activeSeconds / (MaxActiveMinutes * 60f);

            this.color = new Color(Mathf.Lerp(R0, R1, progress), Mathf.Lerp(G0, G1, progress), Mathf.Lerp(B0, B1, progress), 0.5f);
            this.explanation = KeyedResources.RocketMan_Alert_StatsLearning_Explanation.Formatted(MinutesActive, MaxActiveMinutes);
        }
    }
}
