using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace aRandomKiwi.HFM
{
    internal class JobGiver_AnimalHunt : ThinkNode_JobGiver
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            return (JobGiver_AnimalHunt)base.DeepCopy(resolve);
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            List<Pawn> list = new List<Pawn>();
            Comp_Hunting ch;
            bool notifNextGT = false;


            if (pawn == null)
                return null;

            if (pawn.Downed)
                return null;

            //If danger on the map and parameter ok we do not launch the job
            if (GenHostility.AnyHostileActiveThreatToPlayer(pawn.Map) && Settings.disallowHuntingWhenThreat)
                return null;

            if (manualCall)
                notifNextGT = true;

            //If Cat and manual mode, the hunt may fail (kitty does not want to) in 30% of cases
            if (Settings.cats.Contains(pawn.kindDef.defName))
            {
                //If the cat is not authorized to bring gifts
                if (!Settings.allowCatGift)
                    return null;

                if (manualCall && Rand.Chance(0.35f))
                {
                    pawn.TryGetComp<Comp_Hunting>().setNextGT(notifNextGT);
                    Messages.Message("HuntingForMe_ForceKittyHuntingFailDontWant".Translate(pawn.LabelCap), MessageTypeDefOf.NegativeEvent, false);
                    return null;
                }
                //If not in manual mode 15% chance that the cat will fail to hunt
                if (!manualCall && Rand.Chance(0.15f))
                {
                    return null;
                }
            }

            //(If hunting activated OR CAT) and waiting period reached
            if ( (pawn.GetComp<Comp_Hunting>().huntingState || Settings.cats.Contains(pawn.kindDef.defName) ) && pawn.TryGetComp<Comp_Hunting>().nextGTOK())
            {
                foreach (Pawn pawn2 in pawn.Map.mapPawns.AllPawnsSpawned)
                {
                    //Cats do not hunt in packs
                    if (!Settings.cats.Contains(pawn.kindDef.defName) && list.Count < Settings.maxHuntMPack && pawn2.Faction == pawn.Faction && pawn2.training != null && pawn2.training.HasLearned(DefDatabase<TrainableDef>.GetNamed("HuntingTraining", true)))
                    {
                        float lengthHorizontal = (pawn.Position - pawn2.Position).LengthHorizontal;
                        ch = pawn2.TryGetComp<Comp_Hunting>();

                        /*Log.Message(">>HELPER " + pawn2.LabelShortCap);
                        Log.Message((pawn2.kindDef == pawn.kindDef || Settings.allowDiffSpeciesPack) + " "
                        + (pawn2.CurJob == null || !Utils.notInterruptibleJobs.Contains(pawn2.CurJob.def.defName)) + " "
                        + (!pawn2.Dead) + " "
                        + (pawn != pawn2) + " "
                        + (!pawn2.Downed && pawn.GetComp<Comp_Hunting>().huntingState) + " "
                        + (lengthHorizontal <= Settings.radiusBetweenPackMembers) + " "
                        + (ch.huntingMode == pawn.TryGetComp<Comp_Hunting>().huntingMode) + " "
                        + (ch.huntingPreyMode == pawn.TryGetComp<Comp_Hunting>().huntingPreyMode) + " "
                        + (ch.huntingState) + " "
                        + (ch.nextGTOK()) + " "
                        + (ch.huntingCanAssist));*/

                        //If assistants authorized to hunt AND if the other pawn authorized to hunt in the same way as the predator
                        //AND that the prey hunting mode is the same AND that the cooldown is respected AND that the animal has the right to assist during a hunt
                        //AND that assistant does not perform a job in the internal blacklist
                        if ( (pawn2.kindDef == pawn.kindDef || Settings.allowDiffSpeciesPack )
                            && ( pawn2.CurJob == null ||  !Utils.notInterruptibleJobs.Contains(pawn2.CurJob.def.defName))
                            && !pawn2.Dead
                            && pawn != pawn2
                            && !pawn2.Downed && pawn.GetComp<Comp_Hunting>().huntingState 
                            && lengthHorizontal <= Settings.radiusBetweenPackMembers
                            && ch.huntingMode == pawn.TryGetComp<Comp_Hunting>().huntingMode
                            && ch.huntingPreyMode == pawn.TryGetComp<Comp_Hunting>().huntingPreyMode
                            && ch.huntingState
                            && ch.nextGTOK()
                            && ch.huntingCanAssist)
                        {
                            list.Add(pawn2);
                        }
                    }
                }

                //If param in disable solo hunt mode (does not apply to chats)
                if (!Settings.cats.Contains(pawn.kindDef.defName) && ( Settings.disableSoloHunting && list.Count == 0))
                    return null;

                bool huntAlone;
                Pawn pawn3;
                //Number of reinforcements required as far as possible
                int neededAssistants =0;
                bool assisted = false;
                List<Pawn> selectedAssistants = new List<Pawn>();
                IntVec3 selectedWaitingPoint = default(IntVec3);

                //Log.Message("Available assistants : "+list.Count);

                //If supervised mode (not for cats)
                if (!Settings.cats.Contains(pawn.kindDef.defName) && pawn.GetComp<Comp_Hunting>().huntingMode)
                    pawn3 = JobGiver_AnimalHunt.BestPawnToHuntForPredatorSup(pawn, list, out huntAlone,out neededAssistants);
                else
                {
                    pawn3 = JobGiver_AnimalHunt.BestPawnToHuntForPredator(pawn, list, out huntAlone,  out neededAssistants);
                }

                //Log.Message("NeededAssistants = " + neededAssistants.ToString() + ", huntAlone = " + huntAlone.ToString());

                if (pawn3 != null)
                {
                    //If no hunting alone And needed Assistants> 0
                    if (!huntAlone && neededAssistants > 0)
                    {
                        //selectedAssistants
                        //Part of list of effective assistants
                        foreach (Pawn assistant in list)
                        {
                            //Check if not the same pawn as the master pawn of the assault
                            if (assistant != pawn)
                            {
                                selectedAssistants.Add(assistant);
                                assisted = true;
                                neededAssistants--;
                                //If more helpers are needed
                                if (neededAssistants == 0)
                                    break;
                            }
                        }

                        //Closest waiting coordinates deduction for the waiting point
                        int distance =-1;
                        int cdist = 0;
                        foreach (Pawn assistant in selectedAssistants)
                        {
                            cdist = assistant.Position.DistanceToSquared(pawn3.Position);
                            if (cdist < distance || distance == -1)
                            {
                                distance = cdist;
                                selectedWaitingPoint = assistant.Position;
                            }
                        }

                        //Comparison with the leader
                        cdist = pawn.Position.DistanceToSquared(pawn3.Position);
                        if (cdist < distance || distance == -1)
                            selectedWaitingPoint = pawn.Position;

                        //Assigning assistant jobs to assistants
                        foreach (Pawn assistant in selectedAssistants)
                        {
                            ch = assistant.TryGetComp<Comp_Hunting>();
                            //Reset also for the forceHunting timeout assistants (they actually assist the unit)
                            ch.setNextGT(notifNextGT);
                            //Definition of wandering posiiton awaiting the pack and the leader
                            ch.huntingWaitingPoint = selectedWaitingPoint;
                            ch.huntingArrivedToWaitingPoint = false;
                            ch.huntingPackMaster = pawn;

                            Job job = new Job(DefDatabase<JobDef>.GetNamed("HuntTrainedAssist", true), pawn3, pawn) { killIncappedTarget = true };
                            assistant.jobs.StopAll();
                            assistant.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        }
                    }

                    pawn.TryGetComp<Comp_Hunting>().setNextGT(notifNextGT);

                    string jobName = "";
                    if (Settings.cats.Contains(pawn.kindDef.defName))
                        jobName = "HuntForMe_CatGift";
                    else
                    {
                        ch = pawn.TryGetComp<Comp_Hunting>();
                        if (assisted)
                            ch.huntingWaitingPoint = selectedWaitingPoint;
                        else
                            ch.huntingWaitingPoint.x = -1;

                        //Definition list of pack members for the leader
                        ch.huntingPackMembers = new List<Pawn>();
                        foreach (var asst in selectedAssistants)
                            ch.huntingPackMembers.Add(asst);

                        ch.huntingPackMaster = null;

                        jobName = "HuntTrained";
                    }


                    //If notification request within the framework of a pack
                    if (!manualCall && Settings.notifNewHuntingPack && selectedAssistants.Count > 0)
                    {
                        Messages.Message("HuntingForMe_NotifNewHuntingPack".Translate(selectedAssistants.Count+1, pawn3.LabelCap), new LookTargets(selectedWaitingPoint,pawn.Map) , MessageTypeDefOf.NeutralEvent, false);
                    }
                    //If notification is requested in the context of a solo hunter
                    else if (!manualCall && Settings.notifNewHunting)
                    {
                        Messages.Message("HuntingForMe_NotifNewHunting".Translate(pawn.LabelCap, pawn3.LabelCap),pawn, MessageTypeDefOf.NeutralEvent, false);
                    }


                    return new Job(DefDatabase<JobDef>.GetNamed(jobName, true), pawn3)
                    {
                        killIncappedTarget = true
                    };
                }
            }

            //IF manual call display of failures to the user
            if (manualCall)
            {
                //Cat
                if (Settings.cats.Contains(pawn.kindDef.defName))
                {
                    Messages.Message("HuntingForMe_ForceKittyHuntingFail".Translate(pawn.LabelCap), MessageTypeDefOf.NegativeEvent, false);
                }
                else
                {
                    Messages.Message("HuntingForMe_ForceHuntingFail".Translate(pawn.LabelCap), MessageTypeDefOf.NegativeEvent, false);
                }
            }

            //Failed job launch (no prey, bad luck, ...)
            return null;
        }

        /*
         * selection prey from the list of animals defined as (to be hunted)
         */
        private static Pawn BestPawnToHuntForPredatorSup(Pawn predator, List<Pawn> add, out bool huntAlone, out int neededAssistants)
        {
            neededAssistants = 0;
            Pawn pawn = null;
            huntAlone = true;
            if (predator == null)
                return null;

            Comp_Hunting ch = predator.TryGetComp<Comp_Hunting>();

            if (ch == null || predator.meleeVerbs == null || predator.meleeVerbs.TryGetMeleeVerb(null) == null
                || predator.Map == null)
            {
                return null;
            }

            foreach (Designation des in predator.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Hunt))
            {
                Pawn pawn2 = (Pawn)des.target.Thing;
                bool flag2 = true;
                float num = 0f;

                if (!Settings.ignoredPreys.Contains(pawn2.kindDef.defName)
                    && pawn2.Position.InAllowedArea(predator)
                    && (pawn2.TryGetComp<Comp_Hunting>() != null && pawn2.TryGetComp<Comp_Hunting>().huntingPreyBusy == 0)
                    && ((ch.huntingPreyMode && JobGiver_AnimalHunt.IsAcceptablePreyFor(predator, pawn2, add, true,out flag2, out neededAssistants)) || (!ch.huntingPreyMode && JobGiver_AnimalHunt.IsAcceptablePreyFor(predator, pawn2, add, false, out flag2, out neededAssistants)))
                    && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn) )
                {
                    float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2);
                    if (preyScoreFor > num || pawn == null)
                    {
                        huntAlone = flag2;
                        num = preyScoreFor;
                        pawn = pawn2;
                        break;
                    }
                }

            }

            return pawn;
        }

        /*
         * Random prey selection
         */
        private static Pawn BestPawnToHuntForPredator(Pawn predator, List<Pawn> add, out bool huntAlone, out int neededAssistants)
        {
            neededAssistants = 0;
            huntAlone = true;

            if (predator == null)
                return null;

            Comp_Hunting ch = predator.TryGetComp<Comp_Hunting>();

            if (ch == null || predator.meleeVerbs == null || predator.meleeVerbs.TryGetMeleeVerb(null) == null
                || predator.Map == null)
            {
                return null;
            }
            bool flag = false;
            if (predator.health.summaryHealth.SummaryHealthPercent < 0.25f)
            {
                flag = true;
            }
            List<Pawn> allPawnsSpawned = predator.Map.mapPawns.AllPawnsSpawned;
            Pawn pawn = null;
            float num = 0f;
            bool ok = true;
            bool tutorialMode = TutorSystem.TutorialMode;

            for (int i = 0; i < allPawnsSpawned.Count; i++)
            {
                Pawn pawn2 = allPawnsSpawned[i];
                ok = true;
                //Check if not in the animals to be tamed
                foreach (Designation des in predator.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.Tame))
                {
                    Pawn target = (Pawn)des.target.Thing;
                    //The potential prey is in the taming list we go to the next one
                    if (target == pawn2)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn) && pawn2.Position.InAllowedArea(predator) && predator != pawn2 && (!flag || pawn2.Downed))
                {
                    bool flag2 = true;
                    if (!Settings.ignoredPreys.Contains(pawn2.kindDef.defName)
                        && !pawn2.RaceProps.Humanlike
                        && pawn2.Faction != Faction.OfPlayer
                        && (pawn2.TryGetComp<Comp_Hunting>() != null && pawn2.TryGetComp<Comp_Hunting>().huntingPreyBusy == 0)
                        && !pawn2.IsForbidden(predator) && (!tutorialMode || pawn2.Faction != Faction.OfPlayer)
                        && ( ( (Settings.cats.Contains(predator.kindDef.defName) || ch.huntingPreyMode ) && JobGiver_AnimalHunt.IsAcceptablePreyFor(predator, pawn2, add, true, out flag2, out neededAssistants) ) 
                            || (!Settings.cats.Contains(predator.kindDef.defName) && !ch.huntingPreyMode && JobGiver_AnimalHunt.IsAcceptablePreyFor(predator, pawn2, add, false, out flag2, out neededAssistants) ) )
                        && predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly, false, TraverseMode.ByPawn) )
                    {
                        float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2);
                        if (preyScoreFor > num || pawn == null)
                        {
                            //Log.Message(">>>>"+pawn2.Label);
                            huntAlone = flag2;
                            num = preyScoreFor;
                            pawn = pawn2;
                            break;
                        }
                    }
                }
            }
            return pawn;
        }

        /*
         * Default behavior predators only attack prey <= their size without taking into account the pack
         */
        public static bool IsAcceptablePreyFor(Pawn predator, Pawn prey, List<Pawn> add, bool safeMode, out bool huntAlone, out int neededAssistants)
        {
            huntAlone = false;
            neededAssistants = 0;
            int neededAsst = 0;
            if (!prey.RaceProps.IsFlesh)
            {
                return false;
            }

            if (!prey.Downed)
            {
                float preyPower = getPawnScore(prey);
                float predatorsPower = getPawnScore(predator);

                //Are we in secure mode? we only fight creatures of our strength = <a predator
                if (safeMode)
                {
                    if (preyPower > predatorsPower)
                        return false;
                }

                //If no reinforcements check direct 
                if (add.Count == 0)
                {
                    //Log.Message("Score ==> " + preyPower + " , " + predatorsPower);
                    huntAlone = true;
                    if (preyPower > predatorsPower)
                        return false;
                }
                else
                {
                    //IF reinforcements are available
                    //Summation of the fighting powers of predators as long as the required power is not reached for the prey
                    bool ok = false;
                    foreach (Pawn assistant in add)
                    {
                        //We have the balanced combination to face the prey
                        if (preyPower <= predatorsPower)
                        {
                            ok = true;
                            neededAssistants = neededAsst;
                            break;
                        }

                        //Looks like we need an extra assistant
                        predatorsPower += JobGiver_AnimalHunt.getPawnScore(assistant);
                        neededAsst++;
                    }
                    //We check last combination
                    if (preyPower <= predatorsPower)
                    {
                        ok = true;
                        neededAssistants = neededAsst;
                    }

                    //If no balanced combination found
                    if (!ok)
                    {
                        //Log.Message("NOPE => "+prey.Label + " PreyPower = " + preyPower.ToString() + " , predatorPower = " + predatorsPower.ToString());
                        return false;
                    }

                    //If the combination found too "borderline" then add an assistant if possible
                    if ((int)(preyPower / predatorsPower * 100) >= 70 && neededAssistants < add.Count)
                    {
                        //If small predator we try to add 3 instead of 1
                        if (predator.BodySize <= 0.4f)
                        {
                            //Log.Message("CNeedAssistants = "+neededAssistants.ToString()+" "+add.Count);

                            if (neededAssistants + 3 <= add.Count)
                                neededAssistants += 3;
                            else if(neededAssistants + 2 <= add.Count)
                                neededAssistants+=2;
                            else
                                neededAssistants++;
                        }
                        else
                        {
                            neededAssistants++;
                        }


                    }

                    //If param to force multiple hunting
                    if (Settings.disableSoloHunting && neededAssistants == 0)
                    {
                        huntAlone = false;
                        if (add.Count >= 2)
                            neededAssistants = 2;
                        else
                            neededAssistants = 1;
                    }

                    //Log.Message(prey.Label + " PreyPower = " + preyPower.ToString() + " , predatorPower = " + predatorsPower.ToString());
                }
            }
            return (predator.Faction == null || prey.Faction == null || predator.HostileTo(prey)) && (!predator.RaceProps.herdAnimal || predator.def != prey.def);
        }

        /*
         * Calculation of the pawn score
         */
        private static float getPawnScore(Pawn pawn)
        {
            float score;
            //Calculation of raw score
            score = pawn.kindDef.combatPower* pawn.health.summaryHealth.SummaryHealthPercent* pawn.ageTracker.CurLifeStage.bodySizeFactor;

            //Log.Message(pawn.LabelCap + " " + pawn.kindDef.combatPower + " " + pawn.health.summaryHealth.SummaryHealthPercent + " " + pawn.ageTracker.CurLifeStage.bodySizeFactor);

            //Smoothing of the score with the potential defaults at the level of the capacities (consciousness, manipulation, Moving, Sight) of the pawn
            float lvlConsciousness = Math.Max(PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, PawnCapacityDefOf.Consciousness),0.15f);
            float lvlManipulation = Math.Max(PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, PawnCapacityDefOf.Manipulation),0.15f);
            float lvlMoving = Math.Max(PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, PawnCapacityDefOf.Moving),0.15f);
            float lvlSight = Math.Max(PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, PawnCapacityDefOf.Sight),0.15f);

            //the adjustment is only carried out on 50% of the score we bring the adjustment to a value between 0 and 1
            float adjust = Math.Min(1.0f, (lvlConsciousness * lvlManipulation * lvlMoving * lvlSight)+0.5f);

            //Log.Message(pawn.LabelCap+" " + adjust.ToString()+" ( "+lvlConsciousness+" "+lvlManipulation+" "+lvlMoving+" "+lvlSight+" )");

            return score * adjust;
        }

        public bool manualCall = false;
        private const float MinDistFromEnemy = 27f;
    }
}
