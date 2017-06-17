using Harmony;
using QOLTweaksPack.rimworld;
using QOLTweaksPack.utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QOLTweaksPack.tweaks
{
    [HarmonyPatch(typeof(Dialog_Trade), "PostOpen")]
    static class Dialog_Trade_PostOpen_Postfix
    {
        [HarmonyPostfix]
        private static void PostOpen(Dialog_Trade __instance)
        {
            if (QOLTweaksPack.TradingStockpiles.Value == false)
                return;
            List<Tradeable> cachedTradeables = Reflection.GetFieldValue(__instance, "cachedTradeables") as List<Tradeable>;
            if (cachedTradeables == null)
            {
                Log.Warning("Could not grab cachedTradeables via reflection");
            }
            foreach (Tradeable tradeable in cachedTradeables)
            {
                foreach (Thing tradeableThing in tradeable.thingsColony)
                {
                    if (tradeableThing.holdingOwner != null)
                    {
                        SlotGroup storage = StoreUtility.GetSlotGroup(tradeableThing);
                        if (storage == null || storage.parent == null)
                            continue;
                        if (storage.parent is Zone_Stockpile)
                        {
                            if (QOLTweaksPack.savedData.StockpileIsTradeStockpile(storage.parent as Zone_Stockpile))
                            {
                                if (tradeable.CanAdjustBy(-tradeableThing.stackCount).Accepted)
                                    tradeable.AdjustBy(-tradeableThing.stackCount);
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_RenameZone), "NameIsValid")]
    static class Dialog_RenameZone_NameIsValid_Postfix
    {
        [HarmonyPostfix]
        private static void NameIsValid(Dialog_RenameZone __instance, string name, AcceptanceReport __result)
        {
            Zone zone = Reflection.GetFieldValue(__instance, "zone") as Zone;
            if (zone == null)
            {
                Log.Warning("Could not grab zone via reflection");
            }
            if (!(zone is Zone_Stockpile))
                return;
            if (__result.Accepted)
                QOLTweaksPack.savedData.TradeStockpileRenamed(zone as Zone_Stockpile, name);
        }     
    }

    [HarmonyPatch(typeof(Zone_Stockpile), "GetGizmos")]
    static class Zone_Stockpile_GetGizmos_Postfix
    {
        [HarmonyPostfix]
        private static void GetGizmos(Zone_Stockpile __instance, ref IEnumerable<Gizmo> __result)
        {
            if (QOLTweaksPack.TradingStockpiles.Value == false)
                return;
            List<Gizmo> results = new List<Gizmo>();
            foreach(Gizmo gizmo in __result)
            {
                results.Add(gizmo);
            }
            results.Add(new Gizmo_TradeStockpileToggle(__instance));
            __result = results;
        }
    }
}
