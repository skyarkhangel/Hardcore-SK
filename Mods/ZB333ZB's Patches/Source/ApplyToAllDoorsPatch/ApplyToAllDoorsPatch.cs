using HarmonyLib;
using Verse;
using RimWorld;
using System.Linq;
using UnityEngine;
using System;

namespace LocksPatches.ApplyToAllDoors
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            var harmony = new Harmony("com.ZB333ZB.ApplyToAllDoorsPatch");
            harmony.PatchAll();
        }
    }

    public static class UIConstants
    {
        public const float MIN_BUTTON_WIDTH = 60f;
        public const float BUTTON_HEIGHT = 25f;
        public const float BOTTOM_PADDING = 15f;
        public const float LEFT_PADDING = 15f;
    }

    [HarmonyPatch(typeof(Locks.ITab_Lock), "FillTab")]
    public class ITab_Lock_FillTab_Patch
    {
        static void Postfix(Locks.ITab_Lock __instance)
        {
            try
            {
                UIHandler.AddApplyToAllButton(__instance);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ITab_Lock_FillTab_Patch: {ex}");
            }
        }
    }

    public static class UIHandler
    {
        public static void AddApplyToAllButton(Locks.ITab_Lock instance)
        {
            Vector2 windowSize = (Vector2)AccessTools.Field(typeof(Locks.ITab_Lock), "WinSize").GetValue(null);
            float buttonYPosition = windowSize.y - UIConstants.BUTTON_HEIGHT - UIConstants.BOTTOM_PADDING;

            float copyButtonWidth = 60f;
            float copyButtonXPosition = windowSize.x / 2f - 2f - copyButtonWidth;

            float maxApplyToAllWidth = copyButtonXPosition - UIConstants.LEFT_PADDING - 15f;

            float applyToAllWidth = CalculateButtonWidth("ApplyToAllDoorsPatch_Button".Translate(), UIConstants.MIN_BUTTON_WIDTH, maxApplyToAllWidth);

            Rect applyToAllButtonRect = new Rect(UIConstants.LEFT_PADDING, buttonYPosition, applyToAllWidth, UIConstants.BUTTON_HEIGHT);

            if (Widgets.ButtonText(applyToAllButtonRect, "ApplyToAllDoorsPatch_Button".Translate()))
            {
                LockSettingsApplier.ApplySettingsToAllDoors(instance);
            }
        }

        private static float CalculateButtonWidth(string text, float minWidth, float maxWidth)
        {
            Vector2 textSize = Text.CalcSize(text);
            float requiredWidth = textSize.x + 20f;
            return Mathf.Clamp(requiredWidth, minWidth, maxWidth);
        }
    }

    public static class LockSettingsApplier
    {
        public static void ApplySettingsToAllDoors(object instance)
        {
            try
            {
                var selectedDoor = (ThingWithComps)AccessTools.Property(typeof(Locks.ITab_Lock), "SelDoor").GetValue(instance);
                if (selectedDoor == null)
                {
                    Log.Error("ApplySettingsToAllDoors: Selected door is null.");
                    return;
                }

                var selectedDoorLockData = Locks.LockUtility.GetData(selectedDoor);
                if (selectedDoorLockData == null)
                {
                    Log.Error("ApplySettingsToAllDoors: Lock data for selected door is null.");
                    return;
                }

                var allPlayerDoors = selectedDoor.Map.listerThings.AllThings
                    .OfType<Building_Door>()
                    .Where(d => d is ThingWithComps && d.Faction == Faction.OfPlayer && d != selectedDoor)
                    .Cast<ThingWithComps>();


                foreach (var door in allPlayerDoors)
                {
                    ApplyLockSettingsToDoor(door, selectedDoorLockData);
                }

                AccessTools.Method(typeof(Locks.ITab_Lock), "OnOpen").Invoke(instance, null);
                Messages.Message("ApplyToAllDoorsPatch_Message".Translate(), MessageTypeDefOf.TaskCompletion);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in ApplySettingsToAllDoors: {ex}");
            }
        }

        private static void ApplyLockSettingsToDoor(ThingWithComps door, Locks.LockData sourceLockData)
        {
            Locks.LockData doorLockData = Locks.LockUtility.GetData(door);
            if (doorLockData != null)
            {
                doorLockData.WantedState.CopyFrom(sourceLockData.WantedState);
                Locks.LockUtility.UpdateLockDesignation(door);
            }
        }
    }
}