using System;
using System.IO;
using HugsLib.Settings;
using UnityEngine;
using Verse;

namespace Hospitality
{
    internal class Settings
    {
        private const int DefaultMaxGroupSize = 16;

        public static SettingHandle<int> minGuestWorkSkill;
        public static SettingHandle<int> minGuestGroupSize;
        public static SettingHandle<int> maxGuestGroupSize;
        public static SettingHandle<int> maxIncidentsPer3Days;
        public static SettingHandle<bool> disableGuests;
        public static SettingHandle<bool> disableWork;
        public static SettingHandle<bool> disableGifts;
        public static SettingHandle<bool> disableLimits;
        public static SettingHandle<bool> disableArtAndCraft;
        public static SettingHandle<bool> disableOperations;
        public static SettingHandle<bool> disableMedical;
        public static SettingHandle<bool> disableWarden;
        public static SettingHandle<bool> disableGuestsTab;
        public static SettingHandle<bool> useIcon;
        public static SettingHandle<bool> enableBuyNotification;
        public static SettingHandle<bool> enableRecruitNotification;

        public Settings(ModSettingsPack settings)
        {
            disableGuests = settings.GetHandle("disableGuests", "DisableVisitors".Translate(), "DisableVisitorsDesc".Translate(), false);
            disableWork = settings.GetHandle("disableWork", "DisableGuestsHelping".Translate(), "DisableGuestsHelpingDesc".Translate(), false);
            disableArtAndCraft = settings.GetHandle("disableArtAndCraft", "DisableArtAndCraft".Translate(), "DisableArtAndCraftDesc".Translate(), true);
            disableOperations = settings.GetHandle("disableOperations", "DisableOperations".Translate(), "DisableOperationsDesc".Translate(), true);
            disableMedical = settings.GetHandle("disableMedical", "DisableMedical".Translate(), "DisableMedicalDesc".Translate(), false);
            disableWarden = settings.GetHandle("disableWarden", "DisableWarden".Translate(), "DisableWardenDesc".Translate(), false);
            disableGifts = settings.GetHandle("disableGifts", "DisableGifts".Translate(), "DisableGiftsDesc".Translate(), false);
            minGuestWorkSkill = settings.GetHandle("minGuestWorkSkill", "MinGuestWorkSkill".Translate(), "MinGuestWorkSkillDesc".Translate(), 7, WorkSkillLimits);
            minGuestGroupSize = settings.GetHandle("minGuestGroupSize", "MinGuestGroupSize".Translate(), "MinGuestGroupSizeDesc".Translate(), 1, GroupSizeLimitsMin);
            maxGuestGroupSize = settings.GetHandle("maxGuestGroupSize", "MaxGuestGroupSize".Translate(), "MaxGuestGroupSizeDesc".Translate(), DefaultMaxGroupSize, GroupSizeLimitsMax);
            maxIncidentsPer3Days = settings.GetHandle("maxIncidentsPer3Days", "MaxIncidentsPer3Days".Translate(), "MaxIncidentsPer3DaysDesc".Translate(), 5, MaxIncidentsPer3DaysLimitsMin);
            disableLimits = settings.GetHandle("disableLimits", "DisableLimits".Translate(), "DisableLimitsDesc".Translate(), false);
            disableGuestsTab = settings.GetHandle("disableGuestsTab", "DisableGuestsTab".Translate(), "DisableGuestsTabDesc".Translate(), false);
            useIcon = settings.GetHandle("useIcon", "UseIcon".Translate(), "UseIconDesc".Translate(), false);
            enableBuyNotification = settings.GetHandle("enableBuyNotification", "EnableBuyNotification".Translate(), "EnableBuyNotificationDesc".Translate(), false);
            enableRecruitNotification = settings.GetHandle("enableRecruitNotification", "EnableRecruitNotification".Translate(), "EnableRecruitNotificationDesc".Translate(), true);

            string hiddenConfigFile = Path.Combine(GenFilePaths.ConfigFolderPath, "Hospitality.cfg");
            if (File.Exists(hiddenConfigFile))
            {
                try
                {
                    var reader = File.OpenText(hiddenConfigFile);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.StartsWith("#")) continue;
                        string[] keyVal = line.Split('=');
                        if (keyVal.Length != 2) continue;
                        string key = keyVal[0].Trim();
                        string val = keyVal[1].Trim();

                        switch (key)
                        {
                            case "PriceFactor":
                                Log.Message("[Hospitality] Setting PriceFactor to " + val);
                                JobDriver_BuyItem.PriceFactor = float.Parse(val);
                                break;

                            default:
                                Log.Message("[Hospitality] Unrecognized setting: " + key);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error("[Hospitality] Exception loading Hospitality.cfg: " + e.Message);
                }
            }
        }

        // Make sure that it still works when referenced settings are null!
        private static SettingHandle.ValueIsValid WorkSkillLimits { get { return AtLeast(() => disableLimits?.Value != false ? 0 : 6); } }
        private static SettingHandle.ValueIsValid MaxIncidentsPer3DaysLimitsMin { get { return AtLeast(() => 1); } }
        private static SettingHandle.ValueIsValid GroupSizeLimitsMin { get { return Between(() => 1, () => maxGuestGroupSize?.Value ?? DefaultMaxGroupSize); } }
        private static SettingHandle.ValueIsValid GroupSizeLimitsMax { get { return AtLeast(() => Mathf.Max(minGuestGroupSize?.Value ?? 0, disableLimits?.Value != false ? 1 : 8)); } }

        private static SettingHandle.ValueIsValid Between(Func<int> amountMin, Func<int> amountMax)
        {
            return value => int.TryParse(value, out var actual) && actual >= amountMin() && actual <= amountMax();
        }

        private static SettingHandle.ValueIsValid AtLeast(Func<int> amount)
        {
            return value => int.TryParse(value, out var actual) && actual >= amount();
        }

        private static SettingHandle.ValueIsValid AtMost(Func<int> amount)
        {
            return value => int.TryParse(value, out var actual) && actual <= amount();
        }
    }
}
