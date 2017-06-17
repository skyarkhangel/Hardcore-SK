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
    class TeachingUtility
    {
        public static float learningFactor = 0.28f;
        public static int remainingDuration = 1100; // 15 second timer
        public static int ritualDuration = 1100; // 15 seconds max
        public static int reflectDuration = 600; // 10 seconds max
        public static int minSkill = 8;


        public static String getLeaderType(Pawn current)
        {

            HediffLeader h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            HediffLeader h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            HediffLeader h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            HediffLeader h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            String s = "leader1";

            if (h2 != null) { s = "leader2"; }
            if (h3 != null) { s = "leader3"; }
            if (h4 != null) { s = "leader4"; }
            return s;
        }

        public static Pawn DetermineTeacher(Map map)
        {
            Pawn result = null;
            List<Pawn> pawns = IncidentWorker_LeaderElection.getAllColonists();
            List<Pawn> tpawns = new List<Pawn>();
            foreach (Pawn current in pawns)
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));

                if (h1 != null || h2 != null || h3 != null || h4 != null) { if(current.Map == map) tpawns.Add(current); }
               
            }     
           

            return tpawns.RandomElement();
        }

        public static HediffLeader leaderH(Pawn current)
        {
            Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            if (h1 != null) return (HediffLeader)h1;
            h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            if (h1 != null) return (HediffLeader)h1;
            h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            if (h1 != null) return (HediffLeader)h1;
            h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
            if (h1 != null) return (HediffLeader)h1;
            return null;
        }


        public static void AbortLesson(Building_TeachingSpot spot)
        {
            if (spot != null) spot.ChangeState(Building_TeachingSpot.State.notinuse);
        }
        public static void AbortLesson(Building_TeachingSpot spot, String reason)
        {
            if (spot != null) spot.ChangeState(Building_TeachingSpot.State.notinuse);
            Messages.Message(reason + " Aborting lesson.", MessageSound.Negative);
        }

        public static bool IsActorAvailable(Pawn actor, bool downedAllowed = false)
        {
            StringBuilder s = new StringBuilder();
            s.AppendLine("ActorAvailable Checks Initiated");
            if (actor == null)
                return false;
            s.AppendLine("ActorAvailable: Passed null Check");
            if (actor.Dead)
                return false;
            s.AppendLine("ActorAvailable: Passed not-dead");
            if (actor.health == null)
                return false;
            s.AppendLine("ActorAvailable: Passed health check");
            if (actor.health.capacities == null)
                return false;
            s.AppendLine("ActorAvailable: Passed capacities check");
            if (actor.Downed && !downedAllowed)
                return false;
            s.AppendLine("ActorAvailable: Passed downed check & downedAllowed = " + downedAllowed.ToString());
            if (actor.Drafted)
                return false;
            s.AppendLine("ActorAvailable: Passed drafted check");
            if (actor.InAggroMentalState)
                return false;
            s.AppendLine("ActorAvailable: Passed drafted check");
            if (actor.InMentalState)
                return false;
            s.AppendLine("ActorAvailable: Passed InMentalState check");
            s.AppendLine("ActorAvailable Checks Passed");
            return true;
        }


        public static bool ShouldAttendLesson(Pawn p, Building_TeachingSpot spot)
        {
            if (!IsActorAvailable(spot.teacher))
            {
                AbortLesson(spot);
                return false;
            }
            //Everyone get over here!
            if (p != spot.teacher)
            {
                return true;
            }

            return false;
        }

        public static void GetLessonGroup(Building_TeachingSpot spot, Map map, bool forced = false)
        {
            Room room = spot.GetRoom();

            if (room.Role != RoomRoleDefOf.PrisonBarracks && room.Role != RoomRoleDefOf.PrisonCell)
            {
                List<Pawn> listeners = new List<Pawn>();
                if (forced)
                {
                    listeners = map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike && !x.Downed && !x.Dead &&
                                                                                  x.CurJob.def.defName != "AttendLesson" &&
                                                                                  x.CurJob.def != JobDefOf.ExtinguishSelf &&
                                                                                  x.CurJob.def != JobDefOf.Rescue && 
                                                                                  x.CurJob.def != JobDefOf.TendPatient &&
                                                                                  x.CurJob.def != JobDefOf.BeatFire &&   
                                                                                  !spot.ignored.Contains(x) &&                                                                                                               
                                                                                  x.CurJob.def != JobDefOf.FleeAndCower && 
                                                                                  !x.InAggroMentalState && !x.InMentalState) ;
                }
                else
                {
                    listeners = map.mapPawns.AllPawnsSpawned.FindAll(x => x.RaceProps.intelligence == Intelligence.Humanlike && !x.Downed && !x.Dead &&
                                                                                 x.CurJob.def.defName != "AttendLesson" &&
                                                                                  x.CurJob.def != JobDefOf.ExtinguishSelf && 
                                                                                  x.CurJob.def != JobDefOf.Rescue && 
                                                                                  x.CurJob.def != JobDefOf.TendPatient && 
                                                                                  x.CurJob.def != JobDefOf.BeatFire && 
                                                                                  x.CurJob.def != JobDefOf.Lovin && 
                                                                                  x.CurJob.def != JobDefOf.LayDown &&
                                                                                   !spot.ignored.Contains(x) &&
                                                                                  x.CurJob.def != JobDefOf.FleeAndCower && 
                                                                                  !x.InAggroMentalState && !x.InMentalState);
                }
                bool[] flag = new bool[listeners.Count];
                for (int i = 0; i < listeners.Count; i++)
                {
                    if (!flag[i] && TeachingUtility.ShouldAttendLesson(listeners[i], spot))
                    {
                        GiveAttendLessonJob(spot, listeners[i]);
                        flag[i] = true;
                    }
                }
            }
        }

        public static bool IsTeacher(Pawn p)
        {
            List<Thing> list = p.Map.listerThings.AllThings.FindAll(s => s.GetType() == typeof(Building_TeachingSpot));
            foreach (Building_TeachingSpot b in list)
            {
                if (b.teacher == p) return true;
            }
            return false;
        }

        public static void GiveAttendLessonJob(Building_TeachingSpot spot, Pawn attendee)
        {
            if (IsTeacher(attendee)) return;
            if (attendee.Drafted) return;
            if (attendee.IsPrisoner) return;
            if (attendee.jobs.curJob.def.defName == "AttendLesson") return;

            IntVec3 result;
            Building chair;
            if (!WatchBuildingUtility.TryFindBestWatchCell(spot, attendee, true, out result, out chair))
            {
                if (!WatchBuildingUtility.TryFindBestWatchCell(spot as Thing, attendee, false, out result, out chair))
                {
                    return;
                }
            }

            int dir = spot.Rotation.Opposite.AsInt;

            if (chair != null)
            {
                IntVec3 newPos = chair.Position + GenAdj.CardinalDirections[dir];

                Job J = new Job(DefDatabase<JobDef>.GetNamed("AttendLesson"), spot, newPos, chair);                
                J.playerForced = true;
                J.ignoreJoyTimeAssignment = true;
                J.expiryInterval = 9999;
                J.ignoreDesignations = true;
                J.ignoreForbidden = true;
                attendee.jobs.jobQueue.EnqueueLast(J);
                attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
            {
                IntVec3 newPos = result + GenAdj.CardinalDirections[dir];

                Job J = new Job(DefDatabase<JobDef>.GetNamed("AttendLesson"), spot, newPos, result);
                J.playerForced = true;
                J.ignoreJoyTimeAssignment = true;
                J.expiryInterval = 9999;
                J.ignoreDesignations = true;
                J.ignoreForbidden = true;
                attendee.jobs.jobQueue.EnqueueLast(J);
                attendee.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
        }

        public static void TeachingComplete(Pawn teacher, Building_TeachingSpot spot)
        {
            spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.finishing);

            spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.finished);
            //altar.currentState = Building_SacrificialAltar.State.finished;

            FactionBase factionBase = (FactionBase)spot.Map.info.parent;

            Messages.Message("The lesson has finished.", TargetInfo.Invalid, MessageSound.Benefit);
        }

        public static bool leaderHasAnySkill(Pawn teacher, out String report, out String skills)
        {
            String leaderType = TeachingUtility.getLeaderType(teacher);
            List<SkillDef> teachable = new List<SkillDef>();
            String missing = "";
            skills = "";
            bool flag = false;
            if (leaderType == "leader1")
            {
                if (teacher.skills.GetSkill(SkillDefOf.Growing).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Growing);
                if (teacher.skills.GetSkill(SkillDefOf.Medicine).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Medicine);
                if (teacher.skills.GetSkill(SkillDefOf.Animals).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Animals);
                if (teachable.Count < 3) flag = true;
                missing = "SkillSet1".Translate();
            }
            else if (leaderType == "leader2")
            {
                if (teacher.skills.GetSkill(SkillDefOf.Shooting).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Shooting);
                if (teacher.skills.GetSkill(SkillDefOf.Melee).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Melee);
                if (teachable.Count < 2) flag = true;
                missing = "SkillSet2".Translate();
            }
            else if (leaderType == "leader3")
            {
                if (teacher.skills.GetSkill(SkillDefOf.Construction).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Construction);
                if (teacher.skills.GetSkill(SkillDefOf.Crafting).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Crafting);
                if (teacher.skills.GetSkill(SkillDefOf.Artistic).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Artistic);
                if (teachable.Count < 3) flag = true;
                missing = "SkillSet3".Translate();
            }
            else if (leaderType == "leader4")
            {
                if (teacher.skills.GetSkill(SkillDefOf.Intellectual).Level >= TeachingUtility.minSkill) teachable.Add(SkillDefOf.Intellectual);
                if (teachable.Count < 1) flag = true;
                missing = "SkillSet4".Translate();
            }

            if (teachable.NullOrEmpty())
            {
                report = "MustHaveSkill".Translate(new object[] { TeachingUtility.minSkill }) + missing;
                return false;
            }
            else if (flag)
            {
                report = "OnlyTeachIfSkill".Translate(new object[] { TeachingUtility.minSkill }) + missing;
                return true;
            }
            report = "";
            skills = missing.ReplaceFirst(" or "," and ");
            return true;
        }
    }
}
