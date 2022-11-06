using System;
using System.Collections.Generic;
using System.ComponentModel;
using RocketMan;
using Soyuz.Core;
using UnityEngine.Assertions;
using Verse;

namespace Soyuz
{
    public static class HediffHelper
    {
        private static Dictionary<int, HediffTracker> trackers = new Dictionary<int, HediffTracker>();

        public static bool TryGetHediffTracker(this Pawn pawn, out HediffTracker tracker)
        {
            tracker = null;
            if (RocketPrefs.TimeDilationCriticalHediffs)
            {
                return false;
            }
            if (trackers.TryGetValue(pawn.thingIDNumber, out tracker))
            {
                return true;
            }
            return false;
        }

        private static Pawn _pawn;
        private static HediffTracker _tracker;

        public static HediffTracker GetHediffTracker(this Pawn pawn)
        {
            if (RocketPrefs.TimeDilationCriticalHediffs)
            {
                return null;
            }
            if (pawn == _pawn)
            {
                return _tracker;
            }
            _pawn = pawn;
            if (trackers.TryGetValue(pawn.thingIDNumber, out var tracker))
            {
                return _tracker = tracker;
            }
            return _tracker = trackers[pawn.thingIDNumber] = new HediffTracker(pawn);
        }

        public static bool HasCriticalHediff(this Pawn pawn)
        {
            //
            //return pawn.IsPregnant();
            return false;
        }

        //private static Pawn _pregnantPawn;
        //private static bool _pregnant;
        //
        public static bool IsPregnant(this Pawn pawn)
        {
            //if (Finder.timeDilationCriticalHediffs)
            //{
            //    throw new InvalidOperationException("timeDilationCriticalHediffs is disabled!");
            //}
            //if (pawn.gender != Gender.Female)
            //{
            //    return _pregnant = false;
            //}
            //if (_pregnantPawn == pawn)
            //{
            //    return _pregnant;
            //}
            //_pregnantPawn = pawn;
            //return _pregnant = pawn.GetHediffTracker().Pregnant;
            return false;
        }
    }
}
