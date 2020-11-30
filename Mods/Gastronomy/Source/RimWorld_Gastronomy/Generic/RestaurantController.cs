using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Gastronomy.Dining;
using Gastronomy.Timetable;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Gastronomy
{
	public class RestaurantController : MapComponent
	{
		public RestaurantController(Map map) : base(map) { }

		[NotNull] public readonly List<DiningSpot> diningSpots = new List<DiningSpot>();
		[NotNull] private readonly List<Pawn> spawnedDiningPawnsResult = new List<Pawn>();
		private RestaurantMenu menu;
		private RestaurantOrders orders;
		private RestaurantDebt debts;
		private RestaurantStock stock;

		public bool IsOpenedRightNow => openForBusiness && timetableOpen.CurrentAssignment(map);
		public bool openForBusiness = true;

		public bool allowGuests = true;
		public bool allowColonists = true;

		public float guestPricePercentage = 0.5f;

		public TimetableBool timetableOpen;

		public int Seats => diningSpots.Sum(s => s.GetMaxSeats());
		[NotNull] public ReadOnlyCollection<Pawn> Patrons => SpawnedDiningPawns.AsReadOnly();
		[NotNull] public RestaurantMenu Menu => menu;
		[NotNull] public RestaurantOrders Orders => orders;
		[NotNull] public RestaurantDebt Debts => debts;
		[NotNull] public RestaurantStock Stock => stock;
		[NotNull] public List<Pawn> SpawnedDiningPawns
		{
			get
			{
				spawnedDiningPawnsResult.Clear();
				spawnedDiningPawnsResult.AddRange(map.mapPawns.AllPawnsSpawned.Where(pawn => pawn.jobs?.curDriver is JobDriver_Dine));
				return spawnedDiningPawnsResult;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref openForBusiness, "openForBusiness", true);
			Scribe_Values.Look(ref allowGuests, "allowGuests", true);
			Scribe_Values.Look(ref allowColonists, "allowColonists", true);
			Scribe_Deep.Look(ref timetableOpen, "timetableOpen");
			Scribe_Deep.Look(ref menu, "menu");
			Scribe_Deep.Look(ref stock, "stock", this);
			Scribe_Deep.Look(ref orders, "orders", this);
			Scribe_Deep.Look(ref debts, "debts", this);
			InitDeepFieldsInitial();
		}

		private void InitDeepFieldsInitial()
		{
			timetableOpen ??= new TimetableBool();
			menu ??= new RestaurantMenu();
			orders ??= new RestaurantOrders(this);
			debts ??= new RestaurantDebt(this);
			stock ??= new RestaurantStock(this);
		}

		public override void MapGenerated()
		{
			InitDeepFieldsInitial();
		}

		public override void FinalizeInit()
		{
			base.FinalizeInit();

			InitDeepFieldsInitial();

			diningSpots.Clear();
			diningSpots.AddRange(DiningUtility.GetAllDiningSpots(map));
			stock.RareTick();
			orders.RareTick();
			debts.RareTick();
		}

		public override void MapComponentTick()
		{
			// Don't tick everything at once
			if ((GenTicks.TicksGame + map.uniqueID) % 500 == 0) stock.RareTick();
			if ((GenTicks.TicksGame + map.uniqueID) % 500 == 250) orders.RareTick();
			//Log.Message($"Stock: {stock.Select(s => s.def.label).ToCommaList(true)}");
		}

		public bool CanDineHere(Pawn pawn)
		{
			if (!IsOpenedRightNow) return false;

			//var isPrisoner = pawn.IsPrisoner;
			var isGuest = pawn.IsGuest();
			var isColonist = pawn.IsColonist;

			if (!allowColonists && isColonist) return false;
			if (!allowGuests && isGuest) return false;
			
			return true;
		}
	}
}
