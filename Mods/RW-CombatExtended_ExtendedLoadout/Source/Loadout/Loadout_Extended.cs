using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

public static class Loadout_Extensions
{
    public static Loadout_Extended Extended(this Loadout loadout)
    {
        return Loadout_Extended.Get(loadout);
    }

    public static void CopyLoadoutExtended(this Loadout dest, Loadout from)
    {
        dest.Extended().Copy(from.Extended());
    }
}

public class Loadout_Extended
{
    private static readonly Dictionary<Loadout, Loadout_Extended> Loadouts = new();
    public float RefillThreshold = 1f;
    public FloatRange HpRange = FloatRange.ZeroToOne;
    public QualityRange QualityRange = QualityRange.All;

    public static Loadout_Extended Get(Loadout loadout)
    {
        if (!Loadouts.TryGetValue(loadout, out var result))
        {
            result = new Loadout_Extended();
            Loadouts.Add(loadout, result);
        }
        DebugLog();
        return result;
    }

    public bool Allows(Thing t)
    {
        if (t.def.useHitPoints)
        {
            var hpRange = this.HpRange;
            float hp = GenMath.RoundedHundredth(t.HitPoints / (float)t.MaxHitPoints);
            if (!hpRange.IncludesEpsilon(Mathf.Clamp01(hp)))
            {
                DbgLog.Wrn($"{t.LabelCap} not allowed with hp: {hp}. range: {hpRange}");
                return false;
            }
        }

        var qualityRange = this.QualityRange;
        if (qualityRange != QualityRange.All && t.def.FollowQualityThingFilter())
        {
            if (!t.TryGetQuality(out var qc))
            {
                qc = QualityCategory.Normal;
            }

            if (!qualityRange.Includes(qc))
            {
                DbgLog.Wrn($"{t.LabelCap} not allowed with quality: {qc}. range: {qualityRange}");
                return false;
            }
        }

        return true;
    }

    public void Copy(Loadout_Extended from)
    {
        this.RefillThreshold = from.RefillThreshold;
        this.HpRange = from.HpRange;
        this.QualityRange = from.QualityRange;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref RefillThreshold, "RefillThreshold", 1f);
        Scribe_Values.Look(ref HpRange, "hpRange", FloatRange.ZeroToOne);
        Scribe_Values.Look(ref QualityRange, "qualityRange", QualityRange.All);
        DbgLog.Msg($"Loadout_Extended ExposeData");
    }

    [ClearDataOnNewGame]
    public static void ClearData()
    {
        Loadouts.Clear();
        DbgLog.Wrn($"[Loadout_Extended] Clear data");
    }

    [Conditional("DEBUG")]
    private static void DebugLog()
    {
        _ticks++;
        if (_ticks % 100 == 0)
        {
            Log.Warning($"[Loadout_Extended:Loadouts] Count: {Loadouts.Count}");
        }
    }
//#if DEBUG
    private static int _ticks;
//#endif
}