using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Text;
using Verse.Sound;

namespace Nandonalt_ColonyLeadership
{
    public class ITab_LessonSchedule : ITab
    {
        public static Vector2 CardSize = new Vector2(350f, 350f);
        private static float hourWidth;

        private Building_TeachingSpot Spot
        {
            get
            {
                return (Building_TeachingSpot)base.SelThing;
            }
        }

        public ITab_LessonSchedule()
        {
            this.size = CardSize;
            this.labelKey = "Schedule";
            this.tutorTag = "Schedule";
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(5f);
            DrawCard(rect, Spot);
        }

        public static void DrawCard(Rect rect, Building_TeachingSpot spot)
        {
            //GUI.BeginGroup(rect);
            //Rect position = new Rect(0f, 0f, rect.width, 65f);
            Rect position = rect;
            hourWidth = 20.833334f;
            float num = 15f;
            float num2 = 250f;
            Text.Font = GameFont.Medium;
            Rect title = new Rect(rect.x + 5, rect.y + 5, rect.width, rect.height);
            Widgets.Label(title, "LessonSchedule".Translate());

            Text.Font = GameFont.Small;

            Rect text = new Rect(rect.x + 5, rect.y + 40, 300f, 240f);
            Widgets.Label(text, "ScheduleText".Translate());

            
            Rect seasonLabel = new Rect(rect.x + 110f, rect.y + 5 + 230f, 150f, 200f);
            Widgets.Label(seasonLabel, "SeasonDays".Translate());

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.LowerLeft;
                    

            for (int i = 1; i <= 15; i++)
            {
                Rect rect2 = new Rect(num + 4f, num2 + 0f, hourWidth, 30f);
                Widgets.Label(rect2, i.ToString());
                Rect rect3 = new Rect(num, num2 + 30f, hourWidth, 30f);
                DoTimeAssignment(rect3, i - 1, spot);
                num += hourWidth;
            }        

            Text.Anchor = TextAnchor.UpperLeft;
            float dist = 25f;


            Rect button = new Rect(rect.x + dist, rect.y + 175, 140f, 30f);
            String name = (spot.teachers[0] == null ? "NoneL".Translate() : spot.teachers[0].Name.ToStringShort);
            if (Widgets.ButtonText(button, "RedTeacher".Translate() + name, true, false, true))
            {
                listPawns(0, spot);
            }

            Rect button2 = new Rect(rect.x + dist + 150f, rect.y + 175, 140f, 30f);
            String name2 = (spot.teachers[1] == null ? "NoneL".Translate() : spot.teachers[1].Name.ToStringShort);
            if (Widgets.ButtonText(button2, "BlueTeacher".Translate() + name2, true, false, true))
            {
                listPawns(1, spot);
            }

            Rect button3 = new Rect(rect.x + dist, rect.y + 135f, 140f, 30f);
            String hour = spot.lessonHour.ToString() + ":00h";
            if (Widgets.ButtonText(button3, "LessonStart".Translate() + hour, true, false, true))
            {
                listHours(spot);
            }

            Rect button4= new Rect(rect.x + dist + 150f, rect.y + 135f, 140f, 30f);            
            if (Widgets.ButtonText(button4, "IgnoreList".Translate(), true, false, true))
            {
                Find.WindowStack.Add(new Dialog_ChooseIgnored(spot)); //
            }


            // GUI.EndGroup();
        }



        #region list

        public static void listHours(Building_TeachingSpot spot)
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<int> availableHours = new List<int>(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 });
                       
        
            foreach (int i in availableHours)
            {
                list.Add(new FloatMenuOption(i.ToString() + ":00h", delegate
                {
                    spot.lessonHour = i;
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
            }


            Find.WindowStack.Add(new FloatMenu(list));
        }

        public static void listPawns(int index, Building_TeachingSpot spot)
        {

           
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            List<Pawn> tpawns = new List<Pawn>();

            foreach (Pawn current in IncidentWorker_LeaderElection.getAllColonists())
            {
                Hediff h1 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader1"));
                Hediff h2 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader2"));
                Hediff h3 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader3"));
                Hediff h4 = current.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("leader4"));

                if (h1 != null || h2 != null || h3 != null || h4 != null) { if (!spot.teachers.Contains(current)) { tpawns.Add(current); } }

            }


            list.Add(new FloatMenuOption("-None-", delegate
            {
                spot.teachers[index] = null;
            }, MenuOptionPriority.Default, null, null, 0f, null, null));

            foreach (Pawn p in tpawns)
            {
                list.Add(new FloatMenuOption(p.Name.ToStringShort, delegate
                {
                    String report = "";
                    String skills = "";
                    bool hasSkill = TeachingUtility.leaderHasAnySkill(p, out report, out skills);
                    if (hasSkill)
                    {
                        spot.teachers[index] = p;
                        if(report != "")Messages.Message(report, TargetInfo.Invalid, MessageSound.Standard);
                    }                
                    else {
                        spot.teachers[index] = null;
                        Messages.Message(report, TargetInfo.Invalid, MessageSound.RejectInput); }
                  
                }, MenuOptionPriority.Default, null, null, 0f, null, null));
            }


            Find.WindowStack.Add(new FloatMenu(list));

        }
        #endregion


        private static void DoTimeAssignment(Rect rect, int day, Building_TeachingSpot spot)
        {
            rect = rect.ContractedBy(1f);
            Texture2D texture = TimeAssignmentDefOf.Anything.ColorTexture;
            if (spot.seasonSchedule[day] == 1) texture = ModTextures.RedColor;
            else if (spot.seasonSchedule[day] == 2) texture = ModTextures.BlueColor;
            else if (spot.seasonSchedule[day] == 3) texture = ModTextures.YellowColor;

            GUI.DrawTexture(rect, texture);
            if (Mouse.IsOver(rect))
            {
                Widgets.DrawBox(rect, 2);
                //if (Input.GetMouseButton(0))
                if (Widgets.ButtonInvisible(rect))
                {
                    spot.seasonSchedule[day] = (spot.seasonSchedule[day] % 4) + 1;
                    SoundDefOf.DesignateDragStandardChanged.PlayOneShotOnCamera();
                    //p.timetable.SetAssignment(hour, this.selectedAssignment);

                }
            }
        }
    }
}

       


