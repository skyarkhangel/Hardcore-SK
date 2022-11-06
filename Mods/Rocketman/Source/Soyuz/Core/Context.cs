using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RocketMan;
using Soyuz.Profiling;
using Verse;

namespace Soyuz
{
    public static class Context
    {
        public static CameraZoomRange ZoomRange;

        public static CellRect CurViewRect;

        public static SoyuzSettings Settings;

        public static Pawn ProfiledPawn;

        public static bool PartiallyDilatedContext = false;

        public static JobSettings CurJobSettings;

        public static RaceSettings CurRaceSettings;

        public static readonly FlagArray DilationEnabled = new FlagArray(ushort.MaxValue);

        public static readonly FlagArray DilationFastMovingRace = new FlagArray(ushort.MaxValue);

        public static readonly Dictionary<ThingDef, RaceSettings> DilationByDef = new Dictionary<ThingDef, RaceSettings>();

        public static readonly Dictionary<JobDef, JobSettings> JobDilationByDef = new Dictionary<JobDef, JobSettings>();

        public static int DilationRate
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (Context.ZoomRange)
                {
                    default:
                        return 1;
                    case CameraZoomRange.Closest:
                        return 60;
                    case CameraZoomRange.Close:
                        return 20;
                    case CameraZoomRange.Middle:
                        return 15;
                    case CameraZoomRange.Far:
                        return 15;
                    case CameraZoomRange.Furthest:
                        return 7;
                }
            }
        }
    }
}