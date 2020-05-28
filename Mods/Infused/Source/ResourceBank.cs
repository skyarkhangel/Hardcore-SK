using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Infused
{
    public static class ResourceBank
    {
        public static class Strings {
            const string PREFIX = "Infused.";

            static string TL(string s) => (PREFIX + s).Translate();
            static string TL(string s, params object[] args) => (PREFIX + s).Translate(args);

            public static readonly string Infused = "Infused".Translate();
            public static readonly string DescBonus = TL("DescBonus");
            public static readonly string Quality = TL("Quality");
            public static readonly string Mote = TL("Mote");

            public static readonly string Common = TL("Common");
            public static readonly string Uncommon = TL("Uncommon");
            public static readonly string Rare = TL("Rare");
            public static readonly string Epic = TL("Epic");
            public static readonly string Legendary = TL("Legendary");
            public static readonly string Artifact = TL("Artifact");

            public static readonly string SettingsMultiplier = TL("Settings.Multiplier");
            public static readonly string SettingsMultiplierDesc = TL("Settings.MultiplierDesc");
            public static readonly string SettingsBias = TL("Settings.Bias");
            public static readonly string SettingsBiasDesc = TL("Settings.BiasDesc");
            public static readonly string SettingsMaxSlots = TL("Settings.MaxSlots");
            public static readonly string SettingsMaxSlotsDesc = TL("Settings.MaxSlotsDesc");
            public static readonly string SettingsTierCurve = TL("Settings.TierCurve");
            public static readonly string SettingsTierCurveDesc = TL("Settings.TierCurveDesc");
            public static readonly string SettingsExtra = TL("Settings.Extra");

            //Your weapon, {0}, is infused!
            public static string Notice(string weapon) => TL("Notice", weapon);

            public static string Tier(InfusionTier tier)
            {
                switch (tier)
                {
                    case InfusionTier.Common:
                        return Common;
                    case InfusionTier.Uncommon:
                        return Uncommon;
                    case InfusionTier.Rare:
                        return Rare;
                    case InfusionTier.Epic:
                        return Epic;
                    case InfusionTier.Legendary:
                        return Legendary;
                    case InfusionTier.Artifact:
                        return Artifact;
                    default:
                        return Common;
                }
            }
        }

        public static class Colors {
            public static readonly Color Common = new Color(0.61f, 0.61f, 0.61f);
            public static readonly Color Uncommon = new Color(0.12f, 1, 0);
            public static readonly Color Rare = new Color(0, 0.44f, 1);
            public static readonly Color Epic = new Color(0.64f, 0.21f, 0.93f);
            public static readonly Color Legendary = new Color(1, 0.5f, 0);
            public static readonly Color Artifact = new Color(0.92f, 0.84f, 0.56f);

            public static Color InfusionColor(InfusionTier tier)
            {
                switch (tier)
                {
                    case InfusionTier.Common:
                        return Common;
                    case InfusionTier.Uncommon:
                        return Uncommon;
                    case InfusionTier.Rare:
                        return Rare;
                    case InfusionTier.Epic:
                        return Epic;
                    case InfusionTier.Legendary:
                        return Legendary;
                    case InfusionTier.Artifact:
                        return Artifact;
                    default:
                        return Color.white;
                }
            }
        }

        [DefOf]
        public static class Sounds {
            public static SoundDef Infused;
        }

        [DefOf]
        public static class Things
        {
            public static ThingDef InfusedAmplifier;
        }
    }
}
