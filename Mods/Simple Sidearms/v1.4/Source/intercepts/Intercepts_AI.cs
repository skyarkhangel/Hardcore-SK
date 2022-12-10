using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using PeteTimesSix.SimpleSidearms.Utilities;
using Verse.AI;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using SimpleSidearms.rimworld;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms.Intercepts
{

    [HarmonyPatch(typeof(Toil))]
    [HarmonyPatch(MethodType.Constructor)]
    public static class Toil_ctor_Postfix
    {
        [HarmonyPostfix]
        public static void _ctor(Toil __instance) 
        {
            if (Settings.ToolAutoSwitch == true) 
            {
                Toil toil = __instance;
                if (toil == null)
                    return;

                toil.AddPreInitAction(delegate
                {
                    Pawn pawn = toil.GetActor();
                    if (pawn == null || !pawn.IsValidSidearmsCarrier())
                        return;

                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                    if (pawnMemory == null)
                        return;
                    pawnMemory.CheckIfStillOnAutotoolJob();

                    StatDef activeStat = pawn.CurJob?.RecipeDef?.workSpeedStat;

                    SkillDef activeSkill = null;
                    if (toil.activeSkill != null && toil.activeSkill() != null)
                        activeSkill = toil.activeSkill();
                    else
                        activeSkill = pawn.CurJob?.RecipeDef?.workSkill;

                    List<StatDef> possiblyActiveStats = new List<StatDef>();
                    if (activeStat != null)
                        possiblyActiveStats.Add(activeStat);
                    else if (activeSkill != null && SkillStatMap.Map.ContainsKey(activeSkill))
                        possiblyActiveStats.AddRange(SkillStatMap.Map[activeSkill]);

                    bool usingAppropriateTool = WeaponAssingment.equipBestWeaponFromInventoryByStatModifiers(pawn, possiblyActiveStats);
                    if (usingAppropriateTool)
                    {
                        if (pawnMemory != null)
                        {
                            pawnMemory.autotoolToil = toil;
                            pawnMemory.autotoolJob = pawn.CurJobDef;
                        }
                    }
                });
                toil.AddFinishAction(delegate
                {
                    Pawn pawn = toil.GetActor();
                    if (pawn == null || !pawn.IsValidSidearmsCarrier())
                        return;

                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                    if (pawnMemory != null)
                    {
                        if(pawnMemory.autotoolToil == toil)
                            pawnMemory.delayIdleSwitchTimestamp = Find.TickManager.TicksGame;
                        else
                            pawnMemory.autotoolToil = null;
                    }

                });
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), "CarryWeaponOpenly")]
    public static class PawnRenderer_CarryWeaponOpenly_Postfix
    {
        [HarmonyPostfix]
        public static void CarryWeaponOpenly(ref PawnRenderer __instance, ref Pawn ___pawn, ref bool __result)
        {
            if (!___pawn.IsValidSidearmsCarrier())
                return;
            if (__result == true)
                return;
            else
            {
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(___pawn);
                if (pawnMemory != null && pawnMemory.autotoolToil != null) 
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(AutoUndrafter), "AutoUndraftTick")]
    public static class AutoUndrafter_AutoUndraftTick_Postfix
    {
        public const int autoRetrieveDelay = 300;

        [HarmonyPostfix]
        public static void AutoUndraftTick(AutoUndrafter __instance, Pawn ___pawn)
        {
            //Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            Pawn pawn = ___pawn;
            int tick = Find.TickManager.TicksGame;
            if (tick % 100 == 0)
            {
                if (pawn != null && pawn.Map != null && pawn.jobs != null && pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.Wait_Combat && pawn.stances != null && pawn.stances.curStance is Stance_Mobile)
                {
                    //pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
                    
                    WeaponAssingment.equipBestWeaponFromInventoryByPreference(pawn, DroppingModeEnum.Combat);

                    BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                    FieldInfo field = (__instance.GetType()).GetField("lastNonWaitingTick", bindFlags);
                    int lastNonWaitingTick = (int)field.GetValue(__instance);
                    if (tick - lastNonWaitingTick > autoRetrieveDelay)
                    {
                        Job retrieval = JobGiver_RetrieveWeapon.TryGiveJobStatic(pawn, true);
                        if (retrieval != null)
                            pawn.jobs.TryTakeOrderedJob(retrieval, JobTag.Misc);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "Drafted", MethodType.Setter)]
    public static class Pawn_DraftController_Drafted_Setter_Postfix
    {
        [HarmonyPostfix()]
        public static void DraftedSetter(Pawn_DraftController __instance)
        {
            //Log.Message("undraft intercept: " + __instance.Drafted);
            if (!__instance.Drafted)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.IsValidSidearmsCarrier())
                    return;
                if (pawn.IsColonist)
                {
                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                    if (pawnMemory == null)
                        return;

                    pawnMemory.InformOfUndraft();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Stance_Warmup), "StanceTick")]
    public static class Stance_Warmup_StanceTick_Postfix
    {
        public static Type ceRangedVerb;
        public static Type CERangedVerb {
            get {
                if(ceRangedVerb == null)
                    ceRangedVerb = AccessTools.TypeByName("CombatExtended.Verb_ShootCE");
                return ceRangedVerb;
            }
        }

        [HarmonyPostfix]
        public static void StanceTick(Stance_Warmup __instance)
        {

            if (Settings.RangedCombatAutoSwitch == false)
                return;
            Pawn pawn = __instance.stanceTracker.pawn;
            if (IsHunting(pawn))
                return;
            if (!pawn.IsValidSidearmsCarrier())
                return;
            if (!(__instance.verb is Verb_Shoot))
                return;
            /*if (!SimpleSidearms.CEOverride && !(__instance.verb is Verb_Shoot))
                return;
            if (SimpleSidearms.CEOverride && !(CERangedVerb.IsAssignableFrom(__instance.verb.GetType())))
                return;*/
            float statValue = pawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
            int ticks = (__instance.verb.verbProps.warmupTime * statValue).SecondsToTicks();
            
            if (__instance.ticksLeft / (float)ticks < 1f - Settings.RangedCombatAutoSwitchMaxWarmup)
            {
                return;
            }

            LocalTargetInfo target = __instance.focusTarg;
            var EMPGood = false;
            if (target.Pawn != null)
            {
                if (target.Pawn.RaceProps.IsMechanoid)
                    EMPGood = true;
                //unfortunately pawns with EMP grenades dont seem to know they work on shield belts => without forcing the target, the pawn stops attacking
                /*else if(target.Pawn.apparel.WornApparel.Where(a => a is ShieldBelt).Any(b => (b as ShieldBelt).ShieldState == ShieldState.Active)) //check for active shield belts
                {
                    EMPGood = true; 
                } */
            }


            bool skipManualUse = true;
            bool skipDangerous = pawn.IsColonistPlayerControlled && Settings.SkipDangerousWeapons;
            bool skipEMP = (pawn.IsColonistPlayerControlled && Settings.SkipEMPWeapons) || !EMPGood;
            WeaponAssingment.trySwapToMoreAccurateRangedWeapon(pawn, target, MiscUtils.shouldDrop(pawn, DroppingModeEnum.Combat, false), skipManualUse, skipDangerous, skipEMP);
        }

        public static bool IsHunting(Pawn pawn)
        {
            if (pawn.CurJob == null)
            {
                return false; 
            }
            JobDriver_Hunt jobDriver_Hunt = pawn.jobs.curDriver as JobDriver_Hunt;
            JobDriver_PredatorHunt jobDriver_PredatorHunt = pawn.jobs.curDriver as JobDriver_PredatorHunt;
            return jobDriver_Hunt != null || jobDriver_PredatorHunt != null;
        }
    }


    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static class Pawn_HealthTracker_MakeDowned
    {
        //EW EW EW GLOBAL FLAG EW EW
        public static bool beingDowned = false;

        [HarmonyPrefix]
        public static void MakeDowned_Prefix()
        {
            //Log.Message("makeDowned prefix");
            Pawn_HealthTracker_MakeDowned.beingDowned = true;
        }

        [HarmonyFinalizer]
        public static void MakeDowned_Finalizer()
        {
            //Log.Message("makeDowned finalizer");
            Pawn_HealthTracker_MakeDowned.beingDowned = false;
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryDropEquipment")]
    public static class Pawn_EquipmentTracker_TryDropEquipment
    {
        //EW EW EW GLOBAL FLAG EW EW
        public static bool dropEquipmentSourcedBySimpleSidearms = false;

        [HarmonyPostfix]
        public static void TryDropEquipment_Postfix(Pawn_EquipmentTracker __instance, bool __result, ThingWithComps resultingEq)
        {
            if(__result == true && resultingEq != null) 
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.IsValidSidearmsCarrier())
                    return;

                if (!(Pawn_HealthTracker_MakeDowned.beingDowned || dropEquipmentSourcedBySimpleSidearms))
                {
                    CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                    if(pawnMemory != null)
                        pawnMemory.ForgetSidearmMemory(resultingEq.toThingDefStuffDefPair());
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "AddEquipment")]
    public static class Pawn_EquipmentTracker_AddEquipment
    {
        //EW EW EW GLOBAL FLAG EW EW
        public static bool addEquipmentSourcedBySimpleSidearms = false;

        [HarmonyPostfix]
        public static void AddEquipment_Postfix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
        {
            if (!addEquipmentSourcedBySimpleSidearms)
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.IsValidSidearmsCarrier())
                    return;
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (pawnMemory == null)
                    return;
                pawnMemory.InformOfAddedPrimary(newEq);
            }
        }
    }

    [HarmonyPatch(typeof(JobDriver_AttackMelee), "TryMakePreToilReservations")]
    public static class JobDriver_AttackMelee_TryMakePreToilReservations
    {
        public static void Postfix(JobDriver_AttackMelee __instance)
        {
            Pawn caster = __instance.pawn;
            Job job = __instance.job;
            Thing target = job?.targetA.Thing;
            if (caster != null && target != null && target is Pawn && !caster.Dead/* && caster.def.race.Humanlike*/)
            {
                WeaponAssingment.chooseOptimalMeleeForAttack(caster, target as Pawn);
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_DropUnusedInventory))]
    [HarmonyPatch("Drop")]
    public static class JobGiver_DropUnusedInventory_Drop 
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Thing thing)
        {
            if (!(thing is ThingWithComps thingWithComps) || !pawn.IsValidSidearmsCarrier())
                return true;
            else
            {
                var weaponType = thingWithComps.toThingDefStuffDefPair();
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                var rememberedOfType = pawnMemory.rememberedWeapons.Where(w => w == weaponType);
                if (rememberedOfType.Any())
                {

                    var carriedOfType = pawn.getCarriedWeapons(includeTools: true).Where(w => w.toThingDefStuffDefPair() == weaponType);

                    if (rememberedOfType.Count() > carriedOfType.Sum(c => c.stackCount) - thingWithComps.stackCount)
                    {
                        //Log.Message($"was about to dump a weapon we need (need {rememberedOfType.Count()}, dropping {thingWithComps.stackCount} of {carriedOfType.Sum(c => c.stackCount)})");
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_InventoryTracker))]
    [HarmonyPatch("FirstUnloadableThing", MethodType.Getter)]
    public static class Pawn_InventoryTracker_FirstUnloadableThing
    {

        [HarmonyPostfix]
        public static void Postfix(Pawn_InventoryTracker __instance, ref ThingCount __result)
        {
            if (__result == default(ThingCount) ||
                !__result.Thing.def.IsWeapon)
            {
                return;
            }
            else
            {
                Pawn pawn = __instance.pawn;
                if (!pawn.IsValidSidearmsCarrier())
                    return;
                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (
                    pawnMemory == null ||
                    !pawn.IsColonist ||
                    __instance.innerContainer == null
                    )
                {
                    return;
                }
                List<ThingDefStuffDefPair> desiredSidearms = pawnMemory.RememberedWeapons.ListFullCopy();

                if (pawn.equipment.Primary != null)
                {
                    if (desiredSidearms.Contains(pawn.equipment.Primary.toThingDefStuffDefPair()))
                        desiredSidearms.Remove(pawn.equipment.Primary.toThingDefStuffDefPair());
                }


                int inventoryOffset = 0;

                //TODO: this does not preserve best possible weapon, just whichever one is first in inventory. Maybe fix?
                while (inventoryOffset < __instance.innerContainer.Count)
                {
                    Thing candidate = __instance.innerContainer[inventoryOffset];
                    if (candidate.def.IsWeapon & desiredSidearms.Contains(candidate.toThingDefStuffDefPair()))
                    {
                        desiredSidearms.Remove(candidate.toThingDefStuffDefPair());
                    }
                    else
                    {
                        __result = new ThingCount(candidate, candidate.stackCount);
                        return;
                    }
                    inventoryOffset++;
                }
                __result = default(ThingCount);
                return;
            }
        }
    }
}
