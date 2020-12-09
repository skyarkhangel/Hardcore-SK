using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    public static class Analyzer
    {
        private const int MAX_LOG_COUNT = 2000;
        private static int currentLogCount = 0; // How many update cycles have passed since beginning profiling an entry?
        public static List<ProfileLog> logs = new List<ProfileLog>();

        // todo, how can I do this more elegantly?
        private static Comparer<ProfileLog> maxComparer = Comparer<ProfileLog>.Create((ProfileLog first, ProfileLog second) => first.max < second.max ? 1 : -1);
        private static Comparer<ProfileLog> averageComparer = Comparer<ProfileLog>.Create((ProfileLog first, ProfileLog second) => first.average < second.average ? 1 : -1);
        private static Comparer<ProfileLog> percentComparer = Comparer<ProfileLog>.Create((ProfileLog first, ProfileLog second) => first.percent < second.percent ? 1 : -1);
        private static Comparer<ProfileLog> totalComparer = Comparer<ProfileLog>.Create((ProfileLog first, ProfileLog second) => first.total < second.total ? 1 : -1);
        private static Comparer<ProfileLog> callsComparer = Comparer<ProfileLog>.Create((ProfileLog first, ProfileLog second) => first.calls < second.calls ? 1 : -1);
        private static Comparer<ProfileLog> nameComparer = Comparer<ProfileLog>.Create((ProfileLog first, ProfileLog second) => string.Compare(first.label, second.label));

        private static object logicSync = new object();

        private static bool currentlyProfiling = false;
        private static bool currentlyPaused = false;

        public static List<ProfileLog> Logs => logs;
        public static object LogicLock => logicSync;


        public static bool CurrentlyPaused { get => currentlyPaused; set => currentlyPaused = value; }
        public static bool CurrentlyProfiling => currentlyProfiling && !CurrentlyPaused;

        public static int GetCurrentLogCount => currentLogCount;

        // After this function has been called, the analyzer will be actively profiling / incuring lag :)
        public static void BeginProfiling() => currentlyProfiling = true;
        public static void EndProfiling() => currentlyProfiling = false;

        public static bool CurrentlyCleaningUp { get; set; } = false;
        public static SortBy SortBy { get; set; } = SortBy.Percent;

        public static void RefreshLogCount()
        {
            currentLogCount = 0;
            lock (LogicLock)
            {
                logs.Clear();
            }
        }

        // Called every update period (tick / root update)
        internal static void UpdateCycle()
        {
            foreach (var profile in ProfileController.Profiles)
                profile.Value.RecordMeasurement();

            if (currentLogCount < MAX_LOG_COUNT)
                currentLogCount++;
        }

        // Called a variadic amount depending on the user settings
        // Calculates stats for all active profilers (not only the currently selected one)
        internal static void FinishUpdateCycle()
        {
            if (ProfileController.Profiles.Count != 0)
            {
                Comparer<ProfileLog> comparer = percentComparer;
                switch (SortBy)
                {
                    case SortBy.Max: comparer = maxComparer; break;
                    case SortBy.Average: comparer = averageComparer; break;
                    case SortBy.Percent: comparer = percentComparer; break;
                    case SortBy.Total: comparer = totalComparer; break;
                    case SortBy.Calls: comparer = callsComparer; break;
                    case SortBy.Name: comparer = nameComparer; break;
                }

                Task.Factory.StartNew(() => ProfileCalculations(new Dictionary<string, Profiler>(ProfileController.Profiles), currentLogCount, comparer));
            }
        }

        public static void PatchEntry(Entry entry)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var meth = AccessTools.Method(entry.type, "ProfilePatch");

                    if(meth != null) meth.Invoke(null, null);
                    else MethodTransplanting.PatchMethods(entry.type);

                    entry.isLoading = false;
                    entry.isPatched = true;
                }
                catch (Exception e)
                {
#if DEBUG
                    ThreadSafeLogger.Error($"[Analyzer] Failed to patch entry, failed with the message {e.Message}");
#endif
#if NDEBUG
                    if (Settings.verboseLogging)
                        ThreadSafeLogger.Error($"[Analyzer] Failed to patch entry, failed with the message {e.Message}");
#endif
                }
            });
        }

        public static void Cleanup()
        {
            Task.Factory.StartNew(() => CleanupBackground());
        }

        // n = count of profiles
        // m = number of logs
        // o(n*m + n*log(n)); - Could maybe thread this some more, to push higher update speeds
        private static void ProfileCalculations(Dictionary<string, Profiler> Profiles, int currentLogCount, Comparer<ProfileLog> comparer)
        {
            List<ProfileLog> newLogs = new List<ProfileLog>(Profiles.Count);

            double sumOfAverages = 0;

            foreach (var value in Profiles.Values) // o(n)
            {
                // o(m)
                value.CollectStatistics(Mathf.Min(currentLogCount, MAX_LOG_COUNT - 1), out var average, out var max, out var total, out var calls, out var maxCalls);
                newLogs.Add(new ProfileLog(value.label, string.Empty, average, (float)max, null, value.key, string.Empty, 0, (float)total, calls, maxCalls, value.type, value.meth));

                sumOfAverages += average;
            }

            List<ProfileLog> sortedLogs = new List<ProfileLog>(newLogs.Count);

            foreach (var log in newLogs) // o(n)
            {
                float adjustedAverage = (float)(log.average / sumOfAverages);
                log.percent = adjustedAverage;
                log.percentString = adjustedAverage.ToStringPercent();

                // o(logn)
                BinaryInsertion(sortedLogs, log, comparer);
            }

            // Swap our old logs with the new ones
            lock (LogicLock)
            {
                logs = sortedLogs;
            }
        }

        // Assume the array is currently sorted
        // We are looking for a position to insert a new entry
        private static void BinaryInsertion(List<ProfileLog> logs, ProfileLog value, Comparer<ProfileLog> comparer)
        {
            int index = Mathf.Abs(logs.BinarySearch(value, comparer) + 1);

            logs.Insert(index, value);
        }

        // Remove all patches
        // Remove all internal patches
        // Clear all caches which hold information to prevent double patching
        // Clear all temporary entries
        // Clear all profiles
        // Clear all logs

        private static void CleanupBackground()
        {
            try
            {
                CurrentlyCleaningUp = true;

                // unpatch all methods
                Modbase.Harmony.UnpatchAll(Modbase.Harmony.Id);

#if DEBUG 
                ThreadSafeLogger.Warning("Unpatched all profiling methods");
#endif
                // atomic reads and writes.
                Modbase.isPatched = false;

                // clear all patches to prevent double patching
                Utility.ClearPatchCaches();
#if DEBUG 
                ThreadSafeLogger.Warning("Cleared Patch Caches");
#endif

                // clear all profiles
                ProfileController.Profiles.Clear();
#if DEBUG 
                ThreadSafeLogger.Warning("Cleared Profiles");
#endif

                // clear all logs
                Analyzer.Logs.Clear();
#if DEBUG 
                ThreadSafeLogger.Warning("Cleared Logs");
#endif

                // clear all temp entries
                GUIController.ClearEntries();

#if DEBUG 
                ThreadSafeLogger.Warning("Cleared Entries");
#endif
                // call GC
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch(Exception e)
            {
                ThreadSafeLogger.Error("Failed to cleanup analyzer, failed with the error " + e.Message);
            }

#if DEBUG 
            ThreadSafeLogger.Message($"Finished state cleanup");
#endif
            CurrentlyCleaningUp = false;
        }
    }
}
