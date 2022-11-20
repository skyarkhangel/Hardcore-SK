using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace aRandomKiwi.HFM
{
    [StaticConstructorOnStartup]
    static class Utils
    {

        public static bool disableMoveSpeedPatch = false;
        static Utils()
        {
            
        }

        public static bool hasRemoteVerbAttack(IEnumerable<Verb> verbs, Pawn pawn)
        {
            foreach(var v in verbs)
            {
                if (v != null && v.verbProps != null && v.Available() && v.verbProps.LaunchesProjectile && v.IsStillUsableBy(pawn))
                {
                    //Log.Message("==>" + v.GetDamageDef().defName);
                    return true;
                }
            }
            return false;
        }

        public static string TranslateTicksToTextIRLSeconds(int ticks)
        {
            //If less than one hour ingame then display seconds
            if (ticks < 2500)
                return ticks.ToStringSecondsFromTicks();
            else
                return ticks.ToStringTicksToPeriodVerbose(true);
        }

        /*
         * Cancellation of a current pack, notification of all members to quit, reset cooldowns only if manualStop == false
         */
        public static void CancelHuntingPack(Pawn leader, Pawn caller, bool manualStop)
        {
            Comp_Hunting ch = leader.GetComp<Comp_Hunting>();
            if (ch == null)
                return;

            foreach (var asst in ch.huntingPackMembers)
            {
                if (asst != null && asst.jobs != null && caller != asst)
                {
                    //Log.Message("StopJob == " + asst.Label);
                    asst.jobs.StopAll();
                }
                //Log.Message("STOP OTHER PREDATOR JOB");
            }

            if (!manualStop)
            {
                ch.resetNextGT();
                foreach (var asst in ch.huntingPackMembers)
                {
                    if (asst != null && asst.jobs != null)
                    {
                        //Log.Message("ResetNextGT == " + asst.Label);
                        asst.TryGetComp<Comp_Hunting>().resetNextGT();
                    }
                }
            }
            //If caller! = Leader then we stop the jobs on the leader too
            if (leader != caller)
            {
                leader.jobs.StopAll();
                if(!manualStop)
                    leader.TryGetComp<Comp_Hunting>().resetNextGT();
            }

        }


        /*
         * Dynamic modification of "HuntingTraining"
         */
        static public void setAllowAllToHuntState()
        {
            TrainableDef hunting = DefDatabase<TrainableDef>.GetNamed("HuntingTraining");

            if (Settings.allowAllToHunt)
            {
                //Removal of all prerequisites except obedience
                foreach (var x in hunting.prerequisites.ToList())
                {
                    if (x.defName != "Obedience")
                        hunting.prerequisites.Remove(x);
                }
                hunting.minBodySize = 0;
                hunting.requiredTrainability = DefDatabase<TrainabilityDef>.GetNamed("None");
            }
            else
            {
                bool findObedience = false;
                bool findHaul = false;

                //Addition of prerequisites if not found
                foreach (var x in hunting.prerequisites.ToList())
                {
                    if (x.defName == "Obedience")
                        findObedience = true;
                    if (x.defName == "Haul")
                        findHaul = true;
                }

                if (!findObedience)
                {
                    hunting.prerequisites.Add(DefDatabase<TrainableDef>.GetNamed("Obedience"));
                }
                if (!findHaul)
                    hunting.prerequisites.Add(DefDatabase<TrainableDef>.GetNamed("Haul"));

                hunting.minBodySize = 0.4f;
                hunting.requiredTrainability = DefDatabase<TrainabilityDef>.GetNamed("Advanced");
            }
        }

        //Builtin List of uninterruptible jobs
        public static List<string> notInterruptibleJobs = new List<string>(){ "KFM_KillTarget", "KFM_GroupToPoint" };

        public static int maxTicksToCompleteHunting = 20000;

        public static readonly Texture2D texHunting = ContentFinder<Texture2D>.Get("UI/Icons/Hunting", true);
        public static readonly Texture2D texHuntingSupervised = ContentFinder<Texture2D>.Get("UI/Icons/HuntingSupervised", true);
        public static readonly Texture2D texForceHunting = ContentFinder<Texture2D>.Get("UI/Icons/ForceHunting", true);
        public static readonly Texture2D texForceHuntingDisabled = ContentFinder<Texture2D>.Get("UI/Icons/ForceHuntingDisabled", true);
        public static readonly Texture2D texForceKittyGift = ContentFinder<Texture2D>.Get("UI/Icons/KittyGift", true);
        public static readonly Texture2D texForceKittyGiftDisabled = ContentFinder<Texture2D>.Get("UI/Icons/KittyGiftDisabled", true);
        public static readonly Texture2D texForceStopHunting = ContentFinder<Texture2D>.Get("UI/Icons/ForceStopHunting", true);

        public static readonly Texture2D texIsACat = ContentFinder<Texture2D>.Get("UI/Icons/KFM_IsACat", true);
        public static readonly Texture2D texIgnoredPreys = ContentFinder<Texture2D>.Get("UI/Icons/KFM_IgnoredPreys", true);
        public static readonly Texture2D texMeleeForced = ContentFinder<Texture2D>.Get("UI/Icons/KFM_MeleeForced", true);
    }
}
