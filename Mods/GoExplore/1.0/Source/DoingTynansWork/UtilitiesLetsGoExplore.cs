using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace LetsGoExplore
{
    public class RewardGeneratorUtilityLGE
    {
        public static List<Thing> GenerateStockpileReward(float singleRewardChance)
        {
            List<Thing> returnList = new List<Thing>();

            if (Rand.Chance(singleRewardChance))
            {
                List<ThingDef> potentialList = new List<ThingDef>();
                potentialList.Add(ThingDefOf.TechprofSubpersonaCore);
                potentialList.Add(ThingDefOf.AIPersonaCore);
                potentialList.Add(ThingDefOf.MechSerumNeurotrainer);

                ThingDef rewardDef;
                potentialList.TryRandomElement(out rewardDef);
                if (rewardDef != null)
                {
                    if (rewardDef == ThingDefOf.MechSerumNeurotrainer && Rand.Chance(0.8f))
                    {
                        returnList.Add(ThingMaker.MakeThing(rewardDef));
                    }
                    returnList.Add(ThingMaker.MakeThing(rewardDef));
                }
            }
            else
            {
                if (Rand.Chance(0.30f))
                {
                    //Spawn Artifacts
                    List<ThingDef> potentialList = new List<ThingDef>();
                    potentialList.Add(ThingDefOfVanilla.PsychicAnimalPulser);
                    potentialList.Add(ThingDefOfVanilla.PsychicInsanityLance);
                    potentialList.Add(ThingDefOfVanilla.PsychicShockLance);
                    potentialList.Add(ThingDefOfVanilla.PsychicSoothePulser);

                    int artifactCount = Rand.RangeInclusive(3, 4);
                    ThingDef rewardDef;
                    for (int i = 0; i < artifactCount; i++)
                    {
                        potentialList.TryRandomElement(out rewardDef);
                        if (rewardDef != null)
                        {
                            returnList.Add(ThingMaker.MakeThing(rewardDef));
                        }
                    }
                }
                else
                {
                    if (Rand.Chance(0.25f))
                    {
                        //Spawn adv Components
                        int componentStackCount = Rand.RangeInclusive(2, 3);
                        for (int i = 0; i < componentStackCount; i++)
                        {
                            Thing reward = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
                            reward.stackCount = Rand.RangeInclusive(3, 5);
                            returnList.Add(reward);
                        }
                    }
                    else
                    {
                        //Spawn precious materials
                        List<ThingDef> potentialList = new List<ThingDef>();
                        potentialList.Add(ThingDefOf.Plasteel);
                        potentialList.Add(ThingDefOf.Uranium);
                        potentialList.Add(ThingDefOf.Gold);
                        potentialList.Add(ThingDefOf.Silver);

                        ThingDef rewardDef;
                        potentialList.TryRandomElement(out rewardDef);
                        int StackCount = Rand.RangeInclusive(4, 5);
                        if (rewardDef != null)
                        {
                            for (int i = 0; i < StackCount; i++)
                            {
                                Thing reward = ThingMaker.MakeThing(rewardDef);
                                reward.stackCount = Rand.RangeInclusive(15, 35);
                                //Larger Stackcount for Silver Stacks
                                if(rewardDef == ThingDefOf.Silver)
                                {
                                    reward.stackCount = Rand.RangeInclusive(100, 280);
                                }
                                returnList.Add(reward);
                            }
                        }
                        else
                        {
                            Log.Error("Could not resolve thingdef to spawn reward.");
                        }
                    }
                }
            }
            return returnList;
        }

        public static List<Thing> GenerateWeaponsCacheReward(int gunCount)
        {
            List<Thing> returnList = new List<Thing>();

            IEnumerable<ThingDef> weaponList = (from x in ThingSetMakerUtility.allGeneratableItems
                                                where x.weaponTags != null && (x.weaponTags.Contains("SpacerGun") || x.weaponTags.Contains("SniperRifle") || x.weaponTags.Contains("GunHeavy") || x.weaponTags.Contains("IndustrialGunAdvanced"))
                                                select x);
            for (int i = 0; i < gunCount; i++)
            {
                ThingDef thingDef;
                weaponList.TryRandomElement(out thingDef);
                if (thingDef == null)
                {
                    Log.Error("Could not resolve thingdef to spawn weapons");
                    continue;
                }
                Thing weapon = ThingMaker.MakeThing(thingDef);
                CompQuality compQuality = weapon.TryGetComp<CompQuality>();
                if (compQuality != null)
                {
                    compQuality.SetQuality(QualityUtility.GenerateQualityTraderItem(), ArtGenerationContext.Outsider);
                }
                returnList.Add(weapon);
            }

            return returnList;
        }

        public static void GenerateBuildingReward(out ThingDef rewardDef)
        {
            List<ThingDef> potentialList = new List<ThingDef>();
            potentialList.Add(ThingDefOf.PsychicEmanator);
            potentialList.Add(ThingDefOf.InfiniteChemreactor);
            potentialList.Add(ThingDefOf.VanometricPowerCell);

            potentialList.TryRandomElement(out rewardDef);
        }

        public static List<Thing> GenerateApperalReward(int apperalCount)
        {
            List<Thing> returnList = new List<Thing>();

            IEnumerable<ThingDef> apperalList = (from x in DefDatabase<ThingDef>.AllDefs
                                                 where x.IsApparel==true && x.apparel.tags != null && (x.apparel.tags.Contains("SpacerMilitary") || x.apparel.tags.Contains("IndustrialAdvanced") || x.apparel.tags.Contains("BeltDefense") || x.apparel.tags.Contains("BeltDefensePop"))
                                                select x);
            for (int i = 0; i < apperalCount; i++)
            {
                ThingDef thingDef;
                ThingDef stuffDef = null;
                if(apperalList == null)
                {
                    Log.Error("Potential apperal list count is 0");
                    break;
                }
                apperalList.TryRandomElement(out thingDef);
                if (thingDef == null)
                {
                    Log.Error("Could not resolve thingdef to spawn apperal");
                    continue;
                }
                if (thingDef.MadeFromStuff)
                {
                    if (!(from x in GenStuff.AllowedStuffsFor(thingDef, TechLevel.Undefined)
                          where !PawnWeaponGenerator.IsDerpWeapon(thingDef, x)
                          select x).TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out stuffDef))
                    {
                        stuffDef = GenStuff.RandomStuffByCommonalityFor(thingDef, TechLevel.Undefined);
                    }
                }
                Thing apperal = ThingMaker.MakeThing(thingDef, stuffDef);
                CompQuality compQuality = apperal.TryGetComp<CompQuality>();
                if (compQuality != null)
                {
                    compQuality.SetQuality(QualityUtility.GenerateQualityTraderItem(), ArtGenerationContext.Outsider);
                }
                returnList.Add(apperal);
            }
            return returnList;
        }

        public static List<Thing> GenerateStorageBoxReward()
        {
            List<Thing> returnList = new List<Thing>();

            if (Rand.Chance(0.65f)) //0.65 usually
            {
                returnList = RewardGeneratorUtilityLGE.GenerateStockpileReward(0.28f);
            }
            else
            {
                if (Rand.Chance(0.5f)) //0.5 usually
                {
                    returnList = RewardGeneratorUtilityLGE.GenerateWeaponsCacheReward(Rand.RangeInclusive(4, 5));
                }
                else
                {
                    returnList = RewardGeneratorUtilityLGE.GenerateApperalReward(Rand.RangeInclusive(4, 5));
                }
            }

            return returnList;
        }

        public static List<Thing> GenerateAmbrosia(int stockpileCount)
        {
            List<Thing> returnList = new List<Thing>();

            for (int i = 0; i < stockpileCount; i++)
            {
                Thing ambrosia = ThingMaker.MakeThing(ThingDefOfVanilla.Ambrosia);
                ambrosia.stackCount = Rand.RangeInclusive(10, 40);
                returnList.Add(ambrosia);
            }

            return returnList;
        }

        public static List<Thing> GenerateInterceptedMessageReward()
        {
            List<Thing> returnList = new List<Thing>();

            if (Rand.Chance(0.60f)) //0.65 usually
            {
                returnList = RewardGeneratorUtilityLGE.GenerateStockpileReward(0.20f);
            }
            else
            {
                if (Rand.Chance(0.5f)) //0.5 usually
                {
                    returnList = RewardGeneratorUtilityLGE.GenerateWeaponsCacheReward(Rand.RangeInclusive(3, 4));
                }
                else
                {
                    returnList = RewardGeneratorUtilityLGE.GenerateApperalReward(Rand.RangeInclusive(3, 4));
                }
            }

            return returnList;
        }
    }

    public class IncidentUtilityLGE
    {
        public static List<Thing> GenerateShellStocks(int stockpileCount)
        {
            List<Thing> returnList = new List<Thing>();

            for (int i = 0; i < stockpileCount; i++)
            {
                Thing shell = ThingMaker.MakeThing(ThingDefOf.Shell_HighExplosive);
                shell.stackCount = Rand.RangeInclusive(10, 20);
                returnList.Add(shell);
            }

            return returnList;
        }
    }
}
