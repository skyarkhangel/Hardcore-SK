using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld;


namespace Nandonalt_ColonyLeadership
{
    public class JobArrestLeader : JobDriver
    {
        private const TargetIndex TakeeIndex = TargetIndex.A;

        private const TargetIndex BedIndex = TargetIndex.B;

        protected Pawn Takee
        {
            get
            {
                return (Pawn)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Building_Bed DropBed
        {
            get
            {
                return (Building_Bed)base.CurJob.GetTarget(TargetIndex.B).Thing;
            }
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            if (base.CurJob.def == JobDefOf.Rescue)
            {
                this.FailOnNotDowned(TargetIndex.A);
            }
            this.FailOn(delegate
            {
                if (this.CurJob.def.makeTargetPrisoner)
                {
                    if (!this.DropBed.ForPrisoners)
                    {
                        return true;
                    }
                }
                else if (this.DropBed.ForPrisoners != ((Pawn)((Thing)this.TargetA)).IsPrisoner)
                {
                    return true;
                }
                return false;
            });
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1);
            yield return Toils_Reserve.Reserve(TargetIndex.B, this.DropBed.SleepingSlotsCount);
            yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.B, TargetIndex.A);
            this.globalFinishActions.Add(delegate
            {
                if (this.CurJob.def.makeTargetPrisoner && this.Takee.ownership.OwnedBed == this.DropBed && this.Takee.Position != RestUtility.GetBedSleepingSlotPosFor(this.Takee, this.DropBed))
                {
                    this.Takee.ownership.UnclaimBed();
                }
            });
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnNonMedicalBedNotOwned(TargetIndex.B, TargetIndex.A).FailOn(() => this.CurJob.def == JobDefOf.Arrest && !this.Takee.CanBeArrested()).FailOn(() => !this.pawn.CanReach(this.DropBed, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (this.CurJob.def.makeTargetPrisoner)
                    {
                        Pawn pawn = (Pawn)this.CurJob.targetA.Thing;
                        Lord lord = pawn.GetLord();
                        if (lord != null)
                        {
                            lord.Notify_PawnAttemptArrested(pawn);
                        }
                        GenClamor.DoClamor(pawn, 10f, ClamorType.Harm);

                        if (Rand.Value < 0.1f)
                        {
                            Messages.Message("MessageRefusedArrest".Translate(new object[]
                            {
        pawn.LabelShort
                            }), pawn, MessageSound.SeriousAlert);

                            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
                            IncidentWorker_Rebellion.removeLeadership(pawn);
                            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderArrested"));
                            this.pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                            Find.LetterStack.ReceiveLetter("LeaderEndLetter".Translate(), "LeaderEndLetterDesc".Translate(new object[] { pawn.Name.ToStringFull }), LetterDefOf.BadNonUrgent, pawn, null);
                            this.pawn.jobs.EndCurrentJob(JobCondition.Incompletable, true);
                            
                        }
                    }               
            
                }
            };

            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.Touch);
            yield return new Toil
            {
                initAction = delegate
                {
                    if (this.CurJob.def.makeTargetPrisoner)
                    {
                        this.pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                        IncidentWorker_Rebellion.removeLeadership(this.Takee);
                        this.Takee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderArrested"));
                        Find.LetterStack.ReceiveLetter("LeaderEndLetterArrested".Translate(), "LeaderEndLetterDescArrested".Translate(new object[] { Takee.Name.ToStringFull }), LetterDefOf.BadNonUrgent, this.pawn, null);
                        foreach (Pawn p in IncidentWorker_LeaderElection.getAllColonists())
                        {
                            if (p != this.Takee)
                            {                              
                                    p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LeaderArrestedColonist"), null);
                                
                            }
                        }

                        if (this.Takee.guest.released)
                        {
                            this.Takee.guest.released = false;
                            this.Takee.guest.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
                        }
                        if (!this.Takee.IsPrisonerOfColony)
                        {
                            if (this.Takee.Faction != null)
                            {
                                this.Takee.Faction.Notify_MemberCaptured(this.Takee, this.pawn.Faction);
                            }
                            this.Takee.guest.SetGuestStatus(Faction.OfPlayer, true);
                            if (this.Takee.guest.IsPrisoner)
                            {
                                TaleRecorder.RecordTale(TaleDefOf.Captured, new object[]
                                {
                                    this.pawn,
                                    this.Takee
                                });
                                this.pawn.records.Increment(RecordDefOf.PeopleCaptured);
                            }
                        }
                    }
                    else if (this.Takee.Faction != Faction.OfPlayer && this.Takee.HostFaction != Faction.OfPlayer && this.Takee.guest != null)
                    {
                        this.Takee.guest.SetGuestStatus(Faction.OfPlayer, false);
                    }
                    if (this.Takee.playerSettings == null)
                    {
                        this.Takee.playerSettings = new Pawn_PlayerSettings(this.Takee);
                    }
                }
            };
            yield return Toils_Reserve.Release(TargetIndex.B);
            yield return new Toil
            {
                initAction = delegate
                {
                    IntVec3 position = this.DropBed.Position;
                    Thing thing;
                    this.pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out thing, null);
                    if (!this.DropBed.Destroyed && (this.DropBed.owners.Contains(this.Takee) || (this.DropBed.Medical && this.DropBed.AnyUnoccupiedSleepingSlot) || this.Takee.ownership == null))
                    {
                        this.Takee.jobs.Notify_TuckedIntoBed(this.DropBed);
                        if (this.Takee.RaceProps.Humanlike && this.CurJob.def != JobDefOf.Arrest && !this.Takee.IsPrisonerOfColony)
                        {
                            this.Takee.relations.Notify_RescuedBy(this.pawn);
                        }
                    }
                    if (this.Takee.IsPrisonerOfColony)
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, this.Takee, OpportunityType.GoodToKnow);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
