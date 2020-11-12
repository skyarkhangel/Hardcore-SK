using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommonSense
{
    class CravingForOutdoors
    {
        //RimWorld.JobGiver_GetJoy.TryGiveJob
        //protected override Job TryGiveJob(Pawn pawn)
        [HarmonyPatch(typeof(RimWorld.JobGiver_GetJoy), "TryGiveJob", new Type[] { typeof(Pawn) })]
        static class JobGiver_GetJoy_TryGiveJob_CommonSensePatch
        {
            public class JobCrutch: JobGiver_GetJoy
            {
                public bool CanDoDuringMedicalRestCrutch()
                {
                    return CanDoDuringMedicalRest;
                }
                public bool JoyGiverAllowedCrutch(JoyGiverDef def)
                {
                    return JoyGiverAllowed(def);
                }
                public Job TryGiveJobFromJoyGiverDefDirectCrutch(JoyGiverDef def, Pawn pawn)
                {
                    return TryGiveJobFromJoyGiverDefDirect(def, pawn);
                }
            }

            //double pass on trying to give a joyjob. At first, we'll try to give a job, that located outside;
            static bool Prefix(ref Job __result, ref JobCrutch __instance,  ref Pawn pawn)
            {
                if (!Settings.fulfill_outdoors || !__instance.CanDoDuringMedicalRestCrutch() && pawn.InBed() && HealthAIUtility.ShouldSeekMedicalRest(pawn)
                    || pawn.needs.outdoors == null || pawn.needs.outdoors.CurLevel >= 0.4f)
                {
                    return true;
                }

                List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading;
                JoyToleranceSet tolerances = pawn.needs.joy.tolerances;
                DefMap<JoyGiverDef, float> joyGiverChances = new DefMap<JoyGiverDef, float>();
                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    JoyGiverDef joyGiverDef = allDefsListForReading[i];
                    joyGiverChances[joyGiverDef] = 0f;
                    if (__instance.JoyGiverAllowedCrutch(joyGiverDef) && !pawn.needs.joy.tolerances.BoredOf(joyGiverDef.joyKind) && joyGiverDef.Worker.MissingRequiredCapacity(pawn) == null)
                    {
                        if (joyGiverDef.pctPawnsEverDo < 1f)
                        {
                            Rand.PushState(pawn.thingIDNumber ^ 0x3C49C49);
                            if (Rand.Value >= joyGiverDef.pctPawnsEverDo)
                            {
                                Rand.PopState();
                                continue;
                            }
                            Rand.PopState();
                        }
                        float num = tolerances[joyGiverDef.joyKind];
                        float b = Mathf.Pow(1f - num, 5f);
                        b = Mathf.Max(0.001f, b);
                        joyGiverChances[joyGiverDef] = joyGiverDef.Worker.GetChance(pawn) * b;
                    }
                }
                for (int j = 0; j < joyGiverChances.Count; j++)
                {
                    if (!allDefsListForReading.TryRandomElementByWeight((JoyGiverDef d) => joyGiverChances[d], out JoyGiverDef result))
                    {
                        break;
                    }
                    Job job = __instance.TryGiveJobFromJoyGiverDefDirectCrutch(result, pawn);
                    if (job != null && job.targetA != null)
                            if (job.targetA.Thing != null)
                            {
                            if (job.targetA.Thing.GetRoom() != null && job.targetA.Thing.GetRoom().PsychologicallyOutdoors)
                                {
                                    __result = job;
                                    return false;
                                }
                            } else if (job.targetA.Cell != null)
                            {
                                IntVec3 vec3 = (IntVec3)job.targetA;
                                if (job.targetA.Cell.GetRoom(pawn.Map).PsychologicallyOutdoors)
                                {
                                    __result = job;
                                    return false;
                                }
                            }
                    joyGiverChances[result] = 0f;
                }
                return true;
            }
        }
    }
}
