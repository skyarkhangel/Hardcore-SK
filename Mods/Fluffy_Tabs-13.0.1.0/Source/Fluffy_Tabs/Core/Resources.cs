using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Tabs
{
    [StaticConstructorOnStartup]
    public static class Resources
    {
        #region Fields

        public static readonly Texture2D
            Cog                     = ContentFinder<Texture2D>.Get( "UI/Icons/cog" ),
            Wrench                  = ContentFinder<Texture2D>.Get( "UI/Icons/wrench" ),
            Priorities_Int          = ContentFinder<Texture2D>.Get( "UI/Icons/numbers" ),
            Priorities_Toggle       = ContentFinder<Texture2D>.Get( "UI/Icons/checks" ),
            Priorities_Workgivers   = ContentFinder<Texture2D>.Get( "UI/Icons/columns-four" ),
            Priorities_Worktypes    = ContentFinder<Texture2D>.Get( "UI/Icons/columns-two" ),
            Priorities_Scheduler    = ContentFinder<Texture2D>.Get( "UI/Icons/clock-scheduler" ),
            Priorities_WholeDay     = ContentFinder<Texture2D>.Get( "UI/Icons/clock-24hours" ),
            WorkBoxBGTex_Bad        = ContentFinder<Texture2D>.Get( "UI/Widgets/WorkBoxBG_Bad" ),
            WorkBoxBGTex_Mid        = ContentFinder<Texture2D>.Get( "UI/Widgets/WorkBoxBG_Mid" ),
            WorkBoxBGTex_Excellent  = ContentFinder<Texture2D>.Get( "UI/Widgets/WorkBoxBG_Excellent" ),
            WorkBoxCheckTex         = ContentFinder<Texture2D>.Get( "UI/Widgets/WorkBoxCheck" ),
            PassionWorkboxMinorIcon = ContentFinder<Texture2D>.Get( "UI/Icons/PassionMinorGray" ),
            BottomArrow             = ContentFinder<Texture2D>.Get( "UI/Icons/ArrowBottom" ),
            DownArrow               = ContentFinder<Texture2D>.Get( "UI/Icons/ArrowDown" ),
            ResetButton             = ContentFinder<Texture2D>.Get( "UI/Icons/reset" ),
            TopArrow                = ContentFinder<Texture2D>.Get( "UI/Icons/ArrowTop" ),
            UpArrow                 = ContentFinder<Texture2D>.Get( "UI/Icons/ArrowUp" ),
            LeftArrow               = ContentFinder<Texture2D>.Get( "UI/Icons/ArrowLeft" ),
            RightArrow              = ContentFinder<Texture2D>.Get( "UI/Icons/ArrowRight" ),
            PassionWorkboxMajorIcon = ContentFinder<Texture2D>.Get( "UI/Icons/PassionMajorGray" ),
            MoodHappy               = ContentFinder<Texture2D>.Get( "UI/Icons/Mood/happy" ),
            MoodContent             = ContentFinder<Texture2D>.Get( "UI/Icons/Mood/content" ),
            MoodDiscontent          = ContentFinder<Texture2D>.Get( "UI/Icons/Mood/discontent" ),
            MoodUnhappy             = ContentFinder<Texture2D>.Get( "UI/Icons/Mood/unhappy" ),
            MoodBroken              = ContentFinder<Texture2D>.Get( "UI/Icons/Mood/broken" ),
            Favourite               = ContentFinder<Texture2D>.Get( "UI/Icons/favourite" ),
            PinEye                  = ContentFinder<Texture2D>.Get( "UI/Icons/pin-eye" ),
            PinClock                = ContentFinder<Texture2D>.Get( "UI/Icons/pin-clock"),
            ClockNow                = ContentFinder<Texture2D>.Get( "UI/Icons/clock-now" ),
            Clock                   = ContentFinder<Texture2D>.Get( "UI/Icons/clock" ),
            Solid                   = SolidColorMaterials.NewSolidColorTexture( Color.white );

        public static readonly Texture2D[] Icons = ContentFinder<Texture2D>.GetAllInFolder( "UI/Icons/Various" ).ToArray();

        public static Color
            HighPriority = Color.green,
            MidPriority = new Color( 0.8f, 0.7f, 0.5f ),
            LowPriority = new Color( 0.6f, 0.6f, 0.6f );

        public static Dictionary<JobDef, Texture2D> JobIcons = new Dictionary<JobDef, Texture2D>();

        #endregion Fields

        #region Methods

        public static Texture2D StatusIcon( this JobDef job )
        {
            if ( !JobIcons.ContainsKey( job ) )
                JobIcons.Add( job, ContentFinder<Texture2D>.Get( Settings.JobIconPaths[job] ) );
            return JobIcons[job];
        }

        #endregion Methods
    }
}