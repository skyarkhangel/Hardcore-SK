using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace Revolus.AnimalsAreFun {
    public static class Common {
        public const float MinConsciousnessDefault = 0.6f;
        public const float MinMovingDefault = 0.7f;
        public const float MaxBodySizeDefault = 1.6f;
        public const float MaxWildnessDefault = 0.8f;
        public const bool MustBeCuteDefault = true;
        
        public static float MinConsciousness = MinConsciousnessDefault;
        public static float MinMoving = MinMovingDefault;
        public static float MaxBodySize = MaxBodySizeDefault;
        public static float MaxWildness = MaxWildnessDefault;
        public static bool MustBeCute = MustBeCuteDefault;

        public static bool PawnOrAnimalIsIncapable(Pawn pawn) {
            var caps = pawn?.health?.capacities;
            if (caps is null) {
                AnimalsAreFun.Debug($"no health capatibilities: {pawn.ToStringSafe()}");
                return true;  // impossible?
            } else if (caps.GetLevel(PawnCapacityDefOf.Consciousness) < MinConsciousness) {
                AnimalsAreFun.Debug($"not enough Consciousness: {pawn.ToStringSafe()}");
                return true;  // too stupid
            } else if (caps.GetLevel(PawnCapacityDefOf.Moving) < MinMoving) {
                AnimalsAreFun.Debug($"not enough Moving: {pawn.ToStringSafe()}");
                return true;  // walking is difficult
            } else {
                return false;
            }
        }

        public static bool AnimalRaceIsGood(Pawn animal) {
            var race = animal?.def?.race;
            if (race is null) {
                AnimalsAreFun.Debug($"no race: {animal.ToStringSafe()}");
                return false;  // impossible
            } else if (!race.Animal) {
                AnimalsAreFun.Debug($"not an animal: {animal.ToStringSafe()}");
                return false;
            } else if (race.Humanlike) {
                AnimalsAreFun.Debug($"humanlike: {animal.ToStringSafe()}");
                return false;
            } else if (race.FleshType != FleshTypeDefOf.Normal) {
                AnimalsAreFun.Debug($"not flesh: {animal.ToStringSafe()}");
                return false;  // not an animal
            } else if (race.baseBodySize > MaxBodySize) {
                AnimalsAreFun.Debug($"too big: {animal.ToStringSafe()}");
                return false; // too big
            } else if (race.wildness > MaxWildness) {
                AnimalsAreFun.Debug($"too wild: {animal.ToStringSafe()}");
                return false;  // too wild
            } else if (!MustBeCute && race.nuzzleMtbHours < 0f) {
                AnimalsAreFun.Debug($"not cute: {animal.ToStringSafe()}");
                return false;  // not cute
            } else {
                return true;
            }
        }

        public static bool PawnOrAnimalIsGone(Pawn p) {
            if (p.DestroyedOrNull()) {
                AnimalsAreFun.Debug($"destroyed or null: {p.ToStringSafe()}");
                return true;
            } else if (p.Dead) {
                AnimalsAreFun.Debug($"dead: {p.ToStringSafe()}");
                return true;
            } else if (!p.Spawned) {
                AnimalsAreFun.Debug($"not spawned: {p.ToStringSafe()}");
                return true;
            } else {
                return false;
            }
        }

        public static bool PawnOrAnimalIsGoneOrIncapable(Pawn p) {
            if (PawnOrAnimalIsGone(p)) {
                return true;
            } else if (PawnOrAnimalIsIncapable(p)) {
                return true;
            } else {
                return false;
            }
        }

        public static bool PawnMayEnjoyPlayingOutside(Pawn pawn) {
            if (PawnOrAnimalIsGoneOrIncapable(pawn)) {
                return false;
            } else if (pawn.MapHeld is null) {
                AnimalsAreFun.Debug($"MapHeld is null: {pawn.ToStringSafe()}");
                return false;  // impossible?
            } else if (!pawn.IsColonist) {
                AnimalsAreFun.Debug($"not a colonist: {pawn.ToStringSafe()}");
                return false;  // prisoners shouldn't be interacting with our pets
            } else if (!JoyUtility.EnjoyableOutsideNow(pawn)) {
                AnimalsAreFun.Debug($"doesn't want to have fun outside: {pawn.ToStringSafe()}");
                return false;  // world's on fire?
            } else if (PawnUtility.WillSoonHaveBasicNeed(pawn)) {
                AnimalsAreFun.Debug($"will soon have basic need: {pawn.ToStringSafe()}");
                return false;  // too sleepy or hungry
            } else {
                return true;
            }
        }

        public static bool AnimalIsGood(Pawn animal) {
            if (PawnOrAnimalIsGoneOrIncapable(animal)) {
                return false;
            } else if (!AnimalRaceIsGood(animal)) {
                return false;
            } else if (PawnUtility.WillSoonHaveBasicNeed(animal)) {
                AnimalsAreFun.Debug($"will soon have basic need: {animal.ToStringSafe()}");
                return false;  // too hungry or sleepy
            } else if (TimetableUtility.GetTimeAssignment(animal) != TimeAssignmentDefOf.Anything) {
                AnimalsAreFun.Debug($"it's time to sleep: {animal.ToStringSafe()}");
                return false;  // should be sleeping (working is impossible)
            } else if (animal.carryTracker?.CarriedThing != null) {
                AnimalsAreFun.Debug($"currently hauling something: {animal.ToStringSafe()}");
                return false;  // animal is carrying something, i.e. working
            }

            var mindState = animal.mindState;
            if (mindState is null || !mindState.IsIdle) {
                AnimalsAreFun.Debug($"not idle: {animal.ToStringSafe()}");
                return false;  // working
            }

            return true;
        }

        public static bool FindCellForWalking(Pawn pawn, Pawn animal, out IntVec3 walkingCell) {
            // extracted from JoyGiver_GoForWalk.TryGiveJob

            var resultCell = new IntVec3();

            bool CellGoodForWalking(IntVec3 cell) {
                var map = animal.MapHeld;
                return (
                    !PawnUtility.KnownDangerAt(cell, map, pawn) &&
                    !cell.GetTerrain(map).avoidWander &&
                    cell.Standable(map) &&
                    !cell.Roofed(map)
                );
            }

            bool RegionGoodForWalking(Region region) => (
                region.Room.PsychologicallyOutdoors &&
                !region.IsForbiddenEntirely(animal) &&
                !region.IsForbiddenEntirely(pawn) &&
                region.TryFindRandomCellInRegionUnforbidden(animal, CellGoodForWalking, out resultCell) &&
                !resultCell.IsForbidden(pawn)
            );

            var resultGood = CellFinder.TryFindClosestRegionWith(animal.GetRegion(), TraverseParms.For(animal), RegionGoodForWalking, 100, out _);
            walkingCell = resultCell;
            return resultGood;
        }

        public static Pawn GetAnimal(Pawn pawn) {
            bool animalValidator(Thing animalThing) {
                if (!AnimalIsGood(animalThing as Pawn)) {
                    return false;
                } else if (!pawn.CanReserveAndReach(new LocalTargetInfo(animalThing), PathEndMode.ClosestTouch, Danger.None)) {
                    AnimalsAreFun.Debug($"cannot reserve and reach: {animalThing.ToStringSafe()}");
                    return false;
                } else {
                    return true;
                }
            }

            var searchSet = (
                from maybeAnimal in pawn.MapHeld.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Pawn))
                where maybeAnimal.Faction == pawn.Faction
                select maybeAnimal
            );
            var thing = GenClosest.ClosestThing_Global(pawn.Position, searchSet, 30f, animalValidator);
            return thing as Pawn;
        }

        public static bool TryFindOutsideWalkPath(Pawn pawn, Pawn walkingAnimal, out LocalTargetInfo? firstCell, out List<LocalTargetInfo> furtherCells) {
            if (
                FindCellForWalking(pawn, walkingAnimal, out var someCloseOutsideCell) &&  // no first outside cell found
                WalkPathFinder.TryFindWalkPath(pawn, someCloseOutsideCell, out var path)  // no good path for walking outside found
            ) {
                var queue = new List<LocalTargetInfo>(path.Count);
                for (int i = 1, l = path.Count; i < l; ++i) {
                    queue.Add(path[i]);
                }
                
                firstCell = new LocalTargetInfo(path[0]);
                furtherCells = queue;
                return true;
            } else {
                firstCell = default;
                furtherCells = default;
                return false;
            }
        }
    }
}
