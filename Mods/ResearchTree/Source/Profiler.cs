// Profiler.cs
// Copyright Karel Kroeze, 2018-2020

using System.Diagnostics;
using Verse;

namespace FluffyResearchTree
{
    public class Profiler
    {
        [Conditional( "DEBUG" )]
        public static void Start( string label = null )
        {
            DeepProfiler.Start( label );
        }

        [Conditional( "DEBUG" )]
        public static void End()
        {
            DeepProfiler.End();
        }
    }
}