using Verse;
using Verse.AI.Group;

namespace Hospitality
{
    public class Trigger_SentAway : Trigger
    {
        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.Tick)
            {
                return lord?.ownedPawns.Any(SentAway) == true;
            }
            return false;
        }

        private static bool SentAway(Pawn pawn)
        {
            if (pawn?.health?.hediffSet == null) return false;
            return pawn.CompGuest()?.sentAway == true;
        }
    }
}