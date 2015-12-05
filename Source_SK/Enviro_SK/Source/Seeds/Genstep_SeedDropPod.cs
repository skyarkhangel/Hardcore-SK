using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using RimWorld;
using Verse;

namespace SK_Enviro.Seeds
{
    class Genstep_SeedDropPod : Genstep
    {
        private const int m_iNumPotatoSeedsToDrop = 20;

        public override void Generate()
        {
            // unfortunately I can't just combine this with the initial colonist drop, so I need to do it with a separate drop pod if I don't want it
            // to just appear on the ground before the colonists even arrive

            List<Thing> listOfThingsToDrop = new List<Thing>();

            ThingDef potatoSeedDef = DefDatabase<ThingDef>.GetNamed("PotatoSeeds");

            if (potatoSeedDef != null)
            {
                Thing potatoSeedThing = ThingMaker.MakeThing(potatoSeedDef);

                Thing_PlantSeedsItem seed = potatoSeedThing as Thing_PlantSeedsItem;
                //seed.genome.canGrowAtNight = false;
                //seed.genome.canSurviveFrost = false;
                //seed.genome.baseSeedChance = 1;
                //seed.genome.addSeedChance = (float)(Rand.Value * 0.1 + 0.1);
                //seed.genome.growSpeedMult = (float)(Rand.Value * 0.25 + 1.25);
                //seed.genome.harvestAmountMult = (float)(Rand.Value * 0.2 + 0.7);

                seed.stackCount = m_iNumPotatoSeedsToDrop;

                listOfThingsToDrop.Add(seed);
            }

            /* FCTODO: This is problematic due to the spawn order.  Figure it out later.
            
            // throw in a spacer corpse as a thematic element
			Faction faction = Find.FactionManager.FirstFactionOfDef( FactionDefOf.Spacer );

			Pawn crashVictim = PawnGenerator.GeneratePawn( PawnKindDef.Named( "Colonist" ), faction );
            
            // can't kill the victim outright before they're generated on the map, so give them major injuries and set them up to bleed out immediately.

			HealthUtility.GiveInjuriesToForceDowned( crashVictim );

            crashVictim.Health = 1;               

            listOfThingsToDrop.Add( crashVictim );
            */

            if (listOfThingsToDrop.Count > 0)
            {
                DropPodUtility.DropThingsNear(MapGenerator.PlayerStartSpot, listOfThingsToDrop, 110, MapInitData.StartedDirectInEditor, true);
            }
        }
    }
}
