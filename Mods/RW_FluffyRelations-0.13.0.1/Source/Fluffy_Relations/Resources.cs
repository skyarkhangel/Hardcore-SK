using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Relations
{
    public static class Resources
    {
        #region Fields

        public static Color
            ColorNeutral,
            ColorFriend,
            ColorEnemy,
            ColorLover,
            ColorExLover,
            ColorFamily;

        public static MethodInfo DrawSocialLogMI = typeof( SocialCardUtility ).GetMethod( "DrawInteractionsLog", BindingFlags.Static | BindingFlags.NonPublic );

        public static Texture2D
            SlightlyDarkBG,

            // icons
            IconMarried,
            IconFiancee,
            IconLover,
            IconEnemy,
            IconEx,
            IconFamilyNuclear,
            IconFamilyBlood,
            IconFamilyExtention,
            IconAlliance,
            IconPeace,

            // misc
            SelectionReticule;

        private static bool _initialized;

        #endregion Fields

        #region Methods

        public static void InitIfNeeded()
        {
            if ( _initialized )
                return;

            SlightlyDarkBG        = SolidColorMaterials.NewSolidColorTexture( new Color( 0f, 0f, 0f, .4f ) );

            // icons
            IconMarried           = ContentFinder<Texture2D>.Get( "UI/Icons/Spouse", false );
            IconFiancee           = ContentFinder<Texture2D>.Get( "UI/Icons/Fiancee", false );
            IconLover             = ContentFinder<Texture2D>.Get( "UI/Icons/Lover", false );
            IconEnemy             = ContentFinder<Texture2D>.Get( "UI/Icons/Enemy", false );
            IconEx                = ContentFinder<Texture2D>.Get( "UI/Icons/Ex", false );
            IconFamilyNuclear     = ContentFinder<Texture2D>.Get( "UI/Icons/Family", false );
            IconFamilyBlood       = ContentFinder<Texture2D>.Get( "UI/Icons/Family", false );
            IconFamilyExtention   = ContentFinder<Texture2D>.Get( "UI/Icons/Family", false );
            IconAlliance          = ContentFinder<Texture2D>.Get( "UI/Icons/Alliance", false );
            IconPeace             = ContentFinder<Texture2D>.Get( "UI/Icons/Peace", false );

            // misc
            SelectionReticule     = ContentFinder<Texture2D>.Get( "UI/Misc/Reticule" );

            // colors
            ColorNeutral = Color.grey;
            ColorFriend  = Color.green;
            ColorEnemy   = Color.red;
            ColorLover   = GenUI.MouseoverColor;
            ColorExLover = Color.cyan;
            ColorFamily  = Color.white;

            // done!
            _initialized = true;
        }

        #endregion Methods
    }
}