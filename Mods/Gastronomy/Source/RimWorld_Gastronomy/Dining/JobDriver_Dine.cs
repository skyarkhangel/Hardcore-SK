using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Gastronomy.Dining
{
    public class JobDriver_Dine : JobDriver
    {
        public bool wantsToOrder;
        public DiningSpot DiningSpot => job.GetTarget(SpotIndex).Thing as DiningSpot;
        public Pawn Waiter => job.GetTarget(WaiterIndex).Pawn;
        public Thing Meal => job.GetTarget(MealIndex).Thing;

        private const TargetIndex SpotIndex = TargetIndex.A;
        private const TargetIndex WaiterIndex = TargetIndex.B;
        private const TargetIndex MealIndex = TargetIndex.C;


        //public override string GetReport()
        //{
        //    //if (job?.plantDefToSow == null) return base.GetReport();
        //    return "JobDineGoReport".Translate();
        //}

        private float ChewDurationMultiplier => 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref wantsToOrder, "wantsToOrder");
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Faction != null) // Not sure why this check is needed
            {
                var diningSpot = DiningSpot;

                if (diningSpot == null || !diningSpot.Spawned || !pawn.Reserve(diningSpot, job, diningSpot.GetMaxReservations(), 0, null, errorOnFailed))
                {
                    Log.Message($"{pawn.NameShortColored} FAILED to reserve dining spot at {diningSpot.Position}.");
                    return false;
                }
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Declare these early - jumping points
            var waitForWaiter = Toils_Dining.WaitForWaiter(SpotIndex, WaiterIndex).FailOnRestaurantClosed().FailOnDangerous();
            var waitForMeal = Toils_Dining.WaitForMeal(MealIndex).FailOnDangerous();

            this.FailOn(() => DiningSpot.Destroyed);
            yield return Toils_Dining.GoToDineSpot(pawn, SpotIndex).FailOnRestaurantClosed();
            yield return Toils_Dining.TurnToEatSurface(SpotIndex);
            // Order broken? Jump straight to waiter
            yield return Toils_Jump.JumpIf(waitForWaiter, () => !pawn.GetRestaurant().Orders.CheckOrderOfWaitingPawn(pawn));
            // Already has ordered? Jump to waiting for meal
            yield return Toils_Jump.JumpIf(waitForMeal, () => pawn.GetRestaurant().Orders.GetOrderFor(pawn) != null);
            yield return waitForWaiter;
            yield return waitForMeal;
            yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, MealIndex);
            yield return Toils_Reserve.Reserve(MealIndex, 1, 1);
            yield return Toils_Dining.TurnToEatSurface(SpotIndex, MealIndex);
            yield return Toils_Dining.WaitDuringDinner(SpotIndex, 100, 250);
            yield return Toils_Ingest.ChewIngestible(pawn, ChewDurationMultiplier, MealIndex, SpotIndex);
            yield return Toils_Ingest.FinalizeIngest(pawn, MealIndex);
            yield return Toils_Dining.OnCompletedMeal(pawn);
            yield return Toils_Dining.MakeTableMessy(SpotIndex, () => pawn.Position);
            yield return Toils_Jump.JumpIf(waitForWaiter, () => pawn.needs.food.CurLevelPercentage < 0.9f);
            yield return Toils_Dining.WaitDuringDinner(SpotIndex, 100, 250);
            yield return Toils_Dining.PayDebt(pawn);
        }

        public void OnTransferredFood(Thing food)
        {
            var hasIt = pawn.inventory.Contains(food);
            if (hasIt)
            {
                //Log.Message($"{pawn.NameShortColored} has taken {food.Label} to his inventory.");
                job.SetTarget(MealIndex, food); // This triggers WaitForMeal
            }
            else
            {
                //Log.Warning($"{pawn.NameShortColored} doesn't have {food.Label} in his inventory.");
            }
        }

        // Mostly copied from JobDriver_Ingest
        public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
        {
            var placeCell = job.GetTarget(SpotIndex).Cell;
            if (pawn.pather.Moving) return false;
            Thing carriedThing = pawn.carryTracker.CarriedThing;
            if (carriedThing == null || !carriedThing.IngestibleNow) return false;
            if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable)
            {
                drawPos = new Vector3((placeCell.x + pawn.Position.x) * 0.5f + 0.5f, drawPos.y, (placeCell.z + pawn.Position.z) * 0.5f + 0.5f);
                behind = pawn.Rotation != Rot4.South;
                return true;
            }

            if (carriedThing.def.ingestible.ingestHoldOffsetStanding != null)
            {
                HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
                if (holdOffset != null)
                {
                    drawPos += holdOffset.offset;
                    behind = holdOffset.behind;
                    flip = holdOffset.flip;
                    return true;
                }
            }

            return false;
        }
    }
}
