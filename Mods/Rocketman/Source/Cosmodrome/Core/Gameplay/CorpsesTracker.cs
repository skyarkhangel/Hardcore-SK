using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace RocketMan.Gameplay
{
    public class CorpsesTracker : MapComponent
    {
        private const int Interval = 250;
        private const int ScanInterval = 9311;

        private HashSet<Thing> corpses = new HashSet<Thing>();
        private int curIndex;
        private List<CorpseRecord> destroyList = new List<CorpseRecord>();

        private List<CorpseRecord> records = new List<CorpseRecord>();
        private readonly List<CorpseRecord> removalList = new List<CorpseRecord>();
        public int removedThingsCount;

        private readonly Stopwatch stopwatch = new Stopwatch();

        private int tick;
        private CellRect viewRect;

        public CorpsesTracker(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            if (!RocketPrefs.Enabled) return;
            if (!RocketPrefs.CorpsesRemovalEnabled) return;
            removalList.Clear();
            int counter = 0;
            if (tick % ScanInterval == 0) FindCorpses();
            if (tick++ % Interval != 0) return;
            stopwatch.Reset();
            stopwatch.Start();
            viewRect = Find.CameraDriver.CurrentViewRect;
            while (counter < records.Count && stopwatch.ElapsedMilliseconds <= 1)
                try
                {
                    var record = records[curIndex];
                    if (record.thing == null || record.thing.Destroyed || !record.thing.Spawned)
                        removalList.Add(record);
                    else ProcessCorpse(record);
                }
                catch (Exception er)
                {
                    Log.Error($"ROCKETMAN: Error in GC {er}");
                }
                finally
                {
                    curIndex = (curIndex + 1) % records.Count;
                    counter++;
                }

            stopwatch.Stop();
            foreach (var record in removalList)
            {
                records.Remove(record);
                if (record.thing is Corpse corpse) corpses.Remove(corpse);
            }
            removalList.Clear();
            stopwatch.Restart();
            while (destroyList.Count > 0 && stopwatch.ElapsedMilliseconds <= 10)
            {
                try
                {
                    var record = destroyList.Pop();
                    if (RocketDebugPrefs.Debug) Log.Message($"ROCKETMAN: removed thing {record.thing} with total removed {removedThingsCount + 1}");
                    if (!(record.thing?.Destroyed ?? true)) record.thing?.Destroy();
                }
                catch (Exception er)
                {
                    if (RocketDebugPrefs.Debug) Log.Error($"ROCKETMAN: Error in GC while destroying thing {er}");
                }
                finally
                {
                    removedThingsCount++;
                }
            }
            stopwatch.Stop();
            removalList.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            records.RemoveAll(t => t.thing == null || t.thing.Destroyed);
            corpses.RemoveWhere(t => t == null || t.Destroyed);
            destroyList.RemoveAll(t => t.thing == null || t.thing.Destroyed);

            Scribe_Collections.Look(ref records, "garbage", LookMode.Deep);
            Scribe_Collections.Look(ref destroyList, "destroyList", LookMode.Deep);
            Scribe_Collections.Look(ref corpses, "corpses", LookMode.Reference);

            Scribe_Values.Look(ref removedThingsCount, "removedThingsCount");
            Scribe_Values.Look(ref curIndex, "curIndex");
            Scribe_Values.Look(ref tick, "tick");

            if (corpses == null) corpses = new HashSet<Thing>();
            if (records == null) records = new List<CorpseRecord>();
            if (destroyList == null) destroyList = new List<CorpseRecord>();
        }

        private void FindCorpses()
        {
            var corpsesTemp = map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
            foreach (var thing in corpsesTemp)
            {
                if (!corpses.Contains(thing) && thing is Corpse corpse && corpse.CurRotDrawMode == RotDrawMode.Dessicated)
                {
                    var record = new CorpseRecord(corpse);
                    corpses.Add(corpse);
                    records.Add(record);
                }
            }
        }

        private void ProcessCorpse(CorpseRecord record)
        {
            var position = record.thing.positionInt;

            if (viewRect.Contains(position))
                record.RegisterVisibility(true);
            else
                record.RegisterVisibility(false);

            if (record.Age.TicksToDays() >= (RocketDebugPrefs.Debug ? 0.5f : record.thing?.factionInt != null ? 14.0f : 7f) &&
                record.ViewedRatio < 0.25f && Rand.Chance(0.25f))
            {
                if (!ShouldDelete(record.thing)) return;
                removalList.Add(record);
                destroyList.Add(record);
            }
        }

        private bool ShouldDelete(Thing thing)
        {
            if (thing.factionInt == Faction.OfPlayer || !RocketPrefs.CorpsesRemovalEnabled)
                return false;
            return true;
        }

        private class CorpseRecord : IExposable
        {
            private int inFrameCounter;
            private int outOfFrameCounter;
            public Thing thing;

            public int Age
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => inFrameCounter + outOfFrameCounter;
            }

            public float ViewedRatio
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => inFrameCounter / (inFrameCounter + outOfFrameCounter + 1e-5f);
            }

            public CorpseRecord()
            {
            }

            public CorpseRecord(Thing thing)
            {
                this.thing = thing;
                inFrameCounter = 0;
                outOfFrameCounter = 0;
            }

            public void ExposeData()
            {
                Scribe_References.Look(ref thing, "thing");
                Scribe_Values.Look(ref inFrameCounter, "inFrameCounter");
                Scribe_Values.Look(ref outOfFrameCounter, "outOfFrameCounter");
            }

            public void RegisterVisibility(bool visible)
            {
                if (visible) inFrameCounter += Interval;
                else outOfFrameCounter += Interval;
            }
        }
    }
}