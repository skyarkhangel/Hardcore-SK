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
    internal class Window_SearchBar
    {
        internal static Color windowTransparency = new Color(1,1,1,1);
        private const float mouseDistTillClose = 35f;

        public static Rect viewFrustum;
        private static Vector2 scrollOffset = new Vector2(0, 0);
        private static float yHeigthCache = float.MaxValue;
        private static Listing_Standard listing = new Listing_Standard();

        private static bool requiresUpdate = true;
        private static string searchText;
        private static CurrentInput currentInput;
        public static HashSet<string> cachedEntries = new HashSet<string>();

        public static Thread searchThread = null;
        public static object sync = new object();
        public static bool currentlySearching = false;

        public static void UpdateSearchString(string newString)
        {
            if (newString == searchText) return;

            searchText = newString;
            requiresUpdate = true;
        }

        public static void SetCurrentInput(CurrentInput inputType)
        {
            if (inputType == currentInput) return;

            lock (sync)
            {
                cachedEntries = new HashSet<string>();
            }

            currentInput = inputType;
            requiresUpdate = true;
        }

        public static bool CheckShouldClose(Rect r)
        {
            if (r.Contains(Event.current.mousePosition))
            {
                windowTransparency = new Color(1, 1, 1, 1);
                return false;
            }

            var num = GenUI.DistFromRect(r, Event.current.mousePosition);

            windowTransparency = new Color(1f, 1f, 1f, 1f - (num / mouseDistTillClose));

            return num > mouseDistTillClose;
        }

        public static void DoWindowContents(Rect inRect)
        {
            if (requiresUpdate)
            {
                PopulateSearch(searchText, currentInput);
                requiresUpdate = false;
            }

            Draw(inRect);
        }

        public static void Draw(Rect rect)
        {
            var innerRect = rect.AtZero();
            innerRect.height = yHeigthCache;

            viewFrustum = rect.AtZero();
            viewFrustum.y += scrollOffset.y;

            Widgets.BeginScrollView(rect, ref scrollOffset, innerRect, false);
            GUI.BeginGroup(innerRect);
            listing.Begin(innerRect);

            float yHeight = 0;

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;

            lock (sync)
            {
                if (!(cachedEntries.Count == 1 && cachedEntries.First() == searchText))
                {
                    foreach (var entry in cachedEntries)
                    {
                        var r = listing.GetRect(Text.LineHeight);
                        yHeight += r.height;

                        if (!r.Overlaps(viewFrustum)) continue;

                        if (Widgets.ButtonInvisible(r))
                        {
                            Panel_DevOptions.currentInput = entry;
                        }

                        Widgets.DrawBoxSolid(r, Modbase.Settings.GraphCol);

                        if (Mouse.IsOver(r))
                        {
                            Widgets.DrawHighlight(r);
                        }

                        r.width = 2000;
                        Widgets.Label(r, " " + entry);

                    }
                }
            }

            yHeigthCache = yHeight;

            listing.End();
            GUI.EndGroup();
            Widgets.EndScrollView();

            DubGUI.ResetFont();
        }
        

        private static void PopulateSearch(string searchText, CurrentInput inputType)
        {
            bool active = false;

            lock (sync)
            {
                active = currentlySearching;
            }

            if (active) return; // Already a thread doing the computation

            switch (inputType)
            {
                case CurrentInput.Method: case CurrentInput.InternalMethod: case CurrentInput.MethodHarmony:
                    searchThread = new Thread(() => PopulateSearchMethod(searchText));
                    break;
                case CurrentInput.Type: case CurrentInput.TypeHarmony: case CurrentInput.SubClasses:
                    searchThread = new Thread(() => PopulateSearchType(searchText));
                    break;
                default:
                    searchThread = new Thread(() => PopulateSearchAssembly(searchText));
                    break;
            }
            
            searchThread.IsBackground = true;
            searchThread.Start();
        }

        private static void PopulateSearchMethod(string searchText)
        {
            if (searchText.Length <= 4) return;

            lock (sync)
            {
                currentlySearching = true;
            }

            searchText = searchText.ToLower();

            var names = new HashSet<string>();

            foreach (var type in GenTypes.AllTypes)
            {
                if (type.FullName.Contains("Cecil") || type.FullName.Contains("Analyzer")) continue;
                if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null) continue;


                foreach (var meth in type.GetMethods())
                {
                    if (!meth.HasMethodBody()) continue;
                    if (meth.IsGenericMethod || meth.ContainsGenericParameters) continue;

                    var str = string.Concat(meth.DeclaringType, ":", meth.Name);
                    if (str.ToLower()
                        .Contains(searchText))
                        names.Add(str);
                }
            }


            lock (sync)
            {
                cachedEntries = names;
                currentlySearching = false;
            }
        }

        private static void PopulateSearchType(string searchText)
        {
            if (searchText.Length <= 2) return;

            lock (sync)
            {
                currentlySearching = true;
            }

            searchText = searchText.ToLower();

            var names = new HashSet<string>();
            foreach (var type in GenTypes.AllTypes)
            {
                if (type.GetCustomAttribute<CompilerGeneratedAttribute>() != null) continue;


                var tyName = type.FullName;
                if (type.FullName.ToLower()
                    .Contains(searchText) && !type.FullName.Contains("Analyzer"))
                    names.Add(tyName);
            }

            lock (sync)
            {
                cachedEntries = names;
                currentlySearching = false;
            }
        }

        private static void PopulateSearchAssembly(string searchText)
        {
            var names = new HashSet<string>();
            var mods = ModInfoCache.AssemblyToModname.Values;

            foreach (var mod in mods.Where(mod => mod.ToLower()
                .Contains(searchText.ToLower()))) names.Add(mod);

            lock (sync)
            {
                cachedEntries = names;
                currentlySearching = false;
            }
        }
    }
}