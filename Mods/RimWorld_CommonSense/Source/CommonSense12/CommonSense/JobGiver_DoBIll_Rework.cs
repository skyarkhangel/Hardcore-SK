using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommonSense
{
    static class JobGiver_DoBIll_Rework
    {
        //protected override IEnumerable<Toil> JobDriver_DoBill.MakeNewToils()
        [HarmonyPatch(typeof(JobDriver_DoBill), "MakeNewToils")]
        static class JobDriver_DoBill_MakeNewToils_CommonSensePatch
        {
            public class JobDriver_DoBill_Access: JobDriver_DoBill
            {
                public Map MapCrutch()
                {
                    return Map;
                }
                //cloning private methods :T
                public static Toil JumpToCollectNextIntoHandsForBillCrutch(Toil gotoGetTargetToil, TargetIndex ind)
                {
                    Toil toil = new Toil();
                    toil.initAction = delegate ()
                    {
                        Pawn actor = toil.actor;
                        if (actor.carryTracker.CarriedThing == null)
                        {
                            Log.Error("JumpToAlsoCollectTargetInQueue run on " + actor + " who is not carrying something.", false);
                            return;
                        }
                        if (actor.carryTracker.Full)
                        {
                            return;
                        }
                        Job curJob = actor.jobs.curJob;
                        List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(ind);
                        if (targetQueue.NullOrEmpty())
                        {
                            return;
                        }
                        for (int i = 0; i < targetQueue.Count; i++)
                        {
                            if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
                            {
                                if (targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing))
                                {
                                    if ((actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared <= 64f)
                                    {
                                        int num = (actor.carryTracker.CarriedThing != null) ? actor.carryTracker.CarriedThing.stackCount : 0;
                                        int num2 = curJob.countQueue[i];
                                        num2 = Mathf.Min(num2, targetQueue[i].Thing.def.stackLimit - num);
                                        num2 = Mathf.Min(num2, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
                                        if (num2 > 0)
                                        {
                                            curJob.count = num2;
                                            curJob.SetTarget(ind, targetQueue[i].Thing);
                                            List<int> countQueue;
                                            int index;
                                            (countQueue = curJob.countQueue)[index = i] = countQueue[index] - num2;
                                            if (curJob.countQueue[i] <= 0)
                                            {
                                                curJob.countQueue.RemoveAt(i);
                                                targetQueue.RemoveAt(i);
                                            }
                                            actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    };
                    return toil;
                }
            }

            static IEnumerable<Toil> DoMakeToils(JobDriver_DoBill_Access __instance)
            {
                //normal scenario
                __instance.AddEndCondition(delegate
                {
                    //Log.Message("eCondition0");
                    Thing thing = __instance.GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                    if (thing is Building && !thing.Spawned)
                    {
                        return JobCondition.Incompletable;
                    }
                    return JobCondition.Ongoing;
                });
                __instance.FailOnBurningImmobile(TargetIndex.A);
                __instance.FailOn(delegate ()
                {
                    //Log.Message("FailOn00");
                    if (__instance.job.GetTarget(TargetIndex.A).Thing is Filth)
                        return false;
                    
                    IBillGiver billGiver = __instance.job.GetTarget(TargetIndex.A).Thing as IBillGiver;
                    if (billGiver != null)
                    {
                        //Log.Message("FailOn01");
                        if (__instance.job.bill.DeletedOrDereferenced)
                        {
                            return true;
                        }
                        //Log.Message("FailOn02");
                        if (!billGiver.CurrentlyUsableForBills())
                        {
                            return true;
                        }
                    }
                    return false;
                });

                Toil gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

                yield return new Toil
                {
                    initAction = delegate ()
                    {
                        if (__instance.job.targetQueueB != null && __instance.job.targetQueueB.Count == 1)
                        {
                            UnfinishedThing unfinishedThing = __instance.job.targetQueueB[0].Thing as UnfinishedThing;
                            if (unfinishedThing != null)
                            {
                                unfinishedThing.BoundBill = (Bill_ProductionWithUft)__instance.job.bill;
                            }
                        }
                    }
                };
                yield return Toils_Jump.JumpIf(gotoBillGiver, () => __instance.job.GetTargetQueue(TargetIndex.B).NullOrEmpty());

                //hauling patch
                if (Settings.adv_haul_all_ings && __instance.pawn.Faction == Faction.OfPlayer)
                {
                    Toil checklist = new Toil();
                    checklist.initAction = delegate ()
                    {
                        Pawn actor = checklist.actor;
                        Job curJob = actor.jobs.curJob;
                        List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(TargetIndex.B);
                        if (targetQueue.NullOrEmpty())
                            actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                        else
                            foreach (var target in (targetQueue))
                            {
                                if (target == null || target.Thing.DestroyedOrNull())
                                {
                                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                                    break;
                                }
                            }
                    };

                    yield return checklist;

                    Toil extract = new Toil();
                    extract.initAction = delegate ()
                    {
                        Pawn actor = extract.actor;
                        Job curJob = actor.jobs.curJob;
                        List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(TargetIndex.B);
                        if (!curJob.countQueue.NullOrEmpty())
                        {                            
                            if (curJob.countQueue[0] > targetQueue[0].Thing.stackCount)
                            {
                                actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                            }
                            else
                            {
                                curJob.SetTarget(TargetIndex.B, targetQueue[0]);
                                targetQueue.RemoveAt(0);
                                curJob.count = curJob.countQueue[0];
                                curJob.countQueue.RemoveAt(0);
                            }
                        }
                    };

                    Toil PickUpThing;
                    List<LocalTargetInfo> L = __instance.job.GetTargetQueue(TargetIndex.B);
                    if (L.Count < 2 && (L.Count == 0 || L[0].Thing.def.stackLimit < 2))
                        PickUpThing = Toils_Haul.StartCarryThing(TargetIndex.B, true, false, true);
                    else
                    {
                        PickUpThing = new Toil();
                        PickUpThing.initAction = delegate ()
                        {
                            Pawn actor = PickUpThing.actor;
                            Job curJob = actor.jobs.curJob;
                            Thing thing = curJob.GetTarget(TargetIndex.B).Thing;
                            List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(TargetIndex.B);
                            bool InventorySpawned = thing.ParentHolder == actor.inventory;
                            if (InventorySpawned || !Toils_Haul.ErrorCheckForCarry(actor, thing))
                            {
                                if (thing.stackCount < curJob.count)
                                {
                                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                                }
                                else
                                {
                                    Thing splitThing = thing.SplitOff(curJob.count);
                                    if (splitThing.ParentHolder != actor.inventory && !actor.inventory.GetDirectlyHeldThings().TryAdd(splitThing, false))
                                    {
                                        actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                                    }


                                    if (!splitThing.Destroyed && splitThing.stackCount != 0)
                                    {
                                        targetQueue.Add(splitThing);

                                        if (!InventorySpawned)
                                        {
                                            CompUnloadChecker CUC = splitThing.TryGetComp<CompUnloadChecker>();
                                            if (CUC != null) CUC.ShouldUnload = true;
                                        }
                                    }

                                    if (splitThing != thing && actor.Map.reservationManager.ReservedBy(thing, actor, curJob))
                                    {
                                        actor.Map.reservationManager.Release(thing, actor, curJob);
                                    }

                                }
                            }
                        };
                    }

                    Toil TakeToHands = new Toil();
                    TakeToHands.initAction = delegate ()
                    {
                        Pawn actor = TakeToHands.actor;
                        Job curJob = actor.jobs.curJob;
                        List<LocalTargetInfo> targetQueue = curJob.GetTargetQueue(TargetIndex.B);
                        if (!targetQueue.NullOrEmpty() && targetQueue[0].Thing.ParentHolder != actor.carryTracker)
                        {
                            actor.inventory.innerContainer.TryTransferToContainer(targetQueue[0].Thing, actor.carryTracker.innerContainer);
                            actor.Reserve(targetQueue[0], curJob);
                            curJob.SetTarget(TargetIndex.B, targetQueue[0]);
                            targetQueue.RemoveAt(0);
                        }
                    };

                    yield return extract;
                    Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
                    yield return Toils_Jump.JumpIf(PickUpThing, () => __instance.job.GetTarget(TargetIndex.B).Thing.ParentHolder == __instance.pawn.inventory);
                    yield return getToHaulTarget;
                    yield return PickUpThing;
                    yield return Toils_Jump.JumpIf(extract, () => !__instance.job.countQueue.NullOrEmpty());
                    yield return TakeToHands;
                    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.B);
                    Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
                    yield return findPlaceTarget;
                    yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, false);
                    yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, TakeToHands);
                }
                else
                {
                    Toil extract = Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.B, true);
                    yield return extract;
                    Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
                    yield return getToHaulTarget;
                    yield return Toils_Haul.StartCarryThing(TargetIndex.B, true, false, true);
                    yield return JobDriver_DoBill_Access.JumpToCollectNextIntoHandsForBillCrutch(getToHaulTarget, TargetIndex.B);
                    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDestroyedOrNull(TargetIndex.B);
                    Toil findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(TargetIndex.A, TargetIndex.B, TargetIndex.C);
                    yield return findPlaceTarget;
                    yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, findPlaceTarget, false);
                    yield return Toils_Jump.JumpIfHaveTargetInQueue(TargetIndex.B, extract);
                }

                yield return gotoBillGiver; //one line from normal scenario
                
                //cleaning patch
                if (Settings.adv_cleaning && !Utility.IncapableOfCleaning(__instance.pawn))
                {
                    Toil returnToBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                    Toil FilthList = new Toil();
                    FilthList.initAction = delegate ()
                    {
                        Job curJob = FilthList.actor.jobs.curJob;
                        if (curJob.GetTargetQueue(TargetIndex.A).NullOrEmpty())
                        {
                            LocalTargetInfo A = curJob.GetTarget(TargetIndex.A);
                            IEnumerable<Filth> l = Utility.SelectAllFilth(FilthList.actor, A, Settings.adv_clean_num);
                            Utility.AddFilthToQueue(curJob, TargetIndex.A, l, FilthList.actor);
                            FilthList.actor.ReserveAsManyAsPossible(curJob.GetTargetQueue(TargetIndex.A), curJob);
                            curJob.targetQueueA.Add(A);
                        }
                    };
                    yield return FilthList;
                    yield return Toils_Jump.JumpIf(returnToBillGiver, () => __instance.job.GetTargetQueue(TargetIndex.A).NullOrEmpty());
                    Toil CleanFilthList = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets(TargetIndex.A, null);
                    yield return CleanFilthList;
                    yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A, true);
                    yield return Toils_Jump.JumpIf(returnToBillGiver, () => __instance.job.GetTargetQueue(TargetIndex.A).NullOrEmpty());
                    yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, CleanFilthList).JumpIfOutsideHomeArea(TargetIndex.A, CleanFilthList);
                    Toil clean = new Toil();
                    clean.initAction = delegate ()
                    {
                        Filth filth = __instance.job.GetTarget(TargetIndex.A).Thing as Filth;
                        __instance.billStartTick = 0;
                        __instance.ticksSpentDoingRecipeWork = 0;
                        __instance.workLeft = filth.def.filth.cleaningWorkToReduceThickness * filth.thickness;
                    };
                    clean.tickAction = delegate ()
                    {
                        Filth filth = __instance.job.GetTarget(TargetIndex.A).Thing as Filth;
                        __instance.billStartTick += 1;
                        __instance.ticksSpentDoingRecipeWork += 1;
                        if (__instance.billStartTick > filth.def.filth.cleaningWorkToReduceThickness)
                        {
                            filth.ThinFilth();
                            __instance.billStartTick = 0;
                            if (filth.Destroyed)
                            {
                                clean.actor.records.Increment(RecordDefOf.MessesCleaned);
                                __instance.ReadyForNextToil();
                                return;
                            }
                        }
                    };
                    clean.defaultCompleteMode = ToilCompleteMode.Never;
                    clean.WithEffect(EffecterDefOf.Clean, TargetIndex.A);
                    clean.WithProgressBar(TargetIndex.A, () => __instance.ticksSpentDoingRecipeWork / __instance.workLeft, true, -0.5f);
                    clean.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
                    clean.JumpIfDespawnedOrNullOrForbidden(TargetIndex.A, FilthList);
                    clean.JumpIfOutsideHomeArea(TargetIndex.A, FilthList);
                    yield return clean;
                    yield return Toils_Jump.Jump(FilthList);
                    yield return returnToBillGiver;
                }

                //continuation of normal scenario
                yield return Toils_Recipe.MakeUnfinishedThingIfNeeded();
                yield return Toils_Recipe.DoRecipeWork().FailOnDespawnedNullOrForbiddenPlacedThings().FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
                yield return Toils_Recipe.FinishRecipeAndStartStoringProduct();
                if (!__instance.job.RecipeDef.products.NullOrEmpty() || !__instance.job.RecipeDef.specialProducts.NullOrEmpty())
                {
                    yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
                    Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
                    yield return carryToCell;
                    yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
                    Toil recount = new Toil();
                    recount.initAction = delegate ()
                    {
                        Bill_Production bill_Production = recount.actor.jobs.curJob.bill as Bill_Production;
                        if (bill_Production != null && bill_Production.repeatMode == BillRepeatModeDefOf.TargetCount)
                        {
                            __instance.MapCrutch().resourceCounter.UpdateResourceCounts();
                        }
                    };
                    yield return recount;
                }
                yield break;
            }

            static bool Prefix(ref IEnumerable<Toil> __result, ref JobDriver_DoBill_Access __instance)
            {
                if (!Settings.adv_cleaning && !Settings.adv_haul_all_ings)
                    return true;

                __result = DoMakeToils(__instance);
                return false;
            }
        }
    }
}