using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BetterPawnControl;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout;

/// <summary>
/// AssignLink in fields and method headers replaced to object.
/// </summary>
public class BPC_AssignLink_Manager
{
    /// <summary>
    /// Dictionary< AssignLink, List<int> >
    /// </summary>
    /// 
    
    private static readonly Dictionary<object, List<int>> LoadoutIds = new();

    public static void AddColumnsIds(object link, List<int> columnIds)
    {
        if (!LoadoutIds.ContainsKey(link))
        {
            LoadoutIds.Add(link, columnIds);
        }
        else
        {
            LoadoutIds[link] = columnIds;
        }
    }

    public static List<int>? GetColumnsIds(object link)
    {
        return LoadoutIds.ContainsKey(link) ? LoadoutIds[link] : null;
    }

    [ClearDataOnNewGame]
    public static void ClearData()
    {
        if (ModActive.BetterPawnControl)
        {
            LoadoutIds.Clear();
            DbgLog.Wrn($"[BPC_AssignLink_Manager] Clear data");
        }
    }

    public static void ExposeData(object instance)
    {
        _ = LoadoutIds.TryGetValue(instance, out var columns);
        Scribe_Collections.Look(ref columns, "extendedLoadoutColumns", LookMode.Value);
        LoadoutIds[instance] = columns;

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (columns == null)
            {
                DbgLog.Wrn($"[ExtendedLoadout][PostLoadInit] columns = null");
                columns = new List<int>();
            }

            // fix changes in columns count
            int sizeDelta = Loadout_Multi.ColumnsCount - columns.Count;
            if (sizeDelta > 0)
            {
                Log.Warning($"[BPC_AssignLink_Manager] Fix loadouts list. Count difference: {sizeDelta}");
                for (int i = 0; i < sizeDelta; i++)
                {
                    string value = "Loadout_" + LoadoutManager.DefaultLoadout.label + "_";
                    int amount = value.Length;
                    string uniqueID = LoadoutManager.DefaultLoadout.GetUniqueLoadID().Remove(0, amount);
                    columns.Add(Int32.Parse(uniqueID));
                }

                LoadoutIds[instance] = columns;
            }
            else if (sizeDelta < 0)
            {
                Log.Warning($"[BPC_AssignLink_Manager] Fix loadouts list. Count difference: {sizeDelta}");
                for (int i = 0; i < Math.Abs(sizeDelta); i++)
                {
                    columns.RemoveAt(columns.Count - 1);
                }

                LoadoutIds[instance] = columns;
            }

            // fix AssignLink.loadoutId field in case the mod is disabled in the future or useMultiLoadouts=false in config
            {
                // set default value for loadoutId = first column from loadoutMulti
                // because if user disable multi loadouts from config or disable this mod
                // assigntab can't be opened anymore (0 not existing id)
                ((AssignLink)instance).loadoutId = columns[0];
            }
        }
    }
}

public static class BPC
{
    public static void Patch(Harmony h)
    {
        var assignLinkCtor = AccessTools.Constructor(typeof(AssignLink),
            new[]
            {
                typeof(int), typeof(Pawn), typeof(Outfit), typeof(FoodRestriction), typeof(DrugPolicy),
                typeof(HostilityResponseMode), typeof(int), typeof(int)
            });
        var worldDataStoreExposeData = AccessTools.Method(typeof(DataStorage), nameof(DataStorage.ExposeData));
        var assignLinkExposeData = AccessTools.Method(typeof(AssignLink), nameof(AssignLink.ExposeData));
        var saveCurrentState = AccessTools.Method(typeof(AssignManager), nameof(AssignManager.SaveCurrentState));
        var loadState = AccessTools.Method(typeof(AssignManager), nameof(AssignManager.LoadState), new[] { typeof(List<AssignLink>), typeof(List<Pawn>), typeof(Policy) });

        _ = h.Patch(assignLinkCtor, postfix: new HarmonyMethod(typeof(BPC), nameof(AssignLink_Ctor)));
        _ = h.Patch(worldDataStoreExposeData, postfix: new HarmonyMethod(typeof(BPC), nameof(WorldDataStore_ExposeData_Postfix)));
        _ = h.Patch(assignLinkExposeData, postfix: new HarmonyMethod(typeof(BPC), nameof(AssignLink_ExposeData_Postfix)));
        _ = h.Patch(saveCurrentState, transpiler: new HarmonyMethod(typeof(BPC), nameof(BetterPawnControl_AssignManager_SaveCurrentState)));
        _ = h.Patch(loadState, transpiler: new HarmonyMethod(typeof(BPC), nameof(BetterPawnControl_AssignManager_LoadState)));
        Log.Message("[CombatExtended.ExtendedLoadout] BetterPawnControl patches initialized");
    }

    public static void LoadLoadoutById(Pawn pawn, object assignLink)
    {
        var columns = BPC_AssignLink_Manager.GetColumnsIds(assignLink);
        if (columns != null)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                var loadout = LoadoutManager.Loadouts[columns[i]];
                if (loadout == null)
                    loadout = LoadoutManager.DefaultLoadout;
                pawn.SetLoadout(loadout, i);
            }
        }
    }

    public static void SaveLoadoutId(object assignLink, Pawn pawn)
    {
        var loadoutMulti = (Loadout_Multi)pawn.GetLoadout();
        var columns = loadoutMulti.Loadouts.Select(x => x?.uniqueID ?? LoadoutManager.DefaultLoadout.uniqueID).ToList();
        BPC_AssignLink_Manager.AddColumnsIds(assignLink, columns);
    }

    public class AssignLinkComparer : EqualityComparer<object>
    {
        public override bool Equals(object _x, object _y)
        {
            var x = (AssignLink)_x;
            var y = (AssignLink)_y;
            return x.zone == y.zone && x.colonist == y.colonist && x.mapId == y.mapId;
        }

        public override int GetHashCode(object o)
        {
            var obj = (AssignLink)o;
            return (obj.zone * 1) + (obj.colonist.thingIDNumber * 2) + (obj.mapId * 3);
        }
    }

    // Fix BPC AssignLink list when have dublicated records for some reasons
    public static void WorldDataStore_ExposeData_Postfix()
    {
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            int beforeCount = Manager<AssignLink>.links.Count;
            Manager<AssignLink>.links = Manager<AssignLink>.links.Distinct(new AssignLinkComparer()).Cast<AssignLink>().ToList();
            int afterCount = Manager<AssignLink>.links.Count;
            int delta = beforeCount - afterCount;
            if (delta != 0)
            {
                Log.Warning($"[CombatExtended.ExtendedLoadout] Removed dublicated BPC:AssignLink's: {delta}");
            }
        }
    }

    public static void AssignLink_ExposeData_Postfix(object __instance)
    {
        BPC_AssignLink_Manager.ExposeData(__instance);
    }

    public static void AssignLink_Ctor(object __instance, int zone, Pawn colonist, Outfit outfit, FoodRestriction foodPolicy, DrugPolicy drugPolicy, HostilityResponseMode hostilityResponse, int loadoutId, int mapId)
    {
        DbgLog.Msg($"AssignLink_Ctor => {__instance}");
        SaveLoadoutId(__instance, colonist);
    }

    //public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_SaveCurrentState_DBG(IEnumerable<CodeInstruction> instructions)
    //{
    //    var after = BetterPawnControl_AssignManager_SaveCurrentState(instructions);
    //    File.WriteAllLines("a:\\before_save.txt", instructions.Select(x => x.ToString()));
    //    File.WriteAllLines("a:\\after_save.txt", after.Select(x => x.ToString()));
    //    return after;
    //}

    //public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_LoadState_DBG(IEnumerable<CodeInstruction> instructions)
    //{
    //    var after = BetterPawnControl_AssignManager_LoadState(instructions);
    //    File.WriteAllLines("a:\\before_load.txt", instructions.Select(x => x.ToString()));
    //    File.WriteAllLines("a:\\after_load.txt", after.Select(x => x.ToString()));
    //    return after;
    //}

    public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_LoadState(IEnumerable<CodeInstruction> instructions)
    {
        /*
        82	010B	brfalse.s	87 (011A) ldloca.s V_4 (4)
        83	010D	ldloc.3
        84	010E	ldloc.s	V_5 (5)
        85	0110	ldfld	int32 BetterPawnControl.AssignLink::loadoutId
        86	0115	call	void BetterPawnControl.Widget_CombatExtended::SetLoadoutById(class ['Assembly-CSharp']Verse.Pawn, int32)
         */
        var code = instructions.ToList();
        var setLoadoutById = AccessTools.Method($"BetterPawnControl.Widget_CombatExtended:SetLoadoutById");
        int fixedSetLoadoutId = 0;
        for (int i = 0; i < code.Count; i++)
        {
            if (code[i].opcode == OpCodes.Brfalse_S)
            {
#pragma warning disable 252,253
                if (code[i + 1].opcode == OpCodes.Ldloc_3 && (code[i + 4].opcode == OpCodes.Call && code[i + 4].operand == setLoadoutById))
#pragma warning restore 252,253
                {
                    fixedSetLoadoutId++;
                    yield return code[i++]; // 82	010B	brfalse.s	87 (011A) ldloca.s V_4 (4)
                    yield return code[i++]; // 83	010D	ldloc.3
                    yield return code[i++]; // 84	010E	ldloc.s	V_5 (5)
                    i++; // skip: 85	0110	ldfld	int32 BetterPawnControl.AssignLink::loadoutId
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPC), nameof(LoadLoadoutById)));
                    continue;
                }
            }
            yield return code[i];
        }

        if (fixedSetLoadoutId != 1)
        {
            Log.Error($"Transpiler outdated!");
        }
    }

    public static IEnumerable<CodeInstruction> BetterPawnControl_AssignManager_SaveCurrentState(IEnumerable<CodeInstruction> instructions)
    {
        /*
        51	00AE	call	bool BetterPawnControl.Widget_CombatExtended::get_CombatExtendedAvailable()

        52	00B3	brfalse	121 (01AB) ldloca.s V_1 (1)
        53	00B8	ldloc.3
        54	00B9	ldloc.2
        55	00BA	ldfld	class ['Assembly-CSharp']Verse.Pawn BetterPawnControl.AssignManager/'<>c__DisplayClass21_1'::p
        56	00BF	call	int32 BetterPawnControl.Widget_CombatExtended::GetLoadoutId(class ['Assembly-CSharp']Verse.Pawn)
        57	00C4	stfld	int32 BetterPawnControl.AssignLink::loadoutId
        58	00C9	br	121 (01AB) ldloca.s V_1 (1)
        59	00CE	ldc.i4.0
        60	00CF	stloc.s	V_4 (4)
        61	00D1	call	bool BetterPawnControl.Widget_CombatExtended::get_CombatExtendedAvailable()

        62	00D6	brfalse.s	67 (00E5) ldloc.2 
        63	00D8	ldloc.2
        64	00D9	ldfld	class ['Assembly-CSharp']Verse.Pawn BetterPawnControl.AssignManager/'<>c__DisplayClass21_1'::p
        65	00DE	call	int32 BetterPawnControl.Widget_CombatExtended::GetLoadoutId(class ['Assembly-CSharp']Verse.Pawn)
        66	00E3	stloc.s	V_4 (4)

         */
        var code = instructions.ToList();
        var getLoadoutId = AccessTools.Method($"BetterPawnControl.Widget_CombatExtended:GetLoadoutId");
        int fixedGetLoadoutId = 0;
        for (int i = 0; i < code.Count; i++)
        {
            if (code[i].opcode == OpCodes.Brfalse || code[i].opcode == OpCodes.Brfalse_S)
            {
#pragma warning disable 252,253
                if (code[i + 1].opcode == OpCodes.Ldloc_3 && (code[i + 4].opcode == OpCodes.Call && code[i + 4].operand == getLoadoutId))
#pragma warning restore 252,253
                {
                    fixedGetLoadoutId++;
                    yield return code[i++]; // 52	00B3	brfalse
                    yield return code[i++]; // 53	00B8	ldloc.3 => AssignLink
                    yield return code[i++]; // 54	00B9	ldloc.2
                    yield return code[i++]; // 55	00BA	ldfld => Pawn
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPC), nameof(SaveLoadoutId)));
                    i++; // skip: stfld	int32 BetterPawnControl.AssignLink::loadoutId
                    continue;
                }
#pragma warning disable 252,253
                else if (code[i + 1].opcode == OpCodes.Ldloc_2 && (code[i + 3].opcode == OpCodes.Call && code[i + 3].operand == getLoadoutId))
#pragma warning restore 252,253
                {
                    fixedGetLoadoutId++;
                    yield return code[i++]; // 62	00D6	brfalse.s
                    //yield return new CodeInstruction(OpCodes.Ldloc_3); // => AssignLink
                    //yield return code[i++]; // 63	00D8	ldloc.2
                    //yield return code[i++]; // 64	00D9	ldfld => Pawn
                    //yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BPC), nameof(SaveLoadoutId))); // CALL WHEN AssignLink = NULL
                    //i++; // skip: 66	00E3	stloc.s	V_4 (4)
                    i += 3; // skip: call getLoadoutId
                    continue;
                }
            }
            yield return code[i];
        }

        if (fixedGetLoadoutId != 2)
        {
            Log.Error($"Transpiler outdated!");
        }
    }
}