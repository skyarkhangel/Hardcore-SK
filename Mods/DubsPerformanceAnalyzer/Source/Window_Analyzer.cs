using ColourPicker;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;
using Verse;

namespace Analyzer
{
    using Profiling;

    public class Window_Analyzer : Window
    {
        public const float TOP_ROW_HEIGHT = 20f;
        public const float DRAGGABLE_RECT_DIM = 18f;

        public static Vector2 Initial = new Vector2(890, 650);
        public override Vector2 InitialSize => Initial;
        public override float Margin => 0;
        public static bool firstOpen = true;
        public static float GraphHeight = 220f;
        public static bool draggingGraph;


        public Window_Analyzer()
        {
            layer = WindowLayer.Super;
            forcePause = false;
            absorbInputAroundWindow = false;
            closeOnCancel = false;
            soundAppear = SoundDefOf.CommsWindow_Open;
            soundClose = SoundDefOf.CommsWindow_Close;
            doCloseButton = false;
            doCloseX = true;
            draggable = true;
            drawShadow = true;
            preventCameraMotion = false;
            onlyOneOfTypeAllowed = true;
            resizeable = true;
            closeOnCancel = false;
            closeOnAccept = false;
            draggable = false;
        }

        public override void SetInitialSizeAndPosition()
        {
            windowRect = new Rect(50f, (UI.screenHeight - InitialSize.y) / 2f, InitialSize.x, InitialSize.y);
            windowRect = windowRect.Rounded();
        }

        public override void PreOpen()
        {
            if (Analyzer.CurrentlyCleaningUp)
            {
                Find.WindowStack.TryRemove(this);
                ThreadSafeLogger.Error("[Analyzer] Analyzer is currently in the process of cleaning up - This is unlikely to happen, and shouldn't take long. please attempt to re-open the window in a couple seconds");
                return;
            }

            base.PreOpen();

            if (firstOpen) // If we have not been opened yet, load all our entries
            {
                LoadEntries();
                firstOpen = false;
            }

            Analyzer.BeginProfiling();

            if (Modbase.isPatched) return;

        
            Modbase.Harmony.Patch(AccessTools.Method(typeof(Root_Play), nameof(Root_Play.Update)),
                prefix: new HarmonyMethod(typeof(H_RootUpdate), nameof(H_RootUpdate.Prefix)),
                postfix: new HarmonyMethod(typeof(H_RootUpdate), nameof(H_RootUpdate.Postfix)));

            Modbase.Harmony.Patch(AccessTools.Method(typeof(TickManager), nameof(TickManager.DoSingleTick)),
#if DEBUG
            prefix: new HarmonyMethod(typeof(H_DoSingleTickUpdate), nameof(H_DoSingleTickUpdate.Prefix)),
#endif
            postfix: new HarmonyMethod(typeof(H_DoSingleTickUpdate), nameof(H_DoSingleTickUpdate.Postfix)));

            Modbase.isPatched = true;
            
        }

        public override void PostClose()
        {
            base.PostClose();

            Analyzer.EndProfiling();
            GUIController.ResetToSettings();

            Modbase.Settings.Write();

            // Pend the cleaning up of all of our state.
            if(!Settings.disableCleanup)
                Current.Game.GetComponent<GameComponent_Analyzer>().TimeTillCleanup = Modbase.TIME_SINCE_CLOSE_FOR_CLEANUP;
        }

        public static void LoadEntries()
        {
            List<Type> allEntries = GenTypes.AllTypes.Where(m => m.TryGetAttribute<Entry>(out _)).OrderBy(m => m.TryGetAttribute<Entry>().name).ToList();

            foreach (Type entryType in allEntries)
            {
                try
                {
                    Entry entry = entryType.TryGetAttribute<Entry>();
                    entry.Settings = new Dictionary<FieldInfo, Setting>();

                    foreach (FieldInfo fieldInfo in entryType.GetFields().Where(m => m.TryGetAttribute<Setting>(out _)))
                    {
                        Setting sett = fieldInfo.TryGetAttribute<Setting>();
                        entry.Settings.SetOrAdd(fieldInfo, sett);
                    }

                    entry.onMouseOver = AccessTools.Method(entryType, "MouseOver");
                    entry.onClick = AccessTools.Method(entryType, "Clicked");
                    entry.onSelect = AccessTools.Method(entryType, "Selected");
                    entry.checkBox = AccessTools.Method(entryType, "Checkbox");

                    entry.type = entryType;

                    // Find and append Entry to the correct Tab
                    if (!GUIController.Tab(entry.category).entries.ContainsKey(entry))
                        GUIController.Tab(entry.category).entries.Add(entry, entryType);
                }
                catch (Exception e)
                {
                    ThreadSafeLogger.Error(e.ToString());
                }
            }

            // Loop through our static instances and add them to the Correct Tab
            foreach (Entry entry in Entry.entries)
            {
                if (!GUIController.Tab(entry.category).entries.ContainsKey(entry))
                    GUIController.Tab(entry.category).entries.Add(entry, entry.type);
            }

        }

        public void HandleWindowDrag(Rect rect)
        {
            var DragRect = new Rect(rect.x, rect.y, rect.width - 50f, 18f);
            GUI.DragWindow(DragRect);

            DragRect = new Rect(rect.x, rect.y, 18f, rect.height);
            GUI.DragWindow(DragRect);

            DragRect = new Rect(rect.width - 18f, rect.y, 18f, rect.height);
            GUI.DragWindow(DragRect);

            DragRect = new Rect(rect.x, rect.y + rect.height - 18f, rect.width, 18f);
            GUI.DragWindow(DragRect);
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Handle Window Resizing
            HandleWindowDrag(inRect);

            var rect = inRect.ContractedBy(18f); // Adjust by our (removed) margin

            var profilersRect = rect;

            if (GUIController.CurrentProfiler == null)
            {
                Panel_Tabs.Draw(profilersRect, GUIController.Tabs);
                rect.AdjustHorizonallyBy(Panel_Tabs.width);

                if (GUIController.CurrentCategory == Category.Settings)
                {
                    Panel_Settings.Draw(rect);
                    return;
                }

                Panel_TopRow.Draw(rect.TopPartPixels(TOP_ROW_HEIGHT));
                rect.AdjustVerticallyBy(TOP_ROW_HEIGHT);

                Panel_Logs.DrawLogs(rect);

                return;
            }

            profilersRect.height -= GraphHeight + DRAGGABLE_RECT_DIM;

            Panel_Tabs.Draw(profilersRect, GUIController.Tabs);
            rect.AdjustHorizonallyBy(Panel_Tabs.width);
            
            Panel_TopRow.Draw(rect.TopPartPixels(TOP_ROW_HEIGHT));
            rect.AdjustVerticallyBy(TOP_ROW_HEIGHT);

            // If there is a current profiler, we need to adjust the height of the logs 
            rect.height -= GraphHeight + DRAGGABLE_RECT_DIM;

            Panel_Logs.DrawLogs(rect);

            // Move our rect down to just below the Logs
            rect.x -= Panel_Tabs.width;
            rect.width += Panel_Tabs.width;
            rect.y = rect.yMax;
            rect.height = GraphHeight + DRAGGABLE_RECT_DIM;

            var barRect = rect.TopPartPixels(DRAGGABLE_RECT_DIM);
            HandleGraphDrag(inRect, rect, barRect);

            rect.AdjustVerticallyBy(DRAGGABLE_RECT_DIM);

            Panel_BottomRow.Draw(rect, inRect);
        }

        public void HandleGraphDrag(Rect bigRect, Rect rect, Rect graphRect)
        {
            Widgets.DrawHighlightIfMouseover(graphRect);

            if (Input.GetMouseButtonDown(0) && Mouse.IsOver(graphRect) && draggingGraph == false)
            {
                draggingGraph = true;
            }

            if (draggingGraph)
            {
                GraphHeight = rect.height - ((Event.current.mousePosition.y - rect.y) + DRAGGABLE_RECT_DIM/2.0f);
            }

            GraphHeight = Mathf.Clamp(GraphHeight, 50f, bigRect.height - 100f);

            if (Input.GetMouseButtonUp(0))
            {
                draggingGraph = false;
            }
        }
    }
}