// Manager/UIThingFilterSearchable.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:29

using RimWorld;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class ThingFilterUI
    {
        private float viewHeight;

        public void DoThingFilterConfigWindow( Rect canvas, ref Vector2 scrollPosition, ThingFilter filter,
                                               ThingFilter parentFilter = null, int openMask = 1,
                                               bool buttonsAtBottom = false )
        {
            // respect your bounds!
            GUI.BeginGroup( canvas );
            canvas = canvas.AtZero();

            // set up buttons
            Text.Font = GameFont.Tiny;
            float width = canvas.width - 2f;
            Rect clearButtonRect = new Rect( canvas.x + 1f, canvas.y + 1f, width / 2f, 24f );
            Rect allButtonRect = new Rect( clearButtonRect.xMax + 1f, clearButtonRect.y, width / 2f, 24f );

            // offset canvas position for buttons.
            if ( buttonsAtBottom )
            {
                clearButtonRect.y = canvas.height - clearButtonRect.height;
                allButtonRect.y = canvas.height - clearButtonRect.height;
                canvas.yMax -= clearButtonRect.height;
            }
            else
            {
                canvas.yMin = clearButtonRect.height;
            }

            // draw buttons + logic
            if ( Widgets.TextButton( clearButtonRect, "ClearAll".Translate() ) )
            {
                filter.SetDisallowAll();
            }
            if ( Widgets.TextButton( allButtonRect, "AllowAll".Translate() ) )
            {
                filter.SetAllowAll( parentFilter );
            }
            Text.Font = GameFont.Small;

            // do list
            float curY = 2f;
            Rect viewRect = new Rect( 0f, 0f, canvas.width - 16f, viewHeight );

            // scrollview
            Widgets.BeginScrollView( canvas, ref scrollPosition, viewRect );

            // slider(s)
            DrawHitPointsFilterConfig( ref curY, viewRect.width, filter );
            DrawQualityFilterConfig( ref curY, viewRect.width, filter );

            // main listing
            Rect listingRect = new Rect( 0f, curY, 9999f, 9999f );
            float labelWidth = width - Widgets.CheckboxSize - Utilities.Margin;
            Listing_TreeThingFilter listingTreeThingFilter = new Listing_TreeThingFilter( listingRect, filter,
                                                                                          parentFilter,
                                                                                          labelWidth, true );
            TreeNode_ThingCategory node = ThingCategoryNodeDatabase.RootNode;
            if ( parentFilter != null )
            {
                if ( parentFilter.DisplayRootCategory == null )
                {
                    parentFilter.RecalculateDisplayRootCategory();
                }
                node = parentFilter.DisplayRootCategory;
            }

            // draw the actual thing
            listingTreeThingFilter.DoCategoryChildren( node, 0, openMask, true );
            listingTreeThingFilter.End();

            // update height.
            viewHeight = curY + listingTreeThingFilter.CurHeight;
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static void DrawHitPointsFilterConfig( ref float y, float width, ThingFilter filter )
        {
            if ( !filter.allowedHitPointsConfigurable )
            {
                return;
            }
            Rect rect = new Rect( 20f, y, width - 20f, 26f );
            FloatRange allowedHitPointsPercents = filter.AllowedHitPointsPercents;
            Widgets.FloatRange( rect, 1, ref allowedHitPointsPercents, 0f, 1f, ToStringStyle.PercentZero, "HitPoints" );
            filter.AllowedHitPointsPercents = allowedHitPointsPercents;
            y += 26f;
            y += 5f;
            Text.Font = GameFont.Small;
        }

        private static void DrawQualityFilterConfig( ref float y, float width, ThingFilter filter )
        {
            if ( !filter.allowedQualitiesConfigurable )
            {
                return;
            }
            Rect rect = new Rect( 20f, y, width - 20f, 26f );
            QualityRange allowedQualityLevels = filter.AllowedQualityLevels;
            Widgets.QualityRange( rect, 2, ref allowedQualityLevels );
            filter.AllowedQualityLevels = allowedQualityLevels;
            y += 26f;
            y += 5f;
            Text.Font = GameFont.Small;
        }
    }
}