using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using yayoAni.Data;

namespace yayoAni
{
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnAt")]
    public static class Patch_RenderPawnAt2
    {
        public static bool Equal(this CodeInstruction ci, OpCode oc)
        {
            return ci.opcode == oc;
        }

        public static bool OpLoc(this CodeInstruction ci, OpCode oc, int i)
        {
            if (ci.opcode == oc && ci.operand is LocalBuilder localBuilder)
                return localBuilder.LocalIndex == i;

            return false;
        }

        public enum FindPointType
        {
            start,
            after
        }

        public static bool FindPoint(this List<CodeInstruction> ar, List<OpCode> target, out int point, FindPointType findType)
        {
            point = -1;
            for (int i = 0; i < ar.Count - target.Count; i++)
            {
                for (int j = 0; j < target.Count; j++)
                {
                    if (ar[i + j].Equal(target[j]))
                    {
                        if (j == target.Count - 1)
                        {
                            switch (findType)
                            {
                                case FindPointType.start:
                                    point = i;
                                    break;
                                case FindPointType.after:
                                    point = i + j + 1;
                                    break;
                            }

                            return true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return false;
        }


        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> arCi = instructions.ToList();

            // --------------------

            #region GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, rot, PawnDrawMode.BodyAndHead), drawLoc, Quaternion.AngleAxis(0f, Vector3.up), original, drawNow: false);

            var arFind = new List<OpCode>()
            {
                OpCodes.Ldarg_0,
                OpCodes.Ldloc_S,
                OpCodes.Ldloc_0,
                OpCodes.Ldc_I4_0,
                OpCodes.Call,
                OpCodes.Ldarg_1,
                OpCodes.Ldc_R4,
                OpCodes.Call,
                OpCodes.Call,
                OpCodes.Ldloc_S,
                OpCodes.Ldc_I4_0,
                OpCodes.Call
            };

            if (arCi.FindPoint(arFind, out var point, FindPointType.start))
            {
                //for (int i = point; i < point + ar_find.Count; i++)
                //{
                //    Log.Message($"{i} : {ar_ci[i].opcode}");
                //}
                //Log.Message($"--------------change-----------------");

                var tmpPoint = point + 11;
                arCi.RemoveRange(tmpPoint, 1);
                var arInsert = new List<CodeInstruction>()
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn))),
                    new(OpCodes.Call, AccessTools.Method(typeof(Yayo), nameof(Yayo.DrawMeshNowOrLater)))
                };
                arCi.InsertRange(tmpPoint, arInsert);


                tmpPoint = point + 4;
                arCi.RemoveRange(tmpPoint, 1);
                arInsert = new List<CodeInstruction>()
                {
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.pawn))),
                    new(OpCodes.Call, AccessTools.Method(typeof(Yayo), nameof(Yayo.GetBlitMeshUpdatedFrame)))
                };
                arCi.InsertRange(tmpPoint, arInsert);


                tmpPoint = point + 0;
                arCi.RemoveRange(tmpPoint, 1);


                //for (int i = point; i < point + ar_find.Count; i++)
                //{
                //	Log.Message($"{i} : {ar_ci[i].opcode}");

                //}
            }

            #endregion

            #region RenderPawnInternal(drawLoc, 0f, renderBody: true, rot, curRotDrawMode, pawnRenderFlags);

            //ar_find = new List<OpCode>()
            //{
            //	//IL_0106: br.s IL_0118
            //	OpCodes.Br_S,
            //	//IL_0108: ldarg.0
            //	OpCodes.Ldarg_0,
            //	//IL_0109: ldarg.1
            //	OpCodes.Ldarg_1,
            //	//IL_010a: ldc.r4 0.0
            //	OpCodes.Ldc_R4,
            //	//IL_010f: ldc.i4.1
            //	OpCodes.Ldc_I4_1,
            //	//IL_0110: ldloc.0
            //	OpCodes.Ldloc_0,
            //	//IL_0111: ldloc.2
            //	OpCodes.Ldloc_2,
            //	//IL_0112: ldloc.1
            //	OpCodes.Ldloc_1,
            //	//IL_0113: call instance void Verse.PawnRenderer::RenderPawnInternal(valuetype[UnityEngine.CoreModule]UnityEngine.Vector3, float32, bool, valuetype Verse.Rot4, valuetype Verse.RotDrawMode, valuetype Verse.PawnRenderFlags)
            //	OpCodes.Call
            //};

            //if (ar_ci.findPoint(ar_find, out point, findPointType.start))
            //{
            //             for (int i = point - 5; i < point + ar_find.Count + 5; i++)
            //             {
            //                 Log.Message($"{i} : {ar_ci[i].opcode} - {ar_ci[i].operand}");
            //             }
            //             Log.Message($"--------------change-----------------");

            //             tmp_point = point + 8;
            //	ar_ci.RemoveRange(tmp_point, 1);
            //	ar_insert = new List<CodeInstruction>()
            //	{
            //                 //new CodeInstruction(OpCodes.Ldarg_2),
            //		//new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn")),
            //		new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(yayo), "RenderPawnInternal"))

            //	};
            //	ar_ci.InsertRange(tmp_point, ar_insert);


            //             //ar_ci.RemoveRange(point + 0, 2);
            //             //ar_ci.Insert(point + 3, new CodeInstruction(OpCodes.Ldarg_2));


            //             for (int i = point - 5; i < point + ar_find.Count + 5; i++)
            //             {
            //                 Log.Message($"{i} : {ar_ci[i].opcode} - {ar_ci[i].operand}");

            //             }

            //         }

            #endregion


            // result out
            return arCi;
        }
    }


    [HotSwappable]
    public static class Yayo
    {
        public static Mesh GetBlitMeshUpdatedFrame(PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode, Pawn p)
        {
            var pdd = DataUtility.GetData(p);
            rotation = pdd.fixedRot ?? rotation;
            return p.Drawer.renderer.GetBlitMeshUpdatedFrame(frameSet, rotation, drawMode);
        }


        public static void DrawMeshNowOrLater(Mesh mesh, Vector3 loc, Quaternion quat, Material mat, bool drawNow, Pawn p)
        {
            var pdd = DataUtility.GetData(p);
            quat = Quaternion.AngleAxis(pdd.angleOffset, Vector3.up);
            GenDraw.DrawMeshNowOrLater(mesh, loc, quat, mat, drawNow);
        }


        //public static void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
        //{
        //	//pdd = dataUtility.GetData(p);
        //	//angle += pdd.offset_angle;
        // //         bodyFacing = pdd.fixed_rot ?? bodyFacing;
        // //         AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(p.Drawer.renderer, new object[] { rootLoc, angle, renderBody, bodyFacing, bodyDrawType, flags });
        //}

        public static void CheckAni(Pawn pawn, ref Vector3 pos, Rot4 rot, PawnDrawData pdd)
        {
            if (pawn.Dead)
                return;
            if (pdd.jobName != null && // Make sure we've cached some job before cancelling
                pdd.jobName == pawn.CurJob?.def.defName && // Check if the current pawn's job is the same as cached
                Find.TickManager.TicksGame < pdd.nextUpdateTick && // Check if it's the proper tick to update
                (pawn.pather == null || pawn.pather.MovingNow == false) && // Make sure pawn isn't moving
                (pawn.stances?.curStance is not Stance_Busy busy || busy.neverAimWeapon || !busy.focusTarg.IsValid)) // Make sure the pawn isn't aiming at something
            {
                pos += pdd.posOffset;
                return;
            }

            if (pawn.GetPosture() == PawnPosture.Standing)
            {
                Ani0(pawn, ref pos, rot, pdd);
            }
            else
            {
                Ani1(pawn, ref pos, rot, pdd);
            }
        }

        public enum AniType
        {
            none,
            doSomeThing,
            social,
            smash,
            idle,
            gameCeremony,
            crowd,
            solemn
        }

        public static void Ani0(Pawn pawn, ref Vector3 pos, Rot4 rot, PawnDrawData pdd)
        {
            bool changed = false;
            float oa = 0f;
            Vector3 op = Vector3.zero;
            int? nextUpdate = null;

            if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns || Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
            {
                // Ignored
            }
            else if (pawn.pather is { MovingNow: true })
            {
                if (Core.settings.walkEnabled &&
                    (!pawn.RaceProps.IsMechanoid || Core.settings.mechanoidWalkEnabled) &&
                    (!pawn.RaceProps.Animal || Core.settings.animalWalkEnabled))
                {
                    changed = true;
                    int IdTick = pawn.thingIDNumber * 20;

                    float walkSpeed = Core.settings.walkSpeed;
                    if (pawn.CurJob?.def.defName is "Hunt" or "GR_AnimalHuntJob")
                        walkSpeed *= 0.6f;

                    float wiggle = Mathf.Sin((Find.TickManager.TicksGame + IdTick) * 7f * walkSpeed / pawn.pather.nextCellCostTotal);
                    oa = wiggle * 9f * Core.settings.walkAngle;
                    op = new Vector3(wiggle * 0.025f, 0f, 0f);
                }
            }
            else if (Core.settings.anyJobEnabled && pawn.CurJob != null &&
                     (!pawn.RaceProps.IsMechanoid || Core.settings.mechanoidJobEnabled) &&
                     (!pawn.RaceProps.Animal || Core.settings.animalJobEnabled))
            {
                changed = true;
                pdd.jobName = pawn.CurJob.def.defName;
                int IdTick = pawn.thingIDNumber * 20;

                if (Core.settings.debugMode)
                    if (pawn.IsColonist)
                        Log.Message($"{pawn.NameShortColored} : {pawn.CurJob.def.defName}");

                //if (pawn.IsColonist) Log.Message($"{pawn.NameShortColored} : {pawn.CurJob.def.defName} / id {pawn.thingIDNumber}");
                //float wiggle = Mathf.Sin((Find.TickManager.TicksGame + IdTick) * 7f / pawn.pather.nextCellCostTotal);
                int t = 0;
                int t2;
                int total;
                AniType aniType = AniType.none;
                float f;
                Rot4 r;
                Rot4 tr;


                switch (pawn.CurJob.def.defName)
                {
                    // do something
                    case "UseArtifact":
                    case "UseNeurotrainer":
                    case "LinkPsylinkable":
                    case "UseStylingStation":
                    case "UseStylingStationAutomatic":
                    case "DyeHair":
                    case "Wear":
                    case "SmoothWall":
                    case "UnloadYourInventory":
                    case "UnloadInventory":
                    case "Uninstall":
                    case "Train":
                    case "TendPatient":
                    case "Tame":
                    case "FillFermentingBarrel":
                    case "TakeBeerOutOfFermentingBarrel":
#if IDEOLOGY
                    case "StudyThing":
#elif BIOTECH_PLUS
                    case "StudyBuilding":
#endif
                    case "Strip":
                    case "SmoothFloor":
                    case "SlaveSuppress":
                    case "PrisonerAttemptRecruit":
                    case "PrisonerEnslave":
                    case "PrisonerConvert":
                    case "DoBill": // 제작, 조리
                    case "Deconstruct":
                    case "FinishFrame": // 건설
                    case "Equip":
                    case "ExtractRelic":
                    case "ExtractToInventory":
                    case "ExtractSkull":
                    case "ExtractTree":
                    case "Replant":
                    case "GiveSpeech":
                    case "AcceptRole":
                    case "Hack":
                    case "InstallRelic":
                    case "Insult":
                    case "Milk":
                    case "Open":
                    case "Play_MusicalInstrument":
                    case "PlantSeed":
                    case "PruneGauranlenTree":
                    case "RearmTurret":
                    case "RearmTurretAtomic":
                    case "RecolorApparel":
                    case "Refuel":
                    case "RefuelAtomic":
                    case "Reload":
                    case "RemoveApparel":
                    case "RemoveFloor":
                    case "RemoveRoof":
                    case "BuildRoof":
                    case "Repair":
                    case "FixBrokenDownBuilding":
                    case "Research":
                    case "ApplyTechprint":
                    case "OperateDeepDrill":
                    case "OperateScanner":
                    case "Resurrect":
                    case "Sacrifice":
                    case "Scarify":
                    case "Shear":
                    case "Slaughter":
                    case "Ignite":
                    case "ManTurret":
                    case "Clean":
                    case "ClearSnow":
                    case "BuildSnowman":
                    case "HaulToContainer": // Bury pawn
#if BIOTECH_PLUS
                    case "PaintBuilding":
                    case "PaintFloor":
                    case "RemovePaintBuilding":
                    case "RemovePaintFloor":
                    case "InstallMechlink":
                    case "RemoveMechlink":
                    case "DisassembleMech":
                    case "CreateXenogerm":
                    case "ReadDatacore":
                    case "ClearPollution":
#endif
                    // Dubs Paint Shop
                    case "PaintThings":
                    case "PaintCells":
                    // Dubs Bad Hygiene
                    case "TriggerFireSprinkler":
                    case "emptySeptictank":
                    case "emptyLatrine":
                    case "LoadWashing":
                    case "UnloadWashing":
                    case "LoadComposter":
                    case "UnloadComposter":
                    case "RemoveSewage":
                    case "DrainWaterTankJob":
                    case "PlaceFertilizer":
                    case "DBHPackWaterBottle":
                    case "DBHStockpileWaterBottles":
                    case "DBHAdministerFluids":
                    case "useWashBucket":
                    case "washAtCell":
                    case "takeShower":
                    case "washHands":
                    case "washPatient":
                    case "RefillTub":
                    case "RefillWater:":
                    case "cleanBedpan":
                    case "clearBlockage":
                    // Dubs Rimatomics
                    case "RimatomicsResearch":
                    case "SuperviseConstruction":
                    case "SuperviseResearch":
                    case "UpgradeBuilding":
                    case "LoadRailgunMagazine":
                    case "LoadSilo":
                    case "UseReactorConsole":
                    case "RemoveFuelModule":
                    case "LoadSpentFuel":
                    case "UnloadPlutonium":
                    // Rimefeller
                    case "CleanOil":
                    case "SuperviseDrilling":
                    case "EmptyAutoclave":
                    case "FillAutoclave":
                    case "OperateResourceConsole":
                    // Vanilla Furniture Core
                    case "Play_Arcade":
                    case "Play_Piano":
                    case "Play_ComputerIndustrial":
                    case "Play_ComputerModern":
                    // Vanilla Furniture Security
                    case "VFES_RearmTrap":
                    // Vanilla Furniture Power
#if IDEOLOGY
                    // Removed in 1.4
                    case "VPE_JobPlugHole":
#endif
                    // Vanilla Factions Ancients
                    case "VFEA_Play_AncientEducator":
                    // Vanilla Factions Insectoid
                    case "VFEI_InsertFirstGenomeJob":
                    case "VFEI_InsertSecondGenomeJob":
                    case "VFEI_InsertThirdGenomeJob":
                    case "VFEM_RefuelSilo":
                    // Vanilla Factions Pirates
                    case "VFEP_DoWelding":
                    // Vanilla Factions Settlers
                    case "Play_FiveFingerFillet":
                    // Vanilla Factions Vikings
                    case "VFEV_TakeHoneyOutOfApiary":
                    case "VFEV_TendToApiary":
                    case "VFEV_ChangeFacepaint":
                    // Vanilla Factions Medieval
                    case "VFEM_DigTerrain":
                    case "VFEM_FillTerrain":
                    // Vanilla Genetics Expanded
                    case "GR_UseGenomeExcavator":
                    case "GR_HumanoidHybridRecruit":
                    case "GR_AnimalDeconstructJob":
                    case "GR_AnimalHarvestJob":
                    case "GR_InsertIngredients":
                    case "GR_InsertGrowthCell":
                    case "GR_InsertArchotechGrowthCell":
                    // Vanilla Genetics Expanded - More Lab Stuff
                    case "GR_UseGenetrainer":
                    // Vanilla Hair Expanded
                    case "VHE_ChangeHairstyle":
                    // Vanilla Ideology - Memes and Structures
                    case "VME_DeconstructBuilding":
                    case "VME_MaintainInsectNest":
                        aniType = AniType.doSomeThing;
                        break;


                    // social
                    case "GotoAndBeSociallyActive":
                    case "StandAndBeSociallyActive":
                    case "VisitSickPawn":
                    case "SocialRelax":
                    case "WatchTelevision":
                    // Dubs Bad Hygiene
                    case "WatchWashingMachine":
                    case "DBHGoSwimming":
                    case "DBHUseSauna":
                    // Vanilla Expanded Classical
                    case "VFEC_Stage_Performance":
                    case "VFEC_Stage_WatchPerformance":
                    // Vanilla Genetics Expanded
                    case "GR_HumanoidHybridTalk":
                    // Vanilla Social Interactions
                    case "VSIE_VentToFriend":
                    case "VSIE_TalkToSecondPawn":
                    case "VSIE_WatchTelevisionTogether":
                    // More social variant of those 2 interactions
                    case "VSIE_ViewArtTogether":
                    case "VSIE_BuildSnowmanTogether":
                        aniType = AniType.social;
                        break;

                    // Vanilla Expanded Classical
                    case "VFEC_Relax_Thermaebath":
                        var idTickMult = IdTick * 3;
                        var idTickDiv = (Find.TickManager.TicksGame + idTickMult) / 2500;
                        var seed = idTickDiv + idTickMult;
                        nextUpdate = (idTickDiv + 1) * 2500 - idTickMult;
                        rot = Rand.RangeSeeded(0, 4, seed) switch
                        {
                            0 => Rot4.East,
                            1 => Rot4.West,
                            2 => Rot4.South,
                            3 => Rot4.North,
                            _ => rot
                        };
                        aniType = AniType.social;
                        break;


                    case "Wait_Combat":
                    case "Wait":
                    case "Wait_SafeTemperature":
                    case "Wait_Wander":
                    case "ViewArt":
                    case "Meditate":
                    case "Pray":
                    case "Reign": // Meditate royally
                    // Dubs Bad Hygiene
                    case "haveWildPoo":
                    case "UseToilet":
                    // Vanilla Fishing Expanded
                    case "VCEF_FishJob":
                    // Vanilla Books Expanded
                    case "VBE_ReadBook":
                    // Vanilla Social Interactions
                    case "VSIE_StandAndHearVenting": // Don't do any movements that could be interpreted as excitement, etc.
                        aniType = AniType.idle;
                        break;


                    case "Vomit":
                        t = (Find.TickManager.TicksGame + IdTick) % 200;
                        if (!Core.Ani(ref t, 25, ref oa, 15f, 35f, -1f, ref op, rot))
                            if (!Core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot))
                                if (!Core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot))
                                    if (!Core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot))
                                        if (!Core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot))
                                            if (!Core.Ani(ref t, 25, ref oa, 35f, 25f, -1f, ref op, rot))
                                                if (!Core.Ani(ref t, 25, ref oa, 25f, 35f, -1f, ref op, rot))
                                                    Core.Ani(ref t, 25, ref oa, 35f, 15f, -1f, ref op, rot);

                        break;


                    // case "Mate":
                    //     break;


                    case "MarryAdjacentPawn":
                        t = (Find.TickManager.TicksGame) % 310;

                        if (!Core.Ani(ref t, 150, ref nextUpdate))
                        {
                            if (!Core.Ani(ref t, 20, ref oa, 0f, 5f, -1f, ref op, Vector3.zero, new Vector3(0.05f, 0f, 0f), rot))
                                if (!Core.Ani(ref t, 50, ref oa, 5f, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0f), new Vector3(0.05f, 0f, 0f), rot))
                                    if (!Core.Ani(ref t, 50, ref oa, 10, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0f), new Vector3(0.05f, 0f, 0f), rot))
                                        Core.Ani(ref t, 40, ref oa, 10f, 0f, -1f, ref op, new Vector3(0.05f, 0f, 0f), Vector3.zero, rot);
                        }

                        break;
                    case "SpectateCeremony": // 각종 행사, 의식 (결혼식, 장례식, 이념행사)
                        LordJob_Ritual ritualJob = Core.GetPawnRitual(pawn);
                        if (ritualJob == null) // 기본
                        {
                            aniType = AniType.crowd;
                        }
                        else if (ritualJob.Ritual == null)
                        {
                            // 로얄티 수여식 관중
                            aniType = AniType.solemn;
                        }
                        else
                        {
                            aniType = ritualJob.Ritual.def.defName switch
                            {
                                // 장례식
                                "Funeral" => AniType.solemn,
                                _ => AniType.crowd
                            };
                        }

                        break;
                    case "BestowingCeremony": // 로얄티 수여식 받는 대상
                    case "VisitGrave":
                    case "UseTelescope":
                    // Vanilla Social Interactions
                    case "VSIE_HonorPawn":
                        aniType = AniType.solemn;
                        break;


                    // case "Dance":
                    //     break;


                    // joy


                    case "Play_Hoopstone":
                    case "Play_Horseshoes":
                    // Vanilla Furniture Core
                    case "Play_DartsBoard":
                        t = (Find.TickManager.TicksGame + IdTick) % 60;
                        if (!Core.Ani(ref t, 30, ref oa, 10f, -20f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                        {
                            Core.Ani(ref t, 30, ref oa, -20f, 10f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot);
                        }

                        break;


                    case "Play_GameOfUr":
                    case "Play_Poker":
                    case "Play_Billiards":
                    case "Play_Chess":
                    // Vanilla Furniture Core
                    case "Play_Roulette":
                    // Vanilla Factions Ancients
                    case "VFEA_Play_AncientFoosballTable":
                    // Vanilla Factions Settlers
                    case "Play_Faro":
                    // Vanilla Factions Vikings
                    case "Play_Hnefatafl":
                        t = (Find.TickManager.TicksGame + IdTick * 27) % 900;
                        if (t <= 159)
                            aniType = AniType.gameCeremony;
                        else
                            aniType = AniType.doSomeThing;

                        break;


                    case "ExtinguishSelf": // 스스로 불 끄기

                        int tg = 10; // 틱 갭
                        t = (Find.TickManager.TicksGame + IdTick) % (12 * tg);
                        r = Rot4.East;

                        float cx = -0.5f;
                        float cy = 0f; // 중심점

                        float gx = -0.25f; // 이동 갭
                        float gy = 0.25f; // 이동 갭

                        int step = 0; // 이동 단계

                        float a = 45f;

                        if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r,
                                Core.tweenType.line))
                        {
                            rot = Core.Rot90(rot);
                            step++;
                            if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r,
                                    Core.tweenType.line))
                            {
                                rot = Core.Rot90(rot);
                                step++;
                                if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r,
                                        Core.tweenType.line))
                                {
                                    rot = Core.Rot90(rot);
                                    step++;


                                    // reverse
                                    if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step), new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r,
                                            Core.tweenType.line))
                                    {
                                        rot = Core.Rot90b(rot);
                                        step--;
                                        if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, Core.tweenType.line))
                                        {
                                            rot = Core.Rot90b(rot);
                                            step--;
                                            if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                    new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, Core.tweenType.line))
                                            {
                                                rot = Core.Rot90b(rot);
                                                step--;
                                                if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                        new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, Core.tweenType.line))
                                                {
                                                    rot = Core.Rot90b(rot);
                                                    step--;
                                                    if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                            new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, Core.tweenType.line))
                                                    {
                                                        rot = Core.Rot90b(rot);
                                                        step--;
                                                        if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                new Vector3(cx + gx * (step - 1), 0f, cy + gy * (step - 1)), r, Core.tweenType.line))
                                                        {
                                                            rot = Core.Rot90b(rot);
                                                            step--;

                                                            // reverse
                                                            if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                    new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r, Core.tweenType.line))
                                                            {
                                                                rot = Core.Rot90(rot);
                                                                step++;
                                                                if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                        new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r, Core.tweenType.line))
                                                                {
                                                                    rot = Core.Rot90(rot);
                                                                    step++;
                                                                    if (!Core.Ani(ref t, tg, ref oa, a, a, -1f, ref op, new Vector3(cx + gx * step, 0f, cy + gy * step),
                                                                            new Vector3(cx + gx * (step + 1), 0f, cy + gy * (step + 1)), r, Core.tweenType.line))
                                                                    {
                                                                        rot = Core.Rot90(rot);
                                                                        // step++;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        try
                        {
                            nextUpdate = int.MinValue; // Drawing extra stuff, don't cache this job
                            pawn.CurJob.targetA.Thing.DrawAt(pos + op + new Vector3(0f, 0.1f, 0f));
                        }
                        catch
                        {
                            // ignored
                        }


                        break;


                    case "Sow": // 씨뿌리기
                        t = (Find.TickManager.TicksGame + IdTick) % 50;

                        if (!Core.Ani(ref t, 35, ref nextUpdate))
                            if (!Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, rot))
                                Core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot);

                        break;


                    case "CutPlant": // 식물 베기
                    case "Harvest": // 자동 수확
                    case "HarvestDesignated": // 수동 수확
                        if (pawn.CurJob.targetA.Thing?.def.plant?.IsTree != null && pawn.CurJob.targetA.Thing.def.plant.IsTree)
                        {
                            aniType = AniType.smash;
                        }
                        else
                        {
                            aniType = AniType.doSomeThing;
                        }

                        break;

                    case "Mine": // 채굴
                    // Dubs Bad Hygiene
                    case "TipOverSewage":
                    // Vanilla Furniture Core
                    case "Play_PunchingBag":
                        aniType = AniType.smash;
                        break;

                    case "Ingest": // 밥먹기
                    case "EatAtCannibalPlatter":
                    // Dubs Bad Hygiene
                    case "DBHDrinkFromGround":
                    case "DBHDrinkFromBasin":
                    // Vanilla Social Interactions
                    case "VSIE_HaveMealTogether":
                        t = (Find.TickManager.TicksGame + IdTick) % 150;
                        f = 0.03f;
                        if (!Core.Ani(ref t, 10, ref oa, 0f, 15f, -1f, ref op, Vector3.zero, new Vector3(0f, 0f, 0f), rot))
                            if (!Core.Ani(ref t, 10, ref oa, 15f, 0f, -1f, ref op, Vector3.zero, new Vector3(0f, 0f, 0f), rot))
                                if (!Core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, Vector3.zero, new Vector3(0f, 0f, f), rot))
                                    if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot))
                                        if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot))
                                            if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot))
                                                if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot))
                                                    if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot))
                                                        Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot);

                        break;

                    default:
                        nextUpdate = int.MaxValue; // Update on a new job
                        break;
                }


                switch (aniType)
                {
                    case AniType.solemn:
                        t = (Find.TickManager.TicksGame + (IdTick % 25)) % 660;

                        if (!Core.Ani(ref t, 300, ref nextUpdate))
                        {
                            if (!Core.Ani(ref t, 30, ref oa, 0f, 15f, -1f, ref op, Vector3.zero, Vector3.zero, rot))
                                if (!Core.Ani(ref t, 300, ref oa, 15f, 15f, -1f, ref op, Vector3.zero, Vector3.zero, rot))
                                    Core.Ani(ref t, 30, ref oa, 15f, 0f, -1f, ref op, Vector3.zero, Vector3.zero, rot);
                        }

                        break;

                    case AniType.crowd:
                        total = 143;
                        t2 = (Find.TickManager.TicksGame + IdTick) % (total * 2);
                        t = t2 % total;
                        r = Core.Rot90(rot);
                        tr = rot;
                        if (!Core.Ani(ref t, 20, ref nextUpdate))
                        {
                            if (!Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r))
                                if (!Core.Ani(ref t, 20, ref oa, 10f, 10f, -1f, ref op, r))
                                    if (!Core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r))
                                        if (!Core.Ani(ref t, 20, ref oa, -10f, -10f, -1f, ref op, r))
                                        {
                                            if (!Core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                                            {
                                                tr = t2 >= total ? Core.Rot90(rot) : Core.Rot90b(rot);
                                                if (!Core.Ani(ref t, 15, ref oa, 0f, 0f, -1f, ref op, rot)) // 85
                                                {
                                                    tr = rot;
                                                    if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, rot)) // 105


                                                        if (t2 >= total)
                                                        {
                                                            if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                                                                Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, Core.tweenType.line);
                                                        }
                                                        else
                                                        {
                                                            Core.Ani(ref t, 33, ref nextUpdate);
                                                        }
                                                }
                                            }
                                        }
                        }

                        rot = tr;
                        break;

                    case AniType.gameCeremony:

                        // need 159 tick

                        // r = Core.Rot90(rot);
                        // tr = rot;

                        if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                        {
                            if (!Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, Core.tweenType.line))

                                if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                                    if (!Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, Core.tweenType.line))
                                    {
                                        rot = Core.Rot90b(rot);
                                        if (!Core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                        {
                                            rot = Core.Rot90b(rot);
                                            if (!Core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))

                                                if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.60f), rot))
                                                    if (!Core.Ani(ref t, 13, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.60f), new Vector3(0f, 0f, 0f), rot, Core.tweenType.line))
                                                        if (!Core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                                        {
                                                            rot = Core.Rot90b(rot);
                                                            if (!Core.Ani(ref t, 10, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                                            {
                                                                rot = Core.Rot90b(rot);
                                                                Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot);
                                                            }
                                                        }
                                        }
                                    }
                        }


                        break;

                    case AniType.idle:
                        t = (Find.TickManager.TicksGame + IdTick * 13) % 800;
                        f = 4.5f;
                        r = Core.Rot90(rot);
                        if (!Core.Ani(ref t, 500, ref oa, 0f, 0f, -1f, ref op, r))
                            if (!Core.Ani(ref t, 25, ref oa, 0f, f, -1f, ref op, r))
                                if (!Core.Ani(ref t, 50, ref oa, f, -f, -1f, ref op, r))
                                    if (!Core.Ani(ref t, 50, ref oa, -f, f, -1f, ref op, r))
                                        if (!Core.Ani(ref t, 50, ref oa, f, -f, -1f, ref op, r))
                                            if (!Core.Ani(ref t, 50, ref oa, -f, f, -1f, ref op, r))
                                                if (!Core.Ani(ref t, 50, ref oa, f, -f, -1f, ref op, r))
                                                    Core.Ani(ref t, 25, ref oa, -f, 0f, -1f, ref op, r);
                        break;

                    case AniType.smash:
                        t = (Find.TickManager.TicksGame + IdTick) % 133;

                        if (!Core.Ani(ref t, 70, ref oa, 0f, -20f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                        {
                            if (!Core.Ani(ref t, 3, ref oa, -20f, 10f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot, Core.tweenType.line))
                                if (!Core.Ani(ref t, 20, ref oa, 10f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot))
                                    Core.Ani(ref t, 40, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), rot);
                        }

                        break;

                    case AniType.doSomeThing:
                        total = 121;
                        t2 = (Find.TickManager.TicksGame + IdTick) % (total * 2);
                        t = t2 % total;
                        r = Core.Rot90(rot);
                        tr = rot;
                        if (!Core.Ani(ref t, 20, ref nextUpdate))
                            if (!Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r))
                                if (!Core.Ani(ref t, 20, ref oa, 10f, 10f, -1f, ref op, r))
                                    if (!Core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r))
                                        if (!Core.Ani(ref t, 20, ref oa, -10f, -10f, -1f, ref op, r))
                                        {
                                            if (!Core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                                            {
                                                //tr = t2 >= total ? core.Rot90(rot) : core.Rot90b(rot);
                                                if (!Core.Ani(ref t, 15, ref oa, 0f, 0f, -1f, ref op, rot)) // 85
                                                {
                                                    //tr = rot;
                                                    if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, rot)) // 105
                                                        if (!Core.Ani(ref t, 5, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.05f), rot))
                                                            Core.Ani(ref t, 6, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.05f), new Vector3(0f, 0f, 0f), rot);
                                                }
                                            }
                                        }

                        rot = tr;
                        break;


                    case AniType.social:
                        total = 221;
                        t2 = (Find.TickManager.TicksGame + IdTick) % (total * 2);
                        t = t2 % total;
                        r = Core.Rot90(rot);
                        tr = rot;
                        if (!Core.Ani(ref t, 20, ref nextUpdate))
                            if (!Core.Ani(ref t, 5, ref oa, 0f, 10f, -1f, ref op, r))
                                if (!Core.Ani(ref t, 20, ref oa, 10f, 10f, -1f, ref op, r))
                                    if (!Core.Ani(ref t, 5, ref oa, 10f, -10f, -1f, ref op, r))
                                        if (!Core.Ani(ref t, 20, ref oa, -10f, -10f, -1f, ref op, r))
                                        {
                                            if (!Core.Ani(ref t, 5, ref oa, -10f, 0f, -1f, ref op, r))
                                            {
                                                tr = t2 >= total ? Core.Rot90(rot) : Core.Rot90b(rot);
                                                if (!Core.Ani(ref t, 15, ref oa, 0f, 0f, -1f, ref op, rot)) // 85
                                                {
                                                    tr = rot;
                                                    if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, rot)) // 105
                                                        if (!Core.Ani(ref t, 5, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0.05f), rot))
                                                            if (!Core.Ani(ref t, 6, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, 0.05f), new Vector3(0f, 0f, 0f), rot))

                                                                if (!Core.Ani(ref t, 35, ref oa, 0f, 0f, -1f, ref op, rot))
                                                                    if (!Core.Ani(ref t, 10, ref oa, 0f, 10f, -1f, ref op, rot))
                                                                        if (!Core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot))
                                                                            if (!Core.Ani(ref t, 10, ref oa, 0f, 10f, -1f, ref op, rot))
                                                                                if (!Core.Ani(ref t, 10, ref oa, 10f, 0f, -1f, ref op, rot))
                                                                                    Core.Ani(ref t, 25, ref oa, 0f, 0f, -1f, ref op, rot);
                                                }
                                            }
                                        }

                        rot = tr;
                        break;
                }
            }

            if (changed)
            {
                pdd.angleOffset = oa;
                pdd.fixedRot = rot.IsValid ? rot : null;
                op = new Vector3(op.x, 0f, op.z);
                pdd.posOffset = op;
                pos += op;
                pdd.nextUpdateTick = nextUpdate ?? Find.TickManager.TicksGame + Core.settings.updateFrequencyTicks;
            }
            else
            {
                pdd.Reset();
            }
        }


        public static void Ani1(Pawn pawn, ref Vector3 pos, Rot4 rot, PawnDrawData pdd)
        {
            try
            {
                float oa = 0f;
                Vector3 op = Vector3.zero;
                bool changed = false;
                rot = Rot4.Invalid;
                int? nextUpdate = null;


                if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns || Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
                {
                    // Ignored
                }
                else if (pawn.CurJob != null &&
                         (!pawn.RaceProps.IsMechanoid || Core.settings.mechanoidJobEnabled) &&
                         (!pawn.RaceProps.Animal || Core.settings.animalJobEnabled))
                {
                    changed = true;
                    pdd.jobName = pawn.CurJob.def.defName;

                    if (Core.settings.debugMode)
                        if (pawn.IsColonist)
                            Log.Message($"{pawn.NameShortColored} : {pawn.CurJob.def.defName}");


                    int idTick = pawn.thingIDNumber * 20;
                    pdd.forcedShowBody = false;

                    int seed;
                    int idTickMult;
                    int idTickDiv;
                    switch (pawn.CurJob.def.defName)
                    {
                        case "Lovin": // 사랑나누기
                        case "VSIE_OneStandLovin":
                            if (!Core.settings.lovinEnabled) break;
                            Building_Bed bed = pawn.CurrentBed();
                            if (bed == null) break;
                            var t = (Find.TickManager.TicksGame + idTick % 30) % 360;
                            const float f = 0.03f;
                            if (pawn.RaceProps.Humanlike)
                            {
                                rot = Core.getRot(pawn.CurJob.targetA.Pawn.DrawPos - pawn.DrawPos, bed.Rotation);

                                if (t <= 160)
                                {
                                    if (!Core.Ani(ref t, 20, ref oa, 0f, 5f, -1f, ref op, Vector3.zero, new Vector3(0.05f, 0f, 0.05f), rot, Core.tweenType.sin, bed.Rotation))
                                        if (!Core.Ani(ref t, 50, ref oa, 5f, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0.05f), new Vector3(0.05f, 0f, 0.1f), rot, Core.tweenType.sin,
                                                bed.Rotation))
                                            if (!Core.Ani(ref t, 50, ref oa, 10, 10f, -1f, ref op, new Vector3(0.05f, 0f, 0.1f), new Vector3(0.05f, 0f, 0.1f), rot, Core.tweenType.sin,
                                                    bed.Rotation))
                                                Core.Ani(ref t, 40, ref oa, 10f, 0f, -1f, ref op, new Vector3(0.05f, 0f, 0.1f), Vector3.zero, rot, Core.tweenType.sin, bed.Rotation);
                                }
                                else
                                {
                                    t = (Find.TickManager.TicksGame + idTick) % 40;
                                    if (!Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, f), new Vector3(0f, 0f, -f), rot, Core.tweenType.sin, bed.Rotation))
                                        Core.Ani(ref t, 20, ref oa, 0f, 0f, -1f, ref op, new Vector3(0f, 0f, -f), new Vector3(0f, 0f, f), rot, Core.tweenType.sin, bed.Rotation);
                                }
                            }


                            break;

                        case "LayDown": // 잠자기
                            if (!Core.settings.sleepEnabled) break;
                            if (!(pawn.jobs?.curDriver?.asleep ?? false)) break;
#if BIOTECH
                            if (pawn.DevelopmentalStage.Newborn() || pawn.DevelopmentalStage.Baby()) break;
#endif

                            idTickMult = idTick * 5;
                            idTickDiv = (Find.TickManager.TicksGame + idTickMult) / 2500;
                            seed = idTickDiv + idTickMult;
                            nextUpdate = (idTickDiv + 1) * 2500 - idTickMult;

                            rot = Rand.RangeSeeded(0, 4, seed) switch
                            {
                                0 => Rot4.East,
                                1 => Rot4.West,
                                2 => Rot4.South,
                                3 => Rot4.North,
                                _ => rot
                            };

                            switch (Rand.RangeSeeded(0, 3, seed + 100))
                            {
                                case 0:
                                    op = new Vector3(Rand.RangeSeeded(-0.1f, 0.1f, seed + 50), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 100));
                                    break;
                                case 1:
                                    op = new Vector3(Rand.RangeSeeded(-0.2f, 0.2f, seed + 150), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 200));
                                    break;
                                case 2:
                                    op = new Vector3(Rand.RangeSeeded(-0.3f, 0.3f, seed + 250), 0f, Rand.RangeSeeded(-0.2f, 0.2f, seed + 300));
                                    pdd.forcedShowBody = true;
                                    break;
                            }

                            oa = Rand.RangeSeeded(0, 3, seed + 200) switch
                            {
                                2 => Rand.RangeSeeded(-45f, 45f, seed),
                                _ => Rand.RangeSeeded(-15f, 15f, seed),
                            };

                            break;

                        case "Skygaze":
                        case "VSIE_Skygaze":
                            seed = pawn.CurJob.loadID + idTick * 5;

                            nextUpdate = int.MaxValue;
                            op = Rand.RangeSeeded(0, 3, seed + 100) switch
                            {
                                0 => new Vector3(Rand.RangeSeeded(-0.1f, 0.1f, seed + 50), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 100)),
                                1 => new Vector3(Rand.RangeSeeded(-0.2f, 0.2f, seed + 150), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 200)),
                                _ => new Vector3(Rand.RangeSeeded(-0.3f, 0.3f, seed + 250), 0f, Rand.RangeSeeded(-0.2f, 0.2f, seed + 300)),
                            };

                            oa = Rand.RangeSeeded(0f, 360f, seed + 200);

                            break;

                        // // Dubs Bad Hygiene
                        // case "takeBath":
                        //     rot = Rot4.Invalid;
                        //     break;
                        case "UseHotTub":
                            nextUpdate = int.MaxValue;
                            if (Rand.ChanceSeeded(0.5f, pawn.CurJob.loadID + idTick))
                            {
                                op = new Vector3(0, 0f, 0.5f);
                                oa = 180f;
                            }

                            break;
                        case "VFEV_HypothermiaResponse":
                            if (!Core.settings.sleepEnabled) break;

                            idTickMult = idTick * 5;
                            idTickDiv = (Find.TickManager.TicksGame + idTickMult) / 2500;
                            seed = idTickDiv + idTickMult;
                            nextUpdate = (idTickDiv + 1) * 2500 - idTickMult;

                            rot = Rand.RangeSeeded(0, 4, seed) switch
                            {
                                0 => Rot4.East,
                                1 => Rot4.West,
                                2 => Rot4.South,
                                3 => Rot4.North,
                                _ => rot
                            };

                            if (Rand.ChanceSeeded(0.5f, seed + 100))
                                op = new Vector3(Rand.RangeSeeded(-0.1f, 0.1f, seed + 50), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 100));
                            else
                                op = new Vector3(Rand.RangeSeeded(-0.2f, 0.2f, seed + 150), 0f, Rand.RangeSeeded(-0.1f, 0.1f, seed + 200));

                            break;

                        default:
                            nextUpdate = int.MaxValue; // Update on a new job
                            break;
                    }
                }

                if (changed)
                {
                    pdd.angleOffset = oa;
                    pdd.fixedRot = rot.IsValid ? rot : null;
                    op = new Vector3(op.x, 0f, op.z);
                    pdd.posOffset = op;
                    pos += op;
                    pdd.nextUpdateTick = nextUpdate ?? Find.TickManager.TicksGame + Core.settings.updateFrequencyTicks;
                }
                else
                {
                    pdd.Reset();
                }
            }
            catch
            {
                DataUtility.GetData(pawn).Reset();
            }
        }
    }


    // ---------------------------------------------------
    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
    public class Patch_PawnRenderer_RenderPawnAt
    {
        [HarmonyPriority(0)]
        [UsedImplicitly]
        public static void Prefix(PawnRenderer __instance, Pawn ___pawn, ref Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {
            var pdd = DataUtility.GetData(___pawn);
            Yayo.CheckAni(___pawn, ref drawLoc, rotOverride ?? ___pawn.Rotation, pdd);
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "DrawDynamicParts")]
    public class Patch_PawnRenderer_DrawDynamicParts
    {
        [HarmonyPriority(0)]
        [UsedImplicitly]
        public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, ref Rot4 pawnRotation, PawnRenderFlags flags, Pawn ___pawn)
        {
            if (___pawn.GetPosture() == PawnPosture.Standing)
            {
                var pdd = DataUtility.GetData(___pawn);
                angle += pdd.angleOffset;
                pawnRotation = pdd.fixedRot ?? pawnRotation;
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnInternal")]
    public class Patch_PawnRenderer_RenderPawnInternal
    {
        public static bool skipPatch = false;

        [HarmonyPriority(0)]
        [HarmonyBefore("rimworld.Nals.FacialAnimation")]
        [UsedImplicitly]
        public static void Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, bool renderBody, ref Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, Pawn ___pawn)
        {
            if (skipPatch)
            {
                skipPatch = false;
                return;
            }

            if (___pawn.GetPosture() == PawnPosture.Standing)
            {
                var pdd = DataUtility.GetData(___pawn);
                angle += pdd.angleOffset;
                bodyFacing = pdd.fixedRot ?? bodyFacing;
            }
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "RenderCache")]
    public class Patch_PawnRenderer_RenderCache
    {
        [HarmonyPriority(0)]
        [UsedImplicitly]
        public static bool Prefix(PawnRenderer __instance, Pawn ___pawn, Dictionary<Apparel, (Color, bool)> ___tmpOriginalColors, Rot4 rotation, ref float angle, Vector3 positionOffset,
            bool renderHead, bool renderBody, bool portrait, bool renderHeadgear, bool renderClothes, Dictionary<Apparel, Color> overrideApparelColor = null, Color? overrideHairColor = null,
            bool stylingStation = false)
        {
            Vector3 zero = Vector3.zero;
            //PawnRenderFlags pawnRenderFlags = GetDefaultRenderFlags(pawn);
            PawnRenderFlags pawnRenderFlags = __instance.GetDefaultRenderFlags(___pawn);
            if (portrait)
            {
                pawnRenderFlags |= PawnRenderFlags.Portrait;
            }

            pawnRenderFlags |= PawnRenderFlags.Cache;
            pawnRenderFlags |= PawnRenderFlags.DrawNow;
            if (!renderHead)
            {
                pawnRenderFlags |= PawnRenderFlags.HeadStump;
            }

            if (renderHeadgear)
            {
                pawnRenderFlags |= PawnRenderFlags.Headgear;
            }

            if (renderClothes)
            {
                pawnRenderFlags |= PawnRenderFlags.Clothes;
            }

            if (stylingStation)
            {
                pawnRenderFlags |= PawnRenderFlags.StylingStation;
            }

            ___tmpOriginalColors.Clear();
            try
            {
                if (overrideApparelColor != null)
                {
                    foreach (var (key, value) in overrideApparelColor)
                    {
                        CompColorable compColorable = key.TryGetComp<CompColorable>();
                        if (compColorable != null)
                        {
                            ___tmpOriginalColors.Add(key, (compColorable.Color, compColorable.Active));
                            key.SetColor(value);
                        }
                    }
                }

                Color hairColor = Color.white;
                if (___pawn.story != null)
                {
                    hairColor = ___pawn.story.hairColor;
                    if (overrideHairColor.HasValue)
                    {
                        ___pawn.story.hairColor = overrideHairColor.Value;
                        ___pawn.Drawer.renderer.graphics.CalculateHairMats();
                    }
                }

                //RenderPawnInternal(zero + positionOffset, angle, renderBody, rotation, ___CurRotDrawMode, pawnRenderFlags);
                Patch_PawnRenderer_RenderPawnInternal.skipPatch = true;
                RotDrawMode curRotDrawMode = __instance.CurRotDrawMode;
                __instance.RenderPawnInternal(zero + positionOffset, angle, renderBody, rotation, curRotDrawMode, pawnRenderFlags);
                foreach (KeyValuePair<Apparel, (Color, bool)> tmpOriginalColor in ___tmpOriginalColors)
                {
                    if (!tmpOriginalColor.Value.Item2)
                    {
                        tmpOriginalColor.Key.TryGetComp<CompColorable>().Disable();
                    }
                    else
                    {
                        tmpOriginalColor.Key.SetColor(tmpOriginalColor.Value.Item1);
                    }
                }

                if (___pawn.story != null && overrideHairColor.HasValue)
                {
                    ___pawn.story.hairColor = hairColor;
                    ___pawn.Drawer.renderer.graphics.CalculateHairMats();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error rendering pawn portrait: " + ex.Message);
            }
            finally
            {
                ___tmpOriginalColors.Clear();
            }

            return false;
        }
    }


    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "GetBodyPos")]
    internal class Patch_PawnRenderer_GetBodyPos
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Postfix(PawnRenderer __instance, ref Vector3 __result, Vector3 drawLoc, ref bool showBody, Pawn ___pawn)
        {
            var pdd = DataUtility.GetData(___pawn);
            __result += pdd.posOffset;
            if (pdd.forcedShowBody) showBody = true;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "BodyAngle")]
    internal class Patch_PawnRenderer_BodyAngle
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Postfix(PawnRenderer __instance, ref float __result, Pawn ___pawn)
        {
            var pdd = DataUtility.GetData(___pawn);
            __result += pdd.angleOffset;
        }
    }


    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "LayingFacing")]
    internal class Patch_PawnRenderer_LayingFacing
    {
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void Postfix(PawnRenderer __instance, ref Rot4 __result, Pawn ___pawn)
        {
            var pdd = DataUtility.GetData(___pawn);
            __result = pdd.fixedRot ?? __result;
        }
    }


    /*
	// ---------------------------------------------------

	[HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
    public class Patch_PawnRenderer_RenderPawnAt
    {
        static Pawn pawn;

        [HarmonyPriority(0)]
        public static bool Prefix(PawnRenderer __instance, Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
        {

            pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();

            if (!__instance.graphics.AllResolved)
            {
                __instance.graphics.ResolveAllGraphics();
            }

            Rot4 rot = rotOverride ?? pawn.Rotation;
            PawnRenderFlags pawnRenderFlags = (PawnRenderFlags)AccessTools.Method(typeof(PawnRenderer), "GetDefaultRenderFlags").Invoke(__instance, new object[] { pawn });
            if (neverAimWeapon)
            {
                pawnRenderFlags |= PawnRenderFlags.NeverAimWeapon;
            }

            RotDrawMode curRotDrawMode = Traverse.Create(__instance).Property("CurRotDrawMode").GetValue<RotDrawMode>();
            bool flag = pawn.RaceProps.Humanlike && curRotDrawMode != RotDrawMode.Dessicated && !pawn.IsInvisible();
            PawnTextureAtlasFrameSet frameSet = null;
            if (flag && !GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frameSet, out var _))
            {
                flag = false;
            }

            int IdTick = pawn.thingIDNumber * 100;

            if (pawn.GetPosture() == PawnPosture.Standing)
            {
                // yayo
                float angle = 0f;
                yayo.ani0(pawn, ref drawLoc, ref rot, out angle);


                if (flag)
                {
                    Material original = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
                    original = (Material)AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] { original, pawn, false });

					// need fix
					GenDraw.DrawMeshNowOrLater((Mesh)AccessTools.Method(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame").Invoke(__instance, new object[] { frameSet, rot, PawnDrawMode.BodyAndHead }), drawLoc, Quaternion.AngleAxis(angle, Vector3.up), original, drawNow: false);

					// need fix
					AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts").Invoke(__instance, new object[] { drawLoc, angle, rot, pawnRenderFlags });
                }
                else
                {
					// need fix
					AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { drawLoc, angle, true, rot, curRotDrawMode, pawnRenderFlags });
                }
				AccessTools.Method(typeof(PawnRenderer), "DrawCarriedThing").Invoke(__instance, new object[] { drawLoc });
                if (!pawnRenderFlags.FlagSet(PawnRenderFlags.Invisible))
                {
					AccessTools.Method(typeof(PawnRenderer), "DrawInvisibleShadow").Invoke(__instance, new object[] { drawLoc });
                }
            }
            else
            {
                bool showBody = true;
                Vector3 bodyPos = (Vector3)AccessTools.Method(typeof(PawnRenderer), "GetBodyPos").Invoke(__instance, new object[] { drawLoc, showBody });
                float angle = __instance.BodyAngle();
                Rot4 rot2 = __instance.LayingFacing();

				// yayo
				yayo.ani1(pawn, ref bodyPos, ref rot2, ref angle, ref showBody);


                if (flag)
                {
                    Material original2 = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
                    original2 = (Material)AccessTools.Method(typeof(PawnRenderer), "OverrideMaterialIfNeeded").Invoke(__instance, new object[] { original2, pawn, false });

					// need fix
					GenDraw.DrawMeshNowOrLater((Mesh)AccessTools.Method(typeof(PawnRenderer), "GetBlitMeshUpdatedFrame").Invoke(__instance, new object[] { frameSet, rot2, (!showBody) ? PawnDrawMode.HeadOnly : PawnDrawMode.BodyAndHead }), bodyPos, Quaternion.AngleAxis(angle, Vector3.up), original2, drawNow: false);
					// need fix
					AccessTools.Method(typeof(PawnRenderer), "DrawDynamicParts").Invoke(__instance, new object[] { bodyPos, angle, rot, pawnRenderFlags });
                }
                else
                {
					// need fix
					AccessTools.Method(typeof(PawnRenderer), "RenderPawnInternal").Invoke(__instance, new object[] { bodyPos, angle, showBody, rot2, curRotDrawMode, pawnRenderFlags });
                }
            }
            if (pawn.Spawned && !pawn.Dead)
            {
                pawn.stances.StanceTrackerDraw();
                pawn.pather.PatherDraw();
                pawn.roping.RopingDraw();
            }

            AccessTools.Method(typeof(PawnRenderer), "DrawDebug").Invoke(__instance, new object[] { });
            return false;
        }
    }
	*/
}