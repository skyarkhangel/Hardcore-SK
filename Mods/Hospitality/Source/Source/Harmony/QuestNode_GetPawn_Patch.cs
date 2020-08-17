using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace Hospitality.Harmony {
    /// <summary>
    /// For testing. Remove eventually.
    /// </summary>
    [Obsolete]
    internal static class QuestNode_GetPawn_Patch
    {
        [HarmonyPatch(typeof(QuestNode_GetPawn), "TryFindFactionForPawnGeneration")]
        public class TryFindFactionForPawnGeneration
        {
            [HarmonyPostfix]
            public static bool Prefix(Slate slate, out Faction faction, QuestNode_GetPawn __instance, ref bool __result)
            {
                Log.Message($"Trying to get factions for quest {__instance}");
                __result = Internal(slate, out faction, __instance);
                Log.Message($"Result is {__result}, faction is {faction}");
                return false;
            }

            private static bool Internal(Slate slate, out Faction faction, QuestNode_GetPawn __instance)
            {
                Log.Message($"Parameters are royal titles: {__instance.mustHaveRoyalTitleInCurrentFaction.GetValue(slate)}, " 
                            + $"non hostile: {__instance.mustBeNonHostileToPlayer.GetValue(slate)}, " 
                            + $"allow permanent enemy: {!(__instance.allowPermanentEnemyFaction.GetValue(slate) ?? false) }, " 
                            + $"min tech level: {__instance.minTechLevel.GetValue(slate)}");
                return Find.FactionManager.GetFactions(allowHidden: false, allowDefeated: false, allowNonHumanlike: false).Where(delegate(Faction x) {
                    if (__instance.mustHaveRoyalTitleInCurrentFaction.GetValue(slate) && !x.def.HasRoyalTitles)
                    {
                        return false;
                    }

                    if (__instance.mustBeNonHostileToPlayer.GetValue(slate) && x.HostileTo(Faction.OfPlayer))
                    {
                        return false;
                    }

                    if (!(__instance.allowPermanentEnemyFaction.GetValue(slate) ?? false) && x.def.permanentEnemy)
                    {
                        return false;
                    }

                    return ((int) x.def.techLevel >= (int) __instance.minTechLevel.GetValue(slate)) ? true : false;
                }).TryRandomElement(out faction);
            }
        }
    }
}
