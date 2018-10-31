using RimWorld;
using Verse;
using Harmony;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;

namespace Dismiss_Trader
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("mehni.rimworld.traderdismissal.main");

            harmony.Patch(AccessTools.Method(typeof(FloatMenuMakerMap), "AddHumanlikeOrders"), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(FloatMenuMakerMap_AddHumanlikeOrdersToDismissTraders_PostFix)));
        }

        private static void FloatMenuMakerMap_AddHumanlikeOrdersToDismissTraders_PostFix(ref Vector3 clickPos, ref Pawn pawn, ref List<FloatMenuOption> opts)
        {
            foreach (LocalTargetInfo target in GenUI.TargetsAt(clickPos, TargetingParameters.ForTrade(), true))
            {
                Pawn localpawn = pawn;
                LocalTargetInfo dest = target;
                if (!pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly)) return;
                if (pawn.skills.GetSkill(SkillDefOf.Social).TotallyDisabled) return;
                
                Pawn pTarg = (Pawn)dest.Thing;
                void Action()
                {
                    Job job = new Job(TraderDismissalJobDefs.DismissTrader, pTarg) { playerForced = true };
                    localpawn.jobs.TryTakeOrderedJob(job);
                }

                string str = string.Empty;
                if (pTarg.Faction != null)
                {
                    str = " (" + pTarg.Faction.Name + ")";
                }

                string label = "GETOUT".Translate(pTarg.LabelShort + ", " + pTarg.TraderKind.label) + str;

                opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, Action, MenuOptionPriority.InitiateSocial, null, dest.Thing), pawn, pTarg));
            }
        }
    }

    [DefOf]
    public class TraderDismissalJobDefs
    {
        public static JobDef DismissTrader;
    }
}