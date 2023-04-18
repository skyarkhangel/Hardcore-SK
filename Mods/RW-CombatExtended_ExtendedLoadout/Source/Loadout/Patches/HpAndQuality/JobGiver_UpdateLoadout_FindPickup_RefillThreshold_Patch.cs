using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using static CombatExtended.JobGiver_UpdateLoadout;

namespace CombatExtended.ExtendedLoadout;

/// <summary>
/// Add HP and Quality validator for PickUp weapons
///
/// Versions for MultiLoadout and Standart
/// </summary>
[HarmonyPatch(typeof(JobGiver_UpdateLoadout), nameof(FindPickup))]
[HotSwappable]
public class JobGiver_UpdateLoadout_FindPickup_RefillThreshold_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useHpAndQualityInLoadouts;

    public enum ItemPriority : byte
    {
        None,
        Low,
        LowStock,
        Proximity
    }

    [HarmonyPrefix]
    [UsedImplicitly]
    public static bool FindPickup(Pawn pawn, LoadoutSlot curSlot, int findCount, ref ItemPriority curPriority, ref Thing curThing, ref Pawn curCarrier)
    {
        var loadout = pawn.GetLoadout();
        if (loadout == null)
            return true;
        
        if (loadout is Loadout_Multi loadoutMulti)
        {
            loadout = loadoutMulti.FindLoadoutWithSlot(curSlot);
            if (loadout == null) {
                return true;
            }
        }

        float refillThreshold = loadout.Extended().RefillThreshold;
        float refillCount = curSlot.count - curSlot.count * refillThreshold;

        if (findCount < refillCount) {
            curPriority = ItemPriority.None;
            curThing = null!;
            curCarrier = null!;
            return false;
        }

        DbgLog.Msg($"pawn: {pawn}; count: {curSlot.count}; want: {findCount}; refillWhenWant >= {refillCount}");
        return true;
    }
}