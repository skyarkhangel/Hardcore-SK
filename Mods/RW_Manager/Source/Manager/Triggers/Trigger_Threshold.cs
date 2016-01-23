// Manager/TriggerThreshold.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:25

using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class Trigger_Threshold : Trigger
    {
        public enum Ops
        {
            LowerThan,
            Equals,
            HigherThan
        }

        public static int DefaultCount = 500;
        public static int DefaultMaxUpperThreshold = 3000;
        
        public int Count;
        public int MaxUpperThreshold;
        public Ops Op;
        public ThingFilter ThresholdFilter;
        public Zone_Stockpile stockpile;

        public int CurCount
        {
            get { return Utilities.CountProducts( ThresholdFilter, stockpile ); }
        }

        public WindowTriggerThresholdDetails DetailsWindow
        {
            get
            {
                WindowTriggerThresholdDetails window = new WindowTriggerThresholdDetails
                {
                    Trigger = this,
                    closeOnClickedOutside = true,
                    draggable = true
                };
                return window;
            }
        }

        public bool IsValid
        {
            get { return ThresholdFilter.AllowedDefCount > 0; }
        }

        public virtual string OpString
        {
            get
            {
                switch ( Op )
                {
                    case Ops.LowerThan:
                        return " < ";

                    case Ops.Equals:
                        return " = ";

                    case Ops.HigherThan:
                        return " > ";

                    default:
                        return " ? ";
                }
            }
        }

        public override bool State
        {
            get
            {
                switch ( Op )
                {
                    case Ops.LowerThan:
                        return CurCount < Count;

                    case Ops.Equals:
                        return CurCount == Count;

                    case Ops.HigherThan:
                        return CurCount > Count;

                    default:
                        Log.Warning( "Trigger_ThingThreshold was defined without a correct operator" );
                        return true;
                }
            }
        }

        public override string StatusTooltip
        {
            get { return "FMP.ThresholdCount".Translate( CurCount, Count ); }
        }

        public Trigger_Threshold( ManagerJob_Production job )
        {
            Op = Ops.LowerThan;
            MaxUpperThreshold = job.MainProduct.MaxUpperThreshold;
            // TODO: Better way of setting sensible defaults?
            Count = MaxUpperThreshold / 20;
            ThresholdFilter = new ThingFilter();
            ThresholdFilter.SetDisallowAll();
            if ( job.MainProduct.ThingDef != null )
            {
                ThresholdFilter.SetAllow( job.MainProduct.ThingDef, true );
            }
            if ( job.MainProduct.CategoryDef != null )
            {
                ThresholdFilter.SetAllow( job.MainProduct.CategoryDef, true );
            }
        }

        public Trigger_Threshold( ManagerJob_Hunting job )
        {
            Op = Ops.LowerThan;
            MaxUpperThreshold = DefaultMaxUpperThreshold;
            Count = DefaultCount;
            ThresholdFilter = new ThingFilter();
            ThresholdFilter.SetDisallowAll();
            ThresholdFilter.SetAllow( Utilities_Hunting.RawMeat, true );
        }

        public Trigger_Threshold( ManagerJob_Forestry job )
        {
            Op = Ops.LowerThan;
            MaxUpperThreshold = DefaultMaxUpperThreshold;
            Count = DefaultCount;
            ThresholdFilter = new ThingFilter();
            ThresholdFilter.SetDisallowAll();
            ThresholdFilter.SetAllow( Utilities_Forestry.Wood, true );
        }

        public override void DrawProgressBar( Rect rect, bool active )
        {
            // bar always goes a little beyond the actual target
            int max = Math.Max( (int)( Count * 1.2f ), CurCount );

            // draw a box for the bar
            GUI.color = Color.gray;
            Widgets.DrawBox( rect.ContractedBy( 1f ) );
            GUI.color = Color.white;

            // get the bar rect
            Rect barRect = rect.ContractedBy( 2f );
            float unit = barRect.height / max;
            float markHeight = barRect.yMin + ( max - Count ) * unit;
            barRect.yMin += ( max - CurCount ) * unit;

            // draw the bar
            // if the job is active and pending, make the bar blueish green - otherwise white.
            Texture2D barTex = active
                ? Resources.BarBackgroundActiveTexture
                : Resources.BarBackgroundInactiveTexture;
            GUI.DrawTexture( barRect, barTex );

            // draw a mark at the treshold
            Widgets.DrawLineHorizontal( rect.xMin, markHeight, rect.width );

            TooltipHandler.TipRegion( rect, StatusTooltip );
        }

        public override void DrawTriggerConfig( ref Vector2 cur, float width, float entryHeight, bool alt = false )
        {
            // target threshold
            Rect thresholdLabelRect = new Rect( cur.x, cur.y, width, entryHeight );
            if ( alt )
            {
                Widgets.DrawAltRect( thresholdLabelRect );
            }
            Widgets.DrawHighlightIfMouseover( thresholdLabelRect );
            Utilities.Label( thresholdLabelRect,
                             "FMP.ThresholdCount".Translate( CurCount, Count ) + ":",
                             ThresholdFilter?.Summary() + 
                             "FMP.ThresholdCountTooltip".Translate( CurCount, Count ));
            
            // add a little icon to mark interactivity
            Rect searchIconRect = new Rect( thresholdLabelRect.xMax - Utilities.Margin - entryHeight, cur.y, entryHeight, entryHeight );
            if( searchIconRect.height > Utilities.SmallIconSize )
            {
                // center it.
                searchIconRect = searchIconRect.ContractedBy( ( searchIconRect.height - Utilities.SmallIconSize ) / 2 );
            }
            GUI.DrawTexture( searchIconRect, Resources.Search );

            cur.y += entryHeight;
            if ( Widgets.InvisibleButton( thresholdLabelRect ) )
            {
                Find.WindowStack.Add( DetailsWindow );
            }

            Rect thresholdRect = new Rect( cur.x, cur.y, width, Utilities.SliderHeight );
            if ( alt )
            {
                Widgets.DrawAltRect( thresholdRect );
            }
            Count = (int)GUI.HorizontalSlider( thresholdRect, Count, 0, MaxUpperThreshold );
            cur.y += Utilities.SliderHeight;
        }

        public override void ExposeData()
        {
            Scribe_Values.LookValue( ref Count, "Count" );
            Scribe_Values.LookValue( ref MaxUpperThreshold, "MaxUpperThreshold" );
            Scribe_Values.LookValue( ref Op, "Operator" );
            Scribe_Deep.LookDeep( ref ThresholdFilter, "ThresholdFilter" );
        }

        public override string ToString()
        {
            // TODO: Implement Trigger_Threshold.ToString()
            return "Trigger_Threshold.ToString() not implemented";
        }
    }
}