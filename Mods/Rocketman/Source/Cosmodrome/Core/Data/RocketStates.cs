using System;
namespace RocketMan
{
    public static class RocketStates
    {
        public static int LastFrame;

        public static ContextFlag Context = ContextFlag.Unknown;

        public static int TicksSinceStarted = 0;

        public static bool DefsLoaded = false;

        public static bool SingleTickIncrement = false;

        public static int SingleTickLeft = 0;

        public static float[] StatExpiry = new float[ushort.MaxValue];

        public static FlagArray DilatedDefs = new FlagArray(ushort.MaxValue);

        public static object LOCKER = new object();
    }
}
