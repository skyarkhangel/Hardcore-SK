// Decompiled with JetBrains decompiler
// Type: RimWorld.ThoughtWorker_HardWorkerVsLazy
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using Verse;
using Verse.AI;
using RimWorld;

namespace Psychology
{
    public class ThoughtWorker_WorkingPassion : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned)
                return (ThoughtState)false;
            if (!p.RaceProps.Humanlike)
                return (ThoughtState)false;
            if (p.jobs == null)
                return (ThoughtState)false;
            if (p.jobs.curJob == null)
                return (ThoughtState)false;
            JobDef job = p.jobs.curJob.def;
            SkillDef skill = null; //WHY don't jobs have skills associated with them?!
            if (job == JobDefOf.Tame)
                skill = SkillDefOf.Animals;
            else if (job == JobDefOf.Shear)
                skill = SkillDefOf.Animals;
            else if (job == JobDefOf.Slaughter)
                skill = SkillDefOf.Animals;
            else if (job == JobDefOf.Milk)
                skill = SkillDefOf.Animals;
            else if (job == JobDefOf.Train)
                skill = SkillDefOf.Animals;
            else if (job == JobDefOf.BuildRoof)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.Deconstruct)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.FinishFrame)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.FixBrokenDownBuilding)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.Uninstall)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.BuildRoof)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.RemoveRoof)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.SmoothFloor)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.RemoveFloor)
                skill = SkillDefOf.Construction;
            else if (job == JobDefOf.Harvest)
                skill = SkillDefOf.Growing;
            else if (job == JobDefOf.Sow)
                skill = SkillDefOf.Growing;
            else if (job == JobDefOf.DeliverFood)
                skill = SkillDefOf.Medicine;
            else if (job == JobDefOf.FeedPatient)
                skill = SkillDefOf.Medicine;
            else if (job == JobDefOf.TakeToBedToOperate)
                skill = SkillDefOf.Medicine;
            else if (job == JobDefOf.TakeWoundedPrisonerToBed)
                skill = SkillDefOf.Medicine;
            else if (job == JobDefOf.TendPatient)
                skill = SkillDefOf.Medicine;
            else if (job == JobDefOf.VisitSickPawn)
                skill = SkillDefOf.Medicine;
            else if (job == JobDefOf.Rescue)
                skill = SkillDefOf.Medicine;
            else if (job == JobDefOf.AttackMelee)
                skill = SkillDefOf.Melee;
            else if (job == JobDefOf.Mine)
                skill = SkillDefOf.Mining;
            else if (job == JobDefOf.Research)
                skill = SkillDefOf.Research;
            else if (job == JobDefOf.Hunt)
                skill = SkillDefOf.Shooting;
            else if (job == JobDefOf.AttackStatic) //no idea if this is right
                skill = SkillDefOf.Shooting;
            else if (job == JobDefOf.EscortPrisonerToBed)
                skill = SkillDefOf.Social;
            else if (job == JobDefOf.PrisonerAttemptRecruit)
                skill = SkillDefOf.Social;
            else if (job == JobDefOf.PrisonerExecution)
                skill = SkillDefOf.Social;
            else if (job == JobDefOf.PrisonerFriendlyChat)
                skill = SkillDefOf.Social;
            else if (job == JobDefOf.DeliverFood)
                skill = SkillDefOf.Social;
            else if (job == JobDefOf.DoBill)
            {
                TargetInfo info = p.jobs.curJob.GetTarget(TargetIndex.A);
                if (info == null)
                    return (ThoughtState)false;
                IBillGiver billGiver = info.Thing as IBillGiver;
                if (billGiver == null)
                    return (ThoughtState)false;
                skill = billGiver.BillStack.FirstShouldDoNow.recipe.workSkill;
            }
            if (skill == null)
                return (ThoughtState)false;
            if (p.skills.GetSkill(skill).passion != Passion.Major)
                return (ThoughtState)false;
            return (ThoughtState)true;
        }
    }
}
