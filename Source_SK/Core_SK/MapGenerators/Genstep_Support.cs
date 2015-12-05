using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using RimWorld;
using Verse;

namespace Core_SK
{
    class Genstep_SpareParts : Genstep
    {
        private const int m_iNumSparePartsToDrop = 40;

		public override void Generate()
		{
			List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef SparePartsDef = DefDatabase<ThingDef>.GetNamed("Spare_Parts");

            if (SparePartsDef != null)
            {
                Thing SpareParts = ThingMaker.MakeThing(SparePartsDef, GenStuff.DefaultStuffFor(SparePartsDef));

                SpareParts.stackCount = m_iNumSparePartsToDrop;

                listOfThingsToDrop.Add(SpareParts);
            }
            
            if ( listOfThingsToDrop.Count > 0 )
            {
                DropPodUtility.DropThingsNear( MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true );
            }
        }
    }
    class Genstep_PartsSteel : Genstep
    {
        private const int m_iNumPartsSteelToDrop = 60;

        public override void Generate()
        {
            List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef PartsSteelDef = DefDatabase<ThingDef>.GetNamed("Parts_Steel");

            if (PartsSteelDef != null)
            {
                Thing PartsSteel = ThingMaker.MakeThing(PartsSteelDef, GenStuff.DefaultStuffFor(PartsSteelDef));

                PartsSteel.stackCount = m_iNumPartsSteelToDrop;

                listOfThingsToDrop.Add(PartsSteel);
            }

            if (listOfThingsToDrop.Count > 0)
            {
                DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true);
            }
        }
    }
    class Genstep_Mechanism : Genstep
    {
        private const int m_iNumMechanismToDrop = 15;

        public override void Generate()
        {
            List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef MechanismDef = DefDatabase<ThingDef>.GetNamed("Mechanism");

            if (MechanismDef != null)
            {
                Thing Mechanism = ThingMaker.MakeThing(MechanismDef, GenStuff.DefaultStuffFor(MechanismDef));

                Mechanism.stackCount = m_iNumMechanismToDrop;

                listOfThingsToDrop.Add(Mechanism);
            }

            if (listOfThingsToDrop.Count > 0)
            {
                DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true);
            }
        }
    }
    class Genstep_EC : Genstep
    {
        private const int m_iNumECToDrop = 10;

        public override void Generate()
        {
            // unfortunately I can't just combine this with the initial colonist drop, so I need to do it with a separate drop pod if I don't want it
            // to just appear on the ground before the colonists even arrive

            List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef ECDef = DefDatabase<ThingDef>.GetNamed("ElectronicComponents");

            if (ECDef != null)
            {
                Thing EC = ThingMaker.MakeThing(ECDef, GenStuff.DefaultStuffFor(ECDef));

                EC.stackCount = m_iNumECToDrop;

                listOfThingsToDrop.Add(EC);
            }

            if (listOfThingsToDrop.Count > 0)
            {
                DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true);
            }
        }
    }
    class Genstep_Wire : Genstep
    {
        private const int m_iNumWireToDrop = 40;

        public override void Generate()
        {
            // unfortunately I can't just combine this with the initial colonist drop, so I need to do it with a separate drop pod if I don't want it
            // to just appear on the ground before the colonists even arrive

            List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef WireDef = DefDatabase<ThingDef>.GetNamed("Wire");

            if (WireDef != null)
            {
                Thing Wire = ThingMaker.MakeThing(WireDef, GenStuff.DefaultStuffFor(WireDef));

                Wire.stackCount = m_iNumWireToDrop;

                listOfThingsToDrop.Add(Wire);
            }

            if (listOfThingsToDrop.Count > 0)
            {
                DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true);
            }
        }
    }
}
