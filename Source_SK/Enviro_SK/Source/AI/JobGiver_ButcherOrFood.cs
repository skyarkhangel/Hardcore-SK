using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse.AI;
using Verse;
using RimWorld;
using RimWorld.SquadAI;

namespace SK_Enviro.AI
{
    public class JobGiver_ButcherOrFood : ThinkNode_JobGiver
    {
        public const int FOOD_DISTANCE = 55 * 55;
        private int CheckForHunger; // Random 180 to 600

        public HungerCategory HUNGER_THRESHOLD = HungerCategory.UrgentlyHungry;

        public bool IsHungry(Pawn pawn)
        {
            if (pawn.needs.food == null) return false;
            return pawn.needs.food.CurCategory >= HUNGER_THRESHOLD;
        }

        public override float GetPriority(Pawn pawn)
        {
            return IsHungry(pawn) ? 7.5f : 0f;
        }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {

            TraverseParms traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, true);

            // Find the closest meaty-thing and eat it.
            JobDef meatJobDef = Animals_AI.GetEatMeatJobDef();

            if (Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick < CheckForHunger)
            {
                return null;
            }
            else
            {

            pawn.mindState.lastEngageTargetTick = Find.TickManager.TicksGame;
            CheckForHunger = Rand.RangeInclusive(180, 600);

            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != meatJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                Thing thing = FindMeat(pawn, traverseParams);
                if (thing != null)
                {
                    if (thing.SelectableNow() && pawn.needs.mood != null)
                    {
                        if (thing.def.ingestible.isMeat)
                            pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("AteMeat"));
                        if (thing.def.ingestible.ingestedDirectThought == ThoughtDefOf.AteHumanlikeMeatDirect)
                            pawn.needs.mood.thoughts.TryGainThought(ThoughtDef.Named("HumanMeatIsYummy"));
                    }
                    return new Job(meatJobDef, thing);
                }
            }

            // Find the closest dead meaty-thing and eat it.
            JobDef corpseJobDef = Animals_AI.GetEatCorpseJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != corpseJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                Corpse closestCorpse = FindMeatyCorpse(pawn, traverseParams);
                if (closestCorpse != null)
                {
                    return new Job(corpseJobDef, closestCorpse);
                }
            }

            // Find the closest prey to hunt for food
            JobDef huntJobDef = Animals_AI.GetHuntForAnimalsJobDef();
            if ((pawn.jobs.curJob == null) || ((pawn.jobs.curJob.def != huntJobDef) && pawn.jobs.curJob.checkOverrideOnExpire))
            {
                Pawn closestPrey = FindMeatyPrey(pawn, traverseParams);
                if (closestPrey != null)
                {
                    if (closestPrey.Faction == Faction.OfColony)
                    {
                        Find.LetterStack.ReceiveLetter("LetterLabelHungryAnimal".Translate(), "HungryAnimal".Translate(), LetterType.BadNonUrgent, pawn.Position, null);
                    }
                    return new Job(huntJobDef)
                    {
                        targetA = closestPrey,
                        maxNumMeleeAttacks = 4,
                        //checkOverrideOnExpire = true,
                        killIncappedTarget = true,
                        expiryInterval = 500
                    };
                }
            }

            // if we got here that means we're probably hungry, and certainly stuck.
            if (IsHungry(pawn))
            {
                // get a path to nearest food.
                IntVec3 target = pawn.Position;
                bool targetFound = false;
                traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false);

                // meat first;
                Thing meat = JobGiver_ButcherOrFood.FindMeat(pawn, traverseParams);
                if (meat != null)
                {
                    target = meat.Position;
                    targetFound = true;
                }

                // carrion second;
                Corpse carrion = JobGiver_ButcherOrFood.FindMeatyCorpse(pawn, traverseParams);
                if (!targetFound && carrion != null)
                {
                    target = carrion.Position;
                    targetFound = true;
                }

                // hunting last;
                Pawn prey = JobGiver_ButcherOrFood.FindMeatyPrey(pawn, traverseParams);
                if (!targetFound && prey != null)
                {
                    if (prey.Faction == Faction.OfColony)
                    {
                        Find.LetterStack.ReceiveLetter("LetterLabelHungryAnimal".Translate(), "HungryAnimal".Translate(), LetterType.BadUrgent, pawn.Position, null);
                    }
                    target = prey.Position;
                    targetFound = true;
                }

                // if nothing, just get the heck out of this place.
                if (!targetFound)
                {
                    if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawn.Position, out target))
                    {
                        return null; // no targets, no cells out of colony, no hope.
                    }
                }
                
                // get blocking thing
                PawnPath pawnPath = PathFinder.FindPath(pawn.Position, target, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false), PathEndMode.OnCell);
                IntVec3 CellInFront;
                Building door = pawnPath.FirstBlockingBuilding(out CellInFront) as Building;

                // double check make sure the target is a building (really shouldn't matter).
                if (door == null) return null;

                // release the path
                pawnPath.ReleaseToPool();

                return new Job(Animals_AI.GetBashDoorJobDef(), CellInFront, door)
                {
                    maxNumMeleeAttacks = 4,
                    //checkOverrideOnExpire = true,
                    expiryInterval = 500
                };
            }

            return null;
        }
        }


        public static Pawn FindMeatyPrey(Pawn pawn, TraverseParms traverseParams)
        {
            ThingRequest preyRequest = ThingRequest.ForGroup(ThingRequestGroup.Pawn);
            Predicate<Thing> availPreyPredicate = p =>
            {
                Pawn prey = p as Pawn;

                return isPossiblePrey(prey, pawn);
            };

            Pawn closestPrey = GenClosest.ClosestThingReachable(pawn.Position, preyRequest, PathEndMode.Touch, traverseParams, 100f, availPreyPredicate) as Pawn;
            return closestPrey;
        }

        public static Corpse FindMeatyCorpse(Pawn pawn, TraverseParms traverseParams)
        {
            ThingRequest corpseRequest = ThingRequest.ForGroup(ThingRequestGroup.Corpse);
            Predicate<Thing> availCorpsePredicate = corpse =>
            {
                return (isMeaty(pawn, corpse) && (Find.Reservations.CanReserve(pawn, corpse, 1) == true));
            };

            Corpse closestCorpse = GenClosest.ClosestThingReachable(pawn.Position, corpseRequest, PathEndMode.Touch, traverseParams, 100f, availCorpsePredicate) as Corpse;
            return closestCorpse;
        }

        public static Thing FindMeat(Pawn pawn, TraverseParms traverseParams)
        {
            ThingRequest meatRequest = ThingRequest.ForGroup(ThingRequestGroup.FoodNotPlant);
            Predicate<Thing> availMeatPredicate = food =>
            {
                return (isMeaty(pawn, food) && (Find.Reservations.CanReserve(pawn, food, 1) == true));
            };

            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, meatRequest, PathEndMode.Touch, traverseParams, 100f, availMeatPredicate);
            return thing;
        }

        private static bool isPossiblePrey(Pawn prey, Pawn hunter)
        {
            return (hunter != prey)
                && !prey.Dead
                && !isFriendly(hunter, prey)
                && !isGuest(hunter, prey)
                && !isOwnRace(hunter, prey)
                && isNearby(hunter, prey)
                && (prey.RaceProps.baseBodySize < hunter.RaceProps.baseBodySize || (hunter.needs.food.CurCategory == HungerCategory.UrgentlyHungry));
        }

        private static bool isFriendly(Pawn hunter, Pawn prey)
        {
            Pawn pet = (Pawn)hunter;
            if (pet.Faction == Faction.OfColony)
            {
                Pawn preyT = prey as Pawn;
                if (preyT == null)
                {
                    return (prey.Faction == Faction.OfColony) || (prey.def == hunter.def) || prey.IsPrisonerOfColony || prey.Faction.HostileTo(Faction.OfColony);
                }
                else
                    return (prey.Faction == Faction.OfColony) || (preyT.Faction == Faction.OfColony) || prey.IsPrisonerOfColony || preyT.Faction.HostileTo(Faction.OfColony);
            }
            else
                return prey.def == hunter.def;

        }

        private static bool isGuest(Pawn hunter, Pawn prey)
        {
            Pawn pet = (Pawn)hunter;
            if (pet.Faction == Faction.OfColony)
            {
                Pawn preyG = prey as Pawn;
                if (preyG == null)
                {
                    return (prey.Faction != Faction.OfColony) || (prey.Faction.GoodwillWith(Faction.OfColony) >= 0f) || (prey.def == hunter.def);
                }
                else
                    return (prey.Faction != Faction.OfColony) || (preyG.Faction != Faction.OfColony) || (prey.Faction.GoodwillWith(Faction.OfColony) >= 0f) || (preyG.Faction.GoodwillWith(Faction.OfColony) >= 0f);
            }
            else
                return prey.def == hunter.def;

        }

        private static bool isOwnRace(Pawn hunter, Pawn prey)
        {
            Pawn pet = (Pawn)hunter;
            if (pet.RaceProps.Animal == prey.RaceProps.Animal)
            {
                Pawn preyR = prey as Pawn;
                if (preyR == null)
                {
                    return (prey.def == hunter.def);
                }
                else
                    return (prey.RaceProps.Animal != hunter.RaceProps.Animal);
            }
            else
                return prey.def == hunter.def;

        }

        private static bool isMeaty(Pawn pawn, Thing thing)
        {
            Corpse corpse = thing as Corpse;
            if (corpse == null)
            {
                return isNearby(pawn, thing)
                    && thing.def.ingestible.isMeat;
            }
            else
            {
                return isNearby(pawn, corpse)
                && corpse.innerPawn.RaceProps.isFlesh;
            }

        }

        private static bool isNearby(Pawn pawn, Thing thing)
        {
            return (pawn.Position - thing.Position).LengthHorizontalSquared <= FOOD_DISTANCE;
        }
          
    }
}