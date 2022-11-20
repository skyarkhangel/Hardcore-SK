using System;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoAni
{
    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public class Patch_DrawEquipment
    {
        [HarmonyPriority(0)]
        [HarmonyPrefix]
        [UsedImplicitly]
        private static bool Prefix(PawnRenderer __instance, Vector3 rootLoc)
        {
            if (!Core.settings.combatEnabled)
                return true;

            if (Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
                return true;

            Pawn pawn = __instance.pawn;
            if (pawn.Dead || !pawn.Spawned)
                return false;

            if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns)
                return true;

            if (pawn.RaceProps.IsMechanoid && !Core.settings.mechanoidCombatEnabled)
                return true;

            if (pawn.RaceProps.Animal && !Core.settings.animalCombatEnabled)
                return true;

            if (pawn.equipment?.Primary == null)
                return false;

            if (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon)
                return false;

            // duelWeld
            ThingWithComps offHandEquip = null;
            if (Core.usingDualWield)
            {
                if (pawn.equipment.TryGetOffHandEquipment(out ThingWithComps result))
                {
                    offHandEquip = result;
                }
            }

            // 주무기
            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
            PawnRenderer_Override.AnimateEquip(__instance, pawn, rootLoc, pawn.equipment.Primary, stance_Busy, new Vector3(0f, 0f, 0.0005f));

            // 보조무기
            if (offHandEquip != null)
            {
                Stance_Busy offHandStance = null;
                if (pawn.GetStancesOffHand() != null)
                {
                    offHandStance = pawn.GetStancesOffHand().curStance as Stance_Busy;
                }

                PawnRenderer_Override.AnimateEquip(__instance, pawn, rootLoc, offHandEquip, offHandStance, new Vector3(0.1f, 0.1f, 0f), true);
            }

            return false;
        }
    }


    [HotSwappable]
    public static class PawnRenderer_Override
    {
        public static void AnimateEquip(PawnRenderer __instance, Pawn pawn, Vector3 rootLoc, ThingWithComps thing, Stance_Busy stanceBusy, Vector3 offset, bool isSub = false)
        {
            Vector3 rootLoc2 = rootLoc;

            bool isMechanoid = pawn.RaceProps.IsMechanoid;

            offset.z += (pawn.Rotation == Rot4.North) ? (-0.00289575267f) : 0.03474903f;

            // 설정과 무기 무게에 따른 회전 애니메이션 사용 여부
            bool useTwirl = Core.settings.combatTwirlEnabled && !isMechanoid && thing.def.BaseMass < 5f;

            if (stanceBusy != null && !stanceBusy.neverAimWeapon && stanceBusy.focusTarg.IsValid)
            {
                if (thing.def.IsRangedWeapon && !stanceBusy.verb.IsMeleeAttack)
                {
                    // 원거리용

                    //Log.Message((pawn.LastAttackTargetTick + thing.thingIDNumber).ToString());
                    int ticksToNextBurstShot = stanceBusy.verb.ticksToNextBurstShot;
                    int atkType = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 5; // 랜덤 공격타입 결정
                    // Stance_Cooldown Stance_Cooldown = pawn.stances.curStance as Stance_Cooldown;
                    Stance_Warmup Stance_Warmup = pawn.stances.curStance as Stance_Warmup;

                    if (ticksToNextBurstShot > 10)
                    {
                        ticksToNextBurstShot = 10;
                    }

                    //atkType = 2; // 공격타입 테스트

                    float ani_burst = ticksToNextBurstShot;
                    float ani_cool = stanceBusy.ticksLeft;

                    float ani = 0f;
                    if (!isMechanoid)
                        ani = Mathf.Max(ani_cool, 25f) * 0.001f;

                    if (ticksToNextBurstShot > 0)
                        ani = ani_burst * 0.02f;

                    float addAngle = 0f;
                    float addX = offset.x;
                    float addY = offset.y;


                    // 준비동작 애니메이션
                    if (!isMechanoid)
                    {
                        float wiggleSlow;
                        if (!isSub)
                            wiggleSlow = Mathf.Sin(ani_cool * 0.035f) * 0.05f;
                        else
                            wiggleSlow = Mathf.Sin(ani_cool * 0.035f + 0.5f) * 0.05f;

                        switch (atkType)
                        {
                            case 0:
                                // 회전
                                if (useTwirl)
                                {
                                    /*
                                    if (stance_Busy.ticksLeft < 35 && stance_Busy.ticksLeft > 10 && ticksToNextBurstShot == 0 && Stance_Warmup == null)
                                    {
                                        addAngle += ani_cool * 50f + 180f;
                                    }
                                    else if (stance_Busy.ticksLeft > 1)
                                    {
                                        addY += wiggle_slow;
                                    }
                                    */
                                }
                                else
                                {
                                    if (stanceBusy.ticksLeft > 1)
                                    {
                                        addY += wiggleSlow;
                                    }
                                }

                                break;
                            case 1:
                                // 재장전
                                if (ticksToNextBurstShot == 0)
                                {
                                    switch (stanceBusy.ticksLeft)
                                    {
                                        case > 78:
                                            break;
                                        case > 48 when Stance_Warmup == null:
                                        {
                                            float wiggle = Mathf.Sin(ani_cool * 0.1f) * 0.05f;
                                            addX += wiggle - 0.2f;
                                            addY += wiggle + 0.2f;
                                            addAngle += wiggle + 30f + ani_cool * 0.5f;
                                            break;
                                        }
                                        case > 40 when Stance_Warmup == null:
                                        {
                                            float wiggle = Mathf.Sin(ani_cool * 0.1f) * 0.05f;
                                            float wiggle_fast = Mathf.Sin(ani_cool) * 0.05f;
                                            addX += wiggle_fast + 0.05f;
                                            addY += wiggle - 0.05f;
                                            addAngle += wiggle_fast * 100f - 15f;
                                            break;
                                        }
                                        case > 1:
                                            addY += wiggleSlow;
                                            break;
                                    }
                                }

                                break;
                            default:
                                if (stanceBusy.ticksLeft > 1)
                                {
                                    addY += wiggleSlow;
                                }

                                break;
                        }
                    }

                    Vector3 a = stanceBusy.focusTarg.Thing?.DrawPos ?? stanceBusy.focusTarg.Cell.ToVector3Shifted();
                    float num = 0f;
                    if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        num = (a - pawn.DrawPos).AngleFlat();

                    Vector3 drawLoc;
                    if (pawn.Rotation == Rot4.West)
                        drawLoc = rootLoc2 + new Vector3(addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    else if (pawn.Rotation != Rot4.Invalid)
                        drawLoc = rootLoc2 + new Vector3(-addY, offset.z, 0.4f + addX - ani).RotatedBy(num);
                    else
                        drawLoc = Vector3.zero;


                    //drawLoc.y += 0.03787879f;

                    // 반동 계수
                    const float reboundFactor = 70f;

                    if (pawn.Rotation == Rot4.West)
                        __instance.DrawEquipmentAiming(thing, drawLoc, num + ani * reboundFactor + addAngle);
                    else if (pawn.Rotation != Rot4.Invalid)
                        __instance.DrawEquipmentAiming(thing, drawLoc, num - ani * reboundFactor - addAngle);

                    return;
                }
                else
                {
                    // 근접용

                    //Log.Message("A");
                    int atkType = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 3; // 랜덤 공격타입 결정

                    //Log.Message("B");
                    //atkType = 1; // 공격 타입 테스트

                    // 공격 타입에 따른 각도
                    var addAngle = atkType switch
                    {
                        1 =>
                            // 내려찍기
                            25f,
                        2 =>
                            // 머리찌르기
                            -25f,
                        _ => 0f
                    };
                    //Log.Message("C");
                    // 원거리 무기일경우 각도보정
                    if (thing.def.IsRangedWeapon)
                        addAngle -= 35f;

                    //Log.Message("D");

                    const float readyZ = 0.2f;


                    //Log.Message("E");
                    if (stanceBusy.ticksLeft > 15)
                    {
                        //Log.Message("F");
                        // 애니메이션
                        Vector3 a = stanceBusy.focusTarg.Thing?.DrawPos ?? stanceBusy.focusTarg.Cell.ToVector3Shifted();

                        float num = 0f;
                        if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        {
                            num = (a - pawn.DrawPos).AngleFlat();
                        }

                        float ani = Mathf.Min(stanceBusy.ticksLeft, 60f);
                        float ani2 = ani * 0.0075f; // 0.45f -> 0f
                        float addZ = offset.x;
                        float addX = offset.y;

                        switch (atkType)
                        {
                            default:
                                // 평범한 공격
                                addZ += readyZ + 0.05f + ani2; // 높을 수록 무기를 적쪽으로 내밀음
                                addX += 0.45f - 0.5f - ani2 * 0.1f; // 높을수록 무기를 아래까지 내려침
                                break;
                            case 1:
                                // 내려찍기
                                addZ += readyZ + 0.05f + ani2; // 높을 수록 무기를 적쪽으로 내밀음
                                addX += 0.45f - 0.35f + ani2 * 0.5f; // 높을수록 무기를 아래까지 내려침, 애니메이션 반대방향
                                ani = 30f + ani * 0.5f; // 각도 고정값 + 각도 변화량
                                break;
                            case 2:
                                // 머리찌르기
                                addZ += readyZ + 0.05f + ani2; // 높을 수록 무기를 적쪽으로 내밀음
                                addX += 0.45f - 0.35f - ani2; // 높을수록 무기를 아래까지 내려침
                                break;
                        }

                        // 회전 애니메이션
                        // if (useTwirl && pawn.LastAttackTargetTick % 5 == 0 && stanceBusy.ticksLeft <= 25)
                        // {
                        //     //addAngle += ani2 * 5000f;
                        // }

                        // 캐릭터 방향에 따라 적용
                        if (pawn.Rotation == Rot4.West)
                        {
                            Vector3 drawLoc = rootLoc2 + new Vector3(-addX, offset.z, addZ).RotatedBy(num);
                            //drawLoc.y += 0.03787879f;
                            num -= addAngle;

                            __instance.DrawEquipmentAiming(thing, drawLoc, num - ani);
                        }
                        else if (pawn.Rotation == Rot4.East)
                        {
                            Vector3 drawLoc = rootLoc2 + new Vector3(addX, offset.z, addZ).RotatedBy(num);
                            //drawLoc.y += 0.03787879f;
                            num += addAngle;

                            __instance.DrawEquipmentAiming(thing, drawLoc, num + ani);
                        }
                        else if (pawn.Rotation != Rot4.Invalid)
                        {
                            Vector3 drawLoc = rootLoc2 + new Vector3(-addX, offset.z, addZ).RotatedBy(num);
                            //drawLoc.y += 0.03787879f;
                            num += addAngle;

                            __instance.DrawEquipmentAiming(thing, drawLoc, num + ani);
                        }
                    }
                    else
                    {
                        Vector3 a = stanceBusy.focusTarg.Thing?.DrawPos ?? stanceBusy.focusTarg.Cell.ToVector3Shifted();

                        float num = 0f;
                        if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                        {
                            num = (a - pawn.DrawPos).AngleFlat();
                        }

                        Vector3 drawLoc = rootLoc2 + new Vector3(0f, offset.z, readyZ).RotatedBy(num);
                        //drawLoc.y += 0.03787879f;

                        __instance.DrawEquipmentAiming(thing, drawLoc, num);
                    }

                    return;
                }
            }

            //Log.Message("11");
            // 대기
            if ((pawn.carryTracker?.CarriedThing == null) &&
                (pawn.Drafted || (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon) || (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)))
            {
                int tick = Mathf.Abs(pawn.HashOffsetTicks() % 1000000000);
                tick %= 100000000;
                tick %= 10000000;
                tick %= 1000000;

                tick %= 100000;
                tick %= 10000;
                tick %= 1000;
                float wiggle;
                if (!isSub)
                    wiggle = Mathf.Sin(tick * 0.05f);
                else
                    wiggle = Mathf.Sin(tick * 0.05f + 0.5f);

                float aniAngle = -5f;
                float addAngle = 0f;

                if (useTwirl)
                {
                    if (!isSub)
                    {
                        if (tick is < 80 and >= 40)
                        {
                            addAngle += tick * 36f;
                            rootLoc2 += new Vector3(-0.2f, 0f, 0.1f);
                        }
                    }
                    else
                    {
                        if (tick < 40)
                        {
                            addAngle += (tick - 40) * -36f;
                            rootLoc2 += new Vector3(0.2f, 0f, 0.1f);
                        }
                    }
                }

                if (pawn.Rotation == Rot4.South)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 143f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 350f - 143f;
                        aniAngle *= -1f;
                    }

                    //drawLoc2.y += 0.03787879f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }

                if (pawn.Rotation == Rot4.North)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.11f + wiggle * 0.05f);
                        angle = 143f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(0f, offset.z, -0.11f + wiggle * 0.05f);
                        angle = 350f - 143f;
                        aniAngle *= -1f;
                    }

                    //drawLoc3.y += 0f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }

                if (pawn.Rotation == Rot4.East)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 143f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 350f - 143f;
                        aniAngle *= -1f;
                    }

                    //drawLoc4.y += 0.03787879f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }

                if (pawn.Rotation == Rot4.West)
                {
                    Vector3 drawLoc;
                    float angle;
                    if (!isSub)
                    {
                        drawLoc = rootLoc2 + new Vector3(-0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 217f;
                    }
                    else
                    {
                        drawLoc = rootLoc2 + new Vector3(-0.2f, offset.z, -0.22f + wiggle * 0.05f);
                        angle = 350f - 217f;
                        aniAngle *= -1f;
                    }

                    //drawLoc5.y += 0.03787879f;
                    __instance.DrawEquipmentAiming(thing, drawLoc, addAngle + angle + wiggle * aniAngle);
                    return;
                }
            }

            return;
        }
    }

    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
    internal static class patch_DrawEquipmentAiming
    {
        private static Type CompOversizedType;
        private static Type CompOversizedPropsType;
        private static FastInvokeHandler OffsetFromRotationMethod;
        private static FastInvokeHandler NonCombatAngleAdjustmentMethod;
        private static FastInvokeHandler AngleOffsetAtPeaceMethod;
        private static AccessTools.FieldRef<CompProperties, bool> IsDualField;
        private static AccessTools.FieldRef<CompProperties, bool> VerticalFlipNorth;

        [UsedImplicitly]
        public static bool Prepare()
        {
            CompOversizedType = AccessTools.TypeByName("CompOversizedWeapon.CompOversizedWeapon");

            if (CompOversizedType != null)
            {
                CompOversizedPropsType = AccessTools.TypeByName("CompOversizedWeapon.CompProperties_OversizedWeapon");

                if (CompOversizedPropsType != null)
                {
                    // For whatever reason, those methods and fields may not always exist
                    var method = AccessTools.Method("CompOversizedWeapon.HarmonyCompOversizedWeapon:OffsetFromRotation");
                    if (method != null)
                        OffsetFromRotationMethod = MethodInvoker.GetHandler(method);

                    method = AccessTools.Method("CompOversizedWeapon.HarmonyCompOversizedWeapon:NonCombatAngleAdjustment");
                    if (method != null)
                        NonCombatAngleAdjustmentMethod = MethodInvoker.GetHandler(method);

                    method = AccessTools.Method("CompOversizedWeapon.HarmonyCompOversizedWeapon:AngleOffsetAtPeace");
                    if (method != null)
                        AngleOffsetAtPeaceMethod = MethodInvoker.GetHandler(method);

                    var field = AccessTools.Field(CompOversizedPropsType, "isDualWeapon");
                    if (field != null)
                        IsDualField = AccessTools.FieldRefAccess<CompProperties, bool>(field);

                    field = AccessTools.Field(CompOversizedPropsType, "verticalFlipNorth");
                    if (field != null)
                        VerticalFlipNorth = AccessTools.FieldRefAccess<CompProperties, bool>(field);
                }
            }

            return true;
        }

        [HarmonyPriority(9999)]
        [HarmonyPrefix]
        [UsedImplicitly]
        private static bool Prefix(PawnRenderer __instance, Thing eq, Vector3 drawLoc, float aimAngle)
        {
            if (!Core.settings.combatEnabled)
                return true;

            if (Find.CameraDriver.CurrentZoom > Core.settings.maximumZoomLevel)
                return true;

            var pawn = __instance.pawn;

            if (pawn.Faction != Faction.OfPlayer && Core.settings.onlyPlayerPawns)
                return true;

            if (pawn.RaceProps.IsMechanoid && !Core.settings.mechanoidCombatEnabled)
                return true;

            if (pawn.RaceProps.Animal && !Core.settings.animalCombatEnabled)
                return true;

            var pawnRotation = pawn.Rotation;

            var num = aimAngle - 90f;
            Mesh mesh;

            var isMeleeAtk = false;
            var flip = false;

            var stance_Busy = pawn.stances.curStance as Stance_Busy;

            var flag = !(pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon);

            if (flag && stance_Busy is { neverAimWeapon: false, focusTarg: { IsValid: true } })
            {
                if (pawnRotation == Rot4.West)
                {
                    flip = true;
                }

                if (!pawn.equipment.Primary.def.IsRangedWeapon || stance_Busy.verb.IsMeleeAttack)
                {
                    // 근접공격
                    isMeleeAtk = true;
                }
            }

            if (isMeleeAtk)
            {
                if (flip)
                {
                    mesh = MeshPool.plane10Flip;
                    num -= 180f;
                    num -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
            }
            else
            {
                if (aimAngle is > 20f and < 160f)
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
                //else if ((aimAngle > 200f && aimAngle < 340f) || ignore)
                else if (aimAngle is > 200f and < 340f || flip)
                {
                    flip = true;
                    mesh = MeshPool.plane10Flip;
                    num -= 180f;
                    num -= eq.def.equippedAngleOffset;
                }
                else
                {
                    mesh = MeshPool.plane10;
                    num += eq.def.equippedAngleOffset;
                }
            }

            var isOversized = false;
            var isDual = false;
            var oversizedOffset = Vector3.zero;

            if (eq is ThingWithComps eqComps)
            {
                var compEquippable = eqComps.GetComp<CompEquippable>();
                if (compEquippable != null)
                {
                    EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
                    drawLoc += drawOffset;
                    num += angleOffset;
                }

                if (CompOversizedType != null)
                {
                    foreach (var comp in eqComps.comps)
                    {
                        if (CompOversizedType.IsInstanceOfType(comp))
                        {
                            isOversized = true;

                            if (CompOversizedPropsType.IsInstanceOfType(comp.props))
                            {
                                var isFighting = pawn.IsFighting();

                                if (IsDualField != null)
                                    isDual = IsDualField(comp.props);
                                if (OffsetFromRotationMethod != null)
                                    oversizedOffset = (Vector3)OffsetFromRotationMethod(null, pawnRotation, comp.props);
                                if (flip && AngleOffsetAtPeaceMethod != null)
                                    num += (float)AngleOffsetAtPeaceMethod(null, eq, isFighting, comp.props);
                                if (!isFighting)
                                {
                                    if (VerticalFlipNorth != null && pawnRotation == Rot4.North && VerticalFlipNorth(comp.props))
                                    {
                                        num += 180f;
                                    }

                                    if (NonCombatAngleAdjustmentMethod != null)
                                    {
                                        num += (float)NonCombatAngleAdjustmentMethod(null, pawnRotation, comp.props);
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }

            num %= 360f;

            // Material matSingle;
            //if (graphic_StackCount != null)
            //{
            //    matSingle = graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle;
            //}
            //else
            //{
            //    matSingle = eq.Graphic.MatSingle;
            //}
            //Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0);

            Vector3 size = new(eq.def.graphicData.drawSize.x, 1f, eq.def.graphicData.drawSize.y);
            var mat = eq.Graphic is Graphic_StackCount graphicStackCount
                ? graphicStackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq)
                : eq.Graphic.MatSingleFor(eq);

            if (isOversized)
            {
                var matrix4x = Matrix4x4.TRS(drawLoc + oversizedOffset, Quaternion.AngleAxis(num, Vector3.up), size);

                Graphics.DrawMesh(
                    matrix: matrix4x,
                    material: mat,
                    mesh: mesh,
                    layer: 0);

                if (isDual)
                {
                    oversizedOffset = new Vector3(-oversizedOffset.x, oversizedOffset.y, oversizedOffset.z);

                    if (pawn.Rotation == Rot4.North || pawn.Rotation == Rot4.South)
                    {
                        num += 135f;
                        num %= 360f;
                        mesh = flip
                            ? MeshPool.plane10Flip
                            : MeshPool.plane10;
                    }
                    else
                    {
                        oversizedOffset = new Vector3(oversizedOffset.x, oversizedOffset.y - 0.1f, oversizedOffset.z + 0.15f);
                    }

                    matrix4x.SetTRS(drawLoc + oversizedOffset, Quaternion.AngleAxis(num, Vector3.up), size);
                    Graphics.DrawMesh(
                        mesh: mesh,
                        matrix: matrix4x,
                        material: mat,
                        layer: 0);
                }
            }
            else
            {
                var matrix4x = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), size);

                Graphics.DrawMesh(
                    mesh: mesh,
                    matrix: matrix4x,
                    material: mat,
                    layer: 0);
            }

            return false;
        }
    }
}