using Core_SK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
namespace Core_SK
{
    public static class Upgrades
    {
        public static void ConstructionI()
        {
            DefDatabase<ThingDef>.GetNamed("SCWall").stuffCategories.Add(DefDatabase<StuffCategoryDef>.GetNamed("Metallic"));
            DefDatabase<ThingDef>.GetNamed("SCWall").stuffCategories.Add(DefDatabase<StuffCategoryDef>.GetNamed("Stony"));
            DefDatabase<ThingDef>.GetNamed("Door").stuffCategories.Add(DefDatabase<StuffCategoryDef>.GetNamed("Metallic"));
            DefDatabase<ThingDef>.GetNamed("SCWall").SetStatBaseValue(StatDef.Named("MaxHitPoints"), 250);
        }
        public static void ConstructionII()
        {
            DefDatabase<ThingDef>.GetNamed("SCWall").SetStatBaseValue(StatDef.Named("MaxHitPoints"), 275);
        }
        public static void ConstructionIII()
        {
            DefDatabase<ThingDef>.GetNamed("SCWall").SetStatBaseValue(StatDef.Named("MaxHitPoints"), 300);
        }
        public static void ConstructionIV()
        {
            DefDatabase<ThingDef>.GetNamed("SCWall").SetStatBaseValue(StatDef.Named("MaxHitPoints"), 325);
        }
        public static void PowerII()
        {
            DefDatabase<ThingDef>.GetNamed("Battery").comps.First<CompProperties>().efficiency = .65f;
        }
        public static void PowerIII()
        {
            DefDatabase<ThingDef>.GetNamed("Battery").comps.First<CompProperties>().efficiency = .80f;
        }
        public static void PowerIV()
        {
            DefDatabase<ThingDef>.GetNamed("Battery").comps.First<CompProperties>().efficiency = .95f;
        }
        public static void NutrientResynthesisII()
        {
            DefDatabase<ThingDef>.GetNamed("NutrientPasteDispenser").building.foodCostPerDispense = 12;
        }
        public static void ProteinReplication()
        {
            ThingDefOf.MealNutrientPaste = ThingDefOf.MealSimple;
        }
        public static void SentryGunUpgradeBarrelI()
        {
            DefDatabase<ThingDef>.GetNamed("Gun_TurretImprovised").Verbs.First<VerbProperties>().range = 40;
            DefDatabase<ThingDef>.GetNamed("Bullet_TurretImprovised").projectile.damageAmountBase = 15;
            DefDatabase<ThingDef>.GetNamed("Bullet_TurretImprovised").projectile.speed = 140;
        }
        public static void SentryGunUpgradeBarrelII()
        {
            DefDatabase<ThingDef>.GetNamed("Gun_TurretImprovised").Verbs.First<VerbProperties>().range = 50;
            DefDatabase<ThingDef>.GetNamed("Bullet_TurretImprovised").projectile.damageAmountBase = 18;
            DefDatabase<ThingDef>.GetNamed("Bullet_TurretImprovised").projectile.speed = 150;
        }
        public static void SentryGunUpgradeCoolingI()
        {
            DefDatabase<ThingDef>.GetNamed("TurretGun").building.turretBurstCooldownTicks = 160;
        }
        public static void SentryGunUpgradeCoolingII()
        {
            DefDatabase<ThingDef>.GetNamed("TurretGun").building.turretBurstCooldownTicks = 140;
        }
        public static void SentryGunUpgradeLoaderI()
        {
            DefDatabase<ThingDef>.GetNamed("Gun_TurretImprovised").Verbs.First<VerbProperties>().burstShotCount = 4;
            DefDatabase<ThingDef>.GetNamed("Gun_TurretImprovised").Verbs.First<VerbProperties>().forcedMissRadius = 1.25f;
        }
        public static void SentryGunUpgradeLoaderII()
        {
            DefDatabase<ThingDef>.GetNamed("Gun_TurretImprovised").Verbs.First<VerbProperties>().burstShotCount = 5;
            DefDatabase<ThingDef>.GetNamed("Gun_TurretImprovised").Verbs.First<VerbProperties>().forcedMissRadius = 1.5f;
        }
    }
}

