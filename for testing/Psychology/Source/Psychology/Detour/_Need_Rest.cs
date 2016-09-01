using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Psychology
{
    // Token: 0x02000368 RID: 872
    internal static class _Need_Rest
    {
        internal static FieldInfo _ticksAtZero;
        internal static FieldInfo _lastRestEffectiveness;
        internal static FieldInfo _lastRestTick;
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this Need_Rest _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(Need_Rest).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect Need_Rest.pawn!", 0x12348765);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        internal static float GetLastRestEffectiveness(this Need_Rest _this)
        {
            if (_lastRestEffectiveness == null)
            {
                _lastRestEffectiveness = typeof(Need_Rest).GetField("lastRestEffectiveness", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_lastRestEffectiveness == null)
                {
                    Log.ErrorOnce("Unable to reflect Need_Rest.lastRestEffectiveness!", 305432421);
                }
            }
            return (float)_lastRestEffectiveness.GetValue(_this);
        }

        internal static int GetLastRestTick(this Need_Rest _this)
        {
            if (_lastRestTick == null)
            {
                _lastRestTick = typeof(Need_Rest).GetField("lastRestTick", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_lastRestTick == null)
                {
                    Log.ErrorOnce("Unable to reflect Need_Rest.lastRestTick!", 305432421);
                }
            }
            return (int)_lastRestTick.GetValue(_this);
        }

        internal static void SetTicksAtZero(this Need_Rest _this, int ticks)
        {
            if (_ticksAtZero == null)
            {
                _ticksAtZero = typeof(Need_Rest).GetField("ticksAtZero", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_ticksAtZero == null)
                {
                    Log.ErrorOnce("Unable to reflect Need_Rest.ticksAtZero!", 305432421);
                }
            }
            _ticksAtZero.SetValue(_this, ticks);
        }

        internal static bool Resting(this Need_Rest r)
        {
           return Find.TickManager.TicksGame < r.GetLastRestTick() + 2;
        }

        // Token: 0x06000DBA RID: 3514 RVA: 0x00045330 File Offset: 0x00043530
        internal static void _NeedInterval(this Need_Rest r)
        {
            Pawn pawn = r.GetPawn();
            float lastRestEffectiveness = r.GetLastRestEffectiveness();
            if (Resting(r))
            {
                if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOfPsychology.Insomniac))
                {
                    r.CurLevel += 0.005714286f * (lastRestEffectiveness / 3);
                    if(r.CurLevel>(Need_Rest.NaturalWakeThreshold/4f))
                        if(Rand.MTBEventOccurs((Need_Rest.NaturalWakeThreshold-r.CurLevel)/4f, 60000f, 150f) && !pawn.Awake())
                            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
                else
                    r.CurLevel += 0.005714286f * lastRestEffectiveness;
                if(Rand.Value < 0.001f && pawn.RaceProps.Humanlike)
                {
                    if(Rand.Value < 0.5f)
                    {
                        if(Rand.Value < 0.125f)
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DreamNightmare, pawn);
                        }
                        else
                        {
                            pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DreamBad, pawn);
                        }
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemoryThought(ThoughtDefOfPsychology.DreamGood, pawn);
                    }
                }
            }
            else
            {
                r.CurLevel -= r.RestFallPerTick * 150f;
            }
            if (r.CurLevel < 0.0001f)
            {
                r.SetTicksAtZero(r.TicksAtZero+150);
            }
            else
            {
                r.SetTicksAtZero(0);
            }
            if (r.TicksAtZero > 1000)
            {
                float mtb;
                if (r.TicksAtZero < 15000)
                {
                    mtb = 0.25f;
                }
                else if (r.TicksAtZero < 30000)
                {
                    mtb = 0.125f;
                }
                else if (r.TicksAtZero < 45000)
                {
                    mtb = 0.0833333358f;
                }
                else
                {
                    mtb = 0.0625f;
                }
                if (Rand.MTBEventOccurs(mtb, 60000f, 150f))
                {
                    pawn.jobs.StartJob(new Job(JobDefOf.LayDown, pawn.Position), JobCondition.InterruptForced, null, false, true, null);
                    if (PawnUtility.ShouldSendNotificationAbout(pawn))
                    {
                        Messages.Message("MessageInvoluntarySleep".Translate(new object[]
                        {
                            pawn.LabelShort
                        }), pawn, MessageSound.Negative);
                    }
                }
            }
        }
    }
}
