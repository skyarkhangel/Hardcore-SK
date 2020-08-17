using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Hospitality.Harmony
{
    public class JobGiver_Work_Patch
    {
        /// <summary>
        /// Allow guests to do work they enjoy and are reasonably good at
        /// </summary>
        [HarmonyPatch(typeof(JobGiver_Work), "PawnCanUseWorkGiver")]
        public class PawnCanUseWorkGiver
        {
            [HarmonyPrefix]
            public static bool Prefix(ref bool __result, Pawn pawn, WorkGiver giver)
            {
                if (!pawn.IsGuest()) return true;

                var canDo = !giver.ShouldSkip(pawn) && giver.MissingRequiredCapacity(pawn) == null && IsSkilledEnough(pawn, giver.def.workType);
                if (!canDo) return false;

                if (IsNegotiating(giver)) return false;
                if (Settings.disableArtAndCraft.Value && IsArtOrCraft(giver.def.workType.workTags)) return false;
                if (Settings.disableOperations.Value && IsOperation(giver)) return false;
                if (Settings.disableMedical.Value && IsOperation(giver) || IsMedical(giver)) return false;
                if (Settings.disableWarden.Value && IsWarden(giver)) return false;
                if (IsFeeding(giver)) return false; // Too many problems with this (uses food from inventory, wrong food category, etc.

                if (!pawn.GetVisitScore(out var score)) return false;

                var passion = pawn.skills.MaxPassionOfRelevantSkillsFor(giver.def.workType);
                float passionBonus = passion == Passion.Major ? 40 : passion == Passion.Minor ? 20 : 0;

                var desireToHelp = pawn.Faction.GoodwillWith(Faction.OfPlayer) + passionBonus + score*100 + (giver.def.emergency ? 75 : 0);
                //Log.Message($"{pawn.LabelShort}: help with {giver.def.gerund}? {desireToHelp:F0} >= {100 + Rand.ValueSeeded(pawn.thingIDNumber ^ 3436436) * 100:F0}");
                if (desireToHelp < 100 + Rand.ValueSeeded(pawn.thingIDNumber ^ 3436436)*100) return false;

                __result = true;
                return false;
            }

            private static bool IsNegotiating(WorkGiver giver)
            {
                return giver is WorkGiver_Diplomat || giver is WorkGiver_Recruiter;
            }

            private static bool IsArtOrCraft(WorkTags workTags)
            {
                var tags = workTags.GetAllSelectedItems<WorkTags>();
                foreach (var tag in tags)
                {
                    if (tag == WorkTags.Crafting || tag == WorkTags.Artistic) return true;
                }
                return false;
            }

            private static bool IsOperation(WorkGiver workGiver)
            {
                return workGiver is WorkGiver_DoBill && (workGiver.def.billGiversAllHumanlikes || workGiver.def.billGiversAllAnimals);
            }

            private static bool IsMedical(WorkGiver workGiver)
            {
                return workGiver is WorkGiver_Tend;
            }

            private static bool IsWarden(WorkGiver workGiver)
            {
                return workGiver is WorkGiver_Warden;
            }

            private static bool IsFeeding(WorkGiver workGiver)
            {
                return workGiver is WorkGiver_FeedPatient || workGiver is WorkGiver_Warden_Feed;
            }

            private static bool IsSkilledEnough(Pawn pawn, WorkTypeDef workTypeDef)
            {
                if (workTypeDef.relevantSkills.Count == 0) return true;
                return pawn.skills.AverageOfRelevantSkillsFor(workTypeDef) >= Settings.minGuestWorkSkill.Value;
            }
        }

        /// <summary>
        /// Make sure they have workSettings.priorities before they attempt to do work
        /// </summary>
        [HarmonyPatch(typeof(JobGiver_Work), "TryIssueJobPackage")]
        public class TryIssueJobPackage
        {
            private static FieldInfo _fieldPriorities = typeof(Pawn_WorkSettings).GetField("priorities", BindingFlags.NonPublic | BindingFlags.Instance);
            private static ConceptDef guestWorkDef = ConceptDef.Named("GuestWork");

            public static bool Prefix(Pawn pawn)
            {
                if (!Settings.disableWork && pawn.IsGuest())
                {
                    var priorities = _fieldPriorities.GetValue(pawn.workSettings);
                    if (priorities == null)
                    {
                        pawn.workSettings.EnableAndInitialize();
                        LessonAutoActivator.TeachOpportunity(guestWorkDef, pawn, OpportunityType.GoodToKnow);
                    }
                }
                return true;
            }
        }
    }
}