using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.Profiling
{
    public class Watch
    {
        public Stopwatch stopwatch;
        public double lastBegin;

        public Watch()
        {
            lastBegin = 0;
            stopwatch = new Stopwatch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            lastBegin = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Stop()
        {
            stopwatch.Stop();
            return lastBegin - stopwatch.ElapsedMilliseconds;
        }

        public bool IsRunning => stopwatch.IsRunning;
        public TimeSpan Elapsed => stopwatch.Elapsed;
        public void Reset() => stopwatch.Reset();
    }
}
