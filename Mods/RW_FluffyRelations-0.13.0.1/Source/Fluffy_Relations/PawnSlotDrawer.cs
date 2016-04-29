using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Relations
{
    public static class PawnSlotDrawer
    {
        #region Fields

        private static Dictionary<string, Rect> _labelRect = new Dictionary<string, Rect>();

        #endregion Fields

        #region Methods

        public static Rect CacheLabelRect( string name, Rect slot )
        {
            // get the width
            bool WW = Text.WordWrap;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            float width = Text.CalcSize( name ).x;
            Text.Font = GameFont.Small;
            Text.WordWrap = WW;

            // create rect
            Rect labelRect = new Rect(
                ( Settings.SlotSize - width ) / 2f + slot.xMin,
                slot.yMax - Settings.LabelHeight,
                width,
                Settings.LabelHeight );

            // cache and return rect
            _labelRect.Add( name, labelRect );
            return labelRect;
        }

        public static void ClearLabelRectCache()
        {
            _labelRect = new Dictionary<string, Rect>();
        }

        public static void DrawPawnInSlot( Pawn pawn, Rect slot )
        {
            // get the pawn's graphics set, and make sure it's resolved.
            PawnGraphicSet graphics = pawn.Drawer.renderer.graphics;
            if ( !graphics.AllResolved )
                graphics.ResolveAllGraphics();

            // draw base body
            GUI.color = graphics.nakedGraphic.Color;
            GUI.DrawTexture( slot, graphics.nakedGraphic.MatFront.mainTexture );

            // draw apparel
            bool drawHair = true;
            foreach ( var apparel in graphics.apparelGraphics )
            {
                if ( apparel.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead )
                {
                    drawHair = false;
                    continue;
                }
                GUI.color = apparel.graphic.Color;
                GUI.DrawTexture( slot, apparel.graphic.MatFront.mainTexture );
            }

            // draw head, offset further drawing up
            slot.y -= Settings.SlotSize * 1/4f;
            GUI.color = graphics.headGraphic.Color;
            GUI.DrawTexture( slot, graphics.headGraphic.MatFront.mainTexture );

            // draw hair OR hat
            if ( drawHair )
            {
                GUI.color = graphics.hairGraphic.Color;
                GUI.DrawTexture( slot, graphics.hairGraphic.MatFront.mainTexture );
            }
            else
            {
                foreach ( var apparel in graphics.apparelGraphics )
                {
                    Rect slot2 = slot;
                    if ( apparel.sourceApparel.def.apparel.LastLayer == ApparelLayer.Overhead )
                    {
                        GUI.color = apparel.graphic.Color;
                        GUI.DrawTexture( slot2, apparel.graphic.MatFront.mainTexture );
                    }
                }
            }

            // draw dead, frozen overlay

            // reset color, and then we're done here
            GUI.color = Color.white;
        }

        public static bool DrawSlot( this Pawn pawn, Rect slot, bool drawBG = true, bool drawLabel = true, bool drawLabelBG = true, bool drawHealthBar = true, bool drawStatusIcons = true, string label = "" )
        {
            // catch null pawn
            if ( pawn == null )
            {
                Widgets.Label( slot, "NULL" );
                return false;
            }

            // background square
            Rect bgRect = slot.ContractedBy( Settings.Inset );

            // name rect
            if ( label == "" )
                label = pawn.NameStringShort;
            Rect labelRect;
            if ( !_labelRect.TryGetValue( label, out labelRect ) )
                labelRect = CacheLabelRect( label, slot );

            // start drawing
            // draw background square
            if ( drawBG )
            {
                GUI.DrawTexture( bgRect, TexUI.GrayBg );
                Widgets.DrawBox( bgRect );
            }

            // draw pawn
            DrawPawnInSlot( pawn, slot );

            // draw label
            if ( drawLabel )
            {
                Text.Font = GameFont.Tiny;
                if ( drawLabelBG )
                    GUI.DrawTexture( labelRect, Resources.SlightlyDarkBG );
                Widgets.Label( labelRect, label );
                Text.Font = GameFont.Small;
            }

            // draw health bar

            // pass interactions back
            return Widgets.InvisibleButton( slot );
        }

        #endregion Methods
    }
}