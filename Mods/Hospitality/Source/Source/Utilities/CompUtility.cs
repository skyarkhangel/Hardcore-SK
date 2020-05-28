using System.Collections.Generic;
using Verse;

namespace Hospitality
{
    internal static class CompUtility
    {
        private static Dictionary<Pawn, CompGuest> guestComps = new Dictionary<Pawn, CompGuest>();

        public static CompGuest CompGuest(this Pawn pawn)
        {
            if (pawn == null) return null;
            //Log.Error($"Tried to get CompGuest for {pawn.ToStringSafe()}.\n{StackTraceUtility.ExtractStackTrace()}");
            if (guestComps.TryGetValue(pawn, out var comp)) return comp;

            comp = pawn.GetComp<CompGuest>();
            guestComps.Add(pawn, comp);
            return comp;
        }

        public static void OnPawnRemoved(Pawn pawn)
        {
            guestComps.Remove(pawn);
        }
    }
}
