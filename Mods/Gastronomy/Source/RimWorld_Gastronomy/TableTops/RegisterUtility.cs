using System.Linq;
using Gastronomy.Dining;
using JetBrains.Annotations;
using Verse;

namespace Gastronomy.TableTops
{
    public static class RegisterUtility
    {
        public static readonly ThingDef cashRegisterDef = ThingDef.Named("Gastronomy_CashRegister");

        private static Building_CashRegister GetFirstRegister([NotNull] Map map)
        {
            return map.listerThings.ThingsOfDef(cashRegisterDef).OfType<Building_CashRegister>().FirstOrDefault();
        }

        public static void OnDiningSpotCreated([NotNull]DiningSpot diningSpot)
        {
            diningSpot.GetRestaurant().diningSpots.Add(diningSpot);
        }

        public static void OnDiningSpotRemoved([NotNull]DiningSpot diningSpot)
        {
            diningSpot.GetRestaurant().diningSpots.Remove(diningSpot);
        }

        public static void OnBuildingDespawned(Building building, Map map)
        {
            if (building == null) return;
            if (building.def.surfaceType == SurfaceType.Eat || building is Building_TableTop)
            {
                foreach (var pos in building.OccupiedRect())
                {
                    NotifyDespawnedAtPosition(building, map, pos);
                }
            }
        }

        private static void NotifyDespawnedAtPosition(Building building, Map map, IntVec3 pos)
        {
            foreach (var thing in pos.GetThingList(map).ToArray())
            {
                // Notify potential dining spots
                if (DiningUtility.CanPossiblyDineAt(thing.def)) thing.TryGetComp<CompCanDineAt>()?.Notify_BuildingDespawned(building, map);
                // Notify table top
                if (thing is Building_TableTop t) t.Notify_BuildingDespawned(building);
                // Remove blueprints
                else if (thing.def.IsBlueprint && thing.def.entityDefToBuild is ThingDef td && typeof(Building_TableTop).IsAssignableFrom(td.thingClass))
                {
                    thing.Destroy(DestroyMode.Cancel);
                }
            }
        }

        public static void OnBuildingSpawned(Building building, Map map)
        {
            if (building == null) return;

            if (!(building is Building_TableTop)) return;
            foreach (var thing in building.Position.GetThingList(map).ToArray())
            {
                if (thing is DiningSpot)
                {
                    thing.Destroy(DestroyMode.Cancel);
                }
            }
        }
    }
}
