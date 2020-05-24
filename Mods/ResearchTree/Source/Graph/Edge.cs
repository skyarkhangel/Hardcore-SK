// Edge.cs
// Copyright Karel Kroeze, 2018-2020

using System;
using UnityEngine;
using static FluffyResearchTree.Assets;
using static FluffyResearchTree.Constants;

namespace FluffyResearchTree
{
    public class Edge<T1, T2> where T1 : Node where T2 : Node
    {
        private T1 _in;
        private T2 _out;

        public Edge( T1 @in, T2 @out )
        {
            _in     = @in;
            _out    = @out;
            IsDummy = _out is DummyNode;
        }

        public T1 In
        {
            get => _in;
            set
            {
                _in     = value;
                IsDummy = _out is DummyNode;
            }
        }

        public T2 Out
        {
            get => _out;
            set
            {
                _out    = value;
                IsDummy = _out is DummyNode;
            }
        }

        public int   Span    => _out.X - _in.X;
        public float Length  => Mathf.Abs( _in.Yf - _out.Yf ) * ( IsDummy ? 10 : 1 );
        public bool  IsDummy { get; private set; }

        public int DrawOrder
        {
            get
            {
                if ( Out.Highlighted )
                    return 3;
                if ( Out.Completed )
                    return 2;
                if ( Out.Available )
                    return 1;
                return 0;
            }
        }

        public void Draw( Rect visibleRect )
        {
            if ( !In.IsVisible( visibleRect ) && !Out.IsVisible( visibleRect ) )
                return;

            var color = Out.EdgeColor;
            GUI.color = color;

            var left  = In.Right;
            var right = Out.Left;

            // if left and right are on the same level, just draw a straight line.
            if ( Math.Abs( left.y - right.y ) < Epsilon )
            {
                var line = new Rect( left.x, left.y - 2f, right.x - left.x, 4f );
                GUI.DrawTexture( line, Lines.EW );
            }

            // draw three line pieces and two curves.
            else
            {
                // determine top and bottom y positions
                var top    = Math.Min( left.y, right.y ) + NodeMargins.x / 4f;
                var bottom = Math.Max( left.y, right.y ) - NodeMargins.x / 4f;

                // straight bits
                // left to curve
                var leftToCurve = new Rect(
                    left.x,
                    left.y - 2f,
                    NodeMargins.x / 4f,
                    4f );
                GUI.DrawTexture( leftToCurve, Lines.EW );

                // curve to curve
                var curveToCurve = new Rect(
                    left.x + NodeMargins.x / 2f - 2f,
                    top,
                    4f,
                    bottom - top );
                GUI.DrawTexture( curveToCurve, Lines.NS );

                // curve to right
                var curveToRight = new Rect(
                    left.x           + NodeMargins.x / 4f * 3,
                    right.y          - 2f,
                    right.x - left.x - NodeMargins.x / 4f * 3,
                    4f );
                GUI.DrawTexture( curveToRight, Lines.EW );

                // curve positions
                var curveLeft = new Rect(
                    left.x + NodeMargins.x / 4f,
                    left.y - NodeMargins.x / 4f,
                    NodeMargins.x / 2f,
                    NodeMargins.x / 2f );
                var curveRight = new Rect(
                    left.x  + NodeMargins.x / 4f,
                    right.y - NodeMargins.x / 4f,
                    NodeMargins.x / 2f,
                    NodeMargins.x / 2f );

                // going down
                if ( left.y < right.y )
                {
                    GUI.DrawTextureWithTexCoords( curveLeft, Lines.Circle, new Rect( 0.5f, 0.5f, 0.5f, 0.5f ) );
                    // bottom right quadrant
                    GUI.DrawTextureWithTexCoords( curveRight, Lines.Circle, new Rect( 0f, 0f, 0.5f, 0.5f ) );
                    // top left quadrant
                }
                // going up
                else
                {
                    GUI.DrawTextureWithTexCoords( curveLeft, Lines.Circle, new Rect( 0.5f, 0f, 0.5f, 0.5f ) );
                    // top right quadrant
                    GUI.DrawTextureWithTexCoords( curveRight, Lines.Circle, new Rect( 0f, 0.5f, 0.5f, 0.5f ) );
                    // bottom left quadrant
                }
            }

            // draw the end arrow (if not dummy)
            if ( !IsDummy )
            {
                var end = new Rect(
                    right.x - 16f,
                    right.y - 8f,
                    16f,
                    16f );
                GUI.DrawTexture( end, Lines.End );
            }

            // or draw a line piece through the dummy
            else
            {
                var through = new Rect(
                    right.x,
                    right.y - 2,
                    NodeSize.x,
                    4f
                );
                GUI.DrawTexture( through, Lines.EW );
            }

            // reset color
            GUI.color = Color.white;
        }

        public override string ToString()
        {
            return _in + " -> " + _out;
        }
    }
}