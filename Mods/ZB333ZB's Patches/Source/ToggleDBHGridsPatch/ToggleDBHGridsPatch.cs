using System.Collections.Generic;
using System.Linq;
using DubsBadHygiene;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ToggleDBHGridsPatch
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            new Harmony("com.ZB333ZB.ToggleDBHGridsPatch").PatchAll();

            // Add debug logging
            var deepWaterResearch = DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.deepWaterResearchDefName, false);
            if (deepWaterResearch == null)
            {
                Log.Error($"[ToggleDBHGridsPatch] Could not find research project with defName: {ModSettingsLoader.Settings.deepWaterResearchDefName}");
            }
        }
    }

    public class ToggleDBHGridsPatchDef : Def
    {
        public string deepWaterResearchDefName;
    }

    public static class ModSettingsLoader
    {
        public static ToggleDBHGridsPatchDef Settings { get; private set; }

        static ModSettingsLoader()
        {
            Settings = DefDatabase<ToggleDBHGridsPatchDef>.GetNamed("ToggleDBHGridsPatch");
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

    public class WaterAndSewageGridStrategy : IGridStrategy
    {
        public bool IsActive { get; set; }
        public void DrawGrid() => GridDrawer.DrawWaterSewageGrid();
        public bool IsResearchCompleted() => true;
        public string GetResearchMessage() => string.Empty;
        public string GetOptionLabel() => "WaterAndSewageGridOption".Translate();
    }

    public class DeepWaterGridStrategy : IGridStrategy
    {
        public bool IsActive { get; set; }
        public void DrawGrid() => GridDrawer.DrawDeepWaterGrid();
        public bool IsResearchCompleted()
        {
            var research = DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.deepWaterResearchDefName, false);
            if (research == null)
            {
                Log.Warning($"[ToggleDBHGridsPatch] Research check failed - could not find: {ModSettingsLoader.Settings.deepWaterResearchDefName}");
                return false;
            }
            return research.IsFinished;
        }
        public string GetResearchMessage() => "DeepWaterResearchMessage".Translate(DefDatabase<ResearchProjectDef>.GetNamed(ModSettingsLoader.Settings.deepWaterResearchDefName).label);
        public string GetOptionLabel() => "DeepWaterGridOption".Translate();
    }

    public static class GridManager
    {
        private static readonly List<IGridStrategy> _strategies = new()
        {
            new WaterAndSewageGridStrategy(),
            new DeepWaterGridStrategy()
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
        public static void DrawWaterSewageGrid()
        {
            var currentMap = Find.CurrentMap;
            if (currentMap == null) return;

            var mapComponent = currentMap.Hygiene();
            mapComponent.WaterGrid.MarkForDraw();
            mapComponent.SewageGrid.MarkForDraw();
            mapComponent.MarkTowersForDraw = true;
        }

        public static void DrawDeepWaterGrid()
        {
            var currentMap = Find.CurrentMap;
            if (currentMap == null) return;

            var mapComponent = currentMap.Hygiene();
            mapComponent.DeepWaterGrid.MarkForDraw();
            mapComponent.MarkTowersForDraw = true;
        }
    }

    [HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
    public static class Patch_PlaySettings_DoPlaySettingsGlobalControls
    {
        private static readonly Texture2D ShowDBHGridsIcon = ContentFinder<Texture2D>.Get("DBH/UI/ShowDBHGrids");

        public static void Postfix(WidgetRow row, bool worldView)
        {
            if (worldView) return;

            if (row.ButtonIcon(ShowDBHGridsIcon, "ShowDBHGridsTooltip".Translate()))
            {
                var options = CreateMenuOptions();
                Find.WindowStack.Add(new FloatMenu(options));
            }

            DrawSmallCheckbox(row.FinalX + 12f, row.FinalY, GridManager.IsAnyGridActive);
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

    [HarmonyPatch(typeof(PlaceWorker_WaterGrid), nameof(PlaceWorker_WaterGrid.DrawGhost))]
    [HarmonyPatch(typeof(PlaceWorker_SewageGrid), nameof(PlaceWorker_SewageGrid.DrawGhost))]
    public static class Patch_PlaceWorker_DrawGhost
    {
        public static bool Prefix()
        {
            var waterAndSewageStrategy = GridManager.GetStrategies().OfType<WaterAndSewageGridStrategy>().FirstOrDefault();
            if (waterAndSewageStrategy?.IsActive != true) return true;

            GridDrawer.DrawWaterSewageGrid();
            return false;
        }
    }

    [HarmonyPatch(typeof(PlaceWorker_DeepWaterGrid), nameof(PlaceWorker_DeepWaterGrid.DrawGhost))]
    public static class Patch_PlaceWorker_DeepWaterGrid_DrawGhost
    {
        public static bool Prefix()
        {
            var deepWaterStrategy = GridManager.GetStrategies().OfType<DeepWaterGridStrategy>().FirstOrDefault();
            if (deepWaterStrategy?.IsActive != true) return true;

            GridDrawer.DrawDeepWaterGrid();
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
    public static class Patch_Game_ResetDBHGridSettings
    {
        public static void Prefix() => GridManager.ResetSettings();
    }
}
