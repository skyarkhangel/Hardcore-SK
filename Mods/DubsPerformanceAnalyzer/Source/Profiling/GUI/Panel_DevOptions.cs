using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    public enum CurrentInput
    {
        Method,
        MethodHarmony,
        InternalMethod,
        Type,
        SubClasses,
        TypeHarmony,
        Assembly
    }

    class Panel_DevOptions
    {
        public static CurrentInput input = CurrentInput.Method;
        public static Category patchType = Category.Update;
        public static string currentInput = "";

        public static void Draw(Listing_Standard listing, float winHeight)
        {
            DubGUI.CenterText(() => listing.Label("devoptions.heading".TranslateSimple()));

            listing.GapLine(6f);

            DrawButtons(listing);
            var inputBarOffset = DisplayInputField(listing, winHeight);
            DisplaySelectionOptions(listing);

            var x = listing.curX + inputBarOffset;
            var y = listing.curY - Text.LineHeight;

            var width = listing.ColumnWidth - inputBarOffset;
            var height = (winHeight - listing.curY) - 18f;

            var uiPoint = GUIUtility.GUIToScreenPoint(new Vector2(x, y));
            var rect = new Rect(uiPoint, new Vector2(width, height));


            Window_SearchBar.SetCurrentInput(input);
            Window_SearchBar.UpdateSearchString(currentInput);

            bool shouldExit;

            lock (Window_SearchBar.sync)
            {
                shouldExit = Window_SearchBar.CheckShouldClose(new Rect(x, y, width, height)) || Window_SearchBar.cachedEntries.Count == 1 && Window_SearchBar.cachedEntries.First() == currentInput;
            }

            if (shouldExit) return;

            // 0x7FFFFFFF is 011111 ... in binary, in effect takes only the positive component of the hash
            Find.WindowStack.ImmediateWindow(currentInput.GetHashCode() & 0x7FFFFFFF, rect, WindowLayer.Super, () =>
            {
                GUI.color = Window_SearchBar.windowTransparency;
                Window_SearchBar.DoWindowContents(rect.AtZero());
                GUI.color = Color.white;
            }, false);

        }

        public static void DrawButtons(Listing_Standard listing)
        {
            var i = 0;
            var r = new Rect();

            DrawInputIcon(ResourceCache.Strings.devoptions_input_method, CurrentInput.Method);
            DrawInputIcon(ResourceCache.Strings.devoptions_input_methodinternal, CurrentInput.InternalMethod);
            DrawInputIcon(ResourceCache.Strings.devoptions_input_methodharmony, CurrentInput.MethodHarmony);
            DrawInputIcon(ResourceCache.Strings.devoptions_input_type, CurrentInput.Type);
            DrawInputIcon(ResourceCache.Strings.devoptions_input_subclasses, CurrentInput.SubClasses);
            DrawInputIcon(ResourceCache.Strings.devoptions_input_typeharmony, CurrentInput.TypeHarmony);
            DrawInputIcon(ResourceCache.Strings.devoptions_input_assembly, CurrentInput.Assembly);

            void DrawInputIcon(string key, CurrentInput inputType)
            {
                if (i++ % 2 == 0)
                {
                    r = listing.GetRect(Text.LineHeight + 3).LeftPart(.49f);
                    r.x += listing.ColumnWidth * .01f;
                }
                else
                {
                    r.x += listing.ColumnWidth/2;
                }

                DubGUI.OptionalBox(r, key, () => input = inputType, input == inputType);
            }
        }
        
        public static float DisplayInputField(Listing_Standard listing, float winHeight)
        {
            string FieldDescription = null;

            switch (input)
            {
                case CurrentInput.Method: FieldDescription = "Type:Method"; break;
                case CurrentInput.Type: FieldDescription = "Type"; break;
                case CurrentInput.MethodHarmony: FieldDescription = "Type:Method"; break;
                case CurrentInput.TypeHarmony: FieldDescription = "Type"; break;
                case CurrentInput.InternalMethod: FieldDescription = "Type:Method"; break;
                case CurrentInput.SubClasses: FieldDescription = "Type"; break;
                case CurrentInput.Assembly: FieldDescription = "Mod or PackageId"; break;
            }

            var descWidth = FieldDescription.GetWidthCached() + 5f;
            var height = Mathf.Max( Mathf.CeilToInt(descWidth / listing.ColumnWidth), 1) * Text.LineHeight;
            var rect = listing.GetRect(height);

            // Name
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect.LeftPartPixels(FieldDescription.GetWidthCached()), FieldDescription);
            Text.Anchor = TextAnchor.UpperLeft;

            var currentInputWidth = currentInput?.GetWidthCached() ?? 1;
            var inputHeight = Mathf.Max( Mathf.CeilToInt(currentInputWidth / (listing.ColumnWidth - descWidth)), 1) * Text.LineHeight;
            rect = listing.GetRect(inputHeight);
            rect.AdjustHorizonallyBy(descWidth);
            rect.y -= height;
            listing.curY -= ( height - 3f);

            GUI.SetNextControlName(currentInput);

            // textfield
            currentInput = GUI.TextField(rect, currentInput, Text.CurTextAreaStyle);
            bool inFocus = GUI.GetNameOfFocusedControl() == currentInput;

            if (Input.GetMouseButtonDown(0) && inFocus)
            {
                var mousePos = Event.current.mousePosition;
                // todo: mousePos.y is not relative to the current GUI scope or something, so the winHeight is wrong... 
                if (mousePos.x < rect.x || mousePos.x > rect.x + rect.width || mousePos.y < rect.y || mousePos.y > rect.y + rect.height)
                {
                    GUI.FocusControl(null);
                } 
            }

            return descWidth;
        }

        public static void DisplaySelectionOptions(Listing_Standard listing)
        {
            var box = listing.GetRect(Text.LineHeight);

            var tickBox = box.LeftPart(.24f);
            var updateBox = box.LeftPartPixels(listing.ColumnWidth * .48f);
            updateBox.AdjustHorizonallyBy(box.width/4);


            box.AdjustHorizonallyBy(box.width/2);

            DubGUI.OptionalBox(tickBox, "devoptions.patchtype.tick".TranslateSimple(), () => patchType = Category.Tick, patchType == Category.Tick);
            DubGUI.OptionalBox(updateBox, "devoptions.patchtype.update".TranslateSimple(), () => patchType = Category.Update, patchType == Category.Update);

            if (Widgets.ButtonText(box, "Patch"))
            {
                if (!string.IsNullOrEmpty(currentInput)) ExecutePatch();
            }
        }

        public static void ExecutePatch()
        {
            try
            {
                if (patchType == Category.Tick)
                {
                    switch (input)
                    {
                        case CurrentInput.Method:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersTick), Utility.GetMethods(currentInput));
                            break;
                        case CurrentInput.Type:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersTick), Utility.GetTypeMethods(AccessTools.TypeByName(currentInput)));
                            break;
                        case CurrentInput.MethodHarmony:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersTick), Utility.GetMethodsPatching(currentInput));
                            break;
                        case CurrentInput.SubClasses:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersTick), Utility.SubClassImplementationsOf(AccessTools.TypeByName(currentInput), m => true));
                            break;
                        case CurrentInput.TypeHarmony:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersTick), Utility.GetMethodsPatchingType(AccessTools.TypeByName(currentInput)));
                            break;
                        case CurrentInput.InternalMethod:
                            Utility.PatchInternalMethod(currentInput, Category.Tick);
                            return;
                        case CurrentInput.Assembly:
                            Utility.PatchAssembly(currentInput, Category.Tick);
                            return;
                    }

                    GUIController.SwapToEntry("Custom Tick");
                }
                else
                {
                    switch (input)
                    {
                        case CurrentInput.Method:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersUpdate), Utility.GetMethods(currentInput));
                            break;
                        case CurrentInput.Type:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersUpdate), Utility.GetTypeMethods(AccessTools.TypeByName(currentInput)));
                            break;
                        case CurrentInput.MethodHarmony:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersUpdate), Utility.GetMethodsPatching(currentInput));
                            break;
                        case CurrentInput.SubClasses:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersUpdate), Utility.SubClassImplementationsOf(AccessTools.TypeByName(currentInput), m => true));
                            break;
                        case CurrentInput.TypeHarmony:
                            MethodTransplanting.UpdateMethods(typeof(CustomProfilersUpdate), Utility.GetMethodsPatchingType(AccessTools.TypeByName(currentInput)));
                            break;
                        case CurrentInput.InternalMethod:
                            Utility.PatchInternalMethod(currentInput, Category.Update);
                            return;
                        case CurrentInput.Assembly:
                            Utility.PatchAssembly(currentInput, Category.Update);
                            return;
                    }

                    GUIController.SwapToEntry("Custom Update");
                }
            }
            catch (Exception e)
            {
                ThreadSafeLogger.Error($"Failed to process input, failed with the error {e.Message}");
            }
        }
    }
}