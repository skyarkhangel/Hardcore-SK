using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace QOLTweaksPack.tweaks
{
    [HarmonyPatch(typeof(TendUtility), "DoTend")]
    static class TendUtility_DoTend_PrePostfix
    {
        [HarmonyPrefix]
        private static void DoTendPrefix(Pawn doctor, Pawn patient, ref Medicine medicine, Medicine __state)
        {
            if (QOLTweaksPack.BruiseWalkingOff.Value == false)
                return;

            //Log.Message("tending prefix");
            if (medicine == null || doctor == null || patient == null || doctor.Faction.IsPlayer == false)
                return;



            List<Hediff> tmpHediffsToTend = new List<Hediff>();

            TendUtility.GetOptimalHediffsToTendWithSingleTreatment(patient, true, tmpHediffsToTend, null);

            foreach(Hediff tendable in tmpHediffsToTend)
            {
                //Log.Message("tending " + tendable.GetType().ToString());
                if (!(tendable is Hediff_Injury))
                    return;
                Hediff_Injury injury = (tendable as Hediff_Injury);
                if (injury.Bleeding)
                {
                    //Log.Message("is bleeding");
                    return;
                }
                if (injury.TryGetComp<HediffComp_Infecter>() != null)
                {
                    //Log.Message("has infecter");
                    return;
                }
                if (injury.TryGetComp<HediffComp_GetsOld>() != null)
                {
                    //Log.Message("has scarring");
                    return;
                }
                //Log.Message("match");
            }

            __state = medicine;
            medicine = null;
        }

        [HarmonyPostfix]
        private static void DoTendPostfix(Pawn doctor, Pawn patient, ref Medicine medicine, Medicine __state)
        {
            if (QOLTweaksPack.BruiseWalkingOff.Value == false)
                return;

            Thing eh;
            if (__state != null)
                doctor.carryTracker.TryDropCarriedThing(doctor.Position, ThingPlaceMode.Direct, out eh);
        }
    }
}
