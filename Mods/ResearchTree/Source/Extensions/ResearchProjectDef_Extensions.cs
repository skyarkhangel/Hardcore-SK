// ResearchProjectDef_Extensions.cs
// Copyright Karel Kroeze, 2019-2020

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Verse;

namespace FluffyResearchTree
{
    public static class ResearchProjectDef_Extensions
    {
        private static readonly Dictionary<Def, List<Pair<Def, string>>> _unlocksCache =
            new Dictionary<Def, List<Pair<Def, string>>>();

        public static List<ResearchProjectDef> Descendants( this ResearchProjectDef research )
        {
            var descendants = new HashSet<ResearchProjectDef>();

            // recursively go through all children
            // populate initial queue
            var queue = new Queue<ResearchProjectDef>(
                DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(
                    res => res.prerequisites?.Contains( research ) ?? false ) );

            // add to the list, and queue up children.
            while ( queue.Count > 0 )
            {
                var current = queue.Dequeue();
                descendants.Add( current );

                foreach ( var descendant in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(
                    res =>
                        res.prerequisites?.Contains(
                            current ) ??
                        false && !descendants.Contains(
                            res ) ) )
                    queue.Enqueue( descendant );
            }

            return descendants.ToList();
        }

        public static IEnumerable<ThingDef> GetPlantsUnlocked( this ResearchProjectDef research )
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                                        .Where(
                                             td => td.plant?.sowResearchPrerequisites?.Contains( research ) ?? false );
        }

        public static List<ResearchProjectDef> Ancestors( this ResearchProjectDef research )
        {
            // keep a list of prerequites
            var prerequisites = new List<ResearchProjectDef>();
            if ( research.prerequisites.NullOrEmpty() )
                return prerequisites;

            // keep a stack of prerequisites that should be checked
            var stack = new Stack<ResearchProjectDef>( research.prerequisites.Where( parent => parent != research ) );

            // keep on checking everything on the stack until there is nothing left
            while ( stack.Count > 0 )
            {
                // add to list of prereqs
                var parent = stack.Pop();
                prerequisites.Add( parent );

                // add prerequitsite's prereqs to the stack
                if ( !parent.prerequisites.NullOrEmpty() )
                    foreach ( var grandparent in parent.prerequisites )
                        // but only if not a prerequisite of itself, and not a cyclic prerequisite
                        if ( grandparent != parent && !prerequisites.Contains( grandparent ) )
                            stack.Push( grandparent );
            }

            return prerequisites.Distinct().ToList();
        }

        public static IEnumerable<RecipeDef> GetRecipesUnlocked( this ResearchProjectDef research )
        {
            // recipe directly locked behind research
            var direct =
                DefDatabase<RecipeDef>.AllDefsListForReading.Where( rd => rd.researchPrerequisite == research );

            // recipe building locked behind research
            var building = DefDatabase<ThingDef>.AllDefsListForReading
                                                .Where(
                                                     td => ( td.researchPrerequisites?.Contains( research ) ?? false )
                                                        && !td.AllRecipes.NullOrEmpty() )
                                                .SelectMany( td => td.AllRecipes )
                                                .Where( rd => rd.researchPrerequisite == null );

            // return union of these two sets
            return direct.Concat( building ).Distinct();
        }

        public static IEnumerable<TerrainDef> GetTerrainUnlocked( this ResearchProjectDef research )
        {
            return DefDatabase<TerrainDef>.AllDefsListForReading
                                          .Where( td => td.researchPrerequisites?.Contains( research ) ?? false );
        }

        public static IEnumerable<ThingDef> GetThingsUnlocked( this ResearchProjectDef research )
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                                        .Where( td => td.researchPrerequisites?.Contains( research ) ?? false );
        }

        public static List<Pair<Def, string>> GetUnlockDefsAndDescs( this ResearchProjectDef research, bool dedupe = true )
        {
            if ( _unlocksCache.ContainsKey( research ) )
                return _unlocksCache[research];

            var unlocks = new List<Pair<Def, string>>();

            unlocks.AddRange( research.GetThingsUnlocked()
                                      .Where( d => d.IconTexture() != null )
                                      .Select( d => new Pair<Def, string>(
                                                   d, "Fluffy.ResearchTree.AllowsBuildingX".Translate( d.LabelCap ) ) ) );
            unlocks.AddRange( research.GetTerrainUnlocked()
                                      .Where( d => d.IconTexture() != null )
                                      .Select( d => new Pair<Def, string>(
                                                   d, "Fluffy.ResearchTree.AllowsBuildingX".Translate( d.LabelCap ) ) ) );
            unlocks.AddRange( research.GetRecipesUnlocked()
                                      .Where( d => d.IconTexture() != null )
                                      .Select( d => new Pair<Def, string>(
                                                   d, "Fluffy.ResearchTree.AllowsCraftingX".Translate( d.LabelCap ) ) ) );
            unlocks.AddRange( research.GetPlantsUnlocked()
                                      .Where( d => d.IconTexture() != null )
                                      .Select( d => new Pair<Def, string>(
                                                   d, "Fluffy.ResearchTree.AllowsPlantingX".Translate( d.LabelCap ) ) ) );

            // get unlocks for all descendant research, and remove duplicates.
            var descendants = research.Descendants();
            if ( dedupe && descendants.Any() )
            {
                var descendantUnlocks = research.Descendants()
                                                .SelectMany( c => c.GetUnlockDefsAndDescs( false ).Select( u => u.First ) )
                                                .Distinct()
                                                .ToList();
                unlocks = unlocks.Where( u => !descendantUnlocks.Contains( u.First ) ).ToList();
            }

            _unlocksCache.Add( research, unlocks );
            return unlocks;
        }

        public static ResearchNode ResearchNode( this ResearchProjectDef research )
        {
            var node = Tree.Nodes.OfType<ResearchNode>().FirstOrDefault( n => n.Research == research );
            if ( node == null )
                Log.Error( "Node for {0} not found. Was it intentionally hidden or locked?", true, research.LabelCap );
            return node;
        }
    }
}