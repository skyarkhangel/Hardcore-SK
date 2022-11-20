using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality
{
    public class CompVendingMachine : ThingComp, IThingHolder
    {
        private bool isActive;

        private int basePrice = 10;

        private ThingOwner<Thing> silverContainer;

        private Vendable vendable;
        private Gizmo_VendingMachine gizmo;

        public override void PostPostMake()
        {
            base.PostPostMake();
            vendable = parent;
            CurrentPrice = GetDefaultPrice();
        }

        private int GetDefaultPrice()
        {
            // Try comp properties first
            var propPrice = ((CompProperties_VendingMachine)props).defaultPrice;
            if (propPrice > 0) return propPrice;
            // Otherwise get it from vendable
            return vendable.DefaultPrice;
        }

        public ThingOwner<Thing> MainContainer => silverContainer ??= new ThingOwner<Thing>(this, false);

        public int CurrentPrice
        {
            get => basePrice;
            set => SetPrice(value);
        }

        internal void SetPrice(int value)
        {
            basePrice = Mathf.Clamp(value, 0, int.MaxValue);
        }

        public bool IsFree => CurrentPrice == 0;
        public bool ShouldEmpty => MainContainer.Count > 0;
        public int TotalSold => MainContainer.TotalStackCount;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isActive, "isActive");
            Scribe_Values.Look(ref basePrice, "basePrice");
            Scribe_Deep.Look(ref silverContainer, "silverContainer");
            vendable = parent;
        }

        internal void SetAutoPricing()
        {
            CurrentPrice = GetDefaultPrice();
        }

        public bool IsActive() => isActive;

        public void ReceivePayment(ThingOwner<Thing> inventoryContainer, Thing silver)
        {
            inventoryContainer.TryTransferToContainer(silver, MainContainer, Mathf.CeilToInt(CurrentPrice));
        }

        public bool CanAffordFast(Pawn buyerGuest, out Thing silver)
        {
            silver = buyerGuest.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
            if (silver == null) return IsFree;
            return silver.stackCount >= CurrentPrice;
        }

        public bool CanBeUsedBy(Pawn pawn)
        {
            return isActive && CanAffordFast(pawn, out _);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(!Properties.noToggle)
                yield return new Command_Toggle
                {
                    defaultLabel = "Hospitality_VendingMachine".Translate(),
                    defaultDesc = "Hospitality_VendingMachineToggleDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/VendingMachine"),
                    isActive = IsActive,
                    toggleAction = () => ToggleActive(),
                    disabled = false,
                };

            if (isActive || Properties.noToggle)
            {
                yield return gizmo ??= new Gizmo_VendingMachine(new[] { this });
            }
        }

        public bool ToggleActive()
        {
            return isActive = !isActive;
        }

        public CompProperties_VendingMachine Properties => (CompProperties_VendingMachine)props;

        public void GetChildHolders(List<IThingHolder> outChildren) { }

        public ThingOwner GetDirectlyHeldThings() => MainContainer;
    }

    public class CompProperties_VendingMachine : CompProperties
    {
        public int defaultPrice = 0;
        public bool noToggle = false;
        public int priceSteps = 5;

        public CompProperties_VendingMachine()
        {
            compClass = typeof(CompVendingMachine);
        }
    }

    /// <summary>
    /// Wrapper for buildings that can be sold to guests
    /// </summary>
    public class Vendable
    {
        private readonly Func<int> getterPrice;
        public int DefaultPrice => getterPrice?.Invoke() ?? 0;

        public Vendable(Func<int> getterPrice)
        {
            this.getterPrice = getterPrice;
        }

        public static implicit operator Vendable(ThingWithComps thing) => ConvertToVendable(thing);

        private static Vendable ConvertToVendable(ThingWithComps thing)
        {
            if (thing is Building_NutrientPasteDispenser dispenser)
                return new Vendable(() => Mathf.CeilToInt(dispenser.DispensableDef.BaseMarketValue));

            return new Vendable(null);
        }
    }
}
