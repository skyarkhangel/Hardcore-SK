using PeteTimesSix.SimpleSidearms;
using PeteTimesSix.SimpleSidearms.Utilities;
using RimWorld;
using Verse;
using Verse.AI;
using static PeteTimesSix.SimpleSidearms.Utilities.Enums;

namespace SimpleSidearms.rimworld
{
    public class JobGiver_QuicklySwitchWeapons : ThinkNode_JobGiver
    {

        public static Job TryGiveJobStatic(Pawn pawn, bool inCombat)
        {
            if (RestraintsUtility.InRestraints(pawn))
                return null;
            else
            {
                if (!pawn.IsValidSidearmsCarrier())
                    return null;

                CompSidearmMemory pawnMemory = CompSidearmMemory.GetMemoryCompForPawn(pawn);
                if (pawnMemory == null)
                    return null;

                if (pawnMemory.IsUsingAutotool(true, false))
                {
                    pawnMemory.currentJobWeaponReequipDelayed = true;
                    return null;
                }
                else
                {
                    pawnMemory.currentJobWeaponReequipDelayed = false;
                }

                //Log.Message(pawn.Label+" considering switching weapons on the run");
                WeaponAssingment.equipBestWeaponFromInventoryByPreference(pawn, DroppingModeEnum.Calm);

                //yes, I realise that this never actually results in a job.
                //I might at some point in the future decide to make switching weapons non-instaneous, which will happen here.

                return null;
            }
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            return TryGiveJobStatic(pawn, false);
        }
    }
}
