using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace aRandomKiwi.HFM
{
	public class JobDriver_AnimalHunt : JobDriver
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
                //return (Pawn)this.pawn.CurJob.GetTarget(TargetIndex.A).Thing;
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
		}

		private Corpse Corpse
		{
			get
			{
                //return this.pawn.CurJob.GetTarget(TargetIndex.A).Thing as Corpse;
                return this.job.GetTarget(TargetIndex.A).Thing as Corpse;
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
            return true;
            //return ReservationUtility.Reserve(this.pawn, this.Prey, this.job, 1, -1, null);
        }


		protected override IEnumerable<Toil> MakeNewToils()
		{
			base.AddFinishAction(delegate
			{
				base.Map.attackTargetsCache.UpdateTarget(this.pawn);
                OnEndKillingTarget();

            });

            //Map restoration universally
            Map cmap = null;
            foreach (var x in Find.Maps)
            {
                if (x.uniqueID == mapUID)
                    cmap = x;
            }

            if (cmap == null)
            {
                Log.Message("ERROR MAP NULL");
                yield break;
            }

            new Toil().initAction = delegate()
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
            yield return Toils_General.DoAtomic(delegate
            {
                base.Map.attackTargetsCache.UpdateTarget(this.pawn);
            });
            /*yield return new Toil
			{
				initAction = delegate()
				{
					base.Map.attackTargetsCache.UpdateTarget(this.pawn);
				},
				atomicWithPrevious = true,
				defaultCompleteMode = ToilCompleteMode.Instant
			};*/
            //this.pawn.jobs.debugLog = true;
            //pawn.jobs.debugLog = true;

            //We notify that the PREY is BUSY
            Prey.TryGetComp<Comp_Hunting>().huntingPreyBusy++;

            //If the predator is already at the place of formation of the pack (selectedWaitingSite)
            if (pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint.x >= 0)
            {
                if(pawn.Position.DistanceToSquared(pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint) <= 4 )
                    pawn.TryGetComp<Comp_Hunting>().huntingArrivedToWaitingPoint = true;
            }

            Action hitAction = delegate()
			{
				Pawn prey = this.Prey;
                this.job.verbToUse = pawn.meleeVerbs.TryGetMeleeVerb(prey);
                bool surpriseAttack = this.firstHit && !prey.IsColonist;
				if (this.pawn.meleeVerbs.TryMeleeAttack(prey, this.job.verbToUse, surpriseAttack))
					base.Map.attackTargetsCache.UpdateTarget(this.pawn);
				this.firstHit = false;
			};
			Toil startCollectCorpse = this.StartCollectCorpseToil();


            //if applicable Waiting until all members have arrived (at the leader's point)
            if (pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint.x >= 0)
            {
                //Log.Message("Waiting to point");
                //Move to coordinates
                IntVec3 r0;

                //Log.Message("CORD_LEADER ::: " + pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint.ToString());
                r0 = CellFinder.RandomSpawnCellForPawnNear(pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint, cmap);
                IntVec3 r1;
                r1 = CellFinder.RandomSpawnCellForPawnNear(pawn.TryGetComp<Comp_Hunting>().huntingWaitingPoint, cmap);

                Toil gotoWaitingPoint = Toils_Goto.GotoCell(r0, PathEndMode.ClosestTouch);
                yield return gotoWaitingPoint;
                yield return Toils_General.Wait(15);
                yield return Toils_Goto.GotoCell(r1, PathEndMode.ClosestTouch);
                //Waiting for other members as long as they are not close
                yield return Toils_Jump.JumpIf(gotoWaitingPoint, delegate
                {
                    //If all the members of the pack are there we return false to continue the other toils
                    bool ok = true;
                    Comp_Hunting ch = pawn.TryGetComp<Comp_Hunting>();

                    foreach (var asst in ch.huntingPackMembers)
                    {
                        //If a dead or downed member ==> cancels the pack
                        if (asst == null || asst.Downed || asst.Dead)
                        {
                            //Notification to the end of job handler that the end is due to the death of a member before the launch of the pack ==> restore the point counters
                            deadMemberBeforeStartHunting = true;
                            ok = false;
                            base.EndJobWith(JobCondition.Incompletable);
                            return true;
                        }
                        else if(!asst.TryGetComp<Comp_Hunting>().huntingArrivedToWaitingPoint)
                        {
                            ok = false;
                            break;
                        }
                    }

                    //Reinitialization of meeting points to notify pack members that they can attack
                    if (ok)
                    {
                        //Log.Message("END");

                        foreach (var asst in ch.huntingPackMembers)
                        {
                            //Log.Message("Set To Invalid !!!!");
                            asst.TryGetComp<Comp_Hunting>().huntingWaitingPoint.x = -1;
                        }

                        ch.huntingWaitingPoint.x = -1;
                        //We force the task of completing the target
                        this.job.playerForced = true;
                    }

                    return !ok;
                });
            }
            else
            {
                //if no pack we now start the forcing to prevent the creature flee in case of threat
                this.job.playerForced = true;

                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
            }

            //Case of remote attacks via tool (? Perhaps via a mod) OR remote attacks by analyzing the verbs in presence
            if (!Settings.ignoredRangedAttack.Contains(pawn.def.defName) && (localAllowRangedAttack || Settings.allowRangedAttack) && ((pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
                || Utils.hasRemoteVerbAttack(pawn.verbTracker.AllVerbs, pawn)))
            {
                //Log.Message("RANGED ATTACK");
                Toil melee=null;
                Toil start = new Toil();
                yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
                yield return start;
                yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpse);
                Toil doNothing = new Toil();
                Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, false, 0.95f).JumpIf((() => Find.TickManager.TicksGame % 250 == 0), doNothing);
                //Default melee if no ranged attack available dynamically
                melee = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction)
                                    .JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).JumpIf(() => Corpse != null, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting && (float)(this.job.GetTarget(TargetIndex.A).Cell - this.pawn.Position).LengthHorizontalSquared > 4f)
                                    .JumpIf(() => Find.TickManager.TicksGame % 250 == 0 && Utils.hasRemoteVerbAttack(pawn.verbTracker.AllVerbs, pawn), start);

                //this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Succeeded);
                //Toil wait = Toils_General.Wait(5);
                //yield return wait;
                yield return Toils_Jump.JumpIf(melee, delegate ()
                {
                    //Log.Message(">>>> Jump to Melee " + !Utils.hasRemoteVerbAttack(pawn.verbTracker.AllVerbs, pawn));
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
                yield return Toils_Jump.Jump(start).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse);
                yield return melee;
            }
            else
            {
                yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).JumpIf(() => Corpse != null, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting && (float)(this.job.GetTarget(TargetIndex.A).Cell - this.pawn.Position).LengthHorizontalSquared > 4f);
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                //Log.Message("MELEE ATTACK");
                yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).JumpIfDespawnedOrNull(TargetIndex.A, startCollectCorpse).JumpIf(() => Corpse != null, startCollectCorpse).FailOn(() => Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting && (float)(this.job.GetTarget(TargetIndex.A).Cell - this.pawn.Position).LengthHorizontalSquared > 4f);
                //yield return toil;
            }
            yield return Toils_Jump.JumpIfTargetDespawnedOrNull(TargetIndex.A, startCollectCorpse);
            yield return startCollectCorpse;
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
			yield break;
		}

		private Toil StartCollectCorpseToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate()
			{
                //Log.Message("KA1");
                //We go back to safe mode, the predator will abandon its tasks if its life is in danger.
                this.job.playerForced = false;
				if (this.Prey == null)
				{
                    //Log.Message("KA2");
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				Corpse corpse = this.Prey.Corpse;
				if (corpse == null || !this.pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, false))
				{
                    //Log.Message("KA3");
                    this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
					return;
				}
				corpse.SetForbidden(false, true);
				IntVec3 c;
				if (StoreUtility.TryFindBestBetterStoreCellFor(corpse, this.pawn, this.Map, StoragePriority.Unstored, this.pawn.Faction, out c, true))
				{
                    //Log.Message("HERE");
                    ReservationUtility.Reserve(this.pawn, corpse, this.pawn.CurJob, 1, -1, null);
					ReservationUtility.Reserve(this.pawn, c, this.pawn.CurJob, 1, -1, null);
					this.pawn.CurJob.SetTarget(TargetIndex.B, c);
					this.pawn.CurJob.SetTarget(TargetIndex.A, corpse);
					this.pawn.CurJob.count = 1;
					this.pawn.CurJob.haulMode = HaulMode.ToCellStorage;
					return;
				}
				this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true);
			};
			return toil;
		}

        public override void Notify_DamageTaken(DamageInfo dinfo)
        {
            base.Notify_DamageTaken(dinfo);
            /*if (dinfo.Def.ExternalViolenceFor(this.pawn) && dinfo.Def.isRanged && dinfo.Instigator != null && dinfo.Instigator != this.Prey && !this.pawn.InMentalState && !this.pawn.Downed)
            {
                this.pawn.mindState.StartFleeingBecauseOfPawnAction(dinfo.Instigator);
            }*/
        }


        public bool OnFailAttack()
        {
            LocalTargetInfo pos = this.job.GetTarget(TargetIndex.A);

            return (Find.TickManager.TicksGame > this.startTick + Utils.maxTicksToCompleteHunting);
                //&& (pos != null && (float)(pos.Cell - this.pawn.Position).LengthHorizontalSquared > 4f));
        }

        /*
         *  Action summoned when the target is dead or the canvas abruptly ends
         */
        private void OnEndKillingTarget()
        {
            //Log.Message("TOIL ENDDDD");

            Comp_Hunting ch = pawn.TryGetComp<Comp_Hunting>();

            //Interruption of assistants' jobs, if applicable
            if (ch.huntingPackMembers != null && ch.huntingPackMembers.Count > 0)
            {
                //Log.Message("ManualStop == " + manualStop.ToString());
                //If stop following the death of an animal of the pack then we restore the counters
                //ditto for a forced stop
                //Ditto if end for whatever reason and the waitingPoint still defined (waiting pre-attack step)
                if (deadMemberBeforeStartHunting || manualStop || ch.huntingWaitingPoint.x >= 0)
                {
                    //If an unplanned stop from master, we signal it to the assistants (code -2)
                    if (ch.huntingWaitingPoint.x != -1)
                        ch.huntingWaitingPoint.x = -2;
                    Utils.CancelHuntingPack(pawn,pawn, manualStop);
                }

                ch.huntingWaitingPoint.x = -1;
                ch.huntingArrivedToWaitingPoint = false;
                ch.huntingPackMembers.Clear();
            }

            ch = Prey.TryGetComp<Comp_Hunting>();
            //Add here the code to notify that the preys is again eligible for selection in the JobGiver
            if (ch != null)
            {
                ch.huntingPreyBusy--;
            }
        }

        public int mapUID = 0;
        public bool manualStop = false;

        public bool localAllowRangedAttack = false;
        public const TargetIndex PreyInd = TargetIndex.A;
		private const TargetIndex CorpseInd = TargetIndex.A;
		private bool firstHit = true;
        private bool deadMemberBeforeStartHunting = false;
	}
}
