using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CombatExtended.ExtendedLoadout;

[HarmonyPatch(typeof(Dialog_ManageLoadouts))]
[HotSwappable]
public static class HideButtons_PersonalLoadout_Patch
{
    static bool Prepare() => ExtendedLoadoutMod.Instance.useMultiLoadouts;

    public static float Height30(Dialog_ManageLoadouts instance)
    {
        if (instance is Dialog_ManageLoadouts_Extended {IsPersonalLoadout: true})
        {
            return 0f;
        }

        return 30f;
    }

    public static float Y42(Dialog_ManageLoadouts instance)
    {
        if (instance is Dialog_ManageLoadouts_Extended {IsPersonalLoadout: true})
        {
            return 0f;
        }

        return 42f;
    }

    public static void DrawNameFieldNew(Dialog_ManageLoadouts instance, Rect canvas)
    {
        if (instance is Dialog_ManageLoadouts_Extended {IsPersonalLoadout: true})
        {
            return;
        }
        string text = GUI.TextField(canvas, instance.CurrentLoadout.label);
        if (Outfit.ValidNameRegex.IsMatch(text))
        {
            instance.CurrentLoadout.label = text;
        }
    }

    [HarmonyPatch(nameof(Dialog_ManageLoadouts.DoWindowContents))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var height30 = AccessTools.Method(typeof(HideButtons_PersonalLoadout_Patch), nameof(Height30)); ;
        var y42 = AccessTools.Method(typeof(HideButtons_PersonalLoadout_Patch), nameof(Y42)); ;
        var sortLoadouts = AccessTools.Method(typeof(LoadoutManager), nameof(LoadoutManager.SortLoadouts));
        var drawNameField = AccessTools.Method(typeof(Dialog_ManageLoadouts), "DrawNameField");
        var drawNameFieldNew = AccessTools.Method(typeof(HideButtons_PersonalLoadout_Patch), nameof(DrawNameFieldNew));
        bool end = false;

        bool buttonHeightPatched = false;
        bool yTopPatched = false;
        foreach (var ci in instructions)
        {
            if (!end && ci.opcode == OpCodes.Ldc_R4 && (float)ci.operand == 30f)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, height30);
                buttonHeightPatched = true;
            }
            else if (!end && ci.opcode == OpCodes.Ldc_R4 && (float)ci.operand == 42f)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, y42);
                yTopPatched = true;
            }
            else if (!end && ci.Calls(sortLoadouts))
            {
                end = true;
                yield return ci;
            }
            else if (ci.Calls(drawNameField))
            {
                yield return new CodeInstruction(OpCodes.Call, drawNameFieldNew);
            }
            else
            {
                yield return ci;
                //File.AppendAllText("a:/dd.txt", ci.ToString() + "\n");
            }
        }

        if (!buttonHeightPatched)
        {
            Log.Error($"buttonHeightPatched false!");
        }
        if (!yTopPatched)
        {
            Log.Error($"yTopPatched false!");
        }
    }
}

/// <summary>
/// Implemented PostClose in Dialog_ManageLoadouts class. Used for update Slots cache in Loadout_Multi
/// </summary>
[HotSwappable]
public class Dialog_ManageLoadouts_Extended : Dialog_ManageLoadouts
{
    private Pawn? _pawn;
    private Loadout? _pawnLoadout;
    private Vector2 _cardSize;

    public Dialog_ManageLoadouts_Extended(Loadout loadout) : base(loadout)
    {
    }

    public Dialog_ManageLoadouts_Extended(Pawn pawn, Loadout loadout) : base(loadout)
    {
        _pawn = pawn;
        _pawnLoadout = loadout;
        _cardSize = CharacterCardUtility.PawnCardSize(pawn);
        DbgLog.Msg($"cardSize: {_cardSize}");
    }

    public bool IsPersonalLoadout => _pawn != null;

    public override Vector2 InitialSize
    {
        get
        {
            var ceSize = base.InitialSize;
            return _pawn == null ? ceSize : new Vector2(ceSize.x + _cardSize.x + 50f, Mathf.Max(ceSize.y, _cardSize.y));
        }
    }

    public override void DoWindowContents(Rect canvas)
    {
        if (_pawn != null)
        {
            var ceSize = base.InitialSize;
            var cardRect = canvas.RightPartPixels(_cardSize.x);//new Rect(canvas.x + ceSize.x, canvas.y + ceSize.y, _cardSize.x, _cardSize.y);
            CharacterCardUtility.DrawCharacterCard(cardRect, _pawn);
            canvas = canvas.LeftPartPixels(ceSize.x);
             DbgLog.Msg($"ceSize: {ceSize}, cardRect: {cardRect}, pawn: {_pawn}");

            // reset selected pawn after change loadout
            if (CurrentLoadout != _pawnLoadout)
            {
                _pawn = null;
            }
        }

        base.DoWindowContents(canvas);
    }

    /// <summary>
    /// Notify all loadouts multi Dialog_ManageLoadouts closed
    /// </summary>
    public override void PostClose()
    {
        base.PostClose();

        foreach (var loadoutMulti in LoadoutMulti_Manager.LoadoutsMulti)
        {
            loadoutMulti.NotifyLoadoutChanged();
        }
    }
}