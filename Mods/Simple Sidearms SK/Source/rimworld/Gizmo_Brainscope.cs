using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SimpleSidearms.rimworld
{
    class Gizmo_Brainscope : Gizmo
    {
        public const float ContentPadding = 2f;
        public Pawn parent;

        public static Dictionary<Pawn, string> curJobs = new Dictionary<Pawn, string>();
        public static Dictionary<Pawn, string> lastJobs = new Dictionary<Pawn, string>();

        public Gizmo_Brainscope(Pawn parent)
        {
            this.parent = parent;
        }

        public override float GetWidth(float maxWidth)
        {
            return 250f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            return GizmoOnGUI_old(topLeft, maxWidth);
        }

        private GizmoResult GizmoOnGUI_old(Vector2 topLeft, float maxWidth)
        {
            var gizmoRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            var contentRect = gizmoRect.ContractedBy(ContentPadding);
            Widgets.DrawWindowBackground(gizmoRect);

            if(parent == null)
            {
                var str1Rect = new Rect(contentRect.x, contentRect.y - 3f, contentRect.width, 22f);
                GUI.color = Color.white;
                GUI.Label(str1Rect, "Pawn is null");
            }
            else 
            {
                string curJob = parent.CurJobDef == null ? "null" : parent.CurJobDef.defName;
                if (!curJobs.ContainsKey(parent))
                {
                    curJobs[parent] = curJob;
                    lastJobs[parent] = "none yet";
                }
                else if (curJobs[parent] != curJob)
                {
                    lastJobs[parent] = curJobs[parent];
                    curJobs[parent] = curJob;
                }
                string curJobDriverStr = parent.jobs?.curDriver == null ? "null" : parent.jobs.curDriver.ToString();
                //string toilStr = "null";
                //string toilActiveSkillStr = "null";
                /*if (parent?.jobs?.curDriver != null)
                {
                    Toil toil = Traverse.Create(parent.jobs.curDriver).Property("CurToil").GetValue() as Toil;
                    if (toil != null)
                    {
                        toilStr = toil.ToString();
                        if (toil.activeSkill != null)
                            toilActiveSkillStr = toil.activeSkill().ToString();
                    }
                }*/


                var font = Text.Font;
                Text.Font = GameFont.Tiny;

                float offset = 0;
                printBool("Idle:", parent.mindState?.IsIdle, contentRect, 0); offset += 10f;
                printStringPair("Job:", curJobs.ContainsKey(parent) ? curJobs[parent] : "null", Color.white, contentRect, offset); offset += 10f;
                printStringPair("Last job:", lastJobs.ContainsKey(parent) ? lastJobs[parent] : "null", Color.white, contentRect, offset); offset += 10f;
                printStringPair("JobDriver:", curJobDriverStr, Color.white, contentRect, offset); offset += 10f;
                //printStringPair("Toil:", toilStr, Color.white, contentRect, offset); offset += 10f;
                //printStringPair("T. actSk:", toilActiveSkillStr, Color.white, contentRect, offset);

                Text.Font = font;
            }

            if (Widgets.ButtonText(contentRect.RightPartPixels(15), "<>"))
            {
                var tickManager = Find.TickManager;
                if (!tickManager.Paused)
                    tickManager.TogglePaused();

                tickManager.DoSingleTick();
            }

            return new GizmoResult(GizmoState.Clear);
        }

        public void printString(string str, Rect contentRect, float offset)
        {
            var str1Rect = new Rect(contentRect.x, contentRect.y + offset - 3f, contentRect.width, 22f);
            GUI.color = Color.white;
            GUI.Label(str1Rect, str);
        }

        public void printBool(string label, bool? value, Rect contentRect, float offset)
        {
            Color color = value.HasValue ? (value.Value ? Color.green : Color.red) : Color.gray;
            printStringPair(label, value.HasValue ? value.Value.ToString() : "null", color, contentRect, offset);
        }

        public void printStringPair(string str1, string str2, Color secondStrColor, Rect contentRect, float offset)
        {
            var str1Rect = new Rect(contentRect.x, contentRect.y + offset - 3f, contentRect.width/4, 22f);
            var str2Rect = new Rect(contentRect.x + contentRect.width/4, contentRect.y + offset - 3f, (contentRect.width/4) * 3, 22f);
            GUI.color = Color.white;
            GUI.Label(str1Rect, str1);
            GUI.color = secondStrColor;
            GUI.Label(str2Rect, str2);
        }
    }
}
