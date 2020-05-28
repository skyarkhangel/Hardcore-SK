using Verse;
using Verse.AI.Group;

namespace Hospitality
{
    public class TriggerData_VisitorsPleasedMax : TriggerData
    {
        public float threshold = 100;
        public int minStayTicks;
        public int arriveTick;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref threshold, "threshold", 100);
            Scribe_Values.Look(ref arriveTick, "arriveTick");
            Scribe_Values.Look(ref minStayTicks, "minStayTicks");
        }
    }

    public class Trigger_VisitorsPleasedMax : Trigger
    {
        private const int CheckInterval = 800;

        private TriggerData_VisitorsPleasedMax Data { get { return data as TriggerData_VisitorsPleasedMax; } }

        public Trigger_VisitorsPleasedMax(float threshold)
        {
            data = new TriggerData_VisitorsPleasedMax { threshold = threshold,arriveTick = Find.TickManager.TicksGame, minStayTicks = Rand.Range(40000,75000) };
        }

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            //Log.Message((Find.TickManager.TicksGame - Data.arriveTick) + " ticks have passed since arrival");
            if (Find.TickManager.TicksGame < Data.arriveTick + Data.minStayTicks) return false;

            if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame % CheckInterval == 0)
            {
                if (Data != null && lord.faction.PlayerGoodwill >= Data.threshold) return true;
            }
            return false;
        }
    }

    public class TriggerData_VisitorsAngeredMax : TriggerData
    {
        public float threshold = -100;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref threshold, "threshold", -100);
        }

    }

    public class Trigger_VisitorsAngeredMax : Trigger
    {
        private const int CheckInterval = 800;

        private TriggerData_VisitorsAngeredMax Data { get { return data as TriggerData_VisitorsAngeredMax; } }

        private float threshold;
        
        public Trigger_VisitorsAngeredMax(float threshold) // threshold is dynamic, save in data
        {
            this.threshold = threshold;
        }

        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksGame%CheckInterval == 0)
            {
                if (Data != null && lord.faction.PlayerGoodwill <= Data.threshold) return true;
            }
            return false;
        }
    }
}
