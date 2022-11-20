using RimWorld;
using System;
using Verse;
using Verse.AI;

namespace AnimalsLogic
{
    public class ThinkNode_ChancePerHour_AnimalsHaulConfig : ThinkNode_Priority
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            float num = this.MtbHours(pawn);
            if (num <= 0f) // This is why I had to detour whole method. Vanilla ThinkNode_ChancePerHour is hardcoded to return NoJob if MtbHours = 0 while I need it to always return job instead. Also, this is faster since it avoids saving ticks if not necessary. Also, vanilla has hardcoded one hour min delay.
            { // it is not actually too bad though - while it can interfere with other mods, this method is unlikely to be required for anything else and it is runtime only, so no changes in saves
                return base.TryIssueJobPackage(pawn, jobParams);
            }

            ThinkResult result;
            float mtb_duration = Math.Min(MtbHours(pawn) * GenDate.TicksPerHour / 2, 2500);

            if (Find.TickManager.TicksGame < this.GetLastTryTick(pawn) + mtb_duration)
            {
                result = ThinkResult.NoJob;
            }
            else
            {
                this.SetLastTryTick(pawn, Find.TickManager.TicksGame);
                Rand.PushState();
                int salt = Gen.HashCombineInt(base.UniqueSaveKey, 26504059);
                Rand.Seed = pawn.RandSeedForHour(salt);
                bool flag = Rand.MTBEventOccurs(num, GenDate.TicksPerHour, GenDate.TicksPerHour);
                Rand.PopState();
                if (flag)
                {
                    result = base.TryIssueJobPackage(pawn, jobParams);
                }
                else
                {
                    result = ThinkResult.NoJob;
                }
            }
            return result;
        }

        private int GetLastTryTick(Pawn pawn)
        {
            int num;
            int result;
            if (pawn.mindState.thinkData.TryGetValue(base.UniqueSaveKey, out num))
            {
                result = num;
            }
            else
            {
                result = -99999;
            }
            return result;
        }

        private void SetLastTryTick(Pawn pawn, int val)
        {
            pawn.mindState.thinkData[base.UniqueSaveKey] = val;
        }

        float MtbHours(Pawn pawn)
        {
            return Settings.haul_mtb;
        }
    }
}
