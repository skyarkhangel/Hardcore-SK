using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Relations
{
    public enum Page
    {
        Colonists,
        Factions
    }

    public class MainTabWindow_Relations : MainTabWindow_PawnList
    {
        #region Fields

        public static Dictionary<Pawn, Rect> Slots = new Dictionary<Pawn, Rect>();
        private static Page _currentPage = Page.Colonists;
        private static Faction _selectedFaction;
        private static Pawn _selectedPawn;
        private static List<Faction> factions;
        private float _factionDetailHeight = 999f;
        private Vector2 _factionDetailScrollPosition = Vector2.zero;
        private float _factionInformationHeight = 999f;
        private Vector2 _factionInformationScrollPosition = Vector2.zero;
        private Pawn _lastSelectedPawn;
        private Rect detailRect;
        private Rect networkRect;
        private Rect sourceButtonRect;

        #endregion Fields

        #region Constructors

        public MainTabWindow_Relations()
        {
            forcePause = true;
        }

        #endregion Constructors

        #region Properties

        public Page CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                _currentPage = value;
                CreateSlots();
            }
        }

        public override Vector2 InitialWindowSize
        {
            get
            {
                return new Vector2( Screen.width, Screen.height - 35f );
            }
        }

        public Faction SelectedFaction
        {
            get
            {
                return _selectedFaction;
            }
            set
            {
                _selectedFaction = value;
            }
        }

        public Pawn SelectedPawn
        {
            get
            {
                return _selectedPawn;
            }
            set
            {
                _selectedPawn = value;
            }
        }

        #endregion Properties

        #region Methods

        public void CreateSlots()
        {
            // get circle radius and center
            float radius = ( networkRect.width - Settings.SlotSize ) /2f;
            Vector2 centre = networkRect.center;

            // calculate positions
            Slots.Clear();
            if ( CurrentPage == Page.Colonists )
            {
                for ( int i = 0; i < PawnsCount; i++ )
                {
                    Slots.Add( pawns[i], GetRectOnCircle( i, PawnsCount, radius, centre ) );
                }
            }
            else
            {
                factions = new List<Faction>( Find.FactionManager.AllFactionsInViewOrder );
                for ( int i = 0; i < factions.Count; i++ )
                {
                    Pawn leader = GetFactionLeader( factions[i] );
                    if ( leader != null )
                        Slots.Add( leader, GetRectOnCircle( i, factions.Count, radius, centre ) );
                    else
                        Log.Message( factions[i].GetCallLabel() + " leader null" );
                }
            }

            Log.Message( "Created slots for " + CurrentPage + ", " + Slots.Count + " created." );
        }

        public override void DoWindowContents( Rect canvas )
        {
            // set size and draw background
            base.DoWindowContents( canvas );

            // draw source selection rect
            if ( CurrentPage == Page.Colonists )
            {
                if ( Widgets.TextButton( sourceButtonRect, "Fluffy_Relations.Colonists".Translate() ) )
                {
                    CurrentPage = Page.Factions;
                }
            }
            if ( CurrentPage == Page.Factions )
            {
                if ( Widgets.TextButton( sourceButtonRect, "Fluffy_Relations.Factions".Translate() ) )
                {
                    CurrentPage = Page.Colonists;
                }
            }
            TooltipHandler.TipRegion( sourceButtonRect, "Fluffy_Relations.SourceButtonTip".Translate() );

            // draw relevant page
            if ( CurrentPage == Page.Colonists )
                DrawPawnRelations( canvas );
            if ( CurrentPage == Page.Factions )
                DrawFactionRelations( canvas );
        }

        public void DrawDetails( Rect canvas, Pawn pawn )
        {
            GUI.BeginGroup( canvas );

            // set up rects
            Rect relationsTitleRect = new Rect( 0f, 0f, canvas.width, 30f );
            Rect relationsRect = new Rect( 0f, 36f, canvas.width, canvas.height / 2f - 36f );
            Rect interactionsTitleRect = new Rect( 0f, canvas.height / 2f, canvas.width, 30f );
            Rect interactionsRect = new Rect( 0f, canvas.height / 2f + 36f, canvas.width, canvas.height / 2f - 36f );

            // titles
            Text.Font = GameFont.Medium;
            Widgets.Label( relationsTitleRect, "Fluffy_Relations.Possesive".Translate( pawn.LabelBaseShort ) + "Fluffy_Relations.Relations".Translate() );
            Widgets.Label( interactionsTitleRect, "Fluffy_Relations.Possesive".Translate( pawn.LabelBaseShort ) + "Fluffy_Relations.Interactions".Translate() );
            Text.Font = GameFont.Small;

            // draw relations overview.
            SocialCardUtility.DrawRelationsAndOpinions( relationsRect, SelectedPawn );

            // need to call log drawer through reflection. Geez.
            Resources.DrawSocialLogMI.Invoke( null, new object[] { interactionsRect, pawn } );

            GUI.EndGroup();
        }

        public void DrawDetails( Rect canvas, Faction faction )
        {
            // set up rects
            Rect informationTitleRect = new Rect( 0f, 0f, canvas.width, 30f );
            Rect informationRect = new Rect( 0f, 36f, canvas.width, canvas.height / 2f - 36f );
            Rect informationViewRect = new Rect( 0f, 0f, informationRect.width - 16f, _factionInformationHeight );
            Rect relationsTitleRect = new Rect( 0f, canvas.height / 2f, canvas.width, 30f );
            Rect relationsRect = new Rect( 0f, canvas.height / 2f + 36f, canvas.width, canvas.height / 2f - 36f );
            Rect relationsViewRect = new Rect( 0f, 0f, relationsRect.width - 16f, _factionDetailHeight );

            GUI.BeginGroup( canvas );

            // draw titles
            Text.Font = GameFont.Medium;
            Widgets.Label( informationTitleRect, faction.GetCallLabel() );
            Widgets.Label( relationsTitleRect, "Fluffy_Relations.Possesive".Translate( faction.GetCallLabel() ) + "Fluffy_Relations.Relations".Translate() );
            Text.Font = GameFont.Small;

            // information
            Widgets.BeginScrollView( informationRect, ref _factionInformationScrollPosition, informationViewRect );
            float curY = 0f;

            Rect factionTypeRect = new Rect( 0f, curY, informationRect.width, Settings.RowHeight );
            curY += Settings.RowHeight;
            Rect factionLeaderRect = new Rect( 0f, curY, informationRect.width, Settings.RowHeight );
            curY += Settings.RowHeight;
            Rect kidnappedRect = new Rect( 0f, curY, informationRect.width, Settings.RowHeight );
            curY += Settings.RowHeight;

            Widgets.Label( factionTypeRect, faction.def.LabelCap + " (" + faction.def.techLevel + ")" );
            Widgets.Label( factionLeaderRect, faction.def.leaderTitle + ": " + GetFactionLeader( faction ).Name );
            if ( faction.kidnapped?.KidnappedPawnsListForReading.Count > 0 )
            {
                Widgets.Label( kidnappedRect, "Fluffy_Relations.KidnappedColonists".Translate() + ":" );
                foreach ( Pawn kidnappee in faction.kidnapped.KidnappedPawnsListForReading )
                {
                    Rect kidnappeeRow = new Rect( 0f, curY, informationRect.width, Settings.RowHeight );
                    curY += Settings.RowHeight;

                    Widgets.Label( kidnappeeRow, "\t" + kidnappee.Name );
                }
            }

            _factionInformationHeight = curY;
            Widgets.EndScrollView();

            // relations
            Widgets.BeginScrollView( relationsRect, ref _factionDetailScrollPosition, relationsViewRect );
            curY = 0f;

            foreach ( Faction otherFaction in Find.FactionManager
                                                  .AllFactionsVisible
                                                  .Where( of => of != faction &&
                                                          of.RelationWith( faction, true ) != null )
                                                  .OrderByDescending( of => of.GoodwillWith( faction ) ) )
            {
                {
                    Rect row = new Rect( 0f, curY, canvas.width, Settings.RowHeight );
                    curY += Settings.RowHeight;

                    int opinion = Mathf.RoundToInt( faction.GoodwillWith( otherFaction ) );
                    GUI.color = RelationDrawer.GetRelationColor( null, opinion );
                    string label = "";
                    if ( faction.HostileTo( otherFaction ) )
                        label = "HostileTo".Translate( otherFaction.GetCallLabel() );
                    else
                        label = otherFaction.GetCallLabel();
                    label += ": " + opinion;

                    Widgets.DrawHighlightIfMouseover( row );
                    Widgets.Label( row, label );
                    if ( Widgets.InvisibleButton( row ) )
                        SelectedFaction = otherFaction;
                }
            }

            // reset color
            GUI.color = Color.white;

            _factionDetailHeight = curY;
            Widgets.EndScrollView(); // relations

            GUI.EndGroup(); // canvas
        }

        public void DrawFactionRelations( Rect canvas )
        {
            RelationDrawer.ResetForNextTick();
            bool drawAll = SelectedFaction == null;
            foreach ( Faction faction in factions )
            {
                Rect slot;
                if ( !TryGetSlot( faction, out slot ) )
                    continue;

                bool selected = SelectedFaction == faction;
                foreach ( Faction otherFaction in factions )
                {
                    Rect otherSlot;
                    if ( !TryGetSlot( otherFaction, out otherSlot ) )
                        continue;

                    if ( faction != otherFaction && ( drawAll || selected ) )
                        faction.DrawLink( otherFaction, slot, otherSlot, selected );
                }
            }
            foreach ( Faction faction in factions )
            {
                Rect slot;
                if ( !TryGetSlot( faction, out slot ) )
                    continue;

                if ( GetFactionLeader( faction ).DrawSlot( slot, drawBG: false, label: faction.GetCallLabel() ) )
                {
                    Event.current.Use();
                    SelectedFaction = faction;
                }

                if ( Mouse.IsOver( slot ) || faction == SelectedFaction )
                    GUI.DrawTexture( slot, Resources.SelectionReticule );

                TooltipHandler.TipRegion( slot, "Fluffy_Relations.ClickToSelect".Translate( faction.GetCallLabel() ) );
                if ( SelectedFaction != null && SelectedFaction != faction )
                    TooltipHandler.TipRegion( slot,
                        "Fluffy_Relations.Possesive".Translate( SelectedFaction.GetCallLabel() ) +
                        "Fluffy_Relations.OpinionOf".Translate( faction.GetCallLabel(), Mathf.RoundToInt( SelectedFaction.GoodwillWith( faction ) ) ) );
            }

            // draw legend or details in the detail rect
            if ( SelectedFaction != null )
                DrawDetails( detailRect, SelectedFaction );
            else
                DrawLegend( detailRect );

            // reset selected if click somewhere in network rect
            if ( Widgets.InvisibleButton( networkRect ) )
                SelectedFaction = null;
        }

        public void DrawLegend( Rect canvas )
        {
            // TODO: Draw legend.
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.grey;
            Widgets.Label( canvas, "Fluffy_Relations.NothingSelected".Translate() );
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public void DrawPawnRelations( Rect canvas )
        {
            // catch selected pawn changes ( clicking on relations and/or log entries will move selector )
            if ( CurrentPage == Page.Colonists )
                UpdateSelectedPawn();

            // draw pawn relations
            RelationDrawer.ResetForNextTick();
            bool drawAll = SelectedPawn == null;
            foreach ( Pawn pawn in pawns )
            {
                bool selected = SelectedPawn == pawn;
                foreach ( Pawn otherPawn in pawns )
                {
                    if ( pawn != otherPawn && ( drawAll || selected ) )
                        pawn.DrawLink( otherPawn, Slots[pawn], Slots[otherPawn], selected );
                }
            }

            // draw pawn slots
            foreach ( Pawn pawn in pawns )
            {
                if ( pawn.DrawSlot( Slots[pawn], drawBG: false ) )
                {
                    Event.current.Use();
                    SelectedPawn = pawn;
                }

                if ( Mouse.IsOver( Slots[pawn] ) || pawn == SelectedPawn )
                    GUI.DrawTexture( Slots[pawn], Resources.SelectionReticule );
                TooltipHandler.TipRegion( Slots[pawn], "Fluffy_Relations.ClickToSelect".Translate( pawn.LabelBaseShort ) );
                if ( SelectedPawn != null && SelectedPawn != pawn )
                    TooltipHandler.TipRegion( Slots[pawn],
                        "Fluffy_Relations.Possesive".Translate( SelectedPawn.LabelBaseShort ) +
                        SelectedPawn.relations.OpinionExplanation( pawn ) +
                        "\n\n" + "Fluffy_Relations.Possesive".Translate( pawn.LabelBaseShort ) +
                        pawn.relations.OpinionExplanation( SelectedPawn ) );
            }

            // draw legend or details in the detail rect
            if ( SelectedPawn != null )
                DrawDetails( detailRect, SelectedPawn );
            else
                DrawLegend( detailRect );

            // reset selected if click somewhere in network rect
            if ( Widgets.InvisibleButton( networkRect ) )
                SelectedPawn = null;
        }

        public override void PostClose()
        {
            base.PostClose();

            _lastSelectedPawn = null;
        }

        public override void PreOpen()
        {
            base.PreOpen();

            Resources.InitIfNeeded();
        }

        public bool TryGetSlot( Faction faction, out Rect slot )
        {
            slot = new Rect();
            if ( faction == null )
                return false;

            Pawn pawn = GetFactionLeader( faction );
            return TryGetSlot( pawn, out slot );
        }

        public bool TryGetSlot( Pawn pawn, out Rect slot )
        {
            slot = new Rect();
            if ( pawn == null )
                return false;

            if ( !Slots.TryGetValue( pawn, out slot ) )
            {
                // try re-caching
                CreateSlots();
                if ( !Slots.TryGetValue( pawn, out slot ) )
                    return false;
            }
            return true;
        }

        public void UpdateSelectedPawn()
        {
            if ( Find.Selector.SingleSelectedThing as Pawn != _lastSelectedPawn )
            {
                SelectedPawn = Find.Selector.SingleSelectedThing as Pawn;
                _lastSelectedPawn = SelectedPawn;
            }
        }

        /// <summary>
        /// Builds pawn list + slot positions
        /// called from base.PreOpen();
        /// </summary>
        protected override void BuildPawnList()
        {
            // rebuild pawn list
            base.BuildPawnList();

            // recalculate positions
            CreateAreas();
            CreateSlots();

            // clear label cache
            PawnSlotDrawer.ClearLabelRectCache();
        }

        protected override void DrawPawnRow( Rect r, Pawn p )
        {
            // required implementation
            return;
        }

        // split the screen into two areas
        private void CreateAreas()
        {
            // social network on the right, always square, try to fill the whole height - but limited by width.
            float networkSize = Mathf.Min( Screen.height - 35f, Screen.width - Settings.MinDetailWidth ) - 2 * WindowPadding;
            networkRect = new Rect( 0f, 0f, networkSize, networkSize );
            networkRect.x = Screen.width - networkSize - 2 * WindowPadding;

            // detail view on the left, full height (minus what is needed for faction/colonists selection) - fill available width
            detailRect = new Rect( 0f, 36f, Screen.width - networkSize - WindowPadding * 2, Screen.height - 35f - WindowPadding * 2 );

            // selection button rect
            sourceButtonRect = new Rect( 0f, 0f, 200f, 30f );
        }

        private Pawn GetFactionLeader( Faction faction )
        {
            if ( faction == Faction.OfColony )
                return Find.MapPawns.FreeColonists.First();

            if ( faction.leader == null )
                faction.GenerateNewLeader();

            return faction.leader;
        }

        private Rect GetRectOnCircle( int i, int n, float radius, Vector2 centre )
        {
            Rect slot = new Rect( 0f, 0f, Settings.SlotSize, Settings.SlotSize );

            // calculate position on circle
            Vector2 position = new Vector2( radius * Mathf.Cos( Settings.Radians / n * i + Settings.RadianTop ),
                                            radius * Mathf.Sin( Settings.Radians / n * i + Settings.RadianTop ) );
            position += centre;

            // set slot center position
            slot.center = position;
            return slot;
        }

        #endregion Methods
    }
}