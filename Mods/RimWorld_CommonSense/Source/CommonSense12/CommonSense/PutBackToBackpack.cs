using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace CommonSense
{
    public class PCT: Pawn_CarryTracker
    {
        public PCT(Pawn newPawn) : base(newPawn) { }
        public bool SneakyMoveToInventory(IntVec3 dropLoc, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
        {

            CompUnloadChecker cuc = CarriedThing.TryGetComp<CompUnloadChecker>();
            Thing r = null;
            bool b;
            if (cuc == null || !cuc.WasInInventory)
            {
                b = TryDropCarriedThing(dropLoc, mode, out r, placedAction);
                resultingThing = r;
                return b;
            }

            b = innerContainer.TryTransferToContainer(CarriedThing, pawn.inventory.innerContainer, true);
            resultingThing = CarriedThing;
            return b;
        }
    }

    class PutBackToBackpack
    {
        //private void CleanupCurrentJob(JobCondition condition, bool releaseReservations, bool cancelBusyStancesSoft = true)
        [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
        static class Pawn_CleanupCurrentJob_CommonSensePatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase mb)
            {
                //couldn't figure out, why I can't get a method using AccessTools.Method()
                MethodInfo m = null;
                bool b;
                foreach (var mm in typeof(Pawn_CarryTracker).GetMethods())
                {
                    if (mm.Name == "TryDropCarriedThing")
                    {
                        b = true;
                        foreach (var pp in mm.GetParameters())
                        {
                            if(pp.Name == "count")
                            {
                                b = false;
                                break;
                            }
                        }
                        if(b)
                        {
                            m = mm;
                            break;
                        }

                    }
                }
                if(m==null)
                    throw new Exception("Couldn't find TryDropCarriedThing");

                foreach (var i in instructions)
                    if (i.opcode == OpCodes.Callvirt && (MethodInfo)i.operand == m)
                        yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(PCT), nameof(PCT.SneakyMoveToInventory)));
                    else
                        yield return i;
            }
        }
    }
}
