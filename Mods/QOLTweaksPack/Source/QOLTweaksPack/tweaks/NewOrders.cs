using Harmony;
using HugsLib.Utils;
using QOLTweaksPack.rimworld;
using QOLTweaksPack.utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace QOLTweaksPack.tweaks
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    static class FloatMenuMakerMap_AddHumanLikeOrders_Postfix
    {
        [HarmonyPostfix]
        private static void AddHumanlikeOrders(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (QOLTweaksPack.MoveOrder.Value == true)
                DoMoveOrder(clickPos, pawn, opts);

            if (QOLTweaksPack.PickupDropOrders.Value == true)
                DoPickupOrder(clickPos, pawn, opts);
            if (QOLTweaksPack.PickupDropOrders.Value == true)
                DoDropOrder(clickPos, pawn, opts);

        }

        private static void DoMoveOrder(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (!HugsLibUtility.ShiftIsHeld)
                return;

            FloatMenuOption item;
            IntVec3 c = IntVec3.FromVector3(clickPos);
            string text = "MoveToOrder".Translate();

            item = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(text, delegate
            {
                pawn.jobs.TryTakeOrderedJob(new Job(JobDefOf.Goto, c), JobTag.Misc);
                MoteMaker.MakeStaticMote(c, pawn.Map, ThingDefOf.Mote_FeedbackGoto, 1f);
            }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, c, "ReservedBy");
            opts.Add(item);
        }

        internal static bool forceKeep = false;

        private static void DoDropOrder(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (!HugsLibUtility.ShiftIsHeld)
                return;

            if (pawn.carryTracker.CarriedThing != null && pawn.jobs.curDriver is JobDriver_PickupObject)
            {
                IntVec3 c = IntVec3.FromVector3(clickPos);
                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("DropCarriedAt".Translate(), delegate
                {
                    Job job = new Job(NewOrderJobDefOf.QOL_DropObject, c);
                    forceKeep = true;
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    forceKeep = false;
                }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, c, "ReservedBy"));
            }
        }

        private static void DoPickupOrder(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            if (!HugsLibUtility.ShiftIsHeld)
                return;

            IntVec3 c = IntVec3.FromVector3(clickPos);
            Thing item = c.GetFirstItem(pawn.Map);
            if (item != null && item.def.EverHaulable)
            {
                if (!pawn.CanReach(item, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    opts.Add(new FloatMenuOption("CannotPickUp".Translate(new object[]
                    {
                            item.Label
                    }) + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                else if (MassUtility.WillBeOverEncumberedAfterPickingUp(pawn, item, 1))
                {
                    opts.Add(new FloatMenuOption("CannotPickUp".Translate(new object[]
                    {
                            item.Label
                    }) + " (" + "TooHeavy".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                else if (item.stackCount == 1)
                {
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUp".Translate(new object[]
                    {
                            item.Label
                    }), delegate
                    {
                        item.SetForbidden(false, false);
                        Job job = new Job(NewOrderJobDefOf.QOL_PickupObject, item);
                        job.count = 1;
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
                }
                else
                {
                    opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("PickUpAll".Translate(new object[]
                        {
                                item.Label
                        }), delegate
                        {
                            item.SetForbidden(false, false);
                            Job job = new Job(NewOrderJobDefOf.QOL_PickupObject, item);
                            job.count = item.stackCount;
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        }, MenuOptionPriority.High, null, null, 0f, null, null), pawn, item, "ReservedBy"));
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
    static class Pawn_JobTracker_CleanupCurrentJob_PrefixReplacement
    {
        [HarmonyPrefix]
        public static bool CleanupCurrentJob(Pawn_JobTracker __instance, JobCondition condition, bool releaseReservations, bool cancelBusyStancesSoft = true)
        {
            if (QOLTweaksPack.PickupDropOrders.Value == false)
                return true;

            if (!FloatMenuMakerMap_AddHumanLikeOrders_Postfix.forceKeep)
                return true;

            if (!(__instance.curDriver is JobDriver_PickupObject))
                return true;

            if (__instance.curJob == null)
                return true;

            if (__instance.debugLog)
            {
                __instance.DebugLogEvent(string.Concat(new object[]
                {
                    "CleanupCurrentJob ",
                    (__instance.curJob == null) ? "null" : __instance.curJob.def.ToString(),
                    " condition ",
                    condition
                }));
            }

            Pawn pawn = (Pawn)Reflection.GetFieldValue(__instance, "pawn");

            
            __instance.curDriver.ended = true;
            __instance.curDriver.Cleanup(condition);
            __instance.curDriver = null;
            __instance.curJob = null;
            if (releaseReservations)
            {
                pawn.ClearReservations(false);
            }
            if (cancelBusyStancesSoft)
            {
                pawn.stances.CancelBusyStanceSoft();
            }
            if (!pawn.Destroyed && pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
            {
                //Thing thing;
                //pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out thing, null);
                        //Do not do this thing
            }

            return false;
        }
    }
}
