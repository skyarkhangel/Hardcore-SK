using HugsLib.Settings;
using System;
using System.Globalization;
using UnityEngine;
using Verse;
using Color = UnityEngine.Color;

namespace Hospitality
{
    internal class Settings
    {
        public static SettingHandle<int> minGuestWorkSkill;
        public static SettingHandle<ConvertibleIntRange> guestGroupSize;
        public static SettingHandle<int> maxIncidentsPer3Days;
        public static SettingHandle<float> silverMultiplier;
        public static SettingHandle<bool> disableGuests;
        public static SettingHandle<bool> disableWork;
        public static SettingHandle<bool> disableGifts;
        public static SettingHandle<bool> disableArtAndCraft;
        public static SettingHandle<bool> disableOperations;
        public static SettingHandle<bool> disableMedical;
        public static SettingHandle<bool> disableGuestsTab;
        public static SettingHandle<bool> useIcon;
        public static SettingHandle<bool> enableBuyNotification;
        public static SettingHandle<bool> enableRecruitNotification;
        public static SettingHandle<bool> disableFriendlyGearDrops;

        public Settings(ModSettingsPack settings)
        {
            disableGuests = GetToggleHandle(settings, "disableGuests", "DisableVisitors".Translate(), "DisableVisitorsDesc".Translate(), false);
            disableWork = GetToggleHandle(settings, "disableWork", "DisableGuestsHelping".Translate(), "DisableGuestsHelpingDesc".Translate(), false);
            disableArtAndCraft = GetToggleHandle(settings, "disableArtAndCraft", "DisableArtAndCraft".Translate(), "DisableArtAndCraftDesc".Translate(), true, true);
            disableOperations = GetToggleHandle(settings, "disableOperations", "DisableOperations".Translate(), "DisableOperationsDesc".Translate(), true, true);
            disableMedical = GetToggleHandle(settings, "disableMedical", "DisableMedical".Translate(), "DisableMedicalDesc".Translate(), false);
            disableGifts = GetToggleHandle(settings, "disableGifts", "DisableGifts".Translate(), "DisableGiftsDesc".Translate(), false);
            minGuestWorkSkill = GetSliderHandle(settings, "minGuestWorkSkill", "MinGuestWorkSkill".Translate(), "MinGuestWorkSkillDesc".Translate(), 7, () => 0, () => 20, recommended: new IntRange(6, 20));
            guestGroupSize = GetRangeHandle(settings, "guestGroupSize", "GuestGroupSize".Translate(), "GuestGroupSizeDesc".Translate(), new ConvertibleIntRange(1, 16), () => 1, () => 20, recommendedMin: new IntRange(1, 1), recommendedMax: new IntRange(8, 20));
            maxIncidentsPer3Days = GetSliderHandle(settings, "maxIncidentsPer3Days", "MaxIncidentsPer3Days".Translate(), "MaxIncidentsPer3DaysDesc".Translate(), 5, () => 1, () => 10);
            silverMultiplier = GetSliderHandle(settings, "silverMultiplier", "SilverMultiplier".Translate(), "SilverMultiplierDesc".Translate(), 1, () => 0, () => 10f, 0.1f, recommended: new FloatRange(0.5f, 1.5f));
            disableGuestsTab = GetToggleHandle(settings, "disableGuestsTab", "DisableGuestsTab".Translate(), "DisableGuestsTabDesc".Translate(), false, false);
            useIcon = GetToggleHandle(settings, "useIcon", "UseIcon".Translate(), "UseIconDesc".Translate(), false);
            enableBuyNotification = GetToggleHandle(settings, "enableBuyNotification", "EnableBuyNotification".Translate(), "EnableBuyNotificationDesc".Translate(), false);
            enableRecruitNotification = GetToggleHandle(settings, "enableRecruitNotification", "EnableRecruitNotification".Translate(), "EnableRecruitNotificationDesc".Translate(), true);
            disableFriendlyGearDrops = GetToggleHandle(settings, "disableFriendlyGearDrops", "DisableFriendlyGearDrops".Translate(), "DisableFriendlyGearDropsDesc".Translate(), true, true);
        }

        private static SettingHandle<bool> GetToggleHandle(ModSettingsPack settings, string name, TaggedString title, TaggedString description, bool defaultValue, bool? recommended = null)
        {
            var handle = settings.GetHandle(name, title, description, defaultValue);
            handle.CustomDrawer = rect =>
            {
                var value = handle.Value;
                Widgets.CheckboxLabeled(rect.TopPartPixels(34), string.Empty, ref value, placeCheckboxNearText: true);
                handle.Value = value;
                CheckBadValue(rect.BottomPartPixels(52), handle, recommended);
                return true;
            };
            return handle;
        }

        private static SettingHandle<float> GetSliderHandle(ModSettingsPack settings, string name, TaggedString title, TaggedString description, float defaultValue, Func<float> min, Func<float> max, float steps = -1, FloatRange? recommended = null)
        {
            var handle = settings.GetHandle(name, title, description, defaultValue);
            handle.CustomDrawer = rect =>
            {
                handle.Value = Widgets.HorizontalSlider(rect.TopPartPixels(34), Convert.ToSingle(handle.Value), min(), max(), true, handle.Value.ToString(CultureInfo.CurrentCulture), min().ToString(CultureInfo.CurrentCulture), max().ToString(CultureInfo.CurrentCulture), steps);
                CheckBadValue(rect.BottomPartPixels(52), handle, recommended);
                return true;
            };
            return handle;
        }

        private static SettingHandle<int> GetSliderHandle(ModSettingsPack settings, string name, TaggedString title, TaggedString description, int defaultValue, Func<int> min, Func<int> max, int steps = -1, IntRange? recommended = null)
        {
            var handle = settings.GetHandle(name, title, description, defaultValue);
            handle.CustomDrawer = rect =>
            {
                handle.Value = (int)Widgets.HorizontalSlider(rect.TopPartPixels(34), Convert.ToSingle(handle.Value), min(), max(), true, handle.Value.ToString(), min().ToString(), max().ToString(), steps);
                CheckBadValue(rect.BottomPartPixels(52), handle, recommended);
                return true;
            };
            return handle;
        }

        private static SettingHandle<ConvertibleIntRange> GetRangeHandle(ModSettingsPack settings, string name, TaggedString title, TaggedString description, ConvertibleIntRange defaultValue, Func<int> min, Func<int> max, int minRange = 0, IntRange? recommendedMin = null, IntRange? recommendedMax = null)
        {
            var handle = settings.GetHandle(name, title, description, defaultValue);
            handle.CustomDrawer = rect =>
            {
                var range = new IntRange(handle.Value.Min, handle.Value.Max);
                Widgets.IntRange(rect.TopPartPixels(34), handle.GetHashCode(), ref range, min(), max(), range.ToString(), minRange);
                handle.Value = new ConvertibleIntRange(range.TrueMin, range.TrueMax);
                CheckBadValue(rect.BottomPartPixels(52), handle, recommendedMin, recommendedMax);
                return true;
            };

            return handle;
        }

        private static void CheckBadValue(Rect rect, SettingHandle<bool> handle, bool? recommended)
        {
            var badValue = recommended.HasValue && handle.Value != recommended.Value;
            if (badValue)
            {
                var font = Text.Font;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, recommended == true ? "Hospitality_RecommendedOn".Translate().Colorize(Color.red) : "Hospitality_RecommendedOff".Translate().Colorize(Color.red));
                Text.Font = font;
            }
            handle.CustomDrawerHeight = badValue ? 34 + 52 : 34;
        }

        private static void CheckBadValue(Rect rect, SettingHandle<int> handle, IntRange? recommended)
        {
            var badValue = recommended.HasValue && !recommended.Value.Includes(handle.Value);
            if (badValue)
            {
                var font = Text.Font;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, "Hospitality_RecommendedRange".Translate(recommended.Value.TrueMin.ToString(CultureInfo.CurrentCulture), recommended.Value.TrueMax.ToString(CultureInfo.CurrentCulture)).Colorize(Color.red));
                Text.Font = font;
            }
            handle.CustomDrawerHeight = badValue ? 34 + 52 : 34;
        }

        private static void CheckBadValue(Rect rect, SettingHandle<float> handle, FloatRange? recommended)
        {
            var badValue = recommended.HasValue && !recommended.Value.Includes(handle.Value);
            if (badValue)
            {
                var font = Text.Font;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, "Hospitality_RecommendedRange".Translate(recommended.Value.TrueMin.ToString(CultureInfo.CurrentCulture), recommended.Value.TrueMax.ToString(CultureInfo.CurrentCulture)).Colorize(Color.red));
                Text.Font = font;
            }
            handle.CustomDrawerHeight = badValue ? 34 + 52 : 34;
        }

        private static void CheckBadValue(Rect rect, SettingHandle<ConvertibleIntRange> handle, IntRange? recommendedMin, IntRange? recommendedMax)
        {
            var badValueMin = recommendedMin.HasValue && !recommendedMin.Value.Includes(handle.Value.Min);
            var badValueMax = recommendedMax.HasValue && !recommendedMax.Value.Includes(handle.Value.Max);

            if (badValueMin)
            {
                var font = Text.Font;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, "Hospitality_RecommendedRange".Translate(recommendedMin.Value.TrueMin.ToString(CultureInfo.CurrentCulture), recommendedMin.Value.TrueMax.ToString(CultureInfo.CurrentCulture)).Colorize(Color.red));
                Text.Font = font;
            }
            else if (badValueMax)
            {
                var font = Text.Font;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, "Hospitality_RecommendedRange".Translate(recommendedMax.Value.TrueMin.ToString(CultureInfo.CurrentCulture), recommendedMax.Value.TrueMax.ToString(CultureInfo.CurrentCulture)).Colorize(Color.red));
                Text.Font = font;
            }
            handle.CustomDrawerHeight = badValueMin || badValueMax ? 34 + 52 : 34;
        }
    }
}
