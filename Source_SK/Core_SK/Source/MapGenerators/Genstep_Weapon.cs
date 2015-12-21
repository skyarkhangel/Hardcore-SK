using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Core_SK
{
    class Genstep_Weapon : Genstep
    {
        public override void Generate()
        {
            List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef PumpShotgunDef = DefDatabase<ThingDef>.GetNamed("Gun_PumpShotgun");

            Thing PumpShotgun = ThingMaker.MakeThing(PumpShotgunDef, GenStuff.DefaultStuffFor(PumpShotgunDef));

            listOfThingsToDrop.Add(PumpShotgun);


            DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true);
        }
    }
}
