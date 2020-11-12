using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace CommonSense
{
    public class CompUnloadChecker : ThingComp
    {
        public bool ShouldUnload = false;
        public bool WasInInventory = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            if (parent.ParentHolder != null)
            {
                Scribe_Values.Look(ref ShouldUnload, "CommonSenseShouldUnload", defaultValue: false);
                Scribe_Values.Look(ref WasInInventory, "CommonSenseWasInInventory", defaultValue: false);
            }
        }
        public void Init(bool AShouldUnload, bool AWasInInventory)
        {
            ShouldUnload = AShouldUnload;
            WasInInventory = AWasInInventory;
        }

        static public CompUnloadChecker GetChecker(Thing thing, bool InitShouldUnload = false, bool InitWasInInventory = false)
        {
            
            if (!(thing is ThingWithComps) && !thing.GetType().IsSubclassOf(typeof(ThingWithComps)))
                return null;
            ThingWithComps TWC = (ThingWithComps)thing;
            if (TWC.AllComps == null)
                return null;
            CompUnloadChecker thingComp = thing.TryGetComp<CompUnloadChecker>();
            if (thingComp == null)
            {
                thingComp = (CompUnloadChecker)Activator.CreateInstance(typeof(CompUnloadChecker));
                thingComp.parent = TWC;
                TWC.AllComps.Add(thingComp);
            }
            thingComp.ShouldUnload = thingComp.ShouldUnload || InitShouldUnload;
            thingComp.WasInInventory = thingComp.WasInInventory || InitWasInInventory;
            return thingComp;
        }

        public static Thing getFirstMarked(Pawn pawn)
        {
            Thing t = null;
            if(pawn.inventory != null) t = pawn.inventory.innerContainer.FirstOrDefault(x => x.TryGetComp<CompUnloadChecker>() != null && x.TryGetComp<CompUnloadChecker>().ShouldUnload);
            if (!Settings.gui_manual_unload) return t;
            if (t == null && pawn.equipment != null) t = pawn.equipment.AllEquipmentListForReading.FirstOrDefault(x => x.TryGetComp<CompUnloadChecker>() != null && x.TryGetComp<CompUnloadChecker>().ShouldUnload);
            if (t == null && pawn.apparel != null) t = pawn.apparel.WornApparel.FirstOrDefault(x => x.TryGetComp<CompUnloadChecker>() != null && x.TryGetComp<CompUnloadChecker>().ShouldUnload);
            return t;
        }
    }

    //obsolete now, but left in case
    [HarmonyPatch(typeof(GenDrop), nameof(GenDrop.TryDropSpawn))]
    static class GenPlace_TryDropSpawn_CommonSensePatch
    {

        static void Postfix(Thing thing, IntVec3 dropCell, Map map, ThingPlaceMode mode, Thing resultingThing, Action<Thing, int> placedAction, Predicate<IntVec3> nearPlaceValidator)
        {
            CompUnloadChecker UChecker = resultingThing.TryGetComp<CompUnloadChecker>();
            if (UChecker != null)
            {
                UChecker.WasInInventory = false;
                UChecker.ShouldUnload = false;
            }

        }
    }

    [HarmonyPatch(typeof(GenDrop), nameof(GenDrop.TryDropSpawn_NewTmp))]
    static class GenPlace_TryDropSpawn_NewTmp_CommonSensePatch
    {

        static void Postfix(Thing thing, IntVec3 dropCell, Map map, ThingPlaceMode mode, Thing resultingThing, Action<Thing, int> placedAction, Predicate<IntVec3> nearPlaceValidator)
        {
            CompUnloadChecker UChecker = resultingThing.TryGetComp<CompUnloadChecker>();
            if (UChecker != null)
            {
                UChecker.WasInInventory = false;
                UChecker.ShouldUnload = false;
            }

        }
    }

    [HarmonyPatch(typeof(ThingWithComps), "ExposeData")]
    static class ThingWithComps_ExposeData_CommonSensePatch
    {
        static void Postfix(ThingWithComps __instance)
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                bool a = false;
                bool b = false;
                Scribe_Values.Look(ref a, "CommonSenseShouldUnload", defaultValue: false);
                Scribe_Values.Look(ref b, "CommonSenseWasInInventory", defaultValue: false);
                if (a || b)
                    CompUnloadChecker.GetChecker(__instance, a, b);
            }
        }
    }

    [HarmonyPatch(typeof(ThingOwner<Thing>), "TryAdd", new Type[] { typeof(Thing), typeof(bool) })]
    static class ThingOwnerThing_TryAdd_CommonSensePatch
    {
        static void Postfix(ThingOwner<Thing> __instance, bool __result, Thing item)
        {
            if (!__result || item.Destroyed || item.stackCount == 0)
                return;

            if (__instance.Owner is Pawn_InventoryTracker)
            {
                CompUnloadChecker.GetChecker(__instance[__instance.Count - 1], false, true);
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_UnloadYourInventory), "TryGiveJob", new Type[] { typeof(Pawn) })]
    static class JobGiver_UnloadYourInventory_TryGiveJob_CommonSensePatch
    {
        static bool Prefix(ref Job __result, ref JobGiver_UnloadYourInventory __instance, ref Pawn pawn)
        {
            Thing thing = CompUnloadChecker.getFirstMarked(pawn);
            if (thing != null)
            {
                __result = new Job(CommonSenseJobDefOf.UnloadMarkedItems);
                return false;
            }
            return true;
        }
    }

    [DefOf]
    public static class CommonSenseJobDefOf
    {
        public static JobDef UnloadMarkedItems;
    }

    //slightly modified UnloadYourInventory
    public class JobDriver_UnloadMarkedItems : JobDriver
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.countToDrop, "countToDrop", -1, false);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        Apparel Apparel = null;
        ThingWithComps Equipment = null;
        float ticker = 0;
        float duration = 0;


        static bool stillUnloadable(Thing thing)
        {
            CompUnloadChecker c = thing.TryGetComp<CompUnloadChecker>();
            return c != null && c.ShouldUnload;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_General.Wait(10, TargetIndex.None);
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Thing MarkedThing = CompUnloadChecker.getFirstMarked(pawn);
                    if (MarkedThing == null)
                    {
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }
                    //
                    if (pawn.equipment.Contains(MarkedThing))
                    {
                        Equipment = (ThingWithComps)MarkedThing;
                        Apparel = null;
                    }
                    else
                    {
                        Apparel = pawn.apparel.Contains(MarkedThing) ? (Apparel)MarkedThing : null;
                        Equipment = null;
                    }

                    ThingCount firstUnloadableThing = MarkedThing == null ? default(ThingCount) : new ThingCount(MarkedThing, MarkedThing.stackCount);
                    IntVec3 c;
                    if (!StoreUtility.TryFindStoreCellNearColonyDesperate(firstUnloadableThing.Thing, pawn, out c))
                    {
                        Thing thing;
                        pawn.inventory.innerContainer.TryDrop(firstUnloadableThing.Thing, ThingPlaceMode.Near, firstUnloadableThing.Count, out thing, null, null);
                        EndJobWith(JobCondition.Succeeded);
                        return;
                    }

                    job.SetTarget(TargetIndex.A, firstUnloadableThing.Thing);
                    job.SetTarget(TargetIndex.B, c);
                    countToDrop = firstUnloadableThing.Count;
                }
            };
            yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.Touch).FailOnDestroyedOrNull(TargetIndex.A).FailOn(delegate () { return !stillUnloadable(pawn.CurJob.GetTarget(TargetIndex.A).Thing); });

            //preintiating unequip-delay
            Toil unequip = new Toil
            {
                initAction = delegate ()
                {
                    if (Equipment != null)
                        pawn.equipment.TryTransferEquipmentToContainer(Equipment, pawn.inventory.innerContainer);
                    else if (Apparel != null)
                    {
                        ThingOwner<Apparel> a = Traverse.Create(pawn.apparel).Field("wornApparel").GetValue<ThingOwner<Apparel>>();
                        a.TryTransferToContainer(Apparel, pawn.inventory.innerContainer);
                    }
                }
            };
            //if equiped, wait unequipping time
            Toil wait = new Toil();
            wait.initAction = delegate ()
            {
                ticker = 0;
                duration = Apparel != null ? Apparel.GetStatValue(StatDefOf.EquipDelay, true) * 60f : Equipment != null ? 30 : 0;
                pawn.pather.StopDead();
            };
            wait.tickAction = delegate ()
            {
                if(ticker >= duration) ReadyForNextToil();
                ticker++;
            };
            wait.defaultCompleteMode = ToilCompleteMode.Never;
            wait.WithProgressBar(TargetIndex.A, () => ticker / duration);
            //unequip to inventory
            yield return wait;
            yield return unequip;
            //hold in hands
            yield return new Toil
            {
                initAction = delegate ()
                {
                    Thing thing = job.GetTarget(TargetIndex.A).Thing;
                    CompUnloadChecker c = thing.TryGetComp<CompUnloadChecker>();
                    if (c == null || !c.ShouldUnload)
                    {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                    if (thing == null || !pawn.inventory.innerContainer.Contains(thing))
                    {
                        EndJobWith(JobCondition.Incompletable);
                        return;
                    }
                    if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || !thing.def.EverStorable(false))
                    {
                        pawn.inventory.innerContainer.TryDrop(thing, ThingPlaceMode.Near, countToDrop, out thing, null, null);
                        EndJobWith(JobCondition.Succeeded);
                    }
                    else
                    {
                        pawn.inventory.innerContainer.TryTransferToContainer(thing, pawn.carryTracker.innerContainer, countToDrop, out thing, true);
                        job.count = countToDrop;
                        job.SetTarget(TargetIndex.A, thing);
                    }
                    thing.SetForbidden(false, false);
                }
            };
            Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.B).FailOnDestroyedOrNull(TargetIndex.A).FailOn(delegate() { return !stillUnloadable(pawn.CurJob.GetTarget(TargetIndex.A).Thing);  } );
            yield return carryToCell;
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, carryToCell, true);
            yield break;
        }

        private int countToDrop = -1;
        private const TargetIndex ItemToHaulInd = TargetIndex.A;
        private const TargetIndex StoreCellInd = TargetIndex.B;
        private const int UnloadDuration = 10;
    }


    //private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
    [HarmonyPatch(typeof(ITab_Pawn_Gear), "DrawThingRow")]
    public static class ITab_Pawn_Gear_DrawThingRow_CommonSensePatch
    {
        static readonly Color hColor = new Color(1f, 0.8f, 0.8f, 1f);

        static bool IsBiocodedOrLinked(Pawn pawn, Thing thing, bool inventory)
        {
            if (pawn.IsQuestLodger())
            {
                if (inventory)
                {
                    return true;
                }
                else
                {
                    CompBiocodable compBiocodable = thing.TryGetComp<CompBiocodable>();
                    if (compBiocodable != null && compBiocodable.Biocoded)
                    {
                        return true;
                    }
                    else
                    {
                        CompBladelinkWeapon compBladelinkWeapon = thing.TryGetComp<CompBladelinkWeapon>();
                        return (compBladelinkWeapon != null && compBladelinkWeapon.bondedPawn == pawn);
                    }
                }
            }
            else
            {
                return false;
            }
        }

        static bool IsLocked(Pawn pawn, Thing thing)
        {
            Apparel apparel;
            return (apparel = (thing as Apparel)) != null && pawn.apparel != null && pawn.apparel.IsLocked(apparel);
        }

        public static bool Prefix(ITab_Pawn_Gear __instance, ref float y, ref float width, Thing thing, bool inventory)
        {
            if (!Settings.gui_manual_unload)
                return true;

            bool CanControl = Traverse.Create(__instance).Property("CanControl").GetValue<bool>();
            Pawn SelPawnForGear = Traverse.Create(__instance).Property("SelPawnForGear").GetValue<Pawn>();
            Rect rect = new Rect(0f, y, width, 28f);

            if (CanControl 
                && (SelPawnForGear.IsColonistPlayerControlled ||  SelPawnForGear.Spawned && !SelPawnForGear.Map.IsPlayerHome) 
                && (thing is ThingWithComps)
                && !IsBiocodedOrLinked(SelPawnForGear, thing, inventory)
                && !IsLocked(SelPawnForGear, thing))
            {
                Rect rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
                CompUnloadChecker c = CompUnloadChecker.GetChecker(thing,false,true);
                if (c.ShouldUnload)
                {
                    TooltipHandler.TipRegion(rect2, "UnloadThingCancel".Translate());

                    //weird shenanigans with colors
                    var cl = GUI.color;
                    if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Icons/Unload_Thing_Cancel"), hColor))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        c.ShouldUnload = false;

                        if (MassUtility.Capacity(SelPawnForGear, null) < MassUtility.GearAndInventoryMass(SelPawnForGear) 
                            && thing.stackCount * thing.GetStatValue(StatDefOf.Mass, true) > 0 
                            && !thing.def.destroyOnDrop)
                        {
                            Thing t;
                            SelPawnForGear.inventory.innerContainer.TryDrop(thing, SelPawnForGear.Position, SelPawnForGear.Map, ThingPlaceMode.Near, out t, null, null);
                        }
                    }
                    GUI.color = cl;
                }
                else
                {
                    TooltipHandler.TipRegion(rect2, "UnloadThing".Translate());
                    if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Icons/Unload_Thing")))
                    {
                        SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
                        c.ShouldUnload = true;
                    }
                }
                width -= 24f;
            }
            return true;
        }
    }
}