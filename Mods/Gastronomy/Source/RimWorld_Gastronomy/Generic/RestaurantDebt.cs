using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
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

		public RestaurantDebt([NotNull] RestaurantController restaurant)
		{
			Restaurant = restaurant;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref debts, "debts", LookMode.Deep);
			debts ??= new List<Debt>();
		}

		public void RareTick()
		{
			debts.RemoveAll(o => o.patron == null || o.amount == 0 || o.patron.Dead);
		}

		public void Add(Thing meal, Pawn patron)
		{
			var debt = GetOrCreateDebt(patron);

			debt.amount += meal.def.GetPrice(Restaurant);
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
