// Manager/History.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-04 19:32

using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public class History : IExposable
    {
        // types
        public enum Period
        {
            Day,
            Month,
            Year
        }

        public static Color DefaultLineColor              = Color.white;
        private const int Breaks                          = 4;
        private const int DashLength                      = 3;
        private const float Margin                        = Utilities.Margin;
        private const int Size                            = 100;

        // settings
        public bool AllowTogglingLegend                   = true;
        public bool DrawInlineLegend                      = true;
        public bool DrawTargetLine                        = true;
        public bool DrawOptions                           = true;
        public string Suffix                              = String.Empty;

        // for detailed legend
        public bool DrawIcons                             = true;
        public bool DrawCounts                            = true;
        public bool DrawInfoInBar                         = false;
        public bool DrawMaxMarkers                        = false;
        public bool MaxPerChapter                         = false;
        
        // interval per period
        private static Dictionary<Period, int> _intervals = new Dictionary<Period, int>();
        private static readonly Texture2D _plotBG         = Resources.SlightlyDarkBackground;
        private static readonly float _yAxisMargin        = 40f;
        public  static Period[] periods                   = (Period[])Enum.GetValues( typeof (Period) );

        // each chapter holds the history for all periods.
        private List<Chapter> _chapters                   = new List<Chapter>();
        private List<Chapter> _chaptersShown              = new List<Chapter>();
        public Period periodShown                         = Period.Day;

        // for scribe.
        public History() {}
        
        public History( string[] labels, Color[] colors = null )
        {
#if DEBUG
            Log.Message( "History created" + string.Join( ", ", labels ) );
#endif
            // get range of colors if not set
            if( colors == null )
            {
                // default to white for single line
                if( labels.Length == 1 )
                {
                    colors = new[] { Color.white };
                }

                // rainbow!
                else
                {
                    colors = HSV_Helper.Range( labels.Length );
                }
            }

            // create a chapter for each label
            for( int i = 0; i < labels.Length; i++ )
            {
                _chapters.Add( new Chapter( labels[i], Size, colors[i % colors.Length] ) );
            }

            // show all by default
            _chaptersShown.AddRange( _chapters );
        }
        
        public History( ThingCount[] thingCounts, Color[] colors = null )
        {
            // get range of colors if not set
            if( colors == null )
            {
                // default to white for single line
                if( thingCounts.Length == 1 )
                {
                    colors = new[] { Color.white };
                }

                // rainbow!
                else
                {
                    colors = HSV_Helper.Range( thingCounts.Length );
                }
            }

            // create a chapter for each label
            for( int i = 0; i < thingCounts.Length; i++ )
            {
                _chapters.Add( new Chapter( thingCounts[i], Size, colors[i % colors.Length] ) );
            }

            // show all by default
            _chaptersShown.AddRange( _chapters );
        }

        public void ExposeData()
        {
            // settings
            Scribe_Values.LookValue( ref AllowTogglingLegend, "AllowToggingLegend", true );
            Scribe_Values.LookValue( ref DrawInlineLegend, "ShowLegend", true );
            Scribe_Values.LookValue( ref DrawTargetLine, "DrawTargetLine", true );
            Scribe_Values.LookValue( ref DrawOptions, "DrawOptions", true );
            Scribe_Values.LookValue( ref Suffix, "Suffix", "" );
            Scribe_Values.LookValue( ref DrawIcons, "DrawIcons", true );
            Scribe_Values.LookValue( ref DrawCounts, "DrawCounts", true );
            Scribe_Values.LookValue( ref DrawInfoInBar, "DrawInfoInBar", false );
            Scribe_Values.LookValue( ref DrawMaxMarkers, "DrawMaxMarkers", true );
            Scribe_Values.LookValue( ref MaxPerChapter, "MaxPerChapter", false );

            // history chapters
            Scribe_Collections.LookList( ref _chapters, "Chapters", LookMode.Deep );

            // some post load tweaks
            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                // set chapters shown to the newly loaded chapters (instead of the default created empty chapters).
                _chaptersShown.Clear();
                _chaptersShown.AddRange(_chapters);
            }
        }

        public static int Interval( Period period )
        {
            if ( !_intervals.ContainsKey( period ) )
            {
                int ticks;
                switch ( period )
                {
                    case Period.Month:
                        ticks = GenDate.TicksPerMonth;
                        break;
                    case Period.Year:
                        ticks = GenDate.TicksPerYear;
                        break;
                    default:
                        ticks = GenDate.TicksPerDay;
                        break;
                }
                _intervals[period] = ticks / Size;
            }
            return _intervals[period];
        }

        /// <summary>
        /// Round up to given precision
        /// </summary>
        /// <param name="x">input</param>
        /// <param name="precision">number of digits to preserve past the magnitude, should be equal to or greater than zero.</param>
        /// <returns></returns>
        public int CeilToPrecision( float x, int precision = 1)
        {
            int magnitude = Mathf.FloorToInt( Mathf.Log10( x ) );
            int unit = Mathf.FloorToInt( Mathf.Pow( 10, magnitude - precision ) );
            return Mathf.CeilToInt(x / unit) * unit; 
        }

        public string FormatCount( float x, int unit = 1000, string[] suffixes = null )
        {
            if ( suffixes == null ) suffixes = new[] { "", "k", "M", "G" };
            int i = 0;
            while ( x > unit / 10 && i < suffixes.Length )
            {
                x /= unit;
                i++;
            }

            return x.ToString( "0.#" + suffixes[i] + Suffix );
        }

        public void Update( params int[] counts )
        {
            if ( counts.Length != _chapters.Count )
            {
                Log.Warning( "History updated with incorrect number of chapters" );
            }

            for ( int i = 0; i < counts.Length; i++ )
            {
                _chapters[i].Add( counts[i] );
            }
        }

        public void UpdateThingCounts( params int[] counts )
        {
            if( counts.Length != _chapters.Count )
            {
                Log.Warning( "History updated with incorrect number of chapters" );
            }

            for( int i = 0; i < counts.Length; i++ )
            {
                _chapters[i].ThingCount.count = counts[i];
            }
        }

        public void UpdateMax( params int[] maxes )
        {
            if( maxes.Length != _chapters.Count )
            {
                Log.Warning( "History updated with incorrect number of chapters" );
            }

            for( int i = 0; i < maxes.Length; i++ )
            {
                _chapters[i].TrueMax = maxes[i];
            }
        }

        public void UpdateThingCountAndMax( int[] counts, int[] maxes)
        {
            if( maxes.Length != _chapters.Count || maxes.Length != _chapters.Count )
            {
                Log.Warning( "History updated with incorrect number of chapters" );
            }

            for( int i = 0; i < maxes.Length; i++ )
            {
                if ( _chapters[i].ThingCount.count != counts[i] )
                {
                    _chapters[i].TrueMax = maxes[i];
                    _chapters[i].ThingCount.count = counts[i];
                }
            }
        }

        public void DrawPlot( Rect rect, int target = 0, string label = "", bool positiveOnly = false, bool negativeOnly = false )
        {
            // set sign
            int sign = negativeOnly ? -1 : 1;

            // subset chapters
            List<Chapter> chapters = _chaptersShown.Where( chapter => !positiveOnly || chapter.pages[periodShown].Any( i => i > 0 ) )
                               .Where( chapter => !negativeOnly || chapter.pages[periodShown].Any( i => i < 0 ) )
                               .ToList();

            // get out early if no chapters.
            if ( chapters.Count == 0 )
            {
                GUI.DrawTexture( rect.ContractedBy(Utilities.Margin), _plotBG );
                Utilities.Label( rect, "FM.HistoryNoChapters".Translate(), null, TextAnchor.MiddleCenter, color: Color.grey );
                return;
            }


            // stuff we need
            Rect plot = rect.ContractedBy( Utilities.Margin );
            plot.xMin += _yAxisMargin;

            // maximum of all chapters.
            int max = CeilToPrecision( Math.Max( chapters.Select( c => c.Max( periodShown, !negativeOnly ) ).Max(), target ) * 1.2f );

            // size, and pixels per node.
            float w = plot.width;
            float h = plot.height;
            float wu = w / Size; // width per section
            float hu = h / max; // height per count
            int bi = max / ( Breaks + 1 ); // count per break
            float bu = hu * bi; // height per break

            // plot the line(s)
            GUI.DrawTexture( plot, _plotBG );
            GUI.BeginGroup( plot );
            foreach ( Chapter chapter in chapters )
            {
                chapter.Plot( periodShown, plot.AtZero(), wu, hu, sign );
            }

            // handle mouseover events
            if ( Mouse.IsOver( plot.AtZero() ) )
            {
                // very conveniently this is the position within the current group.
                Vector2 pos = Event.current.mousePosition;
                Vector2 upos = new Vector2(pos.x / wu, (plot.height - pos.y) / hu);

                // get distances
                float[] distances = chapters.Select( c => Math.Abs( c.ValueAt( periodShown, (int)upos.x, sign ) - upos.y ) ).ToArray();
                
                // get the minimum index
                float min = int.MaxValue;
                int minIndex = 0;
                for ( int i = 0; i < distances.Count(); i++ )
                {
                    if ( distances[i] < min )
                    {
                        minIndex = i;
                        min = distances[i];
                    }
                }

                // closest line
                Chapter closest = chapters[minIndex];

                // do minimum stuff.
                Vector2 realpos = new Vector2( pos.x, plot.height - closest.ValueAt( periodShown, (int)upos.x, sign ) * hu );
                Rect blipRect = new Rect(realpos.x - Utilities.SmallIconSize / 2f, realpos.y - Utilities.SmallIconSize / 2f, Utilities.SmallIconSize, Utilities.SmallIconSize );
                GUI.color = closest.lineColor;
                GUI.DrawTexture( blipRect, Resources.StageB );
                GUI.color = DefaultLineColor;

                // get orientation of tooltip
                Vector2 tippos = realpos + new Vector2( Utilities.Margin, Utilities.Margin );
                string tip = chapters[minIndex].label + ": " + FormatCount( chapters[minIndex].ValueAt( periodShown, (int)upos.x, sign ));
                Vector2 tipsize = Text.CalcSize( tip );
                bool up = false, left = false;
                if ( tippos.x + tipsize.x > plot.width )
                {
                    left = true;
                    tippos.x -= tipsize.x + 2 * + Utilities.Margin;
                }
                if ( tippos.y + tipsize.y > plot.height )
                {
                    up = true;
                    tippos.y -= tipsize.y + 2 * Utilities.Margin;
                }

                TextAnchor anchor = TextAnchor.UpperLeft;
                if (up && left) anchor = TextAnchor.LowerRight;
                if ( up && !left ) anchor = TextAnchor.LowerLeft;
                if ( !up && left ) anchor = TextAnchor.UpperRight;
                Rect tooltipRect = new Rect( tippos.x, tippos.y, tipsize.x, tipsize.y );
                Utilities.Label( tooltipRect, tip, anchor: anchor, font: GameFont.Tiny );
            }

            // draw target line
            if ( DrawTargetLine )
            {
                GUI.color = Color.gray;
                for ( int i = 0; i < plot.width / DashLength; i += 2 )
                {
                    Widgets.DrawLineHorizontal( i * DashLength, plot.height - target * hu, DashLength );
                }
            }

            // draw legend
            int lineCount = _chapters.Count;
            if ( AllowTogglingLegend && lineCount > 1 && DrawInlineLegend )
            {
                float rowHeight = 20f;
                float lineLength = 30f;
                float labelWidth = 100f;

                Vector2 cur = Vector2.zero;
                foreach ( Chapter chapter in _chapters )
                {
                    GUI.color = chapter.lineColor;
                    Widgets.DrawLineHorizontal(cur.x, cur.y + rowHeight / 2f, lineLength);
                    cur.x += lineLength;
                    Utilities.Label( ref cur, labelWidth, rowHeight, chapter.label, font: GameFont.Tiny );
                    cur.x = 0f;
                }
                GUI.color = Color.white;
            }

            GUI.EndGroup();

            // plot axis
            GUI.BeginGroup( rect );
            Text.Anchor = TextAnchor.MiddleRight;
            Text.Font = GameFont.Tiny;

            // draw ticks + labels
            for ( int i = 1; i < Breaks + 1; i++ )
            {
                Widgets.DrawLineHorizontal( _yAxisMargin + Margin / 2, plot.height - i * bu, Margin );
                Rect labRect = new Rect( 0f, plot.height - i * bu - 4f, _yAxisMargin, 20f );
                Widgets.Label( labRect, FormatCount(  i * bi ) );
            }

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            rect = rect.AtZero(); // ugh, I'm tired, just work.

            // period / variables picker
            if ( DrawOptions )
            {
                Rect switchRect = new Rect( rect.xMax - Utilities.SmallIconSize - Utilities.Margin,
                                            rect.yMin + Utilities.Margin, Utilities.SmallIconSize,
                                            Utilities.SmallIconSize );

                Widgets.DrawHighlightIfMouseover( switchRect );
                if ( Widgets.ImageButton( switchRect, Resources.Cog ) )
                {
                    List<FloatMenuOption> options =
                        periods.Select(
                            p =>
                                new FloatMenuOption( "FM.HistoryPeriod".Translate() + ": " + p.ToString(),
                                                     delegate { periodShown = p; } ) ).ToList();
                    if ( AllowTogglingLegend && _chapters.Count > 1 ) // add option to show/hide legend if appropriate.
                    {
                        options.Add( new FloatMenuOption( "FM.HistoryShowHideLegend".Translate(),
                                                          delegate { DrawInlineLegend = !DrawInlineLegend; } ) );
                    }
                    Find.WindowStack.Add( new FloatMenu( options ) );
                }
            }

            GUI.EndGroup();
        }

        public void DrawDetailedLegend( Rect canvas, ref Vector2 scrollPos, int? max, bool positiveOnly = false, bool negativeOnly = false )
        {
            // set sign
            int sign = negativeOnly ? -1 : 1;

            List<Chapter> ChaptersOrdered = _chapters
                .Where( chapter => !positiveOnly || chapter.pages[periodShown].Any( i => i > 0 ) )
                .Where( chapter => !negativeOnly || chapter.pages[periodShown].Any( i => i < 0 ) )
                .OrderByDescending( chapter => chapter.Last( periodShown ) * sign ).ToList();

            // get out early if no chapters.
            if( ChaptersOrdered.Count == 0 )
            {
                GUI.DrawTexture( canvas.ContractedBy( Utilities.Margin ), _plotBG );
                Utilities.Label( canvas, "FM.HistoryNoChapters".Translate(), null, TextAnchor.MiddleCenter, color: Color.grey );
                return;
            }

            // max 
            float _max = max ?? ( DrawMaxMarkers
                                 ? ChaptersOrdered.Max( chapter => (int)chapter.TrueMax ) 
                                 : ChaptersOrdered.FirstOrDefault()?.Last( periodShown ) * sign )
                             ?? 0;
            
            // cell height
            float height = 30f;
            float barHeight = 18f;

            // n rows
            int n = ChaptersOrdered.Count;

            // scrolling region
            Rect viewRect = canvas;
            viewRect.height = n * height;
            if ( viewRect.height > canvas.height )
            {
                viewRect.width -= 16f + Utilities.Margin;
                canvas.width -= Utilities.Margin;
                canvas.height -= 1f;
            }
            Widgets.BeginScrollView( canvas, ref scrollPos, viewRect);
            for ( int i = 0; i < n; i++ )
            {
                // set up rects
                Rect row      = new Rect( 0f, height * i, viewRect.width, height );
                Rect icon     = new Rect( Utilities.Margin, height * i, height, height ).ContractedBy( Utilities.Margin / 2f ); // icon is square, size defined by height.
                Rect bar      = new Rect( Utilities.Margin + height, height * i, viewRect.width - height - Utilities.Margin, height );

                // if icons should not be drawn make the bar full size.
                if (!DrawIcons) bar.xMin -= height + Utilities.Margin;

                // bar details.
                Rect barBox   = bar.ContractedBy( ( height - barHeight ) / 2f );
                Rect barFill  = barBox.ContractedBy( 2f );
                float maxWidth = barFill.width;
                if ( MaxPerChapter )
                {
                    barFill.width *= ChaptersOrdered[i].Last( periodShown ) * sign / (float)ChaptersOrdered[i].TrueMax;
                }
                else
                {
                    barFill.width *= ChaptersOrdered[i].Last( periodShown ) * sign / _max;
                }
                
                GUI.BeginGroup( viewRect );

                // if DrawIcons and a thing is set, draw the icon.
                ThingDef thing = ChaptersOrdered[i].ThingCount?.thingDef;
                if( DrawIcons && thing != null )
                {
                    // draw the icon in correct proportions
                    float proportion = GenUI.IconDrawScale( thing );
                    Widgets.DrawTextureFitted( icon, thing.uiIcon, proportion );

                    // draw counts in upper left corner
                    if ( DrawCounts )
                    {
                        Utilities.LabelOutline( icon, ChaptersOrdered[i].ThingCount.count.ToString(), null,
                                         TextAnchor.UpperLeft, 0f, 0f, GameFont.Tiny, Color.white, Color.black );
                    }
                }

                // if desired, draw ghost bar
                if( DrawMaxMarkers )
                {
                    Rect ghostBarFill = barFill;
                    ghostBarFill.width = MaxPerChapter ? maxWidth : maxWidth * ( ChaptersOrdered[i].TrueMax / _max );
                    GUI.color = new Color( 1f, 1f, 1f, .2f );
                    GUI.DrawTexture( ghostBarFill, ChaptersOrdered[i].Texture ); // coloured texture
                    GUI.color = Color.white;
                }

                // draw the main bar.
                GUI.DrawTexture( barBox, Resources.SlightlyDarkBackground );
                GUI.DrawTexture( barFill, ChaptersOrdered[i].Texture ); // coloured texture
                GUI.DrawTexture( barFill, Resources.BarShader ); // slightly fancy overlay (emboss).
                
                // draw on bar info
                if ( DrawInfoInBar )
                {
                    string info = ChaptersOrdered[i].label + ": " +
                                  FormatCount( ChaptersOrdered[i].Last( periodShown ) * sign );

                    if ( DrawMaxMarkers )
                    {
                        info += " / " + FormatCount( ChaptersOrdered[i].TrueMax );
                    }

                    // offset label a bit downwards and to the right
                    Rect rowInfoRect = row;
                    rowInfoRect.y += 3f;
                    rowInfoRect.x += Utilities.Margin * 2;

                    // x offset
                    float xOffset = DrawIcons && thing != null ? height + Utilities.Margin * 2 : Utilities.Margin * 2;

                    Utilities.LabelOutline( rowInfoRect, info, null, TextAnchor.MiddleLeft, xOffset, 0f, GameFont.Tiny, Color.white, Color.black );
                }

                // are we currently showing this line?
                bool shown = _chaptersShown.Contains( ChaptersOrdered[i] );

                // tooltip on entire row
                string tooltip = ChaptersOrdered[i].label + ": " + FormatCount( Mathf.Abs( ChaptersOrdered[i].Last( periodShown ) ) );
                tooltip += "FM.HistoryClickToEnable".Translate( shown ? "hide" : "show", ChaptersOrdered[i].label );
                TooltipHandler.TipRegion( row, tooltip);

                // handle input
                if ( Widgets.InvisibleButton( row ) )
                {
                    if ( Event.current.button == 0 )
                    {
                        if ( shown )
                        {
                            _chaptersShown.Remove( ChaptersOrdered[i] );
                        }
                        else
                        {
                            _chaptersShown.Add( ChaptersOrdered[i] );
                        }
                    } else if ( Event.current.button == 1 )
                    {
                        _chaptersShown.Clear();
                        _chaptersShown.Add( ChaptersOrdered[i] );
                    }
                }

                // UI feedback for disabled row
                if ( !shown )
                {
                    GUI.DrawTexture( row, Resources.SlightlyDarkBackground );
                }

                GUI.EndGroup();
            }
            Widgets.EndScrollView();
        }
        
        public class Chapter : IExposable
        {
            public Dictionary<Period, List<int>> pages                     = new Dictionary<Period, List<int>>();
            public string                                 label            = String.Empty;
            public Color                                  lineColor        = DefaultLineColor;
            public Texture2D                              _texture;
            public int                                    size             = Size;
            public ThingCount                             ThingCount       = new ThingCount();
            private int                                   _observedMax     = -1;
            private int                                   _specificMax     = -1;

            public Chapter()
            {
                // empty for scribe.
                // create a dictionary of histories, one for each period, initialize with a zero to avoid errors.
                pages = periods.ToDictionary( k => k, v => new List<int>( new [] { 0 } ) );
            }

            public Chapter( string label, int size, Color color ) : this()
            {
                this.label = label;
                this.size = size;
                lineColor = color;
            }

            public Chapter( ThingCount thingCount, int size, Color color ) : this()
            {
                this.label = thingCount.thingDef.LabelCap;
                this.ThingCount = thingCount;
                this.size = size;
                lineColor = color;
            }

            public Texture2D Texture
            {
                get
                {
                    if ( _texture == null )
                    {
                        _texture = SolidColorMaterials.NewSolidColorTexture( lineColor );
                    }
                    return _texture;
                }
            }

            public bool Active( Period period )
            {
                return pages[period].Any( v => v > 0 );
            }

            public int Last( Period period )
            {
                return pages[period].Last();
            }

            public int ValueAt( Period period, int x , int sign = 1)
            {
                if ( x < 0 || x >= pages[period].Count ) return -1;
                return pages[period][x] * sign;
            }

            public void ExposeData()
            {
                Scribe_Values.LookValue( ref label, "label" );
                Scribe_Values.LookValue( ref size, "size", 100 );
                Scribe_Values.LookValue( ref lineColor, "color", Color.white );
                Scribe_Values.LookValue( ref ThingCount.count, "thingCount_count" );
                Scribe_Defs.LookDef( ref ThingCount.thingDef, "thingCount_def" );

                List<Period> periods = new List<Period>( pages.Keys );
                foreach ( Period period in periods )
                {
                    List<int> values = pages[period];
                    Utilities.Scribe_IntArray( ref values, period.ToString() );

#if DEBUG_SCRIBE
                    Log.Message( Scribe.mode + " for " + label + ", daycount: " + pages[Period.Day].Count );
#endif

                    pages[period] = values;
                }
            }

            public int TrueMax
            {
                get { return Mathf.Max( _observedMax, _specificMax, 1 ); }
                set
                {
                    _observedMax = value != 0 ? value : Max( Period.Day );
                    _specificMax = value;
                }
            }

            public void Add( int count )
            {
                int curTick = Find.TickManager.TicksGame;
                foreach ( Period period in periods )
                {
                    if ( curTick % Interval( period ) == 0 )
                    {
                        pages[period].Add( count );
                        if ( Mathf.Abs( count ) > _observedMax ) _observedMax = Mathf.Abs( count );

                        // cull the list back down to size.
                        while ( pages[period].Count > Size )
                        {
                            pages[period].RemoveAt( 0 );
                        }
                    }
                }
            }

            public int Max( Period period, bool positive = true )
            {
                return positive ? pages[period].Max() : Math.Abs( pages[period].Min() );
            }

            public void Plot( Period period, Rect canvas, float wu, float hu, int sign = 1 )
            {
                if( pages[period].Count > 1 )
                {
                    List<int> hist = pages[period];
                    for( int i = 0; i < hist.Count - 1; i++ ) // line segments, so up till n-1
                    {
                        Vector2 start = new Vector2( wu * i, canvas.height - hu * hist[i] * sign );
                        Vector2 end = new Vector2( wu * ( i + 1 ), canvas.height - hu * hist[i + 1] * sign );
                        Widgets.DrawLine( start, end, lineColor, 1f );
                    }
                }
            }
        }
    }
}