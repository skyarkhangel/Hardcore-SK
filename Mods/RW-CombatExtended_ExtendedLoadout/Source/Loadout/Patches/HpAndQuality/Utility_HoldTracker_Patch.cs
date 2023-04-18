using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout;

/// <summary>
/// Add HP and Quality validator for drop Excess weapons.
///
/// Versions for MultiLoadout and Standart
/// </summary>
[HarmonyPatch(typeof(Utility_HoldTracker))]
public class Utility_HoldTracker_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useHpAndQualityInLoadouts;

    /// <summary>
    /// Drop only 1 weapon per tick, if original false
    /// </summary>
    [HarmonyPatch("GetExcessEquipment")]
    [HarmonyPostfix]
    [UsedImplicitly]
    public static void Utility_HoldTracker_GetExcessEquipment(Pawn pawn, ref ThingWithComps dropEquipment, ref bool __result)
    {
        if (__result)
        {
            return; // try find equipped weapon for drop when not has ExcessThing
        }

        Loadout? loadout = pawn.GetLoadout();
        ThingWithComps? gun = pawn.equipment?.Primary;
        if (loadout == null || loadout.Slots.NullOrEmpty() || gun == null || !gun.def.IsWeapon)
        {
            return;
        }

        if (loadout is Loadout_Multi loadoutMulti)
        {
            loadout = loadoutMulti.FindLoadoutWithThingDef(gun.def);
            if (loadout == null)
            {
                return;
            }
        }

        var loadoutExtended = loadout.Extended();
        if (!loadoutExtended.Allows(gun))
        {
            dropEquipment = gun;
            __result = true;
            DbgLog.Msg($"{pawn.LabelCap} drop equipped {gun}. HP_RANGE:{loadoutExtended.HpRange}; QUALITY_RANGE:{loadoutExtended.QualityRange}");
        }
    }

    /// <summary>
    /// Drop only 1 weapon per tick, if original false
    /// </summary>
    [HarmonyPatch("GetExcessThing")]
    [HarmonyPostfix]
    [UsedImplicitly]
    public static void Utility_HoldTracker_GetExcessThing(Pawn pawn, ref Thing dropThing, ref int dropCount, ref bool __result)
    {
        if (__result)
        {
            return; // try find weapon for drop when not has ExcessThing
        }

        Loadout? loadout = pawn.GetLoadout();
        if (pawn.inventory?.innerContainer == null || loadout == null || loadout.Slots.NullOrEmpty())
        {
            return;
        }

        Loadout_Extended loadoutExtended;
        if (loadout is Loadout_Multi loadoutMulti)
        {
            foreach (Thing thing in pawn.inventory.innerContainer)
            {
                Thing thing2 = thing.GetInnerIfMinified();
                if (!thing2.def.IsWeapon)
                {
                    continue;
                }

                loadout = loadoutMulti.FindLoadoutWithThingDef(thing2.def);
                if (loadout == null)
                {
                    continue;
                }

                loadoutExtended = loadout.Extended();
                if (!loadoutExtended.Allows(thing2))
                {
                    dropThing = thing2;
                    dropCount = 1;
                    __result = true;
                    DbgLog.Msg($"{pawn.LabelCap} drop from inventory {dropThing}. HP_RANGE:{loadoutExtended.HpRange}; QUALITY_RANGE:{loadoutExtended.QualityRange}");
                    return;
                }
            }
            return;
        }

        loadoutExtended = loadout.Extended();
        foreach (Thing thing in pawn.inventory.innerContainer)
        {
            Thing thing2 = thing.GetInnerIfMinified();
            if (thing2.def.IsWeapon && !loadoutExtended.Allows(thing2))
            {
                dropThing = thing2;
                dropCount = 1;
                __result = true;
                DbgLog.Msg($"{pawn.LabelCap} drop from inventory {dropThing}. HP_RANGE:{loadoutExtended.HpRange}; QUALITY_RANGE:{loadoutExtended.QualityRange}");
                return;
            }
        }
    }
}