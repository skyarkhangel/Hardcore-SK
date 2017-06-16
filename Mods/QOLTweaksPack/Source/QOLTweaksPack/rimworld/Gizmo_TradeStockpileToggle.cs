using QOLTweaksPack.utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace QOLTweaksPack.rimworld
{
    class Gizmo_TradeStockpileToggle : Gizmo
    {
        public override float Width { get{ return 75; } }

        private Zone_Stockpile parentZone;

        public Gizmo_TradeStockpileToggle(Zone_Stockpile zone)
        {
            parentZone = zone;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            var gizmoRect = new Rect(topLeft.x, topLeft.y, Width, Width);
            Widgets.DrawWindowBackground(gizmoRect);

            Texture2D gizmoTex;
            string gizmoLabel;
            string gizmoMouseover;

            bool isTradeStockpile = QOLTweaksPack.savedData.StockpileIsTradeStockpile(parentZone);

            if (isTradeStockpile)
            {
                gizmoTex = TextureResources.tradeStockpileOn;
                gizmoLabel = "TradeStockpileOn_label".Translate();
                gizmoMouseover = "TradeStockpileOn_mouseOver".Translate();
            }
            else
            {
                gizmoTex = TextureResources.tradeStockpileOff;
                gizmoLabel = "TradeStockpileOff_label".Translate();
                gizmoMouseover = "TradeStockpileOff_mouseOver".Translate();
            }

            GUI.DrawTexture(gizmoRect, gizmoTex);

            // Log.Warning(gizmoRect.ToString());

            TooltipHandler.TipRegion(gizmoRect, gizmoMouseover);
            MouseoverSounds.DoRegion(gizmoRect, SoundDefOf.MouseoverCommand);

            WidgetsExtensions.DrawGizmoLabel(gizmoLabel, gizmoRect);

            bool interacted;

            if (Widgets.ButtonInvisible(gizmoRect, true))
            {
                if (isTradeStockpile)
                    QOLTweaksPack.savedData.RemoveTradeStockpile(parentZone);
                else
                    QOLTweaksPack.savedData.AddTradeStockpile(parentZone);
                interacted = true;
            }
            else
                interacted = false;

            return interacted ? new GizmoResult(GizmoState.Interacted, Event.current) : new GizmoResult(GizmoState.Clear);
        }
    }
}
