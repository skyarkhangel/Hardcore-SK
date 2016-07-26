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
        #region Methods

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

        public static void DoAllowedAreaSelectorsMC( Rect rect, ref Dictionary<Area, bool> areas, float lrMargin = 0 )
        {
            if ( lrMargin > 0 )
            {
                rect.xMin += lrMargin;
                rect.width -= lrMargin * 2;
            }

            float widthPerArea = rect.width / areas.Count;
            Text.WordWrap = false;
            Text.Font = GameFont.Tiny;
            Rect nullAreaRect = new Rect( rect.x, rect.y, widthPerArea, rect.height );
            int areaIndex = 0;

            // need to use a 'clean' list of keys to iterate over when changing the dictionary values
            List<Area> _areas = new List<Area>( areas.Keys );

            foreach ( Area area in _areas )
            {
                float xOffset = areaIndex++ * widthPerArea;
                Rect areaRect = new Rect( rect.x + xOffset, rect.y, widthPerArea, rect.height );
                areas[area] = DoAreaSelector( areaRect, area, areas[area] );
            }

            Text.WordWrap = true;
            Text.Font = GameFont.Small;
        }

        private static bool DoAreaSelector( Rect rect, Area area, bool status )
        {
            rect = rect.ContractedBy( 1f );
            GUI.DrawTexture( rect, area == null ? BaseContent.GreyTex : area.ColorTexture );
            Text.Anchor = TextAnchor.MiddleLeft;
            string text = AreaUtility.AreaAllowedLabel_Area( area );
            Rect rect2 = rect;
            rect2.xMin += 3f;
            rect2.yMin += 2f;
            Widgets.Label( rect2, text );
            if ( status )
                Widgets.DrawBox( rect, 2 );
            if ( Mouse.IsOver( rect ) )
            {
                if ( area != null )
                    area.MarkForDraw();
                if ( Widgets.InvisibleButton( rect ) )
                {
                    SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera();
                    return !status;
                }
            }
            TooltipHandler.TipRegion( rect, text );
            return status;
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

        #endregion Methods
    }
}