using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CommonSense
{
    static class PatchBadHy
    {
        static MethodBase target;
        static Type TNeed_Bladder;
        static Type TNeed_Hygiene;
        static Type TNeed_Thirst;

        static Need TryGetNeed(this Pawn pawn, Type needType)
        {
            if (pawn.needs == null)
                return null;
            foreach (var i in (pawn.needs.AllNeeds))
            {
                if (i.GetType() == needType)
                {
                    return i;
                }
            }
            return null;
        }

        [HarmonyPatch]
        static class JobGiver_UseToilet_GetPriority_CommonSensePatch
        {
            static bool Prepare()
            {
                Type type;
                if ((type = AccessTools.TypeByName("JobGiver_UseToilet")) != null)
                {
                    //Log.Message($"patching {type}");
                    if ((target = AccessTools.Method(type, "GetPriority"))==null)
                    {
                        Log.Error($"Couldn't get {type}.GetPriority");
                        return false;
                    }
                    TNeed_Bladder = AccessTools.TypeByName("Need_Bladder");
                    if(TNeed_Bladder == null)
                    {
                        Log.Error($"Couldn't get class Need_Bladder");
                        return false;
                    }
                    //TNeed_Hygiene = AccessTools.TypeByName("Need_Hygiene");
                    //TNeed_Thirst = AccessTools.TypeByName("Need_Thirst");
                    return true;
                }
                return false;
            }
            static MethodBase TargetMethod()
            {
                return target;
            }

            static bool Prefix(ref float __result, Pawn pawn)
            {
                if (!Settings.fun_police)
                    return true;

                var need_bladder = pawn.TryGetNeed(TNeed_Bladder);
                if (need_bladder == null)
                {
                    __result = 0f;
                    return false;
                }

                if (FoodUtility.ShouldBeFedBySomeone(pawn))
                {
                    __result = 0f;
                    return false;
                }

                if (need_bladder.CurLevel < 0.3f)
                {
                    __result = 9.6f;
                    return false;
                }

                if (pawn.timetable == null)
                {
                    __result = 0f;
                    return false;
                }

                if (pawn.timetable.CurrentAssignment == TimeAssignmentDefOf.Joy && need_bladder.CurLevel < 0.8f)
                {
                    __result = 6.6f;
                    return false;
                }

                if (pawn.timetable.CurrentAssignment == TimeAssignmentDefOf.Sleep && need_bladder.CurLevel < 0.8f)
                {
                    __result = 3.6f;
                    return false;
                }

                __result = 0f;
                return false;
            }
        }

        [HarmonyPatch]
        static class JobGiver_HaveWash_GetPriority_CommonSensePatch
        {
            static bool Prepare()
            {
                Type type;
                if ((type = AccessTools.TypeByName("JobGiver_HaveWash")) != null)
                {
                    //Log.Message($"patching {type}");
                    if ((target = AccessTools.Method(type, "GetPriority")) == null)
                    {
                        Log.Error($"Couldn't get {type}.GetPriority");
                        return false;
                    }
                    TNeed_Hygiene = AccessTools.TypeByName("Need_Hygiene");
                    if (TNeed_Hygiene == null)
                    {
                        Log.Error($"Couldn't get class Need_Hygiene");
                        return false;
                    }
                    return true;
                }
                return false;
            }

            static MethodBase TargetMethod()
            {
                return target;
            }

            static bool Prefix(ref float __result, Pawn pawn)
            {
                if (!Settings.fun_police)
                    return true;

                var need_hygiene = pawn.TryGetNeed(TNeed_Hygiene);
                if (need_hygiene == null)
                {
                    __result = 0f;
                    return false;
                }

                if (FoodUtility.ShouldBeFedBySomeone(pawn))
                {
                    __result = 0f;
                    return false;
                }

                if (need_hygiene.CurLevel < 0.3f)
                {
                    __result = 9.25f;
                    return false;
                }

                if (pawn.timetable == null)
                {
                    __result = 0f;
                    return false;
                }

                if (pawn.timetable.CurrentAssignment == TimeAssignmentDefOf.Joy && need_hygiene.CurLevel < 0.55f)
                {
                    __result = 6.25f;
                    return false;
                }

                if (pawn.timetable.CurrentAssignment == TimeAssignmentDefOf.Sleep && need_hygiene.CurLevel < 0.55f)
                {
                    __result = 3.25f;
                    return false;
                }

                __result = 0f;
                return false;
            }
        }

        [HarmonyPatch]
        static class JobGiver_DrinkWater_GetPriority_CommonSensePatch
        {
            static bool Prepare()
            {
                Type type;
                if ((type = AccessTools.TypeByName("JobGiver_DrinkWater")) != null)
                {
                    //Log.Message($"patching {type}");
                    if ((target = AccessTools.Method(type, "GetPriority")) == null)
                    {
                        Log.Error($"Couldn't get {type}.GetPriority");
                        return false;
                    }
                    TNeed_Thirst = AccessTools.TypeByName("Need_Thirst");
                    if (TNeed_Thirst == null)
                    {
                        Log.Error($"Couldn't get class Need_Thirst");
                        return false;
                    }
                    return true;
                }
                return false;
            }

            static MethodBase TargetMethod()
            {
                return target;
            }

            static bool Prefix(ref float __result, Pawn pawn)
            {
                if (!Settings.fun_police)
                    return true;

                var need_thirst = pawn.TryGetNeed(TNeed_Thirst);
                if (need_thirst == null)
                {
                    __result = 0f;
                    return false;
                }

                if (FoodUtility.ShouldBeFedBySomeone(pawn))
                {
                    __result = 0f;
                    return false;
                }

                if (need_thirst.CurLevel < 0.3f)
                {
                    __result = 9.6f;
                    return false;
                }

                if (pawn.timetable == null)
                {
                    __result = 0f;
                    return false;
                }

                if (pawn.timetable.CurrentAssignment == TimeAssignmentDefOf.Joy && need_thirst.CurLevel < 0.8f)
                {
                    __result = 6.6f;
                    return false;
                }

                if (pawn.timetable.CurrentAssignment == TimeAssignmentDefOf.Sleep && need_thirst.CurLevel < 0.8f)
                {
                    __result = 3.6f;
                    return false;
                }

                __result = 0f;
                return false;
            }
        }
    }
}
