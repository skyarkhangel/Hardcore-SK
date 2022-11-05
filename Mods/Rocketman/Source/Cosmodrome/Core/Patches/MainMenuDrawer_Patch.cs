using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace RocketMan
{
    [RocketPatch(typeof(MainMenuDrawer), nameof(MainMenuDrawer.DoMainMenuControls))]
    public static class MainMenuDrawer_DoMainMenuControls_Patch
    {
        private static bool finished = false;

        private static bool confirming = false;

        public static bool Prefix()
        {
            if (false
                || Current.ProgramState == ProgramState.Playing
                || finished)
                return true;
            if (IncompatibilityHelper.incompatibleMods.Count == 0)
                return finished = true;
            if (!confirming && !Find.WindowStack.windows.Any(w => w.GetType() == typeof(Dialog_MessageBox)
                                             && w is Dialog_MessageBox dialog
                                             && dialog.title == KeyedResources.RocketMan_IncompatibilityWindow_Title))
            {
                ShowWarning();
            }
            return false;
        }

        private static void ShowWarning()
        {
            string description = KeyedResources.RocketMan_IncompatibilityWindow_Description;
            for (int i = 0; i < IncompatibilityHelper.incompatibleMods.Count; i++)
            {
                description += $"\n\n<color=orange>{i + 1}.</color> { IncompatibilityHelper.incompatibleMods[i]}\n";
            }
            description += "\n<color=red>" + KeyedResources.RocketMan_IncompatibilityWindow_Disclaimer + "</color>";
            Find.WindowStack.Add(new Dialog_MessageBox(description,
                buttonBText: KeyedResources.RocketMan_IncompatibilityWindow_OpenModManager, buttonBAction: () =>
                {
                    finished = true;
                    Find.WindowStack.Add(new Page_ModsConfig());
                },
                buttonAText: KeyedResources.RocketMan_IncompatibilityWindow_Continue, buttonAAction: () =>
                {
                    confirming = true;
                    Dialog_MessageBox confirm = Dialog_MessageBox.CreateConfirmation(KeyedResources.RocketMan_IncompatibilityWindow_Disclaimer, delegate
                    {
                        confirming = false;
                        finished = true;
                    }, destructive: true);
                    confirm.buttonBAction = () =>
                    {
                        confirming = false;
                        ShowWarning();
                    };
                    confirm.interactionDelay = 10;
                    Find.WindowStack.Add(confirm);
                }, title: KeyedResources.RocketMan_IncompatibilityWindow_Title, buttonADestructive: true)
            {
                interactionDelay = 10,
            }
            );
        }
    }
}
