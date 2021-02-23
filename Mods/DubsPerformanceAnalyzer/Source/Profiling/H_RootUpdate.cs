using HarmonyLib;
using System;
using UnityEngine;
using Verse;

namespace Analyzer.Profiling
{
    [Entry("entry.update.frametimes", Category.Update)]
    public class H_RootUpdate
    {
        public static bool Active = false;
        public static string _fpsText;
        public static float _hudRefreshRate = 1f;
        public static float _timer;
        public static string tps;
        private static DateTime PrevTime;
        private static int PrevTicks;
        public static int TPSActual;
        public static float delta;

        public static long CurrentAllocatedMemory;
        public static long LastMinGC;
        public static long LastMaxGC;

        public static string GarbageCollectionInfo { get; private set; }

        public static long totalBytesOfMemoryUsed;

        public static void Prefix()
        {
            if (Analyzer.CurrentlyProfiling)
            {
                long jam = totalBytesOfMemoryUsed;
                totalBytesOfMemoryUsed = GC.GetTotalMemory(false);

                if (jam > totalBytesOfMemoryUsed)
                {
                    LastMinGC = totalBytesOfMemoryUsed;
                    LastMaxGC = jam;
                    GarbageCollectionInfo = $"{totalBytesOfMemoryUsed.ToMb()}MB";
                }

                delta += Time.deltaTime;
                if (delta >= 1f)
                {
                    delta -= 1f;

                    long PreviouslyAllocatedMemory = CurrentAllocatedMemory;
                    CurrentAllocatedMemory = GC.GetTotalMemory(false);

                    long MemoryDifference = CurrentAllocatedMemory - PreviouslyAllocatedMemory;
                    if (MemoryDifference < 0)
                        MemoryDifference = 0;

                    GarbageCollectionInfo = $"{CurrentAllocatedMemory.ToMb():0.00}MB +{MemoryDifference.ToMb():0.00}MB/s";
                }


                if (Time.unscaledTime > _timer)
                {
                    int fps = (int)(1f / Time.unscaledDeltaTime);
                    _fpsText = "FPS: " + fps;
                    _timer = Time.unscaledTime + _hudRefreshRate; // Every second we update our fps
                }

                try
                {
                    if (Current.ProgramState == ProgramState.Playing)
                    {
                        float TRM = Find.TickManager.TickRateMultiplier;
                        int TPSTarget = (int)Math.Round(TRM == 0f ? 0f : 60f * TRM);

                        DateTime CurrTime = DateTime.Now;

                        if (CurrTime.Second != PrevTime.Second)
                        {
                            PrevTime = CurrTime;
                            TPSActual = GenTicks.TicksAbs - PrevTicks;
                            PrevTicks = GenTicks.TicksAbs;
                        }

                        tps = $"TPS: {TPSActual}({TPSTarget})";
                    }
                }
                catch (Exception) { }
            }

            if (Active)
                ProfileController.Start("Game Update");

#if DEBUG
            if (GUIController.CurrentCategory != Category.Tick)
                ProfileController.BeginUpdate();
#endif
        }

        public static void Postfix()
        {
            if (Active)
            {
                ProfileController.Stop("Frame times");
                ProfileController.Stop("Game Update");
            }

            if (GUIController.CurrentCategory != Category.Tick) // If we are tick, we will 'update' in the TickManager.DoSingleTick method
                ProfileController.EndUpdate();

            if (Active)
                ProfileController.Start("Frame times");

        }
    }
}