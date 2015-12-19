using System;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Core_SK
{
    public class Building_Holodeck : Building
    {
        private int fireDelay = 6;
        private string skillDefName = "Social";
        private int skillIncrease = 1;
        public CompMannable WhoIsManningMe;
//        private static readonly Material IconOff = MaterialPool.MatFrom("Things/Building/SCMisc/Holodeck1x2");
//        private static readonly Material IconOn = MaterialPool.MatFrom("Things/Building/SCMisc/HolodeckOn1x2");
        
//        public override Material DrawMaterial(IntRot rot)
//        {
//            return (this.WhoIsManningMe.MannedNow) ? Building_Holodeck.IconOn : Building_Holodeck.IconOff;
//       }
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            WhoIsManningMe = base.GetComp<CompMannable>();
        }
        public override void Tick()
        {
            base.Tick();
            if (this.WhoIsManningMe.MannedNow)
            {
                this.fireDelay--;
                Pawn pawn = this.WhoIsManningMe.ManningPawn;
                if (this.fireDelay <= 0)
                {
                    this.fireDelay = 6;
//                    MoteThrower.MakeSpeechOverlay(pawn);
                    foreach (SkillRecord current in pawn.skills.skills)
                    {
                        if (current.def.defName == this.skillDefName)
                        {
                            current.Learn((float)this.skillIncrease);
                        }
                        if (pawn.needs.food.CurLevel <= 0.25 || pawn.needs.rest.CurLevel <= 0.25)
                        {
                            pawn.jobs.StopAll();
                            pawn.drafter.Drafted = false;
                        }
                    }
                }
            }
        }
    }
}