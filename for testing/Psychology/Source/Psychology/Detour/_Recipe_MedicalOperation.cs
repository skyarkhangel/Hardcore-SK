// Decompiled with JetBrains decompiler
// Type: RimWorld.Recipe_MedicalOperation
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using RimWorld;

namespace Psychology.Detour
{
    internal static class _Recipe_MedicalOperation
    {
        internal static bool _CheckSurgeryFail(this Recipe_MedicalOperation m, Pawn surgeon, Pawn patient, List<Thing> ingredients)
        {
            if (surgeon == null)
            {
                Log.Error("surgeon is null");
                return false;
            }
            if (patient == null)
            {
                Log.Error("patient is null");
                return false;
            }
            float num1 = 1f;
            float f1 = surgeon.GetStatValue(StatDefOf.SurgerySuccessChance, true);
            if ((double)f1 < 1.0)
                f1 = Mathf.Pow(f1, m.recipe.surgeonSurgerySuccessChanceExponent);
            float num2 = num1 * f1;
            Room room = surgeon.GetRoom();
            if (room != null)
            {
                float f2 = room.GetStat(RoomStatDefOf.SurgerySuccessChanceFactor);
                if ((double)f2 < 1.0)
                    f2 = Mathf.Pow(f2, m.recipe.roomSurgerySuccessChanceFactorExponent);
                num2 *= f2;
            }
            float potency = 0.0f;
            var GetAverageMedicalPotency = typeof(Recipe_MedicalOperation).GetMethod("GetAverageMedicalPotency", BindingFlags.Instance | BindingFlags.NonPublic);
            if (GetAverageMedicalPotency != null)
                potency = (float)GetAverageMedicalPotency.Invoke(m, new object[] { ingredients });
            else
                Log.ErrorOnce("Unable to reflect Recipe_MedicalOperation.GetAverageMedicalPotency!", 305432421);
            if ((double)Rand.Value <= (double)(num2 * potency * m.recipe.surgerySuccessChanceFactor))
                return false;
            if ((double)Rand.Value < (double)m.recipe.deathOnFailedSurgeryChance)
            {
                int num3 = 0;
                while (!patient.Dead)
                {
                    HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient);
                    ++num3;
                    if (num3 > 300)
                    {
                        Log.Error("Could not kill patient.");
                        break;
                    }
                }
            }
            else if ((double)Rand.Value < 0.5)
            {
                Messages.Message("MessageMedicalOperationFailureCatastrophic".Translate((object)surgeon.LabelShort, (object)patient.LabelShort), (TargetInfo)((Thing)patient), MessageSound.SeriousAlert);
                HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient);
            }
            else
            {
                Messages.Message("MessageMedicalOperationFailureMinor".Translate((object)surgeon.LabelShort, (object)patient.LabelShort), (TargetInfo)((Thing)patient), MessageSound.Negative);
                HealthUtility.GiveInjuriesOperationFailureMinor(patient);
            }
            if (!patient.Dead)
            {
                var TryGainBotchedSurgeryThought = typeof(Recipe_MedicalOperation).GetMethod("TryGainBotchedSurgeryThought", BindingFlags.Instance | BindingFlags.NonPublic);
                if (TryGainBotchedSurgeryThought != null)
                    TryGainBotchedSurgeryThought.Invoke(m, new object[] { patient, surgeon });
                else
                    Log.ErrorOnce("Unable to reflect Recipe_MedicalOperation.TryGainBotchedSurgeryThought!", 305432421);
            }
            else
                surgeon.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.KilledPatientBleedingHeart, patient);
            return true;
        }
    }
}
