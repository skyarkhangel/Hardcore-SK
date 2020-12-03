using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace ExperimentalOptimizations.Optimizations
{
    public class Pawn_HealthTracker_Settings
    {
        public static bool Pawn_HealthTracker = true;
       
        public static bool Enabled() => Pawn_HealthTracker;

        public static void DoSettingsWindowContents(Listing_Standard l)
        {
            if (l.CheckBoxIsChanged("Pawn_HealthTracker".Translate(), ref Pawn_HealthTracker))
            {
                if (Pawn_HealthTracker) HealthTracker.Patch();
                else HealthTracker.UnPatch();
            }
        }

        public static void ExposeData()
        {
            Scribe_Values.Look(ref Pawn_HealthTracker, "Pawn_HealthTracker", true);
        }
    }

    [Optimization("Pawn_HealthTracker", typeof(Pawn_HealthTracker_Settings))]
    public class HealthTracker
    {
        private const int HealthTickInterval = 5;
        private static List<H.PatchInfo> Patches = new List<H.PatchInfo>();

        public static void Init()
        {
            var harmonyMethod = typeof(HealthTracker).Method(nameof(HealthTick)).ToHarmonyMethod(priority: 999);
            typeof(Pawn_HealthTracker).Method(nameof(Pawn_HealthTracker.HealthTick)).Patch(ref Patches, prefix: harmonyMethod, autoPatch: false);

            harmonyMethod = typeof(HealthTracker).Method(nameof(CompensateReducedImmunityTick)).ToHarmonyMethod(priority: 999);
            typeof(ImmunityRecord).Method(nameof(ImmunityRecord.ImmunityTick)).Patch(ref Patches, transpiler: harmonyMethod, autoPatch: false);

            // Fix hediffs
            typeof(Hediff).Method(nameof(Hediff.Tick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(Hediff_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(Hediff_Pregnant).Method(nameof(Hediff_Pregnant.Tick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(Hediff_Pregnant_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            
            // ref severityAdjustment меняется в самом хедиффе, не нужно еще раз х5 в HediffWithComps.PostTick
            // severityAdjustment используется в: HediffComp_SeverityPerDay, HediffComp_SelfHeal
            //typeof(HediffWithComps).Method(nameof(HediffWithComps.PostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffWithComps_PostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(Hediff_HeartAttack).Method(nameof(Hediff_HeartAttack.Tick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(Hediff_HeartAttack_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);

            typeof(HediffComp_ChanceToRemove).Method(nameof(HediffComp_ChanceToRemove.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_ChanceToRemove_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_ChangeNeed).Method(nameof(HediffComp_ChangeNeed.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_ChangeNeed_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_Disappears).Method(nameof(HediffComp_Disappears.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_Disappears_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_HealPermanentWounds).Method(nameof(HediffComp_HealPermanentWounds.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_HealPermanentWounds_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_Infecter).Method(nameof(HediffComp_Infecter.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_Infecter_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_SelfHeal).Method(nameof(HediffComp_SelfHeal.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_SelfHeal_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_SkillDecay).Method(nameof(HediffComp_SkillDecay.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_SkillDecay_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_TendDuration).Method(nameof(HediffComp_TendDuration.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_TendDuration_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_VerbGiver).Method(nameof(HediffComp_VerbGiver.CompPostTick)).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(HediffComp_VerbGiver_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);

            $"SK.Hediff_Senexium:Tick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(SK_Hediff_Senexium_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"SK.ShieldHediff:Tick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(Asari_SK_ShieldHediff_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"Adrenaline.Hediff_AdrenalineRush:UpdateSeverity".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(Adrenaline_Hediff_AdrenalineRush_UpdateSeverity_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"Adrenaline.Hediff_AdrenalineCrash:UpdateSeverity".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(Adrenaline_Hediff_AdrenalineCrash_UpdateSeverity_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_PartBaseArtifical:Tick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(rjw_Hediff_PartBaseArtifical_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_InsectEgg:Tick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(rjw_Hediff_InsectEgg_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_PartBaseNatural:Tick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(rjw_Hediff_PartBaseNatural_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_BasePregnancy:Tick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(rjw_Hediff_BasePregnancy_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);

            $"CombatExtended.HediffComp_Venom:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(CombatExtended_HediffComp_Venom_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"CombatExtended.HediffComp_InfecterCE:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(CombatExtended_HediffComp_InfecterCE_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"CombatExtended.HediffComp_Stabilize:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(CombatExtended_HediffComp_Stabilize_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"SK.HeddifComp_StandOff:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(SK_HeddifComp_StandOff_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"SK.HeddifComp_Traitor:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: typeof(HealthTracker).Method(nameof(SK_HeddifComp_Traitor_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
        }

        public static void Patch()
        {
            foreach (var patch in Patches) patch.Enable();
            Log.Message($"[ExperimentalOptimizations] PatchHealthTick done");
        }

        public static void UnPatch()
        {
            foreach (var patch in Patches) patch.Disable();
            Log.Message($"[ExperimentalOptimizations] UnPatchHealthTick done");
        }

        private static IEnumerable<CodeInstruction> CompensateReducedImmunityTick(IEnumerable<CodeInstruction> instructions)
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
		
        private static bool HealthTick(Pawn_HealthTracker __instance)
        {
            return __instance.pawn.IsHashIntervalTick(HealthTickInterval);
        }

        /////////////////////////////
        /// FIX HEDIFFS
        
        /* HOW?
        void HediffComp_Discoverable::CompPostTick(ref float severityAdjustment)
        {
	        if (Find.TickManager.TicksGame % 103 == 0)
	        {
		        this.CheckDiscovered();
	        }
        }

        public override void HediffComp_VerbGiver::CompPostTick(ref float severityAdjustment)
        {
	        base.CompPostTick(ref severityAdjustment);
	        this.verbTracker.VerbsTick();
        }

        // Norm ? ageTicks += 5(patched) in Hediff.Tick
        public override void CombatExtended_HediffComp_Stabilize_CompPostTick_Transpiler(ref float severityAdjustment)
		{
			if (this.stabilized && this.bleedModifier < 1f && this.parent.ageTicks % 60 == 0)
			{
				this.bleedModifier += 0.01f;
				if (this.bleedModifier >= 1f)
				{
					this.bleedModifier = 1f;
					return;
				}
			}
			else if (!this.stabilized && this.parent.pawn.Downed)
			{
				LessonAutoActivator.TeachOpportunity(CE_ConceptDefOf.CE_Stabilizing, this.parent.pawn, OpportunityType.Important);
			}
		}

        // Norm !
        public override void SK_HeddifComp_StandOff_CompPostTick_Transpiler(ref float severityAdjustment)
		{
			if (this.t == 0)
			{
				this.pawnl.Add(base.Pawn);
				base.Pawn.SetFaction(Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined), null);
				LordJob_VisitColony lordJob = new LordJob_VisitColony();
				LordMaker.MakeNewLord(base.Pawn.Faction, lordJob, base.Pawn.Map, null);
			}
			if (this.t % 100 == 0 && (GenDate.DayTick((long)GenTicks.TicksAbs, Find.WorldGrid.LongLatOf(base.Pawn.Map.Tile).x) > 29500 && GenDate.DayTick((long)GenTicks.TicksAbs, Find.WorldGrid.LongLatOf(base.Pawn.Map.Tile).x) < 30500 && this.flag))
			{
				Job job = JobMaker.MakeJob(JobDefOf.Wait_Wander);
				base.Pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				base.Pawn.jobs.StopAll(false, true);
				LordJob_DefendPoint lordJob2 = new LordJob_DefendPoint(base.Pawn.Position, null, false, true);
				LordMaker.MakeNewLord(base.Pawn.Faction, lordJob2, base.Pawn.Map, null);
				this.flag = false;
			}
			this.t++;
		}
        // Norm !
        public override void SK_HeddifComp_Traitor_CompPostTick_Transpiler(ref float severityAdjustment)
		{
			if (this.ticksToDisappear == 20)
			{
				base.Pawn.SetFaction(Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Undefined), null);
				string str = "TraitorLabel".Translate();
				string str2 = "Traitor".Translate(base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn, "PAWN", true);
				Find.LetterStack.ReceiveLetter(str, str2, LetterDefOf.ThreatBig, new TargetInfo(base.Pawn.Position, base.Pawn.Map, false), null, null, null, null);
				this.ticksToDisappear = 0;
				List<Pawn> list = new List<Pawn>();
				list.Add(base.Pawn);
				LordJob_DefendPoint lordJob = new LordJob_DefendPoint(base.Pawn.Position, null, false, true);
				LordMaker.MakeNewLord(base.Pawn.Faction, lordJob, base.Pawn.Map, list);
			}
			this.ticksToDisappear--;
		}
         */

        static IEnumerable<CodeInstruction> Hediff_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            if (code[3].opcode != OpCodes.Ldc_I4_1)
            {
                Log.Warning($"Hediff_Tick_Transpiler failed!");
                return code;
            }

            // original: this.ageTicks++;
            code[3].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> Hediff_Pregnant_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            if (code[3].opcode != OpCodes.Ldc_I4_1)
            {
                Log.Warning($"Hediff_Pregnant_Tick_Transpiler failed!");
                return code;
            }

            // original: this.ageTicks++;
            code[3].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> HediffWithComps_PostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var get_Severity = typeof(Hediff).Method("get_Severity");

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldloc_0 && code[i - 1].opcode == OpCodes.Callvirt && code[i - 1].operand == get_Severity)
                {
                    idx = i + 1;
                }
            }

            if (idx == -1)
            {
                Log.Warning($"HediffWithComps_PostTick_Transpiler failed!");
                return code;
            }

            // original: this.Severity += num;
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });
            return code;
        }

        static IEnumerable<CodeInstruction> Hediff_HeartAttack_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var range = typeof(Verse.Rand).Method(nameof(Rand.Range), new []{typeof(float), typeof(float)});

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Call && code[i].operand == range)
                {
                    idx = i + 1;
                }
            }

            if (idx == -1)
            {
                Log.Warning($"Hediff_HeartAttack_Tick_Transpiler failed!");
                return code;
            }

            // original: this.Severity += Rand.Range(-0.4f, 0.6f);
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });
            return code;
        }

        static IEnumerable<CodeInstruction> SK_Hediff_Senexium_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var last = code.Count - 1;
            int idx = -1;
            if (code[last].opcode == OpCodes.Ret && code[last - 1].opcode == OpCodes.Callvirt &&
                code[last - 2].opcode == OpCodes.Add && code[last - 3].opcode == OpCodes.Div)
                idx = last - 2; // after div
            
            if (idx == -1)
            {
                Log.Warning($"SK_Hediff_Senexium_Tick_Transpiler failed!");
                return code;
            }

            // original: hediff_Pregnant.Severity += (this.Severity - 1f) / (this.pawn.RaceProps.gestationPeriodDays * 60000f);
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });
            return code;
        }

        static IEnumerable<CodeInstruction> Asari_SK_ShieldHediff_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var ShieldHediff = AccessTools.TypeByName("SK.ShieldHediff");
            var cooldownTicks = AccessTools.Field(ShieldHediff, "cooldownTicks");
            var shieldCurrent = AccessTools.Field(ShieldHediff, "shieldCurrent");

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == cooldownTicks && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"Asari_SK_ShieldHediff_Tick_Transpiler failed1!");
                return code;
            }

            // original: this.cooldownTicks--;
            code[idx].opcode = OpCodes.Ldc_I4_5;

            ///////////////////
            
            idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == 60f && code[i + 1].opcode == OpCodes.Div)
                {
                    idx = i + 2;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"Asari_SK_ShieldHediff_Tick_Transpiler failed2!");
                return code;
            }

            // original: this.shieldCurrent -= this.shieldDecayPerSec / 60f;
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });

            return code;
        }

        //////////////
        /// COMPS
        static IEnumerable<CodeInstruction> HediffComp_ChanceToRemove_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var currentInterval = AccessTools.Field(typeof(HediffComp_ChanceToRemove), nameof(HediffComp_ChanceToRemove.currentInterval));

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == currentInterval && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_ChanceToRemove_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: this.currentInterval--;
            code[idx].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_ChangeNeed_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == 60000f && code[i + 1].opcode == OpCodes.Div)
                {
                    idx = i + 2;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_ChangeNeed_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: this.Need.CurLevelPercentage += this.Props.percentPerDay / 60000f;
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_Disappears_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var ticksToDisappear = AccessTools.Field(typeof(HediffComp_Disappears), nameof(HediffComp_Disappears.ticksToDisappear));

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == ticksToDisappear && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_Disappears_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: this.ticksToDisappear--;
            code[idx].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_HealPermanentWounds_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var ticksToHeal = AccessTools.Field(typeof(HediffComp_HealPermanentWounds), nameof(HediffComp_HealPermanentWounds.ticksToHeal));

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == ticksToHeal && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_HealPermanentWounds_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: this.ticksToHeal--;
            code[idx].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_Infecter_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var ticksUntilInfect = AccessTools.Field(typeof(HediffComp_Infecter), nameof(HediffComp_Infecter.ticksUntilInfect));

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == ticksUntilInfect && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_Infecter_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: this.ticksUntilInfect--;
            code[idx].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_SelfHeal_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            /*
            public override void HediffComp_SelfHeal::CompPostTick(ref float severityAdjustment)
		    {
			    this.ticksSinceHeal++;
			    if (this.ticksSinceHeal > this.Props.healIntervalTicksStanding)
			    {
				    severityAdjustment -= this.Props.healAmount;
				    this.ticksSinceHeal = 0;
			    }
		    }
             */
            var ticksSinceHeal = AccessTools.Field(typeof(HediffComp_SelfHeal), nameof(HediffComp_SelfHeal.ticksSinceHeal));

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == ticksSinceHeal && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_SelfHeal_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: this.ticksSinceHeal++;
            code[idx].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_SkillDecay_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == 60000f && code[i + 1].opcode == OpCodes.Div)
                {
                    idx = i + 2;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_SkillDecay_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: float num2 = skillRecord.XpRequiredForLevelUp * num / 60000f;
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_TendDuration_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var tendTicksLeft = AccessTools.Field(typeof(HediffComp_TendDuration), nameof(HediffComp_TendDuration.tendTicksLeft));

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == tendTicksLeft && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_TendDuration_CompPostTick failed!");
                return code;
            }

            // original: this.tendTicksLeft--;
            code[idx].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> CombatExtended_HediffComp_Venom_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var HediffComp_Venom = AccessTools.TypeByName("CombatExtended.HediffComp_Venom");
            var _venomPerTick = AccessTools.Field(HediffComp_Venom, "_venomPerTick");

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == _venomPerTick)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"CombatExtended_HediffComp_Venom_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: HealthUtility.AdjustSeverity(this.parent.pawn, CE_HediffDefOf.VenomBuildup, this._venomPerTick);
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });
            return code;
        }

        static IEnumerable<CodeInstruction> CombatExtended_HediffComp_InfecterCE_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var HediffComp_InfecterCE = AccessTools.TypeByName("CombatExtended.HediffComp_InfecterCE");
            var _ticksTended = AccessTools.Field(HediffComp_InfecterCE, "_ticksTended");
            var _ticksUntilInfect = AccessTools.Field(HediffComp_InfecterCE, "_ticksUntilInfect");

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == _ticksTended && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"CombatExtended_HediffComp_InfecterCE_CompPostTick_Transpiler failed 1!");
                return code;
            }

            // original: this._ticksTended++;
            code[idx].opcode = OpCodes.Ldc_I4_5;

            ///////////////
            
            idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == _ticksUntilInfect && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"CombatExtended_HediffComp_InfecterCE_CompPostTick_Transpiler failed 2!");
                return code;
            }

            // original: this._ticksUntilInfect--;
            code[idx].opcode = OpCodes.Ldc_I4_5;

            return code;
        }

        static IEnumerable<CodeInstruction> CombatExtended_HediffComp_Stabilize_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var HediffComp_Stabilize = AccessTools.TypeByName("CombatExtended.HediffComp_Stabilize");
            var bleedModifier = AccessTools.Field(HediffComp_Stabilize, "bleedModifier");

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == bleedModifier && code[i + 1].opcode == OpCodes.Ldc_R4 && (float)code[i + 1].operand == 0.01f)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"CombatExtended_HediffComp_Stabilize_CompPostTick_Transpiler failed 1!");
                return code;
            }

            // original: this.bleedModifier += 0.01f;
            code[idx].operand = 0.05f;

            // TODO: handle => if (this.stabilized && this.bleedModifier < 1f && this.parent.ageTicks % 60 == 0)

            return code;
        }

        static IEnumerable<CodeInstruction> SK_HeddifComp_StandOff_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var HeddifComp_StandOff = AccessTools.TypeByName("SK.HeddifComp_StandOff");
            var t = AccessTools.Field(HeddifComp_StandOff, "t");

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == t && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"SK_HeddifComp_StandOff_CompPostTick_Transpiler failed 1!");
                return code;
            }

            // original: this.t++;
            code[idx].opcode = OpCodes.Ldc_I4_5;

            // TODO: handle => if (this.t % 100 == 0 && (GenDate.D......

            return code;
        }

        static IEnumerable<CodeInstruction> SK_HeddifComp_Traitor_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var HeddifComp_Traitor = AccessTools.TypeByName("SK.HeddifComp_Traitor");
            var ticksToDisappear = AccessTools.Field(HeddifComp_Traitor, "ticksToDisappear");

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == ticksToDisappear && code[i + 1].opcode == OpCodes.Ldc_I4_1)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"SK_HeddifComp_Traitor_CompPostTick_Transpiler failed!");
                return code;
            }

            // original: this.ticksToDisappear--;
            code[idx].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> HediffComp_VerbGiver_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            var VerbsTick = typeof(VerbTracker).Method(nameof(VerbTracker.VerbsTick));
            var verbTracker = AccessTools.Field(typeof(HediffComp_VerbGiver), nameof(HediffComp_VerbGiver.verbTracker));

            var code = instructions.ToList();
            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldarg_0 && code[i + 1].opcode == OpCodes.Ldfld && code[i + 1].operand == verbTracker && code[i + 2].opcode == OpCodes.Callvirt && code[i + 2].operand == VerbsTick)
                {
                    idx = i;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"HediffComp_VerbGiver_CompPostTick_Transpiler failed!");
                return code;
            }

            var loopIndex = ilGen.DeclareLocal(typeof(int));
            var loopStartLabel = ilGen.DefineLabel();
            var compareBelow5Label = ilGen.DefineLabel();

            code[idx].labels.Add(loopStartLabel);
            // call this.verbTracker.VerbsTick();
            code.InsertRange(idx + 3, new [] // after call VerbsTick
            {
                new CodeInstruction(OpCodes.Ldloc, loopIndex), 
                new CodeInstruction(OpCodes.Ldc_I4_1), 
                new CodeInstruction(OpCodes.Add), 
                new CodeInstruction(OpCodes.Stloc, loopIndex), 
                new CodeInstruction(OpCodes.Ldloc, loopIndex).WithLabels(compareBelow5Label), 
                new CodeInstruction(OpCodes.Ldc_I4_5), 
                new CodeInstruction(OpCodes.Blt_S, loopStartLabel), 
            });
            code.Insert(idx, new CodeInstruction(OpCodes.Br_S, compareBelow5Label));
            
            //foreach (var c in code) Log.Warning(c.ToString());

            return code;
        }

        static IEnumerable<CodeInstruction> Adrenaline_Hediff_AdrenalineRush_UpdateSeverity_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var newCode = new List<CodeInstruction>();
            var code = instructions.ToList();
            int changes = 3;
            for (int i = 0; i < code.Count; i++)
            {
                newCode.Add(code[i]);
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == 20f && code[i + 1].opcode == OpCodes.Mul/* && code[i - 1].opcode == OpCodes.Div*/)
                {
                    newCode.AddRange(new []
                    {
                        new CodeInstruction(OpCodes.Ldc_R4, Pawn_NeedsTracker_Settings.Pawn_NeedsTracker_Interval / 150f),
                        new CodeInstruction(OpCodes.Mul),
                    });
                    changes--;
                }
            }

            if (changes != 0)
            {
                Log.Warning($"Adrenaline_Hediff_AdrenalineRush_UpdateSeverity_Transpiler failed!");
                return code;
            }

            return newCode;
        }

        static IEnumerable<CodeInstruction> Adrenaline_Hediff_AdrenalineCrash_UpdateSeverity_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var newCode = new List<CodeInstruction>();
            var code = instructions.ToList();
            int changes = 3;
            for (int i = 0; i < code.Count; i++)
            {
                newCode.Add(code[i]);
                if (code[i].opcode == OpCodes.Ldc_R4 && (float)code[i].operand == 20f && code[i + 1].opcode == OpCodes.Mul/* && code[i - 1].opcode == OpCodes.Div*/)
                {
                    newCode.AddRange(new []
                    {
                        new CodeInstruction(OpCodes.Ldc_R4, Pawn_NeedsTracker_Settings.Pawn_NeedsTracker_Interval / 150f),
                        new CodeInstruction(OpCodes.Mul),
                    });
                    changes--;
                }
            }

            if (changes != 0)
            {
                Log.Warning($"Adrenaline_Hediff_AdrenalineCrash_UpdateSeverity_Transpiler failed!");
                return code;
            }

            return newCode;
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_PartBaseArtifical_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            if (code[3].opcode != OpCodes.Ldc_I4_1)
            {
                Log.Warning($"rjw_Hediff_PartBaseArtifical_Tick_Transpiler failed!");
                return code;
            }

            // original: this.ageTicks++;
            code[3].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_InsectEgg_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            if (code[3].opcode != OpCodes.Ldc_I4_1)
            {
                Log.Warning($"rjw_Hediff_InsectEgg_Tick_Transpiler failed!");
                return code;
            }

            // original: this.ageTicks++;
            code[3].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_PartBaseNatural_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            if (code[3].opcode != OpCodes.Ldc_I4_1)
            {
                Log.Warning($"rjw_Hediff_PartBaseNatural_Tick_Transpiler failed!");
                return code;
            }

            // original: this.ageTicks++;
            code[3].opcode = OpCodes.Ldc_I4_5;
            return code;
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_BasePregnancy_Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            if (code[3].opcode != OpCodes.Ldc_I4_1)
            {
                Log.Warning($"rjw_Hediff_BasePregnancy_Tick_Transpiler failed 1!");
                return code;
            }

            // original: this.ageTicks++;
            code[3].opcode = OpCodes.Ldc_I4_5;

            ////
            
            var Hediff_BasePregnancy = AccessTools.TypeByName("rjw.Hediff_BasePregnancy");
            var progress_per_tick = AccessTools.Field(Hediff_BasePregnancy, "progress_per_tick");

            int idx = -1;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i].operand == progress_per_tick && code[i + 1].opcode == OpCodes.Add)
                {
                    idx = i + 1;
                }
            }
            
            if (idx == -1)
            {
                Log.Warning($"rjw_Hediff_BasePregnancy_Tick_Transpiler failed 2!");
                return code;
            }

            // original: this.GestationProgress += this.progress_per_tick;
            code.InsertRange(idx, new []
            {
                new CodeInstruction(OpCodes.Ldc_R4, 5f),
                new CodeInstruction(OpCodes.Mul),
            });
            return code;
        }
    }
}