using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Hospitality
{
    public abstract class Alert_GuestThought : Alert_Guest
    {
        private static readonly List<Thought> tmpThoughts = new List<Thought>();
        protected abstract ThoughtDef Thought { get; }

        protected override void UpdateAffectedPawnsCache()
        {
            affectedPawnCache.Clear();
            var allGuests = Find.Maps.SelectMany(m => m.GetMapComponent()?.PresentGuests);
            foreach (var guest in allGuests)
            {
                if(guest.Dead) continue;
                if(guest.needs.mood == null) continue;

                guest.needs.mood.thoughts.GetAllMoodThoughts(tmpThoughts);
                if (tmpThoughts.Any(t => t.def == Thought && !ThoughtUtility.ThoughtNullified(guest, t.def)))
                {
                    affectedPawnCache.Add(guest);
                }
                tmpThoughts.Clear();
            }
        }
    }
}
