using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoAni
{
    public class Core : Mod
    {
        public static YayoAniSettings settings;
        public static Harmony harmony;

        public Core(ModContentPack content) : base(content)
        {
            settings = GetSettings<YayoAniSettings>();
            harmony = new Harmony("com.yayo.yayoAni");
            harmony.PatchAll();

            if (usingHar && settings.applyHarPatch) 
                LongEventHandler.ExecuteWhenFinished(() => SetHarPatch(true));
        }

        public override string SettingsCategory() => "Yayo's Animation";

        public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);

        public static bool usingDualWield = false;
        public static bool usingHar = false;
        private static bool harPatchActive = false;

        static Core()
        {
            foreach (var mod in ModsConfig.ActiveModsInLoadOrder)
            {
                switch (mod.PackageId.ToLower())
                {
                    case "roolo.dualwield":
                        usingDualWield = true;
                        DualWield.Init();
                        Log.Message("# DualWield detected");
                        break;
                    case "erdelf.humanoidalienraces":
                        usingHar = true;
                        Log.Message("# HumanoidAlienRaces detected");
                        break;
                }
            }
        }

        public static void SetHarPatch(bool state)
        {
            if (!usingHar || harPatchActive == state) 
                return;

            var method = AccessTools.Method("AlienRace.HarmonyPatches:DrawAddons");
            var patch = AccessTools.Method(
                typeof(HumanoidAlienRaces.Prefix_AlienRace_HarmonyPatches_DrawAddons),
                nameof(HumanoidAlienRaces.Prefix_AlienRace_HarmonyPatches_DrawAddons.Prefix));

            if (state)
                harmony.Patch(method, prefix: new HarmonyMethod(patch, 0));
            else
                harmony.Unpatch(method, patch);

            harPatchActive = state;
        }

        public static Rot4 getRot(Vector3 vel, Rot4 curRot)
        {
            Rot4 r = Rot4.South;
            if (curRot == Rot4.North || curRot == Rot4.East)
            {
                if (Mathf.Abs(vel.x) > Mathf.Abs(vel.z))
                {
                    r = vel.x >= 0 ? Rot4.West : Rot4.East;
                }
                else
                {
                    r = vel.z > 0 ? Rot4.East : Rot4.West;
                }
            }
            else if (curRot == Rot4.South || curRot == Rot4.West)
            {
                if (Mathf.Abs(vel.x) > Mathf.Abs(vel.z))
                {
                    r = vel.x >= 0 ? Rot4.East : Rot4.West;
                }
                else
                {
                    r = vel.z > 0 ? Rot4.West : Rot4.East;
                }
            }

            return r;
        }

        public static bool checkAniTick(ref int tick, int duration)
        {
            if (tick >= duration)
            {
                tick -= duration;
                return false;
            }

            return true;
        }

        private const float piHalf = Mathf.PI / 2f;
        private const float angleReduce = 0.5f;
        private const float angleToPos = 0.01f;

        public enum tweenType
        {
            line,
            sin
        }

        public static bool Ani(ref int tick, int duration, ref float angle, float s_angle, float t_angle, float centerY, ref Vector3 pos, Vector3 s_pos, Vector3 t_pos, Rot4? rot = null,
            tweenType tween = tweenType.sin, Rot4? axis = null)
        {
            if (tick >= duration)
            {
                tick -= duration;
                return false;
            }

            bool needCenterCheck = true;
            if (axis != null)
            {
                if (rot != null)
                {
                    if (rot == Rot4.West)
                    {
                        s_angle = -s_angle;
                        t_angle = -t_angle;
                        s_pos = new Vector3(-s_pos.x, 0f, s_pos.z);
                        t_pos = new Vector3(-t_pos.x, 0f, t_pos.z);
                    }
                }

                if (axis != Rot4.South)
                {
                    needCenterCheck = false;
                }

                if (axis == Rot4.North)
                {
                    //s_angle = -s_angle;
                    //t_angle = -t_angle;
                    s_pos = new Vector3(-s_pos.x, 0f, -s_pos.z);
                    t_pos = new Vector3(-t_pos.x, 0f, -t_pos.z);
                    if (centerY != 0f)
                    {
                        s_pos += new Vector3(s_angle * 0.01f * centerY, 0f, 0f);
                        t_pos += new Vector3(t_angle * 0.01f * centerY, 0f, 0f);
                    }
                }
                else if (axis == Rot4.West)
                {
                    s_pos = new Vector3(s_pos.z, 0f, -s_pos.x);
                    t_pos = new Vector3(t_pos.z, 0f, -t_pos.x);
                    if (centerY != 0f)
                    {
                        s_pos += new Vector3(0f, 0f, s_angle * 0.01f * centerY);
                        t_pos += new Vector3(0f, 0f, t_angle * 0.01f * centerY);
                    }
                }
                else if (axis == Rot4.East)
                {
                    s_pos = new Vector3(-s_pos.z, 0f, s_pos.x);
                    t_pos = new Vector3(-t_pos.z, 0f, t_pos.x);
                    if (centerY != 0f)
                    {
                        s_pos += new Vector3(0f, 0f, -s_angle * 0.01f * centerY);
                        t_pos += new Vector3(0f, 0f, -t_angle * 0.01f * centerY);
                    }
                }
            }
            else if (rot != null)
            {
                if (rot == Rot4.West)
                {
                    s_angle = -s_angle;
                    t_angle = -t_angle;
                    s_pos = new Vector3(-s_pos.x, 0f, s_pos.z);
                    t_pos = new Vector3(-t_pos.x, 0f, t_pos.z);
                }
                else if ((Rot4)rot == Rot4.South)
                {
                    s_angle *= angleReduce;
                    t_angle *= angleReduce;
                    s_pos = new Vector3(0f, 0f, s_pos.z - s_pos.x - s_angle * angleToPos);
                    t_pos = new Vector3(0f, 0f, t_pos.z - t_pos.x - t_angle * angleToPos);
                }
                else if ((Rot4)rot == Rot4.North)
                {
                    s_angle *= -angleReduce;
                    t_angle *= -angleReduce;
                    s_pos = new Vector3(0f, 0f, s_pos.z + s_pos.x - s_angle * angleToPos);
                    t_pos = new Vector3(0f, 0f, t_pos.z + t_pos.x - t_angle * angleToPos);
                }
            }

            if (needCenterCheck && centerY != 0f)
            {
                s_pos += new Vector3(s_angle * -0.01f * centerY, 0f, 0f);
                t_pos += new Vector3(t_angle * -0.01f * centerY, 0f, 0f);
            }

            float tickPer = tween switch
            {
                tweenType.sin => Mathf.Sin(piHalf * (tick / (float)duration)),
                _ => (tick / (float)duration)
            };

            angle += s_angle + (t_angle - s_angle) * tickPer;
            pos += s_pos + (t_pos - s_pos) * tickPer;
            return true;
        }

        public static bool Ani(ref int tick, int duration, ref float angle, float s_angle, float t_angle, float centerY, ref Vector3 pos, Rot4? rot = null, tweenType tween = tweenType.sin)
        {
            return Ani(ref tick, duration, ref angle, s_angle, t_angle, centerY, ref pos, Vector3.zero, Vector3.zero, rot, tween);
        }

        public static bool Ani(ref int tick, int duration, ref float angle, ref Vector3 pos, Vector3 s_pos, Vector3 t_pos, Rot4? rot = null, tweenType tween = tweenType.sin)
        {
            return Ani(ref tick, duration, ref angle, 0f, 0f, 0f, ref pos, s_pos, t_pos, rot, tween);
        }

        public static bool Ani(ref int tick, int duration, ref int? nextUpdateTick)
        {
            if (tick >= duration)
            {
                tick -= duration;
                return false;
            }

            nextUpdateTick = Find.TickManager.TicksGame + (tick - duration);
            return true;
        }

        public static Rot4 Rot90(Rot4 rot) => new(rot.AsInt + 1);
        public static Rot4 Rot90b(Rot4 rot) => new(rot.AsInt - 1);

        private static List<LordJob_Ritual> ar_lordJob_ritual = new();

        public static LordJob_Ritual GetPawnRitual(Pawn p)
        {
            ar_lordJob_ritual = Find.IdeoManager.GetActiveRituals(p.Map);
            if (ar_lordJob_ritual == null) return null;
            foreach (LordJob_Ritual l in ar_lordJob_ritual)
            {
                if (l.PawnsToCountTowardsPresence.Contains(p)) return l;
            }

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }
}