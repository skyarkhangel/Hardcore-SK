using CommunityCoreLibrary;
using CommunityCoreLibrary.ColorPicker;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Fluffy_Tabs
{
    internal static class Settings
    {
        #region Fields

        public const string DefaultJobIconPath = "UI/Icons/Various/unknown";
        public const float IconSize = 24f;
        public const float Margin = 6f;
        public const float MidAptCutOff = 14f;
        public const float PassionOpacity = 0.4f;
        public const float WorkgiverBoxSize = 20f;
        public const float WorkgiverColorOpacity = .1f;
        public const float WorktypeBoxSize = 24f;
        public static bool ColorCodedPassions = true;
        public static Dictionary<JobDef, string> JobIconPaths;
        public static int MaxPriority = 9;
        public static Dictionary<WorkGiverDef, string> WorkgiverDescriptions;
        public static Dictionary<WorkGiverDef, string> WorkgiverLabels;
        public static DefMap<WorkTypeDef, Color> WorktypeColors;

        #endregion Fields

        #region Constructors

        static Settings()
        {
            // initialize worktype colors.
            WorktypeColors = new DefMap<WorkTypeDef, Color>();
            var worktypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            for ( int i = 0; i < worktypes.Count; i++ )
            {
                WorktypeColors[worktypes[i]] = Color.HSVToRGB( 1f / worktypes.Count * i, 1f, 1f );
            }

            // initialize job icon paths
            JobIconPaths = new Dictionary<JobDef, string>();
            foreach ( var job in DefDatabase<JobDef>.AllDefsListForReading )
            {
                JobIconPaths.Add( job, job.DefaultIconPath() );
            }

            // initialize workgiver labels and descriptions
            WorkgiverLabels = new Dictionary<WorkGiverDef, string>();
            WorkgiverDescriptions = new Dictionary<WorkGiverDef, string>();
            foreach ( var workgiver in DefDatabase<WorkGiverDef>.AllDefsListForReading )
            {
                string label;
                string description;

                if ( workgiver.workType.workGiversByPriority.Count < 2 )
                // only one workgiver in type, use type label & description.
                {
                    label = workgiver.workType.labelShort.CapitalizeFirst();
                    description = workgiver.workType.description.CapitalizeFirst();
                }
                else
                // use workgiver label & descripion
                {
                    label = workgiver.label.CapitalizeFirst();
                    description = workgiver.description.CapitalizeFirst();
                }

                // if label is empty, use verb.
                if ( label.NullOrEmpty() )
                {
                    label = workgiver.verb.CapitalizeFirst();
                    Log.Warning( "FluffyTabs :: No label was set for WorkGiverDef " + workgiver.defName + " please inform the author of " + Find_Extensions.ModByDef( workgiver ).Name + "." );
                }

                // if description is empty, try decoding the defname
                if ( description.NullOrEmpty() )
                {
                    // very naive camelcase splitter, should help make things a tad more friendly, added sentence casing.
                    // http://stackoverflow.com/questions/773303/splitting-camelcase
                    description = System.Text.RegularExpressions.Regex.Replace( workgiver.defName, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled ).Trim().ToLower().CapitalizeFirst();
                }

                // populate dictionaries
                WorkgiverLabels.Add( workgiver, label );
                WorkgiverDescriptions.Add( workgiver, description );
            }
        }

        #endregion Constructors
    }
}