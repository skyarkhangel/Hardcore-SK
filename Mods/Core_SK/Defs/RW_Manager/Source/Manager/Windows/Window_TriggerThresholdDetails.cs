// Manager/Window_TriggerThresholdDetails.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:29

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class WindowTriggerThresholdDetails : Window
    {
        public Vector2 FilterScrollPosition = Vector2.zero;
        public string Input;
        public Trigger_Threshold Trigger;
        public override Vector2 InitialWindowSize => new Vector2( 300f, 500 );
        ThingFilterUI filterUI = new ThingFilterUI();

        public override void DoWindowContents( Rect inRect )
        {
            // set up rects
            Rect filterRect = new Rect( inRect.ContractedBy( 6f ) );
            filterRect.height -= 2 * (Utilities.ListEntryHeight + Utilities.Margin);
            Rect zoneRect = new Rect(filterRect.xMin, filterRect.yMax + Utilities.Margin, filterRect.width, Utilities.ListEntryHeight);
            Rect buttonRect = new Rect( filterRect.xMin, zoneRect.yMax + Utilities.Margin, ( filterRect.width - Utilities.Margin ) / 2f, Utilities.ListEntryHeight );

            // draw thingfilter
            filterUI.DoThingFilterConfigWindow( filterRect, ref FilterScrollPosition, Trigger.ThresholdFilter );

            // draw zone selector
            StockpileGUI.DoStockpileSelectors(zoneRect, ref Trigger.stockpile);

            // draw operator button
            if ( Widgets.TextButton( buttonRect, Trigger.OpString ) )
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>
                {
                    new FloatMenuOption( "Lower than", delegate { Trigger.Op = Trigger_Threshold.Ops.LowerThan; } ),
                    new FloatMenuOption( "Equal to", delegate { Trigger.Op = Trigger_Threshold.Ops.Equals; } ),
                    new FloatMenuOption( "Greater than", delegate { Trigger.Op = Trigger_Threshold.Ops.HigherThan; } )
                };
                Find.WindowStack.Add( new FloatMenu( list ) );
            }
            

            // move operator button canvas for count input
            buttonRect.x = buttonRect.xMax + Utilities.Margin;

            // if current input is invalid color the element red
            Color oldColor = GUI.color;
            if ( !Input.IsInt() )
            {
                GUI.color = new Color( 1f, 0f, 0f );
            }
            else
            {
                Trigger.Count = int.Parse( Input );
                if ( Trigger.Count > Trigger.MaxUpperThreshold )
                {
                    Trigger.MaxUpperThreshold = Trigger.Count;
                }
            }

            // draw the input field
            Input = Widgets.TextField( buttonRect, Input );
            GUI.color = oldColor;

            // close on enter
            if ( Event.current.type == EventType.KeyDown &&
                 Event.current.keyCode == KeyCode.Return )
            {
                Event.current.Use();
                Find.WindowStack.TryRemove( this );
            }
        }

        public override void PreOpen()
        {
            base.PreOpen();
            Input = Trigger.Count.ToString();
        }
    }
}