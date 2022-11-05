using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using RimWorld;
using RimWorld.Planet;
using RocketMan;
using UnityEngine;
using Verse;

namespace Soyuz
{
    public class WorldPawnsTicker : GameComponent
    {
        public const int BucketCount = 30;

        private static HashSet<Pawn> pawns = new HashSet<Pawn>();

        private static HashSet<Pawn> caravaningPawns = new HashSet<Pawn>();

        private static HashSet<Pawn> previousBucket = new HashSet<Pawn>();

        private static HashSet<Pawn>[] buckets;

        private static Game game;

        //private static bool dirty = false;

        public static int curIndex = 0;

        public static int curCycle = 0;

        public static bool isActive = false;

        public static HashSet<Pawn> PreviousBucket
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => previousBucket;
        }

        public WorldPawnsTicker(Game game)
        {
            ResetInternalState();
            TryInitialize();
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            curIndex++;
            if (curIndex >= BucketCount)
            {
                curCycle++;
                curIndex = 0;
            }
            //if (RocketPrefs.WarmingUp)
            //{
            //    if (!dirty)
            //    {
            //        ResetInternalState();
            //        dirty = true;
            //    }
            //    return;
            //}
            //if (Find.World != null)
            //{
            //    if (dirty)
            //    {
            //        dirty = false;
            //        ResetInternalState();
            //        Rebuild(Find.WorldPawns);
            //    }
            //    else if (GenTicks.TicksGame % 5000 == 0)
            //    {
            //        ResetInternalState();
            //        Rebuild(Find.WorldPawns);
            //    }
            //}
        }

        public override void StartedNewGame()
        {
            ResetInternalState();
            base.StartedNewGame();
            TryInitialize();
        }

        public override void LoadedGame()
        {
            ResetInternalState();
            base.LoadedGame();
            TryInitialize();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref curIndex, "curIndex", 0);
            if (curIndex >= BucketCount)
            {
                curIndex = 0;
                curCycle = 0;
            }
        }

        public static void TryInitialize()
        {
            if (game != Current.Game || buckets == null)
            {
                ResetInternalState();
                curIndex = curCycle = 0;
                game = Current.Game;
                buckets ??= new HashSet<Pawn>[BucketCount];
                for (int i = 0; i < BucketCount; i++)
                {
                    buckets[i] ??= new HashSet<Pawn>();
                    buckets[i].Clear();
                }
            }
        }

        public static void Rebuild(WorldPawns instance)
        {
            ResetInternalState();
            curCycle = 0;
            curIndex = 0;
            for (int i = 0; i < BucketCount; i++)
            {
                buckets[i].Clear();
            }
            foreach (Pawn pawn in instance.pawnsAlive)
            {
                if (!pawn.Dead && !pawn.Destroyed)
                    Register(pawn);
            }
        }

        public static void ResetInternalState()
        {
            caravaningPawns.Clear();
            pawns.Clear();
            previousBucket.Clear();
        }

        public static void Register(Pawn pawn)
        {
            int index = GetBucket(pawn);            
            if (buckets == null)
                TryInitialize();
            if (buckets[index] == null)
                buckets[index] = new HashSet<Pawn>();
            else if (buckets[index].Contains(pawn) && (pawn.Dead || pawn.Destroyed))
            {
                Deregister(pawn);
                return;
            }
            if (pawn.IsCaravanMember())
            {
                caravaningPawns.Add(pawn);
            }
            pawns.Add(pawn);
            buckets[index].Add(pawn);
        }

        //public static void SetDirty() => dirty = true;

        public static void Deregister(Pawn pawn)
        {
            int index = GetBucket(pawn);
            if (buckets[index] == null) return;
            pawns.RemoveWhere(p => p.thingIDNumber == pawn.thingIDNumber);
            caravaningPawns.RemoveWhere(p => p.thingIDNumber == pawn.thingIDNumber);
            buckets[index].RemoveWhere(p => p.thingIDNumber == pawn.thingIDNumber);
        }

        public static HashSet<Pawn> GetPawns(bool fallbackMode = false)
        {
            if (buckets == null)
            {
                Log.Warning("SOYUZ: GetPawns called before initialization");
                TryInitialize();

                return Find.WorldPawns.pawnsAlive;
            }
            HashSet<Pawn> bucket = buckets[curIndex];
            curIndex = GenTicks.TicksGame % 30;
            if (curIndex == 0)
                curCycle++;
            IEnumerable<Pawn> result = RocketPrefs.TimeDilationCaravans ? bucket : AddExtraPawns(bucket);
            if (!fallbackMode)
            {
                ValidateCollection(result, out IEnumerable<Pawn> validated);
                return previousBucket = validated.ToHashSet();
            }
            return previousBucket = result.ToHashSet();
        }

        public static bool IsCustomWorldTickInterval(Thing thing, int interval)
        {
            return interval <= BucketCount ? true : curCycle % ((int)(interval / BucketCount)) == 0;
        }

        private static void ValidateCollection(IEnumerable<Pawn> pawns, out IEnumerable<Pawn> result)
        {
            List<Pawn> invalidPawns = new List<Pawn>();
            result = null;
            foreach (Pawn pawn in pawns)
            {
                if (pawn.Destroyed || pawn.Spawned || pawn.Dead)
                    invalidPawns.Add(pawn);
            }
            if (invalidPawns.Count == 0)
            {
                result = pawns;
                return;
            }
            foreach (Pawn pawn in invalidPawns)
            {
                Deregister(pawn);
            }
            result = GetPawns(fallbackMode: true);
        }


        private static IEnumerable<Pawn> AddExtraPawns(IEnumerable<Pawn> bucket)
        {
            List<Pawn> temp = new List<Pawn>();
            foreach (Pawn pawn in caravaningPawns)
            {
                if (pawn.Destroyed)
                {
                    ResetInternalState();
                    Rebuild(Find.WorldPawns);
                    throw new Exception("ROCKETMAN: Tried to tick a destroyed pawn!");
                }
                if (!pawn.IsCaravanMember() || pawn.Spawned || pawn.Dead)
                {
                    temp.Add(pawn);
                    continue;
                }
                yield return pawn;
            }
            if (temp.Count > 0)
            {
                caravaningPawns.RemoveWhere(p => temp.Contains(p));
                temp.Clear();
            }
            foreach (Pawn pawn in bucket)
            {
                yield return pawn;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBucket(Pawn pawn)
        {
            int hash;
            unchecked
            {                
                hash = pawn.thingIDNumber;
                if (hash < 0) hash *= -1;
            }
            return hash % BucketCount;
        }
    }
}