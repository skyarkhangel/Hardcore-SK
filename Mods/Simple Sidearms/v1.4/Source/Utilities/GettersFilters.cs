using System.Collections.Generic;
using System.Linq;
using RimWorld;
using SimpleSidearms.rimworld;
using Verse;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms.Utilities
{
    public static class GettersFilters
    {
        public static void getHeaviestWeapons(out float weightMelee, out float weightRanged)
        {
            weightMelee = float.MinValue;
            weightRanged = float.MinValue;
            foreach (ThingDefStuffDefPair weapon in ThingStuffPair.AllWith(t => t.IsWeapon).Select(t => new ThingDefStuffDefPair(t)))
            {
                if (!weapon.thing.PlayerAcquirable)
                    continue;
                float mass = weapon.thing.GetStatValueAbstract(StatDefOf.Mass);
                if (weapon.thing.IsRangedWeapon)
                {
                    if (mass > weightRanged)
                        weightRanged = mass;
                }
                else if (weapon.thing.IsMeleeWeapon)
                {
                    if (mass > weightMelee)
                        weightMelee = mass;
                }
            }
        }

        public static IEnumerable<ThingDefStuffDefPair> pregenedValidWeapons;

        public static IEnumerable<ThingDefStuffDefPair> getValidWeapons()
        {
            if (pregenedValidWeapons == null)
                pregenedValidWeapons = ThingStuffPair.AllWith(t => t.IsWeapon && t.weaponTags != null && t.PlayerAcquirable).Select(t => new ThingDefStuffDefPair(t));
            return pregenedValidWeapons;
        }

        public static IEnumerable<ThingDef> getValidWeaponsThingDefsOnly()
        {
            return getValidWeapons().ToList().ConvertAll(t => t.thing).Distinct();
        }

        public static IEnumerable<ThingDefStuffDefPair> getValidSidearms()
        {
            return getValidWeapons().Where(w => StatCalculator.isValidSidearm(w, out _));
        }

        public static IEnumerable<ThingDef> getValidSidearmsThingDefsOnly()
        {
            return getValidSidearms().ToList().ConvertAll(t => t.thing).Distinct();
        }

        public static IEnumerable<ThingDef> filterForWeaponKind(IEnumerable<ThingDef> options, WeaponSearchType type)
        {
            switch (type)
            {
                case WeaponSearchType.Melee:
                    return options.Where(t => t.IsMeleeWeapon);
                case WeaponSearchType.Ranged:
                    return options.Where(t => t.IsRangedWeapon);
                case WeaponSearchType.MeleeCapable:
                    return options.Where(t => t.IsMeleeWeapon || (t.IsWeapon && !t.tools.NullOrEmpty()));
                case WeaponSearchType.Both:
                default:
                    return options.Where(t => t.IsWeapon);
            }
        }

        public static IEnumerable<ThingDefStuffDefPair> filterForWeaponKind(IEnumerable<ThingDefStuffDefPair> options, WeaponSearchType type)
        {
            switch (type)
            {
                case WeaponSearchType.Melee:
                    return options.Where(t => t.thing.IsMeleeWeapon);
                case WeaponSearchType.Ranged:
                    return options.Where(t => t.thing.IsRangedWeapon);
                case WeaponSearchType.MeleeCapable:
                    return options.Where(t => t.thing.IsMeleeWeapon || (t.thing.IsWeapon && !t.thing.tools.NullOrEmpty()));
                case WeaponSearchType.Both:
                default:
                    return options.Where(t => t.thing.IsWeapon);
            }
        }

        public static IEnumerable<ThingWithComps> filterForWeaponKind(IEnumerable<ThingWithComps> options, WeaponSearchType type)
        {
            switch (type)
            {
                case WeaponSearchType.Melee:
                    return options.Where(t => t.def.IsMeleeWeapon);
                case WeaponSearchType.Ranged:
                    return options.Where(t => t.def.IsRangedWeapon);
                case WeaponSearchType.MeleeCapable:
                    return options.Where(t => t.def.IsMeleeWeapon || (t.def.IsWeapon && !t.def.tools.NullOrEmpty()));
                case WeaponSearchType.Both:
                default:
                    return options.Where(t => t.def.IsWeapon);
            }
        }

        public static float AverageSpeedRanged(IEnumerable<Thing> options)
        {
            int i = 0;
            float total = 0;
            foreach (Thing thing in options)
            {
                total += StatCalculator.RangedSpeed(thing as ThingWithComps);
                i++;
            }
            if (i > 0)
                return total / i;
            else
                return 0;
        }


        public static (ThingWithComps weapon, float dps, float averageSpeed) findBestRangedWeapon(Pawn pawn, LocalTargetInfo? target = null, bool skipManualUse = true, bool skipDangerous = true, bool skipEMP = true, bool includeEquipped = true)
        {
            if (pawn == null || pawn.Dead || pawn.equipment == null || pawn.inventory == null)
                return (null,-1, 0);

            IEnumerable<ThingWithComps> options = pawn.getCarriedWeapons(includeEquipped).Where(t => t.def.IsRangedWeapon);

            if (!Settings.AllowBlockedWeaponUse)
                options = options.Where(t => StatCalculator.canUseSidearmInstance(t, pawn, out _));

            options = options.Where(t => !pawn.IsColonistPlayerControlled || !isManualUse(t));

            if (skipManualUse)
                options = options.Where(t => (!isManualUse(t)));
            if (skipDangerous)
                options = options.Where(t => (!isDangerousWeapon(t)));
            if (skipEMP)
                options = options.Where(t => !isEMPWeapon(t));

            if (options.Count() == 0)
                return (null, -1, 0);

            float averageSpeed = AverageSpeedRanged(options);

            if (target.HasValue)
            {
                CellRect cellRect = (!target.Value.HasThing) ? CellRect.SingleCell(target.Value.Cell) : target.Value.Thing.OccupiedRect();
                float targetDistance = cellRect.ClosestDistSquaredTo(pawn.Position);

                options = options.Where(t =>
                {
                    VerbProperties atkProps = (t.GetComp<CompEquippable>())?.PrimaryVerb?.verbProps;
                    if (atkProps == null)
                        return false;
                    return atkProps.range >= targetDistance;
                });

                if (options.Count() == 0)
                    return (null, -1, 0);

                //TODO: handle DPS vs. armor?
                (ThingWithComps thing, float dps, float averageSpeed) best = (null, -1 , averageSpeed);
                foreach(ThingWithComps candidate in options) 
                {
                    float dps = StatCalculator.RangedDPS(candidate, Settings.SpeedSelectionBiasRanged, averageSpeed, targetDistance);
                    if(dps > best.dps) 
                    {
                        best = (candidate, dps, averageSpeed);
                    }
                }
                return best;
            }
            else
            {
                (ThingWithComps thing, float dps, float averageSpeed) best = (null, -1, averageSpeed);
                foreach (ThingWithComps candidate in options)
                {
                    float dps = StatCalculator.RangedDPSAverage(candidate, Settings.SpeedSelectionBiasRanged, averageSpeed);
                    if (dps > best.dps)
                    {
                        best = (candidate, dps, averageSpeed);
                    }
                }
                return best;
            }
        }

        public static float AverageSpeedMelee(IEnumerable<Thing> options, Pawn pawn)
        {
            int i = 0;
            float total = 0;
            foreach (Thing thing in options)
            {
                total += StatCalculator.MeleeSpeed(thing as ThingWithComps, pawn);
                i++;
            }
            if (i > 0)
                return total / i;
            else
                return 0;
        }

        public static bool findBestMeleeWeapon(Pawn pawn, out ThingWithComps result, bool includeEquipped = true, bool includeRangedWithBash = true, Pawn target = null)
        {
            result = null;
            if (pawn == null || pawn.Dead || pawn.equipment == null || pawn.inventory == null)
                return false;

            IEnumerable<ThingWithComps> options = pawn.getCarriedWeapons(includeEquipped).Where(t =>
            {
            return 
                t.def.IsMeleeWeapon ||
                (includeRangedWithBash && t.def.IsWeapon && !t.def.tools.NullOrEmpty());
            });

            if (!Settings.AllowBlockedWeaponUse)
                options = options.Where(t => StatCalculator.canUseSidearmInstance(t, pawn, out _));

            if (options.Count() < 1)
                return false;

            float averageSpeed = AverageSpeedMelee(options, pawn);

            /*if (target != null)
            {
                //handle DPS vs. armor?
            }
            else*/
            {
                float resultDPS = options.Max(t => StatCalculator.getMeleeDPSBiased(t, pawn, Settings.SpeedSelectionBiasMelee, averageSpeed));
                result = options.MaxBy(t => StatCalculator.getMeleeDPSBiased(t, pawn, Settings.SpeedSelectionBiasMelee, averageSpeed));

                //check if pawn is better when punching
                //if (pawn.GetStatValue(StatDefOf.MeleeDPS) > resultDPS)
                //    result = null;

                return true;
            }
        }

        public static bool isManualUse(ThingWithComps weapon)
        {
            if (weapon == null)
                return false;
            CompEquippable equip = weapon.TryGetComp<CompEquippable>();
            if (equip == null)
                return false;
            if (equip.PrimaryVerb.verbProps.onlyManualCast)
                return true;
            else
                return false;
        }

        public static bool isDangerousWeapon(ThingWithComps weapon)
        {
            if (weapon == null)
                return false;
            CompEquippable equip = weapon.TryGetComp<CompEquippable>();
            if (equip == null)
                return false;
            if (equip.PrimaryVerb.IsIncendiary() || equip.PrimaryVerb.verbProps.ai_IsBuildingDestroyer)
                return true;
            else
                return false;
        }

        public static bool isEMPWeapon(ThingWithComps weapon)
        {
            if (weapon == null)
                return false;
            CompEquippable equip = weapon.TryGetComp<CompEquippable>();
            if (equip == null)
                return false;
            if (equip.PrimaryVerb.IsEMP())
                return true;
            else
                return false;
        }
    }
}
