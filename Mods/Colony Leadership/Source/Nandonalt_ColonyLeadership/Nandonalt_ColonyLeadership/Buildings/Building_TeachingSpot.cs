using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using Verse.AI;

namespace Nandonalt_ColonyLeadership
{
    public class Building_TeachingSpot : Building
    {
        public Pawn tempTeacher;
        public Pawn teacher;
        public bool destroyedFlag = false;
        public List<int> seasonSchedule = new List<int>(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        public List<Pawn> teachers = new List<Pawn>(new Pawn[] { null, null });
        public List<Pawn> ignored = new List<Pawn>();
        public int lessonHour = 15;
        public int lastLessonTick = -999999;
        public int lastTryTick = -999999;
        #region states

        public enum State { notinuse = 0, lesson };
        public State currentState = State.notinuse;
        public enum LessonState { off = 0, started, gathering, teaching, finishing, finished };
        public LessonState currentLessonState = LessonState.off;
      
        public void ChangeState(State type)
        {
            if (type == State.notinuse)
            {
                this.currentState = type;
                this.currentLessonState = LessonState.off;
                    }
            else Log.Error("Changed default state of Sacrificial Altar this should never happen.");
      
        }
        public void ChangeState(State type, LessonState worshipState)
        {
            this.currentState = type;
            this.currentLessonState = worshipState;

        }
        #endregion

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // block further ticker work
            destroyedFlag = true;

            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<int>(ref this.seasonSchedule, "seasonSchedule", LookMode.Value, false);
           Scribe_Collections.Look<Pawn>(ref this.teachers, "teachers", LookMode.Reference, false);
            Scribe_Collections.Look<Pawn>(ref this.ignored, "ignored", LookMode.Reference, false);

            Scribe_Values.Look<int>(ref this.lessonHour, "lessonHour", 15, false);
            Scribe_Values.Look<int>(ref this.lastLessonTick, "lastLessonTick", -999999, false);
            Scribe_Values.Look<int>(ref this.lastTryTick, "lastTryTick", -999999, false);

            Scribe_References.Look<Pawn>(ref this.teacher, "teacher");
            Scribe_References.Look<Pawn>(ref this.tempTeacher, "tempTeacher");

            Scribe_Values.Look<State>(ref this.currentState, "currentState", State.notinuse);
            Scribe_Values.Look<LessonState>(ref this.currentLessonState, "currentLessonState", LessonState.off);

        }

        public override void TickRare()
        {
            if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
                return;


            if (!this.Spawned) return;

            // Don't forget the base work
            base.TickRare();

          


            if (this.currentState != State.lesson)
            {
                if (seasonSchedule[GenLocalDate.DayOfSeason(this.Map)] != 0 && seasonSchedule[GenLocalDate.DayOfSeason(this.Map)] != 4)
                {
                    if(GenLocalDate.HourOfDay(this.Map) == this.lessonHour)
                    {
                        TryTimedLesson();
                    }
                }
            }
            else
            {
                for (int i = 0; i < 2; i++)
                {
                    if (teachers[i] != null)
                    {
                        bool flag = false;
                        if (!isLeader(teachers[i]))
                        {
                            flag = true;
                        }
                        if (!teachers[i].IsColonistPlayerControlled)
                        {
                            flag = true;                          
                        }
                        if (teachers[i].Dead)
                        {
                            flag = true;
                        }

                        if (flag) teachers[i] = null; ;
                    }
                }
            }
    
            LessonRareTick();

        }

        
    
        public override void Tick()
        {
            if (destroyedFlag) // Do nothing further, when destroyed (just a safety)
                return;

            if (!this.Spawned) return;

            base.Tick();
            LessonTick();
         

        }


        public void LessonTick()
        {
            if (currentState == State.lesson)
            {
                switch (currentLessonState)
                {
                    case LessonState.started:
                    case LessonState.gathering:
                        if (TeachingUtility.IsActorAvailable(this.teacher))
                        {
                            if (this.teacher.CurJob.def.defName != "TeachLesson")
                            {
                                TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                                return;
                            }
                        }
                      
                        return;
                    case LessonState.finishing:
                        if (!TeachingUtility.IsActorAvailable(this.teacher))
                        {
                            TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                        }
                      
                        return;
                    case LessonState.finished:
                    case LessonState.off:
                        currentState = State.notinuse;
                        return;
                }
            }
        }

        public bool isLeader(Pawn current)
        {
          
               Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            if (h1 != null || h2 != null || h3 != null || h4 != null) { return true; }
            return false;
            
        }

        public void LessonRareTick()
        {


            if (currentState == State.lesson)
            {
                switch (currentLessonState)
                {
                    case LessonState.started:
                    case LessonState.gathering:
                    case LessonState.teaching:
                        if (!TeachingUtility.IsActorAvailable(this.teacher))
                        {
                            TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                            return;
                        }
                        if (this.teacher.CurJob.def.defName != "TeachLesson")
                        {
                            TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                            return;
                        }
                        TeachingUtility.GetLessonGroup(this, Map);
                       
                        return;
                    case LessonState.finishing:
                        if (!TeachingUtility.IsActorAvailable(this.teacher))
                        {
                            TeachingUtility.AbortLesson(this, "TeacherUnavailable".Translate());
                            return;
                        }
                        /*if (this.teacher.CurJob.def != CultDefOfs.ReflectOnWorship)
                            return;*/
                        TeachingUtility.GetLessonGroup(this, Map);
                    
                        return;
                    case LessonState.finished:
                    case LessonState.off:
                        currentState = State.notinuse;
                        return;
                }
            }
        }



        
        private void TryTimedLesson()
        {

                                    
                int teacherInt = seasonSchedule[GenLocalDate.DayOfSeason(this.Map)];
                if (teacherInt >= 3)
                {
                    tempTeacher = TeachingUtility.DetermineTeacher(this.Map);
                }
                else
                {
                    tempTeacher = teachers[teacherInt-1];
                }                               
            
             TryLesson();
        }

        private void TryLesson(bool forced = false)
        {
            String skills = "";
            if (CanGatherToLessonNow(out skills))
            {
                switch (currentLessonState)
                {
                    case LessonState.finished:
                    case LessonState.off:
                     
                        StartLesson(forced, skills);
                        return;

                    case LessonState.started:
                    case LessonState.gathering:
                    case LessonState.finishing:
                        Messages.Message("A leader has started a lesson on the teaching spot.", TargetInfo.Invalid, MessageSound.RejectInput);
                        return;
                }
            }
        }

        public void StartLesson(bool forced = false, String skills = "")
        {
            teacher = tempTeacher;
     

            if (this.Destroyed || !this.Spawned)
            {
                TeachingUtility.AbortLesson(null, "The spot is unavailable.");
                return;
            }
            if (!TeachingUtility.IsActorAvailable(this.teacher))
            {
                TeachingUtility.AbortLesson(this, "TeacherUnavailableNamed".Translate(new object[] { this.teacher.LabelShort }));
                this.teacher = null;
                return;
            }

            FactionBase factionBase = (FactionBase)this.Map.info.parent;

            Messages.Message("LessonGathering".Translate(new object[] { factionBase.Label, teacher.LabelShort }) + skills, TargetInfo.Invalid, MessageSound.Standard);
            ChangeState(State.lesson, LessonState.started);
            //this.currentState = State.started;
            Job job = new Job(DefDatabase<JobDef>.GetNamed("TeachLesson"), this);
            teacher.jobs.jobQueue.EnqueueLast(job);
            teacher.jobs.EndCurrentJob(JobCondition.InterruptForced);
            TeachingUtility.GetLessonGroup(this, Map, forced);
            lastLessonTick = Find.TickManager.TicksGame;
            //  HediffLeader hediff = TeachingUtility.leaderH(teacher);
            //  if (hediff != null) hediff.lastLessonTick = Find.TickManager.TicksGame;
            GameComponent_ColonyLeadership comp = Utility.getCLComp();
            if(comp != null) comp.lastLessonTick = Find.TickManager.TicksGame;


        }




 
        private bool CanGatherToLessonNow(out String skills)
        {
            skills = "";
            if (Find.TickManager.TicksGame < lastTryTick + (GenDate.TicksPerHour / 3))
            {
               
                return false;
            }
            lastTryTick = Find.TickManager.TicksGame;
            if (Find.TickManager.TicksGame < lastLessonTick + GenDate.TicksPerDay - 1000)                
            {
               return RejectMessage("MustWaitForLesson".Translate());                
            }

            GameComponent_ColonyLeadership comp = Utility.getCLComp();
            if(comp != null && Find.TickManager.TicksGame < comp.lastLessonTick + GenDate.TicksPerDay - 1000)
            {
                return RejectMessage("MustWaitForLesson".Translate());
            }

            if (tempTeacher == null) return RejectMessage("NoTeacherSelected".Translate());
            String report = "";
            
            bool hasSkill = TeachingUtility.leaderHasAnySkill(tempTeacher, out report, out skills);
            if (hasSkill && report != "")
            {
                Messages.Message(report, TargetInfo.Invalid, MessageSound.Standard);
            }
            if (!hasSkill) return RejectMessage(report);
            if (tempTeacher.Drafted) return RejectMessage("TeacherDrafted".Translate());
            if (tempTeacher.Dead || this.tempTeacher.Downed) return RejectMessage("TeacherDownedDead".Translate(), this.tempTeacher);
            if (!tempTeacher.CanReserve(this)) return RejectMessage("TeacherTableReserved".Translate());
           
            return true;
        }

        

        private bool RejectMessage(string s, Pawn pawn = null)
        {
            Messages.Message(s, TargetInfo.Invalid, MessageSound.RejectInput);
            if (pawn != null) pawn = null;
            return false;
        }
    
    }
}
