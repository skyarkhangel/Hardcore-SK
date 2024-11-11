using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using StorageSelector.Core.Storage;
using StorageSelector.UI.Logging;

namespace StorageSelector.Windows
{
    public class StorageSelectionWindow : Window
    {
        private readonly Bill bill;
        private readonly bool isInput;
        private readonly Action<Building_Storage> onStorageSelected;
        private readonly List<Zone_Stockpile> stockpiles;
        private readonly List<Building_Storage> storages;
        private Vector2 scrollPosition = Vector2.zero;
        private string searchText = "";
        private const float RowHeight = 30f;
        private const float ScrollBarWidth = 18f;

        public StorageSelectionWindow(
            Bill bill,
            bool isInput,
            Action<Building_Storage> onStorageSelected = null,
            List<Zone_Stockpile> stockpiles = null)
        {
            try
            {
                this.bill = bill;
                this.isInput = isInput;
                this.onStorageSelected = onStorageSelected;
                this.stockpiles = stockpiles;

                if (stockpiles == null)
                {
                    storages = bill.Map.listerBuildings.allBuildingsColonist
                        .OfType<Building_Storage>()
                        .Where(StorageUtility.IsValidStorageBuilding)
                        .ToList();
                }

                doCloseX = true;
                forcePause = true;
                absorbInputAroundWindow = true;
            }
            catch (Exception e)
            {
                UILogger.LogError("Error initializing storage selection window", e, "StorageWindow");
            }
        }

        public override Vector2 InitialSize => new(400f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            try
            {
                Text.Font = GameFont.Medium;
                var headerLabel = stockpiles != null ?
                    "ZB333ZB.StorageSelector.SelectStockpile".Translate() :
                    "ZB333ZB.StorageSelector.SelectStorage".Translate();
                var headerRect = new Rect(0f, 0f, inRect.width, 34f);
                Widgets.Label(headerRect, headerLabel);
                Text.Font = GameFont.Small;

                var searchRect = new Rect(0f, 40f, inRect.width, 30f);
                searchText = Widgets.TextField(searchRect, searchText);

                var listingRect = new Rect(0f, 80f, inRect.width, inRect.height - 80f);

                float totalHeight = GetTotalHeight();
                bool needsScrollbar = totalHeight > listingRect.height;

                float viewWidth = needsScrollbar ? listingRect.width - ScrollBarWidth : listingRect.width;
                var viewRect = new Rect(0f, 0f, viewWidth, totalHeight);

                Widgets.BeginScrollView(listingRect, ref scrollPosition, viewRect);

                try
                {
                    if (stockpiles != null)
                    {
                        DrawStockpileList(viewRect);
                    }
                    else
                    {
                        DrawStorageList(viewRect);
                    }
                }
                catch (Exception e)
                {
                    UILogger.LogError("Error drawing storage list", e, "StorageWindow");
                }

                Widgets.EndScrollView();
            }
            catch (Exception e)
            {
                UILogger.LogError("Error in DoWindowContents", e, "StorageWindow");
            }
        }

        private void DrawStorageList(Rect viewRect)
        {
            try
            {
                var filteredStorages = storages
                    .Where(s => string.IsNullOrEmpty(searchText) ||
                              StorageUtility.GetStorageLabel(s).ToLower().Contains(searchText.ToLower()));

                float curY = 0f;
                foreach (var storage in filteredStorages)
                {
                    if (curY + RowHeight >= scrollPosition.y && curY <= scrollPosition.y + viewRect.height)
                    {
                        var rowRect = new Rect(0f, curY, viewRect.width, RowHeight);

                        if (Mouse.IsOver(rowRect))
                        {
                            Widgets.DrawHighlight(rowRect);
                        }

                        if (Widgets.ButtonText(rowRect, StorageUtility.GetStorageLabel(storage)))
                        {
                            try
                            {
                                onStorageSelected?.Invoke(storage);
                                Close();
                            }
                            catch (Exception e)
                            {
                                UILogger.LogError("Error selecting storage", e, "StorageWindow");
                            }
                        }

                        var slotGroup = storage.GetSlotGroup();
                        var cellCount = slotGroup?.CellsList?.Count ?? 0;
                        TooltipHandler.TipRegion(rowRect, StorageUtility.GetStorageTooltip(storage));
                    }
                    curY += RowHeight;
                }
            }
            catch (Exception e)
            {
                UILogger.LogError("Error drawing storage list", e, "StorageWindow");
            }
        }

        private void DrawStockpileList(Rect viewRect)
        {
            try
            {
                var filteredStockpiles = stockpiles
                    .Where(s => string.IsNullOrEmpty(searchText) ||
                              s.label.ToLower().Contains(searchText.ToLower()));

                float curY = 0f;
                foreach (var stockpile in filteredStockpiles)
                {
                    if (curY + RowHeight >= scrollPosition.y && curY <= scrollPosition.y + viewRect.height)
                    {
                        var rowRect = new Rect(0f, curY, viewRect.width, RowHeight);

                        if (Mouse.IsOver(rowRect))
                        {
                            Widgets.DrawHighlight(rowRect);
                        }

                        if (Widgets.ButtonText(rowRect, stockpile.label))
                        {
                            try
                            {
                                bill.SetStoreMode(BillStoreModeDefOf.SpecificStockpile, stockpile);
                                Close();
                            }
                            catch (Exception e)
                            {
                                UILogger.LogError("Error selecting stockpile", e, "StorageWindow");
                            }
                        }

                        TooltipHandler.TipRegion(rowRect, StorageUtility.GetStorageTooltip(stockpile));
                    }
                    curY += RowHeight;
                }
            }
            catch (Exception e)
            {
                UILogger.LogError("Error drawing stockpile list", e, "StorageWindow");
            }
        }

        private float GetTotalHeight()
        {
            if (stockpiles != null)
            {
                return stockpiles.Count * RowHeight;
            }
            return storages.Count * RowHeight;
        }
    }
}
