using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using Verse;
using Verse.AI;

namespace Analyzer.Profiling
{
    [StaticConstructorOnStartup]
    internal class H_FindPath
    {
        public static bool Active = false;

        public static bool pathing;

        public static int NodeIndex = 0;

        public static Entry p = Entry.Create("entry.tick.pathfinder", Category.Tick, typeof(H_FindPath), false);
        public static void ProfilePatch()
        {
            HarmonyMethod go = new HarmonyMethod(typeof(H_FindPath), nameof(Start));
            HarmonyMethod biff = new HarmonyMethod(typeof(H_FindPath), nameof(Stop));

            void slop(Type e, string s)
            {
                Modbase.Harmony.Patch(AccessTools.Method(e, s), go, biff);
            }

            MethodInfo mad = AccessTools.Method(typeof(Reachability), nameof(Reachability.CanReach),
                new[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseParms) });
            Modbase.Harmony.Patch(mad, go, biff);

            // slop(typeof(PathFinder), nameof(PathFinder.CalculateDestinationRect));
            slop(typeof(PathFinder), nameof(PathFinder.GetAllowedArea));
            slop(typeof(PawnUtility), nameof(PawnUtility.ShouldCollideWithPawns));
            slop(typeof(PathFinder), nameof(PathFinder.DetermineHeuristicStrength));
            slop(typeof(PathFinder), nameof(PathFinder.CalculateAndAddDisallowedCorners));
            slop(typeof(PathFinder), nameof(PathFinder.InitStatusesAndPushStartNode));
        }

        [HarmonyPriority(Priority.Last)]
        public static void Start(MethodBase __originalMethod, ref Profiler __state)
        {
            if (p.isActive)
            {
                __state = p.Start(__originalMethod.Name, __originalMethod);
            }
        }

        [HarmonyPriority(Priority.First)]
        public static void Stop(Profiler __state)
        {
            if (p.isActive)
            {
                __state?.Stop();
            }
        }

        [HarmonyPriority(Priority.First)]
        public static void Prefix(MethodBase __originalMethod, ref Profiler __state)
        {
            if (p.isActive)
            {
                __state = p.Start("PathFinder.FindPath", __originalMethod);
                pathing = true;
            }
        }

        [HarmonyPriority(Priority.Last)]
        public static void Postfix(Profiler __state)
        {
            if (p.isActive)
            {
                pathing = false;
                __state?.Stop();
            }
        }
    }
}