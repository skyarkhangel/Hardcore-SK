using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Hospitality.Utilities
{
	public class RelationsCache
	{
		private Map Map { get; }
		[NotNull]
		public readonly List<Pawn> colonistsFromBase = new List<Pawn>();
		private readonly Dictionary<Pawn, RelationsCacheSet> allCacheSets = new Dictionary<Pawn, RelationsCacheSet>();

		private int lastUpdateTick;

		public int friendsRequired;

		public RelationsCache(Map map)
		{
			Map = map;
		}

		[NotNull]
		public RelationsCacheSet GetSetFor(Pawn pawn)
		{
			// General stats
			 TryUpdate();

			// Specific stats
			if (!allCacheSets.TryGetValue(pawn, out var set)) allCacheSets.Add(pawn, set = new RelationsCacheSet(pawn));

			set.TryUpdate();
			return set;
		}

		private void TryUpdate()
		{
			if (Current.Game.tickManager.TicksGame <= lastUpdateTick + 50) return;
			lastUpdateTick = Current.Game.tickManager.TicksGame;

			friendsRequired = GetFriendsRequired();
			UpdateColonistsFromBase();
		}

		private void UpdateColonistsFromBase()
		{
			colonistsFromBase.Clear();
			colonistsFromBase.AddRange(Map.mapPawns.FreeColonists.Where(c => !c.IsSlave));
			colonistsFromBase.AddRange( GetNearbyColonists(Map).Where(c=>!c.IsSlave));
		}
		
		private static IEnumerable<Pawn> GetNearbyColonists(Map mapHeld)
		{
			return PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Where(p => IsNearby(mapHeld, p));
		}

		private static bool IsNearby(Map mapHeld, Pawn p)
		{
			if (p.Spawned && p.MapHeld.IsPlayerHome) return false;
			var tile = p.GetRootTile();
			if (tile == -1) return false;

			return Find.WorldGrid.ApproxDistanceInTiles(mapHeld.Tile, tile) < 8; // within 3 tiles counts
		}

		private int GetFriendsRequired()
		{
			var x = colonistsFromBase.Count();
			if (x <= 3) return 1;
			// Formula from: https://mycurvefit.com/share/5b359026-5f44-4ac4-88ed-9b364a242f7b
			var a = 0.887f;
			var b = 0.646f;
			var y = a * Mathf.Pow(x, b);
			var required = y;
			return Mathf.RoundToInt(required);

		}

		public class RelationsCacheSet
		{
			private int lastUpdateTick;

			private Pawn Pawn { get; }
			public int friends;
			public int friendsSeniority;
			public int enemies;
			public int enemiesSeniority;
            
			public RelationsCacheSet(Pawn pawn)
			{
				Pawn = pawn;
			}

			public void TryUpdate()
			{
				if (Current.Game.tickManager.TicksGame <= lastUpdateTick + 50 + Pawn.HashOffset() % 20) return; // Don't update everyone at once	
				lastUpdateTick = Current.Game.tickManager.TicksGame;

				var mapComponent = Pawn.GetMapComponent();
				if (mapComponent == null) return;

				enemies = mapComponent.RelationsCache.colonistsFromBase.Where(p => RelationsUtility.PawnsKnowEachOther(Pawn, p) && Pawn.relations.OpinionOf(p) <= GuestUtility.MaxOpinionForEnemy)
					.Sum(p => GetRelationValue(p, Pawn));
				enemiesSeniority = mapComponent.RelationsCache.colonistsFromBase
					.Where(p => p.royalty?.MostSeniorTitle != null && RelationsUtility.PawnsKnowEachOther(Pawn, p) && Pawn.relations.OpinionOf(p) <= GuestUtility.MaxOpinionForEnemy)
					.Sum(p => p.royalty.MostSeniorTitle.def.seniority + 100); // seniority can be 0!
				float requiredOpinion = Pawn.GetMinRecruitOpinion();
				friends = mapComponent.RelationsCache.colonistsFromBase.Where(p => RelationsUtility.PawnsKnowEachOther(Pawn, p) && Pawn.relations.OpinionOf(p) >= requiredOpinion).Sum(pawn => GetRelationValue(pawn, Pawn));

				friendsSeniority = mapComponent.RelationsCache.colonistsFromBase.Where(p => p.royalty?.MostSeniorTitle != null && RelationsUtility.PawnsKnowEachOther(Pawn, p) && Pawn.relations.OpinionOf(p) >= requiredOpinion)
					.Sum(pawn => pawn.royalty.MostSeniorTitle.def.seniority + 100); // seriority can be 0!
			}

			private static int GetRelationValue(Pawn pawn, Pawn guest)
			{
				return GuestUtility.IsRelated(pawn, guest) ? 2 : 1;
			}
		}
	}
}
