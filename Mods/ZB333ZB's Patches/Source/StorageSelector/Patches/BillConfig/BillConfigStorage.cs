using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using StorageSelector.Windows;
using StorageSelector.Core;

namespace StorageSelector.Patches.BillConfig
{
    [HarmonyPatch]
    public static class BillConfigStorage
    {
        private static readonly FieldInfo StoreZoneField = AccessTools.Field(typeof(Bill_Production), "storeZone");

        [HarmonyPatch(typeof(Bill_Production), "SetStoreMode")]
        public static class SetStoreMode_Patch
        {
            public static void Prefix(Bill_Production __instance, BillStoreModeDef mode, Zone_Stockpile zone)
            {
                try
                {
                    var storage = ExtendedBillDataStorage.GetStorage();
                    if (storage == null) return;

                    if (mode != null || zone != null)
                    {
                        var currentStorage = storage.GetOutputStorage(__instance);
                        if (currentStorage != null)
                        {
                            storage.SetOutputStorage(__instance, null);
                            Messages.Message("ZB333ZB.StorageSelector.StorageCleared".Translate(__instance.Label),
                                MessageTypeDefOf.SilentInput);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[StorageSelector] Error in SetStoreMode Prefix: {e}");
                }
            }
        }

        public static string GetInputStorageButtonText(Bill_Production bill, Building_Storage storage)
        {
            if (storage != null)
                return "ZB333ZB.StorageSelector.TakeFrom".Translate(storage.SlotYielderLabel());

            if (StoreZoneField?.GetValue(bill) is Zone_Stockpile storeZone)
                return "ZB333ZB.StorageSelector.TakeFrom".Translate(storeZone.label);

            return "TakeFromAnyStorage".Translate();
        }

        public static string GetOutputStorageButtonText(Bill_Production bill, Building_Storage storage)
        {
            if (storage != null)
                return "ZB333ZB.StorageSelector.TakeTo".Translate(storage.SlotYielderLabel());

            var storeMode = bill.GetStoreMode();
            if (storeMode == BillStoreModeDefOf.DropOnFloor)
                return "DropOnFloor".Translate();
            if (storeMode == BillStoreModeDefOf.BestStockpile)
                return "BestStockpile".Translate();

            var storeZone = bill.GetStoreZone();
            if (storeZone != null)
                return "ZB333ZB.StorageSelector.TakeTo".Translate(storeZone.label);

            return "TakeToStockpile".Translate();
        }

        public static void ShowInputStorageMenu(Bill_Production bill, ExtendedBillDataStorage storage)
        {
            var floatMenu = new List<FloatMenuOption>
            {
                new("TakeFromAnyStorage".Translate(), () =>
                {
                    StoreZoneField?.SetValue(bill, null);
                    storage.SetInputStorage(bill, null);
                })
            };

            var storages = bill.Map.listerBuildings.allBuildingsColonist
                .OfType<Building_Storage>()
                .Where(s => s.GetSlotGroup()?.parent == s)
                .ToList();

            if (storages.Any())
            {
                floatMenu.Add(new FloatMenuOption("ZB333ZB.StorageSelector.Storages".Translate(), () =>
                {
                    Find.WindowStack.Add(new StorageSelectionWindow(bill, true, storage =>
                    {
                        StoreZoneField?.SetValue(bill, null);
                        ExtendedBillDataStorage.GetStorage()?.SetInputStorage(bill, storage);
                    }));
                }));
            }

            var stockpiles = bill.Map.zoneManager.AllZones.OfType<Zone_Stockpile>().ToList();
            if (stockpiles.Any())
            {
                floatMenu.Add(new FloatMenuOption("ZB333ZB.StorageSelector.Stockpiles".Translate(), () =>
                {
                    Find.WindowStack.Add(new StorageSelectionWindow(bill, true, null, stockpiles));
                }));
            }
            else
            {
                floatMenu.Add(new FloatMenuOption("NoStockpileZones".Translate(), null)
                {
                    Disabled = true
                });
            }

            Find.WindowStack.Add(new FloatMenu(floatMenu));
        }

        public static void ShowOutputStorageMenu(Bill_Production bill, ExtendedBillDataStorage storage)
        {
            var floatMenu = new List<FloatMenuOption>
            {
                new("DropOnFloor".Translate(), () =>
                {
                    bill.SetStoreMode(BillStoreModeDefOf.DropOnFloor, null);
                    storage.SetOutputStorage(bill, null);
                }),
                new("BestStockpile".Translate(), () =>
                {
                    bill.SetStoreMode(BillStoreModeDefOf.BestStockpile, null);
                    storage.SetOutputStorage(bill, null);
                })
            };

            var storages = bill.Map.listerBuildings.allBuildingsColonist
                .OfType<Building_Storage>()
                .Where(s => s.GetSlotGroup()?.parent == s)
                .ToList();

            if (storages.Any())
            {
                floatMenu.Add(new FloatMenuOption("ZB333ZB.StorageSelector.Storages".Translate(), () =>
                {
                    Find.WindowStack.Add(new StorageSelectionWindow(bill, false, storage =>
                    {
                        ExtendedBillDataStorage.GetStorage()?.SetOutputStorage(bill, storage);
                    }));
                }));
            }

            var stockpiles = bill.Map.zoneManager.AllZones.OfType<Zone_Stockpile>().ToList();
            if (stockpiles.Any())
            {
                floatMenu.Add(new FloatMenuOption("ZB333ZB.StorageSelector.Stockpiles".Translate(), () =>
                {
                    Find.WindowStack.Add(new StorageSelectionWindow(bill, false, null, stockpiles));
                }));
            }
            else
            {
                floatMenu.Add(new FloatMenuOption("NoStockpileZones".Translate(), null)
                {
                    Disabled = true
                });
            }

            Find.WindowStack.Add(new FloatMenu(floatMenu));
        }
    }
}
