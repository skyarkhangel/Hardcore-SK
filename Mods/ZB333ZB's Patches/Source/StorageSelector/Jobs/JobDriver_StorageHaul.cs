using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace StorageSelector.Jobs
{
    public class JobDriver_StorageHaul : JobDriver
    {
        private const TargetIndex ItemToHaulIndex = TargetIndex.A;
        private const TargetIndex StorageBuildingIndex = TargetIndex.B;

        protected Thing ItemToHaul => job.GetTarget(ItemToHaulIndex).Thing;
        protected Building_Storage StorageBuilding => job.GetTarget(StorageBuildingIndex).Thing as Building_Storage;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(ItemToHaul, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(StorageBuilding, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(ItemToHaulIndex);
            this.FailOnDestroyedOrNull(StorageBuildingIndex);
            this.FailOnBurningImmobile(StorageBuildingIndex);

            yield return Toils_Goto.GotoThing(ItemToHaulIndex, PathEndMode.ClosestTouch)
                .FailOnSomeonePhysicallyInteracting(ItemToHaulIndex);

            yield return Toils_Haul.StartCarryThing(ItemToHaulIndex);

            var findCell = new Toil
            {
                initAction = () =>
                {
                    var storage = StorageBuilding;
                    if (storage == null)
                    {
                        Log.Error("StorageBuilding is null in JobDriver_StorageHaul");
                        return;
                    }

                    var slotGroup = storage.GetSlotGroup();
                    if (slotGroup == null)
                    {
                        Log.Error("SlotGroup is null in JobDriver_StorageHaul");
                        return;
                    }

                    IntVec3 cell = IntVec3.Invalid;
                    foreach (var c in slotGroup.CellsList)
                    {
                        if (StoreUtility.IsGoodStoreCell(c, Map, ItemToHaul, pawn, pawn.Faction))
                        {
                            cell = c;
                            break;
                        }
                    }

                    if (!cell.IsValid)
                    {
                        Log.Error("Could not find valid storage cell in JobDriver_StorageHaul");
                        return;
                    }

                    job.SetTarget(TargetIndex.C, cell);
                }
            };
            yield return findCell;

            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C);

            var placeInCell = new Toil
            {
                initAction = () =>
                {
                    var thing = ItemToHaul;
                    var cell = job.GetTarget(TargetIndex.C).Cell;
                    if (thing != null && cell.IsValid)
                    {
                        thing.Position = cell;
                        thing.SpawnSetup(Map, false);
                    }
                }
            };
            yield return placeInCell;
        }
    }
}
