using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Hospitality.Utilities
{
    /// <summary>
    /// This cache utility saves all results of <see cref="ThoughtWorkerCached"/> and manages whether it needs an update or not
    /// </summary>
    public static class ThoughtResultCache
    {
        //We save both the last result and the last tick it got cached at
        private static readonly Dictionary<int, ThoughtState> cachedStates = new Dictionary<int, ThoughtState>();
        private static readonly Dictionary<int, int> cachedTick = new Dictionary<int, int>();

        public static void Reset()
        {
            cachedStates.Clear();
            cachedTick.Clear();
        }
        public static void CacheThoughtResult(Pawn forPawn, ThoughtWorkerCached worker, ThoughtState result)
        {
            //For unique caching we get a mix of both the pawn and the worker's hash code
            int hashMix = GenericUtility.CombinedHash(forPawn, worker);
            int curTick = Find.TickManager.TicksGame;

            if (!cachedStates.ContainsKey(hashMix))
            {
                cachedStates.Add(hashMix, result);
                cachedTick.Add(hashMix, curTick);
                return;
            }
            cachedStates[hashMix] = result;
            cachedTick[hashMix] = curTick;
        }

        //If the time between current tick and last cache tick is greater than the interval defined in the worker, we request a new state
        internal static bool NeedsNewState(Pawn forPawn, ThoughtWorkerCached worker, out ThoughtState existingState)
        {
            int hashMix = GenericUtility.CombinedHash(forPawn, worker);
            int curTick = Find.TickManager.TicksGame;

            existingState = cachedStates.TryGetValue(hashMix, false);

            if (cachedTick.TryGetValue(hashMix, out int lastCachedTick))
            {
                return curTick - lastCachedTick > worker.ThoughtCacheInterval;
            }
            return true;
        }
    }
}
