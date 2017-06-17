using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using RimWorld.Planet;

namespace Nandonalt_ColonyLeadership
{
    class Job_Learn : JobDriver
    {
        private TargetIndex Build = TargetIndex.A;
        private TargetIndex Facing = TargetIndex.B;
        private TargetIndex Spot = TargetIndex.C;
        public List<SkillDef> skillPool = new List<SkillDef>();
        private Pawn setTeacher = null;

        protected Building_TeachingSpot Spott
        {
            get
            {
                return (Building_TeachingSpot)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

            protected Pawn TeacherPawn
        {
            get
            {
                if (setTeacher != null) return setTeacher;
                if (Spott.teacher != null) { setTeacher = Spott.teacher; return Spott.teacher; }
                else
                {
                    foreach (Pawn pawn in this.pawn.Map.mapPawns.FreeColonistsSpawned)
                    {
                        if (pawn.CurJob.def.defName == "TeachLesson") { setTeacher = pawn; return pawn; }
                    }
                }
                return null;
            }
        }

        public override void ExposeData()
        {
           base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.setTeacher, "setTeacher");
            Scribe_Collections.Look<SkillDef>(ref this.skillPool, "skillPool", LookMode.Undefined, new object[0]);
        }

        public bool setupSkills(Pawn teacher)
        {
            String leaderType = TeachingUtility.getLeaderType(teacher);
            if (leaderType == "leader1")
            {
                if(teacher.skills.GetSkill(SkillDefOf.Growing).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Growing);
                if (teacher.skills.GetSkill(SkillDefOf.Medicine).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Medicine);
                if (teacher.skills.GetSkill(SkillDefOf.Animals).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Animals);
             
            }
            else if (leaderType == "leader2")
            {
                if (teacher.skills.GetSkill(SkillDefOf.Shooting).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Shooting);
                if (teacher.skills.GetSkill(SkillDefOf.Melee).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Melee);
           
           }
            else if (leaderType == "leader3")
            {
                if (teacher.skills.GetSkill(SkillDefOf.Construction).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Construction);
                if (teacher.skills.GetSkill(SkillDefOf.Crafting).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Crafting);
                if (teacher.skills.GetSkill(SkillDefOf.Artistic).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Artistic);
           
            }
            else if(leaderType == "leader4")
            {
                if (teacher.skills.GetSkill(SkillDefOf.Intellectual).Level >= TeachingUtility.minSkill) this.skillPool.Add(SkillDefOf.Intellectual);
              
            }

            if (this.skillPool.NullOrEmpty()) return false;
            return true;
        }



        protected override IEnumerable<Toil> MakeNewToils()
        {
            rotateToFace = Facing;

            this.AddEndCondition(delegate
            {
                 if (TeacherPawn.CurJob.def.defName != "TeachLesson")
                {
                    return JobCondition.Incompletable;
                }
                return JobCondition.Ongoing;
            });
            this.EndOnDespawnedOrNull(Spot, JobCondition.Incompletable);
            this.EndOnDespawnedOrNull(Build, JobCondition.Incompletable);


            yield return Toils_Reserve.Reserve(Spot, this.CurJob.def.joyMaxParticipants, 0, null);
            Toil gotoPreacher;
            if (this.TargetC.HasThing)
            {
                gotoPreacher = Toils_Goto.GotoThing(Spot, PathEndMode.OnCell);
            }
            else
            {
                gotoPreacher = Toils_Goto.GotoCell(Spot, PathEndMode.OnCell);
            }
            yield return gotoPreacher;


            bool b = setupSkills(this.Spott.teacher);
            Toil spotToil = new Toil();
            spotToil.defaultCompleteMode = ToilCompleteMode.Delay;
            spotToil.defaultDuration = 9999;
            spotToil.AddPreTickAction(() =>
            {
                this.pawn.GainComfortFromCellIfPossible();
                this.ticksLeftThisToil = 9999;
                this.pawn.Drawer.rotator.FaceCell(TargetB.Cell);

                //LEARN
                Pawn actor = this.pawn;

                String leaderReport = this.Spott.teacher.jobs.curDriver.GetReport();
            if (leaderReport == "TeachingDesc".Translate() || leaderReport == "FinishLessonDesc".Translate())
                {
                    actor.skills.Learn(this.skillPool.RandomElementByWeight(delegate (SkillDef d)
{
return 1f + this.Spott.teacher.skills.GetSkill(d).Level;
}), TeachingUtility.learningFactor * Spott.GetStatValue(StatDef.Named("LearningSpeedFactor"), true));
                }
                  
                //

                if (TeacherPawn.CurJob.def.defName != "TeachLesson" || !b)
                {
                    this.ticksLeftThisToil = -1;
                }
            });
            yield return spotToil;

            yield return Toils_Reserve.Release(Spot);

            this.AddFinishAction(() =>
            {
            
                if (Spott.currentLessonState == Building_TeachingSpot.LessonState.finishing ||
                   Spott.currentLessonState == Building_TeachingSpot.LessonState.finished)
                {
                    if(Rand.Range(0f,1f) < 0.8f)this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LessonPositive"), this.Spott.teacher);
                    else this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("LessonNegative"), this.Spott.teacher);
                    //CultUtility.AttendWorshipTickCheckEnd(PreacherPawn, this.pawn);
                    //Cthulhu.Utility.DebugReport("Called end tick check");
                    //what happens to learner
                }
                if (this.TargetC.HasThing)
                {
                    if (Map.reservationManager.IsReserved(this.CurJob.targetC.Thing, Faction.OfPlayer))
                        Map.reservationManager.Release(this.CurJob.targetC.Thing, pawn);
                }
                else
                {
                    if (Map.reservationManager.IsReserved(this.CurJob.targetC.Cell, Faction.OfPlayer))
                        Map.reservationManager.Release(this.CurJob.targetC.Cell, this.pawn);
                }


            });
            yield break;
        }
    }
}
