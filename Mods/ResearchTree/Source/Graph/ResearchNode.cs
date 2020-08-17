// ResearchNode.cs
// Copyright Karel Kroeze, 2020-2020

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using static FluffyResearchTree.Constants;

namespace FluffyResearchTree
{
    public class ResearchNode : Node
    {
        private static readonly Dictionary<ResearchProjectDef, bool> _buildingPresentCache =
            new Dictionary<ResearchProjectDef, bool>();

        private static readonly Dictionary<ResearchProjectDef, List<ThingDef>> _missingFacilitiesCache =
            new Dictionary<ResearchProjectDef, List<ThingDef>>();

        public ResearchProjectDef Research;

        public ResearchNode( ResearchProjectDef research )
        {
            Research = research;

            // initialize position at vanilla y position, leave x at zero - we'll determine this ourselves
            _pos = new Vector2( 0, research.researchViewY + 1 );
        }

        public List<ResearchNode> Parents
        {
            get
            {
                var parents = InNodes.OfType<ResearchNode>();
                parents.Concat( InNodes.OfType<DummyNode>().Select( dn => dn.Parent ) );
                return parents.ToList();
            }
        }

        public override Color Color
        {
            get
            {
                if ( Highlighted )
                    return GenUI.MouseoverColor;
                if ( Completed )
                    return Assets.ColorCompleted[Research.techLevel];
                if ( Available )
                    return Assets.ColorCompleted[Research.techLevel];
                return Assets.ColorUnavailable[Research.techLevel];
            }
        }

        public override Color EdgeColor
        {
            get
            {
                if ( Highlighted )
                    return GenUI.MouseoverColor;
                if ( Completed )
                    return Assets.ColorCompleted[Research.techLevel];
                if ( Available )
                    return Assets.ColorAvailable[Research.techLevel];
                return Assets.ColorUnavailable[Research.techLevel];
            }
        }

        public List<ResearchNode> Children
        {
            get
            {
                var children = OutNodes.OfType<ResearchNode>();
                children.Concat( OutNodes.OfType<DummyNode>().Select( dn => dn.Child ) );
                return children.ToList();
            }
        }

        public override string Label => Research.LabelCap;

        public static bool BuildingPresent( ResearchProjectDef research )
        {
            if ( DebugSettings.godMode && Prefs.DevMode )
                return true;

            // try get from cache
            bool result;
            if ( _buildingPresentCache.TryGetValue( research, out result ) )
                return result;

            // do the work manually
            if ( research.requiredResearchBuilding == null )
                result = true;
            else
                result = Find.Maps.SelectMany( map => map.listerBuildings.allBuildingsColonist )
                             .OfType<Building_ResearchBench>()
                             .Any( b => research.CanBeResearchedAt( b, true ) );

            if ( result )
                result = research.Ancestors().All( BuildingPresent );

            // update cache
            _buildingPresentCache.Add( research, result );
            return result;
        }

        public static bool TechprintAvailable( ResearchProjectDef research )
        {
            return research.TechprintRequirementMet;
        }

        public static void ClearCaches()
        {
            _buildingPresentCache.Clear();
            _missingFacilitiesCache.Clear();
        }

        public static implicit operator ResearchNode( ResearchProjectDef def )
        {
            return def.ResearchNode();
        }

        public int Matches( string query )
        {
            var culture = CultureInfo.CurrentUICulture;
            query = query.ToLower( culture );

            if ( Research.LabelCap.RawText.ToLower( culture ).Contains( query ) )
                return 1;
            if ( Research.GetUnlockDefsAndDescs()
                         .Any( unlock => unlock.First.LabelCap.RawText.ToLower( culture ).Contains( query ) ) )
                return 2;
            if ( Research.description.ToLower( culture ).Contains( query ) )
                return 3;
            return 0;
        }

        public static List<ThingDef> MissingFacilities( ResearchProjectDef research )
        {
            // try get from cache
            List<ThingDef> missing;
            if ( _missingFacilitiesCache.TryGetValue( research, out missing ) )
                return missing;

            // get list of all researches required before this
            var thisAndPrerequisites = research.Ancestors().Where( rpd => !rpd.IsFinished ).ToList();
            thisAndPrerequisites.Add( research );

            // get list of all available research benches
            var availableBenches = Find.Maps.SelectMany( map => map.listerBuildings.allBuildingsColonist )
                                       .OfType<Building_ResearchBench>();
            var availableBenchDefs = availableBenches.Select( b => b.def ).Distinct();
            missing = new List<ThingDef>();

            // check each for prerequisites
            // TODO: We should really build this list recursively so we can re-use results for prerequisites.
            foreach ( var rpd in thisAndPrerequisites )
            {
                if ( rpd.requiredResearchBuilding == null )
                    continue;

                if ( !availableBenchDefs.Contains( rpd.requiredResearchBuilding ) )
                    missing.Add( rpd.requiredResearchBuilding );

                if ( rpd.requiredResearchFacilities.NullOrEmpty() )
                    continue;

                foreach ( var facility in rpd.requiredResearchFacilities )
                    if ( !availableBenches.Any( b => b.HasFacility( facility ) ) )
                        missing.Add( facility );
            }

            // add to cache
            missing = missing.Distinct().ToList();
            _missingFacilitiesCache.Add( research, missing );
            return missing;
        }

        public bool BuildingPresent()
        {
            return BuildingPresent( Research );
        }
        
        public bool TechprintAvailable()
        {
            return TechprintAvailable( Research );
        }

        /// <summary>
        ///     Draw the node, including interactions.
        /// </summary>
        public override void Draw( Rect visibleRect, bool forceDetailedMode = false )
        {
            if ( !IsVisible( visibleRect ) )
            {
                Highlighted = false;
                return;
            }

            var detailedMode = forceDetailedMode ||
                               MainTabWindow_ResearchTree.Instance.ZoomLevel < DetailedModeZoomLevelCutoff;
            var mouseOver = Mouse.IsOver( Rect );
            if ( Event.current.type == EventType.Repaint )
            {
                // researches that are completed or could be started immediately, and that have the required building(s) available
                GUI.color = mouseOver ? GenUI.MouseoverColor : Color;

                if ( mouseOver || Highlighted )
                    GUI.DrawTexture( Rect, Assets.ButtonActive );
                else
                    GUI.DrawTexture( Rect, Assets.Button );

                // grey out center to create a progress bar effect, completely greying out research not started.
                if ( Available )
                {
                    var progressBarRect = Rect.ContractedBy( 3f );
                    GUI.color            =  Assets.ColorAvailable[Research.techLevel];
                    progressBarRect.xMin += Research.ProgressPercent * progressBarRect.width;
                    GUI.DrawTexture( progressBarRect, BaseContent.WhiteTex );
                }

                Highlighted = false;


                // draw the research label
                if ( !Completed && !Available )
                    GUI.color = Color.grey;
                else
                    GUI.color = Color.white;

                if ( detailedMode )
                {
                    Text.Anchor   = TextAnchor.UpperLeft;
                    Text.WordWrap = false;
                    Text.Font     = _largeLabel ? GameFont.Tiny : GameFont.Small;
                    Widgets.Label( LabelRect, Research.LabelCap );
                }
                else
                {
                    Text.Anchor   = TextAnchor.MiddleCenter;
                    Text.WordWrap = false;
                    Text.Font     = GameFont.Medium;
                    Widgets.Label( Rect, Research.LabelCap );
                }

                // draw research cost and icon
                if ( detailedMode )
                {
                    Text.Anchor = TextAnchor.UpperRight;
                    Text.Font   = Research.CostApparent > 1000000 ? GameFont.Tiny : GameFont.Small;
                    Widgets.Label( CostLabelRect, Research.CostApparent.ToStringByStyle( ToStringStyle.Integer ) );
                    GUI.DrawTexture( CostIconRect, !Completed && !Available ? Assets.Lock : Assets.ResearchIcon,
                                     ScaleMode.ScaleToFit );
                }

                Text.WordWrap = true;

                // attach description and further info to a tooltip
                TooltipHandler.TipRegion( Rect, GetResearchTooltipString, Research.GetHashCode() );
                if ( !BuildingPresent() )
                    TooltipHandler.TipRegion( Rect, "Fluffy.ResearchTree.MissingFacilities".Translate(
                                                  string.Join( ", ",
                                                               MissingFacilities()
                                                                  .Select( td => td.LabelCap ).ToArray() ) ) );

                else if ( !TechprintAvailable() )
                    TooltipHandler.TipRegion( Rect, "Fluffy.ResearchTree.MissingTechprints".Translate(
                                                  Research.TechprintsApplied, Research.techprintCount ) );

                // draw unlock icons
                if ( detailedMode )
                {
                    var unlocks = Research.GetUnlockDefsAndDescs();
                    for ( var i = 0; i < unlocks.Count; i++ )
                    {
                        var iconRect = new Rect(
                            IconsRect.xMax - ( i                + 1 )          * ( IconSize.x + 4f ),
                            IconsRect.yMin + ( IconsRect.height - IconSize.y ) / 2f,
                            IconSize.x,
                            IconSize.y );

                        if ( iconRect.xMin - IconSize.x < IconsRect.xMin &&
                             i             + 1          < unlocks.Count )
                        {
                            // stop the loop if we're about to overflow and have 2 or more unlocks yet to print.
                            iconRect.x = IconsRect.x + 4f;
                            GUI.DrawTexture( iconRect, Assets.MoreIcon, ScaleMode.ScaleToFit );
                            var tip = string.Join( "\n",
                                                   unlocks.GetRange( i, unlocks.Count - i ).Select( p => p.Second )
                                                          .ToArray() );
                            TooltipHandler.TipRegion( iconRect, tip );
                            // new TipSignal( tip, Settings.TipID, TooltipPriority.Pawn ) );
                            break;
                        }

                        // draw icon
                        unlocks[i].First.DrawColouredIcon( iconRect );

                        // tooltip
                        TooltipHandler.TipRegion( iconRect, unlocks[i].Second );
                    }
                }

                if ( mouseOver )
                {
                    // highlight prerequisites if research available
                    if ( Available )
                    {
                        Highlighted = true;
                        foreach ( var prerequisite in GetMissingRequiredRecursive() )
                            prerequisite.Highlighted = true;
                    }
                    // highlight children if completed
                    else if ( Completed )
                    {
                        foreach ( var child in Children )
                            child.Highlighted = true;
                    }
                }
            }

            // if clicked and not yet finished, queue up this research and all prereqs.
            if ( Widgets.ButtonInvisible( Rect ) && Available )
            {
                // LMB is queue operations, RMB is info
                if ( Event.current.button == 0 && !Research.IsFinished )
                {
                    if ( !Queue.IsQueued( this ) )
                    {
                        // if shift is held, add to queue, otherwise replace queue
                        var queue = GetMissingRequiredRecursive()
                                   .Concat( new List<ResearchNode>( new[] {this} ) )
                                   .Distinct();
                        Queue.EnqueueRange( queue, Event.current.shift );
                    }
                    else
                    {
                        Queue.Dequeue( this );
                    }
                }

                if ( DebugSettings.godMode && Prefs.DevMode && Event.current.button == 1 && !Research.IsFinished )
                {
                    Find.ResearchManager.FinishProject( Research );
                    Queue.Notify_InstantFinished();
                }
            }
        }

        /// <summary>
        ///     Get recursive list of all incomplete prerequisites
        /// </summary>
        /// <returns>List<Node> prerequisites</Node></returns>
        public List<ResearchNode> GetMissingRequiredRecursive()
        {
            var parents = Research.prerequisites?.Where( rpd => !rpd.IsFinished ).Select( rpd => rpd.ResearchNode() );
            if ( parents == null )
                return new List<ResearchNode>();
            var allParents = new List<ResearchNode>( parents );
            foreach ( var parent in parents )
                allParents.AddRange( parent.GetMissingRequiredRecursive() );

            return allParents.Distinct().ToList();
        }

        public override bool Completed => Research.IsFinished;
        public override bool Available => !Research.IsFinished && ( DebugSettings.godMode || (BuildingPresent() && TechprintAvailable()));

        public List<ThingDef> MissingFacilities()
        {
            return MissingFacilities( Research );
        }

        /// <summary>
        ///     Creates text version of research description and additional unlocks/prereqs/etc sections.
        /// </summary>
        /// <returns>string description</returns>
        private string GetResearchTooltipString()
        {
            // start with the descripton
            var text = new StringBuilder();
            text.AppendLine( Research.description );
            text.AppendLine();

            if ( Queue.IsQueued( this ) )
            {
                text.AppendLine( "Fluffy.ResearchTree.LClickRemoveFromQueue".Translate() );
            }
            else
            {
                text.AppendLine( "Fluffy.ResearchTree.LClickReplaceQueue".Translate() );
                text.AppendLine( "Fluffy.ResearchTree.SLClickAddToQueue".Translate() );
            }

            if ( DebugSettings.godMode ) text.AppendLine( "Fluffy.ResearchTree.RClickInstaFinish".Translate() );


            return text.ToString();
        }

        public void DrawAt( Vector2 pos, Rect visibleRect, bool forceDetailedMode = false )
        {
            SetRects( pos );
            Draw( visibleRect, forceDetailedMode );
            SetRects();
        }
    }
}