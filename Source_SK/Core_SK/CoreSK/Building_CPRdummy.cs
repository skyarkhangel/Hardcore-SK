using System;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Core_SK
{
    public class Building_CPRdummy : Building
    {
        private int fireDelay = 6;
        private string skillDefName = "Medicine";
        private int skillIncrease = 1;
        public CompMannable WhoIsManningMe;
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
//                    MoteThrower.ThrowDustPuff(pawn.TrueCenter(), 1f);
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
