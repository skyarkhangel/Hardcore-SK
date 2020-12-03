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
    class Panel_DevOptions
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

            SearchBar.PopulateSearch(currentInput, input);
            var searchBarRect = new Rect(listing.curX + inputBarOffset, listing.curY - Text.LineHeight, listing.ColumnWidth - inputBarOffset, (winHeight - listing.curY) - 18f);
            SearchBar.DrawSearchBar(searchBarRect);
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
                if (mousePos.x < rect.x || mousePos.x > rect.x + rect.width || mousePos.y < rect.y || mousePos.y > winHeight)
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

            // Enter will also patch the method
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.KeypadEnter)
            {
                if (!string.IsNullOrEmpty(currentInput)) ExecutePatch();
                Event.current.Use();
            }

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

        internal static class SearchBar
        {
            public static Rect viewFrustum;
            public static Thread searchThread = null;
            public static HashSet<string> cachedEntries = new HashSet<string>();
            public static bool curSearching = false;
            public static string prevInput = "";
            public static object sync = new object();
            private static float yHeigthCache = float.MaxValue;
            private static Vector2 searchpos = Vector2.zero;
            public static Listing_Standard listing = new Listing_Standard();

            public static void PopulateSearch(string searchText, CurrentInput inputType)
            {
                bool active = false;
                lock (sync)
                {
                    active = curSearching;
                }

                if (!active && prevInput != currentInput)
                {
                    switch (inputType)
                    {
                        case CurrentInput.Method:
                        case CurrentInput.InternalMethod:
                        case CurrentInput.MethodHarmony:
                            searchThread = new Thread(() => PopulateSearchMethod(searchText));
                            break;
                        case CurrentInput.Type:
                        case CurrentInput.TypeHarmony:
                        case CurrentInput.SubClasses:
                            searchThread = new Thread(() => PopulateSearchType(searchText));
                            break;
                        default:
                            searchThread = new Thread(() => PopulateSearchAssembly(searchText));
                            break;
                    }

                    searchThread.IsBackground = true;
                    prevInput = currentInput;
                    searchThread.Start();
                }
            }

            private static void PopulateSearchMethod(string searchText)
            {
                if (searchText.Length <= 4) return;

                searchText = searchText.ToLower();

                lock (sync)
                {
                    curSearching = true;
                }

                HashSet<string> names = new HashSet<string>();

                foreach (Type type in GenTypes.AllTypes)
                {
                    if (type.FullName.Contains("Cecil") || type.FullName.Contains("Analyzer")) continue;

                    if (type.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() == null)
                    {

                        foreach (MethodInfo meth in type.GetMethods())
                        {
                            if (meth.DeclaringType == type && !meth.IsSpecialName && !meth.IsAssembly && meth.HasMethodBody())
                            {
                                string strf = string.Concat(meth.DeclaringType, ":", meth.Name);
                                string str = strf;
                                if (str.ToLower()
                                    .Contains(searchText))
                                    names.Add(str);
                            }
                        }
                    }
                }


                lock (sync)
                {
                    cachedEntries = names;
                    curSearching = false;
                }
            }

            private static void PopulateSearchType(string searchText)
            {
                if (searchText.Length <= 2) return;

                searchText = searchText.ToLower();

                lock (sync)
                {
                    curSearching = true;
                }

                HashSet<string> names = new HashSet<string>();
                foreach (Type type in GenTypes.AllTypes)
                {
                    if (type.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                    {
                        var tyName = type.FullName;
                        if (type.FullName.ToLower()
                            .Contains(searchText) && !type.FullName.Contains("Analyzer"))
                            names.Add(tyName);
                    }
                }

                lock (sync)
                {
                    cachedEntries = names;
                    curSearching = false;
                }
            }

            private static void PopulateSearchAssembly(string searchText)
            {
                lock (sync)
                {
                    curSearching = true;
                }

                var names = new HashSet<string>();
                foreach (string mod in ModInfoCache.AssemblyToModname.Values)
                {
                    var modname = mod;
                    if (mod.ToLower()
                        .Contains(searchText.ToLower()))
                        names.Add(modname);
                }

                lock (sync)
                {
                    cachedEntries = names;
                    curSearching = false;
                }
            }

            public static void DrawSearchBar(Rect rect)
            {
                if (GUI.GetNameOfFocusedControl() != currentInput) return;

                Rect innerRect = rect.AtZero();
                innerRect.height = yHeigthCache;

                viewFrustum = rect.AtZero();
                viewFrustum.y += searchpos.y;

                Widgets.BeginScrollView(rect, ref searchpos, innerRect, false);
                GUI.BeginGroup(innerRect);
                listing.Begin(innerRect);

                float yHeight = 0;

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Tiny;


                lock (sync)
                {
                    if (!(cachedEntries.Count == 1 && cachedEntries.First() == currentInput))
                    {
                        foreach (var entry in cachedEntries)
                        {
                            var r = listing.GetRect(Text.LineHeight);

                            if (!r.Overlaps(viewFrustum))
                            {
                                yHeight += (r.height + 4f);
                                continue;
                            }

                            if (Widgets.ButtonInvisible(r))
                            {
                                currentInput = entry;
                            }

                            Widgets.DrawBoxSolid(r, Modbase.Settings.GraphCol);

                            if (Mouse.IsOver(r))
                            {
                                Widgets.DrawHighlight(r);
                                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
                                {
                                    currentInput = entry;
                                    Event.current.Use();
                                }

                                GUI.DrawTexture(r.RightPartPixels(r.height), ResourceCache.GUI.enter);    
                            }

                            r.width = 2000;
                            Widgets.Label(r, " " + entry);

                            yHeight += 4f;
                            yHeight += r.height;
                        }
                    }
                }

                yHeigthCache = yHeight;

                listing.End();
                GUI.EndGroup();
                Widgets.EndScrollView();

                DubGUI.ResetFont();
            }
        }
    }
}