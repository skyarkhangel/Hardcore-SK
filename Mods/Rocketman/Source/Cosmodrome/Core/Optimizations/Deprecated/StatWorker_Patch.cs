using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketMan.Optimizations
{
    [RocketPatch(typeof(StatWorker), nameof(StatWorker.GetValue), parameters = new[] { typeof(StatRequest), typeof(bool) })]
    public static class StatWorker_Patch
    {
        [StructLayout(LayoutKind.Sequential, Size = 12)]
        private struct CachedUnit
        {
            public readonly float value;
            public readonly int signature;
            public readonly int tick;

            public CachedUnit(float value, int signature)
            {
                this.value = value;
                this.signature = signature;
                this.tick = GenTicks.TicksGame;
            }
        }

        private static CachedUnit store;

        private static bool hijackedCaller = false;

        private const int MAX_CACHE_SIZE = 10000;

        private static Dictionary<int, CachedUnit> cache = new Dictionary<int, CachedUnit>();

        private static MethodBase mGetValueUnfinalized = AccessTools.Method(typeof(StatWorker), "GetValueUnfinalized", new[] { typeof(StatRequest), typeof(bool) });

        private static MethodBase mGetValueUnfinalized_Replacemant = AccessTools.Method(typeof(StatWorker_Patch), nameof(StatWorker_Patch.Replacemant));

        private static MethodBase mGetValueUnfinalized_Transpiler = AccessTools.Method(typeof(StatWorker_Patch), nameof(StatWorker_Patch.Transpiler));

        private static FieldInfo fHijackedCaller = AccessTools.Field(typeof(StatWorker_Patch), nameof(hijackedCaller));

        // [RocketPatch]
        private static class AutoPatcher_GetValueUnfinalized_Patch
        {
            public static HashSet<MethodBase> callingMethods = new HashSet<MethodBase>();

            public static MethodBase mInterrupt = AccessTools.Method(typeof(AutoPatcher_GetValueUnfinalized_Patch), "Interrupt");

            //public static IEnumerable<MethodBase> TargetMethods()
            //{
            //    foreach (var m in typeof(StatWorker)
            //            .AllLeafSubclasses()
            //            .Where(t => !t.IsAbstract)
            //            .Select(t => AccessTools.Method(t, "GetValueUnfinalized", parameters: new[] { typeof(StatRequest), typeof(bool) }))
            //            .Where(m => m != null && m.IsValidTarget())
            //            .ToHashSet())
            //        yield return m;
            //    MethodBase baseMethod = AccessTools.Method(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized));
            //    if (baseMethod != null && baseMethod.IsValidTarget())
            //        yield return baseMethod;
            //}

            //public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            //{
            //    List<CodeInstruction> codes = instructions.ToList();
            //    Label l1 = generator.DefineLabel();

            //    yield return new CodeInstruction(OpCodes.Ldsfld, fHijackedCaller);
            //    yield return new CodeInstruction(OpCodes.Brtrue_S, l1);
            //    yield return new CodeInstruction(OpCodes.Ldarg_0);
            //    yield return new CodeInstruction(OpCodes.Call, mInterrupt);

            //    if (codes[0].labels == null)
            //        codes[0].labels = new List<Label>();
            //    codes[0].labels.Add(l1);
            //    foreach (var code in codes)
            //        yield return code;
            //}

            //[MethodImpl(MethodImplOptions.NoInlining)]
            //public static void Interrupt(StatWorker statWorker)
            //{
            //    int tick = GenTicks.TicksGame;

            //    if (RocketPrefs.Enabled && Current.Game != null && tick >= 600 && !IgnoreMeDatabase.ShouldIgnore(statWorker.stat))
            //    {
            //        StackTrace trace = new StackTrace(true);
            //        StackFrame frame = trace.GetFrame(2);

            //        MethodBase method = Harmony.GetMethodFromStackframe(frame);
            //        if (method != null && method is MethodInfo replacement)
            //            method = Harmony.GetOriginalMethod((MethodInfo)method) ?? method;
            //        bool patched = false;
            //        try
            //        {
            //            if (method != null)
            //            {
            //                Finder.Harmony.Patch(method, transpiler: new HarmonyMethod((MethodInfo)mGetValueUnfinalized_Transpiler));
            //                patched = true;
            //            }
            //        }
            //        catch
            //        {
            //        }
            //        if (!patched)
            //        {
            //            foreach (MethodBase other in Harmony.GetAllPatchedMethods().Where(m => m.IsValidTarget()))
            //                Finder.Harmony.Patch(method, transpiler: new HarmonyMethod((MethodInfo)mGetValueUnfinalized_Transpiler));
            //        }
            //    }
            //}
        }

        //[RocketPatch(typeof(AutoPatcher_Test), nameof(AutoPatcher_Test.OnTickLong))]
        //public static class AutoPatcher_Test
        //{
        //    private static float i = 0;

        //    public static Pawn GetRandomPawn()
        //    {
        //        return Find.CurrentMap == null ? null : Find.CurrentMap.mapPawns?.AllPawns?.RandomElement() ?? null;
        //    }

        //    [MethodImpl(MethodImplOptions.NoInlining)]
        //    public static void Postfix()
        //    {
        //        Log.Message($"{i}");
        //    }

        //    [Main.OnTickLong]
        //    public static void Ticker()
        //    {
        //        OnTickLong();
        //    }

        //    [MethodImpl(MethodImplOptions.NoInlining)]
        //    public static void OnTickLong()
        //    {
        //        Pawn p = GetRandomPawn();
        //        if (p != null)
        //        {
        //            i = StatDefOf.MoveSpeed.workerInt.GetValueUnfinalized(StatRequest.For(p));
        //        }
        //    }
        //}

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            var codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                if (code.OperandIs(mGetValueUnfinalized))
                {
                    Log.Message($"ROCKETMAN: Hijacking {original.GetMethodSummary()}");
                    break;
                }
            }
            return instructions.MethodReplacer(mGetValueUnfinalized, mGetValueUnfinalized_Replacemant);
        }

        [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Replacemant(StatWorker statWorker, StatRequest req, bool applyPostProcess)
        {
            if (RocketPrefs.Enabled && Current.Game != null && Finder.SessionTicks >= 600
                && !IgnoreMeDatabase.ShouldIgnore(statWorker.stat)
                && RocketStates.Context == ContextFlag.Ticking)
            {
                int tick = GenTicks.TicksGame;
                int key = Tools.GetKey(statWorker, req, applyPostProcess);
                int signature = req.thingInt?.GetSignature() ?? -1;

                if (!cache.TryGetValue(key, out store))
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, storeExists: false);

                if (tick - store.tick - 1 > RocketStates.StatExpiry[statWorker.stat.index] || signature != store.signature)
                {
                    cache.Remove(key);
                    return UpdateCache(key, statWorker, req, applyPostProcess, tick, storeExists: true);
                }
                return store.value;
            }
            return statWorker.GetValueUnfinalized(req, applyPostProcess);
        }

        private static float UpdateCache(int key, StatWorker statWorker, StatRequest req, bool applyPostProcess,
            int tick, bool storeExists = true)
        {
            Cleanup();
            Exception error = null;
            float value = -1;
            try
            {
                hijackedCaller = true;
                value = statWorker.GetValueUnfinalized(req, applyPostProcess);
            }
            catch (Exception er)
            {
                error = er;
            }
            finally
            {
                hijackedCaller = false;
                if (error != null)
                {
                    Logger.Debug($"ROCKETMAN:[NOTROCKETMAN] RocketMan caught an error in StatWorker.GetValueUnfinalized. " +
                                 $"RocketMan doesn't modify the inners of this method. {statWorker.stat} {statWorker.stat?.defName ?? "null stat for worker"}", exception: error);
                    throw error;
                }
            }
            if (storeExists && RocketPrefs.Learning)
            {
                float t = RocketStates.StatExpiry[statWorker.stat.index];
                float T = tick - store.tick;
                float a = Mathf.Abs(value - store.value) / Mathf.Max(value, store.value, 1f);
                RocketStates.StatExpiry[statWorker.stat.index] = Mathf.Clamp(
                        t - Mathf.Clamp(RocketPrefs.LearningRate * (T * a - t), -0.1f, 0.25f),
                        0f, 1024f);
            }
            cache[key] = new CachedUnit(value, req.thingInt?.GetSignature() ?? -1);
            return value;
        }

        private static void Cleanup()
        {
            if (MAX_CACHE_SIZE < cache.Count) cache.Clear();
        }
    }
}