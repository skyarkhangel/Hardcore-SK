using System.Collections.Generic;
using System.IO;
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
            var ht = typeof(HealthTracker);
            var harmonyMethod = ht.Method(nameof(HealthTick)).ToHarmonyMethod(priority: 999);
            typeof(Pawn_HealthTracker).Method(nameof(Pawn_HealthTracker.HealthTick)).Patch(ref Patches, prefix: harmonyMethod, autoPatch: false);
			
			harmonyMethod = ht.Method(nameof(CompensateReducedImmunityTick)).ToHarmonyMethod(priority: 999);
            typeof(ImmunityRecord).Method(nameof(ImmunityRecord.ImmunityTick)).Patch(ref Patches, transpiler: harmonyMethod, autoPatch: false);

            // Fix hediffs
            typeof(Hediff).Method(nameof(Hediff.Tick)).Patch(ref Patches, transpiler: ht.Method(nameof(Hediff_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(Hediff_Pregnant).Method(nameof(Hediff_Pregnant.Tick)).Patch(ref Patches, transpiler: ht.Method(nameof(Hediff_Pregnant_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            
            // ref severityAdjustment меняется в самом хедиффе, не нужно еще раз х5 в HediffWithComps.PostTick
            // severityAdjustment используется в: HediffComp_SeverityPerDay, HediffComp_SelfHeal
            //typeof(HediffWithComps).Method(nameof(HediffWithComps.PostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffWithComps_PostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(Hediff_HeartAttack).Method(nameof(Hediff_HeartAttack.Tick)).Patch(ref Patches, transpiler: ht.Method(nameof(Hediff_HeartAttack_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);

            typeof(HediffComp_ChanceToRemove).Method(nameof(HediffComp_ChanceToRemove.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_ChanceToRemove_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_ChangeNeed).Method(nameof(HediffComp_ChangeNeed.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_ChangeNeed_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_Disappears).Method(nameof(HediffComp_Disappears.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_Disappears_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_HealPermanentWounds).Method(nameof(HediffComp_HealPermanentWounds.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_HealPermanentWounds_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_Infecter).Method(nameof(HediffComp_Infecter.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_Infecter_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_SelfHeal).Method(nameof(HediffComp_SelfHeal.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_SelfHeal_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_SkillDecay).Method(nameof(HediffComp_SkillDecay.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_SkillDecay_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_TendDuration).Method(nameof(HediffComp_TendDuration.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_TendDuration_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            typeof(HediffComp_VerbGiver).Method(nameof(HediffComp_VerbGiver.CompPostTick)).Patch(ref Patches, transpiler: ht.Method(nameof(HediffComp_VerbGiver_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);

            $"SK.Hediff_Senexium:Tick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(SK_Hediff_Senexium_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"SK.ShieldHediff:Tick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(Asari_SK_ShieldHediff_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"Adrenaline.Hediff_AdrenalineRush:UpdateSeverity".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(Adrenaline_Hediff_AdrenalineRush_UpdateSeverity_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"Adrenaline.Hediff_AdrenalineCrash:UpdateSeverity".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(Adrenaline_Hediff_AdrenalineCrash_UpdateSeverity_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_PartBaseArtifical:Tick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(rjw_Hediff_PartBaseArtifical_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_InsectEgg:Tick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(rjw_Hediff_InsectEgg_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_PartBaseNatural:Tick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(rjw_Hediff_PartBaseNatural_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"rjw.Hediff_BasePregnancy:Tick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(rjw_Hediff_BasePregnancy_Tick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);

            $"CombatExtended.HediffComp_Venom:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(CombatExtended_HediffComp_Venom_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"CombatExtended.HediffComp_InfecterCE:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(CombatExtended_HediffComp_InfecterCE_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"CombatExtended.HediffComp_Stabilize:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(CombatExtended_HediffComp_Stabilize_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"SK.HeddifComp_StandOff:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(SK_HeddifComp_StandOff_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
            $"SK.HeddifComp_Traitor:CompPostTick".Method(warn: false).Patch(ref Patches, transpiler: ht.Method(nameof(SK_HeddifComp_Traitor_CompPostTick_Transpiler)).ToHarmonyMethod(priority: 999), autoPatch: false);
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

        private static bool HealthTick(Pawn_HealthTracker __instance)
        {
            return __instance.pawn.IsHashIntervalTick(HealthTickInterval);
        }

        private static IEnumerable<CodeInstruction> CompensateReducedImmunityTick(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("ImmunityRecord.ImmunityTick")
                .Search("call Verse.ImmunityRecord:ImmunityChangePerTick(Verse.Pawn,bool,Verse.Hediff)")
                .Insert("ldc.r4 5;mul")
                .Transpiler(ilGen, instructions);
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

        static IEnumerable<CodeInstruction> Hediff_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Hediff.Tick")
                .Replace("ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.1;add", "ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.5;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> Hediff_Pregnant_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Hediff_Pregnant.Tick")
                .Replace("ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.1;add", "ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.5;add")
                .Search("ldfld Verse.RaceProperties:gestationPeriodDays;ldc.r4 60000;mul;div")
                .Insert("ldc.r4 5;mul")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffWithComps_PostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffWithComps.PostTick")
                .Search("callvirt Verse.Hediff:get_Severity;ldloc.0")
                .Insert("ldc.r4 5;mul")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> Hediff_HeartAttack_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Hediff_HeartAttack.Tick")
                .Search("call Verse.Rand:Range(float,float)")
                .Insert("ldc.r4 5;mul")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> SK_Hediff_Senexium_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("SK.Hediff_Senexium.Tick")
                .Search("ldfld Verse.RaceProperties:gestationPeriodDays;ldc.r4 60000;mul;div")
                .Insert("ldc.r4 5;mul")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> Asari_SK_ShieldHediff_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("SK.ShieldHediff.Tick")
                .Replace("ldfld SK.ShieldHediff:cooldownTicks;ldc.i4.1;sub", "ldfld SK.ShieldHediff:cooldownTicks;ldc.i4.5;sub")
                .Replace("ldfld SK.ShieldHediff:shieldDecayPerSec;ldc.r4 60;div;sub", "ldfld SK.ShieldHediff:shieldDecayPerSec;ldc.r4 60;div;ldc.r4 5;mul;sub")
                .Transpiler(ilGen, instructions);
        }

        //////////////
        /// COMPS
        static IEnumerable<CodeInstruction> HediffComp_ChanceToRemove_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffComp_ChanceToRemove.CompPostTick")
                .Replace("ldfld Verse.HediffComp_ChanceToRemove:currentInterval;ldc.i4.1;sub", "ldfld Verse.HediffComp_ChanceToRemove:currentInterval;ldc.i4.5;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_ChangeNeed_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffComp_ChangeNeed.CompPostTick")
                .Replace("ldc.r4 60000;div;add", "ldc.r4 60000;div;ldc.r4 5;mul;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_Disappears_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffComp_Disappears.CompPostTick")
                .Replace("ldfld Verse.HediffComp_Disappears:ticksToDisappear;ldc.i4.1;sub", "ldfld Verse.HediffComp_Disappears:ticksToDisappear;ldc.i4.5;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_HealPermanentWounds_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffComp_HealPermanentWounds.CompPostTick")
                .Replace("ldfld Verse.HediffComp_HealPermanentWounds:ticksToHeal;ldc.i4.1;sub", "ldfld Verse.HediffComp_HealPermanentWounds:ticksToHeal;ldc.i4.5;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_Infecter_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffComp_Infecter.CompPostTick")
                .Replace("ldfld Verse.HediffComp_Infecter:ticksUntilInfect;ldc.i4.1;sub", "ldfld Verse.HediffComp_Infecter:ticksUntilInfect;ldc.i4.5;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_SelfHeal_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
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
            return new TranspilerFactory("HediffComp_SelfHeal.CompPostTick")
                .Replace("ldfld Verse.HediffComp_SelfHeal:ticksSinceHeal;ldc.i4.1;add", "ldfld Verse.HediffComp_SelfHeal:ticksSinceHeal;ldc.i4.5;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_SkillDecay_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffComp_SkillDecay.CompPostTick")
                .Search("mul;ldc.r4 60000;div")
                .Insert("ldc.r4 5;mul")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_TendDuration_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("HediffComp_TendDuration.CompPostTick")
                .Replace("ldfld Verse.HediffComp_TendDuration:tendTicksLeft;ldc.i4.1;sub", "ldfld Verse.HediffComp_TendDuration:tendTicksLeft;ldc.i4.5;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> CombatExtended_HediffComp_Venom_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("CombatExtended.HediffComp_Venom.CompPostTick")
                .Replace("ldfld CombatExtended.HediffComp_Venom:_venomPerTick;call Verse.HealthUtility:AdjustSeverity", "ldfld CombatExtended.HediffComp_Venom:_venomPerTick;ldc.r4 5;mul;call Verse.HealthUtility:AdjustSeverity")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> CombatExtended_HediffComp_InfecterCE_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("CombatExtended.HediffComp_InfecterCE.CompPostTick")
                .Replace("ldfld CombatExtended.HediffComp_InfecterCE:_ticksTended;ldc.i4.1;add", "ldfld CombatExtended.HediffComp_InfecterCE:_ticksTended;ldc.i4.5;add")
                .Replace("ldfld CombatExtended.HediffComp_InfecterCE:_ticksUntilInfect;ldc.i4.1;sub", "ldfld CombatExtended.HediffComp_InfecterCE:_ticksUntilInfect;ldc.i4.5;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> CombatExtended_HediffComp_Stabilize_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("CombatExtended.HediffComp_Stabilize.CompPostTick")
                // TODO: handle => if (this.stabilized && this.bleedModifier < 1f && this.parent.ageTicks % 60 == 0)
                .Replace("ldfld CombatExtended.HediffComp_Stabilize:bleedModifier;ldc.r4 0.01;add", "ldfld CombatExtended.HediffComp_Stabilize:bleedModifier;ldc.r4 0.05;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> SK_HeddifComp_StandOff_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("SK.HeddifComp_StandOff.CompPostTick")
                // TODO: handle => if (this.t % 100 == 0 && (GenDate.D......
                .Replace("ldfld SK.HeddifComp_StandOff:t;ldc.i4.1;add", "ldfld SK.HeddifComp_StandOff:t;ldc.i4.5;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> SK_HeddifComp_Traitor_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("SK.HeddifComp_Traitor.CompPostTick")
                .Replace("ldfld SK.HeddifComp_Traitor:ticksToDisappear;ldc.i4.1;sub", "ldfld SK.HeddifComp_Traitor:ticksToDisappear;ldc.i4.5;sub")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> HediffComp_VerbGiver_CompPostTick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            /* original method:
                    0	0000	ldarg.0
                    1	0001	ldarg.1
                    2	0002	call	instance void Verse.HediffComp::CompPostTick(float32&)
                    for (int i=0; i<5; i++) {
                        3	0007	ldarg.0
                        4	0008	ldfld	class Verse.VerbTracker Verse.HediffComp_VerbGiver::verbTracker
                        5	000D	callvirt	instance void Verse.VerbTracker::VerbsTick()
                    }
                    6	0012	ret
                 */
            return new TranspilerFactory("HediffComp_VerbGiver.CompPostTick")
                .Search("ldarg.0;ldarg.1;call Verse.HediffComp:CompPostTick(float&)")
                .Insert("localvar int;br.s 1;label 0") // loop start
                .Search("callvirt Verse.VerbTracker:VerbsTick")
                .Insert("ldloc 0;ldc.i4.1;add;stloc 0;label 1;ldloc 0;ldc.i4.5;blt.s 0")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> Adrenaline_Hediff_AdrenalineRush_UpdateSeverity_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Adrenaline.Hediff_AdrenalineRush.UpdateSeverity")
                .Replace("ldc.r4 20;mul", "ldc.r4 20;mul;ldc.r4 5;mul", 3)
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> Adrenaline_Hediff_AdrenalineCrash_UpdateSeverity_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("Adrenaline.Hediff_AdrenalineCrash.UpdateSeverity")
                .Replace("ldc.r4 20;mul", "ldc.r4 20;mul;ldc.r4 5;mul", 3)
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_PartBaseArtifical_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("rjw.Hediff_PartBaseArtifical.Tick")
                .Replace("ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.1;add", "ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.5;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_InsectEgg_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("rjw.Hediff_InsectEgg.Tick")
                .Replace("ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.1;add", "ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.5;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_PartBaseNatural_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("rjw.Hediff_PartBaseNatural.Tick")
                .Replace("ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.1;add", "ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.5;add")
                .Transpiler(ilGen, instructions);
        }

        static IEnumerable<CodeInstruction> rjw_Hediff_BasePregnancy_Tick_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilGen)
        {
            return new TranspilerFactory("rjw.Hediff_BasePregnancy.Tick")
                .Replace("ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.1;add", "ldarg.0;ldfld Verse.Hediff:ageTicks;ldc.i4.5;add")
                .Replace("ldfld rjw.Hediff_BasePregnancy:progress_per_tick;add", "ldfld rjw.Hediff_BasePregnancy:progress_per_tick;ldc.r4 5;mul;add")
                .Transpiler(ilGen, instructions);
        }
    }
}