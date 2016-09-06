using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using RimWorld;
using Verse;
using UnityEngine;
using CommunityCoreLibrary;

namespace Fluffy_Tabs
{
    public class TextureChooser
    {
        readonly Texture2D[] options;
        int curIndex;

        public Texture2D Choice => options[curIndex];

        public TextureChooser( Texture2D[] options, int defaultIndex = 0 )
        {
            if ( options == null || options.Count() == 0 )
                throw new ArgumentOutOfRangeException( "options" );

            this.options = options;
            this.curIndex = defaultIndex;
        }

        public void DrawAt( Rect canvas, float buttonSize = 16f, float margin = Settings.Margin, bool highlight = true )
        {
            if ( canvas.width < canvas.height + 2 * ( buttonSize + margin ) )
                Log.ErrorOnce( "TextureChooser called with crappy dimensions: " + canvas, this.GetHashCode() );

            // create main icon rect
            Rect iconRect = new Rect( 0f, 0f, canvas.height, canvas.height );
            iconRect.center = canvas.center;

            // create button rects
            Rect backButtonRect = new Rect( iconRect.xMin - buttonSize - margin, iconRect.yMin + ( iconRect.height - buttonSize ) / 2f, buttonSize, buttonSize );
            Rect forwardButtonRect = new Rect( iconRect.xMax + margin, iconRect.yMin + ( iconRect.height - buttonSize ) / 2f, buttonSize, buttonSize );

            // draw icon and buttons
            GUI.DrawTexture( iconRect, Choice );
            if ( Widgets.ButtonImage( backButtonRect, Resources.LeftArrow, "FluffyTabs.TextureChooser.Previous".Translate() ) )
                Previous();
            if ( Widgets.ButtonImage( forwardButtonRect, Resources.RightArrow, "FluffyTabs.TextureChooser.Next".Translate() ) )
                Next();

            // highlight
            if ( highlight )
                Verse.Widgets.DrawHighlightIfMouseover( canvas );

            // scrowheel selection
            if (Mouse.IsOver( canvas) && Event.current.type == EventType.ScrollWheel )
            {
                if ( Event.current.delta.y > 0f )
                    Next();
                if ( Event.current.delta.y < 0f )
                    Previous();
                Event.current.Use();
            }
        }

        void Next()
        {
            curIndex = ( curIndex + 1 ) % options.Count();
        }

        void Previous()
        {
            curIndex--;
            if ( curIndex < 0 )
                curIndex = options.Count() - 1;
        }
    }
}
