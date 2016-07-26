// Manager/HSV_Helper.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-21 21:43

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FluffyManager
{
    internal class HSV_Helper
    {
        public static Color[] Range( int n )
        {
            Color[] cols = new Color[n];
            for ( int i = 0; i < n; i++ )
            {
                cols[i] = HSVtoRGB( (float)i / (float)n, 1f, 1f, 1f);
            }
            return cols;
        }


        /// <summary>
        /// From http://answers.unity3d.com/questions/701956/hsv-to-rgb-without-editorguiutilityhsvtorgb.html
        /// </summary>
        /// <param name="H"></param>
        /// <param name="S"></param>
        /// <param name="V"></param>
        /// <param name="A"></param>
        /// <returns>Color</returns>
        public static Color HSVtoRGB( float H, float S, float V, float A = 1f )
        {
            if ( S == 0f )
            {
                return new Color( V, V, V, A );
            }
            if ( V == 0f )
            {
                return new Color( 0f, 0f, 0f, A );
            }

            Color col = Color.black;
            float Hval = H * 6f;
            int sel = Mathf.FloorToInt( Hval );
            float mod = Hval - sel;
            float v1 = V * ( 1f - S );
            float v2 = V * ( 1f - S * mod );
            float v3 = V * ( 1f - S * ( 1f - mod ) );
            switch ( sel + 1 )
            {
                case 0:
                    col.r = V;
                    col.g = v1;
                    col.b = v2;
                    break;
                case 1:
                    col.r = V;
                    col.g = v3;
                    col.b = v1;
                    break;
                case 2:
                    col.r = v2;
                    col.g = V;
                    col.b = v1;
                    break;
                case 3:
                    col.r = v1;
                    col.g = V;
                    col.b = v3;
                    break;
                case 4:
                    col.r = v1;
                    col.g = v2;
                    col.b = V;
                    break;
                case 5:
                    col.r = v3;
                    col.g = v1;
                    col.b = V;
                    break;
                case 6:
                    col.r = V;
                    col.g = v1;
                    col.b = v2;
                    break;
                case 7:
                    col.r = V;
                    col.g = v3;
                    col.b = v1;
                    break;
            }
            col.r = Mathf.Clamp( col.r, 0f, 1f );
            col.g = Mathf.Clamp( col.g, 0f, 1f );
            col.b = Mathf.Clamp( col.b, 0f, 1f );
            col.a = Mathf.Clamp( A, 0f, 1f );
            return col;
        }
    }
}