using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using static PeteTimesSix.SimpleSidearms.SimpleSidearms;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace PeteTimesSix.SimpleSidearms.Utilities
{
    public static class MiscUtils
    {
        public static readonly float ANTI_OSCILLATION_FACTOR = 0.1f;

        public static bool shouldDrop(Pawn pawn, DroppingModeEnum mode, bool ignoreRecoveryChance)
        {
            bool drop;
            switch (Settings.FumbleMode)
            {
                case FumbleModeOptionsEnum.Never:
                    drop = false;
                    break;
                case FumbleModeOptionsEnum.InDistress:
                    if (mode == DroppingModeEnum.InDistress)
                        drop = true;
                    else
                        drop = false;
                    break;
                case FumbleModeOptionsEnum.InCombat:
                    if (mode == DroppingModeEnum.InDistress || mode == DroppingModeEnum.Combat)
                        drop = true;
                    else
                        drop = false;
                    break;
                case FumbleModeOptionsEnum.Always:
                default:
                    drop = true;
                    break;
            }
            if (ignoreRecoveryChance)
            {
                return drop;
            }
            else if (drop) 
            {
                var bestSkill = Math.Max(pawn.skills.GetSkill(SkillDefOf.Shooting).Level, pawn.skills.GetSkill(SkillDefOf.Melee).Level);
                var chance = Settings.FumbleRecoveryChance.Evaluate(bestSkill);
                var recovered = Rand.Chance(chance);
                return !recovered;
            }
            return false;
        }

        public static void DoNothing()
        {
        }

        public static WeaponSearchType LimitTypeToListType(WeaponListKind type)
        {
            switch (type)
            {
                case WeaponListKind.Melee:
                    return WeaponSearchType.Melee;
                case WeaponListKind.Ranged:
                    return WeaponSearchType.Ranged;
                case WeaponListKind.Both:
                default:
                    return WeaponSearchType.Both;
            }
        }
    }

}
