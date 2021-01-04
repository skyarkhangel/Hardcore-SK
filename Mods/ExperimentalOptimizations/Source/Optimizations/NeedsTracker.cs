using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ExperimentalOptimizations.Optimizations
{
    public class Pawn_NeedsTracker_Settings
    {
        private static string DebugNeedsName = "pawn_name";
        private static bool DebugNeeds = false;

        public static bool Pawn_NeedsTracker = true;
        public static int Pawn_NeedsTracker_Interval = 200; // vanilla value 150

        private static bool _intervalChanged = false;
        private static DateTime _intervalChangedTime = DateTime.Now;

        public static bool Enabled() => Pawn_NeedsTracker;

        public static void DoSettingsWindowContents(Listing_Standard l)
        {
            if (l.CheckBoxIsChanged("Pawn_NeedsTracker".Translate(), ref Pawn_NeedsTracker))
            {
                if (Pawn_NeedsTracker) NeedsTracker.Patch();
                else NeedsTracker.UnPatch();
                _intervalChanged = false;
            }
            if (l.TextFieldNumericChanged("Pawn_NeedsTracker_Interval".Translate(), ref Pawn_NeedsTracker_Interval, 150, 1000))
            {
                _intervalChanged = true;
                _intervalChangedTime = DateTime.Now;
            }

            if (_intervalChanged && (DateTime.Now - _intervalChangedTime).TotalMilliseconds > 250)
            {
                if (Pawn_NeedsTracker)
                {
                    NeedsTracker.UnPatch();
                    NeedsTracker.Patch();
                }
                _intervalChanged = false;
            }

            l.GapLine();

            DebugNeedsName = l.TextEntryLabeled("DEBUG: Dump pawn needs(pawn name contain)", DebugNeedsName);
            if (l.CheckBoxIsChanged("DEBUG: Dump needs", ref DebugNeeds))
            {
                if (DebugNeeds) NeedDebugger.Start(DebugNeedsName);
                else NeedDebugger.Stop();
            }
        }

        public static void ExposeData()
        {
            Scribe_Values.Look(ref Pawn_NeedsTracker, "Pawn_NeedsTracker", true);
            Scribe_Values.Look(ref Pawn_NeedsTracker_Interval, "Pawn_NeedsTracker_Interval", 350);
        }
    }

    public static class NeedDebugger
    {
        public class NeedRecord
        {
            public int Tick;
            public float Level;
        }

        private static H.PatchInfo _patch;
        private static Dictionary<Need, NeedRecord> _needRecords = new Dictionary<Need, NeedRecord>();
        private static string _pawnName;

        private static void PawnPostfix(Pawn __instance)
        {
            if (String.IsNullOrWhiteSpace(_pawnName) || !__instance.LabelCap.Contains(_pawnName) || __instance.needs == null)
                return;

            foreach (var need in __instance.needs.AllNeeds)
            {
                if (!_needRecords.TryGetValue(need, out var record))
                {
                    record = new NeedRecord
                    {
                        Tick = Find.TickManager.TicksGame + 1060,
                        Level = need.CurLevel
                    };
                    _needRecords.Add(need, record);
                }
                else if (Find.TickManager.TicksGame >= record.Tick)
                {
                    Log.Warning($"{need.LabelCap}: {need.CurLevel - record.Level}");
                    record.Tick = Find.TickManager.TicksGame + 1060;
                    record.Level = need.CurLevel;
                }
            }
        }

        public static void Start(string pawnNameContain)
        {
            _pawnName = pawnNameContain;
            _patch = typeof(Pawn).Method(nameof(Pawn.Tick)).Patch(postfix: typeof(NeedDebugger).Method(nameof(PawnPostfix)).ToHarmonyMethod(-9999), autoPatch: true);
            Log.Message($"[NeedDebugger] Enabled");
        }

        public static void Stop()
        {
            _patch?.Disable();
            _needRecords.Clear();
            Log.Message($"[NeedDebugger] Disabled");
        }
    }

    [Optimization("Pawn_NeedsTracker", typeof(Pawn_NeedsTracker_Settings))]
    public class NeedsTracker
    {
        private static List<H.PatchInfo> Patches = new List<H.PatchInfo>();
        private static float CompensateMult => Pawn_NeedsTracker_Settings.Pawn_NeedsTracker_Interval / 150f;

        public static void Init()
        {
            var nt = typeof(NeedsTracker);
            var trans = nt.Method(nameof(NeedsTrackerTick_Transpiler)).ToHarmonyMethod(priority: 999);

            typeof(Pawn_NeedsTracker).Method(nameof(Pawn_NeedsTracker.NeedsTrackerTick)).Patch(ref Patches, transpiler: trans, autoPatch: false);
            
            // joy
            typeof(Need_Joy).Method(nameof(Need_Joy.NeedInterval)).Patch(ref Patches, transpiler: nt.Method(nameof(RimWorld_Need_Joy_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            typeof(JoyToleranceSet).Method(nameof(JoyToleranceSet.NeedInterval)).Patch(ref Patches, transpiler: trans, autoPatch: false);
            // beauty, comfort, mood, roomsize
            typeof(Need_Seeker).Method(nameof(Need_Seeker.NeedInterval)).Patch(ref Patches, transpiler: nt.Method(nameof(RimWorld_Need_Seeker_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            {
                // mood fix
                typeof(Thought_Memory).Method(nameof(Thought_Memory.ThoughtInterval)).Patch(ref Patches, transpiler: trans, autoPatch: false);
                typeof(PawnObserver).Method(nameof(PawnObserver.ObserverInterval)).Patch(ref Patches, transpiler: nt.Method(nameof(RimWorld_PawnObserver_ObserverInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            }
            // chemical
            typeof(Need_Chemical).Method(nameof(Need_Chemical.NeedInterval)).Patch(ref Patches, transpiler: trans, autoPatch: false);
            typeof(Need_Chemical_Any).Method(nameof(Need_Chemical_Any.NeedInterval)).Patch(ref Patches, transpiler: nt.Method(nameof(RimWorld_Need_Chemical_Any_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            // food
            typeof(Need_Food).Method(nameof(Need_Food.NeedInterval)).Patch(ref Patches, transpiler: trans, autoPatch: false);
            // outdoors
            typeof(Need_Outdoors).Method(nameof(Need_Outdoors.NeedInterval)).Patch(ref Patches, transpiler: nt.Method(nameof(RimWorld_Need_Outdoors_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            // rest
            typeof(Need_Rest).Method(nameof(Need_Rest.NeedInterval)).Patch(ref Patches, transpiler: trans, autoPatch: false);
            typeof(Need_Rest).Method(nameof(Need_Rest.NeedInterval)).Patch(ref Patches, transpiler: nt.Method(nameof(RimWorld_Need_Rest_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);

            // Dubs Hygiene
            {
                "DubsBadHygiene.Need_Bladder:NeedInterval".Method(warn: false).Patch(ref Patches, transpiler: trans, autoPatch: false);
                "DubsBadHygiene.Need_Hygiene:NeedInterval".Method(warn: false).Patch(ref Patches, transpiler: trans, autoPatch: false);
                "DubsBadHygiene.Need_Thirst:NeedInterval".Method(warn: false).Patch(ref Patches, transpiler: trans, autoPatch: false);
            }
            // Skynet
            {
                "Skynet.Need_Energy:NeedInterval".Method(warn: false).Patch(ref Patches, transpiler: nt.Method(nameof(Skynet_Need_Energy_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            }
            // Androids
            {
                "Androids.Need_Energy:NeedInterval".Method(warn: false).Patch(ref Patches, transpiler: nt.Method(nameof(Androids_Need_Energy_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            }
            // rjw
            {
                "rjw.Need_Sex:NeedInterval".Method(warn: false).Patch(ref Patches, transpiler: nt.Method(nameof(rjw_Need_Sex_NeedInterval_Transpiler)).ToHarmonyMethod(), autoPatch: false);
            }
        }

        public static void Patch()
        {
            foreach (var patch in Patches) patch.Enable();
            Log.Message($"[ExperimentalOptimizations] PatchNeedsTrackerTick done");
        }

        public static void UnPatch()
        {
            foreach (var patch in Patches) patch.Disable();
            Log.Message($"[ExperimentalOptimizations] UnPatchNeedsTrackerTick done");
        }

        private static IEnumerable<CodeInstruction> NeedsTrackerTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool ok = false;
            foreach (var ci in instructions)
            {
                if (ci.opcode == OpCodes.Ldc_I4 && (int)ci.operand == 150)
                {
                    ci.operand = Pawn_NeedsTracker_Settings.Pawn_NeedsTracker_Interval;
                    ok = true;
                }
                else if (ci.opcode == OpCodes.Ldc_R4 && (float)ci.operand == 150)
                {
                    ci.operand = (float)Pawn_NeedsTracker_Settings.Pawn_NeedsTracker_Interval;
                    ok = true;
                }
                yield return ci;
            }
            if (!ok) Log.Error("[Transpiler] Ldc_I4 or Ldc_R4 not found!");
        }

        static IEnumerable<CodeInstruction> RimWorld_Need_Joy_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Need_Joy.NeedInterval")
                .Replace("call RimWorld.Need_Joy:get_FallPerInterval;sub", $"call RimWorld.Need_Joy:get_FallPerInterval;ldc.r4 {CompensateMult};mul;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> RimWorld_Need_Seeker_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Need_Seeker.NeedInterval")
                .Replace("ldc.r4 0.06;mul", $"ldc.r4 0.06;mul;ldc.r4 {CompensateMult};mul", 2)
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> RimWorld_PawnObserver_ObserverInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("PawnObserver.ObserverInterval")
                .Replace("ldfld RimWorld.PawnObserver:intervalsUntilObserve;ldc.i4.1;sub", $"ldfld RimWorld.PawnObserver:intervalsUntilObserve;ldc.r4 {CompensateMult};conv.i4;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> RimWorld_Need_Chemical_Any_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Need_Chemical_Any.NeedInterval")
                .Replace("call RimWorld.Need_Chemical_Any:get_FallPerNeedIntervalTick;sub", $"call RimWorld.Need_Chemical_Any:get_FallPerNeedIntervalTick;ldc.r4 {CompensateMult};mul;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> RimWorld_Need_Outdoors_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Need_Outdoors.NeedInterval")
                .Search("ldc.r4 0.0025;mul")
                .Insert($"ldc.r4 {CompensateMult};mul")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> RimWorld_Need_Rest_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Need_Rest.NeedInterval")
                .Search("ldc.r4 0.005714286")
                .Insert($"ldc.r4 {CompensateMult};mul")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> Skynet_Need_Energy_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Skynet.Need_Energy.NeedInterval")
                .Replace("call Skynet.Need_Energy:get_Drain;sub", $"call Skynet.Need_Energy:get_Drain;ldc.r4 {CompensateMult};mul;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> Androids_Need_Energy_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Androids.Need_Energy.NeedInterval")
                .Search("ldc.r4 0.000833333354;mul")
                .Insert($"ldc.r4 {CompensateMult};mul")
                .Search("ldc.r4 0.0133333337;add")
                .Insert($"ldc.r4 {CompensateMult};mul")
                .Transpiler(ilGen, instructions);
        }
        
        static IEnumerable<CodeInstruction> rjw_Need_Sex_NeedInterval_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("rjw.Need_Sex.NeedInterval")
                .Search("ldsfld rjw.RJWSettings:sexneed_decay_rate;mul")
                .Insert($"ldc.r4 {CompensateMult};mul")
                .Transpiler(ilGen, instructions);
        }
    }
}