using HugsLib.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QOLTweaksPack.rimworld
{
    public class SavedData : UtilityWorldObject
    {

        public List<string> TradeStockpileNames = new List<string>();
        public List<int> TradeStockpileMapIds = new List<int>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string>(ref TradeStockpileNames, "TradeStockpileNames", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look<int>(ref TradeStockpileMapIds, "TradeStockpileMapIds", LookMode.Value, LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (TradeStockpileNames == null) TradeStockpileNames = new List<string>();
                if (TradeStockpileMapIds == null) TradeStockpileMapIds = new List<int>();
            }
        }

        #region stockpile
        public bool StockpileIsTradeStockpile(Zone_Stockpile stockpile)
        {
            for(int i = 0; i < TradeStockpileNames.Count; i++)
            {
                if (TradeStockpileNames[i].Equals(stockpile.label) && TradeStockpileMapIds[i] == stockpile.Map.Index)
                    return true;
            }
            return false;
        }

        public void AddTradeStockpile(Zone_Stockpile stockpile)
        {
            TradeStockpileNames.Add(stockpile.label);
            TradeStockpileMapIds.Add(stockpile.Map.Index);
        }

        public void RemoveTradeStockpile(Zone_Stockpile stockpile)
        {
            for (int i = 0; i < TradeStockpileNames.Count; i++)
            {
                if(TradeStockpileNames[i].Equals(stockpile.label) && TradeStockpileMapIds[i] == stockpile.Map.Index)
                {
                    TradeStockpileNames.RemoveAt(i);
                    TradeStockpileMapIds.RemoveAt(i);
                    return;
                }
            }
        }

        public void TradeStockpileRenamed(Zone_Stockpile stockpile, string newName)
        {
            for (int i = 0; i < TradeStockpileNames.Count; i++)
            {
                if (TradeStockpileNames[i].Equals(stockpile.label) && TradeStockpileMapIds[i] == stockpile.Map.Index)
                {
                    TradeStockpileNames.RemoveAt(i);
                    TradeStockpileMapIds.RemoveAt(i);

                    TradeStockpileNames.Add(newName);
                    TradeStockpileMapIds.Add(stockpile.Map.Index);
                    return;
                }
            }
        }
        #endregion

    }
}
