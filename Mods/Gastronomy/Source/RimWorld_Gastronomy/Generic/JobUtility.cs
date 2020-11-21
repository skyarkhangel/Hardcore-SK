using Gastronomy.Dining;
using Gastronomy.Waiting;
using Verse;
using Verse.AI;

namespace Gastronomy
{
    internal static class JobUtility
    {
        public static T FailOnRestaurantClosed<T>(this T f) where T : IJobEndable
        {
            JobCondition OnRestaurantClosed() => f.GetActor().GetRestaurant().IsOpenedRightNow ? JobCondition.Ongoing : JobCondition.Incompletable;

            f.AddEndCondition(OnRestaurantClosed);
            return f;
        }

        public static T FailOnDangerous<T>(this T f) where T : IJobEndable
        {
            JobCondition OnRegionDangerous()
            {
                Pawn pawn = f.GetActor();
                var check = RestaurantUtility.IsRegionDangerous(pawn, pawn.GetRegion());
                if (!check) return JobCondition.Ongoing;
                Log.Message($"{pawn.NameShortColored} failed {pawn.CurJobDef.label} because of danger ({pawn.GetRegion().DangerFor(pawn)})");
                return JobCondition.Incompletable;
            }

            f.AddEndCondition(OnRegionDangerous);
            return f;
        }

        public static T FailOnDurationExpired<T>(this T f) where T : IJobEndable
        {
            JobCondition OnDurationExpired()
            {
                var pawn = f.GetActor();
                if (pawn.jobs.curDriver.ticksLeftThisToil > 0) return JobCondition.Ongoing;
                Log.Message($"{pawn.NameShortColored} failed {pawn.CurJobDef?.label} because of timeout.");
                return JobCondition.Incompletable;
            }

            f.AddEndCondition(OnDurationExpired);
            return f;
        }

        public static T FailOnNotDining<T>(this T f, TargetIndex patronInd) where T : IJobEndable
        {
            JobCondition PatronIsNotDining()
            {
                var patron = f.GetActor().jobs.curJob.GetTarget(patronInd).Thing as Pawn;
                if (patron?.jobs.curDriver is JobDriver_Dine) return JobCondition.Ongoing;
                Log.Message($"Checked {patron?.NameShortColored}. Not dining >> failing {f.GetActor().NameShortColored}'s job {f.GetActor().CurJobDef?.label}.");
                return JobCondition.Incompletable;
            }

            f.AddEndCondition(PatronIsNotDining);
            return f;
        }

        public static T FailOnNotDiningQueued<T>(this T f, TargetIndex patronInd) where T : IJobEndable
        {
            JobCondition PatronHasNoDiningInQueue()
            {
                var patron = f.GetActor().jobs.curJob.GetTarget(patronInd).Thing as Pawn;
                if (patron.HasDiningQueued()) return JobCondition.Ongoing;
                Log.Message($"Checked {patron?.NameShortColored}. Not planning to dine >> failing {f.GetActor().NameShortColored}'s job {f.GetActor().CurJobDef?.label}.");
                return JobCondition.Incompletable;
            }

            f.AddEndCondition(PatronHasNoDiningInQueue);
            return f;
        }

        public static T GetDriver<T>(this Pawn patron) where T : JobDriver
        {
            // Current
            return patron?.jobs?.curDriver as T;
            // It's not possible to get a driver from queue (only jobs are queued)
        }
    }
}
