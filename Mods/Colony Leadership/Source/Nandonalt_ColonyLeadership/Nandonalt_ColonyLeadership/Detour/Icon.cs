using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Nandonalt_ColonyLeadership
{
    [StaticConstructorOnStartup]
   public class Icon : ColonistBarColonistDrawer
    {

        private static readonly Texture2D DeadColonistTex = ContentFinder<Texture2D>.Get("UI/Misc/DeadColonist", true);

        private static readonly Texture2D Icon_MentalStateNonAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateNonAggro", true);

        private static readonly Texture2D Icon_MentalStateAggro = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MentalStateAggro", true);

        private static readonly Texture2D Icon_MedicalRest = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/MedicalRest", true);

        private static readonly Texture2D Icon_Sleeping = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Sleeping", true);

        private static readonly Texture2D Icon_Fleeing = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Fleeing", true);

        private static readonly Texture2D Icon_Attacking = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Attacking", true);

        private static readonly Texture2D Icon_Idle = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Idle", true);

        private static readonly Texture2D Icon_Burning = ContentFinder<Texture2D>.Get("UI/Icons/ColonistBar/Burning", true);

        private static readonly Texture2D Icon_Star = ContentFinder<Texture2D>.Get("ColonyLeadership/star", true);


        private ColonistBar ColonistBar
        {
            get
            {
                return Find.ColonistBar;
            }
        }

         public void DrawIconsModded(Rect rect, Pawn colonist)
        {
            if (colonist.Dead)
            {
                return;
            }
            float num = 20f * this.ColonistBar.Scale;
            Vector2 vector = new Vector2(rect.x + 1f, rect.yMax - num - 1f);
            bool flag = false;
            if (colonist.CurJob != null)
            {
                JobDef def = colonist.CurJob.def;
                if (def == JobDefOf.AttackMelee || def == JobDefOf.AttackStatic)
                {
                    flag = true;
                }
                else if (def == JobDefOf.WaitCombat)
                {
                    Stance_Busy stance_Busy = colonist.stances.curStance as Stance_Busy;
                    if (stance_Busy != null && stance_Busy.focusTarg.IsValid)
                    {
                        flag = true;
                    }
                }
            }
            Hediff Leader = TeachingUtility.leaderH(colonist);
            if (colonist.InAggroMentalState)
            {
                this.DrawIcon(Icon_MentalStateAggro, ref vector, colonist.MentalStateDef.LabelCap);
            }
            else if (colonist.InMentalState)
            {
                this.DrawIcon(Icon_MentalStateNonAggro, ref vector, colonist.MentalStateDef.LabelCap);
            }
            else if (colonist.InBed() && colonist.CurrentBed().Medical)
            {
                this.DrawIcon(Icon_MedicalRest, ref vector, "ActivityIconMedicalRest".Translate());
            }
            else if (colonist.CurJob != null && colonist.jobs.curDriver.asleep)
            {
                this.DrawIcon(Icon_Sleeping, ref vector, "ActivityIconSleeping".Translate());
            }
            else if (colonist.CurJob != null && colonist.CurJob.def == JobDefOf.FleeAndCower)
            {
                this.DrawIcon(Icon_Fleeing, ref vector, "ActivityIconFleeing".Translate());
            }
            else if (flag)
            {
                this.DrawIcon(Icon_Attacking, ref vector, "ActivityIconAttacking".Translate());
            }
            else if (colonist.mindState.IsIdle && GenDate.DaysPassed >= 1)
            {
                this.DrawIcon(Icon_Idle, ref vector, "ActivityIconIdle".Translate());
            }           
            else if (Leader != null)
            {
                this.DrawIcon(Icon_Star, ref vector, Leader.LabelBase);
           }
     
            if (colonist.IsBurning())
            {
                this.DrawIcon(Icon_Burning, ref vector, "ActivityIconBurning".Translate());
            }
         
        }

        private void DrawIcon(Texture2D icon, ref Vector2 pos, string tooltip)
        {
            float num = 20f * this.ColonistBar.Scale;
            Rect rect = new Rect(pos.x, pos.y, num, num);
            GUI.DrawTexture(rect, icon);
            TooltipHandler.TipRegion(rect, tooltip);
            pos.x += num;
        }

    }
}
