using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using Verse.AI;

namespace AnimalsLogic.Patches
{
    /*
     * Adds more awareness about predators. Predator hunting one of your pawns (humanlike or animal) is considered hostile to whole your faction, like manhunter would be.
     */
    class HostilePredators
    {
        public static void Patch()
        {
            // removes range limit so predators are not only recognized on pouncing range
            /*
            AnimalsLogic.harmony.Patch(
                AccessTools.Method(typeof(GenHostility), "IsPredatorHostileTo", new Type[] { typeof(Pawn), typeof(Pawn) }),
                transpiler: new HarmonyMethod(typeof(HostilePredators).GetMethod(nameof(IsPredatorHostileTo_Transpiler)))
                );
            */

            // fixes turrets vs predators - vanilla method only works for pawns because it has an explicit check if both actors are pawns
            AnimalsLogic.harmony.Patch(
                AccessTools.Method(typeof(GenHostility), "HostileTo", new Type[] { typeof(Thing), typeof(Thing) }),
                postfix: new HarmonyMethod(typeof(HostilePredators).GetMethod(nameof(HostileToThing_Postfix)))
                );

            // experimental
            /*
            AnimalsLogic.harmony.Patch(
                typeof(GenHostility).GetMethod("HostileTo", new Type[] { typeof(Thing), typeof(Faction) }),
                postfix: new HarmonyMethod(typeof(GenHostility_IsPredatorHostileTo_Patch).GetMethod(nameof(HostileToFaction_Postfix)))
                );
            */
            //AnimalsLogic.harmony.Patch(
            //    typeof(GenHostility).GetMethod("IsActiveThreatTo", new Type[] { typeof(IAttackTarget), typeof(Faction) }),
            //    postfix: new HarmonyMethod(typeof(GenHostility_IsPredatorHostileTo_Patch).GetMethod(nameof(IsActiveThreatTo_Postfix)))
            //    );
        }
        /*
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> IsPredatorHostileTo_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                // 	IL_002b: callvirt instance valuetype Verse.IntVec3 Verse.Thing::get_Position()
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand == typeof(Thing).GetMethod("get_Position"))
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
                {
                    codes.RemoveRange(i - 1, 10);
                    break;
                }
            }

            return codes.AsEnumerable();
        }
        */

        [HarmonyPostfix]
        public static void HostileToThing_Postfix(ref bool __result, Thing a, Thing b)
        {
            if (__result == false && Settings.hostile_predators)
            {
                if (CheckHostile(a, b) || CheckHostile(b, a))
                {
                    __result = true;
                }
            }
        }
        private static bool CheckHostile(Thing who, Thing to)
        {
            if (!(who is Pawn) || to.Faction == null)
            {
                return false;
            }

            Pawn agressor = who as Pawn;

            if (to.Faction.HasPredatorRecentlyAttackedAnyone(agressor) || GetPreyOfMyFaction(agressor, to.Faction) != null)
            {
                return true;
            }

            return false;
        }

        // copy-paste from GenHostility
        private static Pawn GetPreyOfMyFaction(Pawn predator, Faction myFaction)
        {
            Job curJob = predator.CurJob;
            if (curJob != null && curJob.def == JobDefOf.PredatorHunt && !predator.jobs.curDriver.ended)
            {
                Pawn pawn = curJob.GetTarget(TargetIndex.A).Thing as Pawn;
                if (pawn != null && pawn.Faction == myFaction)
                {
                    return pawn;
                }
            }
            return null;
        }

        /*
        static void HostileToFaction_Postfix(ref bool __result, Thing a, Thing b)
        {
            if (__result == false && Settings.hostile_predators)
            {
                if (CheckHostile(a, b) || CheckHostile(b, a))
                {
                    __result = true;
                }
            }
        }

        private static bool CheckHostile(Thing who, Faction to)
        {
            if (!(who is Pawn) || to.Faction == null)
            {
                return false;
            }

            Pawn agressor = who as Pawn;

            if (to.Faction.HasPredatorRecentlyAttackedAnyone(agressor) || GetPreyOfMyFaction(agressor, to.Faction) != null)
            {
                return true;
            }

            return false;
        }

        static void IsActiveThreatTo_Postfix(ref bool __result, Thing a, Thing b)
        {
        }
        */
    }
}
