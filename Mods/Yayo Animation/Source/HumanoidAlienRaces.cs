using System.Collections.Generic;
using AlienRace;
using UnityEngine;
using Verse;

namespace yayoAni
{
    public static class HumanoidAlienRaces
    {
        public class Prefix_AlienRace_HarmonyPatches_DrawAddons
        {
            public static bool Prefix(PawnRenderFlags renderFlags, Vector3 vector, Vector3 headOffset, Pawn pawn, Quaternion quat, Rot4 rotation)
            {
                if (pawn.def is not ThingDef_AlienRace alienRace || renderFlags.FlagSet(PawnRenderFlags.Invisible))
                {
                    return false;
                }
                List<AlienPartGenerator.BodyAddon> bodyAddons = alienRace.alienRace.generalSettings.alienPartGenerator.bodyAddons;
                AlienPartGenerator.AlienComp comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                for (int i = 0; i < bodyAddons.Count; i++)
                {
                    AlienPartGenerator.BodyAddon bodyAddon = bodyAddons[i];
                    if (!bodyAddon.CanDrawAddon(pawn))
                    {
                        continue;
                    }
#if IDEOLOGY
                    Vector3 val = (bodyAddon.defaultOffsets.GetOffset(rotation)?.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, comp.crownType) ?? Vector3.zero) + (bodyAddon.offsets.GetOffset(rotation)?.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, comp.crownType) ?? Vector3.zero);
#else
                    Vector3 val = (bodyAddon.defaultOffsets.GetOffset(rotation)?.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, pawn.story.headType) ?? Vector3.zero) + (bodyAddon.offsets.GetOffset(rotation)?.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, pawn.story.headType) ?? Vector3.zero);
#endif
                    val.y = (bodyAddon.inFrontOfBody ? (0.3f + val.y) : (-0.3f - val.y));
                    float num = bodyAddon.angle;
                    if (rotation == Rot4.North)
                    {
                        if (bodyAddon.layerInvert)
                        {
                            val.y = 0f - val.y;
                        }
                        num = 0f;
                    }
                    if (rotation == Rot4.East)
                    {
                        num = 0f - num;
                        val.x = 0f - val.x;
                    }

                    // yayo
                    //float quatDot = Quaternion.Dot(Quaternion.identity, quat);
                    //if (quat.eulerAngles.y > 180f) quatDot = -quatDot;
                    //float acos = quat.eulerAngles.y;
                    //float acos = Mathf.Acos(quatDot) * 2f * 57.29578f;

                    //Log.Message($"{pawn.NameShortColored}: quatDot:{quatDot} quat.eulerAngles:{quat.eulerAngles} acos:{Mathf.Acos(quatDot) * 2f * 57.29578f}");
                    //Log.Message($"{pawn.NameShortColored}: headOffset:{(bodyAddon.alignWithHead ? headOffset : Vector3.zero)}");

                    Graphic graphic = comp.addonGraphics[i];
                    graphic.drawSize = ((renderFlags.FlagSet(PawnRenderFlags.Portrait) && bodyAddon.drawSizePortrait != Vector2.zero) ? bodyAddon.drawSizePortrait : bodyAddon.drawSize) * ((!bodyAddon.scaleWithPawnDrawsize) ? Vector2.one : ((!bodyAddon.alignWithHead) ? (renderFlags.FlagSet(PawnRenderFlags.Portrait) ? comp.customPortraitDrawSize : comp.customDrawSize) : (renderFlags.FlagSet(PawnRenderFlags.Portrait) ? comp.customPortraitHeadDrawSize : comp.customHeadDrawSize))) * 1.5f;

                    // yayo
                    GenDraw.DrawMeshNowOrLater(graphic.MeshAt(rotation), vector + (bodyAddon.alignWithHead ? headOffset : Vector3.zero) + val.RotatedBy(quat.eulerAngles.y), Quaternion.AngleAxis(num, Vector3.up) * quat, graphic.MatAt(rotation), renderFlags.FlagSet(PawnRenderFlags.DrawNow));
                }
                return false;
            }
        }
    }
}