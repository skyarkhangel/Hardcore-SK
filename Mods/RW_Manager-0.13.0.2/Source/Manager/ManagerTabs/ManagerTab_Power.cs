// Manager/ManagerTab_Power.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-12-02 21:12

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class ManagerTab_Power : ManagerTab, IExposable
    {
        private List<List<CompPowerBattery>> _batteries;
        private List<ThingDef> _batteryDefs;
        private Vector2 _consumptionScrollPos = Vector2.zero;
        private Vector2 _overallScrollPos = Vector2.zero;
        private Vector2 _productionScrollPos = Vector2.zero;
        private List<ThingDef> _traderDefs;
        private List<List<CompPowerTrader>> _traders;
        private History overallHistory;
        private History tradingHistory;
        public override string Label => "FME.Power".Translate();
        public override Texture2D Icon => Resources.IconPower;
        public override IconAreas IconArea => IconAreas.Middle;
        public override ManagerJob Selected { get; set; }

        public ManagerTab_Power()
        {
            // get list of thingdefs set to use the power comps - this should be static throughout the game (barring added mods midgame)
            _traderDefs = GetTraderDefs().ToList();
            _batteryDefs = GetBatteryDefs().ToList();

            // get a dictionary of powercomps actually existing on the map for each thingdef.
            RefreshCompLists();

            // set up the history trackers.
            tradingHistory =
                new History(
                    _traderDefs.Select(
                        def => new ThingCount( def, Find.ListerBuildings.AllBuildingsColonistOfDef( def ).Count() ) )
                               .ToArray() )
                {
                    DrawOptions = false,
                    DrawInlineLegend = false,
                    Suffix = "W",
                    DrawInfoInBar = true,
                    DrawMaxMarkers = true
                };

            overallHistory = new History( new[] { "Production", "Consumption", "Batteries" } )
            {
                DrawOptions = false,
                DrawInlineLegend = false,
                Suffix = "W",
                DrawIcons = false,
                DrawCounts = false,
                DrawInfoInBar = true,
                DrawMaxMarkers = true,
                MaxPerChapter = true
            };
        }

        public void ExposeData()
        {
            Scribe_Deep.LookDeep( ref tradingHistory, "tradingHistory" );
            Scribe_Deep.LookDeep( ref overallHistory, "overallHistory" );
        }

        public override void Tick()
        {
            base.Tick();

            // once in a while, update the list of comps, and history thingcounts + theoretical maxes (where known).
            if ( Find.TickManager.TicksGame % 2000 == 0 )
            {
                // get all existing comps for all building defs that have power related comps (in essence, get all powertraders)
                RefreshCompLists();

                // update these counts in the history tracker + reset maxes if count changed.
                tradingHistory.UpdateThingCountAndMax( _traders.Select( list => list.Count ).ToArray(),
                                                       _traders.Select( list => 0 ).ToArray() );

                // update theoretical max for batteries, and reset observed max.
                overallHistory.UpdateMax( 0, 0,
                                          (int)
                                              _batteries.Sum(
                                                  list => list.Sum( battery => battery.Props.storedEnergyMax ) ) );
            }

            // update the history tracker.
            int[] trade = GetCurrentTrade();
            tradingHistory.Update( trade );
            overallHistory.Update( trade.Where( i => i > 0 ).Sum(), trade.Where( i => i < 0 ).Sum( Math.Abs ),
                                   GetCurrentBatteries().Sum() );
        }

        private int[] GetCurrentTrade()
        {
            return _traders.Select( list => (int)list.Sum( trader => trader.PowerOutput ) ).ToArray();
        }

        private int[] GetCurrentBatteries()
        {
            return _batteries.Select( list => (int)list.Sum( battery => battery.StoredEnergy ) ).ToArray();
        }

        private IEnumerable<ThingDef> GetTraderDefs()
        {
            return from td in DefDatabase<ThingDef>.AllDefsListForReading
                   where td.HasComp( typeof (CompPowerTrader) )
                   select td;
        }

        private IEnumerable<ThingDef> GetBatteryDefs()
        {
            return from td in DefDatabase<ThingDef>.AllDefsListForReading
                   where td.HasComp( typeof (CompPowerBattery) )
                   select td;
        }

        private void RefreshCompLists()
        {
            // get list of power trader comps per def for consumers and producers.
            _traders = _traderDefs.Select( def => Find.ListerBuildings.AllBuildingsColonistOfDef( def )
                                                      .Select( t => t.GetComp<CompPowerTrader>() )
                                                      .ToList() )
                                  .ToList();

            // get list of lists of powertrader comps per thingdef.
            _batteries = _batteryDefs
                .Select( v => Find.ListerBuildings.AllBuildingsColonistOfDef( v )
                                  .Select( t => t.GetComp<CompPowerBattery>() )
                                  .ToList() )
                .ToList();
        }

        public override void DoWindowContents( Rect canvas )
        {
            // set up rects
            Rect overviewRect = new Rect( 0f, 0f, canvas.width, 150f );
            Rect consumtionRect = new Rect( 0f, overviewRect.height + Utilities.Margin,
                                            ( canvas.width - Utilities.Margin ) / 2f,
                                            canvas.height - overviewRect.height - Utilities.Margin );
            Rect productionRect = new Rect( consumtionRect.xMax + Utilities.Margin,
                                            overviewRect.height + Utilities.Margin,
                                            ( canvas.width - Utilities.Margin ) / 2f,
                                            canvas.height - overviewRect.height - Utilities.Margin );

            // draw area BG's
            Widgets.DrawMenuSection( overviewRect );
            Widgets.DrawMenuSection( consumtionRect );
            Widgets.DrawMenuSection( productionRect );

            // draw contents
            DrawOverview( overviewRect );
            DrawConsumption( consumtionRect );
            DrawProduction( productionRect );
        }

        private void DrawProduction( Rect canvas )
        {
            // setup rects 
            Rect plotRect = new Rect( canvas.xMin, canvas.yMin, canvas.width, ( canvas.height - Utilities.Margin ) / 2f );
            Rect legendRect = new Rect( canvas.xMin, plotRect.yMax + Utilities.Margin, canvas.width,
                                        ( canvas.height - Utilities.Margin ) / 2f );

            // draw the plot
            tradingHistory.DrawPlot( plotRect, positiveOnly: true );

            // draw the detailed legend
            tradingHistory.DrawDetailedLegend( legendRect, ref _productionScrollPos, null, true );
        }

        private void DrawConsumption( Rect canvas )
        {
            // setup rects 
            Rect plotRect = new Rect( canvas.xMin, canvas.yMin, canvas.width, ( canvas.height - Utilities.Margin ) / 2f );
            Rect legendRect = new Rect( canvas.xMin, plotRect.yMax + Utilities.Margin, canvas.width,
                                        ( canvas.height - Utilities.Margin ) / 2f );

            // draw the plot
            tradingHistory.DrawPlot( plotRect, negativeOnly: true );

            // draw the detailed legend
            tradingHistory.DrawDetailedLegend( legendRect, ref _consumptionScrollPos, null, false, true );
        }

        private void DrawOverview( Rect canvas )
        {
            // setup rects 
            Rect legendRect = new Rect( canvas.xMin, canvas.yMin, ( canvas.width - Utilities.Margin ) / 2f,
                                        canvas.height - Utilities.ButtonSize.y - Utilities.Margin );
            Rect plotRect = new Rect( legendRect.xMax + Utilities.Margin, canvas.yMin,
                                      ( canvas.width - Utilities.Margin ) / 2f, canvas.height );
            Rect buttonsRect = new Rect( canvas.xMin, legendRect.yMax + Utilities.Margin,
                                         ( canvas.width - Utilities.Margin ) / 2f, Utilities.ButtonSize.y );

            // draw the plot
            overallHistory.DrawPlot( plotRect );

            // draw the detailed legend
            overallHistory.DrawDetailedLegend( legendRect, ref _overallScrollPos, null );

            // draw period switcher
            Rect periodRect = buttonsRect;
            periodRect.width /= 2f;

            // label
            Utilities.Label( periodRect, "FME.PeriodShown".Translate( tradingHistory.periodShown.ToString() ),
                             "FME.PeriodShownTooltip".Translate( tradingHistory.periodShown.ToString() ) );

            // mark interactivity
            Rect searchIconRect = periodRect;
            searchIconRect.xMin = searchIconRect.xMax - searchIconRect.height;
            if ( searchIconRect.height > Utilities.SmallIconSize )
            {
                // center it.
                searchIconRect = searchIconRect.ContractedBy( ( searchIconRect.height - Utilities.SmallIconSize ) / 2 );
            }
            GUI.DrawTexture( searchIconRect, Resources.Search );
            Widgets.DrawHighlightIfMouseover( periodRect );
            if ( Widgets.InvisibleButton( periodRect ) )
            {
                List<FloatMenuOption> periodOptions = new List<FloatMenuOption>();
                for ( int i = 0; i < History.periods.Length; i++ )
                {
                    History.Period period = History.periods[i];
                    periodOptions.Add( new FloatMenuOption( period.ToString(), delegate
                    {
                        tradingHistory.periodShown = period;
                        overallHistory.periodShown = period;
                    } ) );
                }
                Find.WindowStack.Add( new FloatMenu( periodOptions ) );
            }
        }
    }
}