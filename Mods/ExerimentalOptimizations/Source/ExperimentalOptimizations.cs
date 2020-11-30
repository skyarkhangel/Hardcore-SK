using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace ExperimentalOptimizations
{
    [StaticConstructorOnStartup]
    public class ExperimentalOptimizations
    {
        private const int HealthTickInterval = 5;

        // Vanilla

        //public static void ImmunityChangePerTick(Pawn pawn, bool sick, Hediff diseaseInstance, ref float __result)
        //{
        //    __result *= HealthTickInterval;
        //}

        public static IEnumerable<CodeInstruction> CompensateReducedImmunityTick(IEnumerable<CodeInstruction> instructions)
        {
            var immunityChangePerTick = AccessTools.Method(typeof(ImmunityRecord), nameof(ImmunityRecord.ImmunityChangePerTick));
            bool ok = false;
            foreach (var ci in instructions)
            {
                yield return ci;
                if (ci.opcode == OpCodes.Call && ci.operand == immunityChangePerTick)
                {
                    // this.immunity += this.ImmunityChangePerTick(pawn, sick, diseaseInstance) * 5; // compensate reduced HealthTick => * 5
                    yield return new CodeInstruction(OpCodes.Ldc_R4, (float)HealthTickInterval);
                    yield return new CodeInstruction(OpCodes.Mul);
                    ok = true;
                }
            }
            if (!ok) Log.Error($"[ExperimentalOptimizations] call ImmunityChangePerTick not found!");
        }
		
        public static bool HealthTick(Pawn_HealthTracker __instance)
        {
            return __instance.pawn.IsHashIntervalTick(HealthTickInterval);
        }

        public static bool MindStateTick(Pawn_MindState __instance)
        {
            return __instance.pawn.IsHashIntervalTick(5);
        }

        // Alien Race
        private static readonly Dictionary<RaceProperties, ThingDef> RaceDb = new Dictionary<RaceProperties, ThingDef>();
        private static MethodInfo CanEatMethod;

        private static void CanEverEat(ref bool __result, RaceProperties __instance, ThingDef t)
        {
            if (!__instance.Humanlike) return;

            ThingDef race;
            if (!RaceDb.TryGetValue(__instance, out race))
            {
                race = DefDatabase<ThingDef>.AllDefsListForReading.ToList()
                    .Concat(U.DefDatabaseAllDefs("AlienRace.ThingDef_AlienRace").Cast<ThingDef>().ToList())
                    .First(td => td.race == __instance);

                RaceDb[__instance] = race;
            }
            
            __result = __result && (bool)CanEatMethod.Invoke(null, new object[] {t, race});
        }

        static ExperimentalOptimizations()
        {
            var h = new Harmony("Experimental.Optimizations");
            
            // Vanilla
            h.Patch(AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.HealthTick)),
                prefix: new HarmonyMethod(typeof(ExperimentalOptimizations), nameof(HealthTick)) {priority = 999});

            h.Patch(AccessTools.Method(typeof(ImmunityRecord), nameof(ImmunityRecord.ImmunityTick)),
                transpiler: new HarmonyMethod(typeof(ExperimentalOptimizations), nameof(CompensateReducedImmunityTick)));

            // problems with recruit prisoners: if (GenLocalDate.DayTick(this.pawn) == 0) { this.interactionsToday = 0; }
            //h.Patch(
            //    AccessTools.Method(typeof(Pawn_MindState), nameof(Pawn_MindState.MindStateTick)),
            //    prefix: new HarmonyMethod(typeof(ExperimentalOptimizations), nameof(MindStateTick)) {priority = 999});

            // AlienRace
            CanEatMethod = AccessTools.Method($"AlienRace.RaceRestrictionSettings:CanEat");

            if (CanEatMethod != null)
            {
                var canEverEat = AccessTools.Method(typeof(Verse.RaceProperties), "CanEverEat", new[] {typeof(ThingDef)});
                if (canEverEat != null)
                {
                    h.Unpatch(canEverEat, HarmonyPatchType.Postfix, "rimworld.erdelf.alien_race.main");
                    h.Patch(canEverEat, postfix: new HarmonyMethod(AccessTools.Method(typeof(ExperimentalOptimizations), nameof(CanEverEat))));
                    Log.Message($"[ExperimentalOptimizations] AlienRace:CanEverEat patch fixed");
                }
            }

            Log.Message($"[ExperimentalOptimizations] initialized");
        }
    }
}
