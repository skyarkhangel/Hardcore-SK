using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

namespace NOQ_CoordReadout
{
    [HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
    static class MouseoverReadout_MouseoverReadoutOnGUI
    {
        static void Postfix()
        {
            if (Settings.ShowCoords)
            {
                if (Event.current.type != EventType.Repaint)
                {
                    return;
                }
                GenUI.DrawTextWinterShadow(new Rect(256f, (float)(UI.screenHeight - 256), -256f, 256f));
                Text.Font = GameFont.Small;
                GUI.color = new Color(1f, 1f, 1f, 0.8f);
                IntVec3 intVec = UI.MouseCell();
                if (intVec.InBounds(Find.CurrentMap))
                {
                    Widgets.Label(new Rect(Settings.xPosUI, (float)UI.screenHeight - Settings.yPosUI, 999f, 999f), intVec.ToString());
                }
                GUI.color = Color.white;
            }
        }
    }
}
