using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace aRandomKiwi.HFM
{
    public class JobDriver_AnimalHuntAssist : JobDriver
    {
        public Pawn Prey
        {
            get
            {
                Corpse corpse = this.Corpse;
                if (corpse != null)
                {
                    return corpse.InnerPawn;
                }
                return (Pawn)this.pawn.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        private Corpse Corpse
        {
            get
            {
                LocalTargetInfo target = this.pawn.CurJob.GetTarget(TargetIndex.A);
                if (target != null)
                    return target.Thing as Corpse;
                else
                    return null;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.firstHit, "firstHit", false, false);
            Scribe_Values.Look<int>(ref this.mapUID, "HFM_mapUID", -1, false);
            Scribe_Values.Look<bool>(ref this.localAllowRangedAttack, "HFM_localAllowRangedAttack", false, false);
        }

        public override string GetReport()
        {
            if (this.Corpse != null)
            {
                return base.ReportStringProcessed(JobDefOf.HaulToCell.reportString);
            }
            return base.GetReport();
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            //Unique ID save of the map
            mapUID = this.pawn.Map.uniqueID;
            localAllowRangedAttack = Settings.allowRangedAttack;
            //return ReservationUtility.Reserve(this.pawn, this.Prey, this.job, 1, -1, null);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            base.AddFinishAction(delegate
            {
                base.Map.attackTargetsCache.UpdateTarget(this.pawn);
                OnEndKillingTarget();
            });

            //Map restoration universally
            Map cmap =null;
            foreach(var x in Find.Maps)
            {
                if (x.uniqueID == mapUID)
                    cmap = x;
            }

            if(cmap == null)
            {
                Log.Message("ERROR MAP NULL");
                yield break;
            }

            new Toil().initAction = delegate ()
            {
                Pawn pawn = this.pawn;
                Pawn prey = this.Prey;
                if (prey == null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }
                Corpse corpse = prey.Corpse;
                if (corpse == null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                    return;
                }
                if (pawn.Faction == Faction.OfPlayer)
                {
                    corpse.SetForbidden(false, false);
                }
                else
                {
                    corpse.SetForbidden(true, false);
                }
                pawn.CurJob.SetTarget(TargetIndex.A, corpse);
            };
            yield return new Toil
            {
                initAction = delegate ()
                {
                    base.Map.attackTargetsCache.UpdateTarget(this.pawn);
                },
                atomicWithPrevious = true,
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            //pawn.jobs.debugLog = true;
            //We notify that the PREY is BUSY
            Prey.TryGetComp<Comp_Hunting>().huntingPreyBusy++;

            Action hitAction = delegate ()
            {
                Pawn prey = this.Prey;
                this.job.verbToUse = pawn.meleeVerbs.TryGetMeleeVerb(prey);
                bool surpriseAttack = this.firstHit && !prey.IsColonist;
                if (this.pawn.meleeVerbs.TryMeleeAttack(prey, this.pawn.CurJob.verbToUse, surpriseAttack))
                    base.Map.attackTargetsCache.UpdateTarget(this.pawn);
                this.firstHit = false;
            };


            //If the predator is already at the place of formation of the pack (selectedWaitingSite)
            if (pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint.x >= 0)
            {
                if (pawn.Position.DistanceToSquared(pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint) <= 4)
                    pawn.TryGetComp<Comp_Hunting>().huntingArrivedToWaitingPoint = true;
            }

            if (pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint.x >= 0)
            {
                //Log.Message("Waiting to point");
                //Move to coordinates
                IntVec3 r0 = new IntVec3();
                //Log.Message("COOOOORD_ASST ::: "+pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint.ToString());
                r0 = CellFinder.RandomSpawnCellForPawnNear(pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint, cmap, 4);
                IntVec3 r1 = new IntVec3();
                r1 = CellFinder.RandomSpawnCellForPawnNear(pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint, cmap, 4);

                Toil gotoWaitingPoint = Toils_Goto.GotoCell(r0, PathEndMode.ClosestTouch);
                yield return gotoWaitingPoint;
                yield return Toils_General.Wait(15);
                yield return Toils_Goto.GotoCell( r1, PathEndMode.ClosestTouch);
                //Waiting for other members as long as they are not close
                yield return Toils_Jump.JumpIf(gotoWaitingPoint, delegate
                {
                    Comp_Hunting ch = pawn.TryGetComp<Comp_Hunting>();
                    //We notify that the pawn has arrived at the meeting point
                    ch.huntingArrivedToWaitingPoint = true;
                    //Check if huntingWaitingPoint still defined (master did not give the order to launch the assault)
                    return !(ch.huntingWaitingPoint.x < 0);
                });
            }
            else
            {
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
            }

            //Case of remote attacks via tool (? Perhaps via a mod) OR remote attacks by analyzing the verbs in presence
            //ONLY if authorized in the config !!
            if (!Settings.ignoredRangedAttack.Contains(pawn.def.defName) && (localAllowRangedAttack || Settings.allowRangedAttack) && ((pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
                || Utils.hasRemoteVerbAttack(pawn.verbTracker.AllVerbs, pawn)))
            {
                //Log.Message("RANGED ATTACK");
                Toil melee = null;
                Toil start = new Toil();
                yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
                yield return start;
                Toil doNothing = new Toil();
                Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, false, 0.95f).JumpIf((() => Find.TickManager.TicksGame % 250 == 0), doNothing);
                //Default melee if no ranged attack available dynamically
                melee = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction)
                                    .EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable)
                                    .FailOn(() => Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting && (float)(this.pawn.CurJob.GetTarget(TargetIndex.A).Cell - this.pawn.Position).LengthHorizontalSquared > 4f)
                                    .JumpIf(() => Find.TickManager.TicksGame % 250 == 0 && Utils.hasRemoteVerbAttack(pawn.verbTracker.AllVerbs, pawn), start);

                //this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Succeeded);
                //Toil wait = Toils_General.Wait(5);
                //yield return wait;
                yield return Toils_Jump.JumpIf(melee, delegate ()
                {
                    return !Utils.hasRemoteVerbAttack(pawn.verbTracker.AllVerbs, pawn);
                });
                yield return gotoCastPos;
                //Toil jumpIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
                yield return doNothing;
                //yield return jumpIfCannotHit;
                yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
                //Ranged attack, if in not completed target mode then stop the attack and jump to nothing
                yield return Toils_Combat.CastVerb(TargetIndex.A, true).FailOn(OnFailAttack);

                //yield return Toils_Jump.Jump(jumpIfCannotHit);
                yield return Toils_Jump.Jump(start).FailOnDespawnedOrNull(TargetIndex.A);
                yield return melee;
            }
            else
            {
                yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable).FailOn(() => Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting && (float)(this.pawn.CurJob.GetTarget(TargetIndex.A).Cell - this.pawn.Position).LengthHorizontalSquared > 4f);
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                //Log.Message("MELEE ATTACK");
                
                yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Incompletable).FailOn(() => Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting && (float)(this.pawn.CurJob.GetTarget(TargetIndex.A).Cell - this.pawn.Position).LengthHorizontalSquared > 4f);
            }

            yield break;
        }


        public bool OnFailAttack()
        {
            LocalTargetInfo pos = this.job.GetTarget(TargetIndex.A);

            return (Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting
                && (pos != null && (float)(pos.Cell - this.pawn.Position).LengthHorizontalSquared > 4f));
        }

        /*
         *  Action summoned when the target is dead or the canvas abruptly ends
         */
        private void OnEndKillingTarget()
        {
            //Add here the code to notify that the preys is again eligible for selection in the JobGiver
            Comp_Hunting ch = pawn.TryGetComp<Comp_Hunting>();
            int wpX = ch.huntingWaitingPoint.x;
            ch.huntingArrivedToWaitingPoint = false;
            ch.huntingWaitingPoint.x = -1;

            //Log.Message("TOIL ASSIST ENDDDD " + pawn.LabelCap);

            //Log.Message("ManualStop == "+manualStop.ToString());
            //If the animal is a member of a pack and the stop was made manually by the player OR if the stop was made for any reason before the arrival at the waiting coordinates
            //And if the end does not come from the master (code -2) or from another assistant
            //then end of the pack
            if ((manualStop || wpX != -1) && ch.huntingPackMaster != null && ch.huntingPackMaster.TryGetComp<Comp_Hunting>().huntingWaitingPoint.x > -2)
            {
                //Notif sur le master et autres assistants que l'appel de fin provient d'un assistant
               ch.huntingPackMaster.TryGetComp<Comp_Hunting>().huntingWaitingPoint.x = -3;
               Utils.CancelHuntingPack(ch.huntingPackMaster, pawn, manualStop);
            }

            ch = Prey.TryGetComp<Comp_Hunting>();
            if (ch != null)
            {
                ch.huntingPreyBusy--;
            }
        }

        public int mapUID = -1;
        public bool localAllowRangedAttack = false;
        public bool manualStop = false;

        public const TargetIndex PreyInd = TargetIndex.A;
        private const TargetIndex CorpseInd = TargetIndex.A;
        private bool firstHit = true;
    }
}
