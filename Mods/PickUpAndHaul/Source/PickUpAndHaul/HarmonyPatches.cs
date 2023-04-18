using System.Reflection;
using HarmonyLib;

namespace PickUpAndHaul;
[StaticConstructorOnStartup]
static class HarmonyPatches
{
	static HarmonyPatches()
	{
		var harmony = new Harmony("mehni.rimworld.pickupandhaul.main");
#if DEBUG
		Harmony.DEBUG = true;
#endif

		if (!ModCompatibilityCheck.CombatExtendedIsActive)
		{
			harmony.Patch(original: AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.GetMaxAllowedToPickUp), new[] { typeof(Pawn), typeof(ThingDef) }),
				prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MaxAllowedToPickUpPrefix)));

			harmony.Patch(original: AccessTools.Method(typeof(PawnUtility), nameof(PawnUtility.CanPickUp)),
				prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CanBeMadeToDropStuff)));
		}

		harmony.Patch(original: AccessTools.Method(typeof(JobGiver_DropUnusedInventory), nameof(JobGiver_DropUnusedInventory.TryGiveJob)),
			postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DropUnusedInventory_PostFix)));

		harmony.Patch(original: AccessTools.Method(typeof(JobDriver_HaulToCell), nameof(JobDriver_HaulToCell.MakeNewToils)),
			postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(JobDriver_HaulToCell_PostFix)));

		harmony.Patch(original: AccessTools.Method(typeof(Pawn_InventoryTracker), nameof(Pawn_InventoryTracker.Notify_ItemRemoved)),
			postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Pawn_InventoryTracker_PostFix)));

		harmony.Patch(original: AccessTools.Method(typeof(JobGiver_DropUnusedInventory), nameof(JobGiver_DropUnusedInventory.Drop)),
			prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(Drop_Prefix)));

		harmony.Patch(original: AccessTools.Method(typeof(JobGiver_Idle), nameof(JobGiver_Idle.TryGiveJob)),
			postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IdleJoy_Postfix)));

		harmony.Patch(original: AccessTools.Method(typeof(ITab_Pawn_Gear), nameof(ITab_Pawn_Gear.DrawThingRow)),
			transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(GearTabHighlightTranspiler)));

		harmony.Patch(original: AccessTools.Method(typeof(WorkGiver_Haul), nameof(WorkGiver_Haul.ShouldSkip)),
			prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SkipCorpses_Prefix)));

		harmony.Patch(AccessTools.Method(typeof(JobGiver_Haul), nameof(JobGiver_Haul.TryGiveJob)),
			transpiler: new(typeof(HarmonyPatches), nameof(JobGiver_Haul_TryGiveJob_Transpiler)));

		Verse.Log.Message("PickUpAndHaul v0.1.3.2⅔ welcomes you to RimWorld with pointless logspam.");
	}

	private static bool Drop_Prefix(Pawn pawn, Thing thing)
	{
		var takenToInventory = pawn.GetComp<CompHauledToInventory>();
		if (takenToInventory == null)
		{
			return true;
		}

		var carriedThing = takenToInventory.GetHashSet();
		return !carriedThing.Contains(thing);
	}

	private static void Pawn_InventoryTracker_PostFix(Pawn_InventoryTracker __instance, Thing item)
	{
		var takenToInventory = __instance.pawn?.GetComp<CompHauledToInventory>();
		if (takenToInventory == null)
		{
			return;
		}

		var carriedThing = takenToInventory.GetHashSet();
		if (carriedThing?.Count > 0)
		{
			carriedThing.Remove(item);
		}
	}

	private static void JobDriver_HaulToCell_PostFix(JobDriver_HaulToCell __instance)
	{
		var pawn = __instance.pawn;
		var takenToInventory = pawn?.GetComp<CompHauledToInventory>();
		if (takenToInventory == null)
		{
			return;
		}

		var carriedThing = takenToInventory.GetHashSet();

		if (__instance.job.haulMode == HaulMode.ToCellStorage
			&& pawn.Faction == Faction.OfPlayerSilentFail
			&& Settings.IsAllowedRace(pawn.RaceProps)
			&& (Settings.AllowCorpses || pawn.carryTracker.CarriedThing is not Corpse)
			&& carriedThing != null
			&& carriedThing.Count != 0) //deliberate hauling job. Should unload.
		{
			PawnUnloadChecker.CheckIfPawnShouldUnloadInventory(pawn, true);
		}
	}

	public static void IdleJoy_Postfix(Pawn pawn) => PawnUnloadChecker.CheckIfPawnShouldUnloadInventory(pawn, true);

	public static void DropUnusedInventory_PostFix(Pawn pawn) => PawnUnloadChecker.CheckIfPawnShouldUnloadInventory(pawn);

	public static bool MaxAllowedToPickUpPrefix(Pawn pawn, ref int __result)
	{
		__result = int.MaxValue;
		return pawn.IsQuestLodger();
	}

	public static bool CanBeMadeToDropStuff(Pawn pawn, ref bool __result)
	{
		__result = !pawn.IsQuestLodger();
		return false;
	}

	public static bool SkipCorpses_Prefix(WorkGiver_Haul __instance, ref bool __result, Pawn pawn)
	{
		if (__instance is not WorkGiver_HaulCorpses)
		{
			return true;
		}

		if (Settings.AllowCorpses //Don't use the vanilla HaulCorpses WorkGiver if PUAH is allowed to haul those
			|| pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).Count < 1) //...or if there are no corpses to begin with. Indeed Tynan did not foresee this situation
		{
			__result = true;
			return false;
		}

		return true;
	}

	/// <summary>
	/// For animal hauling
	/// </summary>
	public static IEnumerable<CodeInstruction> JobGiver_Haul_TryGiveJob_Transpiler(IEnumerable<CodeInstruction> instructions)
		=> instructions.MethodReplacer(HaulAIUtility.HaulToStorageJob, HaulToStorageJobByRace);

	public static Job HaulToStorageJobByRace(Pawn p, Thing t) => Settings.IsAllowedRace(p.RaceProps) ? HaulToInventoryJob(p, t, false) : HaulAIUtility.HaulToStorageJob(p, t);
	private static Func<Pawn, Thing, bool, Job> HaulToInventoryJob => _haulToInventoryJob ??= new(((WorkGiver_Scanner)DefDatabase<WorkGiverDef>.GetNamed("HaulToInventory").Worker).JobOnThing);
	private static Func<Pawn, Thing, bool, Job> _haulToInventoryJob;

	//ITab_Pawn_Gear
	//private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
	public static IEnumerable<CodeInstruction> GearTabHighlightTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase method)
	{
		var ColorWhite = AccessTools.PropertyGetter(typeof(Color), nameof(Color.white));

		var done = false;
		foreach (var i in instructions)
		{
			//// Color color = flag ? Color.grey : Color.white;
			if (!done && i.Calls(ColorWhite))
			{
				yield return FishTranspiler.This;
				yield return FishTranspiler.CallPropertyGetter(typeof(ITab_Pawn_Gear), nameof(ITab_Pawn_Gear.SelPawnForGear));
				yield return FishTranspiler.Argument(method, "thing");
				yield return FishTranspiler.Call(GetColorForHauled);
				done = true;
			}
			else
			{
				yield return i;
			}
		}

		if (!done)
		{
			Verse.Log.Warning("Pick Up And Haul failed to patch ITab_Pawn_Gear.DrawThingRow. This is only used for coloring and totally harmless, but you might wanna know anyway");
		}
	}

	private static Color GetColorForHauled(Pawn pawn, Thing thing)
		=> pawn.GetComp<CompHauledToInventory>()?.GetHashSet().Contains(thing) ?? false
		? Color.Lerp(Color.grey, Color.red, 0.5f)
		: Color.white;
}