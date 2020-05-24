// FloatMenu_Fixed.cs
// Copyright Karel Kroeze, 2018-2020

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FluffyResearchTree
{
    public class FloatMenu_Fixed : FloatMenu
    {
        private readonly Vector2 _position;

        public FloatMenu_Fixed( List<FloatMenuOption> options, Vector2 position, bool focus = false ) : base( options )
        {
            _position            = position;
            vanishIfMouseDistant = false;
            focusWhenOpened      = focus;
        }

        protected override void SetInitialSizeAndPosition()
        {
            var position                                                   = _position;
            if ( position.x + InitialSize.x > UI.screenWidth ) position.x  = UI.screenWidth  - InitialSize.x;
            if ( position.y + InitialSize.y > UI.screenHeight ) position.y = UI.screenHeight - InitialSize.y;
            windowRect = new Rect( position.x, position.y, InitialSize.x, InitialSize.y );
        }
    }
}