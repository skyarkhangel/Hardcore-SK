using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    public struct GeneralInformation
    {
        public MethodBase method;

        public string modName;
        public string assname;
        public string methodName;
        public string typeName;

        public string patchType;
        public List<GeneralInformation> patches;
    }

    public static class Panel_Patches
    {
        public static void Draw(Rect inrect, GeneralInformation? currentInformation)
        {
            if (currentInformation == null || currentInformation.Value.patches.NullOrEmpty()) return;

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleLeft;

            foreach (var patch in currentInformation?.patches)
            {
                var left = $" {patch.patchType}";
                var right = $" {patch.modName}";

                var textHeight = Mathf.Max(Text.CalcHeight(left, inrect.width / 2), Text.CalcHeight(right, inrect.width / 2 - 5f));

                var rect = inrect.TopPartPixels(textHeight);
                inrect.y += textHeight;

                var anchor = Text.Anchor;
                Text.Anchor = TextAnchor.MiddleCenter;

                var leftRect = rect.LeftPart(.5f);
                Widgets.Label(leftRect, left);
                var rightRect = rect.RightPart(.5f);
                rightRect.x += 5;
                Widgets.Label(rightRect, right);

                Text.Anchor = anchor;

                Widgets.DrawHighlightIfMouseover(rect);

                if (Mouse.IsOver(rect))
                {
                    // todo cache tip
                    TooltipHandler.TipRegion(rect, $"Mod Name: {patch.modName}\nPatch Type: {patch.patchType}\nPatch Method: {patch.typeName}:{patch.methodName}");
                }

                if (Input.GetMouseButtonDown(1) && rect.Contains(Event.current.mousePosition)) // mouse button right
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>()
                    {
                        new FloatMenuOption("Open In Github", () => Panel_BottomRow.OpenGithub($"{patch.typeName}.{patch.methodName}")),
                        new FloatMenuOption("Open In Dnspy (requires local path)", () => Panel_BottomRow.OpenDnspy(patch.method))
                    };

                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }

            DubGUI.ResetFont();
        }
    }
}