using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Tabs
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    public static partial class Widgets
    {
        #region Methods

        public static void DrawBackground( Rect canvas, Color color, float opacity = -1f )
        {
            if ( opacity >= 0f && opacity <= 1f )
                color.a = opacity;

            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture( canvas, Resources.Solid );
            GUI.color = oldColor;
        }

        // switch width/height, keep lower left corner the same
        public static Rect Flip( this Rect rect )
        {
            return new Rect( rect.xMax, rect.yMax - rect.width, rect.height, rect.width );
        }

        public static bool ButtonImage( ref Vector2 curPos, Direction direction, Texture2D texture, string tooltip, float iconSize = Settings.IconSize, float margin = Settings.Margin )
        {
            // offset for position if going left or up
            if ( direction == Direction.Left )
                curPos.x -= iconSize;
            if ( direction == Direction.Up )
                curPos.y -= iconSize;

            // draw button
            bool wasClicked = ButtonImage( new Rect( curPos.x, curPos.y, iconSize, iconSize ), texture, tooltip );

            // update position for next button
            if ( direction == Direction.Up )
                curPos.y -= margin;
            if ( direction == Direction.Right )
                curPos.x += iconSize + margin;
            if ( direction == Direction.Down )
                curPos.y += iconSize + margin;
            if ( direction == Direction.Left )
                curPos.x -= margin;

            return wasClicked;
        }

        public static bool ButtonImage( Rect canvas, Texture2D texture, string tooltip )
        {
            TooltipHandler.TipRegion( canvas, tooltip );
            return Verse.Widgets.ButtonImage( canvas, texture );
        }

        public static void Label( Rect canvas, string text, Color color, string tip = "" )
        {
            Label( canvas, text, color, GameFont.Small, TextAnchor.UpperLeft, tip );
        }

        public static void Label( Rect canvas, string text, GameFont font, string tip = "" )
        {
            Label( canvas, text, Color.white, font, TextAnchor.UpperLeft, tip );
        }

        public static void Label( Rect canvas, string text, TextAnchor anchor, string tip = "" )
        {
            Label( canvas, text, Color.white, GameFont.Small, anchor, tip );
        }

        public static void Label( Rect canvas, string text, string tip = "" )
        {
            Label( canvas, text, Color.white, GameFont.Small, TextAnchor.UpperLeft, tip );
        }

        public static void Label( Rect canvas, string text, Color color, GameFont font, TextAnchor anchor, string tip = "" )
        {
            // cache old font settings
            Color oldColor = GUI.color;
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;

            // set new ones
            GUI.color = color;
            Text.Font = font;
            Text.Anchor = anchor;

            // draw label and tip
            Verse.Widgets.Label( canvas, text );
            if ( !tip.NullOrEmpty() )
                TooltipHandler.TipRegion( canvas, tip );

            // reset settings
            GUI.color = oldColor;
            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
        }

        public static void LabelVertical( Rect canvas, string label, Color color, GameFont font, TextAnchor anchor, string tip = "" )
        {
            // rotate 270 degrees around lower right of canvas.
            GUIUtility.RotateAroundPivot( 270, new Vector2( canvas.xMax, canvas.yMax ) );

            // flip canvas
            var rotatedCanvas = canvas.Flip();

            // write label
            Label( rotatedCanvas, label, color, font, anchor, tip );

            // reset rotation
            GUI.matrix = Matrix4x4.identity;
        }

        public static float NoWrapWidth( this string text )
        {
            var oldWW = Text.WordWrap;
            Text.WordWrap = false;
            float result = Text.CalcSize( text ).x;
            Text.WordWrap = oldWW;
            return result;
        }

        public static void Paragraph( ref Vector2 curPos, float width, string label, Color color, GameFont font = GameFont.Small, TextAnchor anchor = TextAnchor.UpperLeft, string tip = "", float lineMargin = 6f )
        {
            var oldFont = Text.Font;
            Text.Font = font;
            float height = Text.CalcHeight( label, width );

            Label( new Rect( curPos.x, curPos.y, width, height ), label, color, font, anchor, tip );
            Text.Font = oldFont;
            curPos.y += height + lineMargin;
        }

        public static string StringJoin( this IEnumerable<string> enmumerable, string seperator = ", " )
        {
            return String.Join( seperator, enmumerable.ToArray() );
        }

        #endregion Methods
    }
}