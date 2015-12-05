using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace SK_collect
{
    public static partial class Widget
    {
        //From Verse.TexButton
        public static readonly Texture2D CloseXBig = ContentFinder<Texture2D>.Get("UI/Widgets/CloseX", true);
        public static readonly Texture2D CloseXSmall = ContentFinder<Texture2D>.Get("UI/Widgets/CloseXSmall", true);
        public static readonly Texture2D NextBig = ContentFinder<Texture2D>.Get("UI/Widgets/NextArrow", true);
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);
        public static readonly Texture2D ReorderUp = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderUp", true);
        public static readonly Texture2D ReorderDown = ContentFinder<Texture2D>.Get("UI/Buttons/ReorderDown", true);
        public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus", true);
        public static readonly Texture2D Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus", true);
        public static readonly Texture2D Suspend = ContentFinder<Texture2D>.Get("UI/Buttons/Suspend", true);
        public static readonly Texture2D SelectOverlappingNext = ContentFinder<Texture2D>.Get("UI/Buttons/SelectNextOverlapping", true);
        public static readonly Texture2D Info = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);
        public static readonly Texture2D Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename", true);
        public static readonly Texture2D OpenStatsReport = ContentFinder<Texture2D>.Get("UI/Buttons/OpenStatsReport", true);
        public static readonly Texture2D ToggleLog = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/ToggleLog", true);
        public static readonly Texture2D OpenDebugActionsMenu = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenDebugActionsMenu", true);
        public static readonly Texture2D OpenInspector = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenInspector", true);
        public static readonly Texture2D OpenInspectSettings = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenInspectSettings", true);
        public static readonly Texture2D ToggleGodMode = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/ToggleGodMode", true);
        public static readonly Texture2D OpenPackageEditor = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/OpenPackageEditor", true);
        public static readonly Texture2D TogglePauseOnError = ContentFinder<Texture2D>.Get("UI/Buttons/DevRoot/TogglePauseOnError", true);
        public static readonly Texture2D Add = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Add", true);
        public static readonly Texture2D NewItem = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/NewItem", true);
        public static readonly Texture2D Reveal = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Reveal", true);
        public static readonly Texture2D Collapse = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Collapse", true);
        public static readonly Texture2D Empty = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Empty", true);
        public static readonly Texture2D Save = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Save", true);
        public static readonly Texture2D NewFile = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/NewFile", true);
        public static readonly Texture2D RenameDev = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Rename", true);
        public static readonly Texture2D Reload = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Reload", true);
        public static readonly Texture2D Play = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Play", true);
        public static readonly Texture2D Stop = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/Stop", true);
        public static readonly Texture2D RangeMatch = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/RangeMatch", true);
        public static readonly Texture2D InspectModeToggle = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/InspectModeToggle", true);
        public static readonly Texture2D CenterOnPointsTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/CenterOnPoints", true);
        public static readonly Texture2D CurveResetTex = ContentFinder<Texture2D>.Get("UI/Buttons/Dev/CurveReset", true);


        public static readonly Texture2D ArrowRightTex = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowRight");
        public static readonly Texture2D ArrowLeftTex = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowLeft");

        public static void LeftRightSelector(Rect inRect, string text, Action leftButtonAction, Action rightButtonAction, bool disabled = false)
        {
            try
            {
                GUI.BeginGroup(inRect);
                Vector2 buttonSize = new Vector2(10f, 10f);

                Rect leftButtonRect = new Rect(0f, inRect.height / 2f - buttonSize.y / 2f, buttonSize.x, buttonSize.y);
                Rect rightButtonRect = new Rect(inRect.width - buttonSize.x, leftButtonRect.y, buttonSize.x,
                    buttonSize.y);
                Rect displayRect = new Rect(leftButtonRect.xMax + 5f, 0f, inRect.width - buttonSize.x * 2 - 10f, inRect.height);

                if (disabled)
                {
                    GUI.DrawTexture(leftButtonRect, ArrowLeftTex);
                    GUI.DrawTexture(rightButtonRect, ArrowRightTex);
                }
                else
                {
                    if (Widgets.ImageButton(leftButtonRect, ArrowLeftTex))
                    {
                        leftButtonAction();
                    }
                    if (Widgets.ImageButton(rightButtonRect, ArrowRightTex))
                    {
                        rightButtonAction();
                    }
                }
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.DrawTexture(displayRect, SolidColorMaterials.NewSolidColorTexture(Color.black));
                Widgets.DrawBox(displayRect);
                Widgets.Label(displayRect, text);
                if (!disabled && displayRect.Contains(Event.current.mousePosition))
                    Widgets.DrawHighlight(displayRect);
                Text.Anchor = TextAnchor.UpperLeft;
            }
            finally
            {
                GUI.EndGroup();
            }
        }


    }
}