//using HarmonyLib;
//using RimWorld;
//using Verse;

//namespace Analyzer.Performance
//{
//    [PerfPatch]
//    internal class H_Alert_Clothing
//    {
//        public static AlertInfo tattered = new AlertInfo();
//        public static AlertInfo nudist = new AlertInfo();

//        public static void PerformancePatch(Harmony harmony)
//        {
//            var firstFlag = AccessTools.Method(typeof(Pawn_ApparelTracker),
//                nameof(Pawn_ApparelTracker.TakeWearoutDamageForDay)); // pawns apparel has taken wearout damage
//            var secondFlag = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear),
//                new[] {typeof(Apparel), typeof(bool), typeof(bool)}); // pawn has equiped new clothes
//            var thirdFlag = AccessTools.Method(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.TryDrop),
//                new[]
//                {
//                    typeof(Apparel), typeof(Apparel).MakeByRefType(), typeof(IntVec3), typeof(bool)
//                }); // pawn has removed a piece of clothing

//            var reaction = new HarmonyMethod(typeof(H_Alert_Clothing), nameof(UpdateFlag));
//            harmony.Patch(firstFlag, null, reaction);
//            harmony.Patch(secondFlag, null, reaction);
//            harmony.Patch(thirdFlag, null, reaction);

//            var meth = AccessTools.Method(typeof(Alert_Thought), nameof(Alert_Thought.GetReport));
//            var pre = new HarmonyMethod(typeof(H_Alert_Clothing), nameof(AlertCheck));
//            var post = new HarmonyMethod(typeof(H_Alert_Clothing), nameof(PostCheck));

//            harmony.Patch(meth, pre, post);
//        }

//        public static void UpdateFlag()
//        {
//            if (!Analyzer.Settings.OptimiseAlerts) return;

//            tattered.dirty = true;
//            nudist.dirty = true;
//        }

//        public static bool AlertCheck(Alert __instance)
//        {
//            if (!Analyzer.Settings.OptimiseAlerts) return true;

//            if (__instance is Alert_TatteredApparel)
//                if (tattered.Dirty())
//                    return true;

//            if (__instance is Alert_UnhappyNudity)
//                if (nudist.Dirty())
//                    return true;

//            return false;
//        }

//        public static void PostCheck(Alert __instance, ref AlertReport __result)
//        {
//            if (!Analyzer.Settings.OptimiseAlerts) return;

//            if (__instance is Alert_TatteredApparel)
//            {
//                if (!tattered.changed)
//                    __result = tattered.report;
//                else
//                    tattered.Update(__result);

//                return;
//            }

//            if (__instance is Alert_UnhappyNudity)
//            {
//                if (!nudist.changed)
//                    __result = nudist.report;
//                else
//                    nudist.Update(__result);
//            }
//        }
//    }
//}