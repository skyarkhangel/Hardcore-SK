using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace aRandomKiwi.HFM
{
    internal static class Pawn_MindState_Patch
    {
        [HarmonyPatch(typeof(Pawn_MindState), "GetGizmos")]
        public class GetGizmos
        {
            [HarmonyPostfix]
            public static void Listener(Pawn_MindState __instance, ref IEnumerable<Gizmo> __result)
            {
                List<Gizmo> ret = new List<Gizmo>();
                Gizmo cur;

                //IF cat and cat not allowed to give gifts on return
                //OR if pawn downed
                if ( __instance == null || (Settings.cats.Contains(__instance.pawn.kindDef.defName) && !Settings.allowCatGift)
                    || __instance.pawn.Downed )
                {
                    return;
                }

                //Display of extra tools?
                if (__instance.pawn.RaceProps.Animal && Settings.showExtraTools)
                {
                    cur = new Command_Toggle
                    {
                        icon = Utils.texIgnoredPreys,
                        defaultLabel = "HuntingForMe_ExtraToolIgnoredPrey".Translate(),
                        defaultDesc = "",
                        isActive = delegate ()
                        {
                            if (Settings.ignoredPreys.Contains(__instance.pawn.def.defName))
                                return true;
                            else
                                return false;
                        },
                        toggleAction = delegate ()
                        {
                            if (Settings.ignoredPreys.Contains(__instance.pawn.def.defName))
                                Settings.ignoredPreys.Remove(__instance.pawn.def.defName);
                            else
                                Settings.ignoredPreys.Add(__instance.pawn.def.defName);
                        }
                    };
                    ret.Add(cur);

                    cur = new Command_Toggle
                    {
                        icon = Utils.texMeleeForced,
                        defaultLabel = "HuntingForMe_ExtraToolForcedMelee".Translate(),
                        defaultDesc = "",
                        isActive = delegate ()
                        {
                            if (Settings.ignoredRangedAttack.Contains(__instance.pawn.def.defName))
                                return true;
                            else
                                return false;
                        },
                        toggleAction = delegate ()
                        {
                            if (Settings.ignoredRangedAttack.Contains(__instance.pawn.def.defName))
                                Settings.ignoredRangedAttack.Remove(__instance.pawn.def.defName);
                            else
                                Settings.ignoredRangedAttack.Add(__instance.pawn.def.defName);
                        }
                    };
                    ret.Add(cur);

                    cur = new Command_Toggle
                    {
                        icon = Utils.texIsACat,
                        defaultLabel = "HuntingForMe_ExtraToolIsACat".Translate(),
                        defaultDesc = "",
                        isActive = delegate ()
                        {
                            if (Settings.cats.Contains(__instance.pawn.def.defName))
                                return true;
                            else
                                return false;
                        },
                        toggleAction = delegate ()
                        {
                            if (Settings.cats.Contains(__instance.pawn.def.defName))
                                Settings.cats.Remove(__instance.pawn.def.defName);
                            else
                                Settings.cats.Add(__instance.pawn.def.defName);
                        }
                    };
                    ret.Add(cur);

                }

                //IF animal with hunt abilities (or cat) AND authorized to hunt, add the button 
                if (__instance.pawn.RaceProps.Animal
                && __instance.pawn.Faction == Faction.OfPlayer
                && ( Settings.cats.Contains(__instance.pawn.kindDef.defName) || __instance.pawn.training.HasLearned(DefDatabase<TrainableDef>.GetNamed("HuntingTraining", true)))
                && __instance.pawn.TryGetComp<Comp_Hunting>().huntingState)
                {
                    int gt = Find.TickManager.TicksGame;
                    string label = "";
                    string desc = "";
                    string labelStopHunting = "HuntForMe_ForceStopHuntingLabel".Translate();
                    string descStopHunting = "";

                    if (Settings.cats.Contains(__instance.pawn.kindDef.defName))
                    {
                        label = "HuntingForMe_ForceKittyHuntingLabel".Translate();
                        desc = "HuntingForMe_ForceKittyHuntingDesc".Translate();
                    }
                    else
                    {
                        label = "HuntingForMe_ForceHuntingLabel".Translate();
                        desc = "HuntingForMe_ForceHuntingDesc".Translate();
                    }
                    Texture2D icon = null;
                    bool ok = false;

                    //If counter before next forcing reached
                    if (__instance.pawn.TryGetComp<Comp_Hunting>().huntingForceNextGT < gt)
                    {
                        if (Settings.cats.Contains(__instance.pawn.kindDef.defName))
                            icon = Utils.texForceKittyGift;
                        else
                            icon = Utils.texForceHunting;
                        ok = true;
                    }
                    else
                    {
                        if (Settings.cats.Contains(__instance.pawn.kindDef.defName))
                            icon = Utils.texForceKittyGiftDisabled;
                        else
                            icon = Utils.texForceHuntingDisabled;
                    }

                    //If still in the process of hunting, proposal to cancel the latter
                    if ( (__instance.pawn.CurJob != null) && (__instance.pawn.CurJob.def.defName == "HuntTrained"
                               || __instance.pawn.CurJob.def.defName == "HuntTrainedAssist"
                               || __instance.pawn.CurJob.def.defName == "HuntForMe_CatGift") )
                    {
                        //Add button to force stop the current hunt
                        cur = new Command_Action
                        {
                            icon = Utils.texForceStopHunting,
                            defaultLabel = labelStopHunting,
                            defaultDesc = descStopHunting,
                            action = delegate ()
                            {
                                //deadMemberBeforeStartHunting
                                if (__instance.pawn.jobs.curDriver is JobDriver_AnimalHunt)
                                {
                                    JobDriver_AnimalHunt jx = (JobDriver_AnimalHunt)__instance.pawn.jobs.curDriver;
                                    jx.manualStop = true;
                                }
                                else
                                {
                                    JobDriver_AnimalHuntAssist jx = (JobDriver_AnimalHuntAssist)__instance.pawn.jobs.curDriver;
                                    jx.manualStop = true;
                                }
                                __instance.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                                Messages.Message("HuntForMe_ForceStopHuntingMsg".Translate(__instance.pawn.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                                return;
                            }
                        };
                        ret.Add(cur);
                    }


                    //If a cat we add the cat gift button
                    if (Settings.cats.Contains(__instance.pawn.kindDef.defName))
                    {
                        cur = new Command_Action
                        {
                            icon = icon,
                            defaultLabel = label,
                            defaultDesc = desc,
                            action = delegate ()
                            {
                                //If danger on the map and parameter ok we do not launch the job
                                if (GenHostility.AnyHostileActiveThreatToPlayer(__instance.pawn.Map) && Settings.disallowHuntingWhenThreat)
                                {
                                    Messages.Message("HuntingForMe_ActiveThreatCannotHunt".Translate(), MessageTypeDefOf.NeutralEvent, false);
                                    return;
                                }

                                //If deadlines not reached before forcing manual flush
                                if (!ok)
                                {
                                    Messages.Message("HuntingForMe_ForceHuntingNeedWait".Translate(Utils.TranslateTicksToTextIRLSeconds((__instance.pawn.TryGetComp<Comp_Hunting>().huntingForceNextGT - gt)), __instance.pawn.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                                }
                                else
                                {
                                    //If still in the process of hunting we cancel
                                    if (__instance.pawn.CurJob.def.defName == "HuntTrained"
                                    || __instance.pawn.CurJob.def.defName == "HuntTrainedAssist"
                                    || __instance.pawn.CurJob.def.defName == "HuntForMe_CatGift")
                                    {
                                        Messages.Message("HuntForMe_AlreadyHunting".Translate(), MessageTypeDefOf.NeutralEvent, false);
                                        return;
                                    }

                                    JobGiver_AnimalHunt jgah = new JobGiver_AnimalHunt();
                                    //Indicator allowing to know that it is about a manual triggering of the hunt so we want a notification
                                    jgah.manualCall = true;
                                    JobIssueParams jb = new JobIssueParams();
                                    ThinkResult res = jgah.TryIssueJobPackage(__instance.pawn, jb);
                                    if (res.IsValid)
                                    {
                                        __instance.pawn.jobs.StartJob(res.Job);
                                        Messages.Message("HuntingForMe_ForceKittyHuntingOK".Translate(__instance.pawn.LabelCap), MessageTypeDefOf.PositiveEvent, false);
                                    }
                                }
                            }
                        };
                        ret.Add(cur);
                    }
                    else
                    {
                        cur = new Command_Action
                        {
                            icon = icon,
                            defaultLabel = label,
                            defaultDesc = desc,
                            action = delegate ()
                            {
                                //If danger on the map and parameter ok we do not launch the job
                                if (GenHostility.AnyHostileActiveThreatToPlayer(__instance.pawn.Map) && Settings.disallowHuntingWhenThreat)
                                {
                                    Messages.Message("HuntingForMe_ActiveThreatCannotHunt".Translate(), MessageTypeDefOf.NeutralEvent, false);
                                    return;
                                }

                                //If deadlines not reached before forcing manual flush
                                if (!ok)
                                {
                                    Messages.Message("HuntingForMe_ForceHuntingNeedWait".Translate(Utils.TranslateTicksToTextIRLSeconds((__instance.pawn.TryGetComp<Comp_Hunting>().huntingForceNextGT - gt)), __instance.pawn.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                                }
                                else
                                {
                                    //If still in the process of hunting we cancel
                                    if (__instance.pawn.CurJob.def.defName == "HuntTrained"
                                    || __instance.pawn.CurJob.def.defName == "HuntTrainedAssist"
                                    || __instance.pawn.CurJob.def.defName == "HuntForMe_CatGift")
                                    {
                                        Messages.Message("HuntForMe_AlreadyHunting".Translate(), MessageTypeDefOf.NeutralEvent, false);
                                        return;
                                    }

                                    JobGiver_AnimalHunt jgah = new JobGiver_AnimalHunt();
                                    //Indicator allowing to know that it is about a manual triggering of the hunt so we want a notification
                                    jgah.manualCall = true;
                                    JobIssueParams jb = new JobIssueParams();
                                    ThinkResult res = jgah.TryIssueJobPackage(__instance.pawn, jb);
                                    if (res.IsValid)
                                    {
                                        __instance.pawn.jobs.StopAll();
                                        __instance.pawn.jobs.StartJob(res.Job);
                                        Messages.Message("HuntingForMe_ForceHuntingOK".Translate(__instance.pawn.LabelCap), MessageTypeDefOf.PositiveEvent, false);
                                    }
                                }
                            }
                        };
                        ret.Add(cur);
                    }
                }
                if(ret.Count >= 1)
                    __result = __result.Concat(ret);
            }
        }

        [HarmonyPatch(typeof(Pawn_MindState), "StartManhunterBecauseOfPawnAction")]
        public class Pawn_MindState_StartManhunterBecauseOfPawnAction
        {
            [HarmonyPrefix]
            public static bool Replacement(Pawn instigator, string letterTextKey, bool causedByDamage, Pawn ___pawn)
            {
                //If the aggressor using one of HFM hunting jobs and the setting to disable prey revenge is enabled => do nothing
                if(Settings.disallowManhunter && (instigator.CurJob != null && (instigator.CurJob.def.defName == "HuntTrained" 
                    || instigator.CurJob.def.defName == "HuntTrainedAssist" 
                    || instigator.CurJob.def.defName == "HuntForMe_CatGift")))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
