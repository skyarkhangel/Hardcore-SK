using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace Revolus.AnimalsAreFun {
    public class AnimalsAreFun : Mod {
        public const bool MessageInDevModeDefault = false;
        public const bool MessageAlwaysDefault = false;

        public static bool MessageInDevMode = MessageInDevModeDefault;
        public static bool MessageAlways = MessageAlwaysDefault;
        
        private const float lineHeight = 24f;

        public AnimalsAreFun(ModContentPack content) : base(content) {
            _ = this.GetSettings<Settings>();
        }

        public static void Debug(string message, [CallerLineNumberAttribute] int line = 0, [CallerMemberName] string caller = null) {
            if (MessageAlways || MessageInDevMode && Prefs.DevMode) {
                Log.Message($"[AnimalsAreFun @ {caller}:{line}] {message}");
            }
        }

        private float MinConsciousnessFloatBuffer, MinMovingFloatBuffer, MaxSizeFloatBuffer, MaxWildnessFloatBuffer;
        private string MinConsciousnessStringBuffer, MinMovingStringBuffer, MaxSizeStringBuffer, MaxWildnessStringBuffer;

        public override void DoSettingsWindowContents(Rect inRect) {
            base.DoSettingsWindowContents(inRect);

            var oldFont = Text.Font;
            var oldAnchor = Text.Anchor;
            try {
                var listing = new Listing_Standard();
                listing.Begin(inRect);

                try {
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleLeft;
                    
                    Widgets.Label(listing.GetRect(lineHeight), "Requirements:");

                    FloatTimes100Value(
                        ref this.MinConsciousnessFloatBuffer, ref this.MinConsciousnessStringBuffer, 10, 100,
                        ref Common.MinConsciousness, "Min. consciousness (%):", listing
                    );
                    FloatTimes100Value(
                        ref this.MinMovingFloatBuffer, ref this.MinMovingStringBuffer, 10, 100,
                        ref Common.MinMoving, "Min. moving (%):", listing
                    );
                    FloatTimes100Value(
                        ref this.MaxSizeFloatBuffer, ref this.MaxSizeStringBuffer, 1, 500,
                        ref Common.MaxBodySize, "Max. body size (cm):", listing
                    );
                    FloatTimes100Value(
                        ref this.MaxWildnessFloatBuffer, ref this.MaxWildnessStringBuffer, 10, 100,
                        ref Common.MaxWildness, "Max. wildness (%):", listing
                    );

                    var row = listing.GetRect(lineHeight);
                    Widgets.DrawHighlightIfMouseover(row);
                    Widgets.CheckboxLabeled(
                        row.ContractedBy(1f).Rounded(),
                        "Must be cute (only play with / walk with nuzzling animals)",
                        ref Common.MustBeCute,
                        placeCheckboxNearText: true
                    );

                    listing.Gap(lineHeight);
                    Widgets.Label(listing.GetRect(lineHeight), "Debugging:");

                    row = listing.GetRect(lineHeight);
                    Widgets.DrawHighlightIfMouseover(row);
                    Widgets.CheckboxLabeled(
                        row,
                        "Show debug messages",
                        ref MessageAlways,
                        placeCheckboxNearText: true
                    );

                    row = listing.GetRect(lineHeight);
                    Widgets.DrawHighlightIfMouseover(row);
                    Widgets.CheckboxLabeled(
                        row,
                        "Show debug messages if dev mode is enabled",
                        ref MessageInDevMode,
                        placeCheckboxNearText: true
                    );
                } finally {
                    listing.End();
                }
            } finally {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
            }
        }

        private static void FloatTimes100Value(ref float floatBuffer, ref string stringBuffer, int min, int max, ref float value, string label, Listing listing) {
            if (floatBuffer <= 0f) {
                floatBuffer = value * 100f;
            }

            var row = listing.GetRect(lineHeight);
            var labelRect = row.LeftHalf().LeftHalf().ContractedBy(1f).Rounded();
            var intValueRect = row.LeftHalf().RightHalf().ContractedBy(1f).Rounded();
            var sliderRect = row.RightHalf().ContractedBy(1f).Rounded();

            Widgets.DrawHighlightIfMouseover(row);

            Widgets.Label(labelRect, label);

            var newIntValue = (int) (value * 100f + 0.5f);
            var oldIntValue = newIntValue;
            Widgets.IntEntry(intValueRect, ref newIntValue, ref stringBuffer);
            if (newIntValue != oldIntValue) {
                oldIntValue = newIntValue;
                newIntValue = Math.Max(min, Math.Min(max, newIntValue));
                floatBuffer = newIntValue;
                value = floatBuffer / 100f;
            }

            var newFloatValue = Widgets.HorizontalSlider(sliderRect, floatBuffer, 0, max);
            newIntValue = (int) (newFloatValue + 0.5f);
            if (newIntValue != oldIntValue) {
                newIntValue = Math.Max(min, Math.Min(max, newIntValue));
                stringBuffer = $"{newIntValue}";
                floatBuffer = newIntValue;
            }
        }

        public override string SettingsCategory() => "Animals are fun";
    }

    public class Settings : ModSettings {
        public override void ExposeData() {
            Scribe_Values.Look(ref AnimalsAreFun.MessageInDevMode, "MessageInDevMode", AnimalsAreFun.MessageInDevModeDefault, true);
            Scribe_Values.Look(ref AnimalsAreFun.MessageAlways, "MessageAlways", AnimalsAreFun.MessageAlwaysDefault, true);
            
            Scribe_Values.Look(ref Common.MinConsciousness, "MinConsciousness", Common.MinConsciousnessDefault, true);
            Scribe_Values.Look(ref Common.MinMoving, "MinMoving", Common.MinMovingDefault, true);
            Scribe_Values.Look(ref Common.MaxBodySize, "MaxBodySize", Common.MaxBodySizeDefault, true);
            Scribe_Values.Look(ref Common.MaxWildness, "MaxWildness", Common.MaxWildnessDefault, true);
            Scribe_Values.Look(ref Common.MustBeCute, "MustBeCute", Common.MustBeCuteDefault, true);

            Common.MinConsciousness = Mathf.Clamp(Common.MinConsciousness, 0.10f, 1.00f);
            Common.MinMoving = Mathf.Clamp(Common.MinMoving, 0.10f, 1.00f);
            Common.MaxBodySize = Mathf.Clamp(Common.MaxBodySize, 0.01f, 5.00f);
            Common.MaxWildness = Mathf.Clamp(Common.MaxWildness, 0.10f, 1.00f);
        }
    }
}
