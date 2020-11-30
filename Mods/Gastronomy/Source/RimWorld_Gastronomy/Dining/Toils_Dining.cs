using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Gastronomy.Dining
{
    public static class Toils_Dining
    {
        public static Toil GoToDineSpot(Pawn pawn, TargetIndex dineSpotInd)
        {
            var toil = new Toil();
            toil.initAction = () => {
                Pawn actor = toil.actor;
                IntVec3 targetPosition = IntVec3.Invalid;
                var diningSpot = (DiningSpot) actor.CurJob.GetTarget(dineSpotInd).Thing;

                bool BaseChairValidator(Thing t)
                {
                    if (t.def.building == null || !t.def.building.isSittable) return false;

                    if (t.IsForbidden(pawn)) return false;

                    if (!actor.CanReserve(t)) return false;

                    if (!t.IsSociallyProper(actor)) return false;

                    if (t.IsBurning()) return false;

                    if (t.HostileTo(pawn)) return false;

                    return true;
                }

                var chair = GenClosest.ClosestThingReachable(diningSpot.Position, diningSpot.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.OnCell, TraverseParms.For(actor), 2,
                    t => BaseChairValidator(t) && t.Position.GetDangerFor(pawn, t.Map) == Danger.None);
                if (chair == null)
                {
                    Log.Message($"{pawn.NameShortColored} could not find a chair around {diningSpot.Position}.");
                    if (diningSpot.MayDineStanding)
                    {
                        targetPosition = RCellFinder.SpotToChewStandingNear(actor, diningSpot);
                        var chewSpotDanger = targetPosition.GetDangerFor(pawn, actor.Map);
                        if (chewSpotDanger != Danger.None)
                        {
                            Log.Message($"{pawn.NameShortColored} could not find a save place around {diningSpot.Position} ({chewSpotDanger}).");
                            actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                            return;
                        }
                    }
                }

                if (chair != null)
                {
                    targetPosition = chair.Position;
                    actor.Reserve(chair, actor.CurJob);
                }

                actor.Map.pawnDestinationReservationManager.Reserve(actor, actor.CurJob, targetPosition);
                actor.pather.StartPath(targetPosition, PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        public static Toil TurnToEatSurface(TargetIndex eatSurfaceInd, TargetIndex foodInd = TargetIndex.None)
        {
            var toil = new Toil();
            toil.initAction = delegate {
                toil.actor.jobs.curDriver.rotateToFace = eatSurfaceInd;
                if (foodInd != TargetIndex.None)
                {
                    var thing = toil.actor.CurJob.GetTarget(foodInd).Thing;
                    if (thing?.def.rotatable == true)
                    {
                        thing.Rotation = Rot4.FromIntVec3(toil.actor.CurJob.GetTarget(eatSurfaceInd).Cell - toil.actor.Position);
                    }
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil WaitForWaiter(TargetIndex diningSpotInd, TargetIndex waiterInd)
        {
            var toil = new Toil();
            toil.initAction = () => GetDriver(toil).wantsToOrder = true;
            toil.tickAction = () => {
                if (diningSpotInd != 0 && toil.actor.CurJob.GetTarget(diningSpotInd).IsValid)
                {
                    toil.actor.rotationTracker.FaceCell(toil.actor.CurJob.GetTarget(diningSpotInd).Cell);
                }
                if(!GetDriver(toil).wantsToOrder) GetDriver(toil).ReadyForNextToil();
            };
            toil.AddFinishAction(() => GetDriver(toil).wantsToOrder = false);

            toil.defaultDuration = 1500;
            //toil.WithProgressBarToilDelay(TargetIndex.A); // TODO: Turn this off later? Or make it go backwards?
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOnDestroyedOrNull(diningSpotInd);
            toil.FailOnDurationExpired(); // Duration over? Fail job!
            toil.socialMode = RandomSocialMode.Normal;
            return toil;
        }

        private static JobDriver_Dine GetDriver(Toil t) => t.actor.jobs.curDriver as JobDriver_Dine;

        public static Toil WaitForMeal(TargetIndex mealInd)
        {
            var toil = new Toil();
            toil.initAction = () => {
                var order = toil.actor.GetRestaurant().Orders.GetOrderFor(toil.actor);
                if (order?.delivered == true && (order.consumable?.Spawned == true || order.consumable?.ParentHolder == toil.actor.inventory))
                {
                    var food = order.consumable;
                    toil.actor.CurJob.SetTarget(mealInd, food);
                    Log.Message($"{toil.actor.NameShortColored} has already received order: {food.Label}");
                    if (toil.actor.inventory.Contains(food))
                    {
                        //Log.Message($"{toil.actor.NameShortColored} has {food.Label} in inventory.");
                        GetDriver(toil).ReadyForNextToil();
                    }
                    else if (food.Position.AdjacentTo8Way(toil.actor.Position))
                    {
                        //Log.Message($"{toil.actor.NameShortColored} has {food.Label} on table.");
                        food.DeSpawn();
                        //var amount = toil.actor.inventory.innerContainer.TryAdd(order.consumable, 1, false);
                        //Log.Message($"{toil.actor.NameShortColored} received {amount} of {food.LabelShort} to his inventory.");
                        GetDriver(toil).ReadyForNextToil();
                    }
                    else
                    {
                        Log.Message($"{toil.actor.NameShortColored}'s food is somewhere else ({food?.Position}). Will wait.");
                        toil.actor.CurJob.SetTarget(mealInd, null);
                        order.delivered = false;
                    }
                }
                else if (order?.delivered == true)
                {
                    // Order not spawned? Already eaten it, or something happened to it
                    // Let it go.
                    Log.Warning($"{toil.actor.NameShortColored}'s food is gone. Already eaten?");
                    GetDriver(toil).EndJobWith(JobCondition.QueuedNoLongerValid);
                }
            };
            toil.tickAction = () => {
                var target = toil.actor.CurJob.GetTarget(mealInd);
                if(target.HasThing && target.IsValid && target.Thing.ParentHolder == toil.actor.inventory) GetDriver(toil).ReadyForNextToil();
            };
            toil.defaultDuration = 1500;
            //toil.WithProgressBarToilDelay(TargetIndex.A); // TODO: Turn this off later? Or make it go backwards?
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.FailOnDurationExpired(); // Duration over? Fail job!
            toil.socialMode = RandomSocialMode.Normal;
            return toil;
        }

        public static Toil WaitDuringDinner(TargetIndex lookAtInd, int minDuration, int maxDuration)
        {
            var toil = Toils_General.Wait(Rand.Range(minDuration, maxDuration), lookAtInd);
            toil.socialMode = RandomSocialMode.Normal;
            return toil;
        }

        public static Toil MakeTableMessy(TargetIndex diningSpotInd, Func<IntVec3> patronPos)
        {
            var toil = new Toil {atomicWithPrevious = true};
            toil.initAction = () => {
                if (toil.actor.CurJob.GetTarget(diningSpotInd).Thing is DiningSpot diningSpot)
                {
                    diningSpot.SetSpotMessy(patronPos.Invoke());
                }
            };
            return toil;
        }

        public static Toil OnCompletedMeal(Pawn pawn)
        {
            return new Toil {atomicWithPrevious = true, initAction = () => { pawn.GetRestaurant().Orders.OnFinishedEatingOrder(pawn); }};
        }

        public static Toil PayDebt(Pawn pawn)
        {
            return new Toil {atomicWithPrevious = true, initAction = () => {
                var debt = pawn.GetRestaurant().Debts.GetDebt(pawn);
                if (debt == null) return;

                var debtAmount = Mathf.FloorToInt(debt.amount);
                if (debtAmount < 0) return;
                var cash = pawn.inventory.innerContainer.FirstOrDefault(t => t.def == ThingDefOf.Silver);
                var payAmount = Mathf.Min(cash.stackCount, debtAmount);
                pawn.inventory.innerContainer.TryDrop(cash, ThingPlaceMode.Near, payAmount, out var droppedSilver);
                pawn.GetRestaurant().Debts.PayDebt(pawn, payAmount);
            }};
        }
    }
}
