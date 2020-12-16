using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    public static class ProfileController
    {
        public static Dictionary<string, Profiler> profiles = new Dictionary<string, Profiler>();

#if DEBUG
        private static bool midUpdate = false;
#endif
        private static float deltaTime = 0.0f;
        public static float updateFrequency => 1 / Settings.updatesPerSecond; // how many ms per update (capped at every 0.05ms)

        public static Dictionary<string, Profiler> Profiles => profiles;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Profiler Start(string key, Func<string> GetLabel = null, Type type = null, Def def = null, Thing thing = null, MethodBase meth = null)
        {
            if (!Analyzer.CurrentlyProfiling) return null;

            if (Profiles.TryGetValue(key, out var prof)) return prof.Start();
            else
            {
                Profiles[key] = GetLabel != null ? new Profiler(key, GetLabel(), type, def, thing, meth)
                                                 : new Profiler(key, key, type, def, thing, meth);

                return Profiles[key].Start();
            }
        }

        public static void Stop(string key)
        {
            if (Profiles.TryGetValue(key, out Profiler prof))
                prof.Stop();
        }

        // Mostly here for book keeping, optimised out of a release build.
        [Conditional("DEBUG")]
        public static void BeginUpdate()
        {
#if DEBUG
            if (Analyzer.CurrentlyPaused) return;

            if (midUpdate) ThreadSafeLogger.Error("[Analyzer] Attempting to begin new update cycle when the previous update has not ended");
            midUpdate = true;
#endif
        }

        public static void EndUpdate()
        {
            if (Analyzer.CurrentlyPaused) return;

            Analyzer.UpdateCycle(); // Update all our profilers, record measurements

            deltaTime += Time.deltaTime;
            if (deltaTime >= updateFrequency)
            {
                Analyzer.FinishUpdateCycle(); // Process the information for all our profilers.
                deltaTime -= updateFrequency;
            }
#if DEBUG
            midUpdate = false;
#endif
        }
    }
}
