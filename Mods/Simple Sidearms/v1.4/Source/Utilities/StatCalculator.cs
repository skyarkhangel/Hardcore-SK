using RimWorld;
using SimpleSidearms.rimworld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms.Utilities
{
    public static class StatCalculator
    {
        public static int countForLimitType(Pawn pawn, WeaponSearchType type)
        {
            return GettersFilters.filterForWeaponKind(pawn.getCarriedWeapons(), type).Count();
        }

        public static float weightForLimitType(Pawn pawn, WeaponSearchType type)
        {
            float total = 0;
            IEnumerable<ThingWithComps> weapons = GettersFilters.filterForWeaponKind(pawn.getCarriedWeapons(), type);
            foreach (ThingWithComps thing in weapons)
            {
                switch (type)
                {
                    case WeaponSearchType.MeleeCapable:
                        if ((thing.def.IsMeleeWeapon || (thing.def.tools != null && thing.def.tools.Any((Tool x) => x.VerbsProperties.Any((VerbProperties y) => y.IsMeleeAttack)))))
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                    case WeaponSearchType.Melee:
                        if (thing.def.IsMeleeWeapon)
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                    case WeaponSearchType.Ranged:
                        if (thing.def.IsRangedWeapon)
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                    case WeaponSearchType.Both:
                    default:
                        if (thing.def.IsWeapon)
                        {
                            total += thing.GetStatValue(StatDefOf.Mass);
                        }
                        break;
                }
            }
            return total;
        }

        public static bool isValidSidearm(ThingDefStuffDefPair sidearm, out string errString)
        {
            float sidearmWeight = sidearm.thing.GetStatValueAbstract(StatDefOf.Mass, sidearm.stuff);

            if (!Settings.SeparateModes)
            {
                switch (Settings.LimitModeSingle)
                {
                    case LimitModeSingleSidearm.AbsoluteWeight:
                        if (sidearmWeight >= Settings.LimitModeSingleMelee_AbsoluteMass)
                        {
                            errString = "SidearmPickupFail_TooHeavyForSidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.Selection:
                        if (!Settings.LimitModeSingle_Selection.Contains<ThingDef>(sidearm.thing))
                        {
                            errString = "SidearmPickupFail_NotASidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.None:
                    default:
                        break;
                }
            }
            else
            {
                if (sidearm.thing.IsMeleeWeapon)
                {
                    switch (Settings.LimitModeSingleMelee)
                    {
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= Settings.LimitModeSingleMelee_AbsoluteMass)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection:
                            if (!Settings.LimitModeSingleMelee_Selection.Contains<ThingDef>(sidearm.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.None:
                        default:
                            break;
                    }
                }
                else if(sidearm.thing.IsRangedWeapon)
                {
                    switch (Settings.LimitModeSingleRanged)
                    {
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= Settings.LimitModeSingleRanged_AbsoluteMass)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection:
                            if (!Settings.LimitModeSingleRanged_Selection.Contains<ThingDef>(sidearm.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.None:
                        default:
                            break;
                    }
                }
            }
            errString = "SidearmPickupPass".Translate();
            return true;
        }

        public static bool canUseSidearmInstance(ThingWithComps sidearmThing, Pawn pawn, out string errString)
        {
            //nicked from EquipmentUtility.CanEquip
            CompBladelinkWeapon compBladelinkWeapon = sidearmThing.TryGetComp<CompBladelinkWeapon>();
            if (compBladelinkWeapon != null && compBladelinkWeapon.Biocodable && compBladelinkWeapon.CodedPawn != null && compBladelinkWeapon.CodedPawn != pawn)
            {
                errString = "BladelinkBondedToSomeoneElse".Translate();
                return false;
            }
            if (CompBiocodable.IsBiocoded(sidearmThing) && !CompBiocodable.IsBiocodedFor(sidearmThing, pawn))
            {
                errString = "BiocodedCodedForSomeoneElse".Translate();
                return false;
            }
            if (EquipmentUtility.AlreadyBondedToWeapon(sidearmThing, pawn))
            {
                errString = "BladelinkAlreadyBondedMessage".Translate(pawn.Named("PAWN"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
                return false;
            }
            if (compBladelinkWeapon != null && !compBladelinkWeapon.Biocoded && !compBladelinkWeapon.TraitsListForReading.Any(t => t.neverBond == true))
            {
                errString = "SidearmPickupFail_NotYetBladelinkBonded".Translate();
                return false;
            }
            if (sidearmThing != null && EquipmentUtility.RolePreventsFromUsing(pawn, sidearmThing, out string roleReason))
            {
                Log.Message($"use of {sidearmThing.Label} prevented by role");
                errString = roleReason;
                return false;
            }

            errString = "No issue";
            return true;
        }

        public static bool canUseSidearmType(ThingDefStuffDefPair sidearmType, Pawn pawn, out string errString)
        {
            if (sidearmType != null && sidearmType.thing != null) 
            {
                if (ModsConfig.IdeologyActive && pawn.Ideo != null)
                {
                    Precept_Role role = pawn.Ideo.GetRole(pawn);
                    if (role != null)
                    {
                        if (role.def.roleEffects != null && role.def.roleEffects.Any())
                        {
                            //hardcoded cos RoleEffect's CanEquip requires an instance of Thing
                            if (sidearmType.thing.IsMeleeWeapon)
                            {
                                if (role.def.roleEffects.Any(e => e is RoleEffect_NoMeleeWeapons))
                                {
                                    errString = "RoleEffectWontUseMeleeWeapons".Translate();
                                    return false;
                                }
                            }
                            else if (sidearmType.thing.IsRangedWeapon)
                            {
                                if (role.def.roleEffects.Any(e => e is RoleEffect_NoRangedWeapons))
                                {
                                    errString = "RoleEffectWontUseRangedWeapons".Translate();
                                    return false;
                                }
                            }
                        }
                    }
                }

            }

            errString = "No issue";
            return true;
        }

        public static bool canCarrySidearmInstance(ThingWithComps sidearmThing, Pawn pawn, out string errString)
        {
            //nicked from EquipmentUtility.CanEquip
            CompBladelinkWeapon compBladelinkWeapon = sidearmThing.TryGetComp<CompBladelinkWeapon>();
            if (compBladelinkWeapon != null && compBladelinkWeapon.Biocodable && compBladelinkWeapon.CodedPawn != null && compBladelinkWeapon.CodedPawn != pawn)
            {
                errString = "BladelinkBondedToSomeoneElse".Translate();
                return false;
            }
            if (CompBiocodable.IsBiocoded(sidearmThing) && !CompBiocodable.IsBiocodedFor(sidearmThing, pawn))
            {
                errString = "BiocodedCodedForSomeoneElse".Translate();
                return false;
            }
            if (EquipmentUtility.AlreadyBondedToWeapon(sidearmThing, pawn))
            {
                errString = "BladelinkAlreadyBondedMessage".Translate(pawn.Named("PAWN"), pawn.equipment.bondedWeapon.Named("BONDEDWEAPON"));
                return false;
            }
            if (compBladelinkWeapon != null && !compBladelinkWeapon.Biocoded && !compBladelinkWeapon.TraitsListForReading.Any(t => t.neverBond == true))
            {
                errString = "SidearmPickupFail_NotYetBladelinkBonded".Translate();
                return false;
            }
            if (EquipmentUtility.RolePreventsFromUsing(pawn, sidearmThing, out string roleReason))
            {
                errString = roleReason;
                return false;
            }

            ThingDefStuffDefPair sidearm = sidearmThing.toThingDefStuffDefPair();
            
            return canCarrySidearmType(sidearm, pawn, out errString);
        }

        public static bool canCarrySidearmType(ThingDefStuffDefPair sidearmType, Pawn pawn, out string errString)
        {
            float maxCapacity = MassUtility.Capacity(pawn);
            float freeCapacity = MassUtility.FreeSpace(pawn);
            float sidearmWeight = sidearmType.thing.GetStatValueAbstract(StatDefOf.Mass, sidearmType.stuff);

            if (((pawn.CombinedDisabledWorkTags & WorkTags.Violent) != 0) && (!sidearmType.isTool()))
            {
                errString = "SidearmPickupFail_NotAToolForPacifist".Translate(pawn.LabelShort);
                return false;
            }

            //this is duplicated in the switches later but Id rather not risk accidentaly deleting a case that might come up
            if (!isValidSidearm(sidearmType, out errString))
                return false;

            if (sidearmWeight >= freeCapacity)
            {
                errString = "SidearmPickupFail_NoFreeSpace".Translate();
                return false;
            }

            if (!Settings.SeparateModes)
            {
                switch (Settings.LimitModeSingle)
                {
                    case LimitModeSingleSidearm.None:
                        break;
                    case LimitModeSingleSidearm.AbsoluteWeight:
                        if(sidearmWeight >= Settings.LimitModeSingle_AbsoluteMass)
                        {
                            errString = "SidearmPickupFail_TooHeavyForSidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.RelativeWeight:
                        if(sidearmWeight >= Settings.LimitModeSingle_RelativeMass * maxCapacity)
                        {
                            errString = "SidearmPickupFail_TooHeavyForSidearm".Translate();
                            return false;
                        }
                        break;
                    case LimitModeSingleSidearm.Selection:
                        if(!Settings.LimitModeSingle_Selection.Contains<ThingDef>(sidearmType.thing))
                        {
                            errString = "SidearmPickupFail_NotASidearm".Translate();
                            return false;
                        }
                        break;
                }
                switch (Settings.LimitModeAmount)
                {
                    case LimitModeAmountOfSidearms.None:
                        break;
                    case LimitModeAmountOfSidearms.AbsoluteWeight:
                        if (sidearmWeight >= (Settings.LimitModeAmount_AbsoluteMass - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.RelativeWeight:
                        if (sidearmWeight >= ((Settings.LimitModeAmount_RelativeMass * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.Slots:
                        if (Settings.LimitModeAmount_Slots <= countForLimitType(pawn, WeaponSearchType.Both))
                        {
                            errString = "SidearmPickupFail_AllSlotsFull".Translate();
                            return false;
                        }
                        break;
                }
            }
            else
            {
                switch (Settings.LimitModeAmountTotal)
                {
                    case LimitModeAmountOfSidearms.None:
                        break;
                    case LimitModeAmountOfSidearms.AbsoluteWeight:
                        if (sidearmWeight >= (Settings.LimitModeAmountTotal_AbsoluteMass - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.RelativeWeight:
                        if (sidearmWeight >= ((Settings.LimitModeAmountTotal_RelativeMass * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Both)))
                        {
                            errString = "SidearmPickupFail_SidearmsTooHeavyInTotal".Translate();
                            return false;
                        }
                        break;
                    case LimitModeAmountOfSidearms.Slots:
                        if (Settings.LimitModeAmountTotal_Slots <= countForLimitType(pawn, WeaponSearchType.Both))
                        {
                            errString = "SidearmPickupFail_AllSlotsFull".Translate();
                            return false;
                        }
                        break;
                }
                if (sidearmType.thing.IsMeleeWeapon)
                {
                    switch (Settings.LimitModeSingleMelee)
                    {
                        case LimitModeSingleSidearm.None:
                            break;
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= Settings.LimitModeSingleMelee_AbsoluteMass)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.RelativeWeight:
                            if (sidearmWeight >= Settings.LimitModeSingleMelee_RelativeMass * maxCapacity)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection:
                            if (!Settings.LimitModeSingleMelee_Selection.Contains<ThingDef>(sidearmType.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmMelee".Translate();
                                return false;
                            }
                            break;
                    }
                    switch (Settings.LimitModeAmountMelee)
                    {
                        case LimitModeAmountOfSidearms.None:
                            break;
                        case LimitModeAmountOfSidearms.AbsoluteWeight:
                            if (sidearmWeight >= (Settings.LimitModeAmountMelee_AbsoluteMass - weightForLimitType(pawn, WeaponSearchType.Melee)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            if (sidearmWeight >= ((Settings.LimitModeAmountMelee_RelativeMass * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Melee)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyMelee".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            if (Settings.LimitModeAmountMelee_Slots <= countForLimitType(pawn, WeaponSearchType.Melee))
                            {
                                errString = "SidearmPickupFail_MeleeSlotsFull".Translate();
                                return false;
                            }
                            break;
                    }
                }
                else if(sidearmType.thing.IsRangedWeapon)
                {
                    switch (Settings.LimitModeSingleRanged)
                    {
                        case LimitModeSingleSidearm.None:
                            break;
                        case LimitModeSingleSidearm.AbsoluteWeight:
                            if (sidearmWeight >= Settings.LimitModeSingleRanged_AbsoluteMass)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.RelativeWeight:
                            if (sidearmWeight >= Settings.LimitModeSingleRanged_RelativeMass * maxCapacity)
                            {
                                errString = "SidearmPickupFail_TooHeavyForSidearmRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeSingleSidearm.Selection: 
                            if (!Settings.LimitModeSingleRanged_Selection.Contains<ThingDef>(sidearmType.thing))
                            {
                                errString = "SidearmPickupFail_NotASidearmRanged".Translate();
                                return false;
                            }
                            break;
                    }
                    switch (Settings.LimitModeAmountRanged)
                    {
                        case LimitModeAmountOfSidearms.None:
                            break;
                        case LimitModeAmountOfSidearms.AbsoluteWeight:
                            if (sidearmWeight >= (Settings.LimitModeAmountRanged_AbsoluteMass - weightForLimitType(pawn, WeaponSearchType.Ranged)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.RelativeWeight:
                            if (sidearmWeight >= ((Settings.LimitModeAmountRanged_RelativeMass * maxCapacity) - weightForLimitType(pawn, WeaponSearchType.Ranged)))
                            {
                                errString = "SidearmPickupFail_SidearmsTooHeavyRanged".Translate();
                                return false;
                            }
                            break;
                        case LimitModeAmountOfSidearms.Slots:
                            if (Settings.LimitModeAmountRanged_Slots <= countForLimitType(pawn, WeaponSearchType.Ranged))
                            {
                                errString = "SidearmPickupFail_RangedSlotsFull".Translate();
                                return false;
                            }
                            break;
                    }
                }
            }
            errString = "SidearmPickupPass".Translate();
            return true;
        }

        public static float AdjustedAccuracy(VerbProperties props, RangeCategory cat, Thing equipment)
        {
            if (equipment != null)
            {
                StatDef stat = null;
                switch (cat)
                {
                    case RangeCategory.Touch:
                        stat = StatDefOf.AccuracyTouch;
                        break;
                    case RangeCategory.Short:
                        stat = StatDefOf.AccuracyShort;
                        break;
                    case RangeCategory.Medium:
                        stat = StatDefOf.AccuracyMedium;
                        break;
                    case RangeCategory.Long:
                        stat = StatDefOf.AccuracyLong;
                        break;
                }
                return equipment.GetStatValue(stat, true);
            }
            switch (cat)
            {
                case RangeCategory.Touch:
                    return props.accuracyTouch;
                case RangeCategory.Short:
                    return props.accuracyShort;
                case RangeCategory.Medium:
                    return props.accuracyMedium;
                case RangeCategory.Long:
                    return props.accuracyLong;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static float RangedSpeed(ThingWithComps weapon)
        {
            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;
            float warmup = atkProps.warmupTime;
            float cooldown = weapon.GetStatValue(StatDefOf.RangedWeapon_Cooldown);
            int burstShot = atkProps.burstShotCount;
            int ticksBetweenShots = atkProps.ticksBetweenBurstShots;
            float speedFactor = (((warmup + cooldown)) + (burstShot - 1) * (ticksBetweenShots / 60f));
            return speedFactor;
        }

        public static float RangedDPSAverage(ThingWithComps weapon, float speedBias, float averageSpeed)
        {
            if (weapon == null)
                return 0;

            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;
            float damage = (atkProps.defaultProjectile == null) ? 0 : atkProps.defaultProjectile.projectile.GetDamageAmount(weapon);
            int burstShot = atkProps.burstShotCount;
            float speedFactor = RangedSpeed(weapon);
            float speedFactorBase = speedFactor;

            float diffFromAverage = speedFactor - averageSpeed;
            diffFromAverage *= (speedBias - 1);
            speedFactor += diffFromAverage;

            float rawDps = (damage * burstShot) / speedFactor;
            //Log.Message(weapon.LabelCap + " dps:" + rawDps + "dam:" + damage * burstShot + " spdfac:" + speedFactor + " spdFacBase:" + speedFactorBase);
            float DpsAvg = 0f;
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Short, weapon);
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Medium, weapon);
            DpsAvg += rawDps * AdjustedAccuracy(atkProps, RangeCategory.Long, weapon);
            return DpsAvg / 3f;
        }

        public static float RangedDPS(ThingWithComps weapon, float speedBias, float averageSpeed, float range)
        {
            Verb atkVerb = (weapon.GetComp<CompEquippable>()).PrimaryVerb;
            VerbProperties atkProps = atkVerb.verbProps;

            if (atkProps.range * atkProps.range < range || atkProps.minRange * atkProps.minRange > range)
                return -1;

            float hitChance = atkProps.GetHitChanceFactor(weapon, range);
            float damage = (atkProps.defaultProjectile == null) ? 0 : atkProps.defaultProjectile.projectile.GetDamageAmount(weapon);
            int burstShot = atkProps.burstShotCount;
            float speedFactor = RangedSpeed(weapon);
            float speedFactorBase = speedFactor;

            float diffFromAverage = speedFactor - averageSpeed;
            diffFromAverage *= (speedBias - 1);
            speedFactor += diffFromAverage;

            float rawDps = (damage * burstShot) / speedFactor;
            float Dps = rawDps * hitChance;
            //Log.Message(weapon.LabelCap + " dps:" + rawDps + "dam:"+damage*burstShot + " spdfac:" + speedFactor + " spdFacBase:" + speedFactorBase);
            return Dps;
        }

        public static float MeleeSpeed(ThingWithComps weapon, Pawn pawn)
        {
            GetVerbsAndTools(weapon, out List<VerbProperties> verbProps, out List<Tool> tools);
            float speed = VerbUtility.GetAllVerbProperties(verbProps, tools).Where(x => x.verbProps.IsMeleeAttack).AverageWeighted(x => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, pawn, weapon, null, false), x => x.verbProps.AdjustedCooldown(x.tool, pawn, weapon));
            return speed;
        }

        public static float MeleePenetration(ThingWithComps weapon, Pawn pawn)
        {
            GetVerbsAndTools(weapon, out List<VerbProperties> verbProps, out List<Tool> tools);
            float penetration = VerbUtility.GetAllVerbProperties(verbProps, tools).Where(x => x.verbProps.IsMeleeAttack).AverageWeighted(x => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, pawn, weapon, null, false), x => x.verbProps.AdjustedArmorPenetration(x.tool, pawn, weapon, null));
            return penetration;
        }


        public static float getMeleeDPSBiased(ThingWithComps weapon, Pawn pawn, float speedBias, float averageSpeed)
        {
            //nicked from StatWorker_MeleeAverageDPS
            GetVerbsAndTools(weapon, out List<VerbProperties> verbProps, out List<Tool> tools);
            float damage = VerbUtility.GetAllVerbProperties(verbProps, tools).Where(x => x.verbProps.IsMeleeAttack).AverageWeighted(x => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, pawn, weapon, null, false), x => x.verbProps.AdjustedMeleeDamageAmount(x.tool, pawn, weapon, null));
            float speed = MeleeSpeed(weapon, pawn);
            float penetration = MeleePenetration(weapon, pawn);
            float speedBase = speed;

            float diffFromAverage = speed - averageSpeed;
            diffFromAverage *= (speedBias - 1);
            speed += diffFromAverage;

            if (speed == 0f)
            {
                return 0f;
            }
            return (damage + (damage * penetration)) / speed;
        }

        public static void GetVerbsAndTools(ThingWithComps weapon, out List<VerbProperties> verbs, out List<Tool> tools)
        {
            if (weapon.def.isTechHediff)
            {
                HediffDef hediffDef = FindTechHediffHediff(weapon);
                if (hediffDef == null)
                {
                    verbs = null;
                    tools = null;
                    return;
                }
                HediffCompProperties_VerbGiver hediffCompProperties_VerbGiver = hediffDef.CompProps<HediffCompProperties_VerbGiver>();
                if (hediffCompProperties_VerbGiver == null)
                {
                    verbs = null;
                    tools = null;
                    return;
                }
                verbs = hediffCompProperties_VerbGiver.verbs;
                tools = hediffCompProperties_VerbGiver.tools;
            }
            else
            {
                verbs = weapon.def.Verbs;
                tools = weapon.def.tools;
            }
        }

        public static HediffDef FindTechHediffHediff(ThingWithComps weapon)
        {
            List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                if (allDefsListForReading[i].addsHediff != null && allDefsListForReading[i].IsIngredient(weapon.def))
                {
                    return allDefsListForReading[i].addsHediff;
                }
            }
            return null;
        }
    }
}
