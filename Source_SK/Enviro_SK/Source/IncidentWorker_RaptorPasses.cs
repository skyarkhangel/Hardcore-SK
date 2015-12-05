using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;         // Always needed
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;            // Needed when you do something with the AI
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace SK_Enviro
{
           internal class IncidentWorker_RaptorPasses : IncidentWorker
        {
            public override bool TryExecute(IncidentParms parms)
            {
                PawnKindDef kindDef = PawnKindDef.Named("FRaptor");
                IntVec3 intVec;
                if (!RCellFinder.TryFindRandomPawnEntryCell(out intVec))
                {
                    return false;
                }
                int num = Rand.RangeInclusive(2, 4);
                for (int i = 0; i < num; i++)
                {
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(intVec, 10);
                    Pawn newThing = PawnGenerator.GeneratePawn(kindDef, null, false, 0);
                    GenSpawn.Spawn(newThing, loc);
                }
                Find.LetterStack.ReceiveLetter("LetterLabelRaptorArrived".Translate(), "RaptorArrived".Translate(), LetterType.BadNonUrgent, intVec, null);
                return true;
            }
        }


    
}
