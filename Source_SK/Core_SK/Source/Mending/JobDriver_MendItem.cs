using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;

namespace SK_Mending
{
    public class JobDriver_MendItem : JobDriver
    {
        public const TargetIndex menderTableTI = TargetIndex.A;
        public const TargetIndex mendingObjectTI = TargetIndex.B;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Thing toMend = this.TargetThingB;

            ToilFailConditions.FailOnDestroyedOrForbidden<JobDriver_MendItem>(this, menderTableTI);
            ToilFailConditions.FailOnBurningImmobile<JobDriver_MendItem>(this, menderTableTI);
            ToilFailConditions.FailOnDestroyedOrForbidden<JobDriver_MendItem>(this, mendingObjectTI);
            ToilFailConditions.FailOnBurningImmobile<JobDriver_MendItem>(this, mendingObjectTI);

            yield return Toils_Reserve.Reserve(menderTableTI, 1);
            yield return Toils_Reserve.Reserve(mendingObjectTI, 1);
            yield return Toils_Goto.GotoCell(mendingObjectTI, PathEndMode.OnCell);
            yield return Toils_Haul.StartCarryThing(mendingObjectTI);
            yield return Toils_Goto.GotoThing(menderTableTI, PathEndMode.InteractionCell);
            yield return Toils_Haul.PlaceHauledThingInCell(menderTableTI, null, false);

            int num = toMend.MaxHitPoints - toMend.HitPoints;
            for (int i = 0; i < num; i++)
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    if (toMend.HitPoints < toMend.MaxHitPoints)
                    {
                        toMend.HitPoints += 1;
                    }
                };
                toil.defaultCompleteMode = ToilCompleteMode.Instant;
                yield return toil;
                yield return Toils_General.Wait(100);
            }

            yield return Toils_Haul.StartCarryThing(mendingObjectTI);
            yield return Toils_Reserve.Release(menderTableTI);

            Toil toil1 = new Toil();
            toil1.initAction = delegate
            {
                this.pawn.carrier.TryDropCarriedThing(this.pawn.Position, ThingPlaceMode.Near, out toMend);
            };
            toil1.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil1;

            yield return Toils_Reserve.Release(mendingObjectTI);
            yield break;
        }
    }
}
