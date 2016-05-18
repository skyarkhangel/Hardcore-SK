// Manager/Resources.cs
//
// Copyright Karel Kroeze, 2015.
//
// Created 2015-11-17 12:59

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FluffyManager
{
    public static class Resources
    {
        #region Fields

        public static Texture2D

            // sorting arrows
            ArrowTop,
            ArrowUp,
            ArrowDown,
            ArrowBottom,

            // stamps
            StampCompleted,
            StampSuspended,
            StampStart,

            // tab icons
            IconOverview,
            IconHammer,
            IconHunting,
            IconImportExport,
            IconForestry,
            IconLivestock,
            IconForaging,
            IconPower,

            // misc
            SlightlyDarkBackground,
            DeleteX,
            Cog,
            BarBackgroundActiveTexture,
            BarBackgroundInactiveTexture,
            Search,
            BarShader,

            // livestock header icons
            WoolIcon,
            MilkIcon,
            StageC,
            StageB,
            StageA,
            FemaleIcon,
            MaleIcon,
            MeatIcon,
            UnkownIcon;

        public static Texture2D[] LifeStages;

        #endregion Fields

        public static void Init()
        {
            // sorting arrows
            ArrowTop                     = ContentFinder<Texture2D>.Get( "UI/Buttons/ArrowTop" );
            ArrowUp                      = ContentFinder<Texture2D>.Get( "UI/Buttons/ArrowUp" );
            ArrowDown                    = ContentFinder<Texture2D>.Get( "UI/Buttons/ArrowDown" );
            ArrowBottom                  = ContentFinder<Texture2D>.Get( "UI/Buttons/ArrowBottom" );

            // stamps
            StampCompleted               = ContentFinder<Texture2D>.Get( "UI/Stamps/Completed" );
            StampSuspended               = ContentFinder<Texture2D>.Get( "UI/Stamps/Suspended" );
            StampStart                   = ContentFinder<Texture2D>.Get( "UI/Stamps/Start" );

            // tab icons
            IconOverview                 = ContentFinder<Texture2D>.Get( "UI/Icons/Overview" );
            IconHammer                   = ContentFinder<Texture2D>.Get( "UI/Icons/Hammer" );
            IconHunting                  = ContentFinder<Texture2D>.Get( "UI/Icons/Hunting" );
            IconImportExport             = ContentFinder<Texture2D>.Get( "UI/Icons/ImportExport" );
            IconForestry                 = ContentFinder<Texture2D>.Get( "UI/Icons/Tree" );
            IconLivestock                = ContentFinder<Texture2D>.Get( "UI/Icons/Livestock" );
            IconForaging                 = ContentFinder<Texture2D>.Get( "UI/Icons/Foraging" );
            IconPower                    = ContentFinder<Texture2D>.Get( "UI/Icons/Power" );

            // misc
            SlightlyDarkBackground       = SolidColorMaterials.NewSolidColorTexture( 0f, 0f, 0f, .4f );
            DeleteX                      = ContentFinder<Texture2D>.Get( "UI/Buttons/Delete", true );
            Cog                          = ContentFinder<Texture2D>.Get( "UI/Buttons/Cog" );
            BarBackgroundActiveTexture   = SolidColorMaterials.NewSolidColorTexture( new Color( 0.2f, 0.8f, 0.85f ) );
            BarBackgroundInactiveTexture = SolidColorMaterials.NewSolidColorTexture( new Color( 0.7f, 0.7f, 0.7f ) );
            Search                       = ContentFinder<Texture2D>.Get( "UI/Buttons/Search" );
            BarShader                    = ContentFinder<Texture2D>.Get( "UI/Misc/BarShader" );

            // livestock header icons
            WoolIcon                     = ContentFinder<Texture2D>.Get( "UI/Icons/wool" );
            MilkIcon                     = ContentFinder<Texture2D>.Get( "UI/Icons/milk" );
            StageC                       = ContentFinder<Texture2D>.Get( "UI/Icons/stage-3" );
            StageB                       = ContentFinder<Texture2D>.Get( "UI/Icons/stage-2" );
            StageA                       = ContentFinder<Texture2D>.Get( "UI/Icons/stage-1" );
            FemaleIcon                   = ContentFinder<Texture2D>.Get( "UI/Icons/female" );
            MaleIcon                     = ContentFinder<Texture2D>.Get( "UI/Icons/male" );
            MeatIcon                     = ContentFinder<Texture2D>.Get( "UI/Icons/meat" );
            UnkownIcon                   = ContentFinder<Texture2D>.Get( "UI/Icons/unknown" );

            LifeStages = new []{ StageA, StageB, StageC };
        }
    }
}