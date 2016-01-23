// Manager/AreaAllowedGUI.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:28

using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FluffyManager
{
    internal class AreaAllowedGUI
    {

        public static Area DoAllowedAreaSelectors( Rect rect, Area areaIn,
                                                   AllowedAreaMode mode = AllowedAreaMode.Humanlike, float lrMargin = 0 )
        {
            Area areaIO = areaIn;
            DoAllowedAreaSelectors( rect, ref areaIO, mode, lrMargin );
            return areaIO;
        }

        // RimWorld.AreaAllowedGUI
        public static void DoAllowedAreaSelectors( Rect rect, ref Area area,
                                                   AllowedAreaMode mode = AllowedAreaMode.Humanlike, float lrMargin = 0 )
        {
            if ( lrMargin > 0 )
            {
                rect.xMin += lrMargin;
                rect.width -= lrMargin * 2;
            }


            List<Area> allAreas = Find.AreaManager.AllAreas;
            int areaCount = 1;
            for ( int i = 0; i < allAreas.Count; i++ )
            {
                if ( allAreas[i].AssignableAsAllowed( mode ) )
                {
                    areaCount++;
                }
            }
            float widthPerArea = rect.width / areaCount;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            Rect nullAreaRect = new Rect( rect.x, rect.y, widthPerArea, rect.height );
            DoAreaSelector( nullAreaRect, ref area, null );
            int areaIndex = 1;
            for ( int j = 0; j < allAreas.Count; j++ )
            {
                if ( allAreas[j].AssignableAsAllowed( mode ) )
                {
                    float xOffset = areaIndex * widthPerArea;
                    Rect areaRect = new Rect( rect.x + xOffset, rect.y, widthPerArea, rect.height );
                    DoAreaSelector( areaRect, ref area, allAreas[j] );
                    areaIndex++;
                }
            }
            Text.WordWrap = true;
            Text.Font = GameFont.Small;
        }



        // RimWorld.AreaAllowedGUI
        private static void DoAreaSelector( Rect rect, ref Area areaAllowed, Area area )
        {
            rect = rect.ContractedBy( 1f );
            GUI.DrawTexture( rect, area == null ? BaseContent.GreyTex : area.ColorTexture );
            Text.Anchor = TextAnchor.MiddleLeft;
            string text = AreaUtility.AreaAllowedLabel_Area( area );
            Rect rect2 = rect;
            rect2.xMin += 3f;
            rect2.yMin += 2f;
            Widgets.Label( rect2, text );
            if ( areaAllowed == area )
            {
                Widgets.DrawBox( rect, 2 );
            }
            if ( Mouse.IsOver( rect ) )
            {
                if ( area != null )
                {
                    area.MarkForDraw();
                }
                if ( Input.GetMouseButton( 0 ) &&
                     areaAllowed != area )
                {
                    areaAllowed = area;
                    SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera();
                }
            }
            TooltipHandler.TipRegion( rect, text );
        }
    }
}