using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace SK_collect
{
    public static class Toils_Collect
    {
        public static Toil MakeAndSpawnThing(ThingDef def, int amount)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
            {
                int num = amount;
                List<Thing> things = new List<Thing>();
                while (num > 0)
                {
                    Thing thing = ThingMaker.MakeThing(def);
                    int num2 = UnityEngine.Mathf.Min(num, def.stackLimit);
                    thing.stackCount = num2;
                    num -= num2;
                    things.Add(thing);
                }
                IntVec3 pos = toil.GetActor().jobs.curJob.targetA.Cell;
                foreach (var thing in things)
                {
                    GenSpawn.Spawn(thing, pos);
                }
            };
            return toil;
        }

        public static Toil MakeAndSpawnThingRandomRange(ThingDef def, int min, int max)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
            {
                int num = Rand.Range(min, max);
                List<Thing> things = new List<Thing>();
                while (num > 0)
                {
                    Thing thing = ThingMaker.MakeThing(def);
                    int num2 = UnityEngine.Mathf.Min(num, def.stackLimit);
                    thing.stackCount = num2;
                    num -= num2;
                    things.Add(thing);
                }
                IntVec3 pos = toil.GetActor().jobs.curJob.targetA.Cell;
                foreach (var thing in things)
                {
                    GenSpawn.Spawn(thing, pos);
                }
            };
            return toil;
        }

        public static Toil RemoveDesignationAtPosition(IntVec3 pos, DesignationDef dDef)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Designation des = Find.DesignationManager.DesignationAt(pos, dDef);
                if (des != null)
                {
                    Find.DesignationManager.RemoveDesignation(des);
                }
            };
            return toil;
        }
    }
}