// DummyNode.cs
// Copyright Karel Kroeze, 2018-2020

using System.Linq;
using UnityEngine;

namespace FluffyResearchTree
{
    public class DummyNode : Node
    {
        #region Overrides of Node

        public override string Label => "DUMMY: " + ( Parent?.Label ?? "??" ) + " -> " + ( Child?.Label ?? "??" );

        #endregion

        #region Overrides of Node

#if DEBUG_DUMMIES
        public override void Draw()
        {
            // cop out if off-screen
            var screen = new Rect( MainTabWindow_ResearchTree._scrollPosition.x,
                                   MainTabWindow_ResearchTree._scrollPosition.y, Screen.width, Screen.height - 35 );
            if ( Rect.xMin > screen.xMax ||
                 Rect.xMax < screen.xMin ||
                 Rect.yMin > screen.yMax ||
                 Rect.yMax < screen.yMin )
            {
                return;
            }

            Widgets.DrawBox( Rect );
            Widgets.Label( Rect, Label );
        }
#endif

        #endregion

        public ResearchNode Parent
        {
            get
            {
                var parent = InNodes.FirstOrDefault() as ResearchNode;
                if ( parent != null )
                    return parent;

                var dummyParent = InNodes.FirstOrDefault() as DummyNode;

                return dummyParent?.Parent;
            }
        }

        public ResearchNode Child
        {
            get
            {
                var child = OutNodes.FirstOrDefault() as ResearchNode;
                if ( child != null )
                    return child;

                var dummyChild = OutNodes.FirstOrDefault() as DummyNode;

                return dummyChild?.Child;
            }
        }

        public override bool  Completed   => OutNodes.FirstOrDefault()?.Completed   ?? false;
        public override bool  Available   => OutNodes.FirstOrDefault()?.Available   ?? false;
        public override bool  Highlighted => OutNodes.FirstOrDefault()?.Highlighted ?? false;
        public override Color Color       => OutNodes.FirstOrDefault()?.Color       ?? Color.white;
        public override Color EdgeColor   => OutNodes.FirstOrDefault()?.EdgeColor   ?? Color.white;
    }
}