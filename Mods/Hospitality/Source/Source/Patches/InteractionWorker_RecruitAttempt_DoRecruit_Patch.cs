using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using Verse;

namespace Hospitality.Patches
{
    public static class InteractionWorker_RecruitAttempt_DoRecruit_Patch
    {
        [HarmonyPatch(typeof(InteractionWorker_RecruitAttempt), "DoRecruit", typeof(Pawn), typeof(Pawn), typeof(bool))]
        public class DoRecruit
        {
            public static void Postfix(Pawn recruiter, Pawn recruitee, bool useAudiovisualEffects = true)
            {
                if (recruiter.IsArrivedGuest(out _))
                {
                    if (recruiter.HostFaction == recruiter.Faction) return;

                    Faction faction = recruiter.HostFaction;
                    recruitee.SetFaction(faction);
                }
            }
        }
    }
}
