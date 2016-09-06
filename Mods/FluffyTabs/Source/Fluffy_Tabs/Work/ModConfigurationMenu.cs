using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityCoreLibrary;
using CommunityCoreLibrary.UI;
using UnityEngine;
using Verse;

namespace Fluffy_Tabs
{
    public class ModConfigurationMenu_Work : ModConfigurationMenu
    {
        LabeledInput_IntClamped max_priority = new LabeledInput_IntClamped( Settings.MaxPriority, "FluffyTabs.MaxPriority".Translate(), new IntRange(4,9), "FluffyTabs.MaxPriorityTip".Translate() );
        
        public override float DoWindowContents( Rect rect )
        {
            // maximum priority
            Rect optionRect = new Rect( rect.xMin, rect.yMin, rect.width, 30f );
            max_priority.Draw( optionRect );
            Settings.MaxPriority = max_priority.Value;

            // color coded passions
            optionRect.y += 30f;
            Verse.Widgets.CheckboxLabeled( optionRect, "FluffyTabs.ColorCodedPassions".Translate(), ref Settings.ColorCodedPassions );
            TooltipHandler.TipRegion( optionRect, "FluffyTabs.ColorCodedPassionsTip".Translate() );

            // 24-hour mode
            optionRect.y += 30f;
            bool twentyFourHourMode = MapComponent_Priorities.Instance.TwentyFourHourMode;
            Verse.Widgets.CheckboxLabeled( optionRect, "FluffyTabs.24HourMode".Translate(), ref twentyFourHourMode );
            MapComponent_Priorities.Instance.TwentyFourHourMode = twentyFourHourMode;
            TooltipHandler.TipRegion( optionRect, "FluffyTabs.24HourModeTip".Translate() );

            // return height
            return optionRect.yMax - rect.yMin;
        }

        public override void ExposeData()
        {
            Scribe_Values.LookValue( ref Settings.ColorCodedPassions, "ColorCodedPassions", true );
            Scribe_Values.LookValue( ref Settings.MaxPriority, "MaxPriority", 9 );
            
            // 24 hour mode is saved in MapComp_Priorities.
        }
    }

    public class LabeledInput_IntClamped : LabeledInput_Int
    {
        private IntRange range;

        public LabeledInput_IntClamped( int value, string label, IntRange range, string tip = "" ) : this( value, label, range, tip, Color.white, GameFont.Small, TextAnchor.UpperLeft ) { }

        public LabeledInput_IntClamped( int value, string label, IntRange range, string tip, Color color, GameFont font, TextAnchor anchor ) : base( value, label, tip, color, font, anchor )
        {
            this.range = range;
            validator = Validator;
        }

        private bool Validator( string s )
        {
            int result;
            bool valid = int.TryParse( s, out result );
            return valid && result >= range.min && result <= range.max;
        }
    }
}
