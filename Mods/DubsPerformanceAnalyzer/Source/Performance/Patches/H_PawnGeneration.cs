//using HarmonyLib;
//using RimWorld;
//using RimWorld.Planet;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Emit;
//using System.Text;
//using System.Threading.Tasks;
//using Analyzer.Profiling;
//using Verse;

//namespace Analyzer.Performance
//{
//    public class H_PawnGeneration : PerfPatch
//    {
//        public static bool Enabled = true;
//        private static IEnumerable<Pawn> ValidCandidates;

//#pragma warning disable 414 // unused field error for release mode.
//        private static Pawn[] pawns = null;
//#pragma warning restore 414
//        private static Dictionary<PawnKindDef, Tuple<NeededWarmth, float>> tempCache = new Dictionary<PawnKindDef, Tuple<NeededWarmth, float>>();
//        public override string Name => "expPawnGen";

//        public override void OnEnabled(Harmony harmony)
//        {
//            try
//            {
//                harmony.Patch(AccessTools.Method(typeof(RaidStrategyWorker), nameof(RaidStrategyWorker.SpawnThreats)), prefix: new HarmonyMethod(typeof(H_PawnGeneration), "Prefix"));
//                harmony.Patch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GetValidCandidatesToRedress)), prefix: new HarmonyMethod(typeof(H_PawnGeneration), "Prefix_ValidCandidates"));
//                harmony.Patch(AccessTools.Method(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.ApparelWarmthNeededNow)), prefix: new HarmonyMethod(typeof(H_PawnGeneration), "Prefix_ApparelWarmthNeededNow"));
//            }
//            catch (Exception e)
//            {
//                ThreadSafeLogger.Error("hi, what" + e.Message);
//            }
//        }

//        public override void OnDisabled(Harmony harmony)
//        {
//            harmony.Unpatch(AccessTools.Method(typeof(RaidStrategyWorker), nameof(RaidStrategyWorker.SpawnThreats)), HarmonyPatchType.Prefix, harmony.Id);
//            harmony.Unpatch(AccessTools.Method(typeof(PawnGenerator), nameof(PawnGenerator.GetValidCandidatesToRedress)), HarmonyPatchType.Prefix, harmony.Id);
//            harmony.Unpatch(AccessTools.Method(typeof(PawnApparelGenerator), nameof(PawnApparelGenerator.ApparelWarmthNeededNow)), HarmonyPatchType.Prefix, harmony.Id);
//        }


//        public static bool Prefix_ValidCandidates(ref IEnumerable<Pawn> __result)
//        {
//            if (ValidCandidates == null) return true;

//            __result = ValidCandidates;
//            return false;
//        }

//        public static bool Prefix_ApparelWarmthNeededNow(Pawn pawn, PawnGenerationRequest request, out float mapTemperature, NeededWarmth __result)
//        {
//            mapTemperature = 0;
//            if (ValidCandidates == null) return true;

//            if (tempCache.TryGetValue(pawn.kindDef, out var tuple))
//            {
//                __result = tuple.Item1;
//                mapTemperature = tuple.Item2;
//            }
//            else
//            {
//                var neededWarmth = PawnApparelGenerator.ApparelWarmthNeededNow(pawn, request, out float maptemp);
//                tempCache.Add(pawn.kindDef, Tuple.Create(neededWarmth, maptemp));

//                __result = neededWarmth;
//                mapTemperature = maptemp;
//            }

//            return false;
//        }

//        public static bool Prefix(RaidStrategyWorker __instance, IncidentParms parms, ref List<Pawn> __result)
//        {
//            if (parms.pawnKind != null)
//            {
//                List<Pawn> list = new List<Pawn>();
//                PawnGenerationRequest request = new PawnGenerationRequest(parms.pawnKind, parms.faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, biocodeWeaponChance: parms.biocodeWeaponsChance, allowFood: __instance.def.pawnsCanBringFood);
//                ValidCandidates = PawnGenerator.GetValidCandidatesToRedress(request).ToHashSet();
//                for (int i = 0; i < parms.pawnCount; i++)
//                {
//                    request.BiocodeApparelChance = 1f;
//                    Pawn pawn = PawnGenerator.GeneratePawn(request);
//                    if (pawn != null)
//                    {
//                        list.Add(pawn);
//                        if (ValidCandidates.Contains(pawn))
//                        {
//                            ValidCandidates = PawnGenerator.GetValidCandidatesToRedress(request).ToHashSet();
//                        }
//                    }
//                }

//                ValidCandidates = null;
//                tempCache = null;
//                if (list.Any())
//                {
//                    parms.raidArrivalMode.Worker.Arrive(list, parms);
//                    __result = list;
//                }
//            }
//            __result = null;

//            return false;
//        }
//    }
//}
