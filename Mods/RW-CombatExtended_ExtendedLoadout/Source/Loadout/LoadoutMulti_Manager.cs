using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CombatExtended.ExtendedLoadout;

public static class LoadoutMulti_Manager
{
    // rimworld exposedata buffers
    private static List<Pawn> keysWorkingList = null!;
    private static List<Loadout_Multi> valuesWorkingList = null!;

    private static Dictionary<Pawn, Loadout_Multi> assignedLoadoutsMulti = new();

    public static void ExposeData(LoadoutManager __instance)
    {
        Scribe_Collections.Look(ref assignedLoadoutsMulti, "assignedLoadoutsMulti", LookMode.Reference, LookMode.Deep, ref keysWorkingList, ref valuesWorkingList, false);
        switch (Scribe.mode)
        {
            case LoadSaveMode.PostLoadInit:
                // fix for old saves
                if (assignedLoadoutsMulti == null)
                {
                    assignedLoadoutsMulti = new Dictionary<Pawn, Loadout_Multi>();
                    // assign CE loadouts
                    if (LoadoutManager.AssignedLoadouts.Any())
                    {
                        foreach (var kv in LoadoutManager.AssignedLoadouts)
                        {
                            SetLoadout(kv.Key, kv.Value, 0);
                        }
                        //__instance._assignedLoadouts.Clear();
                        DbgLog.Wrn($"LoadoutMulti_Manager ExposeData: moved assignmentLoadouts to assignedLoadoutsMulti");
                    }
                    break;
                }
                if (assignedLoadoutsMulti.RemoveAll(x => x.Key is null) > 0)
                    Log.WarningOnce("[Extended Loadout]There are loadouts with null pawns. Those pawns aren't saved anywhere else. This could result in error when loading", 747510);
                break;
        }
        /*if (Scribe.mode == LoadSaveMode.PostLoadInit && assignedLoadoutsMulti == null)
        {
            assignedLoadoutsMulti = new Dictionary<Pawn, Loadout_Multi>();

            // assign CE loadouts
            if (LoadoutManager.AssignedLoadouts.Any())
            {
                foreach (var kv in LoadoutManager.AssignedLoadouts)
                {
                    SetLoadout(kv.Key, kv.Value, 0);
                }
                //__instance._assignedLoadouts.Clear();
                DbgLog.Wrn($"LoadoutMulti_Manager ExposeData: moved assignmentLoadouts to assignedLoadoutsMulti");
            }
        }*/
        DbgLog.Msg($"LoadoutMulti_Manager ExposeData");
    }

    /// <summary>
    /// Clear removed loadout from all pawns
    /// </summary>
    /// <param name="loadout"></param>
    public static void RemoveLoadout(Loadout loadout)
    {
        foreach (var loadoutMulti in assignedLoadoutsMulti.Values)
        {
            var loadouts = loadoutMulti.Loadouts;
            for (int i = 0; i < loadouts.Count; i++)
            {
                if (loadouts[i] == loadout)
                {
                    loadouts[i] = LoadoutManager.DefaultLoadout;
                    DbgLog.Msg($"LoadoutMulti_Manager RemoveLoadout: remove loadout {loadout.LabelCap}");
                }
            }
        }
    }

    public static IEnumerable<Loadout_Multi> LoadoutsMulti => assignedLoadoutsMulti.Values;

    [ClearDataOnNewGame]
    public static void ClearData()
    {
        int count = assignedLoadoutsMulti.Count;
        assignedLoadoutsMulti.Clear();
        Log.Message($"[LoadoutMulti_Manager] Clear data: cleared {count - assignedLoadoutsMulti.Count} loadouts");
    }

    public static int GetUniqueLoadoutID()
    {
        var loadoutsMulti = assignedLoadoutsMulti.Values;
        return loadoutsMulti.Any() ? loadoutsMulti.Max(l => l.uniqueID) + 1 : 1;
    }

    public static Loadout GetLoadout(Pawn pawn)
    {
        if (!assignedLoadoutsMulti.TryGetValue(pawn, out var loadout))
        {
            loadout = new Loadout_Multi(pawn);
            assignedLoadoutsMulti.Add(pawn, loadout);
        }
        // back compatibility
        if (loadout.PersonalLoadout == null)
        {
            loadout.GeneratePersonalLoadout(pawn);
        }
        return loadout;
    }

    public static void SetLoadout(this Pawn pawn, Loadout loadout, int index)
    {
        if (pawn == null)
        {
            throw new ArgumentNullException(nameof(pawn));
        }

        if (assignedLoadoutsMulti.ContainsKey(pawn))
        {
            assignedLoadoutsMulti[pawn][index] = loadout;
        }
        else
        {
            assignedLoadoutsMulti.Add(pawn, new Loadout_Multi(pawn) { [index] = loadout });
        }
    }
}