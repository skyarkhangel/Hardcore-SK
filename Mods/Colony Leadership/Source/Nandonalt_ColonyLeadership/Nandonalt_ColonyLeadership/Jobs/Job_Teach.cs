using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using UnityEngine;

namespace Nandonalt_ColonyLeadership
{
    class Job_Teach : JobDriver
    {
        private const TargetIndex SpotIndex = TargetIndex.A;
        public int tickC=0;

    
        public List<Building_Chalkboard> chalkboards = new List<Building_Chalkboard>();

        protected Building_TeachingSpot Spot
        {
            get
            {
                return (Building_TeachingSpot)base.CurJob.GetTarget(TargetIndex.A).Thing;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Building_Chalkboard>(ref this.chalkboards, "chalkboards", LookMode.Reference);
            Scribe_Values.Look<int>(ref this.tickC, "tickC",0);
        }


     
        public static List<Texture2D> iconList(Pawn current)
        {

            HediffLeader h1 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
            HediffLeader h2 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
            HediffLeader h3 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
            HediffLeader h4 = (HediffLeader)current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));
     

            if (h2 != null) { return ModTextures.icons_leader2; }
            if (h3 != null) { return ModTextures.icons_leader3; }
            if (h4 != null) { return ModTextures.icons_leader4; }
            return ModTextures.icons_leader1; 
        }


        private string report = "";
        public override string GetReport()
        {
            if (report != "")
            {
                return base.ReportStringProcessed(report);
            }
            return base.GetReport();
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
         
            this.FailOnDestroyedOrNull(TargetIndex.A);

            yield return Toils_Reserve.Reserve(SpotIndex, 1, -1, null);

            yield return new Toil
            {
                initAction = delegate
                {
                    Spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.gathering);
                }
            };

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
                 
            Toil waitingTime = new Toil();
            waitingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            TeachingUtility.remainingDuration = TeachingUtility.ritualDuration;
            waitingTime.defaultDuration = TeachingUtility.remainingDuration - 360;
            waitingTime.initAction = delegate
            {
               // Spot.lastLessonTick = Find.TickManager.TicksGame;
               // HediffLeader hediff = TeachingUtility.leaderH(this.pawn);
                //if (hediff != null) hediff.lastLessonTick = Find.TickManager.TicksGame;
                report = "WaitingDesc".Translate();
                MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, ModTextures.waiting);
                Spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.teaching);

                List<Thing> list = GenRadial.RadialDistinctThingsAround(this.pawn.Position, this.pawn.Map, 10f, true).ToList<Thing>();
                foreach (Thing current in list)
                {
                    Building_Chalkboard chalk = current as Building_Chalkboard;
                    bool flag2 = chalk != null;
                    if (flag2)
                    {
                        if (current.def.defName == "ChalkboardCL" && current.Faction == this.pawn.Faction)
                        {
                            if (current.GetRoom() == this.pawn.GetRoom())
                            {
                                string s = TeachingUtility.getLeaderType(this.pawn);
                                chalkboards.Add(chalk);
                                chalk.frame = -1;
                                this.Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlag.Things, true, false);
                            }
                        }
                    }
                }


            };
          
            yield return waitingTime;

       
            for (int i = 0; i < 3 ; i++) {
                Toil teachingTime = new Toil();
                teachingTime.defaultCompleteMode = ToilCompleteMode.Delay;
                TeachingUtility.remainingDuration = TeachingUtility.ritualDuration;
                teachingTime.defaultDuration = TeachingUtility.remainingDuration - 120;
                teachingTime.initAction = delegate
                {
                    string s = TeachingUtility.getLeaderType(this.pawn);
                    foreach (Building_Chalkboard chalk in chalkboards)
                        {
                            if (chalk != null)
                        {                          
                            if (s == "leader1") chalk.state = 1;
                            else if (s == "leader2") chalk.state = 2;
                            else if (s == "leader3") chalk.state = 3;
                            else if (s == "leader4") chalk.state = 4;
                            chalk.frame++;
                            this.Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlag.Things, true, false); }
                        }
                    
                        report = "TeachingDesc".Translate();

                    MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, iconList(this.pawn).RandomElement());
                };

                teachingTime.tickAction = delegate
                {
                    Pawn actor = this.pawn;
                    actor.skills.Learn(SkillDefOf.Social, 0.25f);
                    actor.GainComfortFromCellIfPossible();
                };

                yield return teachingTime;
            }


     
            Toil finishingTime = new Toil();
            finishingTime.defaultCompleteMode = ToilCompleteMode.Delay;
            TeachingUtility.remainingDuration = TeachingUtility.ritualDuration;
            finishingTime.defaultDuration = TeachingUtility.remainingDuration - 360;
            finishingTime.WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
            finishingTime.initAction = delegate
            {
                report = "FinishLessonDesc".Translate();               
                    MoteMaker.MakeInteractionBubble(this.pawn, null, ThingDefOf.Mote_Speech, iconList(this.pawn).RandomElement());
            };
            
            finishingTime.tickAction = delegate
            {
                if(tickC == 120 || tickC == 240 || tickC == 360)
                {
                    foreach (Building_Chalkboard chalk in chalkboards)
                    {
                        if (chalk != null)
                        {
                            if(chalk.frame > -1) chalk.frame--;
                            this.Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlag.Things, true, false);
                        }
                    }

                }
                Pawn actor = this.pawn;
                actor.skills.Learn(SkillDefOf.Social, 0.25f);
                actor.GainComfortFromCellIfPossible();
                tickC++;
            };

            yield return finishingTime;

            yield return new Toil
            {
                initAction = delegate
                {
                    tickC = 0;
                                  TeachingUtility.TeachingComplete(this.pawn, Spot);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            yield return new Toil
            {
                initAction = delegate
                {
                    if (Spot != null)
                    {
                        if (Spot.currentLessonState != Building_TeachingSpot.LessonState.finished)
                        {
                            Spot.ChangeState(Building_TeachingSpot.State.lesson, Building_TeachingSpot.LessonState.finished);
                            //Map.GetComponent<MapComponent_SacrificeTracker>().ClearVariables();
                        }
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };


            this.AddFinishAction(() =>
            {
                foreach(Building_Chalkboard chalk in chalkboards)
                {

                    if (chalk != null) { chalk.frame = 0; chalk.state = 0; this.Map.mapDrawer.MapMeshDirty(chalk.Position, MapMeshFlag.Things, true, false); }
                }
                
                if (Spot.currentLessonState == Building_TeachingSpot.LessonState.finishing ||
                     Spot.currentLessonState == Building_TeachingSpot.LessonState.finished)
                {
                    this.pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("TaughtCL"), null);
                }

            });

            yield break;


        }
    }
}
