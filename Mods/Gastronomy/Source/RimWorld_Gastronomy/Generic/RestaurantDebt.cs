using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Gastronomy
{
	public class RestaurantDebt : IExposable
	{
		private List<Debt> debts = new List<Debt>();

		[NotNull] public ReadOnlyCollection<Debt> AllDebts => debts.AsReadOnly();
		[NotNull] private RestaurantMenu Menu => Restaurant.Menu;
		[NotNull] private RestaurantController Restaurant { get; }
		public float incomeYesterday;
		public float incomeToday;

		public RestaurantDebt([NotNull] RestaurantController restaurant)
		{
			Restaurant = restaurant;
			Restaurant.onNextDay += OnNextDay;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref debts, "debts", LookMode.Deep);
			Scribe_Values.Look(ref incomeToday, "incomeToday");
			Scribe_Values.Look(ref incomeYesterday, "incomeYesterday");
			debts ??= new List<Debt>();
		}

		public void RareTick()
		{
			debts.RemoveAll(o => o.amount <= 0 || !CanHaveDebt(o.patron));
		}

		private void OnNextDay()
		{
			incomeYesterday = incomeToday;
			incomeToday = 0;
			if (incomeYesterday > 0)
			{
				Messages.Message("MessageIncomeToday".Translate(incomeYesterday.ToStringMoney()), MessageTypeDefOf.NeutralEvent);
			}
		}

		public void Add(Thing meal, Pawn patron)
		{
			if (!CanHaveDebt(patron)) return;

			var debt = GetOrCreateDebt(patron);

			var price = meal.def.GetPrice(Restaurant);
			debt.amount += price;
			incomeToday += price;
		}

		private static bool CanHaveDebt(Pawn patron)
		{
			return patron != null && !patron.Dead && patron.Faction?.IsPlayer == false;
		}

		private Debt GetOrCreateDebt(Pawn patron)
		{
			var debt = debts.FirstOrDefault(d => d.patron == patron);
			if (debt == null)
			{
				debt = new Debt {patron = patron};
				debts.Insert(0, debt); // More recent ones on top
			}

			return debt;
		}

		public Debt GetDebt(Pawn patron)
		{
			return debts.FirstOrDefault(d => d.patron == patron);
		}

		public void PayDebt(Pawn patron, int amount)
		{
			var debt = GetOrCreateDebt(patron);
			debt.amount -= amount;
			if (Mathf.Abs(debt.amount) < 1) debt.amount = 0;
		}
	}
}
