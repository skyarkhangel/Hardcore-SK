// Decompiled with JetBrains decompiler
// Type: RimWorld.TendUtility
// Assembly: Assembly-CSharp, Version=0.14.6054.28275, Culture=neutral, PublicKeyToken=null
// MVID: 1AEB3542-500E-442F-87BE-1A3452AE432F
// Assembly location: D:\Steam\steamapps\common\RimWorld\RimWorldWin_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace Psychology.Detour
{
    internal static class _TendUtility
    {
        private static List<Hediff_MissingPart> bleedingStumps = new List<Hediff_MissingPart>();
        private static List<Hediff> otherHediffs = new List<Hediff>();

        internal static void _DoTend(Pawn doctor, Pawn patient, Medicine medicine)
        {
            if (!patient.health.HasHediffsNeedingTend(false))
                return;
            if (medicine != null && medicine.Destroyed)
            {
                Log.Warning("Tried to use destroyed medicine.");
                medicine = (Medicine)null;
            }
            float num1 = medicine == null ? 0.0f : medicine.def.GetStatValueAbstract(StatDefOf.MedicalPotency, (ThingDef)null);
            float quality = medicine == null ? 0.5f : num1;
            if (doctor != null)
                quality *= doctor.GetStatValue(StatDefOf.BaseHealingQuality, true);
            if (patient.InBed())
                quality *= patient.CurrentBed().GetStatValue(StatDefOf.MedicalTreatmentQualityFactor, true);
            if (patient.health.hediffSet.GetInjuriesTendable().Any<Hediff_Injury>())
            {
                float num2 = 0.0f;
                int batchPosition = 0;
                foreach (Hediff_Injury hediffInjury in (IEnumerable<Hediff_Injury>)patient.health.hediffSet.GetInjuriesTendable().OrderByDescending<Hediff_Injury, float>((Func<Hediff_Injury, float>)(x => x.Severity)))
                {
                    float num3 = Mathf.Min(hediffInjury.Severity, 20f);
                    if ((double)num2 + (double)num3 <= 20.0)
                    {
                        num2 += num3;
                        hediffInjury.Tended(quality, batchPosition);
                        if (medicine != null)
                            ++batchPosition;
                        else
                            break;
                    }
                    else
                        break;
                }
            }
            else
            {
                bleedingStumps.Clear();
                List<Hediff_MissingPart> partsCommonAncestors = patient.health.hediffSet.GetMissingPartsCommonAncestors();
                for (int index = 0; index < partsCommonAncestors.Count; ++index)
                {
                    if (partsCommonAncestors[index].IsFresh)
                        bleedingStumps.Add(partsCommonAncestors[index]);
                }
                if (bleedingStumps.Count > 0)
                {
                    bleedingStumps.RandomElement<Hediff_MissingPart>().IsFresh = false;
                    bleedingStumps.Clear();
                }
                else
                {
                    otherHediffs.Clear();
                    otherHediffs.AddRange(patient.health.hediffSet.GetTendableNonInjuryNonMissingPartHediffs());
                    Hediff result;
                    if (otherHediffs.TryRandomElement<Hediff>(out result))
                    {
                        HediffCompProperties hediffCompProperties = result.def.CompPropsFor(typeof(HediffComp_Tendable));
                        if (hediffCompProperties != null && hediffCompProperties.tendAllAtOnce)
                        {
                            int batchPosition = 0;
                            for (int index = 0; index < otherHediffs.Count; ++index)
                            {
                                if (otherHediffs[index].def == result.def)
                                {
                                    otherHediffs[index].Tended(quality, batchPosition);
                                    ++batchPosition;
                                }
                            }
                        }
                        else
                            result.Tended(quality, 0);
                    }
                    otherHediffs.Clear();
                }
            }
            if (doctor != null && patient.HostFaction == null && (patient.Faction != null && patient.Faction != doctor.Faction))
                patient.Faction.AffectGoodwillWith(doctor.Faction, 0.3f);
            if (doctor != null && doctor.RaceProps.Humanlike && (patient.RaceProps.Animal && RelationsUtility.TryDevelopBondRelation(doctor, patient, 0.01f)) && (doctor.Faction != null && doctor.Faction != patient.Faction))
                InteractionWorker_RecruitAttempt.DoRecruit(doctor, patient, 1f, false);
            patient.records.Increment(RecordDefOf.TimesTendedTo);
            if (doctor != null)
            {
                doctor.records.Increment(RecordDefOf.TimesTendedOther);
                doctor.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DoctorBleedingHeart, patient);
            }
            if (medicine == null)
                return;
            if ((patient.Spawned || doctor != null && doctor.Spawned) && (double)num1 > 0.899999976158142)
                SoundDef.Named("TechMedicineUsed").PlayOneShot((SoundInfo)patient.Position);
            if (medicine.stackCount > 1)
            {
                --medicine.stackCount;
            }
            else
            {
                if (medicine.Destroyed)
                    return;
                medicine.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
