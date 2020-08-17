using Verse;

namespace Hospitality
{
    public abstract class EventAction : IExposable
    {
        public abstract void DoAction();
        public abstract void ExposeData();
    }
}