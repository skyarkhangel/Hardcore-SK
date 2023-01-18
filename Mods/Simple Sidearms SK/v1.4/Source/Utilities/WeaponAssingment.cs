using PeteTimesSix.SimpleSidearms.Intercepts;
using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.Sound;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms.Utilities
{
    public static class WeaponAssingment
    {

        public static bool equipSpecificWeaponTypeFromInventory(Pawn pawn, ThingDefStuffDefPair weapon, bool dropCurrent, bool intentionalDrop)
        {
            ThingWithComps match = pawn.inventory.innerContainer
                .Where(t => { return t is ThingWithComps && t.toThingDefStuffDefPair() == weapon; })
                .Cast<ThingWithComps>()
                .Where(t => { return StatCalculator.canUseSidearmInstance(t, pawn, out _); })
                .OrderByDescending(t => t.MarketValue)
                .FirstOrDefault();
            if (match != null)
                return equipSpecificWeaponFromInventory(pawn, match, dropCurrent, intentionalDrop);
            else
                return false;
        }

        public static bool equipSpecificWeaponFromInventory(Pawn pawn, ThingWithComps weapon, bool dropCurrent, bool intentionalDrop)
        {
            return equipSpecificWeapon(pawn, weapon, dropCurrent, intentionalDrop);
        }

        public static bool equipSpecificWeapon(Pawn pawn, ThingWithComps weapon, bool dropCurrent, bool intentionalDrop)
        {
            if (!pawn.IsValidSidearmsCarrier())
                return false;

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return false;

            if (weapon == pawn.equipment.Primary) //attepmpting to equip already-equipped weapon
            {
                Log.Warning("attepmpting to equip already-equipped weapon");
                return false;
            }

            if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canUseSidearmInstance(weapon, pawn, out string reason))
            {
                Log.Warning($"blocked equip of {weapon.Label} at equip-time because of: {reason}");
                return false;
            }

            var currentPrimary = pawn.equipment.Primary;

                //drop current on the ground
            if (dropCurrent && pawn.equipment.Primary != null)
            {
                if (!intentionalDrop)
                    DoFumbleMote(pawn);
                pawnMemory.InformOfDroppedSidearm(weapon, intentionalDrop);
                Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = true;
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out ThingWithComps droppedItem, pawn.Position, false);
                Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = false;
            }   
                //or put it in inventory
            else if (pawn.equipment.Primary != null)
            {
                ThingWithComps oldPrimary = pawn.equipment.Primary;
                bool addedToInventory = pawn.inventory.innerContainer.TryAddOrTransfer(oldPrimary, true);
                //pawn.equipment.Remove(oldPrimary);
                //bool addedToInventory = pawn.inventory.innerContainer.TryAdd(oldPrimary, true);
                if(!addedToInventory)
                    Log.Warning(String.Format("Failed to place primary equipment {0} (initially was {1}) into inventory when swapping to {2} on pawn {3} (colonist: {4}) (dropping: {5}, current drop mode: {6}). Aborting swap. Please report this!",
                       pawn.equipment.Primary != null ? pawn.equipment.Primary.LabelCap : "NULL",
                       currentPrimary != null ? currentPrimary.LabelCap : "NULL",
                       weapon != null ? weapon.LabelCap : "NULL",
                       pawn?.LabelCap,
                       pawn?.IsColonist,
                       dropCurrent,
                       Settings.FumbleMode
                    ));
            }

            if (pawn.equipment.Primary != null) 
            {
                Log.Warning(String.Format("Failed to remove current primary equipment {0} (initially was {1}) when swapping to {2} on pawn {3} (colonist: {4}) (dropping: {5}, current drop mode: {6}). Aborting swap. Please report this!", 
                    pawn.equipment.Primary != null ? pawn.equipment.Primary.LabelCap : "NULL", 
                    currentPrimary != null ? currentPrimary.LabelCap : "NULL", 
                    weapon != null ? weapon.LabelCap : "NULL", 
                    pawn?.LabelCap,
                    pawn?.IsColonist,
                    dropCurrent,
                    Settings.FumbleMode
                    ));
                return false;
            }

            if (weapon == null)
            {
               
            }
            else
            {
                if (weapon.stackCount > 1)
                    weapon = weapon.SplitOff(1) as ThingWithComps; //if this cast doesnt work the world has gone mad

                if (weapon.holdingOwner != null)
                    weapon.holdingOwner.Remove(weapon);
                Pawn_EquipmentTracker_AddEquipment.addEquipmentSourcedBySimpleSidearms = true;
                pawn.equipment.AddEquipment(weapon as ThingWithComps);
                Pawn_EquipmentTracker_AddEquipment.addEquipmentSourcedBySimpleSidearms = false;

                if (weapon.def.soundInteract != null)
                {
                    weapon.def.soundInteract.PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
                }
            }

            //avoid hunting stackoverflowexception
            if (pawn.jobs != null && pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.Hunt)
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);

            return true;
        }

        public static void DoFumbleMote(Pawn pawn)
        {
            var bestSkillc = Math.Max(pawn.skills.GetSkill(SkillDefOf.Shooting).Level, pawn.skills.GetSkill(SkillDefOf.Melee).Level);
            var chancec = Settings.FumbleRecoveryChance.Evaluate(bestSkillc);
            if (!Prefs.DevMode)
            {
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, Prefs.DevMode ? "Fumbled".Translate() : "Fumbled".Translate());
            }
            else
            {
                var bestSkill = Math.Max(pawn.skills.GetSkill(SkillDefOf.Shooting).Level, pawn.skills.GetSkill(SkillDefOf.Melee).Level);
                var chance = Settings.FumbleRecoveryChance.Evaluate(bestSkill);
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, Prefs.DevMode ? "Fumbled_dev".Translate($"{((1f - chance) * 100).ToString("F0")}% chance") : "Fumbled".Translate());
            }
        }

        public static bool equipBestWeaponFromInventoryByStatModifiers(Pawn pawn, List<StatDef> stats)
        {
            //Log.Message("looking for a stat booster for stats " + String.Join(",", stats.Select(s => s.label))); ;
            //Log.Message("equipBestWeaponFromInventoryByStatModifiers Run");
            if (!pawn.IsValidSidearmsCarrier() || stats.Count == 0 || pawn.Drafted)
                return false;

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return false;

            ThingWithComps bestBooster = pawn.getCarriedWeapons(includeTools: true).Where(t =>
            {
                _ = t.toThingDefStuffDefPair().getBestStatBoost(stats, out bool found); return found;
            }).OrderBy(t =>
            {
                return t.toThingDefStuffDefPair().getBestStatBoost(stats, out _);
            }).FirstOrDefault();

            if (bestBooster == default(ThingWithComps))
                return false;

            if (bestBooster == pawn.equipment.Primary)
                return true;

            bool success = equipSpecificWeaponFromInventory(pawn, bestBooster, false, false);
            return success;
        }

        public static void equipBestWeaponFromInventoryByPreference(Pawn pawn, DroppingModeEnum dropMode, PrimaryWeaponMode? modeOverride = null, Pawn target = null)
        {
            //Log.Message("equipBestWeaponFromInventoryByPreference Run");
            if (!pawn.IsValidSidearmsCarrier())
                return;
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return;

            PrimaryWeaponMode mode = modeOverride == null ? pawnMemory.primaryWeaponMode : modeOverride.Value;

            if ((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0)
            {
                if (pawn.equipment.Primary != null)
                {
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            if (pawn.Drafted && 
                (pawnMemory.ForcedUnarmedWhileDrafted || pawnMemory.ForcedUnarmed && pawnMemory.ForcedWeaponWhileDrafted == null))
            {
                if (pawn.equipment.Primary != null)
                {
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
                    
            }
            if (pawn.Drafted && pawnMemory.ForcedWeaponWhileDrafted != null)
            {
                if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.ForcedWeaponWhileDrafted.Value)
                {
                    var requiredWeapon = pawnMemory.ForcedWeaponWhileDrafted.Value;
                    /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                    {
                        //clear invalid
                        pawnMemory.ForcedWeaponWhileDrafted = null;
                        return;
                    }*/
                    bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            if (pawnMemory.ForcedUnarmed)
            {
                if (pawn.equipment.Primary != null)
                {
                    bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
            }
            if (pawnMemory.ForcedWeapon != null)
            {
                if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.ForcedWeapon.Value)
                {
                    var requiredWeapon = pawnMemory.ForcedWeapon.Value;
                    /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                    {
                        //clear invalid
                        pawnMemory.ForcedWeapon = null;
                        return;
                    }*/
                    bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                    if (success)
                        return;
                }
                else
                    return;
            }

            bool readyForGun = false;
            bool huntingNeeded = false;
            string lastJob = pawn.mindState.lastJobTag.ToStringSafe();
            if (lastJob != null && lastJob != "Fieldwork" && lastJob != "MiscWork")
            {
               // if (pawn.GetTimeAssignment().defName == "Work" || pawn.GetTimeAssignment().defName == "Anything" /* && pawn.MapHeld.weatherManager.RainRate > 0*/)
                //{
                    readyForGun = true;
               // }
            }

            if ((pawn.workSettings.GetPriority(WorkTypeDefOf.Hunting) != 0 && pawn.MapHeld.designationManager.AnySpawnedDesignationOfDef(DesignationDefOf.Hunt)))
            {
                huntingNeeded = true;
            }

            if (mode == PrimaryWeaponMode.Ranged ||
                ((mode == PrimaryWeaponMode.BySkill) && (pawn.getSkillWeaponPreference() == PrimaryWeaponMode.Ranged)) && ((readyForGun && Settings.RangedNonCombatAutoSwitch) || pawn.Drafted) || huntingNeeded)
            {
                if (pawnMemory.DefaultRangedWeapon != null && pawn.hasWeaponType(pawnMemory.DefaultRangedWeapon.Value))
                {
                    if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.DefaultRangedWeapon.Value)
                    {
                        var requiredWeapon = pawnMemory.DefaultRangedWeapon.Value;
                        /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                        {
                            //clear invalid
                            pawnMemory.DefaultRangedWeapon = null;
                            return;
                        }*/
                        bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                        if (success)
                        {
                            return;
                        }
                            
                    }
                    else
                    {
                        return;
                    }
                       
                }

                else
                {
                    bool skipManualUse = true;
                    bool skipDangerous = pawn.IsColonistPlayerControlled && Settings.SkipDangerousWeapons;
                    bool skipEMP = true;
                    (ThingWithComps weapon, float dps, float averageSpeed) bestWeapon = GettersFilters.findBestRangedWeapon(pawn, null, skipManualUse, skipDangerous, skipEMP);
                    if (bestWeapon.weapon != null)
                    {
                        if (pawn.equipment.Primary != bestWeapon.weapon)
                        {
                            bool success = equipSpecificWeaponFromInventory(pawn, bestWeapon.weapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                            if (success)
                            {
                                return;
                            }
                               
                        }
                        else
                        {
                            return;
                        }
                          
                    }
                }
            }

            //all that's left is either melee preference or no ranged weapon found - so in either case, we want to equip a melee weapon.

            /*if (mode == GoldfishModule.PrimaryWeaponMode.Melee ||
                ((mode == GoldfishModule.PrimaryWeaponMode.BySkill) && (pawn.getSkillWeaponPreference() == GoldfishModule.PrimaryWeaponMode.Melee)))*/
            {
                //Log.Message("melee mode");
                //prefers melee
                if (pawnMemory.PreferredUnarmed)
                {
                    if (pawn.equipment.Primary != null)
                    {
                        bool success = equipSpecificWeapon(pawn, null, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                        if (success)
                            return;
                    }
                    else
                        return;

                }
                else 
                {
                    if ((pawnMemory.PreferredMeleeWeapon != null && pawn.hasWeaponType(pawnMemory.PreferredMeleeWeapon.Value))/* || needGun != true*/)
                    {
                        if (pawn.equipment.Primary == null || pawn.equipment.Primary.toThingDefStuffDefPair() != pawnMemory.PreferredMeleeWeapon.Value)
                        {
                            var requiredWeapon = pawnMemory.PreferredMeleeWeapon.Value;
                            /*if (!Settings.AllowBlockedWeaponUse && !StatCalculator.canCarrySidearmType(requiredWeapon, pawn, out _))
                            {
                                //clear invalid
                                pawnMemory.PreferredMeleeWeapon = null;
                                return;
                            }*/
                            bool success = equipSpecificWeaponTypeFromInventory(pawn, requiredWeapon, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                            if (success)
                                return;
                        }
                        else
                            return;
                    }
                    else
                    {
                        ThingWithComps result;
                        bool foundAlternative = GettersFilters.findBestMeleeWeapon(pawn, out result, includeRangedWithBash: false);
                        if (foundAlternative)
                        {
                            if (pawn.equipment.Primary != result)
                            {
                                bool success = equipSpecificWeaponFromInventory(pawn, result, MiscUtils.shouldDrop(pawn, dropMode, false), false);
                                if (success)
                                    return;
                            }
                            else
                                return;
                        }
                    }
                }
            }
            return;
        }

        //When hit in Close-Quarter Combat 
        internal static void doCQC(Pawn pawn, Pawn attacker)
        {
            //Log.Message("doCQC Run");
            if (Settings.CQCAutoSwitch == true)
            {
                if (attacker != null)
                {
                    if (attacker.MentalStateDef == MentalStateDefOf.SocialFighting && pawn.MentalStateDef == MentalStateDefOf.SocialFighting)
                        return;

                    if(attacker.equipment != null)
                    {
                        if (attacker.equipment.Primary != null)
                        {
                            if (attacker.equipment.Primary.def.IsRangedWeapon)
                                return;
                        }
                    }
                    

                    if (Settings.CQCTargetOnly == true && attacker != pawn.mindState.lastAttackedTarget.Thing)
                    {
                        return;
                    }

                    if (!Settings.OptimalMelee && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsMeleeWeapon)
                        return;

                    bool changed = tryCQCWeaponSwapToMelee(pawn, attacker, DroppingModeEnum.InDistress);

                    //change targets if shooting something else, or has no set target (or nothing)
                    if (changed && (attacker != pawn.mindState.enemyTarget || pawn.mindState.enemyTarget == null))
                    {
                        if (pawn.jobs.curJob.def == JobDefOf.AttackStatic)
                        {
                            Job atkJob = JobMaker.MakeJob(JobDefOf.AttackMelee, attacker);
                            atkJob.maxNumMeleeAttacks = 1;
                            atkJob.expiryInterval = 200;
                            pawn.jobs.TryTakeOrderedJob(atkJob);
                        }
                    }
                }
            }
        }

        public static void chooseOptimalMeleeForAttack(Pawn pawn, Pawn target)
        {
            if (!Settings.OptimalMelee || target == null || (target.MentalStateDef == MentalStateDefOf.SocialFighting && pawn.MentalStateDef == MentalStateDefOf.SocialFighting))
                    return;

            tryCQCWeaponSwapToMelee(pawn, target, DroppingModeEnum.Combat);
        }

        public static bool tryCQCWeaponSwapToMelee(Pawn pawn, Pawn target, DroppingModeEnum dropMode)
        {
            //Log.Message("tryCQCWeaponSwapToMelee Run");
            if (!pawn.IsValidSidearmsCarrier())
                return false;

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);

            if (pawnMemory == null)
                return false;

            if (!pawn.RaceProps.Humanlike)
                return false;

            if (pawn.equipment.Primary != null)
            {
                if (pawn.equipment.Primary.def.destroyOnDrop)
                    return false;
            }

            if (pawnMemory.IsCurrentWeaponForced(false))
                return false;

            var current = pawn.equipment.Primary;
            equipBestWeaponFromInventoryByPreference(pawn, dropMode, PrimaryWeaponMode.Melee, target: target);
            return (current != pawn.equipment.Primary);
        }


        public static bool trySwapToMoreAccurateRangedWeapon(Pawn pawn, LocalTargetInfo target, bool dropCurrent, bool skipManualUse, bool skipDangerous = true, bool skipEMP = true)
        {
            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);

            if (pawn == null || pawn.Dead || pawnMemory == null || pawn.equipment == null || pawn.inventory == null)
                return false;

            if (pawnMemory.IsCurrentWeaponForced(false))
                return false;

            (ThingWithComps weapon, float dps, float averageSpeed) bestWeapon = GettersFilters.findBestRangedWeapon(pawn, target, skipManualUse, skipDangerous, skipEMP, true);

            if (bestWeapon.weapon == null)
                return false;


            var targetDistance = target.Cell.DistanceTo(pawn.Position);
            float currentDPS = StatCalculator.RangedDPS(pawn.equipment.Primary, Settings.SpeedSelectionBiasRanged, bestWeapon.averageSpeed, targetDistance);
            
            if (bestWeapon.dps < currentDPS + MiscUtils.ANTI_OSCILLATION_FACTOR)
                return false;

            equipSpecificWeaponFromInventory(pawn, bestWeapon.weapon, dropCurrent, false);
            return true;
        }

        public static void dropSidearm(Pawn pawn, Thing weapon, bool intentionalDrop)
        {
            if (weapon == null)
                return;
            if (pawn.IsQuestLodger() && intentionalDrop)
                return;

            if (!intentionalDrop)
                DoFumbleMote(pawn);

            if (pawn.equipment.Primary == weapon)
            {
                Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = true;
                pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out _, pawn.Position, false);
                Pawn_EquipmentTracker_TryDropEquipment.dropEquipmentSourcedBySimpleSidearms = false;
            }
            else
            {
                if (weapon.stackCount > 1)
                {
                    var toDrop = weapon.SplitOff(1);
                    GenDrop.TryDropSpawn(toDrop, pawn.Position, pawn.Map, ThingPlaceMode.Near, out _);
                }
                else
                {
                    pawn.inventory.innerContainer.TryDrop(weapon, pawn.Position, pawn.Map, ThingPlaceMode.Near, out _, null);
                }   
            }

            CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
            if (pawnMemory == null)
                return;
            pawnMemory.InformOfDroppedSidearm(weapon, intentionalDrop);
        }
    }
}
