using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using RimWorld;
using Verse;

namespace Core_SK
{
    class Genstep_Ammo : Genstep
    {
        private const int m_iNumHeavyMachinegunBulletToDrop = 60;

		public override void Generate()
		{
            // unfortunately I can't just combine this with the initial colonist drop, so I need to do it with a separate drop pod if I don't want it
            // to just appear on the ground before the colonists even arrive

			List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef hmgbDef = DefDatabase<ThingDef>.GetNamed("HeavyMachinegunBullet");

            if (hmgbDef != null)
            {
                Thing hmgb = ThingMaker.MakeThing(hmgbDef, GenStuff.DefaultStuffFor(hmgbDef));

                hmgb.stackCount = m_iNumHeavyMachinegunBulletToDrop;

                listOfThingsToDrop.Add(hmgb);
            }
            
            if ( listOfThingsToDrop.Count > 0 )
            {
                DropPodUtility.DropThingsNear( MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true );
            }
        }
    }
}
