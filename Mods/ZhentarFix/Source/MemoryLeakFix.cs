using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;
using Verse.Profile;

namespace ZhentarFix
{
	class MemoryLeakFix : Game
	{
		[DetourMember]
		public void DeinitAndRemoveMap(Map map)
		{
			if (map == null)
			{
				Log.Error("Tried to remove null map.");
				return;
			}
			var maps = Maps;
			if (!maps.Contains(map))
			{
				Log.Error("Tried to remove map " + map + " but it's not here.");
				return;
			}
			bool flag = map.ParentFaction != null && map.ParentFaction.HostileTo(Faction.OfPlayer);
			List<Pawn> list = map.mapPawns.AllPawns.ToList();
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					Pawn pawn = list[i];
					if (pawn.Spawned)
					{
						pawn.DeSpawn();
					}
					if (pawn.IsColonist && flag)
					{
						map.ParentFaction.kidnapped.KidnapPawn(pawn, null);
					}
					else
					{
						Find.WorldPawns.PassToWorld(pawn);
					}
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Could not despawn and pass to world ", list[i], ": ", ex));
				}
			}

			//Clear out regions so they don't hold other objects live
			foreach (var region in map.regionGrid.AllRegions)
			{
				region.Room = null; region.links = null; listerThingsInfo.SetValue(region, null);
				//have to do this before the notify_ calls run
			}

			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int j = 0; j < allFactionsListForReading.Count; j++)
			{
				allFactionsListForReading[j].Notify_MapRemoved(map);
			}
			tickManager.RemoveAllFromMap(map);
			Map visibleMap = VisibleMap;
			int num = maps.IndexOf(map);
			for (int k = num; k < maps.Count; k++)
			{
				if (k == num)
				{
					RealTime.moteList.RemoveAllFromMap(maps[k]);
				}
				else
				{
					List<Mote> allMotes = RealTime.moteList.allMotes;
					for (int l = 0; l < allMotes.Count; l++)
					{
						if (allMotes[l].Map == maps[k])
						{
							allMotes[l].DecrementMapIndex();
						}
					}
				}
				List<Thing> allThings = maps[k].listerThings.AllThings;
				for (int m = 0; m < allThings.Count; m++)
				{
					if (k == num)
					{
						allThings[m].Notify_MyMapRemoved();
					}
					else
					{
						allThings[m].DecrementMapIndex();
					}
				}
				List<Room> allRooms = maps[k].regionGrid.allRooms;
				for (int n = 0; n < allRooms.Count; n++)
				{
					if (k == num)
					{
						allRooms[n].Notify_MyMapRemoved();
					}
					else
					{
						allRooms[n].DecrementMapIndex();
					}
				}
				foreach (Region current in maps[k].regionGrid.AllRegions)
				{
					if (k == num)
					{
						current.Notify_MyMapRemoved();
					}
					else
					{
						current.DecrementMapIndex();
					}
				}
			}
			maps.Remove(map);
			if (visibleMap != null)
			{
				sbyte b = (sbyte)maps.IndexOf(visibleMap);
				if (b < 0)
				{
					if (maps.Any())
					{
						VisibleMap = maps[0];
					}
					else
					{
						VisibleMap = null;
					}
				}
				else
				{
					visibleMapIndex = b;
				}
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}



			//clear all map fields to minimize what is kept live
			map.cellIndices = null;
			map.listerThings = null;
			map.listerBuildings = null;
			map.mapPawns = null;
			map.dynamicDrawManager = null;
			map.mapDrawer = null;
			map.tooltipGiverList = null;
			map.pawnDestinationManager = null;
			map.reservationManager = null;
			map.physicalInteractionReservationManager = null;
			map.designationManager = null;
			map.lordManager = null;
			map.debugDrawer = null;
			map.passingShipManager = null;
			map.slotGroupManager = null;
			map.mapConditionManager = null;
			map.weatherManager = null;
			map.zoneManager = null;
			map.resourceCounter = null;
			map.mapTemperature = null;
			map.temperatureCache = null;
			map.areaManager = null;
			map.attackTargetsCache = null;
			map.attackTargetReservationManager = null;
			map.lordsStarter = null;
			map.thingGrid = null;
			map.coverGrid = null;
			map.edificeGrid = null;
			map.fogGrid = null;
			map.glowGrid = null;
			map.regionGrid = null;
			map.terrainGrid = null;
			map.pathGrid = null;
			map.roofGrid = null;
			map.fertilityGrid = null;
			map.snowGrid = null;
			map.deepResourceGrid = null;
			map.exitMapGrid = null;
			map.linkGrid = null;
			map.glowFlooder = null;
			map.powerNetManager = null;
			map.powerNetGrid = null;
			map.regionMaker = null;
			map.pathFinder = null;
			map.pawnPathPool = null;
			map.regionAndRoomUpdater = null;
			map.regionLinkDatabase = null;
			map.moteCounter = null;
			map.gatherSpotLister = null;
			map.windManager = null;
			map.listerBuildingsRepairable = null;
			map.listerHaulables = null;
			map.listerFilthInHomeArea = null;
			map.reachability = null;
			map.itemAvailability = null;
			map.autoBuildRoofAreaSetter = null;
			map.roofCollapseBufferResolver = null;
			map.roofCollapseBuffer = null;
			map.wildSpawner = null;
			map.steadyAtmosphereEffects = null;
			map.skyManager = null;
			map.overlayDrawer = null;
			map.floodFiller = null;
			map.weatherDecider = null;
			map.fireWatcher = null;
			map.dangerWatcher = null;
			map.damageWatcher = null;
			map.strengthWatcher = null;
			map.wealthWatcher = null;
			map.regionDirtyer = null;
			map.cellsInRandomOrder = null;
			map.rememberedCameraPos = null;
			map.mineStrikeManager = null;
			map.components = null;


			MemoryUtility.UnloadUnusedUnityAssets(); //Unload meshes
		}

		private static readonly FieldInfo listerThingsInfo = typeof(Region).GetField("listerThings", Detours.UniversalBindingFlags);
	}
}
