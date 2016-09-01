using System;
using Verse;
using Verse.AI;
using RimWorld;

namespace Psychology
{
    // Token: 0x02000188 RID: 392
    public class ThoughtWorker_PanicAttack : ThoughtWorker
    {
        // Token: 0x0600060A RID: 1546 RVA: 0x0001CD88 File Offset: 0x0001AF88
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            switch ((p.GetHashCode() ^ (GenDate.DayOfYear + GenDate.CurrentYear + (int)(GenDate.CurrentDayPercent * 5) * 60) * 391) % 50)
            {
                default:
                    return (ThoughtState)false;
                case 48:
                case 49:
                    if (p.story.traits.HasTrait(TraitDefOfPsychology.Anxiety))
                    {
                        if(p.jobs.curJob.def != JobDefOf.FleeAndCower)
                            p.jobs.StartJob(new Job(JobDefOf.FleeAndCower, p.Position), JobCondition.InterruptForced, null, false, true, null);
                        return (ThoughtState)true;
                    }
                    return (ThoughtState)false;
            }
        }
    }
}
