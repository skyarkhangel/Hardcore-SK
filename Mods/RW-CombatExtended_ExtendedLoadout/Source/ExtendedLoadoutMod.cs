using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class HotSwappableAttribute : Attribute
{
}

[StaticConstructorOnStartup]
public static class EarlyInit
{
    static EarlyInit()
    {
        /*
         * Fix for mod: Rim of Madness - Vampires
         * After added postfix from vampires on method JobGiver_UpdateLoadout:TryGiveJob
         * 
         * Need to patch it first, before other mods patch the methods where Loadout::Slots is used
         */
        LoadoutProxy_Patch.Patch();
    }
}

public class ExtendedLoadoutMod : ModBase
{
    public static ExtendedLoadoutMod Instance = null!;

    public const int MaxColumnCount = 10;

    // indicate need patches or not
    public bool useMultiLoadouts, useHpAndQualityInLoadouts;

    protected override bool HarmonyAutoPatch => false;

    private readonly SettingHandle<string>[] loadoutNames = new SettingHandle<string>[MaxColumnCount];

    public static readonly string HarmonyId = "PirateBY.CombatExtended.ExtendedLoadout";
    public static Harmony Harmony => _harmony ??= new Harmony(HarmonyId);

    private static Harmony? _harmony;

    public ExtendedLoadoutMod()
    {
        Instance = this;
    }

    public override string ModIdentifier => "CombatExtended.ExtendedLoadout";

    public override void DefsLoaded()
    {
        // init settings
        ModSettingsPack modSettingsPack = HugsLibController.Instance.Settings.GetModSettings("CombatExtended.ExtendedLoadout");
        SettingHandle<bool> UseHpAndQualityInLoadouts = modSettingsPack.GetHandle("UseHpAndQualityInLoadouts", "Settings.UseHpAndQualityInLoadouts.Label".Translate(), "Settings.UseHpAndQualityInLoadouts.Desc".Translate(), true);
        SettingHandle<bool> UseMultiLoadouts = modSettingsPack.GetHandle("UseMultiLoadouts", "Settings.UseMultiLoadouts.Label".Translate(), "Settings.UseMultiLoadouts.Desc".Translate(), true);
        SettingHandle<int> MultiLoadoutsCount = modSettingsPack.GetHandle("MultiLoadoutsCount", "Settings.MultiLoadoutsCount.Label".Translate(), "Settings.MultiLoadoutsCount.Desc".Translate(), 3, value => int.TryParse(value, out int num) && num is >= 2 and <= 10);
        MultiLoadoutsCount.VisibilityPredicate = () => UseMultiLoadouts;

        // column names settings
        for (int i = 0; i < MaxColumnCount; i++)
        {
            int colId = i;
            loadoutNames[i] = modSettingsPack.GetHandle($"LoadoutName_{i}", $"Loadout{i + 1}".Translate(), "Settings.LoadoutName.Desc".Translate(), $"Loadout{i + 1}".Translate().RawText);
            loadoutNames[i].VisibilityPredicate = () => (bool)UseMultiLoadouts && colId < (int)MultiLoadoutsCount;
            ((SettingHandle)loadoutNames[i]).ValueChanged += ((Action<SettingHandle>)delegate
            {
                PawnColumnDef pawnColumnDef = Enumerable.FirstOrDefault(DefDatabase<PawnTableDef>.GetNamed("Assign").columns, (PawnColumnDef c) => c.defName.Equals($"Loadout_{colId}"));
                if (pawnColumnDef != null)
                {
                    pawnColumnDef.label = loadoutNames[colId].Value;
                    ((Def)pawnColumnDef).ClearCachedData();
                }
            });
        }

        // inject columns and set settings  
        useHpAndQualityInLoadouts = UseHpAndQualityInLoadouts;
        if (UseMultiLoadouts && MultiLoadoutsCount >= 2 && MultiLoadoutsCount <= MaxColumnCount)
        {
            List<PawnColumnDef> columns = DefDatabase<PawnTableDef>.GetNamed("Assign").columns;
            int idx = columns.FindIndex((PawnColumnDef x) => x.defName.Equals("Loadout"));
            if (idx != -1)
            {
                IEnumerable<PawnColumnDef> columnDefs = GeneratePawnColumnDefs(MultiLoadoutsCount);
                DefDatabase<PawnColumnDef>.Add(columnDefs);
                columns.RemoveAt(idx);
                columns.InsertRange(idx, columnDefs);
                Loadout_Multi.ColumnsCount = MultiLoadoutsCount;
                useMultiLoadouts = true;
                Log.Message($"[CombatExtended.ExtendedLoadout] {MultiLoadoutsCount}x Loadout columns injected");
            }
            else
            {
                Log.Error($"[CombatExtended.ExtendedLoadout] Can't find CE Loadout column");
            }
        }

        // apply patches
        if (useMultiLoadouts && ModActive.BetterPawnControl)
        {
            Log.Warning("BPC Is not supported with CE Extended Loadout currently");
            //BPC.Patch(Harmony);
        }

        Harmony.PatchAll();

        if (!useMultiLoadouts) // disable unused patch
        {
            LoadoutProxy_Patch.Unpatch();
        }
        // add generic defs
        MedicineDefs.Initialize();

        Log.Message("[CombatExtended.ExtendedLoadout] Initialized");
    }

    private IEnumerable<PawnColumnDef> GeneratePawnColumnDefs(int count)
    {
        // CE_UpdateLoadoutNow
        yield return new PawnColumnDef()
        {
            defName = $"CE_UpdateLoadoutNow",
            workerClass = typeof(PawnColumnWorker_UpdateLoadoutNow),
            label = "CE_UpdateLoadoutNow".Translate(),
            sortable = false
        };
        // Loadout columns
        for (int i = 0; i < count; i++)
        {
            yield return new PawnColumnDef()
            {
                defName = $"Loadout_{i}",
                workerClass = typeof(PawnColumnWorker_Loadout_Multi),
                label = loadoutNames[i],
                sortable = true
            };
        }
    }
}