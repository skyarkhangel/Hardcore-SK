using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using Rimefeller;

namespace ToggleRimefellerGridsPatch
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            new Harmony("com.ZB333ZB.ToggleRimefellerGridsPatch").PatchAll();

            // Add debug logging
            var oilResearch = DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.oilResearchDefName, false);
            var deepOilResearch = DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.deepOilResearchDefName, false);

            if (oilResearch == null)
            {
                Log.Error($"[ToggleRimefellerGridsPatch] Could not find research project with defName: {ModSettingsLoader.Settings.oilResearchDefName}");
            }
            if (deepOilResearch == null)
            {
                Log.Error($"[ToggleRimefellerGridsPatch] Could not find research project with defName: {ModSettingsLoader.Settings.deepOilResearchDefName}");
            }
        }
    }

    public class ToggleRimefellerGridsPatchDef : Def
    {
        public string oilResearchDefName;
        public string deepOilResearchDefName;
    }

    public static class ModSettingsLoader
    {
        public static ToggleRimefellerGridsPatchDef Settings { get; private set; }

        static ModSettingsLoader()
        {
            Settings = DefDatabase<ToggleRimefellerGridsPatchDef>.GetNamed("ToggleRimefellerGridsPatch");
        }
    }

    public interface IGridStrategy
    {
        bool IsActive { get; set; }
        void DrawGrid();
        bool IsResearchCompleted();
        string GetResearchMessage();
        string GetOptionLabel();
    }

    public class OilGridStrategy : IGridStrategy
    {
        public bool IsActive { get; set; }
        public void DrawGrid() => GridDrawer.DrawOilGrid();
        public bool IsResearchCompleted()
        {
            var research = DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.oilResearchDefName, false);
            if (research == null)
            {
                Log.Warning($"[ToggleRimefellerGridsPatch] Research check failed - could not find: {ModSettingsLoader.Settings.oilResearchDefName}");
                return false;
            }
            return research.IsFinished;
        }
        public string GetResearchMessage() => "OilResearchMessage".Translate(DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.oilResearchDefName).label);
        public string GetOptionLabel() => "OilGridOption".Translate();
    }

    public class DeepOilGridStrategy : IGridStrategy
    {
        public bool IsActive { get; set; }
        public void DrawGrid() => GridDrawer.DrawDeepOilGrid();
        public bool IsResearchCompleted()
        {
            var research = DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.deepOilResearchDefName, false);
            if (research == null)
            {
                Log.Warning($"[ToggleRimefellerGridsPatch] Research check failed - could not find: {ModSettingsLoader.Settings.deepOilResearchDefName}");
                return false;
            }
            return research.IsFinished;
        }
        public string GetResearchMessage() => "DeepOilResearchMessage".Translate(DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.deepOilResearchDefName).label);
        public string GetOptionLabel() => "DeepOilGridOption".Translate();
    }

    public static class GridManager
    {
        private static readonly List<IGridStrategy> _strategies = new()
        {
            new OilGridStrategy(),
            new DeepOilGridStrategy()
        };

        public static bool IsAnyGridActive => _strategies.Any(s => s.IsActive);

        public static void DrawActiveGrids()
        {
            foreach (var strategy in _strategies.Where(s => s.IsActive))
            {
                strategy.DrawGrid();
            }
        }

        public static void ResetSettings()
        {
            foreach (var strategy in _strategies)
            {
                strategy.IsActive = false;
            }
        }

        public static IEnumerable<IGridStrategy> GetStrategies() => _strategies;
    }

    public static class GridDrawer
    {
        public static void DrawOilGrid()
        {
            var currentMap = Find.CurrentMap;
            if (currentMap == null) return;

            var mapComponent = currentMap.Rimefeller();
            mapComponent.OilGrid.MarkFieldsForDraw();
            mapComponent.MarkTowersForDraw = true;
        }

        public static void DrawDeepOilGrid()
        {
            var currentMap = Find.CurrentMap;
            if (currentMap == null) return;

            var mapComponent = currentMap.Rimefeller();
            mapComponent.DeepOilGrid.MarkFieldsForDraw();
            mapComponent.MarkTowersForDraw = true;
        }
    }

    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public static class Patch_PlaySettings_DoPlaySettingsGlobalControls
    {
        private static readonly Texture2D ShowRimefellerGridsIcon = ContentFinder<Texture2D>.Get("Rimefeller/UI/ShowRimefellerGrids");

        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView) return;

            if (row.ButtonIcon(ShowRimefellerGridsIcon, "ShowRimefellerGridsTooltip".Translate()))
            {
                var options = CreateMenuOptions();
                Find.WindowStack.Add(new FloatMenu(options));
            }

            DrawSmallCheckbox(row.FinalX + 12f, row.FinalY - 0f, GridManager.IsAnyGridActive);
        }

        private static void DrawSmallCheckbox(float x, float y, bool active)
        {
            var checkboxRect = new Rect(x, y, 12f, 12f);
            GUI.DrawTexture(checkboxRect, active ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
        }

        private static List<FloatMenuOption> CreateMenuOptions()
        {
            return GridManager.GetStrategies().Select(CreateGridOption).ToList();
        }

        private static FloatMenuOption CreateGridOption(IGridStrategy strategy)
        {
            return new FloatMenuOption(strategy.GetOptionLabel(), () =>
            {
                if (strategy.IsResearchCompleted())
                {
                    strategy.IsActive = !strategy.IsActive;
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else
                {
                    Messages.Message(strategy.GetResearchMessage(), MessageTypeDefOf.RejectInput, historical: false);
                }
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0)
            {
                extraPartWidth = 24f,
                extraPartOnGUI = r =>
                {
                    var checkboxRect = new Rect(r.x + r.width - 20f, r.y + 6f, 18f, 18f);
                    GUI.DrawTexture(checkboxRect, strategy.IsActive ? Widgets.CheckboxOnTex : Widgets.CheckboxOffTex);
                    return false;
                }
            };
        }
    }

    [HarmonyPatch(typeof(PlaceWorker_WellHead), nameof(PlaceWorker_WellHead.DrawGhost))]
    public static class Patch_PlaceWorker_WellHead_DrawGhost
    {
        public static bool Prefix()
        {
            var oilStrategy = GridManager.GetStrategies().OfType<OilGridStrategy>().FirstOrDefault();
            if (oilStrategy?.IsActive != true) return true;

            GridDrawer.DrawOilGrid();
            return false;
        }
    }

    [HarmonyPatch(typeof(PlaceWorker_DeepWellHead), nameof(PlaceWorker_DeepWellHead.DrawGhost))]
    public static class Patch_PlaceWorker_DeepWellHead_DrawGhost
    {
        public static bool Prefix()
        {
            var deepOilStrategy = GridManager.GetStrategies().OfType<DeepOilGridStrategy>().FirstOrDefault();
            if (deepOilStrategy?.IsActive != true) return true;

            GridDrawer.DrawDeepOilGrid();
            return false;
        }
    }

    [HarmonyPatch(typeof(MapInterface), nameof(MapInterface.MapInterfaceOnGUI_BeforeMainTabs))]
    public static class Patch_MapInterface_MapInterfaceOnGUI_BeforeMainTabs
    {
        public static void Postfix()
        {
            if (GridManager.IsAnyGridActive)
            {
                GridManager.DrawActiveGrids();
            }
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    public static class Patch_Game_ResetRimefellerGridSettings
    {
        public static void Prefix() => GridManager.ResetSettings();
    }
}
