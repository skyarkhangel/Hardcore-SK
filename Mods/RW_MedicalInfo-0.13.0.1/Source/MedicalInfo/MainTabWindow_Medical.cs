using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Fluffy
{
    public class MainTabWindow_Medical : MainTabWindow_PawnList
    {
        public enum Order
        {
            Name,
            Care,
            BleedRate,
            Operations,
            Efficiency
        }

        public enum SourceOptions
        {
            Colonists,
            Prisoners
        }

        protected const float LabelRowHeight = 50f;

        public static bool IsDirty;

        public bool Asc = true;

        public List<PawnCapacityDef> CapDefs =
            DefDatabase<PawnCapacityDef>.AllDefsListForReading.Where( x => x.showOnHumanlikes ).ToList( );

        public Order OrderBy = Order.Name;

        public PawnCapacityDef OrderByCapDef = PawnCapacityDefOf.Consciousness;

        public SourceOptions Source = SourceOptions.Colonists;

        public override Vector2 RequestedTabSize
        {
            get { return new Vector2( 1050f, 90f + PawnsCount * 30f + 65f ); }
        }

        public override void PreOpen( )
        {
            base.PreOpen( );
            MainTabWindow_Work.Reinit( );
        }

        protected override void BuildPawnList( )
        {
            pawns.Clear( );

            IEnumerable<Pawn> tempPawns;

            switch ( Source )
            {
                case SourceOptions.Prisoners:
                    tempPawns = Find.MapPawns.PrisonersOfColony;
                    break;
                default:
                    tempPawns = Find.MapPawns.FreeColonists;
                    break;
            }

            switch ( OrderBy )
            {
                case Order.Care:
                    pawns = ( from p in tempPawns
                              orderby p.playerSettings.medCare ascending
                              select p ).ToList( );
                    break;
                case Order.BleedRate:
                    pawns = ( from p in tempPawns
                              orderby p.health.hediffSet.BleedingRate
                              select p ).ToList( );
                    break;
                case Order.Operations:
                    pawns = ( from p in tempPawns
                              orderby p.BillStack.Count
                              select p ).ToList( );
                    break;
                case Order.Efficiency:
                    pawns = ( from p in tempPawns
                              orderby p.health.capacities.GetEfficiency( OrderByCapDef ) descending
                              select p ).ToList( );
                    break;
                default:
                    pawns = ( from p in tempPawns
                              orderby p.LabelCap ascending
                              select p ).ToList( );
                    break;
            }

            if ( !Asc )
            {
                pawns.Reverse( );
            }

            IsDirty = false;
        }

        public override void DoWindowContents( Rect rect )
        {
            base.DoWindowContents( rect );

            if ( IsDirty )
            {
                BuildPawnList( );
            }
            var position = new Rect( 0f, 0f, rect.width, 80f );
            GUI.BeginGroup( position );

            var x = 0f;
            Text.Font = GameFont.Small;

            // prisoner / colonist toggle
            var sourceButton = new Rect( 0f, 0f, 200f, 35f );
            if ( Widgets.TextButton( sourceButton, Source.ToString( ) ) )
            {
                Source = Source == SourceOptions.Colonists ? SourceOptions.Prisoners : SourceOptions.Colonists;
                IsDirty = true;
            }

            // name
            var nameLabel = new Rect( x, 50f, 175f, 30f );
            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label( nameLabel, "FluffyMedical.Name".Translate( ) );
            if ( Widgets.InvisibleButton( nameLabel ) )
            {
                if ( OrderBy == Order.Name )
                {
                    Asc = !Asc;
                }
                else
                {
                    OrderBy = Order.Name;
                    Asc = true;
                }
                IsDirty = true;
            }
            TooltipHandler.TipRegion( nameLabel,
                                      "FluffyMedical.ClickToSortBy".Translate( "FluffyMedical.Name".Translate( ) ) );
            Widgets.DrawHighlightIfMouseover( nameLabel );
            x += 175f;

            // care
            var careLabel = new Rect( x, 50f, 100f, 30f );
            Widgets.Label( careLabel, "FluffyMedical.Care".Translate( ) );
            if ( Widgets.InvisibleButton( careLabel ) )
            {
                if ( Event.current.shift )
                {
                    Utility_Medical.MedicalCareSetterAll( pawns );
                }
                else
                {
                    if ( OrderBy == Order.Care )
                    {
                        Asc = !Asc;
                    }
                    else
                    {
                        OrderBy = Order.Care;
                        Asc = true;
                    }
                    IsDirty = true;
                }
            }
            TooltipHandler.TipRegion( careLabel,
                                      "FluffyMedical.ClickToSortBy".Translate( "FluffyMedical.Care".Translate( ) ) +
                                      "\n" +
                                      "FluffyMedical.ShiftClickTo".Translate( "FluffyMedical.SetCare".Translate( ) ) );
            Widgets.DrawHighlightIfMouseover( careLabel );
            x += 100f;

            // bloodloss
            var bloodLabel = new Rect( x, 50f, 50f, 30f );
            var bloodIcon = new Rect( x + 17f, 60f, 16f, 16f );
            GUI.DrawTexture( bloodIcon, Utility_Medical.BloodTextureWhite );
            if ( Widgets.InvisibleButton( bloodLabel ) )
            {
                if ( OrderBy == Order.BleedRate )
                {
                    Asc = !Asc;
                }
                else
                {
                    OrderBy = Order.BleedRate;
                    Asc = true;
                }
                IsDirty = true;
            }
            TooltipHandler.TipRegion( bloodLabel, "FluffyMedical.ClickToSortBy".Translate( "BleedingRate".Translate( ) ) );
            Widgets.DrawHighlightIfMouseover( bloodLabel );
            x += 50f;

            // Operations
            var opLabel = new Rect( x, 50f, 50f, 30f );
            var opIcon = new Rect( x + 17f, 60f, 16f, 16f );
            GUI.DrawTexture( opIcon, Utility_Medical.OpTexture );
            if ( Widgets.InvisibleButton( opLabel ) )
            {
                if ( OrderBy == Order.Operations )
                {
                    Asc = !Asc;
                }
                else
                {
                    OrderBy = Order.Operations;
                    Asc = true;
                }
                IsDirty = true;
            }
            TooltipHandler.TipRegion( opLabel,
                                      "FluffyMedical.ClickToSortBy".Translate(
                                          "FluffyMedical.CurrentOperations".Translate( ) ) );
            Widgets.DrawHighlightIfMouseover( opLabel );
            x += 50f;

            var offset = true;

            // extra 15f offset for... what? makes labels roughly align.
            var colWidth = ( rect.width - x - 15f ) / CapDefs.Count;
            for ( var i = 0; i < CapDefs.Count; i++ )
            {
                var defLabel = new Rect( x + colWidth * i - colWidth / 2, 10f + ( offset ? 10f : 40f ), colWidth * 2,
                                         30f );
                Widgets.DrawLine( new Vector2( x + colWidth * ( i + 1 ) - colWidth / 2, 40f + ( offset ? 5f : 35f ) ),
                                  new Vector2( x + colWidth * ( i + 1 ) - colWidth / 2, 80f ), Color.gray, 1 );
                Widgets.Label( defLabel, CapDefs[i].LabelCap );
                if ( Widgets.InvisibleButton( defLabel ) )
                {
                    if ( OrderBy == Order.Efficiency && OrderByCapDef == CapDefs[i] )
                    {
                        Asc = !Asc;
                    }
                    else
                    {
                        OrderBy = Order.Efficiency;
                        OrderByCapDef = CapDefs[i];
                        Asc = true;
                    }
                    IsDirty = true;
                }
                TooltipHandler.TipRegion( defLabel, "FluffyMedical.ClickToSortBy".Translate( CapDefs[i].LabelCap ) );
                Widgets.DrawHighlightIfMouseover( defLabel );

                offset = !offset;
            }

            GUI.EndGroup( );

            var content = new Rect( 0f, position.yMax, rect.width, rect.height - position.yMax );
            GUI.BeginGroup( content );
            DrawRows( new Rect( 0f, 0f, content.width, content.height ) );
            GUI.EndGroup( );
        }

        /// <summary>
        /// creates a new square of size in the center of rect.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Rect Inner( Rect rect, float size )
        {
            return new Rect( rect.xMin + ( rect.width - size ) / 2, rect.yMin + ( rect.height - size ) / 2, size, size );
        }

        protected override void DrawPawnRow( Rect rect, Pawn p )
        {
            // name is handled in PreDrawRow, start at 175
            var x = 175f;
            var y = rect.yMin;

            // care
            var careRect = new Rect( x, y, 100f, 30f );
            Utility_Medical.MedicalCareSetter( careRect, ref p.playerSettings.medCare );
            x += 100f;

            // blood
            var bloodRect = new Rect( x, y, 50f, 30f );
            var bleedRate = p.health.hediffSet.BleedingRate; // float in range 0 - 1
            float iconSize;
            if ( bleedRate < 0.01f )
            {
                iconSize = 0f;
            }
            else if ( bleedRate < .1f )
            {
                iconSize = 8f;
            }
            else if ( bleedRate < .3f )
            {
                iconSize = 16f;
            }
            else
            {
                iconSize = 24f;
            }
            var iconRect = Inner( bloodRect, iconSize );
            GUI.DrawTexture( iconRect, Utility_Medical.BloodTexture );
            TooltipHandler.TipRegion( bloodRect,
                                      "BleedingRate".Translate( ) + ": " + bleedRate.ToStringPercent( ) + "/" +
                                      "LetterDay".Translate( ) );
            Widgets.DrawHighlightIfMouseover( bloodRect );
            x += 50f;

            // Operations
            var opLabel = new Rect( x, y, 50f, 30f );
            if ( Widgets.InvisibleButton( opLabel ) )
            {
                if ( Event.current.button == 0 )
                {
                    Utility_Medical.RecipeOptionsMaker( p );
                }
                else if ( Event.current.button == 1 )
                {
                    p.BillStack.Clear( );
                }
            }
            var opLabelString = new StringBuilder( );
            opLabelString.AppendLine( "FluffyMedical.ClickTo".Translate( "FluffyMedical.ScheduleOperation".Translate( ) ) );
            opLabelString.AppendLine(
                "FluffyMedical.RightClickTo".Translate( "FluffyMedical.UnScheduleOperations".Translate( ) ) );
            opLabelString.AppendLine( );
            opLabelString.AppendLine( "FluffyMedical.ScheduledOperations".Translate( ) );

            var opScheduled = false;
            foreach ( var op in p.BillStack )
            {
                opLabelString.AppendLine( op.LabelCap );
                opScheduled = true;
            }

            if ( opScheduled )
            {
                GUI.DrawTexture( Inner( opLabel, 16f ), Widgets.CheckboxOnTex );
            }
            else
            {
                opLabelString.AppendLine( "FluffyMedical.NumCurrentOperations".Translate( "No" ) );
            }

            TooltipHandler.TipRegion( opLabel, opLabelString.ToString( ) );
            Widgets.DrawHighlightIfMouseover( opLabel );
            x += 50f;

            // main window
            Text.Anchor = TextAnchor.MiddleCenter;
            var colWidth = ( rect.width - x ) / CapDefs.Count;
            foreach ( PawnCapacityDef t in CapDefs ) {
                var capDefCell = new Rect( x, y, colWidth, 30f );
                var colorPair = HealthCardUtility.GetEfficiencyLabel( p, t );
                var label = ( p.health.capacities.GetEfficiency( t ) * 100f ).ToString( "F0" ) + "%";
                GUI.color = colorPair.Second;
                Widgets.Label( capDefCell, label );
                if ( Mouse.IsOver( capDefCell ) )
                {
                    GUI.DrawTexture( capDefCell, TexUI.HighlightTex );
                }
                Utility_Medical.DoHediffTooltip( capDefCell, p, t );
                x += colWidth;
            }
        }
    }
}