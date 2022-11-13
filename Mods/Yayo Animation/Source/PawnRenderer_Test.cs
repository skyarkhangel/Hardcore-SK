//using System;
//using System.Collections.Generic;
//using RimWorld;
//using UnityEngine;
//using Verse.AI.Group;

//namespace Verse
//{
//    public class PawnRenderer_Test
//    {
//        private Pawn pawn;

//        public PawnGraphicSet graphics;

//        public PawnDownedWiggler wiggler;

//        private PawnHeadOverlays statusOverlays;

//        private PawnWoundDrawer woundOverlays;

//        private Graphic_Shadow shadowGraphic;

//        private const float CarriedThingDrawAngle = 16f;

//        private const float SubInterval = 0.00289575267f;

//        private const float YOffset_PrimaryEquipmentUnder = -0.00289575267f;

//        private const float YOffset_CarriedThingUnder = -0.00289575267f;

//        private const float YOffset_Behind = 0.00289575267f;

//        private const float YOffset_Utility_South = 0.00579150533f;

//        private const float YOffset_Body = 0.008687258f;

//        private const float YOffsetInterval_Clothes = 0.00289575267f;

//        private const float YOffset_Shell = 3f / 148f;

//        private const float YOffset_Head = 0.0231660213f;

//        private const float YOffset_OnHead = 0.0289575271f;

//        private const float YOffset_Utility = 0.0289575271f;

//        private const float YOffset_PostHead = 0.03185328f;

//        private const float YOffset_CarriedThing = 0.03474903f;

//        private const float YOffset_PrimaryEquipmentOver = 0.03474903f;

//        private const float YOffset_Status = 0.0376447849f;

//        private static Dictionary<Apparel, (Color, bool)> tmpOriginalColors = new Dictionary<Apparel, (Color, bool)>();

//        private RotDrawMode CurRotDrawMode
//        {
//            get
//            {
//                if (pawn.Dead && pawn.Corpse != null)
//                {
//                    return pawn.Corpse.CurRotDrawMode;
//                }
//                return RotDrawMode.Fresh;
//            }
//        }

//        public PawnWoundDrawer WoundOverlays => woundOverlays;

//        private PawnRenderFlags GetDefaultRenderFlags(Pawn pawn)
//        {
//            PawnRenderFlags pawnRenderFlags = PawnRenderFlags.None;
//            if (pawn.IsInvisible())
//            {
//                pawnRenderFlags |= PawnRenderFlags.Invisible;
//            }
//            if (!pawn.health.hediffSet.HasHead)
//            {
//                pawnRenderFlags |= PawnRenderFlags.HeadStump;
//            }
//            return pawnRenderFlags;
//        }

//        private Mesh GetBlitMeshUpdatedFrame(PawnTextureAtlasFrameSet frameSet, Rot4 rotation, PawnDrawMode drawMode)
//        {
//            int index = frameSet.GetIndex(rotation, drawMode);
//            if (frameSet.isDirty[index])
//            {
//                Find.PawnCacheCamera.rect = frameSet.uvRects[index];
//                Find.PawnCacheRenderer.RenderPawn(pawn, frameSet.atlas, Vector3.zero, 1f, 0f, rotation, renderHead: true, drawMode == PawnDrawMode.BodyAndHead);
//                Find.PawnCacheCamera.rect = new Rect(0f, 0f, 1f, 1f);
//                frameSet.isDirty[index] = false;
//            }
//            return frameSet.meshes[index];
//        }

//        private void DrawCarriedThing(Vector3 drawLoc)
//        {
//            if (pawn.carryTracker == null)
//            {
//                return;
//            }
//            Thing carriedThing = pawn.carryTracker.CarriedThing;
//            if (carriedThing == null)
//            {
//                return;
//            }
//            Vector3 drawPos = drawLoc;
//            bool behind = false;
//            bool flip = false;
//            if (pawn.CurJob == null || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref drawPos, ref behind, ref flip))
//            {
//                if (carriedThing is Pawn || carriedThing is Corpse)
//                {
//                    drawPos += new Vector3(0.44f, 0f, 0f);
//                }
//                else
//                {
//                    drawPos += new Vector3(0.18f, 0f, 0.05f);
//                }
//            }
//            if (behind)
//            {
//                drawPos.y -= 0.03474903f;
//            }
//            else
//            {
//                drawPos.y += 0.03474903f;
//            }
//            carriedThing.DrawAt(drawPos, flip);
//        }

//        private void DrawInvisibleShadow(Vector3 drawLoc)
//        {
//            if (pawn.def.race.specialShadowData != null)
//            {
//                if (shadowGraphic == null)
//                {
//                    shadowGraphic = new Graphic_Shadow(pawn.def.race.specialShadowData);
//                }
//                shadowGraphic.Draw(drawLoc, Rot4.North, pawn);
//            }
//            if (graphics.nakedGraphic != null && graphics.nakedGraphic.ShadowGraphic != null)
//            {
//                graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, pawn);
//            }
//        }

//        private Vector3 GetBodyPos(Vector3 drawLoc, out bool showBody)
//        {
//            Building_Bed building_Bed = pawn.CurrentBed();
//            Vector3 result;
//            if (building_Bed != null && pawn.RaceProps.Humanlike)
//            {
//                showBody = building_Bed.def.building.bed_showSleeperBody;
//                AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 18);
//                Vector3 vector;
//                Vector3 vector2 = (vector = pawn.Position.ToVector3ShiftedWithAltitude(altLayer));
//                vector.y += 0.0231660213f;
//                Rot4 rotation = building_Bed.Rotation;
//                rotation.AsInt += 2;
//                float num = 0f - BaseHeadOffsetAt(Rot4.South).z;
//                Vector3 vector3 = rotation.FacingCell.ToVector3();
//                result = vector2 + vector3 * num;
//                result.y += 0.008687258f;
//            }
//            else
//            {
//                showBody = true;
//                result = drawLoc;
//                IThingHolderWithDrawnPawn thingHolderWithDrawnPawn;
//                if ((thingHolderWithDrawnPawn = pawn.ParentHolder as IThingHolderWithDrawnPawn) != null)
//                {
//                    result.y = thingHolderWithDrawnPawn.HeldPawnDrawPos_Y;
//                }
//                else if (!pawn.Dead && pawn.CarriedBy == null)
//                {
//                    result.y = AltitudeLayer.LayingPawn.AltitudeFor() + 0.008687258f;
//                }
//            }
//            return result;
//        }

//        public GraphicMeshSet GetBodyOverlayMeshSet()
//        {
//            if (!pawn.RaceProps.Humanlike)
//            {
//                return MeshPool.humanlikeBodySet;
//            }
//            BodyTypeDef bodyType = pawn.story.bodyType;
//            if (bodyType == BodyTypeDefOf.Male)
//            {
//                return MeshPool.humanlikeBodySet_Male;
//            }
//            if (bodyType == BodyTypeDefOf.Female)
//            {
//                return MeshPool.humanlikeBodySet_Female;
//            }
//            if (bodyType == BodyTypeDefOf.Thin)
//            {
//                return MeshPool.humanlikeBodySet_Thin;
//            }
//            if (bodyType == BodyTypeDefOf.Fat)
//            {
//                return MeshPool.humanlikeBodySet_Fat;
//            }
//            if (bodyType == BodyTypeDefOf.Hulk)
//            {
//                return MeshPool.humanlikeBodySet_Hulk;
//            }
//            return MeshPool.humanlikeBodySet;
//        }

//        public void RenderPawnAt(Vector3 drawLoc, Rot4? rotOverride = null, bool neverAimWeapon = false)
//        {
//            if (!graphics.AllResolved)
//            {
//                graphics.ResolveAllGraphics();
//            }
//            Rot4 rot = rotOverride ?? pawn.Rotation;
//            PawnRenderFlags pawnRenderFlags = GetDefaultRenderFlags(pawn);
//            if (neverAimWeapon)
//            {
//                pawnRenderFlags |= PawnRenderFlags.NeverAimWeapon;
//            }
//            RotDrawMode curRotDrawMode = CurRotDrawMode;
//            bool flag = pawn.RaceProps.Humanlike && curRotDrawMode != RotDrawMode.Dessicated && !pawn.IsInvisible();
//            PawnTextureAtlasFrameSet frameSet = null;
//            if (flag && !GlobalTextureAtlasManager.TryGetPawnFrameSet(pawn, out frameSet, out var _))
//            {
//                flag = false;
//            }
//            if (pawn.GetPosture() == PawnPosture.Standing)
//            {
//                if (flag)
//                {
//                    Material original = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
//                    original = OverrideMaterialIfNeeded(original, pawn);
//                    GenDraw.DrawMeshNowOrLater(yayoAni.yayo.GetBlitMeshUpdatedFrame(frameSet, rot, PawnDrawMode.BodyAndHead, pawn), drawLoc, Quaternion.AngleAxis(0f, Vector3.up), original, drawNow: false);
//                    DrawDynamicParts(drawLoc, 0f, rot, pawnRenderFlags);
//                }
//                else
//                {
//                    yayoAni.yayo.RenderPawnInternal(drawLoc, 0f, renderBody: true, rot, curRotDrawMode, pawnRenderFlags);
//                }
//                DrawCarriedThing(drawLoc);
//                if (!pawnRenderFlags.FlagSet(PawnRenderFlags.Invisible))
//                {
//                    DrawInvisibleShadow(drawLoc);
//                }
//            }
//            else
//            {
//                bool showBody;
//                Vector3 bodyPos = GetBodyPos(drawLoc, out showBody);
//                float angle = BodyAngle();
//                Rot4 rot2 = LayingFacing();
//                if (flag)
//                {
//                    Material original2 = MaterialPool.MatFrom(new MaterialRequest(frameSet.atlas, ShaderDatabase.Cutout));
//                    original2 = OverrideMaterialIfNeeded(original2, pawn);
//                    GenDraw.DrawMeshNowOrLater(GetBlitMeshUpdatedFrame(frameSet, rot2, (!showBody) ? PawnDrawMode.HeadOnly : PawnDrawMode.BodyAndHead), bodyPos, Quaternion.AngleAxis(angle, Vector3.up), original2, drawNow: false);
//                    DrawDynamicParts(bodyPos, angle, rot, pawnRenderFlags);
//                }
//                else
//                {
//                    RenderPawnInternal(bodyPos, angle, showBody, rot2, curRotDrawMode, pawnRenderFlags);
//                }
//            }
//            if (pawn.Spawned && !pawn.Dead)
//            {
//                pawn.stances.StanceTrackerDraw();
//                pawn.pather.PatherDraw();
//                pawn.roping.RopingDraw();
//            }
//            DrawDebug();
//        }

//        public void RenderCache(Rot4 rotation, float angle, Vector3 positionOffset, bool renderHead, bool renderBody, bool portrait, bool renderHeadgear, bool renderClothes, Dictionary<Apparel, Color> overrideApparelColor = null, Color? overrideHairColor = null, bool stylingStation = false)
//        {
//            Vector3 zero = Vector3.zero;
//            PawnRenderFlags pawnRenderFlags = GetDefaultRenderFlags(pawn);
//            if (portrait)
//            {
//                pawnRenderFlags |= PawnRenderFlags.Portrait;
//            }
//            pawnRenderFlags |= PawnRenderFlags.Cache;
//            pawnRenderFlags |= PawnRenderFlags.DrawNow;
//            if (!renderHead)
//            {
//                pawnRenderFlags |= PawnRenderFlags.HeadStump;
//            }
//            if (renderHeadgear)
//            {
//                pawnRenderFlags |= PawnRenderFlags.Headgear;
//            }
//            if (renderClothes)
//            {
//                pawnRenderFlags |= PawnRenderFlags.Clothes;
//            }
//            if (stylingStation)
//            {
//                pawnRenderFlags |= PawnRenderFlags.StylingStation;
//            }
//            tmpOriginalColors.Clear();
//            try
//            {
//                if (overrideApparelColor != null)
//                {
//                    foreach (KeyValuePair<Apparel, Color> item in overrideApparelColor)
//                    {
//                        Apparel key = item.Key;
//                        CompColorable compColorable = key.TryGetComp<CompColorable>();
//                        if (compColorable != null)
//                        {
//                            tmpOriginalColors.Add(key, (compColorable.Color, compColorable.Active));
//                            key.SetColor(item.Value);
//                        }
//                    }
//                }
//                Color hairColor = Color.white;
//                if (pawn.story != null)
//                {
//                    hairColor = pawn.story.hairColor;
//                    if (overrideHairColor.HasValue)
//                    {
//                        pawn.story.hairColor = overrideHairColor.Value;
//                        pawn.Drawer.renderer.graphics.CalculateHairMats();
//                    }
//                }
//                RenderPawnInternal(zero + positionOffset, angle, renderBody, rotation, CurRotDrawMode, pawnRenderFlags);
//                foreach (KeyValuePair<Apparel, (Color, bool)> tmpOriginalColor in tmpOriginalColors)
//                {
//                    if (!tmpOriginalColor.Value.Item2)
//                    {
//                        tmpOriginalColor.Key.TryGetComp<CompColorable>().Disable();
//                    }
//                    else
//                    {
//                        tmpOriginalColor.Key.SetColor(tmpOriginalColor.Value.Item1);
//                    }
//                }
//                if (pawn.story != null && overrideHairColor.HasValue)
//                {
//                    pawn.story.hairColor = hairColor;
//                    pawn.Drawer.renderer.graphics.CalculateHairMats();
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Error("Error rendering pawn portrait: " + ex.Message);
//            }
//            finally
//            {
//                tmpOriginalColors.Clear();
//            }
//        }

//        private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
//        {
//            if (!graphics.AllResolved)
//            {
//                graphics.ResolveAllGraphics();
//            }
//            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
//            Vector3 vector = rootLoc;
//            Vector3 vector2 = rootLoc;
//            if (bodyFacing != Rot4.North)
//            {
//                vector2.y += 0.0231660213f;
//                vector.y += 3f / 148f;
//            }
//            else
//            {
//                vector2.y += 3f / 148f;
//                vector.y += 0.0231660213f;
//            }
//            Vector3 utilityLoc = rootLoc;
//            utilityLoc.y += ((bodyFacing == Rot4.South) ? 0.00579150533f : 0.0289575271f);
//            Mesh bodyMesh = null;
//            Vector3 drawLoc;
//            if (renderBody)
//            {
//                DrawPawnBody(rootLoc, angle, bodyFacing, bodyDrawType, flags, out bodyMesh);
//                drawLoc = rootLoc;
//                drawLoc.y += 0.009687258f;
//                if (bodyDrawType == RotDrawMode.Fresh)
//                {
//                    woundOverlays.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Body, bodyFacing, false);
//                }
//                if (renderBody && flags.FlagSet(PawnRenderFlags.Clothes))
//                {
//                    DrawBodyApparel(vector, utilityLoc, bodyMesh, angle, bodyFacing, flags);
//                }
//                drawLoc = rootLoc;
//                drawLoc.y += 0.0221660212f;
//                if (bodyDrawType == RotDrawMode.Fresh)
//                {
//                    woundOverlays.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Body, bodyFacing, true);
//                }
//            }
//            Vector3 vector3 = Vector3.zero;
//            drawLoc = rootLoc;
//            drawLoc.y += 0.0289575271f;
//            if (graphics.headGraphic != null)
//            {
//                vector3 = quaternion * BaseHeadOffsetAt(bodyFacing);
//                Material material = graphics.HeadMatAt(bodyFacing, bodyDrawType, flags.FlagSet(PawnRenderFlags.HeadStump), flags.FlagSet(PawnRenderFlags.Portrait), !flags.FlagSet(PawnRenderFlags.Cache));
//                if (material != null)
//                {
//                    GenDraw.DrawMeshNowOrLater(MeshPool.humanlikeHeadSet.MeshAt(bodyFacing), vector2 + vector3, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
//                }
//            }
//            if (bodyDrawType == RotDrawMode.Fresh)
//            {
//                woundOverlays.RenderOverBody(drawLoc, bodyMesh, quaternion, flags.FlagSet(PawnRenderFlags.DrawNow), BodyTypeDef.WoundLayer.Head, bodyFacing);
//            }
//            if (graphics.headGraphic != null)
//            {
//                DrawHeadHair(rootLoc, vector3, angle, bodyFacing, bodyFacing, bodyDrawType, flags);
//            }
//            if (!flags.FlagSet(PawnRenderFlags.Portrait) && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0 && graphics.packGraphic != null)
//            {
//                GenDraw.DrawMeshNowOrLater(bodyMesh, Matrix4x4.TRS(vector, quaternion, Vector3.one), graphics.packGraphic.MatAt(bodyFacing), flags.FlagSet(PawnRenderFlags.DrawNow));
//            }
//            if (!flags.FlagSet(PawnRenderFlags.Portrait) && !flags.FlagSet(PawnRenderFlags.Cache))
//            {
//                DrawDynamicParts(rootLoc, angle, bodyFacing, flags);
//            }
//        }

//        private void DrawPawnBody(Vector3 rootLoc, float angle, Rot4 facing, RotDrawMode bodyDrawType, PawnRenderFlags flags, out Mesh bodyMesh)
//        {
//            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
//            Vector3 vector = rootLoc;
//            vector.y += 0.008687258f;
//            Vector3 loc = vector;
//            loc.y += 0.00144787633f;
//            bodyMesh = null;
//            if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && graphics.dessicatedGraphic != null && !flags.FlagSet(PawnRenderFlags.Portrait))
//            {
//                graphics.dessicatedGraphic.Draw(vector, facing, pawn, angle);
//                return;
//            }
//            if (pawn.RaceProps.Humanlike)
//            {
//                bodyMesh = MeshPool.humanlikeBodySet.MeshAt(facing);
//            }
//            else
//            {
//                bodyMesh = graphics.nakedGraphic.MeshAt(facing);
//            }
//            List<Material> list = graphics.MatsBodyBaseAt(facing, bodyDrawType, flags.FlagSet(PawnRenderFlags.Clothes));
//            for (int i = 0; i < list.Count; i++)
//            {
//                Material mat = (flags.FlagSet(PawnRenderFlags.Cache) ? list[i] : OverrideMaterialIfNeeded(list[i], pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
//                GenDraw.DrawMeshNowOrLater(bodyMesh, vector, quat, mat, flags.FlagSet(PawnRenderFlags.DrawNow));
//                vector.y += 0.00289575267f;
//            }
//            if (ModsConfig.IdeologyActive && graphics.bodyTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && (facing != Rot4.North || pawn.style.FaceTattoo.visibleNorth))
//            {
//                GenDraw.DrawMeshNowOrLater(GetBodyOverlayMeshSet().MeshAt(facing), loc, quat, graphics.bodyTattooGraphic.MatAt(facing), flags.FlagSet(PawnRenderFlags.DrawNow));
//            }
//        }

//        private void DrawHeadHair(Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
//        {
//            if (ShellFullyCoversHead(flags))
//            {
//                return;
//            }
//            Vector3 onHeadLoc = rootLoc + headOffset;
//            onHeadLoc.y += 0.0289575271f;
//            List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
//            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
//            bool flag = false;
//            bool flag2 = bodyFacing == Rot4.North || pawn.style == null || pawn.style.beardDef == BeardDefOf.NoBeard;
//            bool flag3 = flags.FlagSet(PawnRenderFlags.Headgear) && (!flags.FlagSet(PawnRenderFlags.Portrait) || !Prefs.HatsOnlyOnMap || flags.FlagSet(PawnRenderFlags.StylingStation));
//            if (flag3)
//            {
//                for (int i = 0; i < apparelGraphics.Count; i++)
//                {
//                    if (apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover)
//                    {
//                        if (apparelGraphics[i].sourceApparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
//                        {
//                            flag2 = true;
//                        }
//                        if (!apparelGraphics[i].sourceApparel.def.apparel.hatRenderedFrontOfFace && !apparelGraphics[i].sourceApparel.def.apparel.forceRenderUnderHair)
//                        {
//                            flag = true;
//                        }
//                    }
//                }
//            }
//            if (ModsConfig.IdeologyActive && graphics.faceTattooGraphic != null && bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump) && (bodyFacing != Rot4.North || pawn.style.FaceTattoo.visibleNorth))
//            {
//                Vector3 loc = onHeadLoc;
//                loc.y -= 0.00144787633f;
//                GenDraw.DrawMeshNowOrLater(graphics.HairMeshSet.MeshAt(headFacing), loc, quat, graphics.faceTattooGraphic.MatAt(headFacing), flags.FlagSet(PawnRenderFlags.DrawNow));
//            }
//            if (!flag2 && bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump) && pawn.style != null && pawn.style.beardDef != null)
//            {
//                Vector3 loc2 = OffsetBeardLocationForCrownType(pawn.story.crownType, headFacing, onHeadLoc) + pawn.style.beardDef.GetOffset(pawn.story.crownType, headFacing);
//                Mesh mesh = graphics.HairMeshSet.MeshAt(headFacing);
//                Material material = graphics.BeardMatAt(headFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
//                if (material != null)
//                {
//                    GenDraw.DrawMeshNowOrLater(mesh, loc2, quat, material, flags.FlagSet(PawnRenderFlags.DrawNow));
//                }
//            }
//            if (flag3)
//            {
//                for (int j = 0; j < apparelGraphics.Count; j++)
//                {
//                    if (apparelGraphics[j].sourceApparel.def.apparel.forceRenderUnderHair)
//                    {
//                        DrawApparel(apparelGraphics[j]);
//                    }
//                }
//            }
//            if (!flag && bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump))
//            {
//                Mesh mesh2 = graphics.HairMeshSet.MeshAt(headFacing);
//                Material material2 = graphics.HairMatAt(headFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
//                if (material2 != null)
//                {
//                    GenDraw.DrawMeshNowOrLater(mesh2, onHeadLoc, quat, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//                }
//            }
//            if (!flag3)
//            {
//                return;
//            }
//            for (int k = 0; k < apparelGraphics.Count; k++)
//            {
//                if ((apparelGraphics[k].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead || apparelGraphics[k].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.EyeCover) && !apparelGraphics[k].sourceApparel.def.apparel.forceRenderUnderHair)
//                {
//                    DrawApparel(apparelGraphics[k]);
//                }
//            }
//            void DrawApparel(ApparelGraphicRecord apparelRecord)
//            {
//                Mesh mesh3 = graphics.HairMeshSet.MeshAt(headFacing);
//                if (!apparelRecord.sourceApparel.def.apparel.hatRenderedFrontOfFace)
//                {
//                    Material material3 = apparelRecord.graphic.MatAt(bodyFacing);
//                    material3 = (flags.FlagSet(PawnRenderFlags.Cache) ? material3 : OverrideMaterialIfNeeded(material3, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
//                    GenDraw.DrawMeshNowOrLater(mesh3, onHeadLoc, quat, material3, flags.FlagSet(PawnRenderFlags.DrawNow));
//                }
//                else
//                {
//                    Material material4 = apparelRecord.graphic.MatAt(bodyFacing);
//                    material4 = (flags.FlagSet(PawnRenderFlags.Cache) ? material4 : OverrideMaterialIfNeeded(material4, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
//                    Vector3 loc3 = rootLoc + headOffset;
//                    if (apparelRecord.sourceApparel.def.apparel.hatRenderedBehindHead)
//                    {
//                        loc3.y += 0.0221660212f;
//                    }
//                    else
//                    {
//                        loc3.y += ((bodyFacing == Rot4.North && !apparelRecord.sourceApparel.def.apparel.hatRenderedAboveBody) ? 0.00289575267f : 0.03185328f);
//                    }
//                    GenDraw.DrawMeshNowOrLater(mesh3, loc3, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
//                }
//            }
//        }

//        private bool ShellFullyCoversHead(PawnRenderFlags flags)
//        {
//            if (!flags.FlagSet(PawnRenderFlags.Clothes))
//            {
//                return false;
//            }
//            List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
//            for (int i = 0; i < apparelGraphics.Count; i++)
//            {
//                if (apparelGraphics[i].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && apparelGraphics[i].sourceApparel.def.apparel.shellCoversHead)
//                {
//                    return true;
//                }
//            }
//            return false;
//        }

//        private Vector3 OffsetBeardLocationForCrownType(CrownType crownType, Rot4 headFacing, Vector3 beardLoc)
//        {
//            if (pawn.story.crownType == CrownType.Narrow)
//            {
//                if (headFacing == Rot4.East)
//                {
//                    beardLoc += Vector3.right * -0.05f;
//                }
//                else if (headFacing == Rot4.West)
//                {
//                    beardLoc += Vector3.right * 0.05f;
//                }
//                beardLoc += Vector3.forward * -0.05f;
//            }
//            return beardLoc;
//        }

//        private void DrawBodyApparel(Vector3 shellLoc, Vector3 utilityLoc, Mesh bodyMesh, float angle, Rot4 bodyFacing, PawnRenderFlags flags)
//        {
//            List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
//            Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
//            for (int i = 0; i < apparelGraphics.Count; i++)
//            {
//                ApparelGraphicRecord apparelGraphicRecord = apparelGraphics[i];
//                if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && !apparelGraphicRecord.sourceApparel.def.apparel.shellRenderedBehindHead)
//                {
//                    Material material = apparelGraphicRecord.graphic.MatAt(bodyFacing);
//                    material = (flags.FlagSet(PawnRenderFlags.Cache) ? material : OverrideMaterialIfNeeded(material, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
//                    Vector3 loc = shellLoc;
//                    if (apparelGraphicRecord.sourceApparel.def.apparel.shellCoversHead)
//                    {
//                        loc.y += 0.00289575267f;
//                    }
//                    GenDraw.DrawMeshNowOrLater(bodyMesh, loc, quaternion, material, flags.FlagSet(PawnRenderFlags.DrawNow));
//                }
//                if (RenderAsPack(apparelGraphicRecord.sourceApparel))
//                {
//                    Material material2 = apparelGraphicRecord.graphic.MatAt(bodyFacing);
//                    material2 = (flags.FlagSet(PawnRenderFlags.Cache) ? material2 : OverrideMaterialIfNeeded(material2, pawn, flags.FlagSet(PawnRenderFlags.Portrait)));
//                    if (apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData != null)
//                    {
//                        Vector2 vector = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(bodyFacing, pawn.story.bodyType);
//                        Vector2 vector2 = apparelGraphicRecord.sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(pawn.story.bodyType);
//                        Matrix4x4 matrix = Matrix4x4.Translate(utilityLoc) * Matrix4x4.Rotate(quaternion) * Matrix4x4.Translate(new Vector3(vector.x, 0f, vector.y)) * Matrix4x4.Scale(new Vector3(vector2.x, 1f, vector2.y));
//                        GenDraw.DrawMeshNowOrLater(bodyMesh, matrix, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//                    }
//                    else
//                    {
//                        GenDraw.DrawMeshNowOrLater(bodyMesh, shellLoc, quaternion, material2, flags.FlagSet(PawnRenderFlags.DrawNow));
//                    }
//                }
//            }
//        }

//        private void DrawDynamicParts(Vector3 rootLoc, float angle, Rot4 pawnRotation, PawnRenderFlags flags)
//        {
//            Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
//            DrawEquipment(rootLoc, pawnRotation, flags);
//            if (pawn.apparel != null)
//            {
//                List<Apparel> wornApparel = pawn.apparel.WornApparel;
//                for (int i = 0; i < wornApparel.Count; i++)
//                {
//                    wornApparel[i].DrawWornExtras();
//                }
//            }
//            Vector3 bodyLoc = rootLoc;
//            bodyLoc.y += 0.0376447849f;
//            statusOverlays.RenderStatusOverlays(bodyLoc, quat, MeshPool.humanlikeHeadSet.MeshAt(pawnRotation));
//        }

//        private void DrawEquipment(Vector3 rootLoc, Rot4 pawnRotation, PawnRenderFlags flags)
//        {
//            if (pawn.Dead || !pawn.Spawned || pawn.equipment == null || pawn.equipment.Primary == null || (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon))
//            {
//                return;
//            }
//            Vector3 drawLoc = new Vector3(0f, (pawnRotation == Rot4.North) ? (-0.00289575267f) : 0.03474903f, 0f);
//            Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
//            if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid && (flags & PawnRenderFlags.NeverAimWeapon) == 0)
//            {
//                Vector3 vector = ((!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : stance_Busy.focusTarg.Thing.DrawPos);
//                float num = 0f;
//                if ((vector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
//                {
//                    num = (vector - pawn.DrawPos).AngleFlat();
//                }
//                drawLoc += rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
//                DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, num);
//            }
//            else if (CarryWeaponOpenly())
//            {
//                if (pawnRotation == Rot4.South)
//                {
//                    drawLoc += rootLoc + new Vector3(0f, 0f, -0.22f);
//                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
//                }
//                else if (pawnRotation == Rot4.North)
//                {
//                    drawLoc += rootLoc + new Vector3(0f, 0f, -0.11f);
//                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
//                }
//                else if (pawnRotation == Rot4.East)
//                {
//                    drawLoc += rootLoc + new Vector3(0.2f, 0f, -0.22f);
//                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 143f);
//                }
//                else if (pawnRotation == Rot4.West)
//                {
//                    drawLoc += rootLoc + new Vector3(-0.2f, 0f, -0.22f);
//                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, 217f);
//                }
//            }
//        }

//        public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
//        {
//            Mesh mesh = null;
//            float num = aimAngle - 90f;
//            if (aimAngle > 20f && aimAngle < 160f)
//            {
//                mesh = MeshPool.plane10;
//                num += eq.def.equippedAngleOffset;
//            }
//            else if (aimAngle > 200f && aimAngle < 340f)
//            {
//                mesh = MeshPool.plane10Flip;
//                num -= 180f;
//                num -= eq.def.equippedAngleOffset;
//            }
//            else
//            {
//                mesh = MeshPool.plane10;
//                num += eq.def.equippedAngleOffset;
//            }
//            num %= 360f;
//            CompEquippable compEquippable = eq.TryGetComp<CompEquippable>();
//            if (compEquippable != null)
//            {
//                EquipmentUtility.Recoil(eq.def, EquipmentUtility.GetRecoilVerb(compEquippable.AllVerbs), out var drawOffset, out var angleOffset, aimAngle);
//                drawLoc += drawOffset;
//                num += angleOffset;
//            }
//            Material material = null;
//            Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
//            Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingleFor(eq) : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingleFor(eq), mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
//        }

//        private Material OverrideMaterialIfNeeded(Material original, Pawn pawn, bool portrait = false)
//        {
//            Material baseMat = ((!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original);
//            return graphics.flasher.GetDamagedMat(baseMat);
//        }

//        private bool CarryWeaponOpenly()
//        {
//            if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
//            {
//                return false;
//            }
//            if (pawn.Drafted)
//            {
//                return true;
//            }
//            if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
//            {
//                return true;
//            }
//            if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)
//            {
//                return true;
//            }
//            Lord lord = pawn.GetLord();
//            if (lord != null && lord.LordJob != null && lord.LordJob.AlwaysShowWeapon)
//            {
//                return true;
//            }
//            return false;
//        }

//        private Rot4 RotationForcedByJob()
//        {
//            if (pawn.jobs != null && pawn.jobs.curDriver != null && pawn.jobs.curDriver.ForcedLayingRotation.IsValid)
//            {
//                return pawn.jobs.curDriver.ForcedLayingRotation;
//            }
//            return Rot4.Invalid;
//        }

//        public Rot4 LayingFacing()
//        {
//            Rot4 result = RotationForcedByJob();
//            if (result.IsValid)
//            {
//                return result;
//            }
//            if (pawn.GetPosture() == PawnPosture.LayingOnGroundFaceUp)
//            {
//                return Rot4.South;
//            }
//            if (pawn.RaceProps.Humanlike)
//            {
//                switch (pawn.thingIDNumber % 4)
//                {
//                    case 0:
//                        return Rot4.South;
//                    case 1:
//                        return Rot4.South;
//                    case 2:
//                        return Rot4.East;
//                    case 3:
//                        return Rot4.West;
//                }
//            }
//            else
//            {
//                switch (pawn.thingIDNumber % 4)
//                {
//                    case 0:
//                        return Rot4.South;
//                    case 1:
//                        return Rot4.East;
//                    case 2:
//                        return Rot4.West;
//                    case 3:
//                        return Rot4.West;
//                }
//            }
//            return Rot4.Random;
//        }

//        public float BodyAngle()
//        {
//            if (pawn.GetPosture() == PawnPosture.Standing)
//            {
//                return 0f;
//            }
//            Building_Bed building_Bed = pawn.CurrentBed();
//            if (building_Bed != null && pawn.RaceProps.Humanlike)
//            {
//                Rot4 rotation = building_Bed.Rotation;
//                rotation.AsInt += 2;
//                return rotation.AsAngle;
//            }
//            IThingHolderWithDrawnPawn thingHolderWithDrawnPawn;
//            if ((thingHolderWithDrawnPawn = pawn.ParentHolder as IThingHolderWithDrawnPawn) != null)
//            {
//                return thingHolderWithDrawnPawn.HeldPawnBodyAngle;
//            }
//            if (pawn.Downed || pawn.Dead)
//            {
//                return wiggler.downedAngle;
//            }
//            if (pawn.RaceProps.Humanlike)
//            {
//                return LayingFacing().AsAngle;
//            }
//            if (RotationForcedByJob().IsValid)
//            {
//                return 0f;
//            }
//            Rot4 rot = Rot4.West;
//            switch (pawn.thingIDNumber % 2)
//            {
//                case 0:
//                    rot = Rot4.West;
//                    break;
//                case 1:
//                    rot = Rot4.East;
//                    break;
//            }
//            return rot.AsAngle;
//        }

//        public Vector3 BaseHeadOffsetAt(Rot4 rotation)
//        {
//            Vector2 headOffset = pawn.story.bodyType.headOffset;
//            switch (rotation.AsInt)
//            {
//                case 0:
//                    return new Vector3(0f, 0f, headOffset.y);
//                case 1:
//                    return new Vector3(headOffset.x, 0f, headOffset.y);
//                case 2:
//                    return new Vector3(0f, 0f, headOffset.y);
//                case 3:
//                    return new Vector3(0f - headOffset.x, 0f, headOffset.y);
//                default:
//                    Log.Error("BaseHeadOffsetAt error in " + pawn);
//                    return Vector3.zero;
//            }
//        }

//        public void Notify_DamageApplied(DamageInfo dam)
//        {
//            graphics.flasher.Notify_DamageApplied(dam);
//            wiggler.Notify_DamageApplied(dam);
//        }

//        public void RendererTick()
//        {
//            wiggler.WigglerTick();
//        }

//        public static bool RenderAsPack(Apparel apparel)
//        {
//            if (apparel.def.apparel.LastLayer.IsUtilityLayer)
//            {
//                if (apparel.def.apparel.wornGraphicData != null)
//                {
//                    return apparel.def.apparel.wornGraphicData.renderUtilityAsPack;
//                }
//                return true;
//            }
//            return false;
//        }

//        private void DrawDebug()
//        {
//            if (DebugViewSettings.drawDuties && Find.Selector.IsSelected(pawn) && pawn.mindState != null && pawn.mindState.duty != null)
//            {
//            }
//        }
//    }
//}
