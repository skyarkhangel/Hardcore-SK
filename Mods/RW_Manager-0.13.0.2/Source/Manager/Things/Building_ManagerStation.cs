// Manager/Building_ManagerStation.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:30

using RimWorld;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class Building_ManagerStation : Building_WorkTable
    {
        // just to give different versions a common interface.
    }

    // special blinking LED texture/glower logic + automagically doing jobs.
    public class Building_AIManager : Building_ManagerStation
    {
        private readonly Color[] _colors =
        {
            Color.white, Color.green, Color.red, Color.blue, Color.yellow, Color.cyan
        };

        private bool _glowDirty;
        private CompGlower _glower;
        private bool _graphicDirty;
        private bool _powered;
        private CompPowerTrader _powerTrader;
        private Comp_ManagerStation _managerStation;
        private Color _primaryColor = Color.black;
        private Color _secondaryColor = Color.black;
        private int _secondaryColourIndex;
        private Color _primaryBlinkerColour = Color.black;
        public override Color DrawColor => PrimaryColourBlinker;
        public override Color DrawColorTwo => SecondaryColour;

        public Color PrimaryColour
        {
            get { return _primaryColor; }
            set
            {
                ColorInt newColour = new ColorInt( (int)( value.r * 255 ), (int)( value.g * 255 ),
                                                   (int)( value.b * 255 ), 0 );
                Glower.Props.glowColor = newColour;
                _primaryColor = value;
                _glowDirty = true;
            }
        }

        public CompGlower Glower
        {
            get { return _glower ?? ( _glower = GetComp<CompGlower>() ); }
        }

        public CompPowerTrader PowerTrader
        {
            get { return _powerTrader ?? ( _powerTrader = PowerComp as CompPowerTrader ); }
        }

        public Comp_ManagerStation ManagerStation
        {
            get { return _managerStation ?? ( _managerStation = GetComp<Comp_ManagerStation>() ); }
        }

        public Color SecondaryColour
        {
            get { return _secondaryColor; }
            set
            {
                _secondaryColor = value;
                _graphicDirty = true;
            }
        }

        public int SecondaryColourIndex
        {
            get { return _secondaryColourIndex; }
            set
            {
                _secondaryColourIndex = value;
                SecondaryColour = _colors[_secondaryColourIndex];
            }
        }

        public bool Powered
        {
            get { return _powered; }
            set
            {
                _powered = value;
                Glower.SetLit( value );
                PrimaryColourBlinker = value ? PrimaryColour : Color.black;
                SecondaryColour = value ? _colors[_secondaryColourIndex] : Color.black;
            }
        }

        public Color PrimaryColourBlinker
        {
            get { return _primaryBlinkerColour; }
            set
            {
                _primaryBlinkerColour = value;
                _graphicDirty = true;
            }
        }

        public Building_AIManager()
        {
            _powerTrader = PowerComp as CompPowerTrader;
            _glower = GetComp<CompGlower>();
        }

        public override void Tick()
        {
            base.Tick();

            if ( Powered != PowerTrader.PowerOn )
            {
                Powered = PowerTrader.PowerOn;
            }

            if ( Powered )
            {
                int tick = Find.TickManager.TicksGame;

                // turn on glower
                Glower.SetLit( true );

                // random blinking on secondary
                if ( tick % 30 == Rand.RangeInclusive( 0, 25 ) )
                {
                    SecondaryColourIndex = ( SecondaryColourIndex + 1 ) % _colors.Length;
                }

                // primary colour
                if ( tick % ManagerStation.props.Speed == 0 )
                {
                    PrimaryColour = Manager.Get.DoWork() ? Color.green : Color.red;
                }

                // blinking on primary
                if ( tick % 30 < 25 )
                {
                    PrimaryColourBlinker = PrimaryColour;
                }
                else
                {
                    PrimaryColourBlinker = Color.black;
                }
            }

            // apply changes
            if ( _graphicDirty )
            {
                // update LED colours
                Notify_ColorChanged();
                _graphicDirty = false;
            }
            if ( _glowDirty )
            {
                // Update glow grid
                Find.GlowGrid.MarkGlowGridDirty( Position );

                // the following two should not be necesarry, but for some reason do seem to be.
                Find.MapDrawer.MapMeshDirty( Position, MapMeshFlag.GroundGlow );
                Find.MapDrawer.MapMeshDirty( Position, MapMeshFlag.Things );

                _glowDirty = false;
            }
        }
    }
}