// Assets.cs
// Copyright Karel Kroeze, 2018-2020

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace FluffyResearchTree
{
    [StaticConstructorOnStartup]
    public static class Assets
    {
        public static            Texture2D Button       = ContentFinder<Texture2D>.Get( "Buttons/button" );
        public static            Texture2D ButtonActive = ContentFinder<Texture2D>.Get( "Buttons/button-active" );
        public static            Texture2D ResearchIcon = ContentFinder<Texture2D>.Get( "Icons/Research" );
        public static            Texture2D MoreIcon     = ContentFinder<Texture2D>.Get( "Icons/more" );
        public static            Texture2D Lock         = ContentFinder<Texture2D>.Get( "Icons/padlock" );
        internal static readonly Texture2D CircleFill   = ContentFinder<Texture2D>.Get( "Icons/circle-fill" );

        public static Color                        NegativeMouseoverColor = new Color( .4f, .1f, .1f );
        public static Dictionary<TechLevel, Color> ColorCompleted         = new Dictionary<TechLevel, Color>();
        public static Dictionary<TechLevel, Color> ColorAvailable         = new Dictionary<TechLevel, Color>();
        public static Dictionary<TechLevel, Color> ColorUnavailable       = new Dictionary<TechLevel, Color>();
        public static Color                        TechLevelColor         = new Color( 1f, 1f, 1f, .2f );

        public static Texture2D SlightlyDarkBackground =
            SolidColorMaterials.NewSolidColorTexture( 0f, 0f, 0f, .1f );

        public static Texture2D Search =
            ContentFinder<Texture2D>.Get( "Icons/magnifying-glass" );

        static Assets()
        {
            var techlevels = Tree.RelevantTechLevels;
            var n          = techlevels.Count;
            for ( var i = 0; i < n; i++ )
            {
                ColorCompleted[techlevels[i]]   = Color.HSVToRGB( 1f / n * i, .75f, .75f );
                ColorAvailable[techlevels[i]]   = Color.HSVToRGB( 1f / n * i, .33f, .33f );
                ColorUnavailable[techlevels[i]] = Color.HSVToRGB( 1f / n * i, .125f, .33f );
            }
        }

        [StaticConstructorOnStartup]
        public static class Lines
        {
            public static Texture2D Circle = ContentFinder<Texture2D>.Get( "Lines/Outline/circle" );
            public static Texture2D End    = ContentFinder<Texture2D>.Get( "Lines/Outline/end" );
            public static Texture2D EW     = ContentFinder<Texture2D>.Get( "Lines/Outline/ew" );
            public static Texture2D NS     = ContentFinder<Texture2D>.Get( "Lines/Outline/ns" );
        }
    }
}