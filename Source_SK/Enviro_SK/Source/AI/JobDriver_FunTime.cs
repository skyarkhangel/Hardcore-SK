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

namespace SK_Enviro.AI
{
    public class JobDriver_FunTime : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            int num = Rand.RangeInclusive(3, 8);

            for (int i = 0; i < num; i++)
            {
                yield return this.CrazyTime();
            }
            
            yield break;
        }

        public Toil CrazyTime()
        {
            Toil toil = new Toil();
            IntVec3 target = CellFinder.RandomClosewalkCellNear(this.pawn.Position, 10);

            toil.initAction = delegate
            {
                this.CurJob.locomotionUrgency = LocomotionUrgency.Sprint;
                this.pawn.pather.StartPath(target, PathEndMode.OnCell);
            };

            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }
    }
}
